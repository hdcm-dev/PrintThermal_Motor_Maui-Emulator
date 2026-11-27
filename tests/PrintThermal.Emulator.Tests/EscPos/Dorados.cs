// cdd: REQ-HIS-Ticket-A-Png
using System.IO.Compression;

namespace PrintThermal.Emulator.Tests.EscPos;

/// <summary>Acceso a los vectores dorados y lectura de PNG para comparar píxeles (no bytes: la compresión puede variar).</summary>
internal static class Dorados
{
    public static string Raiz()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "PrintThermal.Emulator.slnx")))
        {
            dir = dir.Parent;
        }
        Assert.True(dir is not null, "no se encontró PrintThermal.Emulator.slnx subiendo desde " + AppContext.BaseDirectory);
        return dir!.FullName;
    }

    public static string Carpeta => Path.Combine(Raiz(), "tests", "PrintThermal.Emulator.Tests", "EscPos", "Vectores");

    public static byte[] Leer(string archivo) => File.ReadAllBytes(Path.Combine(Carpeta, archivo));

    /// <summary>Deja el PNG obtenido junto a los binarios de la prueba, para mirarlo cuando no coincide.</summary>
    public static string GuardarObtenido(string nombre, byte[] png)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "obtenidos");
        Directory.CreateDirectory(dir);
        var ruta = Path.Combine(dir, nombre + ".png");
        File.WriteAllBytes(ruta, png);
        return ruta;
    }
}

/// <summary>Imagen leída de un PNG en escala de grises de 1 bit: true = punto negro.</summary>
internal sealed record Imagen(int Ancho, int Alto, bool[] Negro)
{
    public bool this[int x, int y] => Negro[y * Ancho + x];

    public static Imagen DePng(byte[] png)
    {
        byte[] firma = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(firma, png[..8]);
        int pos = 8, ancho = 0, alto = 0;
        using var idat = new MemoryStream();
        while (pos < png.Length)
        {
            int largo = (png[pos] << 24) | (png[pos + 1] << 16) | (png[pos + 2] << 8) | png[pos + 3];
            string tipo = System.Text.Encoding.ASCII.GetString(png, pos + 4, 4);
            var datos = png.AsSpan(pos + 8, largo);
            if (tipo == "IHDR")
            {
                ancho = (datos[0] << 24) | (datos[1] << 16) | (datos[2] << 8) | datos[3];
                alto = (datos[4] << 24) | (datos[5] << 16) | (datos[6] << 8) | datos[7];
                Assert.Equal(1, datos[8]);  // profundidad de 1 bit
                Assert.Equal(0, datos[9]);  // escala de grises
                Assert.Equal(0, datos[12]); // sin entrelazado
            }
            else if (tipo == "IDAT")
            {
                idat.Write(datos);
            }
            pos += 12 + largo;
        }
        idat.Position = 0;
        using var z = new ZLibStream(idat, CompressionMode.Decompress);
        using var crudo = new MemoryStream();
        z.CopyTo(crudo);
        var b = crudo.ToArray();
        int porFila = (ancho + 7) / 8;
        Assert.Equal((porFila + 1) * alto, b.Length);
        var negro = new bool[ancho * alto];
        var previa = new byte[porFila];
        var fila = new byte[porFila];
        for (int y = 0; y < alto; y++)
        {
            int f = b[y * (porFila + 1)];
            for (int i = 0; i < porFila; i++)
            {
                int x = b[y * (porFila + 1) + 1 + i];
                int a = i > 0 ? fila[i - 1] : 0, u = previa[i], c = i > 0 ? previa[i - 1] : 0;
                fila[i] = (byte)(f switch
                {
                    0 => x,
                    1 => x + a,
                    2 => x + u,
                    3 => x + ((a + u) >> 1),
                    4 => x + Paeth(a, u, c),
                    _ => throw new InvalidDataException("filtro PNG " + f),
                });
            }
            for (int xx = 0; xx < ancho; xx++)
            {
                negro[y * ancho + xx] = ((fila[xx >> 3] >> (7 - (xx & 7))) & 1) == 0; // 0 = negro en gris de 1 bit
            }
            (previa, fila) = (fila, previa);
        }
        return new Imagen(ancho, alto, negro);
    }

    private static int Paeth(int a, int b, int c)
    {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    public int PuntosNegros => Negro.Count(n => n);
}
