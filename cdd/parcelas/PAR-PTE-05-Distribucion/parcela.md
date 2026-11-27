---
id: PAR-PTE-05
titulo: Contenedor, release en ghcr.io y prueba de extremo a extremo en el emulador de Android API 37
version: 1.0
estado: lista
velocidad: completa
base_commit: 07d938ce85db406f4d06fd0fe1b2706a6ed07854
sprint: SPR-01
epica: REQ-EPI-Distribucion@v1
historias:
  - REQ-HIS-Imagen-Publicada@v1
lee:
  - VIS@v1
  - REQ-EPI-Distribucion@v1
  - ADR-0006@v1
  - REQ-HIS-Api-De-Tickets@v1
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v1
modifica:
  - REQ-HIS-Imagen-Publicada@v1
appetite: dos días
pendientes:
decisiones:
  - {id: D1, tema: "imagen base aspnet de versión fija y etiqueta semántica del release", clase: K1}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Una imagen de versión fija en `ghcr.io`, probada de punta a punta con una app de ejemplo de la librería cliente en el emulador de Android API 37.

## Escenarios

- Camino feliz: un release publica la imagen; con ella, la app de ejemplo imprime y la API entrega el PNG.
- No feliz: la prueba de extremo a extremo falla; el release no se publica.

## Requisitos EARS

- REQ-01 · Cuando se publica un release, el sistema deberá construir y publicar la imagen en `ghcr.io` con la versión del release.
  prueba: tests/PrintThermal.Emulator.Tests/Distribucion/ReleaseTests.cs · ElWorkflowDeReleasePublicaEnGhcr
- REQ-02 · Cuando la app de ejemplo imprime en el emulador de Android API 37, el sistema deberá entregar el PNG del ticket por la API.
  prueba: tests/PrintThermal.Emulator.Tests/Distribucion/ExtremoAExtremoTests.cs · LaAppDeEjemploImprimeYLaApiEntregaElPng

## Casos borde

- Etiqueta de release repetida; imagen sin la carpeta de salida montada.

## Fuera de alcance

- Conocimiento de montaje en el repositorio de escenas (REQ-HIS-Conocimiento-De-Montaje, sprint siguiente).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Distribucion/ReleaseTests.cs · ElWorkflowDeReleasePublicaEnGhcr
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Distribucion/ExtremoAExtremoTests.cs · LaAppDeEjemploImprimeYLaApiEntregaElPng

## Plan

El emulador de Android corre con su turno, en su contenedor propio, y el del emulador de impresora comparte su red.

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
