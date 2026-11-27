using PrintThermal.Emulator.Client;

namespace PrintThermal.Emulator.Api.Api;

/// <summary>Rutas de la API de tickets: esqueleto sin rutas (rojo de PAR-PTE-04).</summary>
public static class EndpointsDeTickets
{
    /// <summary>Plazo de espera máximo, en segundos.</summary>
    public const int EsperaMaxima = 60;

    /// <summary>Sin rutas todavía.</summary>
    public static IEndpointRouteBuilder MapApiDeTickets(this IEndpointRouteBuilder rutas) => rutas;

    /// <summary>Datos públicos de un ticket guardado.</summary>
    public static TicketInfo Info(TicketGuardado t) => throw new NotImplementedException();
}
