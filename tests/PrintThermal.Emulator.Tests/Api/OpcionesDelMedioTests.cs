// cdd: REQ-HIS-Api-De-Tickets
using Microsoft.Extensions.Configuration;
using PrintThermal.Emulator.Api.Integracion;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>PAR-PTE-07: la configuración del medio llega al periférico (reconexión breve y traza HCI).</summary>
public sealed class OpcionesDelMedioTests
{
    private static OpcionesDelMedio Leer(params (string Clave, string Valor)[] valores) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(valores.Select(v => new KeyValuePair<string, string?>(v.Clave, v.Valor)))
            .Build().GetSection("Emulador:Bluetooth").Get<OpcionesDelMedio>() ?? new OpcionesDelMedio();

    [Fact]
    public void SinConfigurarLaReconexionBreveEstaPrendidaYLaTrazaApagada()
    {
        var p = MedioBluetooth.OpcionesDelPeriferico(Leer(("Emulador:Bluetooth:Netsim", "http://127.0.0.1:1")));
        Assert.True(p.ReconexionTrasVinculoSinCifrar);
        Assert.False(p.TrazaHci);
    }

    [Fact]
    public void LasVariablesLlegan()
    {
        var p = MedioBluetooth.OpcionesDelPeriferico(Leer(
            ("Emulador:Bluetooth:Netsim", "http://127.0.0.1:1"),
            ("Emulador:Bluetooth:ReconexionTrasVinculoSinCifrar", "false"),
            ("Emulador:Bluetooth:TrazaHci", "true")));
        Assert.False(p.ReconexionTrasVinculoSinCifrar);
        Assert.True(p.TrazaHci);
    }
}
