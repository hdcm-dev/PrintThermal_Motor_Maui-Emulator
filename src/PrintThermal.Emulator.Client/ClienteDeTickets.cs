namespace PrintThermal.Emulator.Client;

/// <summary>
/// Cliente HTTP tipado de la API de tickets del emulador (ADR-0003, opción A). Lo usan el visor y las pruebas;
/// sirve a cualquier consumidor .NET con un <see cref="HttpClient"/> cuya dirección base es la del emulador.
/// </summary>
public sealed class ClienteDeTickets
{
    /// <summary>Plazo máximo que acepta la API para esperar un ticket.</summary>
    public static readonly TimeSpan EsperaMaxima = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Valor de <c>despuesDe</c> menor que cualquier id: espera desde el principio. Sirve cuando el estado no tiene
    /// <see cref="EstadoDelEmulador.UltimoId"/> (no había tickets antes de imprimir).
    /// </summary>
    public const string DesdeElPrincipio = "0";

    private readonly HttpClient _http;

    /// <summary>Crea el cliente sobre un <see cref="HttpClient"/> con <c>BaseAddress</c> del emulador.</summary>
    public ClienteDeTickets(HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        _http = http;
    }

    /// <summary>Ruta relativa del PNG de un ticket, para enlaces e imágenes.</summary>
    public static string RutaPng(string id) => throw new NotImplementedException();

    /// <summary>Ruta relativa de los bytes recibidos de un ticket.</summary>
    public static string RutaBin(string id) => throw new NotImplementedException();

    /// <summary>Estado del emulador.</summary>
    public Task<EstadoDelEmulador> ObtenerEstadoAsync(CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>Tickets guardados.</summary>
    public Task<IReadOnlyList<TicketInfo>> ListarAsync(int? limite = null, CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>El ticket más nuevo.</summary>
    public Task<TicketInfo?> ObtenerUltimoAsync(CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>Un ticket por id.</summary>
    public Task<TicketInfo?> ObtenerAsync(string id, CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>PNG del más nuevo.</summary>
    public Task<byte[]?> ObtenerPngDelUltimoAsync(CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>PNG de un ticket.</summary>
    public Task<byte[]?> ObtenerPngAsync(string id, CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>Bytes recibidos de un ticket.</summary>
    public Task<byte[]?> ObtenerBinAsync(string id, CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>Espera un ticket.</summary>
    public Task<TicketInfo?> EsperarAsync(string? despuesDe, TimeSpan tiempoMaximo, CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>Borra todos.</summary>
    public Task BorrarTodosAsync(CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>Borra uno.</summary>
    public Task<bool> BorrarAsync(string id, CancellationToken ct = default) => throw new NotImplementedException();
}
