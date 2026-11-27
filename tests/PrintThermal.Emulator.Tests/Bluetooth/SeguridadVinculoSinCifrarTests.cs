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

    private static async Task<(ControladorFalso C, Task Corrida)> EmparejarAsync()
    {
        var c = new ControladorFalso();
        var p = new PerifericoBluetooth(c, new OpcionesPeriferico(), new ReceptorEnMemoria(), new AlmacenDeClavesEnMemoria());
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
        await Task.Delay(300);
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
        await Task.Delay(300);
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
        await Task.Delay(300);
        Assert.DoesNotContain(c.Comandos.Skip(n), k => ControladorFalso.Opcode(k) == 0x0405);
        c.Cortar();
        await corrida.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
