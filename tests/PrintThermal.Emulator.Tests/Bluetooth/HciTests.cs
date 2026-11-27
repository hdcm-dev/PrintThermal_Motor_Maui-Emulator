// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>Escenario no feliz de PAR-PTE-03: netsim no responde; el servicio lo informa en su estado y reintenta.</summary>
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
