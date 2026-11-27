using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Client;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>
/// El host del emulador en memoria (almacén en memoria, sin medio). El cliente que usa el visor sale por el servidor
/// de pruebas y deja registro de cada pedido, para comprobar que el visor llega a los datos por la API.
/// </summary>
public sealed class FabricaDelEmulador : WebApplicationFactory<Program>
{
    /// <summary>Rutas (con consulta) que pidió el cliente del visor.</summary>
    public ConcurrentQueue<string> PedidosDelVisor { get; } = new();

    /// <summary>El almacén del host.</summary>
    public IAlmacenDeTickets Almacen => Services.GetRequiredService<IAlmacenDeTickets>();

    /// <summary>Guarda un ticket con un PNG distinto por <paramref name="semilla"/>.</summary>
    public TicketGuardado Imprimir(int semilla) =>
        Almacen.Guardar([0x1B, 0x40, (byte)semilla, 0x0A], PngDePrueba(semilla));

    /// <summary>Un PNG de 384 puntos de ancho, distinto por semilla.</summary>
    public static byte[] PngDePrueba(int semilla) =>
        Png.DeUnBit(384, 40 + semilla, (x, y) => (x + (y * semilla)) % (semilla + 2) == 0);

    /// <summary>Huella SHA-256 en hexadecimal.</summary>
    public static string Huella(byte[] datos) => Convert.ToHexString(SHA256.HashData(datos));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(servicios =>
            servicios.AddHttpClient<ClienteDeTickets>()
                .ConfigurePrimaryHttpMessageHandler(sp => ((TestServer)sp.GetRequiredService<IServer>()).CreateHandler())
                .AddHttpMessageHandler(() => new Registro(PedidosDelVisor)));
    }

    private sealed class Registro(ConcurrentQueue<string> pedidos) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct)
        {
            pedidos.Enqueue(pedido.RequestUri?.PathAndQuery ?? "");
            return base.SendAsync(pedido, ct);
        }
    }
}
