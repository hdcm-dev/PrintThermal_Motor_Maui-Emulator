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
# LAZ-0016 · Casos del vínculo sin medir tras PAR-PTE-07

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-07, 2026-11-27 (`cdd/parcelas/PAR-PTE-07-Vinculo-Desde-Ajustes/evidencia/`): Android con netsim atribuye los
eventos del manejador 0 al primer equipo que cargó de su configuración. Medido en esa parcela: vincular conectando como
primera conexión con otros equipos vinculados vence en el primer intento y, tras la reconexión breve (ADR-0009), el
segundo cifra (1 de 1). Sin medir:

- una app que llama `createBond()` y conecta enseguida, antes de que Android corte la conexión del vínculo (unos 5 s);
- un teléfono real en lugar del emulador de Android;
- que Android use la conexión breve para la app (la impresora la corta apenas se completa) o que el corte de la breve
  falle en el controlador (la conexión quedaría abierta sin tráfico);
- que Android borre el vínculo de otro equipo cuando una vinculación sin confirmar falla como primera conexión (visto
  1 vez, del lado de Android).

## 2. Clasificación

Aclaración: no cambia el alcance; falta medir un caso del camino ya especificado.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| — | — | — |

## 4. Decisión

Pendiente: medir los casos de §1 que se puedan provocar (`createBond()` con conexión inmediata, teléfono real) y
documentar el resultado en `docs/emparejar-e-imprimir.md`.
