namespace PrintThermal.Emulator.Api.Api;

/// <summary>Direcciones del host: esqueleto (rojo de PAR-PTE-04).</summary>
public static class Direcciones
{
    /// <summary>Dirección por omisión.</summary>
    public const string EscuchaPorOmision = "http://127.0.0.1:5080";

    /// <summary>Sin implementar.</summary>
    public static string? EscuchaSinConfigurar(IConfiguration configuracion) => throw new NotImplementedException();

    /// <summary>Sin implementar: siempre la dirección por omisión.</summary>
    public static Uri BaseDelVisor(string? urlApi, IEnumerable<string>? direccionesDelServidor) => new(EscuchaPorOmision + "/");
}
