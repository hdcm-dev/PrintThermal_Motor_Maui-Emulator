// cdd: REQ-HIS-Almacen-De-Tickets
using System.Text.RegularExpressions;
using PrintThermal.Emulator.Core.Almacen;
using PrintThermal.Emulator.Core.EscPos;
using PrintThermal.Emulator.Tests.EscPos;

namespace PrintThermal.Emulator.Tests.Almacen;

/// <summary>Almacén de tickets: un .bin y un .png por ticket, con su id, en la carpeta de salida (CE-4).</summary>
public sealed class AlmacenTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "pte-almacen-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
    }

    private static TicketInterpretado Ticket(string vector)
    {
        TicketInterpretado? t = null;
        var interprete = new Interprete();
        interprete.TicketTerminado += x => t = x;
        interprete.Alimentar(Dorados.Leer(vector + ".bin"));
        interprete.Finalizar();
        return t ?? throw new InvalidOperationException("sin ticket");
    }

    [Fact]
    public void CadaTicketGuardaBinYPng()
    {
        var almacen = new AlmacenDeTickets(_carpeta);
        var ticket = Ticket("01-texto");

        var guardado = almacen.Guardar(ticket);

        Assert.Matches(new Regex(@"^\d{8}T\d{9}-\d{4}$"), guardado.Id);
        Assert.Equal(ticket.Bytes, File.ReadAllBytes(Path.Combine(_carpeta, guardado.Id + ".bin")));
        Assert.Equal(ticket.Png, File.ReadAllBytes(Path.Combine(_carpeta, guardado.Id + ".png")));
        Assert.Equal((384, ticket.Alto, (long)ticket.Bytes.Length), (guardado.Ancho, guardado.Alto, guardado.BytesRecibidos));
        Assert.Equal(guardado, Assert.Single(almacen.Listar()));
        Assert.All(Directory.EnumerateFiles(_carpeta), f => Assert.True(f.EndsWith(".bin") || f.EndsWith(".png"), f));
    }

    [Fact]
    public void LosIdsSonUnicosYOrdenadosYSobrevivenAUnaNuevaInstancia()
    {
        var almacen = new AlmacenDeTickets(_carpeta);
        var a = almacen.Guardar(Ticket("01-texto"));
        var b = almacen.Guardar(Ticket("02-acentos"));

        var otra = new AlmacenDeTickets(_carpeta);
        var c = otra.Guardar(Ticket("03-imagen"));

        Assert.Equal([a.Id, b.Id, c.Id], otra.Listar().Select(t => t.Id));
        Assert.Equal(c.Id, otra.Ultimo()?.Id);
        Assert.Equal(Ticket("02-acentos").Png, otra.LeerPng(b.Id));
        Assert.Equal(Ticket("02-acentos").Bytes, otra.LeerBin(b.Id));
    }

    [Theory]
    [InlineData("../secreto")]
    [InlineData("20261127T120000000-0001/../../x")]
    [InlineData("")]
    public void UnIdInvalidoNoSaleDeLaCarpeta(string id)
    {
        var almacen = new AlmacenDeTickets(_carpeta);

        Assert.Null(almacen.Obtener(id));
        Assert.Null(almacen.LeerPng(id));
        Assert.False(almacen.Borrar(id));
    }

    [Fact]
    public void BorrarQuitaLosDosArchivos()
    {
        var almacen = new AlmacenDeTickets(_carpeta);
        var a = almacen.Guardar(Ticket("01-texto"));
        almacen.Guardar(Ticket("02-acentos"));

        Assert.True(almacen.Borrar(a.Id));
        Assert.False(File.Exists(Path.Combine(_carpeta, a.Id + ".bin")));
        Assert.Single(almacen.Listar());
        Assert.Equal(1, almacen.BorrarTodo());
        Assert.Empty(almacen.Listar());
    }
}
