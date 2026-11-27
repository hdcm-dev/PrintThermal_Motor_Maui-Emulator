// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>PAR-PTE-03: escenario no feliz (netsim no responde: el servicio lo informa y reintenta) y señalización L2CAP.</summary>
public class HciTests
{
    [Fact]
    public async Task SiNetsimNoRespondeElServicioLoInformaYReintenta()
    {
        var intentos = 0;
        string? errorVisto = null;
        EstadoPeriferico? estadoVisto = null;
        ControladorFalso? c = null;
        ServicioPeriferico? s = null;
        s = new ServicioPeriferico(_ =>
        {
            intentos++;
            if (intentos == 2)
            {
                errorVisto = s!.UltimoError;
                estadoVisto = s.Estado;
            }
            if (intentos < 3)
            {
                throw new HttpRequestException("netsim no contesta");
            }
            c = new ControladorFalso();
            return Task.FromResult<ITransporteHci>(c);
        }, new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria(), TimeSpan.FromMilliseconds(10));

        using var cts = new CancellationTokenSource();
        var corrida = s.EjecutarAsync(cts.Token);
        await Esperar(() => s.Estado == EstadoPeriferico.Anunciando);
        Assert.Equal(3, s.Intentos);
        Assert.Contains("netsim no contesta", errorVisto);
        Assert.Equal(EstadoPeriferico.Conectando, estadoVisto);

        // Se corta el transporte: el servicio vuelve a conectar.
        c!.Cortar();
        await Esperar(() => s.Intentos == 4 && s.Estado == EstadoPeriferico.Anunciando);

        cts.Cancel();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(EstadoPeriferico.Detenido, s.Estado);
    }

    [Fact]
    public async Task UnaOpcionL2capDesconocidaQueNoEsSugerenciaSeRechaza()
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        var android = new AndroidSimulado(c);
        await android.ConectarAclAsync();

        android.Senal(0x02, 0x01, 0x01, 0x00, 0x41, 0x00); // Connection Request al PSM 1 (SDP) desde el CID 0x0041
        var respuesta = await c.SiguienteL2capAsync();
        Assert.Equal(0x03, respuesta[4]);
        var cidHost = (ushort)(respuesta[8] | (respuesta[9] << 8));
        Assert.Equal(0x04, (await c.SiguienteL2capAsync())[4]); // el host pide su configuración

        // Configuration Request con una opción 0x20 (desconocida, sin el bit de sugerencia) y otra 0xA0 (sugerencia).
        android.Senal(0x04, 0x02, (byte)cidHost, (byte)(cidHost >> 8), 0, 0, 0x20, 0x01, 0x00, 0xA0, 0x01, 0x00);
        var r = await c.SiguienteL2capAsync();
        Assert.Equal(0x05, r[4]);
        Assert.Equal(new byte[] { 0x41, 0x00, 0x00, 0x00, 0x03, 0x00, 0x20 }, r[8..]); // resultado 0x0003 con la opción 0x20

        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static async Task Esperar(Func<bool> condicion)
    {
        var limite = DateTime.UtcNow.AddSeconds(5);
        while (!condicion())
        {
            Assert.True(DateTime.UtcNow < limite, "no se llegó al estado esperado en 5 s");
            await Task.Delay(10);
        }
    }
}
