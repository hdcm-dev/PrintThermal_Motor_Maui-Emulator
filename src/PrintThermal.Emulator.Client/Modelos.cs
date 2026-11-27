namespace PrintThermal.Emulator.Client;

/// <summary>Datos de un ticket guardado, sin sus bytes (PAR-PTE-04, contrato D3).</summary>
/// <param name="Id">Identificador opaco del ticket.</param>
/// <param name="Recibido">Fecha y hora en que terminó de llegar.</param>
/// <param name="BytesBin">Cantidad de bytes recibidos (<c>.bin</c>).</param>
/// <param name="BytesPng">Tamaño del PNG generado, en bytes.</param>
/// <param name="AnchoPx">Ancho del PNG en puntos.</param>
/// <param name="AltoPx">Alto del PNG en puntos.</param>
public sealed record TicketInfo(
    string Id,
    DateTimeOffset Recibido,
    long BytesBin,
    long BytesPng,
    int AnchoPx,
    int AltoPx);

/// <summary>Estado del emulador que informa <c>GET /api/estado</c>.</summary>
/// <param name="Version">Versión del emulador.</param>
/// <param name="Medio">Nombre del medio por el que llegan los tickets.</param>
/// <param name="MedioConectado">Si el medio está conectado.</param>
/// <param name="Tickets">Cantidad de tickets guardados.</param>
/// <param name="UltimoId">Id del ticket más nuevo, si hay.</param>
/// <param name="Hora">Hora del servidor.</param>
public sealed record EstadoDelEmulador(
    string Version,
    string Medio,
    bool MedioConectado,
    int Tickets,
    string? UltimoId,
    DateTimeOffset Hora);
