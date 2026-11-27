using PrintThermal.Emulator.Api.Api;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>Fuente simulada de tickets (PAR-PTE-04, D4).</summary>
public sealed class FuenteSimuladaTests
{
    [Fact]
    public void ImprimeUnTicketDe384PuntosEnElAlmacen()
    {
        var almacen = new AlmacenEnMemoria();
        var fuente = new FuenteSimulada(almacen, TimeProvider.System, 5);

        var ticket = fuente.Imprimir();

        Assert.Same(ticket, almacen.Ultimo());
        Assert.Equal(384, Png.Tamano(ticket.Png).Ancho);
        Assert.True(Png.Tamano(ticket.Png).Alto > 0);
        Assert.Equal([0x1B, 0x40], ticket.Bin[..2]);
        Assert.Equal("simulado", fuente.Nombre);
        Assert.True(fuente.Conectado);
    }

    [Fact]
    public void ElAlmacenDescartaElMasViejoAlLlenarse()
    {
        var almacen = new AlmacenEnMemoria(TimeProvider.System, 2);
        var primero = almacen.Guardar([1], FabricaDelEmulador.PngDePrueba(1));
        almacen.Guardar([2], FabricaDelEmulador.PngDePrueba(2));
        almacen.Guardar([3], FabricaDelEmulador.PngDePrueba(3));

        Assert.Equal(2, almacen.Listar().Count);
        Assert.Null(almacen.Obtener(primero.Id));
    }
}
