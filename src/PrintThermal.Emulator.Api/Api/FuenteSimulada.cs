using System.Globalization;
using System.Text;

namespace PrintThermal.Emulator.Api.Api;

/// <summary>Opciones de la fuente simulada (<c>Emulador:Simulacion</c>).</summary>
public sealed class OpcionesDeSimulacion
{
    /// <summary>Segundos entre tickets simulados; 0 apaga la simulación.</summary>
    public int IntervaloSegundos { get; set; }
}

/// <summary>
/// Fuente de tickets simulada (D4): sin Bluetooth, genera cada tanto un ticket sintético de 384 puntos de ancho
/// con su número y su hora, y su <c>.bin</c> ESC/POS. Sirve para probar la API y el visor sin el medio real.
/// </summary>
public sealed class FuenteSimulada(IAlmacenDeTickets almacen, TimeProvider reloj, int intervaloSegundos)
    : BackgroundService, IEstadoDelMedio
{
    /// <summary>Ancho del ticket en puntos (el de la librería cliente; NO VERIFICADO en la 58HB6).</summary>
    public const int AnchoPuntos = 384;

    private int _numero;

    /// <inheritdoc />
    public string Nombre => "simulado";

    /// <inheritdoc />
    public bool Conectado => true;

    /// <summary>Genera y guarda un ticket simulado.</summary>
    public TicketGuardado Imprimir()
    {
        var numero = Interlocked.Increment(ref _numero);
        var hora = reloj.GetLocalNow().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var linea1 = "#" + numero.ToString("D3", CultureInfo.InvariantCulture);
        return almacen.Guardar(Bin(linea1, hora), Dibujar(linea1, hora));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var temporizador = new PeriodicTimer(TimeSpan.FromSeconds(intervaloSegundos), reloj);
        while (await temporizador.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            Imprimir();
        }
    }

    private static byte[] Bin(string linea1, string hora)
    {
        var texto = new List<byte> { 0x1B, 0x40 }; // ESC @: inicializar
        texto.AddRange(Encoding.ASCII.GetBytes("TICKET SIMULADO " + linea1 + "\n" + hora + "\n\n\n"));
        texto.AddRange([0x1D, 0x56, 0x01]); // GS V 1: corte parcial
        return [.. texto];
    }

    private static byte[] Dibujar(string linea1, string hora)
    {
        const int escala = 6;
        const int margen = 16;
        var altoTexto = 7 * escala;
        var alto = margen + altoTexto + margen + altoTexto + margen + 24 + margen;
        var y1 = margen;
        var y2 = y1 + altoTexto + margen;
        var yBarras = y2 + altoTexto + margen;
        return Png.DeUnBit(AnchoPuntos, alto, (x, y) =>
            x < 2 || y < 2 || x >= AnchoPuntos - 2 || y >= alto - 2
            || Texto(linea1, margen, y1, escala, x, y)
            || Texto(hora, margen, y2, escala, x, y)
            || (y >= yBarras && y < yBarras + 24 && x >= margen && x < AnchoPuntos - margen && (x / 3 + (x / 7)) % 2 == 0));
    }

    private static bool Texto(string texto, int x0, int y0, int escala, int x, int y)
    {
        if (y < y0 || y >= y0 + (7 * escala) || x < x0)
        {
            return false;
        }

        var paso = 6 * escala;
        var i = (x - x0) / paso;
        if (i >= texto.Length)
        {
            return false;
        }

        var columna = (x - x0 - (i * paso)) / escala;
        var fila = (y - y0) / escala;
        return columna < 5 && Glifos.TryGetValue(texto[i], out var g) && g[fila][columna] == '#';
    }

    private static readonly Dictionary<char, string[]> Glifos = new()
    {
        ['0'] = [" ### ", "#   #", "#  ##", "# # #", "##  #", "#   #", " ### "],
        ['1'] = ["  #  ", " ##  ", "  #  ", "  #  ", "  #  ", "  #  ", " ### "],
        ['2'] = [" ### ", "#   #", "    #", "   # ", "  #  ", " #   ", "#####"],
        ['3'] = ["#####", "   # ", "  #  ", "   # ", "    #", "#   #", " ### "],
        ['4'] = ["   # ", "  ## ", " # # ", "#  # ", "#####", "   # ", "   # "],
        ['5'] = ["#####", "#    ", "#### ", "    #", "    #", "#   #", " ### "],
        ['6'] = ["  ## ", " #   ", "#    ", "#### ", "#   #", "#   #", " ### "],
        ['7'] = ["#####", "    #", "   # ", "  #  ", " #   ", " #   ", " #   "],
        ['8'] = [" ### ", "#   #", "#   #", " ### ", "#   #", "#   #", " ### "],
        ['9'] = [" ### ", "#   #", "#   #", " ####", "    #", "   # ", " ##  "],
        [':'] = ["     ", "  #  ", "  #  ", "     ", "  #  ", "  #  ", "     "],
        ['#'] = [" # # ", " # # ", "#####", " # # ", "#####", " # # ", " # # "],
    };
}
