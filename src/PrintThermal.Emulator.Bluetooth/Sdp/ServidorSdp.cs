// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
namespace PrintThermal.Emulator.Bluetooth.Sdp;

/// <summary>Servidor SDP (rojo: sin implementar).</summary>
public sealed class ServidorSdp
{
    public void Agregar(uint handle, IReadOnlyDictionary<ushort, ElementoSdp> atributos) => throw new NotImplementedException();

    public byte[] Responder(ReadOnlySpan<byte> pedido, int mtu) => throw new NotImplementedException();
}

/// <summary>Registro SDP del perfil de puerto serie (rojo: sin implementar).</summary>
public static class RegistroSpp
{
    public static IReadOnlyDictionary<ushort, ElementoSdp> Crear(uint handle, byte canal, string nombre) => throw new NotImplementedException();
}
