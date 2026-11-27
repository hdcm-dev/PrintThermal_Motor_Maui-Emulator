// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using System.Threading.Channels;

namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>Un comando HCI terminó con estado distinto de 0.</summary>
public sealed class ErrorHciException(ushort opcode, byte estado)
    : Exception($"el comando HCI 0x{opcode:X4} terminó con estado 0x{estado:X2}")
{
    public ushort Opcode { get; } = opcode;

    public byte Estado { get; } = estado;
}

/// <summary>
/// Capa HCI del host: cola de comandos con el control de flujo del controlador (Num_HCI_Command_Packets), envío de
/// L2CAP en fragmentos ACL con los buffers del controlador (Number Of Completed Packets) y reensamblado de lo que llega.
/// No es seguro para hilos salvo la cola de comandos: <see cref="Procesar"/> se llama desde un solo lector.
/// </summary>
internal sealed class HostHci
{
    private sealed record Comando(ushort Opcode, byte[] Paquete, TaskCompletionSource<byte[]>? Respuesta);

    private readonly object _sync = new();
    private readonly Channel<PaqueteHci> _salida = Channel.CreateUnbounded<PaqueteHci>(new UnboundedChannelOptions { SingleReader = true });
    private readonly Queue<Comando> _cola = new();
    private readonly List<Comando> _esperando = [];
    private readonly Queue<(ushort Handle, byte[] Paquete)> _colaAcl = new();
    private readonly Dictionary<ushort, int> _aclEnVuelo = [];
    private readonly Dictionary<ushort, List<byte>> _reensamblado = [];
    private int _comandosPermitidos = 1;
    private ushort _aclMaximo;
    private int _aclLibres;

    /// <summary>Eventos que no son de control de flujo: código y parámetros.</summary>
    public Action<byte, byte[]>? Evento { get; set; }

    /// <summary>Una trama L2CAP completa recibida en una conexión.</summary>
    public Action<ushort, byte[]>? L2cap { get; set; }

    public ChannelReader<PaqueteHci> Salida => _salida.Reader;

    public static byte[] Armar(ushort opcode, ReadOnlySpan<byte> parametros) =>
        [(byte)opcode, (byte)(opcode >> 8), (byte)parametros.Length, .. parametros];

    /// <summary>Manda un comando y espera su Command Complete (o Command Status). Falla si el estado no es 0.</summary>
    public Task<byte[]> EjecutarAsync(ushort opcode, params byte[] parametros)
    {
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        Encolar(new Comando(opcode, Armar(opcode, parametros), tcs));
        return tcs.Task;
    }

    /// <summary>Manda un comando sin esperar la respuesta.</summary>
    public void Enviar(ushort opcode, params byte[] parametros) => Encolar(new Comando(opcode, Armar(opcode, parametros), null));

    public void ConfigurarAcl(ushort largoMaximo, ushort paquetes)
    {
        lock (_sync)
        {
            _aclMaximo = largoMaximo;
            _aclLibres = paquetes;
            BombearAcl();
        }
    }

    /// <summary>Fragmenta una trama L2CAP en paquetes ACL (el primero con PB = 10, los siguientes con PB = 01).</summary>
    public void EnviarL2cap(ushort handle, byte[] trama)
    {
        lock (_sync)
        {
            var maximo = _aclMaximo == 0 ? 27 : _aclMaximo;
            for (var i = 0; i < trama.Length; i += maximo)
            {
                var n = Math.Min(maximo, trama.Length - i);
                var pb = i == 0 ? 0b10 : 0b01;
                var p = new byte[4 + n];
                BinaryPrimitives.WriteUInt16LittleEndian(p, (ushort)(handle | (pb << 12)));
                BinaryPrimitives.WriteUInt16LittleEndian(p.AsSpan(2), (ushort)n);
                trama.AsSpan(i, n).CopyTo(p.AsSpan(4));
                _colaAcl.Enqueue((handle, p));
            }
            BombearAcl();
        }
    }

    /// <summary>La conexión se cerró: el controlador descarta sus paquetes pendientes y devuelve los buffers.</summary>
    public void OlvidarConexion(ushort handle)
    {
        lock (_sync)
        {
            if (_aclEnVuelo.Remove(handle, out var n))
            {
                _aclLibres += n;
            }
            var resto = _colaAcl.Where(x => x.Handle != handle).ToList();
            _colaAcl.Clear();
            foreach (var x in resto)
            {
                _colaAcl.Enqueue(x);
            }
            _reensamblado.Remove(handle);
            BombearAcl();
        }
    }

