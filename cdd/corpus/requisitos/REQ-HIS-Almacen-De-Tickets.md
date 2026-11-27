---
id: REQ-HIS-Almacen-De-Tickets
version: 1
epica: REQ-EPI-Ticket-Png
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Core/Almacen/**
---
# REQ-HIS-Almacen-De-Tickets

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Cada ticket recibido se guarda con un id, los bytes recibidos (`.bin`) y su PNG, en una carpeta de salida montable.

## 2. Criterios

- Dado un ticket recibido, cuando termina la impresión, entonces hay un `.bin` y un `.png` con su id en la carpeta de salida (CE-4).

## 3. Trazas

- Sube de: REQ-EPI-Ticket-Png
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
