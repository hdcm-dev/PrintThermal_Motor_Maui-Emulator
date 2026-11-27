using System.Globalization;

namespace PrintThermal.Emulator.Api.Api;

/// <summary>
/// Almacén de tickets en memoria, con capacidad acotada (descarta el más viejo). Es el de esta parcela (D4):
/// no persiste nada; el almacén en disco de REQ-HIS-Almacen-De-Tickets lo reemplaza detrás de la interfaz.
/// </summary>
public sealed class AlmacenEnMemoria : IAlmacenDeTickets
{
    /// <summary>Capacidad por omisión.</summary>
    public const int CapacidadPorOmision = 500;

    private readonly object _cerrojo = new();
    private readonly List<TicketGuardado> _tickets = [];
    private readonly TimeProvider _reloj;
    private readonly int _capacidad;
    private long _ultimaSecuencia;
    private DateTimeOffset _ultimaHora = DateTimeOffset.MinValue;
    private TaskCompletionSource _llegada = NuevaSenal();

    /// <summary>Crea el almacén con el reloj del sistema y la capacidad por omisión.</summary>
    public AlmacenEnMemoria() : this(TimeProvider.System, CapacidadPorOmision)
    {
    }

    /// <summary>Crea el almacén con un reloj y una capacidad dados.</summary>
    public AlmacenEnMemoria(TimeProvider reloj, int capacidad)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacidad, 1);
        _reloj = reloj;
        _capacidad = capacidad;
    }

    /// <inheritdoc />
    public TicketGuardado Guardar(byte[] bin, byte[] png)
    {
        ArgumentNullException.ThrowIfNull(bin);
        ArgumentNullException.ThrowIfNull(png);
        TicketGuardado ticket;
        TaskCompletionSource llegada;
        lock (_cerrojo)
        {
            var secuencia = ++_ultimaSecuencia;
            var hora = _reloj.GetUtcNow();
            if (hora < _ultimaHora)
            {
                hora = _ultimaHora; // el reloj no hace retroceder el orden de los ids
            }

            _ultimaHora = hora;
            ticket = new TicketGuardado(Id(hora, secuencia), hora, bin.ToArray(), png.ToArray());
            _tickets.Add(ticket);
            if (_tickets.Count > _capacidad)
            {
                _tickets.RemoveAt(0);
            }

            llegada = _llegada;
            _llegada = NuevaSenal();
        }

        llegada.TrySetResult();
        return ticket;
    }

    /// <inheritdoc />
    public IReadOnlyList<TicketGuardado> Listar()
    {
        lock (_cerrojo)
        {
            return _tickets.AsEnumerable().Reverse().ToList();
        }
    }

    /// <inheritdoc />
    public TicketGuardado? Obtener(string id)
    {
        lock (_cerrojo)
        {
            var i = Indice(id);
            return i < 0 ? null : _tickets[i];
        }
    }

    /// <inheritdoc />
    public TicketGuardado? Ultimo()
    {
        lock (_cerrojo)
        {
            return _tickets.Count == 0 ? null : _tickets[^1];
        }
    }

    /// <inheritdoc />
    public bool Borrar(string id)
    {
        lock (_cerrojo)
        {
            var i = Indice(id);
            if (i < 0)
            {
                return false;
            }

            _tickets.RemoveAt(i);
            return true;
        }
    }

    /// <inheritdoc />
    public void BorrarTodos()
    {
        lock (_cerrojo)
        {
            _tickets.Clear();
        }
    }

    /// <inheritdoc />
    public async Task<TicketGuardado> EsperarAsync(string? despuesDe, CancellationToken ct)
    {
        string? umbral;
        lock (_cerrojo)
        {
            umbral = string.IsNullOrEmpty(despuesDe) ? (_tickets.Count == 0 ? null : _tickets[^1].Id) : despuesDe;
        }

        while (true)
        {
            Task senal;
            lock (_cerrojo)
            {
                foreach (var ticket in _tickets)
                {
                    if (umbral is null || string.CompareOrdinal(ticket.Id, umbral) > 0)
                    {
                        return ticket;
                    }
                }

                senal = _llegada.Task;
            }

            await senal.WaitAsync(ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Id ordenable: hora UTC hasta el milisegundo y secuencia del proceso, de ancho fijo y sin puntos
    /// (por ejemplo <c>20261127T143005123-000001</c>).
    /// </summary>
    public static string Id(DateTimeOffset hora, long secuencia) =>
        hora.UtcDateTime.ToString("yyyyMMdd'T'HHmmssfff", CultureInfo.InvariantCulture) + "-"
        + secuencia.ToString("D6", CultureInfo.InvariantCulture);

    private int Indice(string id) => _tickets.FindIndex(t => string.Equals(t.Id, id, StringComparison.Ordinal));

    private static TaskCompletionSource NuevaSenal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
