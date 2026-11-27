// cdd: REQ-HIS-Documentacion-De-Uso
using PrintThermal.Emulator.Client;

namespace PrintThermal.Emulator.Tests.Documentacion;

/// <summary>
/// Ejemplos del cliente tipado que <c>docs/api.md</c> copia al pie de la letra (PAR-PTE-06). Cada región
/// <c>ejemplo:nombre</c> compila con la solución; <see cref="DocumentacionTests"/> compara el texto con el de la
/// documentación y corre el ejemplo contra el host.
/// </summary>
public static class EjemplosDelCliente
{
    // <ejemplo:imprimir-y-esperar>
    public static async Task<byte[]> ImprimirYObtenerPngAsync(Uri emulador, Func<Task> imprimir)
    {
        // El tiempo de espera del HttpClient tiene que superar el de la espera (60 s como máximo).
        using var http = new HttpClient { BaseAddress = emulador, Timeout = TimeSpan.FromSeconds(75) };
        var cliente = new ClienteDeTickets(http);

        // 1. Leer el último id antes de imprimir: así no se pierde un ticket que llegue rápido.
        var antes = await cliente.ObtenerEstadoAsync();

        // 2. Imprimir desde la app (fuera del emulador de impresora).
        await imprimir();

        // 3. Esperar el primer ticket posterior; null si vence el plazo.
        var ticket = await cliente.EsperarAsync(
                antes.UltimoId ?? ClienteDeTickets.DesdeElPrincipio, TimeSpan.FromSeconds(60))
            ?? throw new TimeoutException("No llegó ningún ticket en 60 s.");

        // 4. El PNG tal como lo generó el emulador.
        return await cliente.ObtenerPngAsync(ticket.Id)
            ?? throw new InvalidOperationException($"El ticket {ticket.Id} ya no está.");
    }
    // </ejemplo:imprimir-y-esperar>
}
