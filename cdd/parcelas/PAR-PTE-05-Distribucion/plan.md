# Plan · PAR-PTE-05

> Sólo velocidad completa. Lo que va acá no se repite en `parcela.md`.

## Índice

1. [Enfoque técnico](#1-enfoque-técnico)
2. [Superficies](#2-superficies)
3. [Riesgos](#3-riesgos)

## 1. Enfoque técnico

- **Host (D2).** `src/PrintThermal.Emulator.Api/Integracion/`: un almacén en disco que cumple `IAlmacenDeTickets` sobre el almacén de PAR-PTE-02, un receptor RFCOMM que alimenta un intérprete por conexión y guarda cada ticket terminado (el cierre de la conexión termina el ticket abierto), y un servicio alojado que corre el periférico de PAR-PTE-03 y cumple `IEstadoDelMedio`. `Program.cs` los registra si la configuración trae `Emulador:Salida` y `Emulador:Bluetooth:Netsim`; sin ellas sigue como en PAR-PTE-04.
- **Imagen (D1, D3).** `Dockerfile` en dos etapas: compila con el SDK de `global.json` y corre sobre `aspnet:10.0.14-noble-chiseled` fijada por digest, como usuario sin privilegios. Escucha en `http://0.0.0.0:5080` dentro de su red de contenedor y guarda en `/data` (volumen).
- **Release (REQ-01).** `.github/workflows/release.yml`: al empujar una etiqueta `v*`, en `ubuntu-latest`, compila, prueba, publica la imagen en `ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:<versión>` con `GITHUB_TOKEN` (`packages: write`) y crea el release con el binario publicado. Una etiqueta repetida falla antes de publicar.
- **Extremo a extremo (REQ-02, D4).** Emulador de Android API 37 en su contenedor con turno y emu-slot; el contenedor de la imagen con `--network container:` dentro del mismo turno. La app de ejemplo `MotorDsl.SampleApp` de la librería cliente (compilada con turno en el equipo de cómputo) empareja, conecta e imprime cinco veces; la API entrega cada PNG. Los bytes capturados quedan como vector para repetir la cadena en la CI.

## 2. Superficies

`Dockerfile`, `.dockerignore`, `.github/workflows/release.yml`, `src/PrintThermal.Emulator.Api/Integracion/**`, `src/PrintThermal.Emulator.Api/Program.cs`, `tests/PrintThermal.Emulator.Tests/Distribucion/**`, `tests/PrintThermal.Emulator.Tests/Api/IntegracionTests.cs`. Además: `src/PrintThermal.Emulator.Api/PrintThermal.Emulator.Api.csproj` (referencias a Core y Bluetooth), `README.md` (uso previsto, D4), el corpus de las dos historias y el avance en LAZ-0001, LAZ-0006, LAZ-0008 y LAZ-0010.

## 3. Riesgos

- Emulador y periférico deben caber en 2 CPU y 6 GB (D4): el emulador corre con 1 CPU.
- La imagen publicada con `GITHUB_TOKEN` nace privada: hacerla pública lo decide la persona.
