---
id: REQ-HIS-Descubrimiento-Y-Emparejamiento
version: 3
epica: REQ-EPI-Medio-Emulado
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Bluetooth/Hci/**
  - src/PrintThermal.Emulator.Bluetooth/L2cap/**
  - src/PrintThermal.Emulator.Bluetooth/Sdp/**
  - src/PrintThermal.Emulator.Bluetooth/Seguridad/**
  - tests/PrintThermal.Emulator.Tests/Bluetooth/Hci*.cs
  - tests/PrintThermal.Emulator.Tests/Bluetooth/Sdp*.cs
  - tests/PrintThermal.Emulator.Tests/Bluetooth/Seguridad*.cs
---
# REQ-HIS-Descubrimiento-Y-Emparejamiento

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

La impresora emulada se anuncia por Bluetooth Classic con el nombre «58HB6», publica en SDP el perfil de puerto serie (UUID 0x1101) y admite el emparejamiento seguro por confirmación, con una dirección Bluetooth fija entre reinicios; habla HCI con el controlador virtual del emulador (netsim) por gRPC (ADR-0004).

## 2. Criterios

- Dado el emulador de Android con el contenedor del emulador de impresora en su espacio de red, cuando Android busca dispositivos cercanos, entonces «58HB6» aparece en 30 s o menos, 3 de 3 (CE-1).
- Dado «58HB6» visible, cuando Android la empareja, entonces figura vinculada y lo sigue estando tras reiniciar el emulador de impresora (CE-2).
- Dado «58HB6» vinculada desde Ajustes de Android (vínculo dedicado, `createBond`), cuando una app conecta después por RFCOMM, entonces la conexión se cifra con la clave guardada, sin reemparejar, y el ticket llega íntegro, 5 de 5 (CE-2, LAZ-0011).

## 3. Trazas

- Sube de: REQ-EPI-Medio-Emulado
- Implementado en: `src/PrintThermal.Emulator.Bluetooth/{Hci,L2cap,Sdp,Seguridad}/` (PAR-PTE-03); pruebas en `tests/PrintThermal.Emulator.Tests/Bluetooth/`.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-03 | `rutas:` suma las pruebas de la historia (`tests/…/Bluetooth/Hci*.cs`, `Sdp*.cs`, `Seguridad*.cs`); código en `Hci`, `L2cap`, `Sdp` y `Seguridad` |
| v3 | LAZ-0011 | Criterio del camino de Ajustes: vínculo dedicado y conexión RFCOMM posterior cifrada |
