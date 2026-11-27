---
id: REQ-HIS-Cliente-Tipado
version: 3
epica: REQ-EPI-Acceso
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Client/*.cs
  - tests/PrintThermal.Emulator.Tests/Cliente/**
---
# REQ-HIS-Cliente-Tipado

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un cliente HTTP tipado y reutilizable de la API de tickets, que usan el visor y las pruebas de extremo a extremo (ADR-0003).

`ClienteDeTickets`, en el proyecto `PrintThermal.Emulator.Client`, se arma sobre un `HttpClient` con la dirección del emulador. Cubre cada ruta de la API y devuelve `null` cuando la API responde 404 o 204. Los modelos son `TicketInfo` y `EstadoDelEmulador`. `DesdeElPrincipio` (`"0"`) sirve para esperar cuando no había tickets. La opción A de ADR-0003 la eligió una mesa ciega (PAR-PTE-04, D2); la persona la aprueba en el lote de K2 de la Review.

## 2. Criterios

- Dado el servicio en marcha, cuando el cliente pide el último ticket, entonces devuelve el mismo PNG que la API.

## 3. Trazas

- Sube de: REQ-EPI-Acceso
- Implementado en: `src/PrintThermal.Emulator.Client/ClienteDeTickets.cs` y `Modelos.cs`; pruebas en `tests/PrintThermal.Emulator.Tests/Cliente/`.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-01 | `rutas:` cubre sólo el código (`**/*.cs`); el `.csproj` es estructura del lock, como el de los demás proyectos |
| v3 | PAR-PTE-04 | Cliente `ClienteDeTickets` con todas las rutas del contrato y `DesdeElPrincipio`; rutas de pruebas; `rutas:` pasa de `**/*.cs` a `*.cs` porque la compuerta compara con `fnmatch` y `**/*.cs` no cubre los archivos de la raíz del proyecto |
