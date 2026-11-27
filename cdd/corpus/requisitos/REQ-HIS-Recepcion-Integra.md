---
id: REQ-HIS-Recepcion-Integra
version: 2
epica: REQ-EPI-Medio-Emulado
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Bluetooth/Rfcomm/**
  - tests/PrintThermal.Emulator.Tests/Bluetooth/Rfcomm*.cs
---
# REQ-HIS-Recepcion-Integra

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Los bytes que la app envía por RFCOMM llegan al emulador sin cambios y se entregan al almacén de tickets.

## 2. Criterios

- Dado un canal RFCOMM abierto desde Android, cuando la app envía un trabajo de impresión, entonces el SHA-256 de lo recibido es igual al de lo enviado, 5 de 5 (CE-3).

## 3. Trazas

- Sube de: REQ-EPI-Medio-Emulado
- Implementado en: `src/PrintThermal.Emulator.Bluetooth/Rfcomm/` (PAR-PTE-03): la entrega va por `IReceptorDeBytes`, que el host del servicio conecta con el almacén de tickets.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-03 | `rutas:` suma `tests/…/Bluetooth/Rfcomm*.cs`; la frontera con el almacén es `IReceptorDeBytes` |
