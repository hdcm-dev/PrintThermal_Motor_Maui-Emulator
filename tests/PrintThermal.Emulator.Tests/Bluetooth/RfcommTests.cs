// cdd: REQ-HIS-Recepcion-Integra
using System.Buffers.Binary;
using System.Security.Cryptography;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Rfcomm;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>REQ-03 de PAR-PTE-03: los bytes que llegan por RFCOMM se entregan sin cambios.</summary>
public class RfcommTests
{
    private const ushort CidAndroid = 0x0041;
    private const byte DlciCanal1 = 2; // canal 1, bit de dirección del iniciador en 0

    [Fact]
    public void LaFcsCoincideConLasTramasDeReferencia()
    {
        // SABM y UA sobre el DLCI 0: F9 03 3F 01 1C F9 y F9 03 73 01 D7 F9 (TS 07.10, modo básico).
        Assert.Equal(0x1C, Fcs.Calcular([0x03, 0x3F, 0x01]));
        Assert.Equal(0xD7, Fcs.Calcular([0x03, 0x73, 0x01]));
        Assert.Equal(AndroidSimulado.FcsBitABit([0x0B, 0xEF]), Fcs.Calcular([0x0B, 0xEF]));
    }

    // REQ-03
    [Fact]
    public async Task LosBytesLleganIntegros()
    {
        var c = new ControladorFalso();
        var receptor = new ReceptorEnMemoria();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), receptor, new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        var android = new AndroidSimulado(c);
        await android.ConectarAclAsync();

        var rnd = new Random(58);
        var cargas = new List<byte[]>();
        for (var i = 0; i < 5; i++)
        {
            var b = new byte[i == 0 ? 4104 : 65536];
            rnd.NextBytes(b);
            cargas.Add(b);
        }

        for (var i = 0; i < cargas.Count; i++)
        {
            var info = await EnviarPorRfcommAsync(c, android, receptor, (ushort)(CidAndroid + i), cargas[i]);
            Assert.Equal(AndroidSimulado.Direccion, info.Par);
            Assert.Equal(1, info.Canal);
            Assert.Equal(SHA256.HashData(cargas[i]), SHA256.HashData(receptor.Datos[info.Id].ToArray()));
        }
        Assert.Equal(5, receptor.Abiertas.Count);
        Assert.Equal(5, receptor.Cerradas.Count);
        Assert.True(c.AclPendientesMaximo <= c.AclPaquetes, $"el host tuvo {c.AclPendientesMaximo} paquetes ACL en vuelo con {c.AclPaquetes} buffers");

        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnCanalSinServidorSeRechazaConDm()
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        var android = new AndroidSimulado(c);
        await android.ConectarAclAsync();
        var (cidHost, _) = await android.AbrirL2capAsync(0x0003, CidAndroid);
        c.Acl(AndroidSimulado.L2cap(cidHost, AndroidSimulado.Trama(0, true, 0x3F, [])));
        Assert.Equal(0x73, AndroidSimulado.Leer(await android.RecibirAsync(CidAndroid), false).Control);
        c.Acl(AndroidSimulado.L2cap(cidHost, AndroidSimulado.Trama(10, true, 0x3F, []))); // canal 5
        var dm = AndroidSimulado.Leer(await android.RecibirAsync(CidAndroid), false);
        Assert.Equal(10, dm.Dlci);
        Assert.Equal(0x1F, dm.Control);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnaDesconexionAMitadDelTicketCierraLaConexionConLoRecibido()
    {
        var c = new ControladorFalso();
        var receptor = new ReceptorEnMemoria();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), receptor, new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        var android = new AndroidSimulado(c);
        await android.ConectarAclAsync();
        var (cidHost, n1, _) = await AbrirCanalAsync(c, android, CidAndroid);

        var parte = new byte[Math.Min(n1, 300)];
        new Random(6).NextBytes(parte);
        var cerrada = receptor.UltimaCerrada.Task;
        c.Acl(AndroidSimulado.L2cap(cidHost, AndroidSimulado.Trama(DlciCanal1, true, 0xEF, parte)));
        c.Evento(0x05, 0x00, (byte)c.Handle, (byte)(c.Handle >> 8), 0x08); // Disconnection Complete: se perdió el enlace

