// cdd: REQ-HIS-Recepcion-Integra
using System.Buffers.Binary;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.L2cap;

namespace PrintThermal.Emulator.Bluetooth.Rfcomm;

/// <summary>
/// Sesión RFCOMM (multiplexor TS 07.10 sobre L2CAP, PSM 0x0003) del lado que responde: acepta el DLCI 0 y el canal
/// del perfil de puerto serie, negocia el control de flujo por créditos (PN con 0xF0 → 0xE0), intercambia MSC y
/// entrega los datos de las tramas UIH al receptor, en orden y sin cambios, reponiendo créditos a medida que llegan.
/// </summary>
internal sealed class SesionRfcomm(byte canalServidor, byte creditosIniciales, IReceptorDeBytes receptor,
    Func<long> nuevoId, Action<string> registro) : IServicioL2cap
{
    public const ushort Psm = 0x0003;

    private const byte Sabm = 0x2F;
    private const byte Ua = 0x63;
    private const byte Dm = 0x0F;
    private const byte Disc = 0x43;
    private const byte Uih = 0xEF;
    private const byte Pf = 0x10;

    private const byte Pn = 0x80;
    private const byte Msc = 0xE0;
    private const byte Rpn = 0x90;
    private const byte Rls = 0x50;
    private const byte Test = 0x20;
    private const byte FCon = 0xA0;
    private const byte FCoff = 0x60;
    private const byte Nsc = 0x10;

    private sealed class Dlc(byte dlci)
    {
        public byte Dlci { get; } = dlci;

        public bool Creditos { get; set; }

        public int N1 { get; set; } = 127;

        /// <summary>Créditos que el otro lado todavía puede usar para mandarnos tramas.</summary>
        public int CreditosDados { get; set; }

        /// <summary>Créditos que el otro lado nos dio (para mandarle tramas).</summary>
        public int CreditosRecibidos { get; set; }

        public InfoConexionRfcomm? Conexion { get; set; }
    }

    private readonly Dictionary<byte, Dlc> _dlcs = [];
    private CanalL2cap? _l2cap;
    private bool _abierta;

    public void Abierto(CanalL2cap canal) => _l2cap = canal;

    public void Cerrado(CanalL2cap canal)
    {
        foreach (var d in _dlcs.Values.ToList())
        {
            CerrarDlc(d);
        }
        _abierta = false;
        _l2cap = null;
    }

    public void Recibir(CanalL2cap canal, byte[] sdu)
    {
        if (!Leer(sdu, out var dlci, out var control, out var largoCab, out var largo))
        {
            registro("RFCOMM: trama inválida (largo o FCS); se descarta");
            return;
        }
        var tipo = (byte)(control & ~Pf);
        var pf = (control & Pf) != 0;
        switch (tipo)
        {
            case Sabm:
                Sabm_(dlci);
                break;
            case Disc:
                Disc_(dlci);
                break;
            case Uih:
                Uih_(dlci, pf, sdu.AsSpan(largoCab, largo + (pf && dlci != 0 && Credito(dlci) ? 1 : 0)));
                break;
            case Ua or Dm:
                break; // no se inician conexiones desde este lado
            default:
                registro($"RFCOMM: control 0x{control:X2} no admitido");
                break;
        }
    }

    private bool Credito(byte dlci) => _dlcs.TryGetValue(dlci, out var d) && d.Creditos;

    private void Sabm_(byte dlci)
    {
        if (dlci == 0)
        {
            _abierta = true;
            Enviar(0, respuesta: true, Ua | Pf, []);
            return;
        }
        if (!_abierta || dlci >> 1 != canalServidor)
        {
            registro($"RFCOMM: SABM en DLCI {dlci} sin servidor; DM");
            Enviar(dlci, respuesta: true, Dm | Pf, []);
            return;
        }
        if (!_dlcs.TryGetValue(dlci, out var d))
        {
            d = new Dlc(dlci); // sin PN: valores por defecto, sin créditos
            _dlcs[dlci] = d;
        }
        if (d.Conexion is not null)
        {
            Enviar(dlci, respuesta: true, Ua | Pf, []);
            return;
        }
        d.Conexion = new InfoConexionRfcomm(nuevoId(), _l2cap!.Par, canalServidor);
        Enviar(dlci, respuesta: true, Ua | Pf, []);
        registro($"RFCOMM: abierto DLCI {dlci} (conexión {d.Conexion.Id}, créditos {(d.Creditos ? "sí" : "no")}, N1 {d.N1})");
        receptor.Abierta(d.Conexion);
        // Estado de las señales del módem: listo (EA, RTC, RTR, DV).
        Mcc(Msc, comando: true, [(byte)((dlci << 2) | 0x03), 0x8D]);
    }

    private void Disc_(byte dlci)
    {
        if (dlci == 0)
        {
            Enviar(0, respuesta: true, Ua | Pf, []);
            foreach (var d in _dlcs.Values.ToList())
            {
                CerrarDlc(d);
            }
            _abierta = false;
            return;
        }
        if (_dlcs.TryGetValue(dlci, out var dlc) && dlc.Conexion is not null)
        {
            Enviar(dlci, respuesta: true, Ua | Pf, []);
            CerrarDlc(dlc);
        }
        else
        {
            Enviar(dlci, respuesta: true, Dm | Pf, []);
        }
    }

    private void CerrarDlc(Dlc d)
    {
        _dlcs.Remove(d.Dlci);
        if (d.Conexion is { } c)
        {
            d.Conexion = null;
            registro($"RFCOMM: cerrado DLCI {d.Dlci} (conexión {c.Id})");
            receptor.Cerrada(c);
        }
    }

    private void Uih_(byte dlci, bool pf, ReadOnlySpan<byte> cuerpo)
    {
        if (dlci == 0)
        {
            ControlDelMultiplexor(cuerpo);
            return;
        }
        if (!_dlcs.TryGetValue(dlci, out var d) || d.Conexion is null)
        {
            return;
        }
        if (pf && d.Creditos)
        {
            d.CreditosRecibidos += cuerpo[0];
            cuerpo = cuerpo[1..];
        }
        if (cuerpo.IsEmpty)
        {
            return;
        }
        receptor.Recibidos(d.Conexion, cuerpo);
        if (d.Creditos)
        {
            d.CreditosDados--;
            if (d.CreditosDados <= creditosIniciales / 2)
            {
                var dar = (byte)(creditosIniciales - Math.Max(0, d.CreditosDados));
                d.CreditosDados += dar;
                Enviar(dlci, respuesta: false, Uih | Pf, [], dar);
            }
        }
    }

    private void ControlDelMultiplexor(ReadOnlySpan<byte> info)
    {
        if (info.Length < 2)
        {
            return;
        }
        var tipo = (byte)(info[0] & 0xFC);
        var esComando = (info[0] & 0x02) != 0;
        int largo, cab;
        if ((info[1] & 1) == 1)
        {
            largo = info[1] >> 1;
            cab = 2;
        }
        else
        {
            if (info.Length < 3)
            {
                return;
            }
            largo = (info[1] >> 1) | (info[2] << 7);
            cab = 3;
        }
        if (info.Length < cab + largo)
        {
            return;
        }
        var valores = info.Slice(cab, largo).ToArray();
        if (!esComando)
        {
            return; // respuestas a lo propio (MSC): nada que hacer
        }
        switch (tipo)
        {
            case Pn when valores.Length >= 8:
                NegociarParametros(valores);
                break;
            case Msc when valores.Length >= 2:
                Mcc(Msc, comando: false, valores);
                break;
            case Rpn when valores.Length == 1:
                // Consulta: 9600 baudios, 8 bits, sin paridad, sin control de flujo, XON/XOFF por defecto.
                Mcc(Rpn, comando: false, [valores[0], 0x03, 0x03, 0x00, 0x11, 0x13, 0x00, 0x00]);
                break;
            case Rpn or Rls or Test:
                Mcc(tipo, comando: false, valores);
                break;
            case FCon or FCoff:
                Mcc(tipo, comando: false, []);
                break;
            default:
                Mcc(Nsc, comando: false, [info[0]]);
                break;
        }
    }

    private void NegociarParametros(byte[] v)
    {
        var dlci = (byte)(v[0] & 0x3F);
        if (dlci >> 1 != canalServidor)
        {
            Enviar(dlci, respuesta: true, Dm | Pf, []);
            return;
        }
        if (!_dlcs.TryGetValue(dlci, out var d))
        {
            d = new Dlc(dlci);
            _dlcs[dlci] = d;
        }
        var pedidoN1 = BinaryPrimitives.ReadUInt16LittleEndian(v.AsSpan(4));
        var tope = Math.Min(_l2cap!.MtuLocal, _l2cap.MtuRemoto) - 6;
        d.N1 = Math.Clamp((int)pedidoN1, 1, tope);
        d.Creditos = (v[1] & 0xF0) == 0xF0;
        if (d.Creditos && d.Conexion is null)
        {
            d.CreditosRecibidos = v[7] & 0x07;
            d.CreditosDados = Math.Min(creditosIniciales, (byte)7);
        }
        Mcc(Pn, comando: false,
        [
            dlci, (byte)(d.Creditos ? 0xE0 : 0x00), v[2], 0x00,
            (byte)d.N1, (byte)(d.N1 >> 8), 0x00, (byte)(d.Creditos ? d.CreditosDados : 0),
        ]);
    }

    private void Mcc(byte tipo, bool comando, byte[] valores)
    {
        byte[] info = [(byte)(tipo | (comando ? 0x02 : 0x00) | 0x01), (byte)((valores.Length << 1) | 1), .. valores];
        Enviar(0, respuesta: false, Uih, info);
    }

    /// <summary>
    /// Arma y manda una trama. Este lado es el que responde: sus respuestas (UA, DM) llevan C/R = 1 y sus comandos
    /// (UIH, DISC) C/R = 0 (TS 07.10, §5.2.1.2).
    /// </summary>
    private void Enviar(byte dlci, bool respuesta, int control, ReadOnlySpan<byte> info, byte? creditos = null)
    {
        if (_l2cap is null)
        {
            return;
        }
        var t = new List<byte>(info.Length + 6) { (byte)((dlci << 2) | (respuesta ? 0x02 : 0x00) | 0x01), (byte)control };
        if (info.Length <= 127)
        {
            t.Add((byte)((info.Length << 1) | 1));
        }
        else
        {
            t.Add((byte)(info.Length << 1));
            t.Add((byte)(info.Length >> 7));
        }
        var cubre = ((byte)control & ~Pf) == Uih ? 2 : t.Count;
        if (creditos is byte k)
        {
            t.Add(k);
        }
        t.AddRange(info.ToArray());
        t.Add(Fcs.Calcular(t.GetRange(0, cubre).ToArray()));
        _l2cap.Enviar(t.ToArray());
    }

    /// <summary>Valida largo y FCS. <paramref name="largoCab"/> es la posición del cuerpo (después del largo).</summary>
    private static bool Leer(byte[] t, out byte dlci, out byte control, out int largoCab, out int largo)
    {
        dlci = control = 0;
        largoCab = largo = 0;
        if (t.Length < 4)
        {
            return false;
        }
        dlci = (byte)(t[0] >> 2);
        control = t[1];
        if ((t[2] & 1) == 1)
        {
            largo = t[2] >> 1;
            largoCab = 3;
        }
        else
        {
            largo = (t[2] >> 1) | (t[3] << 7);
            largoCab = 4;
        }
        var esUih = (control & ~Pf & 0xFF) == Uih;
        var cubre = esUih ? 2 : largoCab;
        // El cuerpo puede llevar además un byte de créditos (UIH con P/F en un DLCI con créditos).
        var extra = t.Length - 1 - largoCab - largo;
        return extra is 0 or 1 && Fcs.Calcular(t.AsSpan(0, cubre)) == t[^1];
    }
}
