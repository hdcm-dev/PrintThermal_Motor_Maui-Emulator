---
id: LAZ-0007
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
# LAZ-0007 · Escala del visor: 8 puntos por milímetro sin medir en la 58HB6

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-04, 2026-11-27, D6. El visor dibuja el ticket con 8 puntos por milímetro: 384 puntos ocupan 48 mm en un papel de 58 mm. La especificación desde la librería lo marca como NO VERIFICADO para la 58HB6 («384 puntos = 48 mm … para la 58HB6: **NO VERIFICADO / [MEDIR]**», `Especificacion-58HB6-Desde-La-Libreria.md` §5, repositorio de documentación).

Si la densidad real es otra, la escala del visor queda mal. La constante es `PuntosPorMilimetro` en `src/PrintThermal.Emulator.Api/Visor/Inicio.razor`.

## 2. Clasificación

`aclaracion`: supuesto que cambia la escala mostrada si es falso; se resuelve con una medida de la impresora o de su hoja de datos.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| | | |

## 4. Decisión
