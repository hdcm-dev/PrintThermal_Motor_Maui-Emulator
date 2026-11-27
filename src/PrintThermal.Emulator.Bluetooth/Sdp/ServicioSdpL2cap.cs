// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.L2cap;

namespace PrintThermal.Emulator.Bluetooth.Sdp;

/// <summary>El servidor SDP sobre su PSM (0x0001): cada SDU es un pedido y se contesta en el mismo canal.</summary>
internal sealed class ServicioSdpL2cap(ServidorSdp servidor) : IServicioL2cap
{
    public const ushort Psm = 0x0001;

    public void Abierto(CanalL2cap canal)
    {
    }

    public void Recibir(CanalL2cap canal, byte[] sdu) => canal.Enviar(servidor.Responder(sdu, canal.MtuRemoto));

    public void Cerrado(CanalL2cap canal)
    {
    }
}
