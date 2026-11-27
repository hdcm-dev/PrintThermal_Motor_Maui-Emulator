// cdd: REQ-HIS-Documentacion-De-Uso
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PrintThermal.Emulator.Api.Api;
using PrintThermal.Emulator.Api.Integracion;
using PrintThermal.Emulator.Tests.Api;
using PrintThermal.Emulator.Tests.EscPos;

namespace PrintThermal.Emulator.Tests.Documentacion;

/// <summary>
/// La documentación de uso de <c>docs/</c> (PAR-PTE-06). REQ-01: los ejemplos marcados como ejecutables corren con
/// bash y curl contra el host real (Kestrel, fuente simulada) y el ejemplo del cliente tipado corre tal como está
/// escrito. REQ-02: rutas, parámetros, claves de configuración, imagen, puerto, mensajes de registro y ejemplos de
/// código citados en <c>docs/</c> existen en el código; si el código cambia y la documentación no, falla.
/// </summary>
public sealed partial class DocumentacionTests
{
    /// <summary>Un bloque de código de la documentación, con la marca <c>&lt;!-- ejemplo: … --&gt;</c> que lo precede.</summary>
    private sealed record Bloque(string Archivo, int Linea, string Lenguaje, string? Marca, string Texto)
    {
        public override string ToString() => $"{Archivo}:{Linea}";
    }

    private static string Docs => Path.Combine(Dorados.Raiz(), "docs");

    private static IEnumerable<string> ArchivosDeDocs() =>
        Directory.EnumerateFiles(Docs, "*.md", SearchOption.AllDirectories).Order(StringComparer.Ordinal);

    private static string Relativa(string ruta) => Path.GetRelativePath(Dorados.Raiz(), ruta).Replace('\\', '/');

    [GeneratedRegex(@"^<!--\s*ejemplo:\s*(?<marca>.+?)\s*-->\s*$")]
    private static partial Regex MarcaDeEjemplo();

    private static List<Bloque> Bloques()
    {
        var bloques = new List<Bloque>();
        foreach (var archivo in ArchivosDeDocs())
        {
            var lineas = File.ReadAllLines(archivo);
            string? marca = null;
            for (int i = 0; i < lineas.Length; i++)
            {
                var l = lineas[i];
                if (MarcaDeEjemplo().Match(l.Trim()) is { Success: true } m)
                {
                    marca = m.Groups["marca"].Value;
                    continue;
                }

                if (l.StartsWith("```", StringComparison.Ordinal))
                {
                    var lenguaje = l[3..].Trim();
                    int inicio = i + 1;
                    int fin = inicio;
                    while (fin < lineas.Length && !lineas[fin].StartsWith("```", StringComparison.Ordinal))
                    {
                        fin++;
                    }

                    bloques.Add(new Bloque(Relativa(archivo), i + 1, lenguaje, marca,
                        string.Join('\n', lineas[inicio..Math.Min(fin, lineas.Length)])));
                    i = fin;
                    marca = null;
                    continue;
                }

                if (l.Trim().Length > 0)
                {
                    marca = null;
                }
            }
        }

        return bloques;
    }

    // ---------------------------------------------------------------- REQ-01

