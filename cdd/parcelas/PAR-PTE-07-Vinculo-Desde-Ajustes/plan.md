# Plan · PAR-PTE-07

> Sólo velocidad completa. Lo que va acá no se repite en `parcela.md`.

## Índice

1. [Enfoque técnico](#1-enfoque-técnico)
2. [Superficies](#2-superficies)
3. [Riesgos](#3-riesgos)

## 1. Enfoque técnico

- **Diagnóstico.** Traza HCI opcional de la pila propia (comandos y eventos) y, del lado de Android, la captura HCI de netsim y el registro de la pila de Bluetooth: Link Key Request y Notification, Authentication Complete, Encryption Change, a qué equipo atribuye Android cada evento.
- **Corrección.** En la pila C# (`Hci/`), según lo que muestre el diagnóstico, con prueba automática que falle antes y pase después.
- **Prueba real.** Emulador de Android API 37 con turno y emu-slot; el contenedor de la imagen con `--network container:` dentro del mismo turno (D4 de PAR-PTE-05). Camino (a): vínculo desde Ajustes por interfaz con `uiautomator` y la app de ejemplo imprime 5 de 5; camino (b): vincular conectando y la app imprime 5 de 5. Huella del PNG de la API igual a la de la carpeta.
- **Visor.** CSS de la tarjeta del ticket; medición en Chrome con emulación de ancho, antes y después.

## 2. Superficies

`src/PrintThermal.Emulator.Bluetooth/Hci/**`, `src/PrintThermal.Emulator.Bluetooth/Seguridad/**`, `src/PrintThermal.Emulator.Api/Integracion/MedioBluetooth.cs`, `src/PrintThermal.Emulator.Api/wwwroot/visor.css`, `tests/PrintThermal.Emulator.Tests/Bluetooth/**`, `docs/` (emparejar, diagnóstico, montaje).

## 3. Riesgos

- La causa puede estar fuera de la pila propia (Android o netsim): la corrección se limita a lo que la impresora emulada puede hacer y se documenta.
- Emulador y periférico deben caber en 2 CPU y 6 GB.
