---
id: REQ-HIS-Ticket-A-Png
version: 1
epica: REQ-EPI-Ticket-Png
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Core/EscPos/**
---
# REQ-HIS-Ticket-A-Png

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un intérprete ESC/POS para papel de 58 mm (384 puntos de ancho) convierte los bytes de un ticket en una imagen PNG (ADR-0005).

## 2. Criterios

- Dados los bytes que la librería cliente genera para un ticket, cuando se interpretan, entonces el PNG coincide con su vector dorado.
- Dado un comando ESC/POS que el intérprete no conoce, cuando aparece en el ticket, entonces se registra y el resto del ticket se interpreta igual.

## 3. Trazas

- Sube de: REQ-EPI-Ticket-Png
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
