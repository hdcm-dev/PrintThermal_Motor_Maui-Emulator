// cdd: REQ-HIS-Imagen-Publicada
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Tests.EscPos;

namespace PrintThermal.Emulator.Tests.Distribucion;

/// <summary>
/// El workflow de release y el Dockerfile (PAR-PTE-05 REQ-01 y REQ-04). Se leen como texto: la corrida real del
/// release queda en la evidencia de la parcela; acá se fija lo que no puede cambiar sin que una prueba lo note.
/// </summary>
public sealed partial class ReleaseTests
{
    private const string Imagen = "ghcr.io/hdcm-dev/printthermal_motor_maui-emulator";

    private static string Leer(params string[] partes) => File.ReadAllText(Path.Combine([Dorados.Raiz(), .. partes]));

    [GeneratedRegex(@"^\s*-?\s*uses:\s*(?<accion>[^@\s]+)@(?<ref>\S+)", RegexOptions.Multiline)]
    private static partial Regex Usos();

    [Fact]
    public void ElWorkflowDeReleasePublicaEnGhcr()
    {
        var yml = Leer(".github", "workflows", "release.yml");

        // Se dispara sólo al empujar una etiqueta v*, en un runner de GitHub.
        Assert.Matches(@"(?ms)^on:\s*\n\s+push:\s*\n\s+tags:\s*\n\s+- ['""]?v\*['""]?", yml);
        Assert.DoesNotMatch(@"(?m)^\s+pull_request:", yml);
        Assert.Contains("runs-on: ubuntu-latest", yml);

        // Permisos mínimos: escribir paquetes (ghcr.io) y el release; el token es el del workflow.
        Assert.Matches(@"(?m)^\s+packages: write\s*$", yml);
        Assert.Matches(@"(?m)^\s+contents: write\s*$", yml);
        Assert.Contains("secrets.GITHUB_TOKEN", yml);
        Assert.DoesNotMatch(@"secrets\.(?!GITHUB_TOKEN)", yml);

        // Compila y prueba antes de publicar; la imagen lleva la versión de la etiqueta.
        int prueba = yml.IndexOf("dotnet test", StringComparison.Ordinal);
        int push = yml.IndexOf("docker push", StringComparison.Ordinal);
        int release = yml.IndexOf("gh release create", StringComparison.Ordinal);
        Assert.True(prueba > 0, "no prueba");
        Assert.True(push > prueba, "publica la imagen antes de probar");
        Assert.True(release > prueba, "crea el release antes de probar");
        Assert.Contains(Imagen + ":${VERSION}", yml);
        Assert.Contains("docker login ghcr.io", yml);

        // Una etiqueta repetida falla antes de publicar nada.
        int repetida = yml.IndexOf("gh release view", StringComparison.Ordinal);
        Assert.True(repetida > 0 && repetida < push, "no comprueba la etiqueta repetida antes de publicar");

        // El binario va adjunto al release.
        Assert.Matches(@"gh release create[^\n]*\.tar\.gz", yml);

        // Acciones fijadas por SHA de commit.
        var usos = Usos().Matches(yml);
        Assert.NotEmpty(usos);
        Assert.All(usos, u => Assert.Matches("^[0-9a-f]{40}$", u.Groups["ref"].Value));
    }

    [Fact]
    public void LaImagenFijaEscuchaYSalida()
    {
        var docker = Leer("Dockerfile");

        // Base aspnet de versión fija, por etiqueta exacta y digest; el SDK de la compilación es el de global.json.
        Assert.Matches(@"(?m)^FROM mcr\.microsoft\.com/dotnet/aspnet:10\.0\.\d+-noble-chiseled@sha256:[0-9a-f]{64}\s*$", docker);
        var sdk = Regex.Match(Leer("global.json"), @"""version"":\s*""(?<v>[^""]+)""").Groups["v"].Value;
        Assert.Matches(@$"(?m)^FROM mcr\.microsoft\.com/dotnet/sdk:{Regex.Escape(sdk)}-noble@sha256:[0-9a-f]{{64}} AS ", docker);

        // Escucha explícita en su red de contenedor (LAZ-0008) y carpeta de salida montable.
        Assert.Matches(@"(?m)^ENV ASPNETCORE_HTTP_PORTS=\s*$", docker);
        Assert.Matches(@"(?m)^ENV ASPNETCORE_URLS=http://0\.0\.0\.0:5080\s*$", docker);
        Assert.Matches(@"(?m)^ENV Emulador__Salida=/data\s*$", docker);
        Assert.Matches(@"(?m)^VOLUME \[?""?/data""?\]?\s*$", docker);
        Assert.Matches(@"(?m)^EXPOSE 5080\s*$", docker);
        Assert.Matches(@"(?m)^USER app\s*$", docker);
        Assert.Contains("org.opencontainers.image.source=https://github.com/hdcm-dev/PrintThermal_Motor_Maui-Emulator", docker);

        // Con esa configuración el host respeta la dirección de la imagen (no impone 127.0.0.1).
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["urls"] = "http://0.0.0.0:5080",
            ["http_ports"] = "",
        }).Build();
        Assert.Null(Direcciones.EscuchaSinConfigurar(config));
    }
}
