// cdd: REQ-HIS-Api-De-Tickets
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PrintThermal.Emulator.Api.Integracion;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Rfcomm;
using PrintThermal.Emulator.Client;
using PrintThermal.Emulator.Core.EscPos;
using PrintThermal.Emulator.Tests.EscPos;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>
/// El host con el almacén en disco y el receptor RFCOMM conectados (PAR-PTE-05 REQ-03, D2; LAZ-0006, LAZ-0010, LAZ-0001).
/// Los bytes entran por <see cref="IReceptorDeBytes"/>, la misma frontera que usa el periférico Bluetooth.
/// </summary>
public sealed class IntegracionTests : IDisposable
{
    private readonly string _salida = Path.Combine(Path.GetTempPath(), "pte05-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_salida))
        {
            Directory.Delete(_salida, recursive: true);
        }
    }

    /// <summary>El host con <c>Emulador:Salida</c> configurada.</summary>
    public sealed class FabricaConSalida(string salida) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseSetting("Emulador:Salida", salida);
    }

    /// <summary>Entrega <paramref name="datos"/> por el receptor en trozos de <paramref name="trozo"/> bytes y cierra.</summary>
    public static void Transmitir(IReceptorDeBytes receptor, long conexion, byte[] datos, int trozo, bool cerrar = true)
    {
        var info = new InfoConexionRfcomm(conexion, DireccionBluetooth.Parse("02:00:00:00:00:01"), 1);
        receptor.Abierta(info);
        for (var i = 0; i < datos.Length; i += trozo)
        {
            receptor.Recibidos(info, datos.AsSpan(i, Math.Min(trozo, datos.Length - i)));
        }
        if (cerrar)
        {
            receptor.Cerrada(info);
        }
    }

    /// <summary>El PNG que da el intérprete para esos bytes, de una vez y cerrando al final.</summary>
    public static byte[] PngEsperado(byte[] datos)
    {
        var interprete = new Interprete();
        var pngs = new List<byte[]>();
        interprete.TicketTerminado += t => pngs.Add(t.Png);
        interprete.Alimentar(datos);
        interprete.Finalizar();
        return Assert.Single(pngs);
    }

    [Fact]
    public async Task ElTicketRecibidoPorRfcommLlegaALaApi()
    {
        await using var fabrica = new FabricaConSalida(_salida);
        using var http = fabrica.CreateClient();
        var receptor = fabrica.Services.GetRequiredService<ReceptorAlAlmacen>();
        var datos = Dorados.Leer("01-texto.bin");

        Transmitir(receptor, 1, datos, trozo: 7);

        var info = await http.GetFromJsonAsync<TicketInfo>("/api/tickets/ultimo");
        Assert.NotNull(info);
        var png = await http.GetByteArrayAsync($"/api/tickets/{info!.Id}.png");
        var bin = await http.GetByteArrayAsync($"/api/tickets/{info.Id}.bin");
        Assert.Equal(FabricaDelEmulador.Huella(PngEsperado(datos)), FabricaDelEmulador.Huella(png));
        Assert.Equal(datos, bin);
        Assert.Equal(png, File.ReadAllBytes(Path.Combine(_salida, info.Id + ".png")));
        Assert.Equal(datos, File.ReadAllBytes(Path.Combine(_salida, info.Id + ".bin")));
    }

    [Fact]
    public async Task UnTicketSinCorteTerminaAlCerrarLaConexion()
    {
        await using var fabrica = new FabricaConSalida(_salida);
        using var http = fabrica.CreateClient();
        var receptor = fabrica.Services.GetRequiredService<ReceptorAlAlmacen>();
        byte[] datos = [0x1B, 0x40, .. "Hola sin corte\n"u8];

        Transmitir(receptor, 2, datos, trozo: 3, cerrar: false);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("/api/tickets/ultimo")).StatusCode);

        receptor.Cerrada(new InfoConexionRfcomm(2, DireccionBluetooth.Parse("02:00:00:00:00:01"), 1));
        Assert.Equal(datos, await http.GetByteArrayAsync("/api/tickets/ultimo.bin"));
    }

    [Fact]
    public async Task LaEsperaDevuelveElTicketQueLlegaPorRfcomm()
    {
        await using var fabrica = new FabricaConSalida(_salida);
        using var http = fabrica.CreateClient();
        var receptor = fabrica.Services.GetRequiredService<ReceptorAlAlmacen>();

        var espera = http.GetAsync("/api/tickets/esperar?segundos=10");
        await Task.Delay(300);
        Assert.False(espera.IsCompleted, "la espera respondió antes de que llegara un ticket");
        Transmitir(receptor, 3, Dorados.Leer("04-qr.bin"), trozo: 64);

        using var respuesta = await espera;
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var info = await respuesta.Content.ReadFromJsonAsync<TicketInfo>();
        Assert.True(File.Exists(Path.Combine(_salida, info!.Id + ".png")));
    }

    [Fact]
    public async Task OtroHostSobreLaMismaCarpetaVeLosTickets()
    {
        await using (var primera = new FabricaConSalida(_salida))
        {
            _ = primera.CreateClient();
            Transmitir(primera.Services.GetRequiredService<ReceptorAlAlmacen>(), 4, Dorados.Leer("05-ean13.bin"), trozo: 5);
        }

        await using var segunda = new FabricaConSalida(_salida);
        using var http = segunda.CreateClient();
        var lista = await http.GetFromJsonAsync<List<TicketInfo>>("/api/tickets");
        Assert.Single(lista!);
        Assert.Equal(Dorados.Leer("05-ean13.bin"), await http.GetByteArrayAsync("/api/tickets/ultimo.bin"));
    }

    [Fact]
    public async Task BorrarQuitaLosArchivosDeLaCarpeta()
    {
        await using var fabrica = new FabricaConSalida(_salida);
        using var http = fabrica.CreateClient();
        Transmitir(fabrica.Services.GetRequiredService<ReceptorAlAlmacen>(), 5, Dorados.Leer("01-texto.bin"), trozo: 100);
        var info = await http.GetFromJsonAsync<TicketInfo>("/api/tickets/ultimo");

        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"/api/tickets/{info!.Id}")).StatusCode);
        Assert.False(File.Exists(Path.Combine(_salida, info.Id + ".png")));
        Assert.False(File.Exists(Path.Combine(_salida, info.Id + ".bin")));
    }
}
