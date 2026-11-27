// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Buffers.Binary;
using PrintThermal.Emulator.Bluetooth.Hci;

namespace PrintThermal.Emulator.Bluetooth.L2cap;

/// <summary>Lo que corre sobre un canal L2CAP (SDP, RFCOMM).</summary>
internal interface IServicioL2cap
{
    void Abierto(CanalL2cap canal);

    void Recibir(CanalL2cap canal, byte[] sdu);

    void Cerrado(CanalL2cap canal);
}

/// <summary>Un canal L2CAP orientado a conexión, en modo básico.</summary>
internal sealed class CanalL2cap(ushort psm, ushort cidLocal, ushort cidRemoto, ushort mtuLocal, DireccionBluetooth par,
    IServicioL2cap servicio, Action<byte[]> enviarTrama)
{
    public ushort Psm { get; } = psm;

    public ushort CidLocal { get; } = cidLocal;

    public ushort CidRemoto { get; } = cidRemoto;

    public ushort MtuLocal { get; } = mtuLocal;

    /// <summary>MTU del otro lado: el SDU más grande que se le puede mandar. 672 si no lo configura.</summary>
    public ushort MtuRemoto { get; set; } = 672;

    public DireccionBluetooth Par { get; } = par;

    public IServicioL2cap Servicio { get; } = servicio;

    public bool ConfigRecibida { get; set; }

    public bool ConfigAceptada { get; set; }

    public bool Abierto { get; set; }

    public void Enviar(ReadOnlySpan<byte> sdu)
    {
        if (sdu.Length > MtuRemoto)
        {
            throw new InvalidOperationException($"SDU de {sdu.Length} bytes con MTU remoto {MtuRemoto}");
        }
        enviarTrama(MultiplexorL2cap.Trama(CidRemoto, sdu));
    }
}

