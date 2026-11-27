// cdd: REQ-HIS-Compilacion-Verificada
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PrintThermal.Emulator.Tests;

/// <summary>Pruebas del esqueleto de la solución (PAR-PTE-01): leen los archivos de la solución desde la raíz del repositorio.</summary>
public class EsqueletoTests
{
    private const string Solucion = "PrintThermal.Emulator.slnx";

    private static readonly string[] Proyectos =
    [
        "PrintThermal.Emulator.Core",
        "PrintThermal.Emulator.Bluetooth",
        "PrintThermal.Emulator.Api",
        "PrintThermal.Emulator.Client",
        "PrintThermal.Emulator.Tests",
    ];

    private static string Raiz()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, Solucion)))
        {
            dir = dir.Parent;
        }
        Assert.True(dir is not null, $"no se encontró {Solucion} subiendo desde {AppContext.BaseDirectory}");
        return dir!.FullName;
    }

    private static IEnumerable<string> ArchivosAnidados(string raiz, params string[] nombres) =>
        new[] { "src", "tests" }
            .Select(d => Path.Combine(raiz, d))
            .Where(Directory.Exists)
            .SelectMany(d => Directory.EnumerateFiles(d, "*", SearchOption.AllDirectories))
            .Where(f => nombres.Contains(Path.GetFileName(f)))
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(p => p is "bin" or "obj"));

    private static IEnumerable<string> RutasDeProyectos(string raiz) =>
        XDocument.Load(Path.Combine(raiz, Solucion))
            .Descendants("Project")
            .Select(p => (string?)p.Attribute("Path"))
            .Where(p => p is not null)
            .Select(p => Path.Combine(raiz, p!.Replace('\\', '/')));

    // REQ-01
    [Fact]
    public void LaSolucionDeclaraLosCincoProyectosEnNet10()
    {
        var raiz = Raiz();
        var rutas = RutasDeProyectos(raiz).ToList();
        var nombres = rutas.Select(Path.GetFileNameWithoutExtension).Order().ToArray();
        Assert.Equal(Proyectos.Order().ToArray(), nombres);
        foreach (var ruta in rutas)
        {
            Assert.True(File.Exists(ruta), $"falta {ruta}");
        }

        // Un Directory.Build.props/.targets anidado podría cambiar el destino sin que la solución lo muestre.
        Assert.Empty(ArchivosAnidados(raiz, "Directory.Build.props", "Directory.Build.targets"));
        var props = XDocument.Load(Path.Combine(raiz, "Directory.Build.props"));
        Assert.Equal("net10.0", props.Descendants("TargetFramework").Single().Value);
        foreach (var ruta in rutas)
        {
            var csproj = XDocument.Load(ruta);
            Assert.DoesNotContain(csproj.Descendants("TargetFramework"), e => e.Value != "net10.0");
            Assert.Empty(csproj.Descendants("TargetFrameworks"));
        }
    }

    // REQ-02
    [Fact]
    public void CadaPaqueteTieneVersionFijaYCentral()
    {
        var raiz = Raiz();
        var central = XDocument.Load(Path.Combine(raiz, "Directory.Packages.props"));
        Assert.Equal("true", central.Descendants("ManagePackageVersionsCentrally").Single().Value);
        var versiones = central.Descendants("PackageVersion").ToList();
        Assert.NotEmpty(versiones);
        var exacta = new Regex(@"^\d+\.\d+\.\d+$");
        foreach (var v in versiones)
        {
            var version = (string?)v.Attribute("Version") ?? "";
            Assert.True(exacta.IsMatch(version), $"{v.Attribute("Include")?.Value}: «{version}» no es una versión fija");
        }

        Assert.Empty(ArchivosAnidados(raiz, "Directory.Packages.props", "Directory.Build.props", "Directory.Build.targets"));
        Assert.Empty(XDocument.Load(Path.Combine(raiz, "Directory.Build.props")).Descendants("PackageReference"));
        var declaradas = versiones.Select(v => (string)v.Attribute("Include")!).ToHashSet();
        foreach (var ruta in RutasDeProyectos(raiz))
        {
            foreach (var r in XDocument.Load(ruta).Descendants("PackageReference"))
            {
                var nombre = (string?)r.Attribute("Include") ?? "";
                Assert.Null(r.Attribute("Version"));
                Assert.Null(r.Attribute("VersionOverride"));
                Assert.Contains(nombre, declaradas);
            }
        }
    }

    // REQ-03
    [Fact]
    public void ElSdkEstaFijado()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(Raiz(), "global.json")));
        var sdk = json.RootElement.GetProperty("sdk");
        Assert.Matches(@"^10\.0\.\d+$", sdk.GetProperty("version").GetString());
        Assert.Equal("disable", sdk.GetProperty("rollForward").GetString());
    }

    // REQ-04
    [Fact]
    public void LaCiCompilaYPruebaSinDescargarLaNorma()
    {
        var lineas = File.ReadAllLines(Path.Combine(Raiz(), ".github", "workflows", "cdd.yml"))
            .Select(l => l.Trim())
            .Where(l => !l.StartsWith('#'))
            .ToList();
        var texto = string.Join('\n', lineas);

        Assert.Contains("pull_request:", lineas);
        Assert.Contains("push:", lineas);
        Assert.Contains("branches: [main]", lineas);
        Assert.True(lineas.IndexOf("push:") == lineas.IndexOf("branches: [main]") - 1, "branches: [main] va debajo de push:");
        Assert.DoesNotContain(lineas, l => Regex.IsMatch(l, @"^continue-on-error:\s*true"));
        Assert.DoesNotContain(lineas, l => Regex.IsMatch(l, @"^(- )?if:\s*(false|\$\{\{\s*false\s*\}\})"));
        Assert.Contains("runs-on: ubuntu-latest", lineas);
        Assert.Contains(lineas, l => Regex.IsMatch(l, @"^(- )?run: dotnet build PrintThermal\.Emulator\.slnx\b"));
        Assert.Contains(lineas, l => Regex.IsMatch(l, @"^(- )?run: dotnet test PrintThermal\.Emulator\.slnx\b"));
        Assert.DoesNotContain("cdd.py", texto);
        Assert.DoesNotContain("CDD_REPO_URL", texto);
        Assert.DoesNotContain("CDD_READ_TOKEN", texto);
        Assert.DoesNotContain("git clone", texto);
    }
}
