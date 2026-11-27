using System.Net;
using PrintThermal.Emulator.Tests.Api;

namespace PrintThermal.Emulator.Tests.Visor;

/// <summary>Visor Blazor (REQ-HIS-Visor-Web; PAR-PTE-04 REQ-04). El recorrido interactivo va en navegador real.</summary>
public sealed class VisorTests
{
    [Fact]
    public async Task ElVisorMuestraElUltimoTicket()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var viejo = fabrica.Imprimir(1);
        var ultimo = fabrica.Imprimir(2);

        using var respuesta = await http.GetAsync("/");
        var html = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("data-estado=\"datos\"", html);
        Assert.Contains($"data-ticket=\"{viejo.Id}\"", html);
        Assert.Contains($"data-ticket=\"{ultimo.Id}\"", html);
        Assert.Contains($"src=\"api/tickets/{ultimo.Id}.png\"", html);
        Assert.Contains($"href=\"api/tickets/{ultimo.Id}.bin\"", html);
        // Los datos llegaron por la API, con el cliente tipado del visor.
        Assert.Contains("/api/tickets", fabrica.PedidosDelVisor);
        Assert.Contains("/api/estado", fabrica.PedidosDelVisor);
    }

    [Fact]
    public async Task SinTicketsElVisorMuestraElEstadoVacio()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        var html = await http.GetStringAsync("/");

        Assert.Contains("data-estado=\"vacio\"", html);
        Assert.Contains("Todavía no llegó ningún ticket", html);
    }
}
