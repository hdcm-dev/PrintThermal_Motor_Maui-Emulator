---
id: LAZ-0016
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
# LAZ-0016 · Vincular conectando como primera conexión tras encender el Bluetooth de Android, sin medir

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-07, 2026-11-27 (`cdd/parcelas/PAR-PTE-07-Vinculo-Desde-Ajustes/evidencia/2026-11-27-diagnostico.md`): Android
con netsim atribuye los eventos del manejador 0 al primer equipo que cargó de su configuración. El camino de vincular
conectando se midió en una conexión que no era la primera después de encender el Bluetooth de Android. Si esa conexión
fuera la primera y Android tuviera otros equipos vinculados, el cifrado de esa misma conexión podría no llegar; la
reconexión breve de ADR-0009 se dispararía al cortarse y la conexión siguiente debería cifrar. NO VERIFICADO.

## 2. Clasificación

Aclaración: no cambia el alcance; falta medir un caso del camino ya especificado.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| — | — | — |

## 4. Decisión

Pendiente: medir en el emulador de Android API 37 (Bluetooth recién encendido, otros equipos vinculados, vincular
conectando) y documentar el resultado en `docs/emparejar-e-imprimir.md`.
