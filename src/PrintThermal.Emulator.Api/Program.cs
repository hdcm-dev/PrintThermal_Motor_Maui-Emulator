// Host del emulador: esqueleto sin API, OpenAPI ni Scalar (rojo de PAR-PTE-04).
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Api.Visor;
using PrintThermal.Emulator.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IAlmacenDeTickets, AlmacenEnMemoria>();
builder.Services.AddSingleton<IEstadoDelMedio, SinMedio>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient<ClienteDeTickets>(http => http.BaseAddress = Direcciones.BaseDelVisor(null, null));

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapApiDeTickets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

/// <summary>Punto de entrada; público para <c>WebApplicationFactory</c> en las pruebas.</summary>
public partial class Program;
