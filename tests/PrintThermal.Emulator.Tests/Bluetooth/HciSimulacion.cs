// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using System.Threading.Channels;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Rfcomm;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>
/// Controlador HCI falso para las pruebas: contesta los comandos como un controlador (Command Complete o Command Status),
/// deja inyectar eventos y ACL del lado de Android y junta lo que manda el host. Es independiente del código de producción:
/// arma y lee los paquetes con su propio código.
/// </summary>
internal sealed class ControladorFalso : ITransporteHci
{
    public static readonly DireccionBluetooth Propia = DireccionBluetooth.Parse("04:00:00:00:00:00");

    private readonly Channel<PaqueteHci> _haciaHost = Channel.CreateUnbounded<PaqueteHci>();
    private readonly Channel<PaqueteHci> _aclDesdeHost = Channel.CreateUnbounded<PaqueteHci>();
    private readonly List<PaqueteHci> _comandos = [];
    private readonly SemaphoreSlim _hayComando = new(0);

    public ushort AclMaximo { get; init; } = 27;

    public ushort AclPaquetes { get; init; } = 3;

    public ushort Handle { get; init; } = 0x000B;

    public int AclPendientesMaximo { get; private set; }

    private int _aclPendientes;

    public IReadOnlyList<PaqueteHci> Comandos
    {
        get
        {
            lock (_comandos)
            {
                return [.. _comandos];
            }
        }
    }

    public static ushort Opcode(PaqueteHci p) => BinaryPrimitives.ReadUInt16LittleEndian(p.Datos);

    public static byte[] Parametros(PaqueteHci p) => p.Datos[3..];

    public ValueTask EnviarAsync(PaqueteHci p, CancellationToken ct = default)
    {
        if (p.Tipo == TipoPaqueteHci.Comando)
        {
            lock (_comandos)
            {
                _comandos.Add(p);
            }
            _hayComando.Release();
            Responder(p);
        }
        else if (p.Tipo == TipoPaqueteHci.Acl)
        {
            var pendientes = Interlocked.Increment(ref _aclPendientes);
            AclPendientesMaximo = Math.Max(AclPendientesMaximo, pendientes);
            _aclDesdeHost.Writer.TryWrite(p);
        }
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<PaqueteHci> RecibirAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var p in _haciaHost.Reader.ReadAllAsync(ct))
        {
            yield return p;
        }
    }

    public ValueTask DisposeAsync()
    {
        _haciaHost.Writer.TryComplete();
        return ValueTask.CompletedTask;
    }

    /// <summary>Corta el transporte, como si netsim se cayera.</summary>
    public void Cortar() => _haciaHost.Writer.TryComplete();

    public void Evento(byte codigo, params byte[] parametros) =>
        _haciaHost.Writer.TryWrite(new PaqueteHci(TipoPaqueteHci.Evento, [codigo, (byte)parametros.Length, .. parametros]));

    /// <summary>Entrega al host una trama L2CAP en fragmentos ACL de <paramref name="fragmento"/> bytes.</summary>
    public void Acl(byte[] l2cap, int fragmento = 19)
    {
        for (var i = 0; i < l2cap.Length; i += fragmento)
        {
            var trozo = l2cap.AsSpan(i, Math.Min(fragmento, l2cap.Length - i));
            var pb = i == 0 ? 0b10 : 0b01;
            var cab = (ushort)(Handle | (pb << 12));
            var d = new byte[4 + trozo.Length];
            BinaryPrimitives.WriteUInt16LittleEndian(d, cab);
            BinaryPrimitives.WriteUInt16LittleEndian(d.AsSpan(2), (ushort)trozo.Length);
            trozo.CopyTo(d.AsSpan(4));
            _haciaHost.Writer.TryWrite(new PaqueteHci(TipoPaqueteHci.Acl, d));
        }
    }

    /// <summary>Espera un comando del host con ese opcode, a partir del índice <paramref name="desde"/>.</summary>
    public async Task<PaqueteHci> EsperarComandoAsync(ushort opcode, int desde = 0, int segundos = 5)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(segundos));
        while (true)
        {
            var hallado = Comandos.Skip(desde).Where(c => Opcode(c) == opcode).ToList();
            if (hallado.Count > 0)
            {
                return hallado[0];
            }
            try
            {
                await _hayComando.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"el host no envió el comando 0x{opcode:X4}");
            }
        }
    }

    /// <summary>La próxima trama L2CAP completa que manda el host (reensambla los fragmentos ACL).</summary>
    public async Task<byte[]> SiguienteL2capAsync(int segundos = 5)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(segundos));
        var buf = new List<byte>();
        var total = -1;
        while (total < 0 || buf.Count < total)
        {
            PaqueteHci p;
            try
            {
                p = await _aclDesdeHost.Reader.ReadAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("el host no envió la trama L2CAP esperada");
            }
            var cab = BinaryPrimitives.ReadUInt16LittleEndian(p.Datos);
            var largo = BinaryPrimitives.ReadUInt16LittleEndian(p.Datos.AsSpan(2));
            Assert.Equal(Handle, cab & 0x0FFF);
            Assert.True(largo <= AclMaximo, $"fragmento ACL de {largo} bytes con un máximo de {AclMaximo}");
            Assert.Equal(largo, p.Datos.Length - 4);
            var pb = (cab >> 12) & 0b11;
            Assert.Equal(total < 0 ? 0b10 : 0b01, pb);
            buf.AddRange(p.Datos.AsSpan(4).ToArray());
            if (total < 0)
            {
                total = 4 + BinaryPrimitives.ReadUInt16LittleEndian(buf.ToArray());
            }
            // El controlador devuelve el crédito ACL, como Number Of Completed Packets.
            Interlocked.Decrement(ref _aclPendientes);
            Evento(0x13, 1, (byte)Handle, (byte)(Handle >> 8), 1, 0);
        }
        Assert.Equal(total, buf.Count);
        return [.. buf];
    }

    private void Responder(PaqueteHci p)
    {
        var op = Opcode(p);
        var prm = Parametros(p);
        byte[] retorno = op switch
        {
            0x1005 => [0, (byte)AclMaximo, (byte)(AclMaximo >> 8), 0, (byte)AclPaquetes, (byte)(AclPaquetes >> 8), 0, 0],
            0x1009 => [0, .. Propia.ComoHci()],
            0x042B or 0x042C or 0x040B or 0x040C or 0x040D or 0x040E => [0, .. prm[..6]],
            _ => [0],
        };
        if (op is 0x0405 or 0x0406)
        {
            Evento(0x0F, 0, 1, (byte)op, (byte)(op >> 8)); // Create Connection y Disconnect contestan con Command Status
            return;
        }
        if (op == 0x0409)
        {
            Evento(0x0F, 0, 1, (byte)op, (byte)(op >> 8));
            Evento(0x03, [0, (byte)Handle, (byte)(Handle >> 8), .. prm[..6], 0x01, 0x00]);
            return;
        }
        Evento(0x0E, [1, (byte)op, (byte)(op >> 8), .. retorno]);
    }
}

