// cdd: REQ-HIS-Recepcion-Integra
using PrintThermal.Emulator.Bluetooth.Hci;

namespace PrintThermal.Emulator.Bluetooth.Rfcomm;

/// <summary>Una conexión RFCOMM abierta por el otro lado sobre el canal del perfil de puerto serie.</summary>
public sealed record InfoConexionRfcomm(long Id, DireccionBluetooth Par, byte Canal);

/// <summary>
/// Destino de los bytes que llegan por RFCOMM, sin cambios y en orden. Es la frontera con el almacén de tickets
/// del Core: el host del servicio la implementa con el almacén.
/// </summary>
public interface IReceptorDeBytes
{
    void Abierta(InfoConexionRfcomm conexion);

    void Recibidos(InfoConexionRfcomm conexion, ReadOnlySpan<byte> datos);

    void Cerrada(InfoConexionRfcomm conexion);
}
