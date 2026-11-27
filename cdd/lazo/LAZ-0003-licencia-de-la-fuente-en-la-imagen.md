---
id: LAZ-0003
fecha: 2026-11-27
origen: review
clase: aclaracion
estado: abierto
alcance: corpus
ids:
deuda:
normalizado:
parcela:
---
# LAZ-0003 · La licencia de la fuente en la imagen publicada

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

Auditoría del cierre de PAR-PTE-02 (2026-11-27, segunda ronda hallazgo 2 y tercera ronda hallazgo 3): Core embebe Terminus Font 24x12 (SIL OFL 1.1). La OFL §2 pide que cada copia lleve el aviso de copyright y la licencia. `PrintThermal.Emulator.Core.csproj` copia `EscPos/Fuentes/OFL.txt` como `Licencias/TerminusFont-OFL.txt` a la salida de la compilación y de la publicación; la prueba `LaLicenciaDeLaFuenteViajaConLoCompilado` lo verifica sólo en la salida de la compilación. Nadie verifica todavía que la imagen de contenedor publicada (PAR-PTE-05) lo incluya.

## 2. Clasificación

Aclaración (L1): no cambia el alcance; es una condición de la licencia que la parcela de distribución tiene que comprobar.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| PAR-PTE-05 | No menciona la licencia de la fuente | Por decidir: una prueba sobre la imagen publicada |

## 4. Decisión

Pendiente.
