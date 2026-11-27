// cdd: REQ-HIS-Imagen-Publicada
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PrintThermal.Emulator.Api.Integracion;
using PrintThermal.Emulator.Client;
using PrintThermal.Emulator.Tests.Api;
using PrintThermal.Emulator.Tests.EscPos;

namespace PrintThermal.Emulator.Tests.Distribucion;

/// <summary>
/// PAR-PTE-05 REQ-02. La corrida real (emulador de Android API 37, app de ejemplo de la librería cliente, imagen del
/// emulador de impresora en la red del emulador) está en <c>cdd/parcelas/PAR-PTE-05-Distribucion/evidencia/</c>. De
/// ella quedan los bytes que entregó la API por cada ticket y la huella del PNG que entregó: esta prueba repite la
/// cadena receptor → almacén → API con esos bytes, en trozos chicos, y exige la misma huella. Es un candado de
/// regresión de la corrida real (la huella sale del mismo intérprete), no una prueba de fidelidad contra la 58HB6.
/// </summary>
public sealed class ExtremoAExtremoTests : IDisposable
{
    private readonly string _salida = Path.Combine(Path.GetTempPath(), "pte05-e2e-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_salida))
        {
            Directory.Delete(_salida, recursive: true);
        }
    }

    private static string Captura => Path.Combine(Dorados.Raiz(), "tests", "PrintThermal.Emulator.Tests", "Distribucion", "Captura");

    /// <summary>Líneas «archivo.bin huella-del-png trozo» de la corrida real.</summary>
    public static TheoryData<string, string, int> Capturas => LeerCapturas();

    private static TheoryData<string, string, int> LeerCapturas()
    {
        var datos = new TheoryData<string, string, int>();
        foreach (var linea in File.ReadAllLines(Path.Combine(Captura, "huellas.txt")))
        {
            if (linea.StartsWith('#') || string.IsNullOrWhiteSpace(linea))
            {
                continue;
            }
            var p = linea.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            datos.Add(p[0], p[1], int.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture));
        }
        return datos;
    }

    [Theory]
    [MemberData(nameof(Capturas))]
    public async Task LaAppDeEjemploImprimeYLaApiEntregaElPng(string archivo, string huellaPng, int trozo)
    {
        var bytes = File.ReadAllBytes(Path.Combine(Captura, archivo));
        await using var fabrica = new IntegracionTests.FabricaConSalida(_salida);
        using var http = fabrica.CreateClient();

        IntegracionTests.Transmitir(fabrica.Services.GetRequiredService<ReceptorAlAlmacen>(), 1, bytes, trozo);

        var info = await http.GetFromJsonAsync<TicketInfo>("/api/tickets/ultimo");
        var png = await http.GetByteArrayAsync($"/api/tickets/{info!.Id}.png");
        Assert.Equal(huellaPng, FabricaDelEmulador.Huella(png), ignoreCase: true);
        Assert.Equal(bytes, await http.GetByteArrayAsync($"/api/tickets/{info.Id}.bin"));
    }

    [Fact]
    public void LaCorridaRealDejoCincoTicketsConSuArchivo()
    {
        var capturas = Capturas.ToList();
        Assert.Equal(5, capturas.Count);
        Assert.All(capturas, c => Assert.True(File.Exists(Path.Combine(Captura, (string)c[0])), (string)c[0]));
    }
}
