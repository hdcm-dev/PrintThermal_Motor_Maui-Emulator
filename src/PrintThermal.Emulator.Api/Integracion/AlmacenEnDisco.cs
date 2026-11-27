// cdd: REQ-HIS-Api-De-Tickets
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Core.EscPos;
using Disco = PrintThermal.Emulator.Core.Almacen;

namespace PrintThermal.Emulator.Api.Integracion;

/// <summary>
/// <see cref="IAlmacenDeTickets"/> sobre el almacén en disco de PAR-PTE-02 (PAR-PTE-05, D2; LAZ-0006). La carpeta es la
/// fuente de verdad: los ids <c>AAAAMMDDTHHMMSSmmm-NNNN</c> ordenan por llegada, no llevan puntos ni barras y son
/// mayores que <c>"0"</c>, como pide el contrato de la API. La espera se despierta con cada ticket guardado por este
/// host y, además, relee la carpeta cada segundo para ver los que guarde otra instancia.
/// </summary>
public sealed class AlmacenEnDisco : IAlmacenDeTickets
{
    private static readonly TimeSpan Relectura = TimeSpan.FromSeconds(1);
    private readonly Disco.AlmacenDeTickets _disco;
    private readonly object _cerrojo = new();
    private TaskCompletionSource _llegada = NuevaSenal();

    /// <summary>Abre (y crea si falta) la carpeta de salida.</summary>
    public AlmacenEnDisco(string carpeta, TimeProvider? reloj = null) => _disco = new Disco.AlmacenDeTickets(carpeta, reloj);

    /// <summary>Carpeta de salida, con ruta completa.</summary>
    public string Carpeta => _disco.Carpeta;

    /// <summary>Guarda un ticket que terminó el intérprete y despierta a quienes esperan.</summary>
    public TicketGuardado Guardar(TicketInterpretado ticket)
    {
        var guardado = _disco.Guardar(ticket);
        Senalar();
        return new TicketGuardado(guardado.Id, guardado.Fecha, ticket.Bytes, ticket.Png);
    }

    /// <inheritdoc />
    public TicketGuardado Guardar(byte[] bin, byte[] png)
    {
        ArgumentNullException.ThrowIfNull(bin);
        ArgumentNullException.ThrowIfNull(png);
        var (ancho, alto) = Png.Tamano(png);
        return Guardar(new TicketInterpretado(bin.ToArray(), png.ToArray(), ancho, alto, []));
    }

    /// <inheritdoc />
    public IReadOnlyList<TicketGuardado> Listar() =>
        _disco.Listar().Reverse().Select(Leer).OfType<TicketGuardado>().ToList();

    /// <inheritdoc />
    public TicketGuardado? Obtener(string id) => _disco.Obtener(id) is { } t ? Leer(t) : null;

    /// <inheritdoc />
    public TicketGuardado? Ultimo() => _disco.Ultimo() is { } t ? Leer(t) : null;

    /// <inheritdoc />
    public bool Borrar(string id) => _disco.Borrar(id);

    /// <inheritdoc />
    public void BorrarTodos() => _disco.BorrarTodo();

    /// <inheritdoc />
    public async Task<TicketGuardado> EsperarAsync(string? despuesDe, CancellationToken ct)
    {
        var umbral = string.IsNullOrEmpty(despuesDe) ? _disco.Ultimo()?.Id : despuesDe;
        while (true)
        {
            Task senal;
            lock (_cerrojo)
            {
                senal = _llegada.Task;
            }
            foreach (var t in _disco.Listar())
            {
                if ((umbral is null || string.CompareOrdinal(t.Id, umbral) > 0) && Leer(t) is { } ticket)
                {
                    return ticket;
                }
            }
            await Task.WhenAny(senal, Task.Delay(Relectura, ct)).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
        }
    }

    private TicketGuardado? Leer(Disco.TicketGuardado t) =>
        _disco.LeerBin(t.Id) is { } bin && _disco.LeerPng(t.Id) is { } png ? new TicketGuardado(t.Id, t.Fecha, bin, png) : null;

    private void Senalar()
    {
        TaskCompletionSource llegada;
        lock (_cerrojo)
        {
            llegada = _llegada;
            _llegada = NuevaSenal();
        }
        llegada.TrySetResult();
    }

    private static TaskCompletionSource NuevaSenal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
