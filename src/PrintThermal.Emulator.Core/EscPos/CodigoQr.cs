// cdd: REQ-HIS-Ticket-A-Png
namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>
/// Codificador QR modelo 2 en modo byte (versiones 1 a 40), para dibujar lo que la impresora recibe con GS ( k.
/// La tabla de bloques y la selección de máscara siguen a QRCoder 1.8.0 (MIT,
/// https://github.com/Shane32/QRCoder/tree/v1.8.0/QRCoder/QRCodeGenerator, CapacityTables.cs y ModulePlacer*.cs),
/// que es la referencia contra la que se prueba. Cómo codifica la 58HB6 (modo, máscara) no está medido.
/// </summary>
public static class CodigoQr
{
    /// <summary>Nivel de corrección; GS ( k fn 69 lo pide como 48 + nivel (L = 48 … H = 51).</summary>
    public enum Correccion { L, M, Q, H }

    // Por versión y nivel (L, M, Q, H): palabras de datos, EC por bloque, bloques del grupo 1, palabras del grupo 1,
    // bloques del grupo 2, palabras del grupo 2. Transcrito de QRCoder 1.8.0, CapacityTables._capacityECCBaseValues.
    private static readonly int[] Tabla =
    [
        19, 7, 1, 19, 0, 0, 16, 10, 1, 16, 0, 0, 13, 13, 1, 13, 0, 0, 9, 17, 1, 9, 0, 0, 34, 10, 1, 34, 0, 0, 28, 16, 1, 28, 0, 0, 22, 22, 1, 22, 0, 0, 16, 28, 1, 16, 0, 0,
        55, 15, 1, 55, 0, 0, 44, 26, 1, 44, 0, 0, 34, 18, 2, 17, 0, 0, 26, 22, 2, 13, 0, 0, 80, 20, 1, 80, 0, 0, 64, 18, 2, 32, 0, 0, 48, 26, 2, 24, 0, 0, 36, 16, 4, 9, 0, 0,
        108, 26, 1, 108, 0, 0, 86, 24, 2, 43, 0, 0, 62, 18, 2, 15, 2, 16, 46, 22, 2, 11, 2, 12, 136, 18, 2, 68, 0, 0, 108, 16, 4, 27, 0, 0, 76, 24, 4, 19, 0, 0, 60, 28, 4, 15, 0, 0,
        156, 20, 2, 78, 0, 0, 124, 18, 4, 31, 0, 0, 88, 18, 2, 14, 4, 15, 66, 26, 4, 13, 1, 14, 194, 24, 2, 97, 0, 0, 154, 22, 2, 38, 2, 39, 110, 22, 4, 18, 2, 19, 86, 26, 4, 14, 2, 15,
        232, 30, 2, 116, 0, 0, 182, 22, 3, 36, 2, 37, 132, 20, 4, 16, 4, 17, 100, 24, 4, 12, 4, 13, 274, 18, 2, 68, 2, 69, 216, 26, 4, 43, 1, 44, 154, 24, 6, 19, 2, 20, 122, 28, 6, 15, 2, 16,
        324, 20, 4, 81, 0, 0, 254, 30, 1, 50, 4, 51, 180, 28, 4, 22, 4, 23, 140, 24, 3, 12, 8, 13, 370, 24, 2, 92, 2, 93, 290, 22, 6, 36, 2, 37, 206, 26, 4, 20, 6, 21, 158, 28, 7, 14, 4, 15,
        428, 26, 4, 107, 0, 0, 334, 22, 8, 37, 1, 38, 244, 24, 8, 20, 4, 21, 180, 22, 12, 11, 4, 12, 461, 30, 3, 115, 1, 116, 365, 24, 4, 40, 5, 41, 261, 20, 11, 16, 5, 17, 197, 24, 11, 12, 5, 13,
        523, 22, 5, 87, 1, 88, 415, 24, 5, 41, 5, 42, 295, 30, 5, 24, 7, 25, 223, 24, 11, 12, 7, 13, 589, 24, 5, 98, 1, 99, 453, 28, 7, 45, 3, 46, 325, 24, 15, 19, 2, 20, 253, 30, 3, 15, 13, 16,
        647, 28, 1, 107, 5, 108, 507, 28, 10, 46, 1, 47, 367, 28, 1, 22, 15, 23, 283, 28, 2, 14, 17, 15, 721, 30, 5, 120, 1, 121, 563, 26, 9, 43, 4, 44, 397, 28, 17, 22, 1, 23, 313, 28, 2, 14, 19, 15,
        795, 28, 3, 113, 4, 114, 627, 26, 3, 44, 11, 45, 445, 26, 17, 21, 4, 22, 341, 26, 9, 13, 16, 14, 861, 28, 3, 107, 5, 108, 669, 26, 3, 41, 13, 42, 485, 30, 15, 24, 5, 25, 385, 28, 15, 15, 10, 16,
        932, 28, 4, 116, 4, 117, 714, 26, 17, 42, 0, 0, 512, 28, 17, 22, 6, 23, 406, 30, 19, 16, 6, 17, 1006, 28, 2, 111, 7, 112, 782, 28, 17, 46, 0, 0, 568, 30, 7, 24, 16, 25, 442, 24, 34, 13, 0, 0,
        1094, 30, 4, 121, 5, 122, 860, 28, 4, 47, 14, 48, 614, 30, 11, 24, 14, 25, 464, 30, 16, 15, 14, 16, 1174, 30, 6, 117, 4, 118, 914, 28, 6, 45, 14, 46, 664, 30, 11, 24, 16, 25, 514, 30, 30, 16, 2, 17,
        1276, 26, 8, 106, 4, 107, 1000, 28, 8, 47, 13, 48, 718, 30, 7, 24, 22, 25, 538, 30, 22, 15, 13, 16, 1370, 28, 10, 114, 2, 115, 1062, 28, 19, 46, 4, 47, 754, 28, 28, 22, 6, 23, 596, 30, 33, 16, 4, 17,
        1468, 30, 8, 122, 4, 123, 1128, 28, 22, 45, 3, 46, 808, 30, 8, 23, 26, 24, 628, 30, 12, 15, 28, 16, 1531, 30, 3, 117, 10, 118, 1193, 28, 3, 45, 23, 46, 871, 30, 4, 24, 31, 25, 661, 30, 11, 15, 31, 16,
        1631, 30, 7, 116, 7, 117, 1267, 28, 21, 45, 7, 46, 911, 30, 1, 23, 37, 24, 701, 30, 19, 15, 26, 16, 1735, 30, 5, 115, 10, 116, 1373, 28, 19, 47, 10, 48, 985, 30, 15, 24, 25, 25, 745, 30, 23, 15, 25, 16,
        1843, 30, 13, 115, 3, 116, 1455, 28, 2, 46, 29, 47, 1033, 30, 42, 24, 1, 25, 793, 30, 23, 15, 28, 16, 1955, 30, 17, 115, 0, 0, 1541, 28, 10, 46, 23, 47, 1115, 30, 10, 24, 35, 25, 845, 30, 19, 15, 35, 16,
        2071, 30, 17, 115, 1, 116, 1631, 28, 14, 46, 21, 47, 1171, 30, 29, 24, 19, 25, 901, 30, 11, 15, 46, 16, 2191, 30, 13, 115, 6, 116, 1725, 28, 14, 46, 23, 47, 1231, 30, 44, 24, 7, 25, 961, 30, 59, 16, 1, 17,
        2306, 30, 12, 121, 7, 122, 1812, 28, 12, 47, 26, 48, 1286, 30, 39, 24, 14, 25, 986, 30, 22, 15, 41, 16, 2434, 30, 6, 121, 14, 122, 1914, 28, 6, 47, 34, 48, 1354, 30, 46, 24, 10, 25, 1054, 30, 2, 15, 64, 16,
        2566, 30, 17, 122, 4, 123, 1992, 28, 29, 46, 14, 47, 1426, 30, 49, 24, 10, 25, 1096, 30, 24, 15, 46, 16, 2702, 30, 4, 122, 18, 123, 2102, 28, 13, 46, 32, 47, 1502, 30, 48, 24, 14, 25, 1142, 30, 42, 15, 32, 16,
        2812, 30, 20, 117, 4, 118, 2216, 28, 40, 47, 7, 48, 1582, 30, 43, 24, 22, 25, 1222, 30, 10, 15, 67, 16, 2956, 30, 19, 118, 6, 119, 2334, 28, 18, 47, 31, 48, 1666, 30, 34, 24, 34, 25, 1276, 30, 20, 15, 61, 16,
    ];

