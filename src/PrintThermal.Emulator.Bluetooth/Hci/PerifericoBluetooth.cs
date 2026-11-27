// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using System.Text;
using PrintThermal.Emulator.Bluetooth.L2cap;
using PrintThermal.Emulator.Bluetooth.Rfcomm;
using PrintThermal.Emulator.Bluetooth.Sdp;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>
/// Host Bluetooth Classic de la impresora emulada sobre un transporte HCI: se anuncia con nombre, clase y EIR, queda
/// visible y conectable, acepta conexiones ACL, empareja por SSP, publica el registro SDP del perfil de puerto serie y
/// entrega al receptor lo que llega por RFCOMM.
/// </summary>
public sealed class PerifericoBluetooth
{
    private const uint HandleSpp = 0x00010001;

    private readonly ITransporteHci _transporte;
    private readonly OpcionesPeriferico _opciones;
    private readonly IReceptorDeBytes _receptor;
    private readonly Action<string> _registro;
    private readonly HostHci _host = new();
    private readonly GestorDeSeguridad _seguridad;
    private readonly ServidorSdp _sdp = new();
    private readonly Dictionary<ushort, (DireccionBluetooth Par, MultiplexorL2cap L2cap)> _conexiones = [];
    private readonly TaskCompletionSource _anunciando = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private long _ultimoId;

    public PerifericoBluetooth(ITransporteHci transporte, OpcionesPeriferico opciones, IReceptorDeBytes receptor,
        IAlmacenDeClaves claves, Action<string>? registro = null)
    {
        _transporte = transporte;
        _opciones = opciones;
        _receptor = receptor;
        _registro = registro ?? (_ => { });
        _seguridad = new GestorDeSeguridad(_host, claves, opciones.Pin, _registro);
        _sdp.Agregar(HandleSpp, RegistroSpp.Crear(HandleSpp, opciones.CanalRfcomm, opciones.Nombre));
        _host.Evento = AlEvento;
        _host.L2cap = (handle, trama) =>
        {
            if (_conexiones.TryGetValue(handle, out var c))
            {
                c.L2cap.Recibir(trama);
            }
        };
    }

    public EstadoPeriferico Estado { get; private set; } = EstadoPeriferico.Detenido;

    /// <summary>La dirección que informa el controlador (Read BD_ADDR).</summary>
    public DireccionBluetooth? DireccionLocal { get; private set; }

    /// <summary>Se completa cuando terminó la inicialización y el periférico es visible y conectable.</summary>
    public Task Anunciando => _anunciando.Task;

    public int ConexionesAcl => _conexiones.Count;

    /// <summary>
    /// Inicializa el controlador y atiende hasta que se cancela o el transporte se cierra (vuelve sin error); falla si
    /// la inicialización falla o el transporte da error.
    /// </summary>
    public async Task EjecutarAsync(CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Estado = EstadoPeriferico.Iniciando;
        var escritor = Task.Run(async () =>
        {
            await foreach (var p in _host.Salida.ReadAllAsync(cts.Token))
            {
                if (_opciones.TrazaHci && p.Tipo == TipoPaqueteHci.Comando)
                {
                    _registro($"HCI > {Convert.ToHexString(p.Datos)}");
                }
                await _transporte.EnviarAsync(p, cts.Token);
            }
        }, CancellationToken.None);
        var lector = Task.Run(async () =>
        {
            await foreach (var p in _transporte.RecibirAsync(cts.Token))
            {
                if (_opciones.TrazaHci && p.Tipo == TipoPaqueteHci.Evento && p.Datos.Length > 0 && p.Datos[0] != 0x13)
                {
                    _registro($"HCI < {Convert.ToHexString(p.Datos)}");
                }
                try
                {
                    _host.Procesar(p);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    // Un paquete mal formado no tira abajo al periférico.
                    _registro($"error al procesar un paquete {p.Tipo}: {e.Message}");
                }
            }
        }, CancellationToken.None);
        try
        {
            var inicio = InicializarAsync();
            if (await Task.WhenAny(inicio, lector) == inicio)
            {
                await inicio;
                Estado = EstadoPeriferico.Anunciando;
                _registro($"anunciando «{_opciones.Nombre}» en {DireccionLocal}, canal RFCOMM {_opciones.CanalRfcomm}");
                _anunciando.TrySetResult();
                await lector;
            }
            else
            {
                await lector;
                throw new IOException("el transporte HCI se cerró durante la inicialización");
            }
            Estado = EstadoPeriferico.Detenido;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            Estado = EstadoPeriferico.Detenido;
        }
        catch (Exception e)
        {
            Estado = EstadoPeriferico.Error;
            _anunciando.TrySetException(e);
            throw;
        }
        finally
        {
            cts.Cancel();
            try
            {
                await Task.WhenAll(escritor, lector);
            }
            catch (Exception)
            {
                // Lo que importa ya se informó arriba; acá sólo se espera a que terminen.
            }
            foreach (var (handle, c) in _conexiones.ToList())
            {
                _conexiones.Remove(handle);
                c.L2cap.Cerrar();
            }
        }
    }

