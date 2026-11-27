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
        var nueva = Nueva(conexion);
        Conexion? vieja = null;
        _conexiones.AddOrUpdate(conexion.Id, nueva, (_, anterior) => { vieja = anterior; return nueva; });
        if (vieja is not null)
        {
            // Los ids de conexión vuelven a empezar con cada periférico nuevo: una entrada vieja con el mismo id es de
            // una conexión que no avisó su cierre. Se termina su ticket y no se mezcla con la nueva.
            Terminar(vieja);
            _registro($"conexión {conexion.Id}: había una anterior sin cerrar; se terminó");
        }
        _registro($"conexión {conexion.Id} abierta desde {conexion.Par} (canal {conexion.Canal})");
    }

    /// <inheritdoc />
    public void Recibidos(InfoConexionRfcomm conexion, ReadOnlySpan<byte> datos)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        if (!_conexiones.TryGetValue(conexion.Id, out var c))
        {
            _registro($"conexión {conexion.Id}: {datos.Length} bytes sin conexión abierta, descartados");
            return;
        }
        lock (c.Cerrojo)
        {
            if (c.Cerrada)
            {
                _registro($"conexión {conexion.Id}: {datos.Length} bytes después del cierre, descartados");
                return;
            }
            c.Interprete.Alimentar(datos);
        }
    }

    /// <inheritdoc />
    public void Cerrada(InfoConexionRfcomm conexion)
    {
        ArgumentNullException.ThrowIfNull(conexion);
        if (_conexiones.TryRemove(conexion.Id, out var c))
        {
            Terminar(c);
            _registro($"conexión {conexion.Id} cerrada");
        }
    }

    private static void Terminar(Conexion c)
    {
        lock (c.Cerrojo)
        {
            if (!c.Cerrada)
            {
                c.Cerrada = true;
                c.Interprete.Finalizar();
            }
        }
    }

    private Conexion Nueva(InfoConexionRfcomm conexion)
    {
        var interprete = new Interprete(perfil);
        interprete.TicketTerminado += Guardar;
        interprete.Aviso += a => _registro($"conexión {conexion.Id}: {a}");
        return new Conexion(interprete);
    }

    private void Guardar(TicketInterpretado ticket)
    {
        try
        {
            var g = almacen.Guardar(ticket);
            _registro($"ticket {g.Id}: {ticket.Bytes.Length} bytes, {ticket.Ancho}x{ticket.Alto}");
        }
        catch (Exception e)
        {
            // Ningún error de guardado sale por Alimentar: cortaría el medio Bluetooth y todas sus conexiones.
            _registro($"no se pudo guardar un ticket de {ticket.Bytes.Length} bytes en {almacen.Carpeta}: {e.Message}");
        }
    }

    private sealed class Conexion(Interprete interprete)
    {
        public Interprete Interprete { get; } = interprete;

        public Lock Cerrojo { get; } = new();

        public bool Cerrada { get; set; }
    }
}
