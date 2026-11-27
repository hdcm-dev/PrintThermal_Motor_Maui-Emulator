// cdd: REQ-HIS-Ticket-A-Png
namespace PrintThermal.Emulator.Core.EscPos;

/// <summary>Estado del papel y de la tapa que informan las consultas DLE EOT.</summary>
public enum EstadoDelPapel
{
    Normal,
    SinPapel,
    TapaAbierta,
}

/// <summary>
/// Parámetros de la impresora emulada. Los valores por defecto salen de lo que la librería cliente emite y espera
/// (Especificacion-58HB6 §2, §4 y §5); los que dependen de la 58HB6 física no están medidos (intake §6) y se pueden cambiar.
/// </summary>
public sealed record PerfilDeImpresora
{
    /// <summary>Puntos por línea del cabezal: 384 (58 mm). NO VERIFICADO en la 58HB6 física.</summary>
    public int AnchoPuntos { get; init; } = 384;

    /// <summary>Interlineado por defecto en puntos (ESC 2): fuente de 24 más 6. NO VERIFICADO.</summary>
    public int Interlineado { get; init; } = 30;

    /// <summary>Margen en blanco arriba y abajo del PNG, en puntos.</summary>
    public int Margen { get; init; } = 8;

    /// <summary>Alto máximo de un ticket en puntos (unos 2,5 m de papel a 8 puntos por mm): lo que sigue no se dibuja.</summary>
    public int AltoMaximo { get; init; } = 20_000;

    /// <summary>Respuesta a DLE EOT 1, 2 y 4 (tabla de Especificacion-58HB6 §4.3).</summary>
    public EstadoDelPapel Estado { get; init; } = EstadoDelPapel.Normal;

    /// <summary>Respuesta a GS I 67 (nombre del modelo). Sin cabecera: el formato oficial no está verificado.</summary>
    public byte[] RespuestaModelo { get; init; } = [(byte)'5', (byte)'8', (byte)'H', (byte)'B', (byte)'6', 0x00];

    /// <summary>Respuesta a GS ( L fn 48 (capacidad NV). Null: no responde, que es lo recomendado hasta medir.</summary>
    public byte[]? RespuestaNv { get; init; }

    /// <summary>Alto de las barras de un código de barras (GS h) y ancho de módulo (GS w), en puntos. NO VERIFICADO.</summary>
    public int AltoDeBarras { get; init; } = 80;

    /// <inheritdoc cref="AltoDeBarras"/>
    public int AnchoDeModulo { get; init; } = 2;
}
