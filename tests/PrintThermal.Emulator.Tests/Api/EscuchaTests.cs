using System.Net;
using Microsoft.Extensions.Configuration;
using PrintThermal.Emulator.Api.Api;

namespace PrintThermal.Emulator.Tests.Api;

/// <summary>Dónde escucha el host y a dónde llama el visor (PAR-PTE-04 REQ-06; ADR-0001).</summary>
public sealed class EscuchaTests
{
    private static IConfiguration Configuracion(params (string Clave, string Valor)[] valores) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(valores.Select(v => new KeyValuePair<string, string?>(v.Clave, v.Valor)))
            .Build();

    [Fact]
    public void SinConfiguracionEscuchaSoloEnLocal()
    {
        var escucha = Direcciones.EscuchaSinConfigurar(Configuracion());

        Assert.NotNull(escucha);
        Assert.True(IPAddress.IsLoopback(IPAddress.Parse(new Uri(escucha).Host)), $"«{escucha}» no es local");
    }

    [Theory]
    [InlineData("urls", "http://0.0.0.0:8080")]
    [InlineData("http_ports", "8080")]
    [InlineData("https_ports", "8443")]
    [InlineData("Kestrel:Endpoints:Http:Url", "http://0.0.0.0:8080")]
    public void ConDireccionConfiguradaLaRespeta(string clave, string valor)
    {
        Assert.Null(Direcciones.EscuchaSinConfigurar(Configuracion((clave, valor))));
    }

    [Theory]
    [InlineData(null, "http://[::]:8080", "http://localhost:8080/")]
    [InlineData(null, "http://0.0.0.0:8080", "http://localhost:8080/")]
    [InlineData(null, "http://+:8080", "http://localhost:8080/")]
    [InlineData(null, "http://127.0.0.1:5080", "http://127.0.0.1:5080/")]
    [InlineData("http://emulador:9000", "http://[::]:8080", "http://emulador:9000/")]
    public void ElVisorLlamaALaApiDelMismoServicio(string? urlApi, string direccion, string esperada)
    {
        Assert.Equal(new Uri(esperada), Direcciones.BaseDelVisor(urlApi, [direccion]));
    }
}
