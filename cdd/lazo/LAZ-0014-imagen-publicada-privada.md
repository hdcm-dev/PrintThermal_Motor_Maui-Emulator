---
id: LAZ-0014
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
# LAZ-0014 · La imagen publicada es privada y el montaje documentado necesita credenciales

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-06, 2026-11-27. El release `v1.0.0` publicó `ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:1.0.0` con el
token del workflow, y la imagen quedó privada: un pull sin credenciales responde «unauthorized» (bitácora del repositorio
de documentación, release v1.0.0). El plan de PAR-PTE-05 ya dejaba la visibilidad a la persona. El camino principal de
`docs/montaje.md` (`docker pull` de la versión fija) no funciona para un agente sin acceso al paquete; `docs/` ofrece
dos salidas (iniciar sesión con un token de lectura de paquetes, o construir desde la etiqueta).

## 2. Clasificación

Aclaración: no cambia el alcance. Hacer pública la imagen es una decisión de la persona (K3); si la toma, `docs/`
quita la advertencia de la imagen privada.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| `docs/montaje.md` §2, `docs/diagnostico.md` §5, `docs/recetas.md` §5 | imagen privada, con dos salidas | según la decisión de la persona (propuesta) |

## 4. Decisión

Pendiente: la persona decide la visibilidad del paquete en `ghcr.io`.
