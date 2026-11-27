// cdd: REQ-HIS-Ticket-A-Png
using PrintThermal.Emulator.Core.EscPos;

namespace PrintThermal.Emulator.Tests.EscPos;

/// <summary>Intérprete ESC/POS de 58 mm: bytes reales de la librería cliente contra sus PNG dorados (PAR-PTE-02).</summary>
public class InterpreteTests
{
    private static readonly string[] Nombres =
    [
        "01-texto", "02-acentos", "03-imagen", "04-qr", "05-ean13", "06-escpos-feed", "07-escpos-corte-total",
    ];

    public static TheoryData<string> Vectores() => new(Nombres);

    /// <summary>Alimenta como lo hace la librería: trozos fijos que no respetan los límites de los comandos.</summary>
    private static List<TicketInterpretado> Interpretar(byte[] bytes, int trozo = 256, PerfilDeImpresora? perfil = null)
    {
        var tickets = new List<TicketInterpretado>();
        var interprete = new Interprete(perfil);
        interprete.TicketTerminado += tickets.Add;
        for (int i = 0; i < bytes.Length; i += trozo)
        {
            interprete.Alimentar(bytes.AsSpan(i, Math.Min(trozo, bytes.Length - i)));
        }
        interprete.Finalizar();
        return tickets;
    }

    private static byte[] B(params object[] partes) => partes.SelectMany(p => p switch
    {
        string s => System.Text.Encoding.ASCII.GetBytes(s),
        byte[] b => b,
        int n => [(byte)n],
        _ => throw new ArgumentException(p.GetType().Name),
    }).ToArray();