    public void Procesar(PaqueteHci p)
    {
        if (p.Tipo == TipoPaqueteHci.Evento && p.Datos.Length >= 2)
        {
            var codigo = p.Datos[0];
            var prm = p.Datos.AsSpan(2, Math.Min(p.Datos[1], p.Datos.Length - 2)).ToArray();
            switch (codigo)
            {
                case 0x0E when prm.Length >= 3:
                    Completar(prm[0], BinaryPrimitives.ReadUInt16LittleEndian(prm.AsSpan(1)), prm[3..], estado: prm.Length > 3 ? prm[3] : (byte)0);
                    break;
                case 0x0F when prm.Length >= 4:
                    Completar(prm[1], BinaryPrimitives.ReadUInt16LittleEndian(prm.AsSpan(2)), [], estado: prm[0]);
                    if (prm[0] != 0)
                    {
                        Evento?.Invoke(codigo, prm);
                    }
                    break;
                case 0x13 when prm.Length >= 1:
                    lock (_sync)
                    {
                        for (var i = 0; i < prm[0] && 1 + 4 * i + 4 <= prm.Length; i++)
                        {
                            var h = (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(prm.AsSpan(1 + 4 * i)) & 0x0FFF);
                            var n = BinaryPrimitives.ReadUInt16LittleEndian(prm.AsSpan(3 + 4 * i));
                            if (_aclEnVuelo.TryGetValue(h, out var v))
                            {
                                var liberados = Math.Min(v, n);
                                _aclEnVuelo[h] = v - liberados;
                                _aclLibres += liberados;
                            }
                        }
                        BombearAcl();
                    }
                    break;
                default:
                    Evento?.Invoke(codigo, prm);
                    break;
            }
        }
        else if (p.Tipo == TipoPaqueteHci.Acl && p.Datos.Length >= 4)
        {
            RecibirAcl(p.Datos);
        }
    }

    private void RecibirAcl(byte[] d)
    {
        var cab = BinaryPrimitives.ReadUInt16LittleEndian(d);
        var handle = (ushort)(cab & 0x0FFF);
        var pb = (cab >> 12) & 0b11;
        var largo = Math.Min(BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(2)), d.Length - 4);
        var datos = d.AsSpan(4, largo);
        if (pb == 0b01)
        {
            if (!_reensamblado.TryGetValue(handle, out var buf))
            {
                return; // continuación sin comienzo
            }
            buf.AddRange(datos.ToArray());
        }
        else
        {
            _reensamblado[handle] = [.. datos.ToArray()];
        }
        var b = _reensamblado[handle];
        if (b.Count >= 4)
        {
            var total = 4 + (b[0] | (b[1] << 8));
            if (b.Count >= total)
            {
                _reensamblado.Remove(handle);
                L2cap?.Invoke(handle, b.GetRange(0, total).ToArray());
            }
        }
    }

    private void Completar(byte permitidos, ushort opcode, byte[] retorno, byte estado)
    {
        Comando? c;
        lock (_sync)
        {
            _comandosPermitidos = permitidos;
            c = _esperando.FirstOrDefault(x => x.Opcode == opcode);
            if (c is not null)
            {
                _esperando.Remove(c);
            }
            BombearComandos();
        }
        if (c?.Respuesta is { } r)
        {
            if (estado != 0)
            {
                r.TrySetException(new ErrorHciException(opcode, estado));
            }
            else
            {
                r.TrySetResult(retorno);
            }
        }
    }

    private void Encolar(Comando c)
    {
        lock (_sync)
        {
            _cola.Enqueue(c);
            BombearComandos();
        }
    }

    private void BombearComandos()
    {
        while (_comandosPermitidos > 0 && _cola.Count > 0)
        {
            var c = _cola.Dequeue();
            _comandosPermitidos--;
            _esperando.Add(c);
            _salida.Writer.TryWrite(new PaqueteHci(TipoPaqueteHci.Comando, c.Paquete));
        }
    }

    private void BombearAcl()
    {
        while (_aclLibres > 0 && _colaAcl.Count > 0)
        {
            var (handle, paquete) = _colaAcl.Dequeue();
            _aclLibres--;
            _aclEnVuelo[handle] = _aclEnVuelo.GetValueOrDefault(handle) + 1;
            _salida.Writer.TryWrite(new PaqueteHci(TipoPaqueteHci.Acl, paquete));
        }
    }
}
