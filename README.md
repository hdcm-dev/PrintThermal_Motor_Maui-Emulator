# PrintThermal_Motor_Maui-Emulator

Emulador de una impresora térmica Bluetooth ESC/POS de 58 mm (58HB6) para pruebas locales, en contenedor y de extremo a extremo con el emulador de Android. Entrega cada ticket como imagen PNG por una API HTTP (OpenAPI y Scalar) y un visor web. Tecnología: .NET 10; solución `PrintThermal.Emulator`.

**Estado:** visión firmada (`cdd/corpus/vision.md`); sprint 1 en curso (`cdd/sprints/SPR-01.md`). Todavía no hay release.

## Proyectos

| Proyecto | Qué hace |
|---|---|
| `PrintThermal.Emulator.Core` | Almacén de tickets e intérprete ESC/POS de 58 mm (384 puntos) a PNG |
| `PrintThermal.Emulator.Bluetooth` | Pila Bluetooth Classic propia en C# contra el controlador virtual del emulador de Android (netsim) |
| `PrintThermal.Emulator.Api` | Host ASP.NET Core: API de tickets, OpenAPI, Scalar y visor Blazor |
| `PrintThermal.Emulator.Client` | Cliente HTTP tipado de la API |
| `PrintThermal.Emulator.Tests` | Pruebas (xUnit) |

## Compilar y probar

```bash
dotnet build PrintThermal.Emulator.slnx
dotnet test PrintThermal.Emulator.slnx
```

El SDK está fijado en `global.json` y las versiones de los paquetes en `Directory.Packages.props`.

## Gobierno

El producto se gobierna con la norma CDD (repositorio privado). Sus compuertas se corren en local; la CI pública (`.github/workflows/cdd.yml`) compila y prueba la solución sin descargar la norma. Ver `AGENTS.md`.

Documentación: [PrintThermal_Motor_Maui-Emulator.Documentacion](https://github.com/hdcm-dev/PrintThermal_Motor_Maui-Emulator.Documentacion).