    [Fact]
    public async Task LosEjemplosDeLaDocumentacionCorren()
    {
        Assert.True(Directory.Exists(Docs), "falta docs/");
        var ejecutables = Bloques().Where(b => b.Marca == "ejecutable").ToList();
        Assert.True(ejecutables.Count >= 3, $"docs/ tiene {ejecutables.Count} ejemplos ejecutables; se esperan al menos 3");
        Assert.All(ejecutables, b => Assert.True(b.Lenguaje == "bash", $"{b}: un ejemplo ejecutable es bash"));
        Assert.True(await HerramientaAsync("curl"), "hace falta curl para correr los ejemplos de docs/");

        await using var fabrica = new FabricaEnKestrel();
        fabrica.UseKestrel();
        fabrica.StartServer();
        var baseUrl = fabrica.Base;

        var carpeta = Directory.CreateTempSubdirectory("docs-ejemplos-");
        try
        {
            foreach (var bloque in ejecutables)
            {
                var (codigo, salida) = await CorrerBashAsync(bloque.Texto, baseUrl, carpeta.FullName);
                Assert.True(codigo == 0, $"{bloque}: el ejemplo salió con {codigo}\n{salida}");
            }
        }
        finally
        {
            carpeta.Delete(recursive: true);
        }

        // El ejemplo del cliente tipado, tal como está en docs/api.md (la igualdad del texto la mide REQ-02).
        var png = await EjemplosDelCliente.ImprimirYObtenerPngAsync(new Uri(baseUrl + "/"), () => Task.CompletedTask);
        Assert.True(png.Length > 8, "el ejemplo no devolvió un PNG");
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, png[..4]);
    }

    /// <summary>El host del emulador escuchando de verdad en un puerto libre del loopback, con la fuente simulada.</summary>
    private sealed class FabricaEnKestrel : WebApplicationFactory<Program>
    {
        /// <summary>Dirección base sin barra final, como <c>$BASE</c> en la documentación.</summary>
        public string Base =>
            Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
                .First().Replace("://[::]", "://127.0.0.1", StringComparison.Ordinal).TrimEnd('/');

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("urls", "http://127.0.0.1:0");
            builder.UseSetting("Emulador:Simulacion:IntervaloSegundos", "1");
        }
    }

    private static async Task<bool> HerramientaAsync(string nombre)
    {
        var (codigo, _) = await CorrerAsync("bash", ["-c", "command -v " + nombre], null, Path.GetTempPath(), TimeSpan.FromSeconds(10));
        return codigo == 0;
    }

    private static async Task<(int Codigo, string Salida)> CorrerBashAsync(string guion, string baseUrl, string carpeta)
    {
        var archivo = Path.Combine(carpeta, "ejemplo.sh");
        await File.WriteAllTextAsync(archivo, "set -euo pipefail\n" + guion + "\n");
        return await CorrerAsync("bash", [archivo], new() { ["BASE"] = baseUrl }, carpeta, TimeSpan.FromSeconds(120));
    }

    private static async Task<(int Codigo, string Salida)> CorrerAsync(
        string programa, string[] argumentos, Dictionary<string, string>? entorno, string carpeta, TimeSpan plazo)
    {
        var inicio = new ProcessStartInfo(programa)
        {
            WorkingDirectory = carpeta,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var a in argumentos)
        {
            inicio.ArgumentList.Add(a);
        }

        if (entorno is not null)
        {
            foreach (var par in entorno)
            {
                inicio.Environment[par.Key] = par.Value;
            }
        }

        using var proceso = Process.Start(inicio)!;
        var salida = proceso.StandardOutput.ReadToEndAsync();
        var errores = proceso.StandardError.ReadToEndAsync();
        using var cancelar = new CancellationTokenSource(plazo);
        try
        {
            await proceso.WaitForExitAsync(cancelar.Token);
        }
        catch (OperationCanceledException)
        {
            proceso.Kill(entireProcessTree: true);
            return (-1, $"venció el plazo de {plazo.TotalSeconds} s\n{await salida}\n{await errores}");
        }

        return (proceso.ExitCode, await salida + "\n" + await errores);
    }

    // ---------------------------------------------------------------- REQ-02

    [GeneratedRegex(@"(?:\$\{BASE\}|\$BASE|http://127\.0\.0\.1:(?<puerto>\d+))(?<ruta>/[^\s""'`)\\]*)")]
    private static partial Regex UrlEnBloque();

    [GeneratedRegex(@"`(?:GET |DELETE )?(?<ruta>/(?:api|openapi|scalar)[^`\s]*)`")]
    private static partial Regex RutaEnLinea();

    [GeneratedRegex(@"\$\{?[A-Z_][A-Z0-9_]*\}?|<[^>]+>")]
    private static partial Regex Comodin();

    [GeneratedRegex(@"(?<![A-Za-z0-9_])(?<clave>[A-Za-z][A-Za-z0-9]*(?:__[A-Za-z][A-Za-z0-9]*)+)=")]
    private static partial Regex VariableDeEntorno();

    [GeneratedRegex(@"ghcr\.io/[a-z0-9_./-]+(?::(?<etiqueta>[^\s""'`]+))?")]
    private static partial Regex ImagenEnDocs();

    [GeneratedRegex(@"-t\s+(?<imagen>ghcr\.io/[a-z0-9_./-]+):")]
    private static partial Regex ImagenDelRelease();

    [GeneratedRegex(@"Configuration\[""(?<clave>[^""]+)""\]|GetSection\(""(?<clave>[^""]+)""\)")]
    private static partial Regex ClaveEnProgram();

    [GeneratedRegex(@"\$""(?<texto>(?:[^""\\]|\\.)*)""")]
    private static partial Regex CadenaInterpolada();

    [GeneratedRegex(@"\]\((?<destino>[^)\s#]*)(?:#(?<ancla>[^)\s]*))?\)")]
    private static partial Regex Enlace();

    [Fact]
    public async Task LaDocumentacionSigueAlCodigo()
    {
        var bloques = Bloques();
        Assert.NotEmpty(bloques);
        var errores = new List<string>();

        await RutasYParametrosAsync(bloques, errores);
        ClavesDeConfiguracion(bloques, errores);
        ImagenYPuerto(bloques, errores);
        MensajesDeRegistro(bloques, errores);
        EjemplosDeCodigo(bloques, errores);
        Enlaces(errores);

        Assert.True(errores.Count == 0, "docs/ no sigue al código:\n- " + string.Join("\n- ", errores));
    }

    private static async Task RutasYParametrosAsync(List<Bloque> bloques, List<string> errores)
    {
        await using var fabrica = new FabricaDelEmulador();
        using var http = fabrica.CreateClient();
        using var openApi = JsonDocument.Parse(await http.GetStringAsync("/openapi/v1.json"));
        var rutas = openApi.RootElement.GetProperty("paths");
        CamposJson(openApi.RootElement, errores);
        var fueraDeLaApi = new HashSet<string>(StringComparer.Ordinal) { "/", "/scalar", "/openapi/v1.json" };

        var citas = new List<(string Donde, string Metodo, string Url)>();
        foreach (var b in bloques)
        {
            // Une las líneas continuadas con «\» para leer cada comando entero.
            foreach (var comando in b.Texto.Replace("\\\n", " ", StringComparison.Ordinal).Split('\n'))
            {
                var metodo = Regex.IsMatch(comando, @"-X\s*DELETE") ? "delete" : "get";
                foreach (Match m in UrlEnBloque().Matches(comando))
                {
                    citas.Add((b.ToString(), metodo, m.Groups["ruta"].Value));
                }
            }
        }

        foreach (var archivo in ArchivosDeDocs())
        {
            var lineas = File.ReadAllLines(archivo);
            bool enBloque = false;
            for (int i = 0; i < lineas.Length; i++)
            {
                if (lineas[i].StartsWith("```", StringComparison.Ordinal))
                {
                    enBloque = !enBloque;
                    continue;
                }

                if (!enBloque)
                {
                    foreach (Match m in RutaEnLinea().Matches(lineas[i]))
                    {
                        var metodo = m.Value.StartsWith("`DELETE", StringComparison.Ordinal) ? "delete" : "get";
                        citas.Add(($"{Relativa(archivo)}:{i + 1}", metodo, m.Groups["ruta"].Value));
                    }
                }
            }
        }

        Assert.True(citas.Count >= 5, $"docs/ cita {citas.Count} rutas; se esperan más");
        foreach (var (donde, metodo, url) in citas)
        {
            var partes = url.Split('?', 2);
            var ruta = partes[0].TrimEnd('.', ',', ';', ':');
            bool conComodin = Comodin().IsMatch(ruta);
            var concreta = Comodin().Replace(ruta, "X");

            if (fueraDeLaApi.Contains(concreta))
            {
                continue;
            }

            JsonElement? operacion = null;
            foreach (var p in rutas.EnumerateObject())
            {
                bool coincide = conComodin
                    ? Regex.IsMatch(concreta, "^" + Regex.Replace(Regex.Escape(p.Name), @"\\\{[^}]+}", "[^/]+") + "$")
                    : p.Name == concreta;
                if (coincide && p.Value.TryGetProperty(metodo, out var op))
                {
                    operacion = op;
                    break;
                }
            }

            if (operacion is null)
            {
                errores.Add($"{donde}: {metodo.ToUpperInvariant()} {ruta} no está en /openapi/v1.json");
                continue;
            }

            if (partes.Length == 2)
            {
                var admitidos = new HashSet<string>(StringComparer.Ordinal);
                if (operacion.Value.TryGetProperty("parameters", out var ps))
                {
                    foreach (var x in ps.EnumerateArray())
                    {
                        admitidos.Add(x.GetProperty("name").GetString() ?? "");
                    }
                }

                foreach (var par in partes[1].Split('&'))
                {
                    var nombre = par.Split('=')[0];
                    if (!admitidos.Contains(nombre))
                    {
                        errores.Add($"{donde}: el parámetro «{nombre}» no existe en {ruta}");
                    }
                }
            }
        }
    }

    private static void ClavesDeConfiguracion(List<Bloque> bloques, List<string> errores)
    {
        var program = File.ReadAllText(Path.Combine(Dorados.Raiz(), "src", "PrintThermal.Emulator.Api", "Program.cs"));
        var conocidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in ClaveEnProgram().Matches(program))
        {
            conocidas.Add(m.Groups["clave"].Value);
        }

        // Las secciones se enlazan a sus opciones: cada propiedad pública es una clave.
        foreach (var (seccion, tipo) in new[] { ("Emulador:Bluetooth", typeof(OpcionesDelMedio)), ("Emulador:Simulacion", typeof(OpcionesDeSimulacion)) })
        {
            Assert.Contains(seccion, conocidas);
            foreach (var p in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                conocidas.Add(seccion + ":" + p.Name);
            }
        }

        int vistas = 0;
        foreach (var b in bloques)
        {
            foreach (Match m in VariableDeEntorno().Matches(b.Texto))
            {
                vistas++;
                var clave = m.Groups["clave"].Value.Replace("__", ":", StringComparison.Ordinal);
                if (!conocidas.Contains(clave))
                {
                    errores.Add($"{b}: la variable {m.Groups["clave"].Value} no corresponde a ninguna clave de configuración del host");
                }
            }
        }

        // También las que se nombran en la prosa y las tablas, entre comillas invertidas.
        foreach (var archivo in ArchivosDeDocs())
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(archivo), @"`(?<clave>[A-Za-z][A-Za-z0-9]*(?:__[A-Za-z][A-Za-z0-9]*)+)`"))
            {
                vistas++;
                var clave = m.Groups["clave"].Value.Replace("__", ":", StringComparison.Ordinal);
                if (!conocidas.Contains(clave))
                {
                    errores.Add($"{Relativa(archivo)}: la variable {m.Groups["clave"].Value} no corresponde a ninguna clave de configuración del host");
                }
            }
        }

        if (vistas == 0)
        {
            errores.Add("docs/ no muestra ninguna variable de configuración");
        }

        // Valores por omisión y rangos citados: salen del código.
        var montaje = File.ReadAllText(Path.Combine(Docs, "montaje.md"));
        var api = File.ReadAllText(Path.Combine(Docs, "api.md"));
        var porOmision = new OpcionesDelMedio().Direccion;
        if (!montaje.Contains($"| `{porOmision}` |", StringComparison.Ordinal))
        {
            errores.Add($"docs/montaje.md no da la dirección Bluetooth por omisión del código ({porOmision})");
        }

        foreach (var texto in new[]
        {
            $"de 1 a {EndpointsDeTickets.EsperaMaxima}; {EndpointsDeTickets.EsperaPorOmision} si falta",
            $"`limite` va de 1 a {EndpointsDeTickets.LimiteMaximo}",
        })
        {
            if (!api.Contains(texto, StringComparison.Ordinal))
            {
                errores.Add($"docs/api.md no dice «{texto}», que es lo que hace el código");
            }
        }
    }

    /// <summary>
    /// Los campos JSON que <c>docs/api.md</c> nombra entre comillas invertidas existen en los esquemas o en los
    /// parámetros de <c>/openapi/v1.json</c>.
    /// </summary>
    private static void CamposJson(JsonElement openApi, List<string> errores)
    {
        var nombres = new HashSet<string>(StringComparer.Ordinal) { "null", "true", "false", "jq", "detail" };
        if (openApi.TryGetProperty("components", out var componentes) && componentes.TryGetProperty("schemas", out var esquemas))
        {
            foreach (var e in esquemas.EnumerateObject())
            {
                if (e.Value.TryGetProperty("properties", out var props))
                {
                    foreach (var p in props.EnumerateObject())
                    {
                        nombres.Add(p.Name);
                    }
                }
            }
        }

        foreach (var ruta in openApi.GetProperty("paths").EnumerateObject())
        {
            foreach (var op in ruta.Value.EnumerateObject())
            {
                if (op.Value.ValueKind == JsonValueKind.Object && op.Value.TryGetProperty("parameters", out var ps))
                {
                    foreach (var x in ps.EnumerateArray())
                    {
                        nombres.Add(x.GetProperty("name").GetString() ?? "");
                    }
                }
            }
        }

        var lineas = File.ReadAllLines(Path.Combine(Docs, "api.md"));
        bool enBloque = false;
        for (int i = 0; i < lineas.Length; i++)
        {
            if (lineas[i].StartsWith("```", StringComparison.Ordinal))
            {
                enBloque = !enBloque;
                continue;
            }

            if (enBloque)
            {
                continue;
            }

            foreach (Match m in Regex.Matches(lineas[i], @"`(?<campo>[a-z][a-zA-Z]*)`"))
            {
                if (!nombres.Contains(m.Groups["campo"].Value))
                {
                    errores.Add($"docs/api.md:{i + 1}: «{m.Groups["campo"].Value}» no es un campo ni un parámetro de /openapi/v1.json");
                }
            }
        }
    }

    private static void ImagenYPuerto(List<Bloque> bloques, List<string> errores)
    {
        var raiz = Dorados.Raiz();
        var release = File.ReadAllText(Path.Combine(raiz, ".github", "workflows", "release.yml"));
        var imagen = ImagenDelRelease().Match(release).Groups["imagen"].Value;
        Assert.False(string.IsNullOrEmpty(imagen), "release.yml no nombra la imagen");
        var dockerfile = File.ReadAllText(Path.Combine(raiz, "Dockerfile"));
        var puerto = Regex.Match(dockerfile, @"(?m)^ENV ASPNETCORE_URLS=http://0\.0\.0\.0:(?<p>\d+)").Groups["p"].Value;
        Assert.False(string.IsNullOrEmpty(puerto), "el Dockerfile no fija el puerto");
        var volumen = Regex.Match(dockerfile, @"(?m)^VOLUME \[""(?<v>[^""]+)""\]").Groups["v"].Value;
        // La versión que documenta docs/ se declara una vez, en su índice; toda etiqueta citada es ésa.
        var versionDocumentada = Regex.Match(File.ReadAllText(Path.Combine(Docs, "README.md")), @"versión `(?<v>\d+\.\d+\.\d+)` de la imagen").Groups["v"].Value;
        Assert.False(string.IsNullOrEmpty(versionDocumentada), "docs/README.md no dice qué versión de la imagen documenta");

        bool montaVolumen = false;
        foreach (var b in bloques)
        {
            foreach (Match m in ImagenEnDocs().Matches(b.Texto))
            {
                var nombre = m.Value.Split(':')[0];
                var etiqueta = m.Groups["etiqueta"].Value;
                if (nombre != imagen)
                {
                    errores.Add($"{b}: la imagen {nombre} no es la que publica release.yml ({imagen})");
                }

                if (!Regex.IsMatch(etiqueta, @"^(\d+\.\d+\.\d+|<[^>]+>|\$\{?[A-Z_]+\}?)$"))
                {
                    errores.Add($"{b}: la imagen va con versión fija, no «{etiqueta}»");
                }
                else if (Regex.IsMatch(etiqueta, @"^\d") && etiqueta != versionDocumentada)
                {
                    errores.Add($"{b}: la imagen va con {etiqueta} y docs/README.md documenta la {versionDocumentada}");
                }
            }

            foreach (Match m in UrlEnBloque().Matches(b.Texto))
            {
                if (m.Groups["puerto"].Success && m.Groups["puerto"].Value != puerto)
                {
                    errores.Add($"{b}: el puerto {m.Groups["puerto"].Value} no es el de la imagen ({puerto})");
                }
            }

            foreach (Match m in Regex.Matches(b.Texto, @"-p\s+(?:[\d.]+:)?\d+:(?<interno>\d+)"))
            {
                if (m.Groups["interno"].Value != puerto)
                {
                    errores.Add($"{b}: «{m.Value}» publica el puerto {m.Groups["interno"].Value}, y la imagen escucha en {puerto}");
                }
            }

            montaVolumen |= b.Texto.Contains(":" + volumen + " ", StringComparison.Ordinal);
        }

        if (!montaVolumen)
        {
            errores.Add($"docs/ no monta la carpeta de salida de la imagen ({volumen})");
        }
    }

    private static void MensajesDeRegistro(List<Bloque> bloques, List<string> errores)
    {
        var plantillas = new List<Regex>();
        foreach (var archivo in Directory.EnumerateFiles(Path.Combine(Dorados.Raiz(), "src"), "*.cs", SearchOption.AllDirectories))
        {
            foreach (Match m in CadenaInterpolada().Matches(File.ReadAllText(archivo)))
            {
                AgregarPlantilla(plantillas, m.Groups["texto"].Value);
            }
        }

        // También las plantillas de ILogger con marcadores con nombre.
        foreach (var archivo in Directory.EnumerateFiles(Path.Combine(Dorados.Raiz(), "src"), "*.cs", SearchOption.AllDirectories))
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(archivo), @"Log(?:Information|Warning|Error)\(\s*""(?<texto>[^""]*)"""))
            {
                AgregarPlantilla(plantillas, m.Groups["texto"].Value);
            }
        }

        var registros = bloques.Where(b => b.Marca == "registro").ToList();
        if (registros.Count == 0)
        {
            errores.Add("docs/ no cita ningún mensaje de registro (marca «ejemplo: registro»)");
        }

        foreach (var b in registros)
        {
            foreach (var linea in b.Texto.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')))
            {
                if (!plantillas.Any(p => p.IsMatch(linea)))
                {
                    errores.Add($"{b}: el mensaje «{linea}» no sale de ninguna plantilla de src/");
                }
            }
        }
    }

    /// <summary>
    /// Una plantilla de mensaje como expresión: cada hueco <c>{…}</c> admite cualquier texto. Las que no tienen al
    /// menos cuatro letras propias (p. ej. <c>"{Mensaje}"</c>) no cuentan: admitirían cualquier línea.
    /// </summary>
    private static void AgregarPlantilla(List<Regex> plantillas, string texto)
    {
        var partes = Regex.Split(texto, @"\{[^{}]*\}");
        if (partes.Sum(p => p.Count(char.IsLetter)) >= 4)
        {
            plantillas.Add(new Regex("^" + string.Join(".+?", partes.Select(Regex.Escape)) + "$"));
        }
    }

    private static void EjemplosDeCodigo(List<Bloque> bloques, List<string> errores)
    {
        var deCodigo = bloques.Where(b => b.Marca is { } m && m.Contains('#', StringComparison.Ordinal)).ToList();
        if (deCodigo.Count == 0)
        {
            errores.Add("docs/ no trae el ejemplo del cliente tipado (marca «ejemplo: <archivo>#<región>»)");
        }

        foreach (var b in deCodigo)
        {
            var (ruta, region) = (b.Marca!.Split('#')[0], b.Marca.Split('#')[1]);
            var archivo = Path.Combine(Dorados.Raiz(), ruta);
            if (!File.Exists(archivo))
            {
                errores.Add($"{b}: no existe {ruta}");
                continue;
            }

            var texto = File.ReadAllText(archivo);
            var m = Regex.Match(texto, @"// <ejemplo:" + Regex.Escape(region) + @">\r?\n(?<cuerpo>.*?)\r?\n[ \t]*// </ejemplo:" + Regex.Escape(region) + ">", RegexOptions.Singleline);
            if (!m.Success)
            {
                errores.Add($"{b}: {ruta} no tiene la región «{region}»");
                continue;
            }

            if (Sangria(m.Groups["cuerpo"].Value) != Sangria(b.Texto))
            {
                errores.Add($"{b}: el ejemplo no es igual a la región «{region}» de {ruta}");
            }
        }
    }

    /// <summary>Quita la sangría común y los espacios finales, para comparar el texto del ejemplo.</summary>
    private static string Sangria(string texto)
    {
        var lineas = texto.Replace("\r", "", StringComparison.Ordinal).Split('\n').Select(l => l.TrimEnd()).ToList();
        int comun = lineas.Where(l => l.Length > 0).Select(l => l.Length - l.TrimStart().Length).DefaultIfEmpty(0).Min();
        var sb = new StringBuilder();
        foreach (var l in lineas)
        {
            sb.Append(l.Length >= comun ? l[comun..] : l).Append('\n');
        }

        return sb.ToString().Trim('\n');
    }

    private static void Enlaces(List<string> errores)
    {
        var raiz = Dorados.Raiz();
        var archivos = ArchivosDeDocs().Append(Path.Combine(raiz, "README.md"));
        foreach (var archivo in archivos)
        {
            foreach (Match m in Enlace().Matches(File.ReadAllText(archivo)))
            {
                var destino = m.Groups["destino"].Value;
                if (destino.Contains("://", StringComparison.Ordinal) || destino.StartsWith("mailto:", StringComparison.Ordinal))
                {
                    continue;
                }

                var completo = destino.Length == 0
                    ? archivo
                    : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(archivo)!, destino));
                if (!File.Exists(completo) && !Directory.Exists(completo))
                {
                    errores.Add($"{Relativa(archivo)}: enlace roto a {destino}");
                    continue;
                }

                var ancla = m.Groups["ancla"].Value;
                if (ancla.Length > 0 && File.Exists(completo) && !Anclas(completo).Contains(ancla))
                {
                    errores.Add($"{Relativa(archivo)}: el ancla #{ancla} no es un título de {Relativa(completo)}");
                }
            }
        }

        if (!File.ReadAllText(Path.Combine(raiz, "README.md")).Contains("](docs/README.md)", StringComparison.Ordinal))
        {
            errores.Add("README.md no enlaza docs/README.md");
        }

        // Las páginas de la estructura decidida (D1 de PAR-PTE-06), enlazadas desde el índice.
        var indice = File.ReadAllText(Path.Combine(Docs, "README.md"));
        foreach (var pagina in Paginas)
        {
            if (!File.Exists(Path.Combine(Docs, pagina)) || !indice.Contains("](" + pagina + ")", StringComparison.Ordinal))
            {
                errores.Add($"falta docs/{pagina} o su enlace desde docs/README.md");
            }
        }
    }

    private static readonly string[] Paginas =
    [
        "que-es-y-limites.md", "montaje.md", "emparejar-e-imprimir.md", "api.md", "visor-y-scalar.md",
        "diagnostico.md", "recetas.md", "diseno-documental.md",
    ];

    /// <summary>Anclas de los títulos de un Markdown, como las arma GitHub (minúsculas, sin puntuación, guiones).</summary>
    private static HashSet<string> Anclas(string archivo)
    {
        var anclas = new HashSet<string>(StringComparer.Ordinal);
        bool enBloque = false;
        foreach (var l in File.ReadLines(archivo))
        {
            if (l.StartsWith("```", StringComparison.Ordinal))
            {
                enBloque = !enBloque;
            }
            else if (!enBloque && Regex.Match(l, @"^#{1,6}\s+(?<t>.+?)\s*$") is { Success: true } m)
            {
                var t = m.Groups["t"].Value.ToLowerInvariant();
                anclas.Add(new string(t.Where(c => char.IsLetterOrDigit(c) || c is ' ' or '-' or '_').ToArray()).Replace(' ', '-'));
            }
        }

        return anclas;
    }
}
