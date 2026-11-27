// Host del emulador: API de tickets, OpenAPI, Scalar y visor Blazor (PAR-PTE-04; ADR-0001, ADR-0002, ADR-0003),
// con el almacén en disco y el medio Bluetooth conectados por configuración (PAR-PTE-05).
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Api.Integracion;
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
// Almacén: en disco si hay carpeta de salida (PAR-PTE-02, conectado en PAR-PTE-05 D2); si no, en memoria (PAR-PTE-04 D4).
var salida = builder.Configuration["Emulador:Salida"];
if (!string.IsNullOrWhiteSpace(salida))
{
    string carpeta = salida;
    builder.Services.AddSingleton(sp => new AlmacenEnDisco(carpeta, sp.GetRequiredService<TimeProvider>()));
    builder.Services.AddSingleton<IAlmacenDeTickets>(sp => sp.GetRequiredService<AlmacenEnDisco>());
    builder.Services.AddSingleton(sp =>
    {
        var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("PrintThermal.Emulator.Recepcion");
        return new ReceptorAlAlmacen(sp.GetRequiredService<AlmacenEnDisco>(), registro: m => log.LogInformation("{Mensaje}", m));
    });
}
else
{
    builder.Services.AddSingleton<IAlmacenDeTickets, AlmacenEnMemoria>();
}

// Medio: Bluetooth sobre netsim si Emulador:Bluetooth:Netsim está (PAR-PTE-03, conectado en PAR-PTE-05 D2);
// si no, la fuente simulada (D4 de PAR-PTE-04): un ticket cada tantos segundos si Emulador:Simulacion:IntervaloSegundos > 0.
var bluetooth = builder.Configuration.GetSection("Emulador:Bluetooth").Get<OpcionesDelMedio>();
var simulacion = builder.Configuration.GetSection("Emulador:Simulacion").Get<OpcionesDeSimulacion>() ?? new();
if (!string.IsNullOrWhiteSpace(bluetooth?.Netsim))
{
    if (string.IsNullOrWhiteSpace(salida))
    {
        throw new InvalidOperationException("Emulador:Bluetooth:Netsim necesita Emulador:Salida (carpeta de los tickets).");
    }
    OpcionesDelMedio medio = bluetooth!;
    string carpeta = salida;
    builder.Services.AddSingleton(sp => new MedioBluetooth(
        medio, carpeta, sp.GetRequiredService<ReceptorAlAlmacen>(), sp.GetRequiredService<ILogger<MedioBluetooth>>()));
    builder.Services.AddSingleton<IEstadoDelMedio>(sp => sp.GetRequiredService<MedioBluetooth>());
    builder.Services.AddHostedService(sp => sp.GetRequiredService<MedioBluetooth>());
}
else if (simulacion.IntervaloSegundos > 0)
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
builder.Services.AddProblemDetails(); // también para errores de enlace y rutas inexistentes
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

app.UseStatusCodePages();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapApiDeTickets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/// <summary>Punto de entrada; público para <c>WebApplicationFactory</c> en las pruebas.</summary>
public partial class Program;