    [Theory]
    [MemberData(nameof(Vectores))]
    public void ElTicketDeLaLibreriaCoincideConSuVectorDorado(string vector)
    {
        var bytes = Dorados.Leer(vector + ".bin");

        var tickets = Interpretar(bytes);

        var ticket = Assert.Single(tickets);
        Assert.Equal(bytes, ticket.Bytes);
        Assert.Equal(384, ticket.Ancho);
        Assert.Empty(ticket.Avisos);
        var obtenido = Imagen.DePng(ticket.Png);
        var ruta = Dorados.GuardarObtenido(vector, ticket.Png);
        var dorado = Path.Combine(Dorados.Carpeta, vector + ".png");
        Assert.True(File.Exists(dorado), $"falta el PNG dorado {dorado}; el obtenido quedó en {ruta}");
        var esperado = Imagen.DePng(File.ReadAllBytes(dorado));
        Assert.Equal((esperado.Ancho, esperado.Alto), (obtenido.Ancho, obtenido.Alto));
        Assert.True(esperado.Negro.SequenceEqual(obtenido.Negro), $"el PNG difiere del dorado; el obtenido quedó en {ruta}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(256)]
    [InlineData(100_000)]
    public void ElTroceoNoCambiaElResultado(int trozo)
    {
        foreach (var vector in Nombres)
        {
            var bytes = Dorados.Leer(vector + ".bin");
            var entero = Assert.Single(Interpretar(bytes, bytes.Length));
            var troceado = Assert.Single(Interpretar(bytes, trozo));
            Assert.True(Imagen.DePng(entero.Png).Negro.SequenceEqual(Imagen.DePng(troceado.Png).Negro), vector);
        }
    }

    [Fact]
    public void UnComandoDesconocidoSeRegistraYSigue()
    {
        byte[] desconocido = [0x1D, 0x99];
        var con = B(0x1B, 0x40, "Antes", 0x0A, desconocido, "Despues", 0x0A, 0x1D, 0x56, 0x01);
        var sin = B(0x1B, 0x40, "Antes", 0x0A, "Despues", 0x0A, 0x1D, 0x56, 0x01);

        var ticket = Assert.Single(Interpretar(con));
        var referencia = Assert.Single(Interpretar(sin));

        var aviso = Assert.Single(ticket.Avisos);
        Assert.Contains("desconocido", aviso);
        Assert.Contains("1D 99", aviso);
        Assert.True(Imagen.DePng(referencia.Png).Negro.SequenceEqual(Imagen.DePng(ticket.Png).Negro),
            "el resto del ticket no se interpretó igual");
        Assert.True(Imagen.DePng(ticket.Png).PuntosNegros > 0);
    }

    [Fact]
    public void LaImagenRasterSeDibujaBitABit()
    {
        // Oráculo independiente del dorado: los bits de GS v 0 que emitió la librería aparecen tal cual en el PNG.
        var bytes = Dorados.Leer("03-imagen.bin");
        int i = Enumerable.Range(0, bytes.Length - 4).First(k => bytes[k] == 0x1D && bytes[k + 1] == 0x76 && bytes[k + 2] == 0x30);
        int x = bytes[i + 4] | (bytes[i + 5] << 8), y = bytes[i + 6] | (bytes[i + 7] << 8);
        var bits = bytes.AsSpan(i + 8, x * y).ToArray();

        var png = Imagen.DePng(Assert.Single(Interpretar(bytes)).Png);

        bool Coincide(int fila0)
        {
            for (int f = 0; f < y; f++)
                for (int c = 0; c < x * 8; c++)
                    if (png[c, fila0 + f] != (((bits[f * x + (c >> 3)] >> (7 - (c & 7))) & 1) == 1)) return false;
            return true;
        }
        Assert.Contains(Enumerable.Range(0, png.Alto - y + 1), Coincide);
    }

    [Theory]
    [InlineData("qr-1")]
    [InlineData("qr-2")]
    [InlineData("qr-3")]
    public void ElQrCoincideConLaReferencia(string vector)
    {
        var lineas = File.ReadAllLines(Path.Combine(Dorados.Carpeta, vector + ".txt"));
        var datos = lineas[0]["# datos: ".Length..];
        var filas = lineas.Where(l => !l.StartsWith("# ")).ToArray();

        var matriz = CodigoQr.Generar(System.Text.Encoding.ASCII.GetBytes(datos), CodigoQr.Correccion.H);

        Assert.Equal(filas.Length, matriz.GetLength(0));
        for (int f = 0; f < filas.Length; f++)
            Assert.Equal(filas[f], new string(Enumerable.Range(0, filas.Length).Select(c => matriz[f, c] ? '#' : '.').ToArray()));
    }

    [Fact]
    public void Ean13CalculaElDigitoVerificador()
    {
        // Ejemplo de https://en.wikipedia.org/wiki/International_Article_Number §Check digit: 400638133393 → 1.
        Assert.Equal("4006381333931", CodigoEan13.Completar("400638133393"));
        // Dato de la prueba de la librería (BarcodeEscPosTests): 7·1+7·3+9·1+1·3+2·1+3·3+4·1+5·3+6·1+7·3+8·1+9·3 = 132 → 8.
        Assert.Equal("7791234567898", CodigoEan13.Completar("779123456789"));
        Assert.Equal("7791234567898", CodigoEan13.Completar("7791234567898"));
        Assert.Null(CodigoEan13.Completar("7791234567890"));
        // Guarda, primer dígito 7 → paridad LGLGLG: el 7 de la posición 2 va en L (0111011).
        Assert.Equal("1010111011", new string(CodigoEan13.Modulos("7791234567898").Take(10).Select(b => b ? '1' : '0').ToArray()));
        Assert.Equal(95, CodigoEan13.Modulos("7791234567898").Length);
        Assert.Null(CodigoEan13.Completar("77912345678"));
    }

    [Theory]
    [InlineData(EstadoDelPapel.Normal, 0x12, 0x12, 0x12)]
    [InlineData(EstadoDelPapel.SinPapel, 0x1A, 0x32, 0x72)]
    [InlineData(EstadoDelPapel.TapaAbierta, 0x1A, 0x16, 0x12)]
    public void LasConsultasDeEstadoRespondenSegunElPapel(EstadoDelPapel estado, int n1, int n2, int n4)
    {
        var respuestas = new List<byte[]>();
        var interprete = new Interprete(new PerfilDeImpresora { Estado = estado });
        interprete.Respuesta += respuestas.Add;
        var tickets = new List<TicketInterpretado>();
        interprete.TicketTerminado += tickets.Add;

        interprete.Alimentar([0x10, 0x04]);
        interprete.Alimentar([0x01, 0x10, 0x04, 0x02, 0x10, 0x04, 0x04]);
        interprete.Finalizar();

        Assert.Equal([[(byte)n1], [(byte)n2], [(byte)n4]], respuestas);
        Assert.Empty(tickets);
    }

    [Fact]
    public void ElModeloYLaCapacidadNvRespondenComoSeConfiguro()
    {
        byte[] gsI67 = [0x1D, 0x49, 0x43], nv = [0x1D, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x30];
        var porDefecto = new List<byte[]>();
        var a = new Interprete();
        a.Respuesta += porDefecto.Add;
        a.Alimentar([.. gsI67, .. nv]);

        var conNv = new List<byte[]>();
        var b = new Interprete(new PerfilDeImpresora { RespuestaNv = [0x01] });
        b.Respuesta += conNv.Add;
        b.Alimentar(nv);

        var modelo = Assert.Single(porDefecto);
        Assert.Equal("58HB6", System.Text.Encoding.ASCII.GetString(modelo).Trim('\0', ' ', '\r', '\n', '\t'));
        Assert.Equal([[0x01]], conNv);
    }

    [Fact]
    public void UnTicketVacioSeGuardaEnBlancoYUnCorteSueltoNo()
    {
        var vacio = Assert.Single(Interpretar([0x1B, 0x40, 0x1D, 0x56, 0x01]));
        Assert.Equal(384, vacio.Ancho);
        Assert.Equal(0, Imagen.DePng(vacio.Png).PuntosNegros);

        var avisos = new List<string>();
        var interprete = new Interprete();
        interprete.Aviso += avisos.Add;
        var tickets = new List<TicketInterpretado>();
        interprete.TicketTerminado += tickets.Add;
        interprete.Alimentar([0x1D, 0x56, 0x01]);
        interprete.Finalizar();
        Assert.Empty(tickets);
        Assert.Contains(avisos, a => a.Contains("corte"));
    }

    [Fact]
    public void UnaImagenMasAnchaQue384SeRecortaConAviso()
    {
        const int x = 50, y = 2; // 400 puntos
        var bits = Enumerable.Repeat((byte)0xFF, x * y).ToArray();
        var bytes = B(0x1B, 0x40, 0x1D, 0x76, 0x30, 0x00, x, 0, y, 0, bits, 0x1D, 0x56, 0x01);

        var ticket = Assert.Single(Interpretar(bytes));

        Assert.Equal(384, ticket.Ancho);
        Assert.Contains(ticket.Avisos, a => a.Contains("384"));
        var png = Imagen.DePng(ticket.Png);
        int fila = Enumerable.Range(0, png.Alto).First(f => png[0, f]);
        Assert.All(Enumerable.Range(0, 384), c => Assert.True(png[c, fila] && png[c, fila + 1]));
    }

    [Fact]
    public void UnEscArrobaConContenidoCierraElTicketAnterior()
    {
        var bytes = B(0x1B, 0x40, "Uno", 0x0A, 0x1B, 0x64, 0x04, 0x1B, 0x40, "Dos", 0x0A, 0x1D, 0x56, 0x01);

        var tickets = Interpretar(bytes);

        Assert.Equal(2, tickets.Count);
        Assert.Equal(bytes.Length, tickets.Sum(t => t.Bytes.Length));
    }
}