    private static int Dato(int version, Correccion c, int campo) => Tabla[((version - 1) * 4 + (int)c) * 6 + campo];

    /// <summary>Matriz de módulos (true = oscuro) sin zona de silencio, en la versión mínima que admite los datos.</summary>
    /// <exception cref="ArgumentException">Los datos no entran ni en la versión 40.</exception>
    public static bool[,] Generar(byte[] datos, Correccion correccion)
    {
        int version = 1;
        while (version <= 40 && 4 + (version < 10 ? 8 : 16) + 8 * datos.Length > Dato(version, correccion, 0) * 8) version++;
        if (version > 40) throw new ArgumentException($"{datos.Length} bytes no entran en un QR con corrección {correccion}");

        var palabras = Palabras(datos, version, correccion);
        var q = new Matriz(version);
        q.DibujarFunciones(correccion);
        q.ColocarDatos(palabras);

        int mejor = -1, puntaje = int.MaxValue;
        for (int m = 0; m < 8; m++)
        {
            var prueba = q.ConMascara(m, correccion);
            int p = Penalizacion(prueba);
            if (p < puntaje)
            {
                puntaje = p;
                mejor = m;
            }
        }
        return q.ConMascara(mejor, correccion);
    }

    private static byte[] Palabras(byte[] datos, int version, Correccion c)
    {
        int capacidad = Dato(version, c, 0);
        var bits = new List<bool>();
        void Poner(int valor, int largo)
        {
            for (int i = largo - 1; i >= 0; i--) bits.Add(((valor >> i) & 1) == 1);
        }
        Poner(0b0100, 4);
        Poner(datos.Length, version < 10 ? 8 : 16);
        foreach (var b in datos) Poner(b, 8);
        Poner(0, Math.Min(4, capacidad * 8 - bits.Count));
        while (bits.Count % 8 != 0) bits.Add(false);
        var datosCompletos = new byte[capacidad];
        for (int i = 0; i < bits.Count / 8; i++)
            for (int j = 0; j < 8; j++)
                if (bits[i * 8 + j]) datosCompletos[i] |= (byte)(0x80 >> j);
        for (int i = bits.Count / 8, k = 0; i < capacidad; i++, k++) datosCompletos[i] = (byte)(k % 2 == 0 ? 0xEC : 0x11);

        int ec = Dato(version, c, 1), b1 = Dato(version, c, 2), p1 = Dato(version, c, 3), b2 = Dato(version, c, 4), p2 = Dato(version, c, 5);
        var divisor = Divisor(ec);
        var bloques = new List<byte[]>();
        var correcciones = new List<byte[]>();
        int pos = 0;
        for (int i = 0; i < b1 + b2; i++)
        {
            int largo = i < b1 ? p1 : p2;
            var bloque = datosCompletos.AsSpan(pos, largo).ToArray();
            pos += largo;
            bloques.Add(bloque);
            correcciones.Add(Resto(bloque, divisor));
        }
        var salida = new List<byte>();
        for (int i = 0; i < Math.Max(p1, p2); i++)
            foreach (var b in bloques)
                if (i < b.Length) salida.Add(b[i]);
        for (int i = 0; i < ec; i++)
            foreach (var e in correcciones) salida.Add(e[i]);
        return [.. salida];
    }

