---
id: PAR-PTE-03
titulo: Bluetooth Classic propio en C# contra netsim: descubrimiento, emparejamiento y recepción por RFCOMM
version: 1.0
estado: propuesta
velocidad: completa
base_commit: 9dec7ce0bdb56b1ee990cc97cba6015cf79c0afb
sprint: SPR-01
epica: REQ-EPI-Medio-Emulado@v1
historias:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v1
  - REQ-HIS-Recepcion-Integra@v1
lee:
  - REQ-HIS-Compilacion-Verificada@v2
  - VIS@v1
  - REQ-EPI-Medio-Emulado@v1
  - ADR-0004@v1
modifica:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v1
  - REQ-HIS-Recepcion-Integra@v1
appetite: tres días
pendientes:
decisiones:
  - {id: D1, tema: "TS-2 y TS-3: pila propia frente a puente y emparejamiento emulado frente al real (mesa sobre el resultado juzgado de la prueba corta)", clase: K2, adr: ADR-0004}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Que el emulador de Android descubra, empareje e imprima en «58HB6» (CE-1, CE-2, CE-3).

## Escenarios

- Camino feliz: Android encuentra «58HB6», la empareja por confirmación y le imprime; los bytes llegan íntegros.
- No feliz: netsim no responde por gRPC; el servicio lo informa en su estado y reintenta.

## Requisitos EARS

- REQ-01 · Cuando Android busca dispositivos cercanos, el sistema deberá anunciarse como «58HB6» con el perfil de puerto serie (UUID 0x1101) en SDP.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/SdpTests.cs · ElRegistroSdpPublicaSppConNombre58HB6
- REQ-02 · Cuando Android pide emparejar, el sistema deberá aceptar el emparejamiento seguro por confirmación con su dirección fija.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadTests.cs · ElEmparejamientoPorConfirmacionSeCompleta
- REQ-03 · Cuando llegan datos por RFCOMM, el sistema deberá entregarlos al almacén sin cambios.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/RfcommTests.cs · LosBytesLleganIntegros

## Casos borde

- Reinicio del emulador de impresora con el vínculo guardado en Android; desconexión a mitad de un ticket.

## Fuera de alcance

- iOS y NFC (intake §6).
- Inyección de fallos (intake §6).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Bluetooth/SdpTests.cs · ElRegistroSdpPublicaSppConNombre58HB6
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadTests.cs · ElEmparejamientoPorConfirmacionSeCompleta
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Bluetooth/RfcommTests.cs · LosBytesLleganIntegros

## Plan

Espera la mesa de TS-2 y TS-3 (DEC-001): queda en `propuesta` hasta que ADR-0004 pase a `aceptado`.

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
