---
id: LAZ-0010
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
# LAZ-0010 · La entrega de RFCOMM al almacén de tickets no tiene parcela que la conecte

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-03, 2026-11-27, auditor de cierre (hallazgo 1). REQ-HIS-Recepcion-Integra pide que los bytes de RFCOMM «se
entregan al almacén de tickets». PAR-PTE-03 entrega por `IReceptorDeBytes` (`src/PrintThermal.Emulator.Bluetooth/Rfcomm/`)
porque el almacén del Core no estaba fusionado (D6). La conexión del receptor con el almacén va en el host del servicio
(PAR-PTE-04), pero PAR-PTE-04 no lee ni modifica REQ-HIS-Recepcion-Integra: hoy ninguna parcela planificada la hace. Relacionado: el registro de PAR-PTE-04 sobre conectar el almacén y el medio
reales al host (su LAZ-0006, PR #4), que mira el mismo trabajo desde la API.

## 2. Clasificación

Aclaración: no cambia el alcance (la historia ya lo pide); falta asignarla a una parcela. Propuesta: que PAR-PTE-04 sume
REQ-HIS-Recepcion-Integra a `lee` (o a `modifica` si cambia su texto) y conecte `IReceptorDeBytes` con el almacén, con
una prueba de extremo a extremo que reciba bytes por RFCOMM y los encuentre como ticket en la API.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| PAR-PTE-04 `lee`/`modifica` | sin REQ-HIS-Recepcion-Integra | con REQ-HIS-Recepcion-Integra (propuesta) |

## 4. Decisión

Pendiente: la re-planificación la decide el orquestador 02/08.

## Avance en PAR-PTE-05 (2026-11-27)

PAR-PTE-05 (D2) conecta la recepción RFCOMM con el almacén por `ReceptorAlAlmacen`: un intérprete por conexión y cada ticket terminado se guarda; prueba `ElTicketRecibidoPorRfcommLlegaALaApi`. Queda para el 08 especificarlo o cerrarlo.
