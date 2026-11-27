---
id: PAR-PTE-01
titulo: Esqueleto de la solución PrintThermal.Emulator: cinco proyectos que compilan, versiones fijas y CI .NET
version: 1.0
estado: lista
velocidad: rapida
base_commit: 07d938ce85db406f4d06fd0fe1b2706a6ed07854
sprint: SPR-01
epica: REQ-EPI-Distribucion@v1
historias:
  - REQ-HIS-Compilacion-Verificada@v1
lee:
  - VIS@v1
  - REQ-EPI-Distribucion@v1
  - ADR-0006@v1
modifica:
  - REQ-HIS-Compilacion-Verificada@v1
appetite: medio día
pendientes:
decisiones:
  - {id: D1, tema: "SDK .NET 10.0.403 fijado en global.json (el del host y una etiqueta de mcr.microsoft.com/dotnet/sdk)", clase: K1}
  - {id: D2, tema: "xUnit 2.9.3 con xunit.runner.visualstudio 3.1.5 y Microsoft.NET.Test.Sdk 18.12.0, versiones de nuget.org del 2026-11-27", clase: K1}
  - {id: D3, tema: "el proyecto Api usa el SDK web como biblioteca hasta PAR-PTE-04, que le da su Program.cs", clase: K1}
  - {id: D4, tema: "acciones de GitHub fijadas por SHA de commit de su etiqueta", clase: K1}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Una solución .NET 10 vacía y verificable, con la que las parcelas siguientes empiezan a escribir código y que la CI pública compila y prueba en cada PR (REQ-HIS-Compilacion-Verificada).

## Escenarios

- Camino feliz: `dotnet test PrintThermal.Emulator.slnx` compila los cinco proyectos y corre las pruebas del esqueleto en verde, en local (contenedor con turno) y en `ubuntu-latest`.
- No feliz: un proyecto declara la versión de un paquete; la prueba de versiones centrales falla.

## Requisitos EARS

- REQ-01 · El sistema deberá declarar en `PrintThermal.Emulator.slnx` los proyectos Core, Bluetooth, Api, Client y Tests, todos con destino `net10.0`.
  prueba: tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · LaSolucionDeclaraLosCincoProyectosEnNet10
- REQ-02 · El sistema deberá fijar la versión exacta de cada paquete NuGet en `Directory.Packages.props`, sin versiones en los proyectos.
  prueba: tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · CadaPaqueteTieneVersionFijaYCentral
- REQ-03 · El sistema deberá fijar en `global.json` la versión del SDK de .NET, sin avance a otra versión.
  prueba: tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · ElSdkEstaFijado
- REQ-04 · Cuando se abre un PR o se empuja a `main`, el sistema deberá compilar la solución y correr sus pruebas en `ubuntu-latest` sin descargar la norma CDD.
  prueba: tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · LaCiCompilaYPruebaSinDescargarLaNorma

## Casos borde

- Un rango de versiones (`[1.0,)`) o un comodín (`1.*`) en `Directory.Packages.props` no es una versión fija.

## Fuera de alcance

- Código de dominio (intérprete, Bluetooth, API, visor, cliente): PAR-PTE-02 a PAR-PTE-04.
- Contenedor, release e imagen: PAR-PTE-05.
- Compuertas de CDD en la CI (DEC-006).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · LaSolucionDeclaraLosCincoProyectosEnNet10
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · CadaPaqueteTieneVersionFijaYCentral
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · ElSdkEstaFijado
- Criterio REQ-04 → tests/PrintThermal.Emulator.Tests/EsqueletoTests.cs · LaCiCompilaYPruebaSinDescargarLaNorma

## Plan

Proyectos en `src/` y `tests/`; `Directory.Build.props` con `net10.0`, `Nullable` e `ImplicitUsings`; `Directory.Packages.props` con gestión central; `global.json`; `cdd.yml` pasa de manual a compilar y probar. Pruebas que leen los archivos de la solución desde la raíz del repositorio.

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
