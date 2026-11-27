// cdd: REQ-HIS-Ticket-A-Png
namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>Un ticket terminado: los bytes que lo formaron, su PNG y lo que el intérprete registró al leerlo.</summary>
/// <param name="Bytes">Bytes recibidos desde el inicio del ticket hasta su fin (corte, nuevo ESC @ o fin de la conexión).</param>
/// <param name="Png">Imagen PNG en escala de grises de 1 bit; negro = punto impreso.</param>
/// <param name="Ancho">Ancho del PNG en puntos.</param>
/// <param name="Alto">Alto del PNG en puntos.</param>
/// <param name="Avisos">Comandos desconocidos, recortes y otras anomalías, en orden de aparición.</param>
public sealed record TicketInterpretado(byte[] Bytes, byte[] Png, int Ancho, int Alto, IReadOnlyList<string> Avisos);
