namespace PrintThermal.Emulator.Api.Api;

/// <summary>Direcciones del host: dónde escucha y a dónde llama el visor (PAR-PTE-04, D7).</summary>
public static class Direcciones
{
    /// <summary>Dirección por omisión: sólo la interfaz local.</summary>
    public const string EscuchaPorOmision = "http://127.0.0.1:5080";

    /// <summary>
    /// La dirección de escucha por omisión si la configuración no fija ninguna (<c>urls</c>, <c>http_ports</c>,
    /// <c>https_ports</c> o <c>Kestrel:Endpoints</c>); <c>null</c> si la fija, para respetarla.
    /// </summary>
    public static string? EscuchaSinConfigurar(IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        var configurada =
            !string.IsNullOrWhiteSpace(configuracion["urls"])
            || !string.IsNullOrWhiteSpace(configuracion["http_ports"])
            || !string.IsNullOrWhiteSpace(configuracion["https_ports"])
            || configuracion.GetSection("Kestrel:Endpoints").GetChildren().Any();
        return configurada ? null : EscuchaPorOmision;
    }

    /// <summary>
    /// Dirección base con la que el visor llama a la API: <c>Visor:UrlApi</c> si está; si no, la primera dirección
    /// del servidor, con un comodín (<c>0.0.0.0</c>, <c>[::]</c>, <c>+</c>, <c>*</c>) cambiado por <c>localhost</c>.
    /// </summary>
    public static Uri BaseDelVisor(string? urlApi, IEnumerable<string>? direccionesDelServidor)
    {
        var texto = !string.IsNullOrWhiteSpace(urlApi)
            ? urlApi
            : direccionesDelServidor?.FirstOrDefault() ?? EscuchaPorOmision;
        texto = texto.Replace("://0.0.0.0", "://localhost", StringComparison.Ordinal)
            .Replace("://[::]", "://localhost", StringComparison.Ordinal)
            .Replace("://+", "://localhost", StringComparison.Ordinal)
            .Replace("://*", "://localhost", StringComparison.Ordinal);
        return new Uri(texto.EndsWith('/') ? texto : texto + "/");
    }
}
