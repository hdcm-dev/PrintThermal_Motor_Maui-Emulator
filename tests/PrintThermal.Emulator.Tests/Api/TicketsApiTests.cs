using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using PrintThermal.Emulator.Client;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>API de tickets (REQ-HIS-Api-De-Tickets; PAR-PTE-04 REQ-01 y REQ-02, y casos borde).</summary>
public sealed class TicketsApiTests
{
    [Fact]
    public async Task ElPngLlegaConLaMismaHuella()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        // CE-5: 5 de 5.
        for (var i = 1; i <= 5; i++)
        {
            var png = FabricaDelEmulador.PngDePrueba(i);
            var ticket = fabrica.Almacen.Guardar([0x1B, 0x40, (byte)i], png);

            using var respuesta = await http.GetAsync($"/api/tickets/{ticket.Id}.png");

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal("image/png", respuesta.Content.Headers.ContentType?.MediaType);
            Assert.Equal(FabricaDelEmulador.Huella(png), FabricaDelEmulador.Huella(await respuesta.Content.ReadAsByteArrayAsync()));
        }

        using var ultimo = await http.GetAsync("/api/tickets/ultimo.png");
        Assert.Equal("image/png", ultimo.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            FabricaDelEmulador.Huella(FabricaDelEmulador.PngDePrueba(5)),
            FabricaDelEmulador.Huella(await ultimo.Content.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task LaEsperaVenceSinColgarse()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        var reloj = Stopwatch.StartNew();
        using var respuesta = await http.GetAsync("/api/tickets/esperar?segundos=1");
        reloj.Stop();

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.InRange(reloj.Elapsed, TimeSpan.FromMilliseconds(900), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task LaEsperaDevuelveElTicketQueLlega()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        var espera = http.GetAsync("/api/tickets/esperar?segundos=10");
        await Task.Delay(300);
        Assert.False(espera.IsCompleted, "la espera respondió antes de que llegara un ticket");
        var ticket = fabrica.Imprimir(1);

        using var respuesta = await espera;
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(ticket.Id, (await respuesta.Content.ReadFromJsonAsync<TicketInfo>())!.Id);
    }

    [Fact]
    public async Task LaEsperaConDespuesDeDevuelveElSiguienteSinEsperar()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var primero = fabrica.Imprimir(1);
        var segundo = fabrica.Imprimir(2);
        fabrica.Imprimir(3);

        using var respuesta = await http.GetAsync($"/api/tickets/esperar?segundos=1&despuesDe={primero.Id}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(segundo.Id, (await respuesta.Content.ReadFromJsonAsync<TicketInfo>())!.Id);
    }

    [Fact]
    public async Task LaEsperaSinDespuesDeNoDevuelveUnTicketViejo()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        fabrica.Imprimir(1);

        using var respuesta = await http.GetAsync("/api/tickets/esperar?segundos=1");

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
    }

    [Theory]
    [InlineData("segundos=0")]
    [InlineData("segundos=61")]
    [InlineData("segundos=abc")]
    public async Task LaEsperaFueraDeRangoDa400(string consulta)
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        using var respuesta = await http.GetAsync("/api/tickets/esperar?" + consulta);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SinTicketsLaListaEstaVaciaYElUltimoDa404()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();

        Assert.Empty((await http.GetFromJsonAsync<List<TicketInfo>>("/api/tickets"))!);
        using var ultimo = await http.GetAsync("/api/tickets/ultimo");
        Assert.Equal(HttpStatusCode.NotFound, ultimo.StatusCode);
        Assert.Equal("application/problem+json", ultimo.Content.Headers.ContentType?.MediaType);
        using var png = await http.GetAsync("/api/tickets/ultimo.png");
        Assert.Equal(HttpStatusCode.NotFound, png.StatusCode);
    }

    [Theory]
    [InlineData("/api/tickets/no-existe")]
    [InlineData("/api/tickets/no-existe.png")]
    [InlineData("/api/tickets/no-existe.bin")]
    [InlineData("/api/tickets/con.punto")]
    public async Task UnIdInexistenteDa404(string ruta)
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        fabrica.Imprimir(1);

        using var respuesta = await http.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LaListaYElBinRespondenConLosDatosDelTicket()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var viejo = fabrica.Imprimir(1);
        var nuevo = fabrica.Imprimir(2);

        var lista = (await http.GetFromJsonAsync<List<TicketInfo>>("/api/tickets"))!;
        Assert.Equal([nuevo.Id, viejo.Id], lista.Select(t => t.Id));
        Assert.Equal(384, lista[0].AnchoPx);
        Assert.Equal(42, lista[0].AltoPx);
        Assert.Equal(nuevo.Bin.Length, lista[0].BytesBin);
        Assert.Equal(nuevo.Png.Length, lista[0].BytesPng);
        Assert.Single((await http.GetFromJsonAsync<List<TicketInfo>>("/api/tickets?limite=1"))!);
        using var limiteMalo = await http.GetAsync("/api/tickets?limite=0");
        Assert.Equal(HttpStatusCode.BadRequest, limiteMalo.StatusCode);
        Assert.Equal("application/problem+json", limiteMalo.Content.Headers.ContentType?.MediaType);

        using var bin = await http.GetAsync($"/api/tickets/{viejo.Id}.bin");
        Assert.Equal("application/octet-stream", bin.Content.Headers.ContentType?.MediaType);
        Assert.Equal(viejo.Bin, await bin.Content.ReadAsByteArrayAsync());

        var estado = (await http.GetFromJsonAsync<EstadoDelEmulador>("/api/estado"))!;
        Assert.Equal(2, estado.Tickets);
        Assert.Equal(nuevo.Id, estado.UltimoId);
        Assert.Equal("sin medio", estado.Medio);
        Assert.False(estado.MedioConectado);
    }

    [Fact]
    public async Task LosIdsSonOrdenablesYSinPuntos()
    {
        await using var fabrica = new FabricaDelEmulador();
        var ids = Enumerable.Range(1, 20).Select(i => fabrica.Imprimir(i).Id).ToList();

        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.DoesNotContain(ids, id => id.Contains('.') || id.Contains('/'));
    }

    [Fact]
    public async Task BorrarUnoYBorrarTodos()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var uno = fabrica.Imprimir(1);
        fabrica.Imprimir(2);

        using (var borrado = await http.DeleteAsync($"/api/tickets/{uno.Id}"))
        {
            Assert.Equal(HttpStatusCode.NoContent, borrado.StatusCode);
        }

        using (var otraVez = await http.DeleteAsync($"/api/tickets/{uno.Id}"))
        {
            Assert.Equal(HttpStatusCode.NotFound, otraVez.StatusCode);
        }

        using (var todos = await http.DeleteAsync("/api/tickets"))
        {
            Assert.Equal(HttpStatusCode.NoContent, todos.StatusCode);
        }

        Assert.Empty((await http.GetFromJsonAsync<List<TicketInfo>>("/api/tickets"))!);
    }
}