/// <summary>
/// L2CAP sobre una conexión ACL: canal de señalización (CID 0x0001) y canales dinámicos (Bluetooth Core, vol. 3, parte A).
/// Atiende Connection, Configuration, Disconnection, Information y Echo.
/// </summary>
internal sealed class MultiplexorL2cap(DireccionBluetooth par, ushort mtuLocal, Func<ushort, IServicioL2cap?> servicioPara,
    Action<byte[]> enviarTrama, Action<string> registro)
{
    private const ushort CidSenalizacion = 0x0001;

    private readonly Dictionary<ushort, CanalL2cap> _canales = [];
    private ushort _siguienteCid = 0x0040;
    private byte _id;

    public static byte[] Trama(ushort cid, ReadOnlySpan<byte> carga)
    {
        var t = new byte[4 + carga.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(t, (ushort)carga.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(t.AsSpan(2), cid);
        carga.CopyTo(t.AsSpan(4));
        return t;
    }

    public void Recibir(byte[] trama)
    {
        if (trama.Length < 4)
        {
            return;
        }
        var largo = BinaryPrimitives.ReadUInt16LittleEndian(trama);
        var cid = BinaryPrimitives.ReadUInt16LittleEndian(trama.AsSpan(2));
        if (largo != trama.Length - 4)
        {
            registro($"L2CAP: largo {largo} distinto de {trama.Length - 4}; se descarta");
            return;
        }
        if (cid == CidSenalizacion)
        {
            Senalizacion(trama.AsSpan(4));
        }
        else if (_canales.TryGetValue(cid, out var canal) && canal.Abierto)
        {
            canal.Servicio.Recibir(canal, trama[4..]);
        }
    }

    /// <summary>Se cayó la conexión ACL: se cierran todos los canales.</summary>
    public void Cerrar()
    {
        foreach (var c in _canales.Values.ToList())
        {
            Quitar(c);
        }
    }

    private void Senalizacion(ReadOnlySpan<byte> d)
    {
        while (d.Length >= 4)
        {
            var codigo = d[0];
            var id = d[1];
            var largo = BinaryPrimitives.ReadUInt16LittleEndian(d[2..]);
            if (d.Length < 4 + largo)
            {
                return;
            }
            var datos = d.Slice(4, largo).ToArray();
            d = d[(4 + largo)..];
            switch (codigo)
            {
                case 0x02 when datos.Length >= 4:
                    PedidoDeConexion(id, datos);
                    break;
                case 0x04 when datos.Length >= 4:
                    PedidoDeConfiguracion(id, datos);
                    break;
                case 0x05 when datos.Length >= 6:
                    RespuestaDeConfiguracion(datos);
                    break;
                case 0x06 when datos.Length >= 4:
                    PedidoDeDesconexion(id, datos);
                    break;
                case 0x08:
                    Senal(0x09, id, datos);
                    break;
                case 0x0A when datos.Length >= 2:
                    PedidoDeInformacion(id, datos);
                    break;
                case 0x01 or 0x03 or 0x07 or 0x09 or 0x0B:
                    break; // respuestas y rechazos a pedidos propios: nada que hacer
                default:
                    Senal(0x01, id, [0x00, 0x00]); // Command Reject: no entendido
                    break;
            }
        }
    }

    private void PedidoDeConexion(byte id, byte[] d)
    {
        var psm = BinaryPrimitives.ReadUInt16LittleEndian(d);
        var cidRemoto = BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(2));
        var servicio = servicioPara(psm);
        if (servicio is null)
        {
            registro($"L2CAP: PSM 0x{psm:X4} sin servicio; se rechaza");
            Senal(0x03, id, [0, 0, (byte)cidRemoto, (byte)(cidRemoto >> 8), 0x02, 0x00, 0, 0]);
            return;
        }
        var cid = _siguienteCid++;
        var canal = new CanalL2cap(psm, cid, cidRemoto, mtuLocal, par, servicio, enviarTrama);
        _canales[cid] = canal;
        Senal(0x03, id, [(byte)cid, (byte)(cid >> 8), (byte)cidRemoto, (byte)(cidRemoto >> 8), 0, 0, 0, 0]);
        Senal(0x04, SiguienteId(), [(byte)cidRemoto, (byte)(cidRemoto >> 8), 0, 0, 0x01, 0x02, (byte)mtuLocal, (byte)(mtuLocal >> 8)]);
    }

    private void PedidoDeConfiguracion(byte id, byte[] d)
    {
        var cid = BinaryPrimitives.ReadUInt16LittleEndian(d);
        var banderas = BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(2));
        if (!_canales.TryGetValue(cid, out var canal))
        {
            Senal(0x01, id, [0x02, 0x00, (byte)cid, (byte)(cid >> 8), 0, 0]); // Command Reject: CID inválido
            return;
        }
        var resultado = (ushort)0;
        var opcionesRespuesta = new List<byte>();
        for (var o = 4; o + 2 <= d.Length && o + 2 + d[o + 1] <= d.Length; o += 2 + d[o + 1])
        {
            var tipo = d[o] & 0x7F;
            var valor = d.AsSpan(o + 2, d[o + 1]);
            if (tipo == 0x01 && valor.Length >= 2)
            {
                canal.MtuRemoto = BinaryPrimitives.ReadUInt16LittleEndian(valor);
            }
            else if (tipo == 0x04 && valor.Length >= 1 && valor[0] != 0x00)
            {
                // Sólo modo básico: se propone el básico.
                resultado = 0x0001;
                opcionesRespuesta.AddRange([0x04, 0x09, 0x00, 0, 0, 0, 0, 0, 0, 0, 0]);
            }
        }
        Senal(0x05, id, [(byte)canal.CidRemoto, (byte)(canal.CidRemoto >> 8), (byte)(banderas & 1), 0,
            (byte)resultado, (byte)(resultado >> 8), .. opcionesRespuesta]);
        if (resultado == 0 && (banderas & 1) == 0)
        {
            canal.ConfigRecibida = true;
            AbrirSiCorresponde(canal);
        }
    }

    private void RespuestaDeConfiguracion(byte[] d)
    {
        var cid = BinaryPrimitives.ReadUInt16LittleEndian(d);
        var resultado = BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(4));
        if (!_canales.TryGetValue(cid, out var canal))
        {
            return;
        }
        if (resultado != 0)
        {
            registro($"L2CAP: configuración rechazada en CID 0x{cid:X4} (resultado {resultado}); se pide sin opciones");
            Senal(0x04, SiguienteId(), [(byte)canal.CidRemoto, (byte)(canal.CidRemoto >> 8), 0, 0]);
            return;
        }
        canal.ConfigAceptada = true;
        AbrirSiCorresponde(canal);
    }

    private void PedidoDeDesconexion(byte id, byte[] d)
    {
        var cid = BinaryPrimitives.ReadUInt16LittleEndian(d);
        var cidRemoto = BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(2));
        if (!_canales.TryGetValue(cid, out var canal) || canal.CidRemoto != cidRemoto)
        {
            Senal(0x01, id, [0x02, 0x00, (byte)cid, (byte)(cid >> 8), (byte)cidRemoto, (byte)(cidRemoto >> 8)]);
            return;
        }
        Senal(0x07, id, d[..4]);
        Quitar(canal);
    }

    private void PedidoDeInformacion(byte id, byte[] d)
    {
        var tipo = BinaryPrimitives.ReadUInt16LittleEndian(d);
        byte[] respuesta = tipo switch
        {
            0x0002 => [0x02, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00], // máscara de funciones: canales fijos
            0x0003 => [0x03, 0x00, 0x00, 0x00, 0x02, 0, 0, 0, 0, 0, 0, 0], // canales fijos: sólo señalización
            _ => [(byte)tipo, (byte)(tipo >> 8), 0x01, 0x00], // no admitido
        };
        Senal(0x0B, id, respuesta);
    }

    private void AbrirSiCorresponde(CanalL2cap canal)
    {
        if (canal.ConfigRecibida && canal.ConfigAceptada && !canal.Abierto)
        {
            canal.Abierto = true;
            registro($"L2CAP: canal abierto PSM 0x{canal.Psm:X4} CID 0x{canal.CidLocal:X4}, MTU remoto {canal.MtuRemoto}");
            canal.Servicio.Abierto(canal);
        }
    }

    private void Quitar(CanalL2cap canal)
    {
        _canales.Remove(canal.CidLocal);
        if (canal.Abierto)
        {
            canal.Abierto = false;
            canal.Servicio.Cerrado(canal);
        }
    }

    /// <summary>Identificador de los pedidos propios: de 1 a 255 (el 0 no es válido).</summary>
    private byte SiguienteId() => _id = (byte)(_id == 255 ? 1 : _id + 1);

    private void Senal(byte codigo, byte id, ReadOnlySpan<byte> datos)
    {
        var s = new byte[4 + datos.Length];
        s[0] = codigo;
        s[1] = id;
        BinaryPrimitives.WriteUInt16LittleEndian(s.AsSpan(2), (ushort)datos.Length);
        datos.CopyTo(s.AsSpan(4));
        enviarTrama(Trama(CidSenalizacion, s));
    }
}
