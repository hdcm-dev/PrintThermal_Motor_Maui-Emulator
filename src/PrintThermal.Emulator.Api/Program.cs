// Host del emulador: API de tickets, OpenAPI, Scalar y visor Blazor (PAR-PTE-04; ADR-0001, ADR-0002, ADR-0003).
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Api.Visor;
using PrintThermal.Emulator.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Sólo la interfaz local salvo configuración explícita (ADR-0001, D7).
if (Direcciones.EscuchaSinConfigurar(builder.Configuration) is { } escucha)
{
    builder.WebHost.UseUrls(escucha);
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IAlmacenDeTickets, AlmacenEnMemoria>();

// Fuente simulada (D4): sin Bluetooth, un ticket cada tantos segundos si Emulador:Simulacion:IntervaloSegundos > 0.
var simulacion = builder.Configuration.GetSection("Emulador:Simulacion").Get<OpcionesDeSimulacion>() ?? new();
if (simulacion.IntervaloSegundos > 0)
{
    builder.Services.AddSingleton(sp => new FuenteSimulada(
        sp.GetRequiredService<IAlmacenDeTickets>(), sp.GetRequiredService<TimeProvider>(), simulacion.IntervaloSegundos));
    builder.Services.AddSingleton<IEstadoDelMedio>(sp => sp.GetRequiredService<FuenteSimulada>());
    builder.Services.AddHostedService(sp => sp.GetRequiredService<FuenteSimulada>());
}
else
{
    builder.Services.AddSingleton<IEstadoDelMedio, SinMedio>();
}

builder.Services.AddRouting(o => o.ConstraintMap[EndpointsDeTickets.IdDeTicket.Nombre] = typeof(EndpointsDeTickets.IdDeTicket));
builder.Services.AddOpenApi();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// El visor llama a la API por HTTP con el cliente tipado (ADR-0002 A, ADR-0003 A).
builder.Services.AddHttpClient<ClienteDeTickets>((sp, http) =>
{
    var direcciones = sp.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
    http.BaseAddress = Direcciones.BaseDelVisor(builder.Configuration["Visor:UrlApi"], direcciones);
    http.Timeout = TimeSpan.FromSeconds(EndpointsDeTickets.EsperaMaxima + 15);
});

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapApiDeTickets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/// <summary>Punto de entrada; público para <c>WebApplicationFactory</c> en las pruebas.</summary>
public partial class Program;
