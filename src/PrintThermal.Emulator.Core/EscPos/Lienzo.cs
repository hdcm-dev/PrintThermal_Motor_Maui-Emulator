// cdd: REQ-HIS-Ticket-A-Png
using System.IO.Compression;

namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>Papel de ancho fijo que crece hacia abajo; un byte por punto (1 = negro).</summary>
internal sealed class Lienzo
{
    private readonly List<byte[]> _filas = [];

    public Lienzo(int ancho, int altoMaximo)
    {
        Ancho = ancho;
        AltoMaximo = altoMaximo;
    }

    public int Ancho { get; }

    /// <summary>Tope de filas: lo que cae más abajo no se dibuja (un flujo corrupto no agota la memoria).</summary>
    public int AltoMaximo { get; }

    public int Alto => _filas.Count;

    public void Asegurar(int alto)
    {
        alto = Math.Min(alto, AltoMaximo);
        while (_filas.Count < alto) _filas.Add(new byte[Ancho]);
    }

    public void Marcar(int x, int y)
    {
        if (x < 0 || x >= Ancho || y < 0 || y >= AltoMaximo) return;
        Asegurar(y + 1);
        _filas[y][x] = 1;
    }

    /// <summary>PNG en escala de grises de 1 bit (0 = negro, 1 = blanco), sin filtro.</summary>
    public byte[] APng()
    {
        int alto = Math.Max(1, Alto), porFila = (Ancho + 7) / 8;
        var crudo = new byte[(porFila + 1) * alto];
        for (int y = 0; y < alto; y++)
        {
            int o = y * (porFila + 1) + 1;
            for (int x = 0; x < Ancho; x++)
            {
                bool negro = y < Alto && _filas[y][x] == 1;
                if (!negro) crudo[o + (x >> 3)] |= (byte)(0x80 >> (x & 7));
            }
        }
        using var comprimido = new MemoryStream();
        using (var z = new ZLibStream(comprimido, CompressionLevel.Optimal, leaveOpen: true)) z.Write(crudo);

        using var png = new MemoryStream();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var ihdr = new byte[13];
        Entero(ihdr, 0, Ancho);
        Entero(ihdr, 4, alto);
        ihdr[8] = 1; // profundidad
        ihdr[9] = 0; // escala de grises
        Trozo(png, "IHDR", ihdr);
        Trozo(png, "IDAT", comprimido.ToArray());
        Trozo(png, "IEND", []);
        return png.ToArray();
    }

    private static void Entero(byte[] b, int i, int v)
    {
        b[i] = (byte)(v >> 24);
        b[i + 1] = (byte)(v >> 16);
        b[i + 2] = (byte)(v >> 8);
        b[i + 3] = (byte)v;
    }

    private static void Trozo(Stream s, string tipo, byte[] datos)
    {
        var cab = new byte[8];
        Entero(cab, 0, datos.Length);
        for (int i = 0; i < 4; i++) cab[4 + i] = (byte)tipo[i];
        s.Write(cab);
        s.Write(datos);
        uint crc = Crc32.Calcular(cab.AsSpan(4, 4), datos);
        var c = new byte[4];
        Entero(c, 0, unchecked((int)crc));
        s.Write(c);
    }

    private static class Crc32
    {
        private static readonly uint[] Tabla = Enumerable.Range(0, 256).Select(n =>
        {
            uint c = (uint)n;
            for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            return c;
        }).ToArray();

        public static uint Calcular(ReadOnlySpan<byte> tipo, ReadOnlySpan<byte> datos)
        {
            uint c = 0xFFFFFFFFu;
            foreach (var b in tipo) c = Tabla[(c ^ b) & 0xFF] ^ (c >> 8);
            foreach (var b in datos) c = Tabla[(c ^ b) & 0xFF] ^ (c >> 8);
            return c ^ 0xFFFFFFFFu;
        }
    }
}
