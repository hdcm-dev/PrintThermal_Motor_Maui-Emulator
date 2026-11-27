---
id: LAZ-0005
fecha: 2026-11-27
origen: parcela
clase: aclaracion
estado: abierto
alcance: corpus
ids:
deuda:
normalizado:
parcela: PAR-PTE-05
---
# LAZ-0005 · El periférico en un contenedor propio unido a la red del emulador no está medido

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-03, 2026-11-27. El intake (§8) pide un «contenedor aparte unido a la red del contenedor del emulador». La prueba
real de PAR-PTE-03 corrió el periférico dentro del contenedor del emulador (`docker exec`), igual que la prueba corta,
porque netsim publica su gRPC sólo en loopback dentro de ese contenedor y no se llega desde otro equipo
(`cdd/parcelas/PAR-PTE-03-Medio-Emulado/evidencia/2026-11-27-prueba-real.txt` §1). Un contenedor propio con
`--network container:<emulador>` comparte ese loopback, pero no se midió: falta un turno aparte en el equipo del
emulador. Supuesto que cambia la imagen y el montaje (PAR-PTE-05, PAR-PTE-06) si es falso.

## 2. Clasificación

Aclaración: no cambia el alcance; verifica un supuesto de la tecnología del intake antes de publicar la imagen.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| — | — | — |

## 4. Decisión

Pendiente: se mide en PAR-PTE-05 (contenedor propio con `--network container:` y la misma prueba real: descubrir,
emparejar, SHA-256 5 de 5).
