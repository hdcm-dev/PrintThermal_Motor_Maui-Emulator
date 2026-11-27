// cdd: REQ-HIS-Ticket-A-Png
using System.IO.Compression;
using System.Text;

namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>
/// Fuente A de 12 x 24 puntos: Terminus Font en formato PSF2 (normal y negrita), con su tabla Unicode.
/// La métrica real de la 58HB6 no está medida (Especificacion-58HB6 §8, ítem 4).
/// </summary>
internal sealed class Fuente
{
    public const int Ancho = 12;
    public const int Alto = 24;

    private static readonly Lazy<Fuente> NormalPerezosa = new(() => Cargar("Uni2-Terminus24x12.psf.gz"));
    private static readonly Lazy<Fuente> NegritaPerezosa = new(() => Cargar("Uni2-TerminusBold24x12.psf.gz"));

    public static Fuente Normal => NormalPerezosa.Value;
    public static Fuente Negrita => NegritaPerezosa.Value;

    private readonly byte[] _glifos;
    private readonly int _bytesPorGlifo;
    private readonly int _bytesPorFila;
    private readonly Dictionary<int, int> _porCodigo;

    private Fuente(byte[] glifos, int bytesPorGlifo, int bytesPorFila, Dictionary<int, int> porCodigo)
    {
        _glifos = glifos;
        _bytesPorGlifo = bytesPorGlifo;
        _bytesPorFila = bytesPorFila;
        _porCodigo = porCodigo;
    }

    public bool Tiene(int codigo) => _porCodigo.ContainsKey(codigo);

    /// <summary>Punto (x, y) del glifo del carácter; si la fuente no lo tiene, el de «?».</summary>
    public bool Punto(int codigo, int x, int y)
    {
        if (!_porCodigo.TryGetValue(codigo, out var indice)) indice = _porCodigo['?'];
        var b = _glifos[indice * _bytesPorGlifo + y * _bytesPorFila + (x >> 3)];
        return ((b >> (7 - (x & 7))) & 1) == 1;
    }

    private static Fuente Cargar(string nombre)
    {
        using var recurso = typeof(Fuente).Assembly.GetManifestResourceStream("Fuentes/" + nombre)
            ?? throw new InvalidOperationException("falta el recurso Fuentes/" + nombre);
        using var gz = new GZipStream(recurso, CompressionMode.Decompress);
        using var ms = new MemoryStream();
        gz.CopyTo(ms);
        var d = ms.ToArray();
        int U(int i) => BitConverter.ToInt32(d, i);
        if (U(0) != unchecked((int)0x864AB572)) throw new InvalidDataException(nombre + " no es PSF2");
        int cabecera = U(8), banderas = U(12), cantidad = U(16), porGlifo = U(20), alto = U(24), ancho = U(28);
        if (alto != Alto || ancho != Ancho) throw new InvalidDataException($"{nombre}: {ancho}x{alto}, se esperaba {Ancho}x{Alto}");
        var glifos = d.AsSpan(cabecera, cantidad * porGlifo).ToArray();
        var porCodigo = new Dictionary<int, int>();
        if ((banderas & 1) == 1)
        {
            int p = cabecera + cantidad * porGlifo;
            for (int g = 0; g < cantidad && p < d.Length; g++)
            {
                int fin = Array.IndexOf(d, (byte)0xFF, p);
                int secuencias = Array.IndexOf(d, (byte)0xFE, p, fin - p);
                var texto = Encoding.UTF8.GetString(d, p, (secuencias < 0 ? fin : secuencias) - p);
                foreach (var r in texto.EnumerateRunes()) porCodigo.TryAdd(r.Value, g);
                p = fin + 1;
            }
        }
        return new Fuente(glifos, porGlifo, (ancho + 7) / 8, porCodigo);
    }
}

/// <summary>Tablas de caracteres de ESC t n que el emulador conoce (Especificacion-58HB6 §6: al menos 0, 2 y 19).</summary>
internal static class TablasDeCaracteres
{
    private static readonly Dictionary<int, int> PaginaPorN = new()
    {
        [0] = 437, [2] = 850, [3] = 860, [4] = 863, [5] = 865, [16] = 1252, [17] = 866, [19] = 858,
    };

    // Los inicializadores corren en orden: el proveedor se registra antes de pedir la primera página.
    private static readonly bool Registrado = Registrar();

    private static bool Registrar()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return true;
    }

    public static Encoding? Para(int n) => Registrado && PaginaPorN.TryGetValue(n, out var cp) ? Encoding.GetEncoding(cp) : null;

    public static Encoding Pc437 { get; } = Para(0)!;
}
