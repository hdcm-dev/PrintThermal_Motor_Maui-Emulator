---
id: REQ-HIS-Visor-Web
version: 1
epica: REQ-EPI-Acceso
gobierno: parcela
tipo: referencia
interfaz: n2
rutas:
  - src/PrintThermal.Emulator.Api/Visor/**
  - src/PrintThermal.Emulator.Api/wwwroot/**
---
# REQ-HIS-Visor-Web

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un visor Blazor Interactive Server sin usuarios, en `/` del mismo servicio, lista los tickets y muestra su PNG llamando a la API por HTTP con el cliente tipado (ADR-0002).

## 2. Criterios

- Dado un ticket impreso, cuando se abre el visor en un navegador real, entonces se ve su imagen, con capturas y consola sin errores (CE-5).

## 3. Trazas

- Sube de: REQ-EPI-Acceso
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
