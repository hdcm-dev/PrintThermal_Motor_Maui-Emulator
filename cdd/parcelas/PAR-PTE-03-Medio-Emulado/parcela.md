---
id: PAR-PTE-03
titulo: Bluetooth Classic propio en C# contra netsim: descubrimiento, emparejamiento y recepción por RFCOMM
version: 1.0
estado: en-curso
velocidad: completa
base_commit: 9dec7ce0bdb56b1ee990cc97cba6015cf79c0afb
sprint: SPR-01
epica: REQ-EPI-Medio-Emulado@v1
historias:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v1
  - REQ-HIS-Recepcion-Integra@v1
lee:
  - REQ-HIS-Compilacion-Verificada@v3
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
  - {id: D2, tema: "gRPC de netsim sin paquetes: HTTP/2 sin TLS de HttpClient y protobuf escrito a mano para los cinco mensajes que se usan; no suma dependencias ni toca Directory.Packages.props", clase: K1}
  - {id: D3, tema: "SSP sin entrada ni salida (IO 0x03) y bonding general sin MITM (0x04): Android pide consentimiento y el emulador confirma; PIN 0000 sólo si Android usa emparejamiento heredado", clase: K1}
  - {id: D4, tema: "registro SDP: el de la prueba corta (handle, clase 0x1101, L2CAP + RFCOMM canal 1, navegación pública) más perfil SPP 1.2, base de idioma y nombre de servicio «58HB6»; el registro real de la 58HB6 sigue sin medir", clase: K1}
  - {id: D5, tema: "re-planificación: lee REQ-HIS-Compilacion-Verificada@v2 (I-3); ADR-0004 sale de modifica porque tiene gobierno humano y la parcela no lo toca; las historias suben a v2 para cubrir sus pruebas en rutas", clase: K1}
  - {id: D6, tema: "la frontera con el almacén de tickets es IReceptorDeBytes en Rfcomm (Core no está fusionado); el host del servicio la conecta en PAR-PTE-04", clase: K1}
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

`plan.md` y `tasks.md`. Opción A de ADR-0004 (la del intake firmado); D1 la decide una mesa de otro contexto con el resultado de la prueba real, y el PO la aprueba en lote en la Review (K2).

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