    private async Task InicializarAsync()
    {
        await _host.EjecutarAsync(0x0C03); // Reset
        var buffers = await _host.EjecutarAsync(0x1005); // Read Buffer Size
        _host.ConfigurarAcl(BinaryPrimitives.ReadUInt16LittleEndian(buffers.AsSpan(1)), BinaryPrimitives.ReadUInt16LittleEndian(buffers.AsSpan(4)));
        var direccion = await _host.EjecutarAsync(0x1009); // Read BD_ADDR
        DireccionLocal = DireccionBluetooth.DesdeHci(direccion.AsSpan(1));
        await _host.EjecutarAsync(0x0C01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F); // Set Event Mask: con los de SSP
        await _host.EjecutarAsync(0x0C56, 0x01); // Write Simple Pairing Mode
        await _host.EjecutarAsync(0x0C13, NombreLocal()); // Write Local Name
        var cod = _opciones.ClaseDeDispositivo;
        await _host.EjecutarAsync(0x0C24, (byte)cod, (byte)(cod >> 8), (byte)(cod >> 16)); // Write Class of Device
        await _host.EjecutarAsync(0x0C52, Eir()); // Write Extended Inquiry Response
        await _host.EjecutarAsync(0x0C1A, 0x03); // Write Scan Enable: inquiry scan + page scan
    }

    private byte[] NombreLocal()
    {
        var n = new byte[248];
        var b = Encoding.UTF8.GetBytes(_opciones.Nombre);
        b.AsSpan(0, Math.Min(b.Length, 247)).CopyTo(n);
        return n;
    }

    /// <summary>FEC no requerido + 240 bytes: nombre completo (0x09) y UUID de 16 bits completos (0x03) con 0x1101.</summary>
    private byte[] Eir()
    {
        var e = new byte[241];
        var nombre = Encoding.UTF8.GetBytes(_opciones.Nombre);
        var n = Math.Min(nombre.Length, 200);
        var i = 1;
        e[i++] = (byte)(n + 1);
        e[i++] = 0x09;
        nombre.AsSpan(0, n).CopyTo(e.AsSpan(i));
        i += n;
        e[i++] = 0x03;
        e[i++] = 0x03;
        e[i++] = unchecked((byte)RegistroSpp.PerfilPuertoSerie);
        e[i] = RegistroSpp.PerfilPuertoSerie >> 8;
        return e;
    }

    private void AlEvento(byte codigo, byte[] p)
    {
        if (GestorDeSeguridad.Atiende(codigo))
        {
            _seguridad.Procesar(codigo, p);
            return;
        }
        switch (codigo)
        {
            case 0x04 when p.Length >= 10: // Connection Request
                if (p[9] == 0x01)
                {
                    _registro($"pedido de conexión ACL de {DireccionBluetooth.DesdeHci(p)}; se acepta");
                    _host.Enviar(0x0409, [.. p[..6], 0x01]); // seguir como periférico
                }
                else
                {
                    _host.Enviar(0x040A, [.. p[..6], 0x0D]); // SCO/eSCO: recursos limitados
                }
                break;
            case 0x03 when p.Length >= 11: // Connection Complete
                if (p[0] == 0 && p[9] == 0x01)
                {
                    var handle = (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(p.AsSpan(1)) & 0x0FFF);
                    var par = DireccionBluetooth.DesdeHci(p.AsSpan(3));
                    var l2cap = new MultiplexorL2cap(par, _opciones.MtuL2cap, psm => ServicioPara(psm), t => _host.EnviarL2cap(handle, t), _registro);
                    _conexiones[handle] = (par, l2cap);
                    _registro($"conexión ACL 0x{handle:X3} con {par}");
                }
                else
                {
                    _registro($"conexión fallida: estado 0x{p[0]:X2}");
                }
                break;
            case 0x05 when p.Length >= 4: // Disconnection Complete
                var h = (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(p.AsSpan(1)) & 0x0FFF);
                if (_conexiones.Remove(h, out var c))
                {
                    _registro($"desconexión ACL 0x{h:X3} de {c.Par}, motivo 0x{p[3]:X2}");
                    c.L2cap.Cerrar();
                }
                _host.OlvidarConexion(h);
                break;
            case 0x08 when p.Length >= 4: // Encryption Change
                _registro($"cifrado en 0x{BinaryPrimitives.ReadUInt16LittleEndian(p.AsSpan(1)) & 0x0FFF:X3}: estado 0x{p[0]:X2}, activo {p[3]}");
                break;
            case 0x0F when p.Length >= 4: // Command Status con error, de un comando sin espera
                _registro($"comando 0x{BinaryPrimitives.ReadUInt16LittleEndian(p.AsSpan(2)):X4}: estado 0x{p[0]:X2}");
                break;
        }
    }

    private IServicioL2cap? ServicioPara(ushort psm) => psm switch
    {
        ServicioSdpL2cap.Psm => new ServicioSdpL2cap(_sdp),
        SesionRfcomm.Psm => new SesionRfcomm(_opciones.CanalRfcomm, _opciones.CreditosRfcomm, _receptor,
            () => Interlocked.Increment(ref _ultimoId), _registro),
        _ => null,
    };
}
