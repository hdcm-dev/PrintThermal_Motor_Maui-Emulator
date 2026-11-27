using System.Net;
using System.Text.Json;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>OpenAPI y Scalar (PAR-PTE-04 REQ-03).</summary>
public sealed class OpenApiTests
{
    [Fact]
    public async Task OpenApiYScalarResponden()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        using var openApi = await http.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
        Assert.Equal("application/json", openApi.Content.Headers.ContentType?.MediaType);
        using var documento = JsonDocument.Parse(await openApi.Content.ReadAsStringAsync());
        var rutas = documento.RootElement.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();
        Assert.Contains("/api/estado", rutas);
        Assert.Contains("/api/tickets", rutas);
        Assert.Contains("/api/tickets/ultimo", rutas);
        Assert.Contains("/api/tickets/esperar", rutas);
        Assert.Contains(rutas, r => r.StartsWith("/api/tickets/{id", StringComparison.Ordinal) && r.EndsWith(".png", StringComparison.Ordinal));

        using var scalar = await http.GetAsync("/scalar");
        Assert.Equal(HttpStatusCode.OK, scalar.StatusCode);
        Assert.Equal("text/html", scalar.Content.Headers.ContentType?.MediaType);
        Assert.Contains("scalar", await scalar.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}