/// <summary>Lado de Android simulado sobre el controlador falso: ACL, señalización L2CAP y tramas RFCOMM.</summary>
internal sealed class AndroidSimulado(ControladorFalso c)
{
    public static readonly DireccionBluetooth Direccion = DireccionBluetooth.Parse("02:00:00:00:00:01");

    private byte _id;

    public async Task ConectarAclAsync()
    {
        c.Evento(0x04, [.. Direccion.ComoHci(), 0x0C, 0x02, 0x5A, 0x01]);
        await c.EsperarComandoAsync(0x0409);
    }

    public static byte[] L2cap(ushort cid, byte[] carga)
    {
        var t = new byte[4 + carga.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(t, (ushort)carga.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(t.AsSpan(2), cid);
        carga.CopyTo(t, 4);
        return t;
    }

    public void Senal(byte codigo, byte id, params byte[] datos)
    {
        var s = new byte[4 + datos.Length];
        s[0] = codigo;
        s[1] = id;
        BinaryPrimitives.WriteUInt16LittleEndian(s.AsSpan(2), (ushort)datos.Length);
        datos.CopyTo(s, 4);
        c.Acl(L2cap(0x0001, s));
    }

    /// <summary>Abre un canal L2CAP al PSM y completa la configuración en los dos sentidos. Devuelve el CID del host.</summary>
    public async Task<(ushort CidHost, ushort MtuHost)> AbrirL2capAsync(ushort psm, ushort cidPropio, ushort mtuPropio = 1024)
    {
        var id = ++_id;
        Senal(0x02, id, (byte)psm, (byte)(psm >> 8), (byte)cidPropio, (byte)(cidPropio >> 8));
        ushort cidHost = 0, mtuHost = 672;
        bool respuesta = false, pedidoDelHost = false, configAceptada = false;
        while (!(respuesta && pedidoDelHost && configAceptada))
        {
            var t = await c.SiguienteL2capAsync();
            Assert.Equal(0x0001, BinaryPrimitives.ReadUInt16LittleEndian(t.AsSpan(2)));
            var codigo = t[4];
            var sid = t[5];
            var d = t.AsSpan(8);
            switch (codigo)
            {
                case 0x03:
                    Assert.Equal(id, sid);
                    Assert.Equal(0, BinaryPrimitives.ReadUInt16LittleEndian(d[4..])); // resultado: éxito
                    Assert.Equal(cidPropio, BinaryPrimitives.ReadUInt16LittleEndian(d[2..]));
                    cidHost = BinaryPrimitives.ReadUInt16LittleEndian(d);
                    respuesta = true;
                    var idCfg = ++_id;
                    Senal(0x04, idCfg, (byte)cidHost, (byte)(cidHost >> 8), 0, 0, 0x01, 0x02, (byte)mtuPropio, (byte)(mtuPropio >> 8));
                    break;
                case 0x04:
                    Assert.Equal(cidPropio, BinaryPrimitives.ReadUInt16LittleEndian(d));
                    for (var o = 4; o + 2 <= d.Length; o += 2 + d[o + 1])
                    {
                        if ((d[o] & 0x7F) == 0x01)
                        {
                            mtuHost = BinaryPrimitives.ReadUInt16LittleEndian(d[(o + 2)..]);
                        }
                    }
                    Senal(0x05, sid, (byte)cidHost, (byte)(cidHost >> 8), 0, 0, 0, 0);
                    pedidoDelHost = true;
                    break;
                case 0x05:
                    Assert.Equal(cidPropio, BinaryPrimitives.ReadUInt16LittleEndian(d));
                    Assert.Equal(0, BinaryPrimitives.ReadUInt16LittleEndian(d[4..]));
                    configAceptada = true;
                    break;
                default:
                    Assert.Fail($"señal L2CAP inesperada 0x{codigo:X2}");
                    break;
            }
        }
        return (cidHost, mtuHost);
    }

    /// <summary>La próxima trama del host en el canal <paramref name="cidPropio"/>; sin carga de señalización.</summary>
    public async Task<byte[]> RecibirAsync(ushort cidPropio)
    {
        var t = await c.SiguienteL2capAsync();
        Assert.Equal(cidPropio, BinaryPrimitives.ReadUInt16LittleEndian(t.AsSpan(2)));
        return t[4..];
    }

    // --- RFCOMM, del lado del iniciador (C/R = 1 en sus comandos). FCS calculada bit a bit, sin tabla. ---

    public static byte FcsBitABit(ReadOnlySpan<byte> datos)
    {
        byte crc = 0xFF;
        foreach (var b in datos)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (byte)((crc >> 1) ^ 0xE0) : (byte)(crc >> 1);
            }
        }
        return (byte)(0xFF - crc);
    }

