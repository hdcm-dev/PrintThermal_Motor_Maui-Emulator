---
id: REQ-EPI-Acceso
version: 1
gobierno: parcela
tipo: explicacion
rutas: []
---
# REQ-EPI-Acceso

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

API HTTP de tickets con OpenAPI y página Scalar, visor web Blazor sin usuarios integrado al mismo servicio que consume la API por HTTP, y cliente HTTP tipado reutilizable (`vision.md` §2 y §4; ADR-0001, ADR-0002, ADR-0003).

## 2. Criterios

- CE-5: el PNG de un ticket se pide por la API HTTP y llega como `image/png` con la misma huella SHA-256 que el generado, 5 de 5; y el visor lo muestra en un recorrido en navegador real (`vision.md` §3, P3 A de la ronda K3).

## 3. Trazas

- Sube de: `vision.md` §4 (Product Goal)
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
