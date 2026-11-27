namespace PrintThermal.Emulator.Api.Api;

/// <summary>Un ticket guardado: los bytes recibidos y el PNG generado (PAR-PTE-04, D4).</summary>
/// <param name="Id">
/// Identificador sin puntos ni barras, ordenable: en orden ordinal, un ticket posterior tiene un id mayor, también
/// después de reiniciar el almacén, y todo id es mayor que <c>"0"</c> (contrato D3, cambio 2 de la mesa).
/// </param>
/// <param name="Recibido">Fecha y hora en que terminó de llegar.</param>
/// <param name="Bin">Bytes recibidos por el medio.</param>
/// <param name="Png">PNG generado a partir de esos bytes.</param>
public sealed record TicketGuardado(string Id, DateTimeOffset Recibido, byte[] Bin, byte[] Png);

/// <summary>
/// Almacén de tickets que consume la API. Esta parcela lo cumple en memoria (<see cref="AlmacenEnMemoria"/>);
/// el almacén en disco de REQ-HIS-Almacen-De-Tickets se conecta detrás de esta interfaz.
/// </summary>
public interface IAlmacenDeTickets
{
    /// <summary>Guarda un ticket nuevo y despierta a quienes esperan.</summary>
    TicketGuardado Guardar(byte[] bin, byte[] png);

    /// <summary>Todos los tickets, el más nuevo primero.</summary>
    IReadOnlyList<TicketGuardado> Listar();

    /// <summary>El ticket con ese id, o <c>null</c>.</summary>
    TicketGuardado? Obtener(string id);

    /// <summary>El ticket más nuevo, o <c>null</c> si no hay.</summary>
    TicketGuardado? Ultimo();

    /// <summary>Borra un ticket; <c>false</c> si no existía.</summary>
    bool Borrar(string id);

    /// <summary>Borra todos los tickets.</summary>
    void BorrarTodos();

    /// <summary>
    /// Devuelve el primer ticket con id mayor que <paramref name="despuesDe"/> (orden ordinal), en cuanto exista.
    /// Sin <paramref name="despuesDe"/>, espera un ticket posterior a los presentes al empezar. Termina con
    /// <see cref="OperationCanceledException"/> si se cancela.
    /// </summary>
    Task<TicketGuardado> EsperarAsync(string? despuesDe, CancellationToken ct);
}

/// <summary>Estado del medio por el que llegan los tickets (simulado ahora; Bluetooth en REQ-EPI-Medio-Emulado).</summary>
public interface IEstadoDelMedio
{
    /// <summary>Nombre del medio para mostrar.</summary>
    string Nombre { get; }

    /// <summary>Si el medio está conectado.</summary>
    bool Conectado { get; }
}

/// <summary>Medio por omisión: ninguno conectado.</summary>
public sealed class SinMedio : IEstadoDelMedio
{
    /// <inheritdoc />
    public string Nombre => "sin medio";

    /// <inheritdoc />
    public bool Conectado => false;
}
