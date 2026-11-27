// cdd: REQ-HIS-Api-De-Tickets
using PrintThermal.Emulator.Bluetooth.Rfcomm;

namespace PrintThermal.Emulator.Api.Integracion;

/// <summary>Esqueleto para las pruebas en rojo de PAR-PTE-05.</summary>
public sealed class ReceptorAlAlmacen : IReceptorDeBytes
{
    public void Abierta(InfoConexionRfcomm conexion) => throw new NotImplementedException();
    public void Recibidos(InfoConexionRfcomm conexion, ReadOnlySpan<byte> datos) => throw new NotImplementedException();
    public void Cerrada(InfoConexionRfcomm conexion) => throw new NotImplementedException();
}
