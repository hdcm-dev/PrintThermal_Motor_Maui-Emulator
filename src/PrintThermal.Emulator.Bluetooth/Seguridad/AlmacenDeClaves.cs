// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using System.Text.Json;
using PrintThermal.Emulator.Bluetooth.Hci;

namespace PrintThermal.Emulator.Bluetooth.Seguridad;

/// <summary>
/// Claves de enlace en un archivo JSON (<c>{"02:00:00:00:00:01": {"Clave": "…hex…", "Tipo": 4}}</c>), para que el
/// vínculo con Android sobreviva al reinicio del emulador de impresora. Son claves de un medio emulado.
/// </summary>
public sealed class AlmacenDeClavesEnArchivo(string ruta) : IAlmacenDeClaves
{
    private sealed record Entrada(string Clave, byte Tipo);

    private readonly object _sync = new();

    public string Ruta { get; } = ruta;

    public ClaveDeEnlace? Buscar(DireccionBluetooth par)
    {
        lock (_sync)
        {
            return Leer().TryGetValue(par.ToString(), out var e) ? new ClaveDeEnlace(Convert.FromHexString(e.Clave), e.Tipo) : null;
        }
    }

    public void Guardar(DireccionBluetooth par, ClaveDeEnlace clave)
    {
        lock (_sync)
        {
            var todas = Leer();
            todas[par.ToString()] = new Entrada(Convert.ToHexString(clave.Clave), clave.Tipo);
            var dir = Path.GetDirectoryName(Path.GetFullPath(Ruta));
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var temporal = Ruta + ".tmp";
            File.WriteAllText(temporal, JsonSerializer.Serialize(todas, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporal, Ruta, overwrite: true);
        }
    }

    private Dictionary<string, Entrada> Leer() =>
        File.Exists(Ruta)
            ? JsonSerializer.Deserialize<Dictionary<string, Entrada>>(File.ReadAllText(Ruta)) ?? []
            : [];
}

/// <summary>Claves en memoria: se pierden al reiniciar.</summary>
public sealed class AlmacenDeClavesEnMemoria : IAlmacenDeClaves
{
    private readonly Dictionary<DireccionBluetooth, ClaveDeEnlace> _claves = [];

    public ClaveDeEnlace? Buscar(DireccionBluetooth par)
    {
        lock (_claves)
        {
            return _claves.GetValueOrDefault(par);
        }
    }

    public void Guardar(DireccionBluetooth par, ClaveDeEnlace clave)
    {
        lock (_claves)
        {
            _claves[par] = clave;
        }
    }
}
