// cdd: REQ-HIS-Api-De-Tickets
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Hci.Netsim;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Api.Integracion;

/// <summary>Configuración del medio Bluetooth (sección <c>Emulador:Bluetooth</c>).</summary>
public sealed class OpcionesDelMedio
{
    /// <summary>URL del gRPC de netsim, p. ej. <c>http://127.0.0.1:8554</c> (loopback de la red compartida).</summary>
    public string? Netsim { get; set; }

    /// <summary>Dirección Bluetooth fija de la impresora emulada.</summary>
    public string Direccion { get; set; } = "04:00:00:00:00:01";

    /// <summary>Archivo de claves de emparejamiento; por omisión, <c>claves-bluetooth.txt</c> en la carpeta de salida.</summary>
    public string? ArchivoDeClaves { get; set; }

    /// <summary>Tras un vínculo que terminó sin cifrar, una conexión breve con el teléfono (LAZ-0011, ADR-0009).</summary>
    public bool ReconexionTrasVinculoSinCifrar { get; set; } = true;

    /// <summary>Registra cada comando y evento HCI del periférico (diagnóstico).</summary>
    public bool TrazaHci { get; set; }
}

/// <summary>
/// Corre el periférico «58HB6» de PAR-PTE-03 contra netsim como servicio del host y entrega los bytes al
/// <see cref="ReceptorAlAlmacen"/> (PAR-PTE-05, D2; LAZ-0006). Informa su estado a la API y al visor.
/// </summary>
public sealed class MedioBluetooth : BackgroundService, IEstadoDelMedio
{
    private readonly ServicioPeriferico _servicio;
    private readonly ReceptorAlAlmacen _receptor;

    /// <summary>Prepara el periférico con la dirección y el archivo de claves de <paramref name="opciones"/>.</summary>
    public MedioBluetooth(OpcionesDelMedio opciones, string carpetaDeSalida, ReceptorAlAlmacen receptor, ILogger<MedioBluetooth> log)
    {
        ArgumentNullException.ThrowIfNull(opciones);
        ArgumentException.ThrowIfNullOrWhiteSpace(opciones.Netsim);
        var url = new Uri(opciones.Netsim);
        var direccion = DireccionBluetooth.Parse(opciones.Direccion);
        var periferico = new OpcionesPeriferico { TrazaHci = opciones.TrazaHci, ReconexionTrasVinculoSinCifrar = opciones.ReconexionTrasVinculoSinCifrar };
        var claves = new AlmacenDeClavesEnArchivo(opciones.ArchivoDeClaves ?? Path.Combine(carpetaDeSalida, "claves-bluetooth.txt"));
        _receptor = receptor;
        _servicio = new ServicioPeriferico(
            _ => Task.FromResult<ITransporteHci>(TransporteNetsim.Conectar(url, direccion, periferico.Nombre)),
            periferico, receptor, claves, TimeSpan.FromSeconds(2), m => log.LogInformation("{Mensaje}", m));
        log.LogInformation("Medio Bluetooth: «{Nombre}» en {Direccion}, netsim {Netsim}", periferico.Nombre, direccion, url);
    }

    /// <inheritdoc />
    public string Nombre => $"Bluetooth «58HB6» ({_servicio.Estado}{(_receptor.Abiertas > 0 ? ", con conexión RFCOMM" : "")})";

    /// <inheritdoc />
    public bool Conectado => _servicio.Estado == EstadoPeriferico.Anunciando;

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => _servicio.EjecutarAsync(stoppingToken);
}
