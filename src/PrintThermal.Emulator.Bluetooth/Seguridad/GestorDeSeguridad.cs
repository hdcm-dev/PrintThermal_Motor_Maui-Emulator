// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Text;
using PrintThermal.Emulator.Bluetooth.Hci;

namespace PrintThermal.Emulator.Bluetooth.Seguridad;

/// <summary>
/// Emparejamiento seguro simple (SSP) del lado de la impresora: sin entrada ni salida (como una impresora sin pantalla),
/// así que Android pide el consentimiento y este lado confirma; guarda la clave de enlace y la devuelve en las
/// reconexiones para no volver a emparejar. Si Android usa emparejamiento heredado, contesta con el PIN configurado.
/// </summary>
internal sealed class GestorDeSeguridad(HostHci host, IAlmacenDeClaves claves, string pin, Action<string> registro)
{
    /// <summary>IO_Capability: NoInputNoOutput.</summary>
    public const byte SinEntradaNiSalida = 0x03;

    /// <summary>Authentication_Requirements: bonding general, sin protección MITM (no hay cómo comparar un número).</summary>
    public const byte BondingGeneral = 0x04;

    public static bool Atiende(byte codigo) => codigo is 0x16 or 0x17 or 0x18 or 0x31 or 0x32 or 0x33 or 0x34 or 0x36;

    public void Procesar(byte codigo, byte[] p)
    {
        if (p.Length < 6)
        {
            return;
        }
        var direccion = p[..6];
        var par = DireccionBluetooth.DesdeHci(direccion);
        switch (codigo)
        {
            case 0x17: // Link Key Request
                if (claves.Buscar(par) is { } clave)
                {
                    registro($"seguridad: clave de enlace guardada para {par}");
                    host.Enviar(0x040B, [.. direccion, .. clave.Clave]);
                }
                else
                {
                    host.Enviar(0x040C, direccion);
                }
                break;
            case 0x18 when p.Length >= 23: // Link Key Notification
                claves.Guardar(par, new ClaveDeEnlace(p[6..22], p[22]));
                registro($"seguridad: vínculo guardado con {par} (tipo de clave {p[22]})");
                break;
            case 0x31: // IO Capability Request
                host.Enviar(0x042B, [.. direccion, SinEntradaNiSalida, 0x00, BondingGeneral]);
                break;
            case 0x32 when p.Length >= 9: // IO Capability Response
                registro($"seguridad: {par} declara IO 0x{p[6]:X2}, autenticación 0x{p[8]:X2}");
                break;
            case 0x33: // User Confirmation Request: se confirma
                registro($"seguridad: confirmación pedida por {par}; se acepta");
                host.Enviar(0x042C, direccion);
                break;
            case 0x34: // User Passkey Request: no hay teclado
                host.Enviar(0x042F, direccion);
                break;
            case 0x36 when p.Length >= 7: // Simple Pairing Complete (estado, dirección)
                registro($"seguridad: emparejamiento con {DireccionBluetooth.DesdeHci(p.AsSpan(1))} terminó con estado 0x{p[0]:X2}");
                break;
            case 0x16: // PIN Code Request
                var bytes = Encoding.ASCII.GetBytes(pin);
                var relleno = new byte[16];
                bytes.AsSpan(0, Math.Min(16, bytes.Length)).CopyTo(relleno);
                host.Enviar(0x040D, [.. direccion, (byte)Math.Min(16, bytes.Length), .. relleno]);
                break;
        }
    }
}
