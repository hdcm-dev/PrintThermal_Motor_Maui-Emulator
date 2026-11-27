// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>Tipo de paquete HCI, con los valores del indicador H4. Netsim lo lleva aparte (<c>HCIPacket.packet_type</c>).</summary>
public enum TipoPaqueteHci : byte
{
    Comando = 1,
    Acl = 2,
    Sco = 3,
    Evento = 4,
    Iso = 5,
}

/// <summary>Un paquete HCI sin el byte de tipo H4.</summary>
public readonly record struct PaqueteHci(TipoPaqueteHci Tipo, byte[] Datos);

/// <summary>Canal de paquetes HCI con el controlador (netsim por gRPC o un controlador falso en las pruebas).</summary>
public interface ITransporteHci : IAsyncDisposable
{
    ValueTask EnviarAsync(PaqueteHci paquete, CancellationToken ct = default);

    IAsyncEnumerable<PaqueteHci> RecibirAsync(CancellationToken ct = default);
}
