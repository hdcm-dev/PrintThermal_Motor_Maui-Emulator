---
id: REQ-HIS-Cliente-Tipado
version: 2
epica: REQ-EPI-Acceso
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Client/**/*.cs
---
# REQ-HIS-Cliente-Tipado

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un cliente HTTP tipado y reutilizable de la API de tickets, que usan el visor y las pruebas de extremo a extremo (ADR-0003).

## 2. Criterios

- Dado el servicio en marcha, cuando el cliente pide el último ticket, entonces devuelve el mismo PNG que la API.

## 3. Trazas

- Sube de: REQ-EPI-Acceso
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-01 | `rutas:` cubre sólo el código (`**/*.cs`); el `.csproj` es estructura del lock, como el de los demás proyectos |
