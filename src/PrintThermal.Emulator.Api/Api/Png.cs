using System.Buffers.Binary;
using System.IO.Compression;

namespace PrintThermal.Emulator.Api.Api;

/// <summary>
/// Lo mínimo de PNG que necesita el host: leer el tamaño de la cabecera IHDR y escribir una imagen de un bit
/// (la de la fuente simulada). El PNG de un ticket real lo genera el intérprete de REQ-HIS-Ticket-A-Png.
/// </summary>
public static class Png
{
    private static readonly byte[] Firma = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly uint[] TablaCrc = CrearTablaCrc();

    /// <summary>Ancho y alto de un PNG según su IHDR, o (0, 0) si los bytes no son un PNG.</summary>
    public static (int Ancho, int Alto) Tamano(ReadOnlySpan<byte> png)
    {
        if (png.Length < 24 || !png[..8].SequenceEqual(Firma) || !png.Slice(12, 4).SequenceEqual("IHDR"u8))
        {
            return (0, 0);
        }

        return ((int)BinaryPrimitives.ReadUInt32BigEndian(png.Slice(16, 4)),
                (int)BinaryPrimitives.ReadUInt32BigEndian(png.Slice(20, 4)));
    }

    /// <summary>
    /// Escribe un PNG en escala de grises de un bit. <paramref name="negro"/> indica, por punto, si va impreso.
    /// </summary>
    public static byte[] DeUnBit(int ancho, int alto, Func<int, int, bool> negro)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ancho, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(alto, 1);
        ArgumentNullException.ThrowIfNull(negro);

        var bytesPorFila = (ancho + 7) / 8;
        var crudo = new byte[(bytesPorFila + 1) * alto];
        for (var y = 0; y < alto; y++)
        {
            var fila = y * (bytesPorFila + 1);
            crudo[fila] = 0; // filtro «None»
            for (var x = 0; x < ancho; x++)
            {
                if (!negro(x, y))
                {
                    crudo[fila + 1 + (x / 8)] |= (byte)(0x80 >> (x % 8)); // 1 = blanco en escala de grises
                }
            }
        }

        using var comprimido = new MemoryStream();
        using (var zlib = new ZLibStream(comprimido, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(crudo);
        }

        var ihdr = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(0, 4), (uint)ancho);
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4, 4), (uint)alto);
        ihdr[8] = 1; // profundidad: un bit
        ihdr[9] = 0; // tipo de color: escala de grises

        using var salida = new MemoryStream();
        salida.Write(Firma);
        Bloque(salida, "IHDR"u8, ihdr);
        Bloque(salida, "IDAT"u8, comprimido.ToArray());
        Bloque(salida, "IEND"u8, []);
        return salida.ToArray();
    }

    private static void Bloque(Stream salida, ReadOnlySpan<byte> tipo, ReadOnlySpan<byte> datos)
    {
        Span<byte> entero = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(entero, (uint)datos.Length);
        salida.Write(entero);
        salida.Write(tipo);
        salida.Write(datos);
        var crc = 0xFFFFFFFFu;
        crc = Crc(crc, tipo);
        crc = Crc(crc, datos);
        BinaryPrimitives.WriteUInt32BigEndian(entero, crc ^ 0xFFFFFFFFu);
        salida.Write(entero);
    }

    private static uint Crc(uint crc, ReadOnlySpan<byte> datos)
    {
        foreach (var b in datos)
        {
            crc = TablaCrc[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] CrearTablaCrc()
    {
        var tabla = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            tabla[n] = c;
        }

        return tabla;
    }
}
