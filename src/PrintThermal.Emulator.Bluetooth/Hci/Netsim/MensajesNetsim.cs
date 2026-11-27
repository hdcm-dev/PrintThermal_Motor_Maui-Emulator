// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
namespace PrintThermal.Emulator.Bluetooth.Hci.Netsim;

/// <summary>Respuesta de PacketStreamer: un error o un paquete HCI.</summary>
public sealed record RespuestaNetsim(string? Error, PaqueteHci? Hci);

/// <summary>Mensajes de netsim.packet.PacketStreamer (rojo: sin implementar).</summary>
public static class MensajesNetsim
{
    public static byte[] PedidoInicial(DireccionBluetooth direccion, string nombreDispositivo) => throw new NotImplementedException();

    public static byte[] PedidoHci(PaqueteHci paquete) => throw new NotImplementedException();

    public static RespuestaNetsim LeerRespuesta(ReadOnlySpan<byte> mensaje) => throw new NotImplementedException();
}
