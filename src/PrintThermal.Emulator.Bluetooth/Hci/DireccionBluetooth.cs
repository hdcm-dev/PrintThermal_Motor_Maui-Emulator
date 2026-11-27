// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Globalization;

namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>Dirección Bluetooth (BD_ADDR) escrita como la muestra Android: <c>04:00:00:00:00:00</c>.</summary>
public sealed class DireccionBluetooth : IEquatable<DireccionBluetooth>
{
    private readonly byte[] _bytes; // en el orden en que se escribe, el más significativo primero

    private DireccionBluetooth(byte[] bytes) => _bytes = bytes;

    public static DireccionBluetooth Parse(string texto)
    {
        var partes = texto.Trim().Split(':');
        if (partes.Length != 6)
        {
            throw new FormatException($"dirección Bluetooth inválida: '{texto}'");
        }
        var bytes = new byte[6];
        for (var i = 0; i < 6; i++)
        {
            if (partes[i].Length != 2 || !byte.TryParse(partes[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
            {
                throw new FormatException($"dirección Bluetooth inválida: '{texto}'");
            }
        }
        return new DireccionBluetooth(bytes);
    }

    /// <summary>Desde los 6 bytes de un paquete HCI, que van en orden little-endian.</summary>
    public static DireccionBluetooth DesdeHci(ReadOnlySpan<byte> hci)
    {
        if (hci.Length < 6)
        {
            throw new ArgumentException("una dirección HCI tiene 6 bytes", nameof(hci));
        }
        var bytes = hci[..6].ToArray();
        Array.Reverse(bytes);
        return new DireccionBluetooth(bytes);
    }

    /// <summary>Los 6 bytes en orden little-endian, como van en HCI.</summary>
    public byte[] ComoHci()
    {
        var hci = (byte[])_bytes.Clone();
        Array.Reverse(hci);
        return hci;
    }

    /// <summary>
    /// La cadena que se pasa en <c>Chip.address</c> a netsim. Netsim la toma en orden invertido: en la prueba corta,
    /// pasar <c>02:11:22:33:44:55</c> hizo que Android viera <c>55:44:33:22:11:02</c> (S6b).
    /// </summary>
    public string ComoNetsim() => string.Join(':', _bytes.Reverse().Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    public override string ToString() => string.Join(':', _bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    public bool Equals(DireccionBluetooth? otra) => otra is not null && _bytes.AsSpan().SequenceEqual(otra._bytes);

    public override bool Equals(object? obj) => Equals(obj as DireccionBluetooth);

    public override int GetHashCode() => BitConverter.ToInt32(_bytes, 0) ^ (_bytes[4] << 8 | _bytes[5]);
}
