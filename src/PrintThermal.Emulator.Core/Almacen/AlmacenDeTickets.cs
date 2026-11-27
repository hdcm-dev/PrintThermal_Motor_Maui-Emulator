// cdd: REQ-HIS-Almacen-De-Tickets
using System.Globalization;
using System.Text.RegularExpressions;
using PrintThermal.Emulator.Core.EscPos;

namespace PrintThermal.Emulator.Core.Almacen;

/// <summary>
/// Guarda cada ticket en la carpeta de salida como <c>{id}.bin</c> (bytes recibidos) y <c>{id}.png</c>.
/// El id es <c>AAAAMMDDTHHMMSSmmm-NNNN</c> (hora UTC y número de secuencia): ordena por llegada y no sale de la carpeta.
/// La carpeta es la fuente de verdad, así que otra instancia (o un reinicio) ve los mismos tickets.
/// </summary>
public sealed partial class AlmacenDeTickets
{
    private readonly TimeProvider _reloj;
    private readonly Lock _cerrojo = new();
    private int _secuencia = -1;

    public AlmacenDeTickets(string carpeta, TimeProvider? reloj = null)
    {
        Carpeta = Path.GetFullPath(carpeta);
        _reloj = reloj ?? TimeProvider.System;
        Directory.CreateDirectory(Carpeta);
    }

    public string Carpeta { get; }

    [GeneratedRegex(@"^\d{8}T\d{9}-\d{4}$")]
    private static partial Regex FormaDeId();

    /// <summary>Escribe el .bin y después el .png (cada uno por archivo temporal y renombre): un .png visible está completo.</summary>
    public TicketGuardado Guardar(TicketInterpretado ticket)
    {
        lock (_cerrojo)
        {
            if (_secuencia < 0)
                _secuencia = Ids().Select(i => int.Parse(i[^4..], CultureInfo.InvariantCulture)).DefaultIfEmpty(0).Max();
            _secuencia = _secuencia % 9999 + 1;
            var fecha = _reloj.GetUtcNow();
            var id = fecha.ToString("yyyyMMdd'T'HHmmssfff", CultureInfo.InvariantCulture) + "-" + _secuencia.ToString("D4", CultureInfo.InvariantCulture);
            Escribir(id + ".bin", ticket.Bytes);
            Escribir(id + ".png", ticket.Png);
            return new TicketGuardado(id, Fecha(id), ticket.Bytes.LongLength, ticket.Ancho, ticket.Alto);
        }
    }

    /// <summary>Tickets completos (con .bin y .png), del más viejo al más nuevo.</summary>
    public IReadOnlyList<TicketGuardado> Listar() => Ids().Select(Obtener).OfType<TicketGuardado>().ToList();

    public TicketGuardado? Obtener(string id)
    {
        if (!EsId(id)) return null;
        var bin = new FileInfo(Path.Combine(Carpeta, id + ".bin"));
        var png = Path.Combine(Carpeta, id + ".png");
        if (!bin.Exists || !File.Exists(png)) return null;
        var (ancho, alto) = Medidas(png);
        return new TicketGuardado(id, Fecha(id), bin.Length, ancho, alto);
    }

    public TicketGuardado? Ultimo() => Ids().Select(Obtener).OfType<TicketGuardado>().LastOrDefault();

    public byte[]? LeerPng(string id) => Leer(id, ".png");

    public byte[]? LeerBin(string id) => Leer(id, ".bin");

    public bool Borrar(string id)
    {
        if (Obtener(id) is null) return false;
        File.Delete(Path.Combine(Carpeta, id + ".png"));
        File.Delete(Path.Combine(Carpeta, id + ".bin"));
        return true;
    }

    /// <summary>Borra todos los tickets completos y devuelve cuántos eran.</summary>
    public int BorrarTodo() => Ids().Count(Borrar);

    private static bool EsId(string? id) => id is not null && FormaDeId().IsMatch(id);

    private List<string> Ids() =>
        Directory.EnumerateFiles(Carpeta, "*.png")
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(EsId)
            .Order(StringComparer.Ordinal)
            .ToList();

    private byte[]? Leer(string id, string extension)
    {
        if (Obtener(id) is null) return null;
        return File.ReadAllBytes(Path.Combine(Carpeta, id + extension));
    }

    private void Escribir(string nombre, byte[] contenido)
    {
        var destino = Path.Combine(Carpeta, nombre);
        var temporal = destino + ".tmp";
        File.WriteAllBytes(temporal, contenido);
        File.Move(temporal, destino, overwrite: true);
    }

    private static DateTimeOffset Fecha(string id) =>
        DateTimeOffset.ParseExact(id[..18], "yyyyMMdd'T'HHmmssfff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    /// <summary>Ancho y alto del encabezado IHDR (bytes 16 a 23 de todo PNG).</summary>
    private static (int Ancho, int Alto) Medidas(string png)
    {
        Span<byte> cab = stackalloc byte[24];
        using var f = File.OpenRead(png);
        if (f.Read(cab) < 24) return (0, 0);
        return (System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(cab[16..]),
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(cab[20..]));
    }
}