        var info = await cerrada.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(parte, receptor.Datos[info.Id].ToArray());
        Assert.Equal(0, p.ConexionesAcl);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary>L2CAP al PSM 3, SABM y PN con créditos en el DLCI 0, SABM en el canal y MSC. Devuelve N1 y los créditos.</summary>
    private static async Task<(ushort CidHost, int N1, int Creditos)> AbrirCanalAsync(ControladorFalso c, AndroidSimulado android, ushort cid)
    {
        var (cidHost, mtuHost) = await android.AbrirL2capAsync(0x0003, cid);
        void Mandar(byte[] trama) => c.Acl(AndroidSimulado.L2cap(cidHost, trama));
        async Task<AndroidSimulado.TramaLeida> Recibir() => AndroidSimulado.Leer(await android.RecibirAsync(cid), conCreditos: true);

        Mandar(AndroidSimulado.Trama(0, true, 0x3F, []));
        var ua = await Recibir();
        Assert.Equal(((byte)0, true, (byte)0x73), (ua.Dlci, ua.Cr, ua.Control));

        // PN: DLCI 2, pide control por créditos (0xF0), N1 = 1000, da 7 créditos al host.
        Mandar(AndroidSimulado.Trama(0, true, 0xEF, [0x83, 0x11, DlciCanal1, 0xF0, 0x00, 0x00, 0xE8, 0x03, 0x00, 0x07]));
        var pn = await Recibir();
        Assert.Equal(((byte)0, false, (byte)0xEF), (pn.Dlci, pn.Cr, pn.Control));
        Assert.Equal(0x81, pn.Info[0]);
        Assert.Equal(DlciCanal1, pn.Info[2]);
        Assert.Equal(0xE0, pn.Info[3]);
        var n1 = BinaryPrimitives.ReadUInt16LittleEndian(pn.Info.AsSpan(6));
        Assert.InRange(n1, 1, Math.Min(1000, mtuHost - 6));
        var creditos = pn.Info[9] & 0x07;
        Assert.True(creditos > 0, "el host no dio créditos iniciales");

        Mandar(AndroidSimulado.Trama(DlciCanal1, true, 0x3F, []));
        var uaCanal = await Recibir();
        Assert.Equal((DlciCanal1, true, (byte)0x73), (uaCanal.Dlci, uaCanal.Cr, uaCanal.Control));

        // El host manda su MSC; Android le contesta y manda el suyo.
        var mscHost = await Recibir();
        Assert.Equal(0xE3, mscHost.Info[0]);
        Mandar(AndroidSimulado.Trama(0, false, 0xEF, [0xE1, .. mscHost.Info[1..]]));
        Mandar(AndroidSimulado.Trama(0, true, 0xEF, [0xE3, 0x05, (DlciCanal1 << 2) | 3, 0x8D]));
        var mscResp = await Recibir();
        Assert.Equal(new byte[] { 0xE1, 0x05, (DlciCanal1 << 2) | 3, 0x8D }, mscResp.Info);

        return (cidHost, n1, creditos);
    }

    /// <summary>
    /// Lo que hace Android con createRfcommSocketToServiceRecord + write: abre el canal, manda los datos en UIH respetando
    /// los créditos que da el host, y cierra con DISC.
    /// </summary>
    private static async Task<InfoConexionRfcomm> EnviarPorRfcommAsync(ControladorFalso c, AndroidSimulado android, ReceptorEnMemoria receptor, ushort cid, byte[] carga)
    {
        var (cidHost, n1, creditos) = await AbrirCanalAsync(c, android, cid);
        void Mandar(byte[] trama) => c.Acl(AndroidSimulado.L2cap(cidHost, trama));
        async Task<AndroidSimulado.TramaLeida> Recibir() => AndroidSimulado.Leer(await android.RecibirAsync(cid), conCreditos: true);

        for (var o = 0; o < carga.Length;)
        {
            while (creditos == 0)
            {
                var t = await Recibir();
                Assert.Equal(DlciCanal1, t.Dlci);
                Assert.Equal(0xFF, t.Control);
                Assert.Empty(t.Info);
                creditos += t.Creditos!.Value;
            }
            var largo = Math.Min(n1, carga.Length - o);
            Mandar(AndroidSimulado.Trama(DlciCanal1, true, 0xEF, carga[o..(o + largo)]));
            creditos--;
            o += largo;
        }

        var cerrada = receptor.UltimaCerrada.Task;
        Mandar(AndroidSimulado.Trama(DlciCanal1, true, 0x53, []));
        AndroidSimulado.TramaLeida fin;
        do
        {
            fin = await Recibir(); // pueden quedar tramas de crédito antes del UA
        }
        while (fin.Control == 0xFF);
        Assert.Equal((DlciCanal1, (byte)0x73), (fin.Dlci, fin.Control));
        Mandar(AndroidSimulado.Trama(0, true, 0x53, []));
        var ua0 = await Recibir();
        Assert.Equal(((byte)0, (byte)0x73), (ua0.Dlci, ua0.Control));

        android.Senal(0x06, 0x7E, (byte)cidHost, (byte)(cidHost >> 8), (byte)cid, (byte)(cid >> 8));
        var disc = await c.SiguienteL2capAsync();
        Assert.Equal(0x07, disc[4]);
        return await cerrada;
    }
}
