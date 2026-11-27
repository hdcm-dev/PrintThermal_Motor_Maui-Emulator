// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.Hci;

namespace PrintThermal.Emulator.Bluetooth.Seguridad;

/// <summary>Claves en un archivo JSON (rojo: sin implementar).</summary>
public sealed class AlmacenDeClavesEnArchivo(string ruta) : IAlmacenDeClaves
{
    public string Ruta { get; } = ruta;

    public ClaveDeEnlace? Buscar(DireccionBluetooth par) => throw new NotImplementedException();

    public void Guardar(DireccionBluetooth par, ClaveDeEnlace clave) => throw new NotImplementedException();
}

/// <summary>Claves en memoria (rojo: sin implementar).</summary>
public sealed class AlmacenDeClavesEnMemoria : IAlmacenDeClaves
{
    public ClaveDeEnlace? Buscar(DireccionBluetooth par) => throw new NotImplementedException();

    public void Guardar(DireccionBluetooth par, ClaveDeEnlace clave) => throw new NotImplementedException();
}