    public static byte[] Trama(byte dlci, bool cr, byte control, byte[] info, byte? creditos = null)
    {
        var t = new List<byte> { (byte)((dlci << 2) | (cr ? 2 : 0) | 1), control };
        if (info.Length <= 127)
        {
            t.Add((byte)((info.Length << 1) | 1));
        }
        else
        {
            t.Add((byte)(info.Length << 1));
            t.Add((byte)(info.Length >> 7));
        }
        var cubre = (control & 0xEF) == 0xEF ? 2 : t.Count;
        if (creditos is byte k)
        {
            t.Add(k);
        }
        t.AddRange(info);
        t.Add(FcsBitABit(t.Take(cubre).ToArray()));
        return [.. t];
    }

    /// <summary>Trama RFCOMM leída sin el código de producción.</summary>
    public sealed record TramaLeida(byte Dlci, bool Cr, byte Control, byte[] Info, byte? Creditos);

    public static TramaLeida Leer(byte[] t, bool conCreditos)
    {
        var dlci = (byte)(t[0] >> 2);
        var control = t[1];
        int largo, cab;
        if ((t[2] & 1) == 1)
        {
            largo = t[2] >> 1;
            cab = 3;
        }
        else
        {
            largo = (t[2] >> 1) | (t[3] << 7);
            cab = 4;
        }
        var esUih = (control & 0xEF) == 0xEF;
        Assert.Equal(FcsBitABit(t.AsSpan(0, esUih ? 2 : cab)), t[^1]);
        byte? creditos = null;
        if (esUih && (control & 0x10) != 0 && conCreditos && dlci != 0)
        {
            creditos = t[cab];
            cab++;
        }
        Assert.Equal(cab + largo + 1, t.Length);
        return new TramaLeida(dlci, (t[0] & 2) != 0, control, t[cab..(cab + largo)], creditos);
    }
}

/// <summary>Receptor que junta los bytes por conexión.</summary>
internal sealed class ReceptorEnMemoria : IReceptorDeBytes
{
    public List<InfoConexionRfcomm> Abiertas { get; } = [];

    public List<InfoConexionRfcomm> Cerradas { get; } = [];

    public Dictionary<long, MemoryStream> Datos { get; } = [];

    public TaskCompletionSource<InfoConexionRfcomm> UltimaCerrada { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Abierta(InfoConexionRfcomm conexion)
    {
        lock (this)
        {
            Abiertas.Add(conexion);
            Datos[conexion.Id] = new MemoryStream();
        }
    }

    public void Recibidos(InfoConexionRfcomm conexion, ReadOnlySpan<byte> datos)
    {
        lock (this)
        {
            Datos[conexion.Id].Write(datos);
        }
    }

    public void Cerrada(InfoConexionRfcomm conexion)
    {
        TaskCompletionSource<InfoConexionRfcomm> tcs;
        lock (this)
        {
            Cerradas.Add(conexion);
            tcs = UltimaCerrada;
            UltimaCerrada = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        tcs.TrySetResult(conexion);
    }
}
