---
id: REQ-HIS-Visor-Web
version: 2
epica: REQ-EPI-Acceso
gobierno: parcela
tipo: referencia
interfaz: n2
rutas:
  - src/PrintThermal.Emulator.Api/Visor/**
  - src/PrintThermal.Emulator.Api/wwwroot/**
  - tests/PrintThermal.Emulator.Tests/Visor/**
---
# REQ-HIS-Visor-Web

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un visor Blazor Interactive Server sin usuarios, en `/` del mismo servicio, lista los tickets y muestra su PNG llamando a la API por HTTP con el cliente tipado (ADR-0002).

En PAR-PTE-04 el visor es una sola página:

- Cabecera con el estado del medio y la cantidad de tickets.
- Lista con la hora y el tamaño de cada ticket.
- El ticket elegido a escala: 384 puntos ocupan 48 mm en un papel de 58 mm (D6, no medido en la 58HB6).
- Descarga del PNG y del `.bin`.
- Limpiar, con confirmación.
- Cuatro estados: cargando, vacío, datos y error.

La página se actualiza sola con la espera larga de `/api/tickets/esperar`. Usa CSS propio, sin CDN.

## 2. Criterios

- Dado un ticket impreso, cuando se abre el visor en un navegador real, entonces se ve su imagen, con capturas y consola sin errores (CE-5).

## 3. Trazas

- Sube de: REQ-EPI-Acceso
- Implementado en: `src/PrintThermal.Emulator.Api/Visor/` (`App`, `Routes`, `Inicio`) y `src/PrintThermal.Emulator.Api/wwwroot/` (`visor.css`, `favicon.svg`); prueba en `tests/PrintThermal.Emulator.Tests/Visor/`; recorrido en navegador real en `cdd/parcelas/PAR-PTE-04-Acceso/recorrido/`.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-04 | Visor de una página con lista, ticket a escala, descargas, limpiar, estado y actualización por espera; rutas de pruebas |
