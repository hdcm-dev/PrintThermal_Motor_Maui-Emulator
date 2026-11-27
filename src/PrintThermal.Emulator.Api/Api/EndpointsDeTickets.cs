using System.Reflection;
using PrintThermal.Emulator.Client;

namespace PrintThermal.Emulator.Api.Api;

/// <summary>
/// Rutas de la API de tickets (contrato D3 de PAR-PTE-04, sobre ADR-0001, con los cambios de la mesa). Los cambios
/// futuros del contrato son sólo aditivos; los errores salen como ProblemDetails (RFC 9457).
/// </summary>
public static class EndpointsDeTickets
{
    /// <summary>Plazo de espera por omisión, en segundos.</summary>
    public const int EsperaPorOmision = 30;

    /// <summary>Plazo de espera máximo, en segundos.</summary>
    public const int EsperaMaxima = 60;

    /// <summary>Tope de <c>limite</c> en la lista.</summary>
    public const int LimiteMaximo = 1000;

    private const string TipoPng = "image/png";
    private const string TipoBin = "application/octet-stream";

    // Un id no lleva puntos ni barras (IdDeTicket); los literales «ultimo» y «esperar» ganan por precedencia de ruta.
    private const string Id = "{id:" + IdDeTicket.Nombre + "}";

    /// <summary>Mapea <c>/api/estado</c> y <c>/api/tickets/...</c>.</summary>
    public static IEndpointRouteBuilder MapApiDeTickets(this IEndpointRouteBuilder rutas)
    {
        var version = typeof(EndpointsDeTickets).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

        rutas.MapGet("/api/estado", (IAlmacenDeTickets almacen, IEstadoDelMedio medio, TimeProvider reloj) =>
            {
                var todos = almacen.Listar(); // una sola lectura: cantidad y último coherentes
                return new EstadoDelEmulador(
                    version, medio.Nombre, medio.Conectado, todos.Count, todos.FirstOrDefault()?.Id, reloj.GetUtcNow());
            })
            .WithName("ObtenerEstado")
            .WithTags("Estado")
            .WithSummary("Estado del emulador y de su medio.")
            .WithDescription("«ultimoId» sirve para esperar el próximo ticket: leer el estado, imprimir y esperar con despuesDe=ultimoId.");

        var tickets = rutas.MapGroup("/api/tickets").WithTags("Tickets");

        tickets.MapGet("", Listar)
            .WithName("ListarTickets")
            .WithSummary("Tickets guardados, el más nuevo primero; «limite» opcional (1 a 1000).")
            .Produces<List<TicketInfo>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        tickets.MapGet("/ultimo", (IAlmacenDeTickets almacen) =>
                almacen.Ultimo() is { } t ? Results.Ok(Info(t)) : SinTickets())
            .WithName("ObtenerUltimoTicket")
            .WithSummary("El ticket más nuevo.")
            .Produces<TicketInfo>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapGet("/ultimo.png", (IAlmacenDeTickets almacen) =>
                almacen.Ultimo() is { } t ? Results.File(t.Png, TipoPng) : SinTickets())
            .WithName("ObtenerPngDelUltimoTicket")
            .WithSummary("PNG del ticket más nuevo.")
            .Produces(StatusCodes.Status200OK, contentType: TipoPng)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapGet("/esperar", EsperarAsync)
            .WithName("EsperarTicket")
            .WithSummary("Espera el primer ticket con id mayor que «despuesDe», hasta «segundos» (1 a 60; 30 si falta).")
            .WithDescription(
                "Sin «despuesDe», espera un ticket posterior a los que había al recibir el pedido. Al vencer el plazo " +
                "responde 204. Patrón: leer /api/estado, imprimir y esperar con despuesDe=ultimoId (o despuesDe=0 si no " +
                "había tickets: todo id es mayor que 0). El cliente debe " +
                "tener un tiempo de espera mayor que «segundos».")
            .Produces<TicketInfo>()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        tickets.MapGet("/" + Id, (string id, IAlmacenDeTickets almacen) =>
                almacen.Obtener(id) is { } t ? Results.Ok(Info(t)) : NoExiste(id))
            .WithName("ObtenerTicket")
            .WithSummary("Un ticket por id.")
            .Produces<TicketInfo>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapGet("/" + Id + ".png", (string id, IAlmacenDeTickets almacen) =>
                almacen.Obtener(id) is { } t ? Results.File(t.Png, TipoPng) : NoExiste(id))
            .WithName("ObtenerPngDeTicket")
            .WithSummary("PNG de un ticket, tal como se generó.")
            .Produces(StatusCodes.Status200OK, contentType: TipoPng)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapGet("/" + Id + ".bin", (string id, IAlmacenDeTickets almacen) =>
                almacen.Obtener(id) is { } t ? Results.File(t.Bin, TipoBin, t.Id + ".bin") : NoExiste(id))
            .WithName("ObtenerBinDeTicket")
            .WithSummary("Bytes recibidos de un ticket.")
            .Produces(StatusCodes.Status200OK, contentType: TipoBin)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapDelete("", (IAlmacenDeTickets almacen) =>
            {
                almacen.BorrarTodos();
                return Results.NoContent();
            })
            .WithName("BorrarTickets")
            .WithSummary("Borra todos los tickets.")
            .WithDescription("Sin autenticación: también vacía los tickets que esperan otros consumidores del mismo emulador.")
            .Produces(StatusCodes.Status204NoContent);

        tickets.MapDelete("/" + Id, (string id, IAlmacenDeTickets almacen) =>
                almacen.Borrar(id) ? Results.NoContent() : NoExiste(id))
            .WithName("BorrarTicket")
            .WithSummary("Borra un ticket.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return rutas;
    }

    /// <summary>Datos públicos de un ticket guardado.</summary>
    public static TicketInfo Info(TicketGuardado t)
    {
        var (ancho, alto) = Png.Tamano(t.Png);
        return new TicketInfo(t.Id, t.Recibido, t.Bin.LongLength, t.Png.LongLength, ancho, alto);
    }

    private static IResult Listar(IAlmacenDeTickets almacen, int? limite)
    {
        if (limite is < 1 or > LimiteMaximo)
        {
            return Results.Problem($"«limite» debe estar entre 1 y {LimiteMaximo}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var todos = almacen.Listar();
        return Results.Ok(todos.Take(limite ?? todos.Count).Select(Info).ToList());
    }

    private static async Task<IResult> EsperarAsync(
        IAlmacenDeTickets almacen, string? despuesDe, int? segundos, CancellationToken ct)
    {
        var plazo = segundos ?? EsperaPorOmision;
        if (plazo < 1 || plazo > EsperaMaxima)
        {
            return Results.Problem(
                $"«segundos» debe estar entre 1 y {EsperaMaxima}.", statusCode: StatusCodes.Status400BadRequest);
        }

        // ct es el del pedido: si el cliente se desconecta, la espera se corta.
        using var vencimiento = CancellationTokenSource.CreateLinkedTokenSource(ct);
        vencimiento.CancelAfter(TimeSpan.FromSeconds(plazo));
        try
        {
            var ticket = await almacen.EsperarAsync(despuesDe, vencimiento.Token).ConfigureAwait(false);
            return Results.Ok(Info(ticket));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Results.NoContent();
        }
    }

    /// <summary>Restricción de ruta de un id de ticket: letras, dígitos, guion y guion bajo.</summary>
    public sealed class IdDeTicket : IRouteConstraint
    {
        /// <summary>Nombre de la restricción en las plantillas de ruta.</summary>
        public const string Nombre = "idticket";

        /// <inheritdoc />
        public bool Match(
            HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) =>
            values.TryGetValue(routeKey, out var valor) && valor is string id && EsValido(id);

        /// <summary>Si el texto es un id posible.</summary>
        public static bool EsValido(string id) =>
            id.Length > 0 && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
    }

    private static IResult SinTickets() =>
        Results.Problem("Todavía no hay tickets.", statusCode: StatusCodes.Status404NotFound);

    private static IResult NoExiste(string id) =>
        Results.Problem($"No existe el ticket «{id}».", statusCode: StatusCodes.Status404NotFound);
}
