// cdd: REQ-HIS-Api-De-Tickets
using System.Collections.Concurrent;
using PrintThermal.Emulator.Bluetooth.Rfcomm;
using PrintThermal.Emulator.Core.EscPos;

namespace PrintThermal.Emulator.Api.Integracion;

/// <summary>
/// Une la recepción RFCOMM (PAR-PTE-03) con el intérprete y el almacén en disco (PAR-PTE-02): un intérprete por
/// conexión, alimentado con los bytes tal como llegan; cada ticket que termina se guarda. El cierre de la conexión
/// termina el ticket en curso aunque no traiga corte (PAR-PTE-05, D2; LAZ-0010 y LAZ-0001). Las respuestas a
/// consultas (DLE EOT, GS I) no vuelven a la app: la frontera <see cref="IReceptorDeBytes"/> no tiene canal de vuelta.
/// </summary>
public sealed class ReceptorAlAlmacen(AlmacenEnDisco almacen, PerfilDeImpresora? perfil = null, Action<string>? registro = null)
    : IReceptorDeBytes
{
    private readonly ConcurrentDictionary<long, Conexion> _conexiones = new();
    private readonly Action<string> _registro = registro ?? (_ => { });

    /// <summary>Conexiones RFCOMM abiertas ahora.</summary>
    public int Abiertas => _conexiones.Count;

    /// <inheritdoc />
    public void Abierta(InfoConexionRfcomm conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        var interprete = new Interprete(perfil);
        interprete.TicketTerminado += Guardar;
        interprete.Aviso += a => _registro($"conexión {conexion.Id}: {a}");
        var anterior = _conexiones.GetOrAdd(conexion.Id, _ => new Conexion(interprete));
        if (!ReferenceEquals(anterior.Interprete, interprete))
        {
            _registro($"conexión {conexion.Id}: abierta dos veces; sigue la primera");
        }
        _registro($"conexión {conexion.Id} abierta desde {conexion.Par} (canal {conexion.Canal})");
    }

    /// <inheritdoc />
    public void Recibidos(InfoConexionRfcomm conexion, ReadOnlySpan<byte> datos)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        if (!_conexiones.TryGetValue(conexion.Id, out var c))
        {
            Abierta(conexion);
            c = _conexiones[conexion.Id];
        }
        lock (c.Cerrojo)
        {
            c.Interprete.Alimentar(datos);
        }
    }

    /// <inheritdoc />
    public void Cerrada(InfoConexionRfcomm conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        if (_conexiones.TryRemove(conexion.Id, out var c))
        {
            lock (c.Cerrojo)
            {
                c.Interprete.Finalizar();
            }
            _registro($"conexión {conexion.Id} cerrada");
        }
    }

    private void Guardar(TicketInterpretado ticket)
    {
        try
        {
            var g = almacen.Guardar(ticket);
            _registro($"ticket {g.Id}: {ticket.Bytes.Length} bytes, {ticket.Ancho}x{ticket.Alto}");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _registro($"no se pudo guardar un ticket de {ticket.Bytes.Length} bytes en {almacen.Carpeta}: {e.Message}");
        }
    }

    private sealed class Conexion(Interprete interprete)
    {
        public Interprete Interprete { get; } = interprete;

        public Lock Cerrojo { get; } = new();
    }
}
