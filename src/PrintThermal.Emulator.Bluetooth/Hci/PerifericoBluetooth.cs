// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
#pragma warning disable CS9113 // rojo: parámetros sin leer hasta la implementación
using PrintThermal.Emulator.Bluetooth.Rfcomm;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>Host Bluetooth Classic de la impresora emulada (rojo: sin implementar).</summary>
public sealed class PerifericoBluetooth(ITransporteHci transporte, OpcionesPeriferico opciones, IReceptorDeBytes receptor,
    IAlmacenDeClaves claves, Action<string>? registro = null)
{
    public EstadoPeriferico Estado => EstadoPeriferico.Detenido;

    public DireccionBluetooth? DireccionLocal => null;

    public Task Anunciando => Task.FromException(new NotImplementedException());

    public Task EjecutarAsync(CancellationToken ct = default) => throw new NotImplementedException();
}

/// <summary>Conecta, ejecuta y reintenta el periférico (rojo: sin implementar).</summary>
public sealed class ServicioPeriferico(Func<CancellationToken, Task<ITransporteHci>> conectar, OpcionesPeriferico opciones,
    IReceptorDeBytes receptor, IAlmacenDeClaves claves, TimeSpan esperaEntreIntentos, Action<string>? registro = null)
{
    public EstadoPeriferico Estado => EstadoPeriferico.Detenido;

    public string? UltimoError => null;

    public int Intentos => 0;

    public Task EjecutarAsync(CancellationToken ct = default) => throw new NotImplementedException();
}
