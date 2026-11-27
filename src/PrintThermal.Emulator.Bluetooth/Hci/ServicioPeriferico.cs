// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.Rfcomm;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Bluetooth.Hci;

/// <summary>
/// Mantiene el periférico vivo: conecta el transporte (netsim por gRPC), corre el periférico y, si la conexión falla o
/// se corta, lo informa en <see cref="Estado"/> y <see cref="UltimoError"/> y reintenta tras una espera.
/// </summary>
public sealed class ServicioPeriferico(Func<CancellationToken, Task<ITransporteHci>> conectar, OpcionesPeriferico opciones,
    IReceptorDeBytes receptor, IAlmacenDeClaves claves, TimeSpan esperaEntreIntentos, Action<string>? registro = null)
{
    private readonly Action<string> _registro = registro ?? (_ => { });
    private volatile PerifericoBluetooth? _actual;
    private volatile string? _ultimoError;
    private int _intentos;
    private int _estado = (int)EstadoPeriferico.Detenido;

    public EstadoPeriferico Estado
    {
        get
        {
            var e = (EstadoPeriferico)Volatile.Read(ref _estado);
            return e == EstadoPeriferico.Iniciando && _actual?.Estado == EstadoPeriferico.Anunciando ? EstadoPeriferico.Anunciando : e;
        }
    }

    /// <summary>Mensaje de la última falla de conexión o del último corte; queda hasta la próxima falla.</summary>
    public string? UltimoError => _ultimoError;

    public int Intentos => Volatile.Read(ref _intentos);

    /// <summary>El periférico de la conexión vigente, si hay.</summary>
    public PerifericoBluetooth? Periferico => _actual;

    public async Task EjecutarAsync(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            Interlocked.Increment(ref _intentos);
            Poner(EstadoPeriferico.Conectando);
            try
            {
                await using var transporte = await conectar(ct);
                var p = new PerifericoBluetooth(transporte, opciones, receptor, claves, _registro);
                _actual = p;
                Poner(EstadoPeriferico.Iniciando);
                await p.EjecutarAsync(ct);
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                Fallo("netsim cerró la conexión");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                Fallo(e.Message);
            }
            finally
            {
                _actual = null;
            }
            try
            {
                await Task.Delay(esperaEntreIntentos, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        Poner(EstadoPeriferico.Detenido);
    }

    private void Fallo(string mensaje)
    {
        _ultimoError = mensaje;
        Poner(EstadoPeriferico.Error);
        _registro($"periférico: {mensaje}; reintento en {esperaEntreIntentos.TotalSeconds:0.#} s");
    }

    private void Poner(EstadoPeriferico e) => Volatile.Write(ref _estado, (int)e);
}
