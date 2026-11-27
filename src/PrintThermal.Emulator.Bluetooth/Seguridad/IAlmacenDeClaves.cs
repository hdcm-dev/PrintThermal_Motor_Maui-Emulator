// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.Hci;

namespace PrintThermal.Emulator.Bluetooth.Seguridad;

/// <summary>Clave de enlace que deja el emparejamiento (HCI Link Key Notification).</summary>
public sealed record ClaveDeEnlace(byte[] Clave, byte Tipo);

/// <summary>Guarda las claves de enlace por dirección, para reconectar sin volver a emparejar.</summary>
public interface IAlmacenDeClaves
{
    ClaveDeEnlace? Buscar(DireccionBluetooth par);

    void Guardar(DireccionBluetooth par, ClaveDeEnlace clave);
}
