// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Text;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Hci.Netsim;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>REQ-02 de PAR-PTE-03: emparejamiento seguro por confirmación, con dirección fija.</summary>
public sealed class SeguridadTests : IDisposable
{
    private static readonly byte[] Clave = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "pte-seg-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    private static async Task<(ControladorFalso C, PerifericoBluetooth P, Task Corrida)> ArrancarAsync(IAlmacenDeClaves claves)
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), new ReceptorEnMemoria(), claves);
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        return (c, p, corrida);
    }

    // REQ-02
    [Fact]
    public async Task ElEmparejamientoPorConfirmacionSeCompleta()
    {
        var archivo = Path.Combine(_dir, "claves.json");
        var (c, _, corrida) = await ArrancarAsync(new AlmacenDeClavesEnArchivo(archivo));
        var android = new AndroidSimulado(c);
        await android.ConectarAclAsync();
        var par = AndroidSimulado.Direccion.ComoHci();

        // Sin clave guardada: Link Key Request → respuesta negativa (empieza el emparejamiento).
        var n = c.Comandos.Count;
        c.Evento(0x17, par);
        Assert.Equal(par, ControladorFalso.Parametros(await c.EsperarComandoAsync(0x040C, n)));

        // IO Capability Request → sin entrada ni salida (0x03), sin OOB, bonding general.
        n = c.Comandos.Count;
        c.Evento(0x31, par);
        Assert.Equal((byte[])[.. par, 0x03, 0x00, 0x04], ControladorFalso.Parametros(await c.EsperarComandoAsync(0x042B, n)));
        c.Evento(0x32, [.. par, 0x01, 0x00, 0x05]);

        // User Confirmation Request → el emulador confirma.
        n = c.Comandos.Count;
        c.Evento(0x33, [.. par, 0x40, 0xE2, 0x01, 0x00]);
        Assert.Equal(par, ControladorFalso.Parametros(await c.EsperarComandoAsync(0x042C, n)));
        c.Evento(0x36, [0x00, .. par]);

        // Link Key Notification → la clave queda guardada en el archivo.
        c.Evento(0x18, [.. par, .. Clave, 0x04]);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(File.Exists(archivo));

        // Reinicio del emulador de impresora con el mismo archivo: Link Key Request → la misma clave, sin reemparejar.
        var (c2, _, corrida2) = await ArrancarAsync(new AlmacenDeClavesEnArchivo(archivo));
        await new AndroidSimulado(c2).ConectarAclAsync();
        n = c2.Comandos.Count;
        c2.Evento(0x17, par);
        Assert.Equal((byte[])[.. par, .. Clave], ControladorFalso.Parametros(await c2.EsperarComandoAsync(0x040B, n)));
        Assert.DoesNotContain(c2.Comandos, k => ControladorFalso.Opcode(k) == 0x042B);
        c2.Cortar();
        await corrida2.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ElPinHeredadoSeContestaConElDeLaImpresora()
    {
        var (c, _, corrida) = await ArrancarAsync(new AlmacenDeClavesEnMemoria());
        var par = AndroidSimulado.Direccion.ComoHci();
        var n = c.Comandos.Count;
        c.Evento(0x16, par);
        var r = ControladorFalso.Parametros(await c.EsperarComandoAsync(0x040D, n));
        Assert.Equal(par, r[..6]);
        Assert.Equal(4, r[6]);
        Assert.Equal("0000"u8.ToArray(), r[7..11]);
        Assert.Equal(23, r.Length);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void LaDireccionFijaViajaANetsimEnOrdenInvertido()
    {
        var d = DireccionBluetooth.Parse("04:00:00:00:00:00");
        Assert.Equal("00:00:00:00:00:04", d.ComoNetsim());
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 4 }, d.ComoHci());

        // PacketRequest{initial_info(1): ChipInfo{chip(2): Chip{kind(1)=BLUETOOTH, address(9)}, device_info(3)}}
        var m = MensajesNetsim.PedidoInicial(d, "58HB6");
        Assert.Equal(0x0A, m[0]);
        var direccion = Encoding.ASCII.GetBytes("00:00:00:00:00:04");
        var i = IndiceDe(m, [0x4A, (byte)direccion.Length, .. direccion]);
        Assert.True(i > 0, "falta Chip.address (campo 9) con la dirección invertida");
        Assert.True(IndiceDe(m, [0x08, 0x01]) > 0, "falta Chip.kind = BLUETOOTH");
    }

    [Fact]
    public void LosPaquetesHciViajanSinElByteH4()
    {
        var m = MensajesNetsim.PedidoHci(new PaqueteHci(TipoPaqueteHci.Comando, [0x03, 0x0C, 0x00]));
        Assert.Equal(new byte[] { 0x12, 0x07, 0x08, 0x01, 0x12, 0x03, 0x03, 0x0C, 0x00 }, m);

        var r = MensajesNetsim.LeerRespuesta([0x12, 0x0B, 0x08, 0x04, 0x12, 0x07, 0x0E, 0x04, 0x01, 0x03, 0x0C, 0x00, 0x00]);
        Assert.Null(r.Error);
        Assert.Equal(TipoPaqueteHci.Evento, r.Hci!.Value.Tipo);
        Assert.Equal(new byte[] { 0x0E, 0x04, 0x01, 0x03, 0x0C, 0x00, 0x00 }, r.Hci!.Value.Datos);

        var e = MensajesNetsim.LeerRespuesta([0x0A, 0x03, (byte)'m', (byte)'a', (byte)'l']);
        Assert.Equal("mal", e.Error);
    }

    private static int IndiceDe(byte[] donde, byte[] que)
    {
        for (var i = 0; i + que.Length <= donde.Length; i++)
        {
            if (donde.AsSpan(i, que.Length).SequenceEqual(que))
            {
                return i;
            }
        }
        return -1;
    }
}
