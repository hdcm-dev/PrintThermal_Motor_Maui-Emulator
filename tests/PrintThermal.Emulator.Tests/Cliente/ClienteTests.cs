using PrintThermal.Emulator.Client;
using PrintThermal.Emulator.Tests.Api;

namespace PrintThermal.Emulator.Tests.Cliente;

/// <summary>Cliente HTTP tipado (REQ-HIS-Cliente-Tipado; PAR-PTE-04 REQ-05).</summary>
public sealed class ClienteTests
{
    [Fact]
    public async Task ElClienteDevuelveElMismoPngQueLaApi()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var cliente = new ClienteDeTickets(http);
        fabrica.Imprimir(1);
        var ultimo = fabrica.Imprimir(2);

        var delCliente = await cliente.ObtenerPngDelUltimoAsync();
        var deLaApi = await http.GetByteArrayAsync("/api/tickets/ultimo.png");

        Assert.NotNull(delCliente);
        Assert.Equal(FabricaDelEmulador.Huella(deLaApi), FabricaDelEmulador.Huella(delCliente));
        Assert.Equal(FabricaDelEmulador.Huella(ultimo.Png), FabricaDelEmulador.Huella(delCliente));
        Assert.Equal(ultimo.Id, (await cliente.ObtenerUltimoAsync())?.Id);
        Assert.Equal(ultimo.Png, await cliente.ObtenerPngAsync(ultimo.Id));
        Assert.Equal(ultimo.Bin, await cliente.ObtenerBinAsync(ultimo.Id));
        Assert.Equal(ultimo.Id, (await cliente.ObtenerAsync(ultimo.Id))?.Id);
        Assert.Equal(2, (await cliente.ListarAsync()).Count);
        Assert.Equal([ultimo.Id], (await cliente.ListarAsync(limite: 1)).Select(t => t.Id));
        Assert.Equal(ultimo.Id, (await cliente.ObtenerEstadoAsync()).UltimoId);
    }

    [Fact]
    public async Task SinTicketsElClienteDevuelveNull()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var cliente = new ClienteDeTickets(http);

        Assert.Null(await cliente.ObtenerUltimoAsync());
        Assert.Null(await cliente.ObtenerPngDelUltimoAsync());
        Assert.Null(await cliente.ObtenerAsync("no-existe"));
        Assert.Null(await cliente.ObtenerPngAsync("no-existe"));
        Assert.Null(await cliente.EsperarAsync(null, TimeSpan.FromSeconds(1)));
        Assert.Empty(await cliente.ListarAsync());
        Assert.Equal(0, (await cliente.ObtenerEstadoAsync()).Tickets);
    }

    [Fact]
    public async Task ElClienteEsperaYBorra()
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        var cliente = new ClienteDeTickets(http);
        var antes = (await cliente.ObtenerEstadoAsync()).UltimoId ?? ClienteDeTickets.DesdeElPrincipio;

        // Patrón del contrato: el ticket llega antes de que empiece la espera y no se pierde.
        var ticket = fabrica.Imprimir(1);
        Assert.Equal(ticket.Id, (await cliente.EsperarAsync(antes, TimeSpan.FromSeconds(10)))?.Id);
        var espera = cliente.EsperarAsync(ticket.Id, TimeSpan.FromSeconds(10));
        var siguiente = fabrica.Imprimir(3);
        Assert.Equal(siguiente.Id, (await espera)?.Id);
        Assert.True(await cliente.BorrarAsync(ticket.Id));
        Assert.False(await cliente.BorrarAsync(ticket.Id));
        fabrica.Imprimir(2);
        await cliente.BorrarTodosAsync();
        Assert.Empty(await cliente.ListarAsync());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cliente.EsperarAsync(null, TimeSpan.Zero));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cliente.EsperarAsync(null, TimeSpan.FromSeconds(61)));
    }
}
