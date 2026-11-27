---
id: REQ-HIS-Api-De-Tickets
version: 1
epica: REQ-EPI-Acceso
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Api/Api/**
  - src/PrintThermal.Emulator.Api/Program.cs
---
# REQ-HIS-Api-De-Tickets

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un host ASP.NET Core expone `/api/...`: estado, lista de tickets, último, un ticket por id en `.png` y `.bin`, esperar un ticket con tiempo máximo y borrar; publica OpenAPI en `/openapi/v1.json` y la página Scalar en `/scalar`; escucha sólo en local salvo configuración (ADR-0001).

## 2. Criterios

- Dado un ticket impreso, cuando se pide su PNG por la API, entonces llega como `image/png` con la misma huella SHA-256 que el generado, 5 de 5 (CE-5).
- Dado que no llega ningún ticket, cuando se espera con tiempo máximo, entonces la API responde al vencer el plazo sin quedar colgada.

## 3. Trazas

- Sube de: REQ-EPI-Acceso
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
