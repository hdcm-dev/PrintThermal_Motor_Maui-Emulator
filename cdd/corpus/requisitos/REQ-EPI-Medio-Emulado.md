---
id: REQ-EPI-Medio-Emulado
version: 1
gobierno: parcela
tipo: explicacion
rutas: []
---
# REQ-EPI-Medio-Emulado

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Una impresora 58HB6 emulada que el emulador de Android descubre en «Dispositivos cercanos» de Bluetooth, empareja y a la que imprime por Bluetooth Classic, RFCOMM, perfil de puerto serie (`vision.md` §2 y §4; ADR-0004).

## 2. Criterios

- CE-1: aparece en 30 s o menos, 3 de 3 (`vision.md` §3).
- CE-2: la impresora figura vinculada en Android (`vision.md` §3).
- CE-3: huella recibida igual a la enviada, 5 de 5 (`vision.md` §3).

## 3. Trazas

- Sube de: `vision.md` §4 (Product Goal)
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
