---
id: LAZ-0012
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
# LAZ-0012 · El consumo de tokens de la pila propia no se asienta frente al umbral firmado

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-03, 2026-11-27, mesa de TS-2 y TS-3 (h2, procedente 5 de 5) y auditor de cierre (ronda 2, N1). El intake firmado
(§8, R:15) dice que la pila propia en C# vuelve a la persona, con Bumble como puente, si supera 2,5 millones de tokens.
La estimación previa de la pila era de 1,5 a 2,5 millones (Medios y factibilidad §3, NO VERIFICADA). PAR-PTE-03 no asentó
su consumo, y faltan parcelas que tocan la pila (PAR-PTE-04 conecta el receptor, PAR-PTE-05 mide el contenedor aparte).
Sin ese registro, el disparador firmado no se puede activar.

## 2. Clasificación

Aclaración: no cambia el alcance; falta el registro que hace verificable una condición ya firmada. Si el consumo supera el
umbral, D1 de PAR-PTE-03 vuelve a la persona (texto propuesto para ADR-0004, §4).

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| Parcelas de la pila | sin consumo asentado | consumo de tokens por parcela, con su fuente (propuesta) |

## 4. Decisión

Pendiente: definir dónde y cómo se asienta el consumo (fuente medible por sesión o por parcela) y asentar el de PAR-PTE-03.
