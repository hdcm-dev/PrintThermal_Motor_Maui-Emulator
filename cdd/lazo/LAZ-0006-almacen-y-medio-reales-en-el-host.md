---
id: LAZ-0006
fecha: 2026-11-27
origen: parcela
clase: aclaracion
estado: abierto
alcance: corpus
ids:
deuda:
normalizado:
parcela:
---
# LAZ-0006 · Conectar el almacén en disco y el medio Bluetooth al host de la API

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-04, 2026-11-27. La API y el visor consumen `IAlmacenDeTickets` e `IEstadoDelMedio`, declaradas en `src/PrintThermal.Emulator.Api/Api/AlmacenDeTickets.cs`. Esta parcela las cumple con un almacén en memoria y una fuente simulada (D4), porque PAR-PTE-02 (almacén en disco, REQ-HIS-Almacen-De-Tickets) y PAR-PTE-03 (medio Bluetooth) no estaban cerradas.

Ninguna parcela de SPR-01 nombra el trabajo de conectar el almacén en disco y el medio real al host. Ese trabajo incluye:

- un adaptador desde Core y Bluetooth, que no pueden referenciar al proyecto Api;
- el registro en `Program.cs`;
- que los ids del almacén en disco respeten el contrato: ordenables, sin puntos y mayores que `0`.

Pregunta: ¿lo hace PAR-PTE-05 (la imagen necesita el host completo) o una parcela nueva? ¿El contrato de las dos interfaces pasa a Core para que lo implementen sin adaptador?

## 2. Clasificación

`aclaracion` provisoria (no se sabe todavía si es trabajo de una parcela ya planificada); el 08 la reclasifica.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| | | |

## 4. Decisión
