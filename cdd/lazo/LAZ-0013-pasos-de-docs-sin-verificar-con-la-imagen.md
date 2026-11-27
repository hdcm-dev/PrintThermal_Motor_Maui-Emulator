---
id: LAZ-0013
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
# LAZ-0013 · Pasos de la documentación de uso que la CI no corre y no se midieron con la imagen publicada

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-06, 2026-11-27. La CI corre los ejemplos de API, visor y recetas contra el host en proceso con la fuente
simulada, y contrasta con el código las rutas, las variables, la imagen y el puerto. No corre lo que necesita Docker o
el emulador de Android, y esta parcela no lo midió con la imagen `1.0.0` (privada al 2026-11-27):

- el `docker run` del montaje con la imagen publicada (PAR-PTE-05 lo midió con una imagen local de `5066fa3`);
- la búsqueda del puerto gRPC de netsim: el archivo fue `/tmp/netsim.ini` en PAR-PTE-03; otro montaje puede usar
  `netsimd.ini` en otra carpeta;
- la fuente simulada con la imagen (`-p 127.0.0.1:5080:5080`), la llegada a la API desde el equipo por la dirección
  del contenedor del emulador y el `docker build` desde la etiqueta como reemplazo de la imagen privada;
- que el vínculo con Android siga sirviendo al reiniciar el contenedor con la misma carpeta y dirección;
- el acceso directo al visor desde Chrome del emulador de Android (en PAR-PTE-05 hizo falta un reenvío TCP temporal);
- el esqueleto de workflow de GitHub Actions de `docs/recetas.md` §5, que no se corrió en un runner.

`docs/` marca cada uno «NO VERIFICADO» (el montaje con la imagen publicada, en `docs/montaje.md` §2).

## 2. Clasificación

Aclaración: no cambia el alcance. Si un paso es falso, la documentación de uso (REQ-HIS-Documentacion-De-Uso) se
corrige; si falla el montaje, toca también REQ-HIS-Imagen-Publicada.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| `docs/montaje.md`, `docs/recetas.md`, `docs/diagnostico.md` | pasos marcados NO VERIFICADO | medidos con la imagen publicada, o corregidos (propuesta) |

## 4. Decisión

Pendiente: medir los pasos con la imagen publicada en el próximo extremo a extremo con turno y puesto de emulador.
