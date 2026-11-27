using System.Globalization;
using System.Net;
using System.Net.Http.Json;

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
    public static string RutaPng(string id) => $"api/tickets/{Uri.EscapeDataString(id)}.png";

    /// <summary>Ruta relativa de los bytes recibidos de un ticket.</summary>
    public static string RutaBin(string id) => $"api/tickets/{Uri.EscapeDataString(id)}.bin";

    /// <summary>Estado del emulador.</summary>
    public async Task<EstadoDelEmulador> ObtenerEstadoAsync(CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<EstadoDelEmulador>("api/estado", ct).ConfigureAwait(false)
        ?? throw new InvalidOperationException("La API devolvió un estado vacío.");

    /// <summary>Tickets guardados, el más nuevo primero; a lo sumo <paramref name="limite"/> si se indica.</summary>
    public async Task<IReadOnlyList<TicketInfo>> ListarAsync(int? limite = null, CancellationToken ct = default)
    {
        var ruta = limite is { } n ? "api/tickets?limite=" + n.ToString(CultureInfo.InvariantCulture) : "api/tickets";
        return await _http.GetFromJsonAsync<List<TicketInfo>>(ruta, ct).ConfigureAwait(false) ?? [];
    }

    /// <summary>El ticket más nuevo, o <c>null</c> si no hay.</summary>
    public Task<TicketInfo?> ObtenerUltimoAsync(CancellationToken ct = default) =>
        LeerOpcionalAsync<TicketInfo>("api/tickets/ultimo", ct);

    /// <summary>Un ticket por id, o <c>null</c> si no existe.</summary>
    public Task<TicketInfo?> ObtenerAsync(string id, CancellationToken ct = default) =>
        LeerOpcionalAsync<TicketInfo>($"api/tickets/{Uri.EscapeDataString(id)}", ct);

    /// <summary>PNG del ticket más nuevo, o <c>null</c> si no hay.</summary>
    public Task<byte[]?> ObtenerPngDelUltimoAsync(CancellationToken ct = default) =>
        LeerBytesAsync("api/tickets/ultimo.png", ct);

    /// <summary>PNG de un ticket, o <c>null</c> si no existe.</summary>
    public Task<byte[]?> ObtenerPngAsync(string id, CancellationToken ct = default) =>
        LeerBytesAsync(RutaPng(id), ct);

    /// <summary>Bytes recibidos de un ticket, o <c>null</c> si no existe.</summary>
    public Task<byte[]?> ObtenerBinAsync(string id, CancellationToken ct = default) =>
        LeerBytesAsync(RutaBin(id), ct);

    /// <summary>
    /// Espera el primer ticket con id mayor que <paramref name="despuesDe"/> hasta <paramref name="tiempoMaximo"/>
    /// (entre 1 y 60 segundos); sin <paramref name="despuesDe"/>, uno posterior a los presentes. Devuelve <c>null</c>
    /// si el plazo vence sin ticket. Para no perder un ticket: leer <see cref="EstadoDelEmulador.UltimoId"/>, imprimir
    /// y esperar con ese id, o con <see cref="DesdeElPrincipio"/> si no había. El <see cref="HttpClient.Timeout"/> debe
    /// superar el plazo.
    /// </summary>
    public async Task<TicketInfo?> EsperarAsync(string? despuesDe, TimeSpan tiempoMaximo, CancellationToken ct = default)
    {
        var segundos = (int)Math.Ceiling(tiempoMaximo.TotalSeconds);
        if (segundos < 1 || segundos > EsperaMaxima.TotalSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(tiempoMaximo), tiempoMaximo, "Entre 1 y 60 segundos.");
        }

        var ruta = "api/tickets/esperar?segundos=" + segundos.ToString(CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(despuesDe))
        {
            ruta += "&despuesDe=" + Uri.EscapeDataString(despuesDe);
        }

        using var respuesta = await _http.GetAsync(ruta, ct).ConfigureAwait(false);
        if (respuesta.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<TicketInfo>(ct).ConfigureAwait(false);
    }

    /// <summary>Borra todos los tickets.</summary>
    public async Task BorrarTodosAsync(CancellationToken ct = default)
    {
        using var respuesta = await _http.DeleteAsync("api/tickets", ct).ConfigureAwait(false);
        respuesta.EnsureSuccessStatusCode();
    }

    /// <summary>Borra un ticket; <c>false</c> si no existía.</summary>
    public async Task<bool> BorrarAsync(string id, CancellationToken ct = default)
    {
        using var respuesta = await _http.DeleteAsync($"api/tickets/{Uri.EscapeDataString(id)}", ct).ConfigureAwait(false);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        respuesta.EnsureSuccessStatusCode();
        return true;
    }

    private async Task<T?> LeerOpcionalAsync<T>(string ruta, CancellationToken ct) where T : class
    {
        using var respuesta = await _http.GetAsync(ruta, ct).ConfigureAwait(false);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<T>(ct).ConfigureAwait(false);
    }

    private async Task<byte[]?> LeerBytesAsync(string ruta, CancellationToken ct)
    {
        using var respuesta = await _http.GetAsync(ruta, ct).ConfigureAwait(false);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
    }
}
