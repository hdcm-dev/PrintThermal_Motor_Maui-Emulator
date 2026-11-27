# PrintThermal_Motor_Maui-Emulator

Emulador de una impresora térmica Bluetooth ESC/POS de 58 mm (58HB6) para pruebas locales, en contenedor y de extremo a extremo con el emulador de Android. Su meta es entregar cada ticket como imagen PNG por una API HTTP (OpenAPI y Scalar) y un visor web. Tecnología: .NET 10; solución `PrintThermal.Emulator`.

**Estado:** versión 1.0 (sprint 1, `cdd/sprints/SPR-01.md`). La impresora emulada se anuncia por Bluetooth Classic como «58HB6» en el emulador de Android, recibe por RFCOMM, interpreta ESC/POS a PNG y entrega cada ticket por la API y el visor. La fidelidad contra la 58HB6 física no está medida.

## Proyectos

| Proyecto | Qué hace |
|---|---|
| `PrintThermal.Emulator.Core` | Almacén de tickets e intérprete ESC/POS de 58 mm (384 puntos) a PNG |
| `PrintThermal.Emulator.Bluetooth` | Pila Bluetooth Classic propia en C# contra el controlador virtual del emulador de Android (netsim) |
| `PrintThermal.Emulator.Api` | Host ASP.NET Core: API de tickets, OpenAPI, Scalar y visor Blazor ; con `Emulador:Salida` guarda en disco y con `Emulador:Bluetooth:Netsim` corre el medio Bluetooth |
| `PrintThermal.Emulator.Client` | Cliente HTTP tipado de la API |
| `PrintThermal.Emulator.Tests` | Pruebas (xUnit) |

## Compilar y probar

```bash
dotnet build PrintThermal.Emulator.slnx
dotnet test PrintThermal.Emulator.slnx
```

El SDK está fijado en `global.json` y las versiones de los paquetes en `Directory.Packages.props`.

## Imagen y uso previsto

La imagen se publica en `ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:<versión>` con cada etiqueta `v*` (`.github/workflows/release.yml`); el release adjunta el binario.

Uso previsto: en el mismo equipo que el emulador de Android, unida a la red del contenedor del emulador, dentro del mismo turno y puesto de emulador, y se baja junto con él (nunca queda viva sola). netsim escucha sólo en el loopback de ese contenedor; con la red compartida se llega por `127.0.0.1` sin exponerlo. Su puerto gRPC está en `/tmp/netsim.ini` del contenedor del emulador (`grpc.port`).

```bash
docker run -d --rm --name impresora-58hb6 --network container:<contenedor del emulador Android> \
  --cpus 1 --memory 512m -v <carpeta de salida>:/data \
  -e Emulador__Bluetooth__Netsim=http://127.0.0.1:<puerto gRPC de netsim> \
  -e Emulador__Bluetooth__Direccion=04:00:00:00:00:01 \
  ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:<versión>
```

- La API (`/api/...`), `/openapi/v1.json`, `/scalar` y el visor (`/`) quedan en el puerto 5080 de la red del emulador; la imagen no publica puertos.
- Cada ticket queda en la carpeta de salida como `{id}.bin` y `{id}.png`; la carpeta tiene que admitir escritura del usuario de la imagen (uid 1654).
- La app empareja por confirmación (SSP) con la impresora emulada; las claves quedan en `claves-bluetooth.txt` de la carpeta de salida.
- Fuera de un contenedor, el host escucha sólo en `http://127.0.0.1:5080` salvo configuración.

## Gobierno

El producto se gobierna con la norma CDD (repositorio privado). Sus compuertas se corren en local; la CI pública (`.github/workflows/cdd.yml`) compila y prueba la solución sin descargar la norma. Ver `AGENTS.md`.

Documentación: [PrintThermal_Motor_Maui-Emulator.Documentacion](https://github.com/hdcm-dev/PrintThermal_Motor_Maui-Emulator.Documentacion).
