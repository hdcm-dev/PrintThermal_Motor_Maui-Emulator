---
id: REQ-EPI-Distribucion
version: 1
gobierno: parcela
tipo: explicacion
rutas: []
---
# REQ-EPI-Distribucion

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Release de GitHub con imagen de contenedor de versión fija en `ghcr.io`, compilada y probada en runners de GitHub (`vision.md` §4; ADR-0006).

## 2. Criterios

- Dado un release etiquetado, cuando termina su workflow, entonces la imagen de esa versión está en `ghcr.io` y pasó las pruebas de la solución.

## 3. Trazas

- Sube de: `vision.md` §4
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
