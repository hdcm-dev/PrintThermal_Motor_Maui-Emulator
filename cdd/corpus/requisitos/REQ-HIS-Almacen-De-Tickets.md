---
id: REQ-HIS-Almacen-De-Tickets
version: 2
epica: REQ-EPI-Ticket-Png
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Core/Almacen/**
  - tests/PrintThermal.Emulator.Tests/Almacen/**
---
# REQ-HIS-Almacen-De-Tickets

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Cada ticket recibido se guarda con un id, los bytes recibidos (`.bin`) y su PNG, en una carpeta de salida montable. El id es `AAAAMMDDTHHMMSSmmm-NNNN` (hora UTC y secuencia), ordena por llegada y se valida antes de tocar el disco; la carpeta es la fuente de verdad, así que un reinicio ve los mismos tickets.

## 2. Criterios

- Dado un ticket recibido, cuando termina la impresión, entonces hay un `.bin` y un `.png` con su id en la carpeta de salida (CE-4).
- Dado un id que no tiene la forma de un id, cuando se pide, lee o borra, entonces no se toca ningún archivo fuera de la carpeta.

## 3. Trazas

- Sube de: REQ-EPI-Ticket-Png
- Implementado en: `src/PrintThermal.Emulator.Core/Almacen/` (`AlmacenDeTickets`, `TicketGuardado`); pruebas en `tests/PrintThermal.Emulator.Tests/Almacen/`.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-02 | Forma del id, listado desde la carpeta, lectura y borrado; `rutas:` suma sus pruebas |
