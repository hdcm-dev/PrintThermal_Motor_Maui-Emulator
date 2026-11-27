// cdd: REQ-HIS-Almacen-De-Tickets
namespace PrintThermal.Emulator.Core.Almacen;

/// <summary>Un ticket guardado en la carpeta de salida: <c>{Id}.bin</c> y <c>{Id}.png</c>.</summary>
public sealed record TicketGuardado(string Id, DateTimeOffset Fecha, long BytesRecibidos, int Ancho, int Alto);
