// cdd: REQ-HIS-Ticket-A-Png
namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>
/// EAN-13: dígito verificador módulo 10 con pesos 1 y 3 (el último dígito de datos pesa 3) y módulos de las tablas L, G y R.
/// Fuente de las tablas: https://en.wikipedia.org/wiki/International_Article_Number (consultada el 2026-11-27).
/// </summary>
public static class CodigoEan13
{
    private static readonly string[] L = ["0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011"];
    private static readonly string[] G = ["0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111"];
    private static readonly string[] R = ["1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100"];
    private static readonly string[] Paridad = ["LLLLLL", "LLGLGG", "LLGGLG", "LLGGGL", "LGLLGG", "LGGLLG", "LGGGLL", "LGLGLG", "LGLGGL", "LGGLGL"];

    /// <summary>Doce dígitos: les agrega el verificador. Trece: los devuelve si el verificador es correcto. Otro caso: null.</summary>
    public static string? Completar(string digitos)
    {
        if (digitos.Length is not (12 or 13) || !digitos.All(char.IsAsciiDigit)) return null;
        int suma = 0;
        for (int i = 0; i < 12; i++) suma += (digitos[i] - '0') * (i % 2 == 0 ? 1 : 3);
        char verificador = (char)('0' + (10 - suma % 10) % 10);
        if (digitos.Length == 13) return digitos[12] == verificador ? digitos : null;
        return digitos + verificador;
    }

    /// <summary>Los 95 módulos (true = barra) de un EAN-13 completo, con las guardas.</summary>
    public static bool[] Modulos(string trece)
    {
        if (trece.Length != 13 || !trece.All(char.IsAsciiDigit)) throw new ArgumentException("se esperan 13 dígitos", nameof(trece));
        var s = new System.Text.StringBuilder("101");
        var paridad = Paridad[trece[0] - '0'];
        for (int i = 1; i <= 6; i++) s.Append((paridad[i - 1] == 'L' ? L : G)[trece[i] - '0']);
        s.Append("01010");
        for (int i = 7; i <= 12; i++) s.Append(R[trece[i] - '0']);
        s.Append("101");
        return s.ToString().Select(c => c == '1').ToArray();
    }
}
