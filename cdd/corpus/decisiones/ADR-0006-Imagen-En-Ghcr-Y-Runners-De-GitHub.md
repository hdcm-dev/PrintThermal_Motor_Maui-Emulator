---
id: ADR-0006
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0006 · Release con imagen de contenedor de versión fija en ghcr.io, compilada en runners de GitHub

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

- Release en GitHub con imagen de contenedor de versión fija; compilar al vuelo sólo para desarrollar el emulador (intake §8, Q2:16, TS-6). Compilación sólo en runners de GitHub, sin runner propio, por ser repositorio público (intake §8, Q2:18, TS-5).
- Contenedor aparte unido a la red del contenedor del emulador (intake §8; R:13, X:10).
- La persona, después de la ronda 02: «Ok. No sabía que se podía publicar el contenedor en github . Tiene algún costo?» (bloque C4 de `PROMPTs/00-Fase-Previa/01-Relevamiento/OUTPUTs/02-respuestas.md`). Al firmar: «Vos podes publicar el contenedor listo para usar.» (`Idea-Inicial.md` §8).
- Documentación oficial de GitHub, abierta el 2026-11-27:
  - https://docs.github.com/en/billing/concepts/product-billing/github-packages: «GitHub Packages usage is free for public packages. In addition, data transferred in from any source is free.» y «Container image storage and bandwidth for the Container registry is currently free.»
  - https://docs.github.com/en/actions/tutorials/publish-packages/publish-docker-images: «You can use the automatically-generated `GITHUB_TOKEN` secret for the password.» y «`registry`: Must be set to `ghcr.io`.», con el permiso `packages: write` en el trabajo.

## 2. Opciones

- A. Imagen en `ghcr.io` del mismo repositorio, etiqueta de versión fija por release, publicada por un workflow en runners de GitHub con `GITHUB_TOKEN` (sin token de Docker Hub; reversible: se puede publicar además en otro registro).
- B. Docker Hub (pide un token de una cuenta externa como secreto; la persona preguntó por él en la ronda 02, C2).
- C. Compilar la imagen al vuelo en cada uso (descartada por TS-6 salvo para desarrollar).

## 3. Decisión

A. La publicación (K3) la autorizó la persona: Q2:16 y «Vos podes publicar el contenedor listo para usar» (2026-11-27). El registro `ghcr.io` es K2 dentro de esa autorización; respaldo: las dos páginas oficiales citadas. El contenedor no se expone fuera de la red de pruebas (TS-7).

### Conocimientos que no cargan

- `Red-Movil-Problematica` 2.0 (base de conocimientos privada, `fe51898`): condición «antes de poner una falla de red en un emulador, de diseñar una prueba de resiliencia de red de una app móvil o de escribir una especialización de una falla». Hecho que la incumple: este producto une su contenedor al espacio de red del emulador para llegar a netsim, sin poner fallas de red; la inyección de fallos está fuera de la primera etapa (intake §6). Trata la materia (la red del contenedor de un emulador Android), por eso se nombra. Si una etapa posterior prueba la impresión con red deficiente, la condición se cumple y se declara en `consulta:` con un ADR.

## 4. Consecuencias

- Épica de distribución (`vision.md` §4).
- La CI de CDD (`.github/workflows/cdd.yml`) corre en `ubuntu-latest` (runners de GitHub), sin runner propio.
