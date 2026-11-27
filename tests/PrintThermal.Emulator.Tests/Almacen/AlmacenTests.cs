// cdd: REQ-HIS-Almacen-De-Tickets
using System.Text.RegularExpressions;
using PrintThermal.Emulator.Core.Almacen;
using PrintThermal.Emulator.Core.EscPos;
using PrintThermal.Emulator.Tests.EscPos;

namespace PrintThermal.Emulator.Tests.Almacen;

/// <summary>Almacén de tickets: un .bin y un .png por ticket, con su id, en la carpeta de salida (CE-4).</summary>
public sealed class AlmacenTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "pte-almacen-" + Guid.NewGuid().ToString("N"));

    private string _carpeta => Path.Combine(_raiz, "salida");

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
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

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
    }

    [Fact]
    public void CadaTicketGuardaBinYPng()
    {
        // De punta a punta: el intérprete termina el ticket y el almacén lo guarda sin que nadie llame a Guardar.
        var almacen = new AlmacenDeTickets(_carpeta);
        var interprete = new Interprete();
        using var suscripcion = almacen.Escuchar(interprete);
        TicketInterpretado? ticket = null;
        interprete.TicketTerminado += t => ticket = t;
        var bytes = Dorados.Leer("01-texto.bin");
        for (int i = 0; i < bytes.Length; i += 256) interprete.Alimentar(bytes.AsSpan(i, Math.Min(256, bytes.Length - i)));

        Assert.NotNull(ticket);
        var guardado = Assert.Single(almacen.Listar());

        Assert.Matches(new Regex(@"^\d{8}T\d{9}-\d{4}$"), guardado.Id);
        Assert.Equal(ticket.Bytes, File.ReadAllBytes(Path.Combine(_carpeta, guardado.Id + ".bin")));
        Assert.Equal(ticket.Png, File.ReadAllBytes(Path.Combine(_carpeta, guardado.Id + ".png")));
        Assert.Equal((384, ticket.Alto, (long)ticket.Bytes.Length), (guardado.Ancho, guardado.Alto, guardado.BytesRecibidos));
        Assert.Equal(guardado, almacen.Ultimo());
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

    [Fact]
    public void DosInstanciasEnElMismoMilisegundoNoSePisan()
    {
        // Intercaladas: uno guarda (secuencia 1), el otro guarda (lee la carpeta: 2) y el primero vuelve con su secuencia
        // propia, que también da 2 en el mismo milisegundo. Tiene que saltar a 3 sin pisar el ticket del otro.
        var reloj = new RelojFijo(new DateTimeOffset(2026, 11, 27, 12, 0, 0, TimeSpan.Zero));
        var uno = new AlmacenDeTickets(_carpeta, reloj);
        var otro = new AlmacenDeTickets(_carpeta, reloj);
        var a = uno.Guardar(Ticket("01-texto"));
        var b = otro.Guardar(Ticket("02-acentos"));
        var c = uno.Guardar(Ticket("03-imagen"));

        Assert.Equal(["20261127T120000000-0001", "20261127T120000000-0002", "20261127T120000000-0003"], new[] { a.Id, b.Id, c.Id });
        Assert.Equal(Ticket("01-texto").Bytes, otro.LeerBin(a.Id));
        Assert.Equal(Ticket("02-acentos").Bytes, otro.LeerBin(b.Id));
        Assert.Equal(Ticket("03-imagen").Bytes, otro.LeerBin(c.Id));
        Assert.Empty(Directory.EnumerateFiles(_carpeta, "*.tmp"));
    }

    [Fact]
    public void UnErrorDeDiscoNoTumbaAlInterprete()
    {
        var almacen = new AlmacenDeTickets(_carpeta);
        Directory.Delete(_carpeta);
        File.WriteAllBytes(_carpeta, [0]); // la carpeta ahora es un archivo: toda escritura falla
        var errores = new List<Exception>();
        almacen.ErrorAlGuardar += (ticket, error) => errores.Add(error);
        var interprete = new Interprete();
        using var suscripcion = almacen.Escuchar(interprete);
        var tickets = new List<TicketInterpretado>();
        interprete.TicketTerminado += tickets.Add;

        interprete.Alimentar(Dorados.Leer("01-texto.bin"));
        interprete.Alimentar(Dorados.Leer("02-acentos.bin"));

        Assert.Equal(2, tickets.Count);
        Assert.Equal(2, errores.Count);
    }

    [Fact]
    public void UnArchivoConFormaDeIdPeroFechaImposibleNoRompeElListado()
    {
        Directory.CreateDirectory(_carpeta);
        File.WriteAllBytes(Path.Combine(_carpeta, "20261399T120000000-0001.png"), [1]);
        File.WriteAllBytes(Path.Combine(_carpeta, "20261399T120000000-0001.bin"), [1]);
        var almacen = new AlmacenDeTickets(_carpeta);
        var bueno = almacen.Guardar(Ticket("01-texto"));

        Assert.Equal(bueno.Id, Assert.Single(almacen.Listar()).Id);
        Assert.Null(almacen.Obtener("20261399T120000000-0001"));
    }

    [Theory]
    [InlineData("20261127T120000000-0001\n")]
    [InlineData("../secreto")]
    [InlineData("20261127T120000000-0001/../../x")]
    [InlineData("")]
    public void UnIdInvalidoNoSaleDeLaCarpeta(string id)
    {
        var almacen = new AlmacenDeTickets(_carpeta);
        // Señuelos donde caería el id si se usara como ruta: no deben leerse ni borrarse.
        var senuelos = new[] { ".png", ".bin" }.Select(e => Path.GetFullPath(Path.Combine(_carpeta, id + e))).ToArray();
        Assert.All(senuelos, f => Assert.StartsWith(_raiz, f));
        foreach (var f in senuelos)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(f)!);
            File.WriteAllBytes(f, [0x89, 0x50, 0x4E, 0x47]);
        }

        Assert.Null(almacen.Obtener(id));
        Assert.Null(almacen.LeerPng(id));
        Assert.Null(almacen.LeerBin(id));
        Assert.False(almacen.Borrar(id));
        Assert.All(senuelos, f => Assert.True(File.Exists(f), f));
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