    private static int Multiplicar(int x, int y)
    {
        int z = 0;
        for (int i = 7; i >= 0; i--)
        {
            z = (z << 1) ^ ((z >> 7) * 0x11D);
            z ^= ((y >> i) & 1) * x;
        }
        return z;
    }

    private static byte[] Divisor(int grado)
    {
        var r = new byte[grado];
        r[grado - 1] = 1;
        int raiz = 1;
        for (int i = 0; i < grado; i++)
        {
            for (int j = 0; j < r.Length; j++)
            {
                r[j] = (byte)Multiplicar(r[j], raiz);
                if (j + 1 < r.Length) r[j] ^= r[j + 1];
            }
            raiz = Multiplicar(raiz, 2);
        }
        return r;
    }

    private static byte[] Resto(byte[] datos, byte[] divisor)
    {
        var r = new byte[divisor.Length];
        foreach (var b in datos)
        {
            int factor = b ^ r[0];
            Array.Copy(r, 1, r, 0, r.Length - 1);
            r[^1] = 0;
            for (int i = 0; i < r.Length; i++) r[i] ^= (byte)Multiplicar(divisor[i], factor);
        }
        return r;
    }

    /// <summary>Penalización de máscara como la calcula QRCoder 1.8.0 (MaskPattern.Score), sobre la matriz sin zona de silencio.</summary>
    private static int Penalizacion(bool[,] m)
    {
        int n = m.GetLength(0), s1 = 0, s2 = 0, s3 = 0;
        for (int y = 0; y < n; y++)
        {
            int enFila = 0, enColumna = 0;
            bool ultimaFila = m[y, 0], ultimaColumna = m[0, y];
            for (int x = 0; x < n; x++)
            {
                enFila = m[y, x] == ultimaFila ? enFila + 1 : 1;
                if (enFila == 5) s1 += 3; else if (enFila > 5) s1++;
                ultimaFila = m[y, x];
                enColumna = m[x, y] == ultimaColumna ? enColumna + 1 : 1;
                if (enColumna == 5) s1 += 3; else if (enColumna > 5) s1++;
                ultimaColumna = m[x, y];
            }
        }
        for (int y = 0; y < n - 1; y++)
            for (int x = 0; x < n - 1; x++)
                if (m[y, x] == m[y, x + 1] && m[y, x] == m[y + 1, x] && m[y, x] == m[y + 1, x + 1]) s2 += 3;
        bool[] a = [true, false, true, true, true, false, true, false, false, false, false];
        bool[] b = [false, false, false, false, true, false, true, true, true, false, true];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n - 10; x++)
            {
                bool fa = true, fb = true, ca = true, cb = true;
                for (int k = 0; k < 11; k++)
                {
                    fa &= m[y, x + k] == a[k];
                    fb &= m[y, x + k] == b[k];
                    ca &= m[x + k, y] == a[k];
                    cb &= m[x + k, y] == b[k];
                }
                if (fa || fb) s3 += 40;
                if (ca || cb) s3 += 40;
            }
        int oscuros = 0;
        foreach (var v in m) if (v) oscuros++;
        int quinto = oscuros * 20 / (n * n);
        int s4 = Math.Min(Math.Abs(quinto - 10), Math.Abs(quinto - 9)) * 10;
        return s1 + s2 + s3 + s4;
    }

    private sealed class Matriz
    {
        private readonly int _version;
        private readonly int _n;
        private readonly bool[,] _m;
        private readonly bool[,] _funcion;

        public Matriz(int version)
        {
            _version = version;
            _n = 17 + 4 * version;
            _m = new bool[_n, _n];
            _funcion = new bool[_n, _n];
        }

        private void F(bool[,] m, int x, int y, bool v)
        {
            m[y, x] = v;
            _funcion[y, x] = true;
        }

        public void DibujarFunciones(Correccion c)
        {
            for (int i = 0; i < _n; i++)
            {
                F(_m, 6, i, i % 2 == 0);
                F(_m, i, 6, i % 2 == 0);
            }
            Buscador(3, 3);
            Buscador(_n - 4, 3);
            Buscador(3, _n - 4);
            var pos = PosicionesDeAlineacion();
            for (int i = 0; i < pos.Length; i++)
                for (int j = 0; j < pos.Length; j++)
                    if (!(i == 0 && j == 0 || i == 0 && j == pos.Length - 1 || i == pos.Length - 1 && j == 0))
                        for (int dy = -2; dy <= 2; dy++)
                            for (int dx = -2; dx <= 2; dx++)
                                F(_m, pos[i] + dx, pos[j] + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
            Formato(_m, c, 0);
            Version(_m);
        }

        private void Buscador(int x, int y)
        {
            for (int dy = -4; dy <= 4; dy++)
                for (int dx = -4; dx <= 4; dx++)
                {
                    int d = Math.Max(Math.Abs(dx), Math.Abs(dy)), xx = x + dx, yy = y + dy;
                    if (xx >= 0 && xx < _n && yy >= 0 && yy < _n) F(_m, xx, yy, d != 2 && d != 4);
                }
        }

        private int[] PosicionesDeAlineacion()
        {
            if (_version == 1) return [];
            int cantidad = _version / 7 + 2;
            int paso = _version == 32 ? 26 : (_version * 4 + cantidad * 2 + 1) / (cantidad * 2 - 2) * 2;
            var r = new int[cantidad];
            r[0] = 6;
            for (int i = cantidad - 1, p = _n - 7; i >= 1; i--, p -= paso) r[i] = p;
            return r;
        }

        private void Formato(bool[,] m, Correccion c, int mascara)
        {
            int nivel = c switch { Correccion.L => 1, Correccion.M => 0, Correccion.Q => 3, _ => 2 };
            int datos = nivel << 3 | mascara, rem = datos;
            for (int i = 0; i < 10; i++) rem = (rem << 1) ^ ((rem >> 9) * 0x537);
            int bits = (datos << 10 | rem) ^ 0x5412;
            bool B(int i) => ((bits >> i) & 1) == 1;
            for (int i = 0; i <= 5; i++) F(m, 8, i, B(i));
            F(m, 8, 7, B(6));
            F(m, 8, 8, B(7));
            F(m, 7, 8, B(8));
            for (int i = 9; i < 15; i++) F(m, 14 - i, 8, B(i));
            for (int i = 0; i < 8; i++) F(m, _n - 1 - i, 8, B(i));
            for (int i = 8; i < 15; i++) F(m, 8, _n - 15 + i, B(i));
            F(m, 8, _n - 8, true);
        }

        private void Version(bool[,] m)
        {
            if (_version < 7) return;
            int rem = _version;
            for (int i = 0; i < 12; i++) rem = (rem << 1) ^ ((rem >> 11) * 0x1F25);
            int bits = _version << 12 | rem;
            for (int i = 0; i < 18; i++)
            {
                bool v = ((bits >> i) & 1) == 1;
                int a = _n - 11 + i % 3, b = i / 3;
                F(m, a, b, v);
                F(m, b, a, v);
            }
        }

        public void ColocarDatos(byte[] palabras)
        {
            int i = 0;
            for (int derecha = _n - 1; derecha >= 1; derecha -= 2)
            {
                if (derecha == 6) derecha = 5;
                for (int vert = 0; vert < _n; vert++)
                    for (int j = 0; j < 2; j++)
                    {
                        int x = derecha - j;
                        bool subiendo = ((derecha + 1) & 2) == 0;
                        int y = subiendo ? _n - 1 - vert : vert;
                        if (!_funcion[y, x] && i < palabras.Length * 8)
                        {
                            _m[y, x] = ((palabras[i >> 3] >> (7 - (i & 7))) & 1) == 1;
                            i++;
                        }
                    }
            }
        }

        public bool[,] ConMascara(int mascara, Correccion c)
        {
            var r = (bool[,])_m.Clone();
            for (int y = 0; y < _n; y++)
                for (int x = 0; x < _n; x++)
                {
                    if (_funcion[y, x]) continue;
                    bool inv = mascara switch
                    {
                        0 => (x + y) % 2 == 0,
                        1 => y % 2 == 0,
                        2 => x % 3 == 0,
                        3 => (x + y) % 3 == 0,
                        4 => (x / 3 + y / 2) % 2 == 0,
                        5 => x * y % 2 + x * y % 3 == 0,
                        6 => (x * y % 2 + x * y % 3) % 2 == 0,
                        _ => ((x + y) % 2 + x * y % 3) % 2 == 0,
                    };
                    r[y, x] ^= inv;
                }
            Formato(r, c, mascara);
            Version(r);
            return r;
        }
    }
}
