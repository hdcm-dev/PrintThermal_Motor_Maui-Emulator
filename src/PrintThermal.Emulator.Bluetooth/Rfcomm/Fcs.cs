// cdd: REQ-HIS-Recepcion-Integra
namespace PrintThermal.Emulator.Bluetooth.Rfcomm;

/// <summary>
/// FCS de RFCOMM (TS 07.10): CRC-8 de polinomio x^8 + x^2 + x + 1 en forma reflejada (0xE0), valor inicial 0xFF,
/// y la FCS es el complemento a 0xFF del resto.
/// </summary>
public static class Fcs
{
    private static readonly byte[] Tabla = CrearTabla();

    public static byte Calcular(ReadOnlySpan<byte> datos)
    {
        byte crc = 0xFF;
        foreach (var b in datos)
        {
            crc = Tabla[crc ^ b];
        }
        return (byte)(0xFF - crc);
    }

    private static byte[] CrearTabla()
    {
        var t = new byte[256];
        for (var i = 0; i < 256; i++)
        {
            var c = (byte)i;
            for (var j = 0; j < 8; j++)
            {
                c = (c & 1) != 0 ? (byte)((c >> 1) ^ 0xE0) : (byte)(c >> 1);
            }
            t[i] = c;
        }
        return t;
    }
}
