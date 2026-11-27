// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>Cómo se presenta la impresora emulada.</summary>
public sealed class OpcionesPeriferico
{
    /// <summary>Nombre local (HCI Write Local Name y EIR) y nombre del servicio en SDP.</summary>
    public string Nombre { get; init; } = "58HB6";

    /// <summary>Clase de dispositivo: 0x040680 = impresora (mayor «imaging», menor «printer»), la que usó la prueba corta.</summary>
    public uint ClaseDeDispositivo { get; init; } = 0x040680;

    /// <summary>Canal RFCOMM del perfil de puerto serie publicado en SDP.</summary>
    public byte CanalRfcomm { get; init; } = 1;

    /// <summary>PIN para el emparejamiento heredado (sólo si Android no usa SSP). El de la 58HB6 según la librería cliente.</summary>
    public string Pin { get; init; } = "0000";

    /// <summary>Tamaño máximo de SDU L2CAP que este lado acepta (MTU propio).</summary>
    public ushort MtuL2cap { get; init; } = 1024;

    /// <summary>Créditos RFCOMM que se le dan al otro lado al abrir el canal y al reponer.</summary>
    public byte CreditosRfcomm { get; init; } = 7;

    /// <summary>
    /// Tras un emparejamiento que terminó sin cifrar (el dedicado de Ajustes de Android), hace una conexión breve con el
    /// teléfono y la corta (LAZ-0011, ADR-0009).
    /// </summary>
    public bool ReconexionTrasVinculoSinCifrar { get; init; } = true;

    /// <summary>Registra cada comando y evento HCI (sin los datos ACL ni el control de flujo), para diagnosticar.</summary>
    public bool TrazaHci { get; init; }
}

/// <summary>Estado observable del periférico.</summary>
public enum EstadoPeriferico
{
    Detenido,
    Conectando,
    Iniciando,
    Anunciando,
    Error,
}
