// cdd: REQ-HIS-Descubrimiento-Y-Emparejamiento
using PrintThermal.Emulator.Bluetooth.Hci;
using PrintThermal.Emulator.Bluetooth.Seguridad;

namespace PrintThermal.Emulator.Tests.Bluetooth;

/// <summary>
/// LAZ-0011 (PAR-PTE-07): un emparejamiento dedicado (Ajustes de Android, createBond) termina sin cifrar el enlace.
/// Android con netsim puede atribuir ese cierre a otro vínculo guardado (manejador 0) y quedar «autenticando»;
/// la impresora hace entonces una conexión breve y la corta, para que Android cierre el vínculo con el manejador bueno.
/// </summary>
public sealed class SeguridadVinculoSinCifrarTests
{
    private const ushort HandleBreve = 0x000C;

    private static async Task<(ControladorFalso C, Task Corrida)> EmparejarAsync(bool reconexion = true)
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico { ReconexionTrasVinculoSinCifrar = reconexion },
            new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        await new AndroidSimulado(c).ConectarAclAsync();
        var par = AndroidSimulado.Direccion.ComoHci();
        c.Evento(0x31, par);
        await c.EsperarComandoAsync(0x042B);
        c.Evento(0x32, [.. par, 0x01, 0x00, 0x03]); // Android: pantalla sí/no, emparejamiento dedicado con MITM
        c.Evento(0x33, [.. par, 0x1B, 0x00, 0x00, 0x00]);
        await c.EsperarComandoAsync(0x042C);
        c.Evento(0x36, [0x00, .. par]);
        c.Evento(0x18, [.. par, .. Enumerable.Repeat((byte)0x01, 16), 0x04]);
        return (c, corrida);
    }

    private static byte[] Cierre(ushort handle) => [0x00, (byte)handle, (byte)(handle >> 8), 0x16];

    /// <summary>
    /// Punto de sincronía: un evento que el host contesta. Los eventos se procesan en orden, así que cuando llega la
    /// respuesta ya se atendió todo lo anterior; después se espera además 1,5 s por si una implementación difiere su
    /// reacción.
    /// </summary>
    private static async Task SincronizarAsync(ControladorFalso c)
    {
        var n = c.Comandos.Count;
        c.Evento(0x31, AndroidSimulado.Direccion.ComoHci());
        await c.EsperarComandoAsync(0x042B, n);
        await Task.Delay(1500);
    }

    [Fact]
    public async Task TrasUnVinculoSinCifrarLaImpresoraReconectaYCorta()
    {
        var (c, corrida) = await EmparejarAsync();
        var n = c.Comandos.Count;
        c.Evento(0x05, Cierre(c.Handle)); // Android corta sin haber cifrado (emparejamiento dedicado)

        var crear = ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0405, n));
        Assert.Equal(AndroidSimulado.Direccion.ComoHci(), crear[..6]);

        n = c.Comandos.Count;
        c.Evento(0x03, [0x00, (byte)HandleBreve, 0x00, .. AndroidSimulado.Direccion.ComoHci(), 0x01, 0x00]);
        var corte = ControladorFalso.Parametros(await c.EsperarComandoAsync(0x0406, n));
        Assert.Equal((byte[])[(byte)HandleBreve, 0x00, 0x13], corte);

        // La conexión breve no se repite al cerrarse.
        n = c.Comandos.Count;
        c.Evento(0x05, Cierre(HandleBreve));
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0405);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnVinculoCifradoNoProvocaReconexion()
    {
        var (c, corrida) = await EmparejarAsync();
        c.Evento(0x08, [0x00, (byte)c.Handle, (byte)(c.Handle >> 8), 0x01]); // Encryption Change: activo
        var n = c.Comandos.Count;
        c.Evento(0x05, Cierre(c.Handle));
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0405);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SinVinculoNuevoNoHayReconexion()
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        await new AndroidSimulado(c).ConectarAclAsync();
        var n = c.Comandos.Count;
        c.Evento(0x05, Cierre(c.Handle)); // una conexión sin emparejar (p. ej. sólo SDP) que se cierra
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0405);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SiAndroidSeAdelantaLaConexionNoSeCorta()
    {
        var (c, corrida) = await EmparejarAsync();
        var n = c.Comandos.Count;
        c.Evento(0x05, Cierre(c.Handle));
        await c.EsperarComandoAsync(0x0405, n);

        // Android conecta antes de que la conexión propia se complete: es la conexión de la app y no se corta.
        n = c.Comandos.Count;
        await new AndroidSimulado(c).ConectarAclAsync();
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0406);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SiLaReconexionNoArrancaLaConexionSiguienteNoSeCorta()
    {
        var (c, corrida) = await EmparejarAsync();
        var n = c.Comandos.Count;
        c.Evento(0x05, Cierre(c.Handle));
        await c.EsperarComandoAsync(0x0405, n);
        c.Evento(0x0F, 0x0C, 0x01, 0x05, 0x04); // Command Status de Create Connection con error

        n = c.Comandos.Count;
        c.Evento(0x03, [0x00, (byte)HandleBreve, 0x00, .. AndroidSimulado.Direccion.ComoHci(), 0x01, 0x00]);
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0406);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnCorteFallidoNoProvocaReconexion()
    {
        var (c, corrida) = await EmparejarAsync();
        var n = c.Comandos.Count;
        c.Evento(0x05, [0x0C, (byte)c.Handle, (byte)(c.Handle >> 8), 0x16]); // Disconnection Complete con error: el ACL sigue
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0405);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LaTrazaHciNoMuestraLaClaveDeEnlace()
    {
        var registro = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico { TrazaHci = true }, new ReceptorEnMemoria(),
            new AlmacenDeClavesEnMemoria(), registro.Enqueue);
        var corrida = p.EjecutarAsync();
        await p.Anunciando.WaitAsync(TimeSpan.FromSeconds(5));
        await new AndroidSimulado(c).ConectarAclAsync();
        var par = AndroidSimulado.Direccion.ComoHci();
        var clave = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");
        c.Evento(0x18, [.. par, .. clave, 0x04]);
        var n = c.Comandos.Count;
        c.Evento(0x17, par); // Link Key Request: el host contesta con la clave guardada
        await c.EsperarComandoAsync(0x040B, n);
        await Task.Delay(300);
        Assert.Contains(registro, l => l.StartsWith("HCI < 18", StringComparison.Ordinal) && l.Contains("<clave>", StringComparison.Ordinal));
        Assert.Contains(registro, l => l.StartsWith("HCI > 0B04", StringComparison.Ordinal) && l.Contains("<clave>", StringComparison.Ordinal));
        Assert.DoesNotContain(registro, l => l.Contains("00112233445566778899AABBCCDDEEFF", StringComparison.Ordinal));
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ConLaOpcionApagadaNoHayReconexion()
    {
        var (c, corrida) = await EmparejarAsync(reconexion: false);
        var n = c.Comandos.Count;
        c.Evento(0x05, Cierre(c.Handle));
        await SincronizarAsync(c);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0405);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
