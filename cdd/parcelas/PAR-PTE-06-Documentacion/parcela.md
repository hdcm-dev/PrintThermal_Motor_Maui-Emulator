---
id: PAR-PTE-06
titulo: Documentación de uso como código para agentes
version: 1.0
estado: lista
velocidad: rapida
base_commit: 07d938ce85db406f4d06fd0fe1b2706a6ed07854
sprint: SPR-01
epica: REQ-EPI-Montaje-Y-Uso@v1
historias:
  - REQ-HIS-Documentacion-De-Uso@v1
lee:
  - VIS@v1
  - REQ-EPI-Montaje-Y-Uso@v1
  - ADR-0007@v1
  - REQ-HIS-Api-De-Tickets@v1
  - REQ-HIS-Imagen-Publicada@v1
modifica:
  - REQ-HIS-Documentacion-De-Uso@v1
appetite: medio día
pendientes:
decisiones:
  - {id: D1, tema: "estructura de docs/ según el conocimiento de documentación como código declarado en consulta: del lock", clase: K1}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Que un agente monte y use el emulador leyendo `docs/`, sin leer el código.

## Escenarios

- Camino feliz: un agente sigue `docs/` y obtiene el PNG de un ticket.
- No feliz: un comando de la documentación ya no funciona; la prueba de la documentación falla.

## Requisitos EARS

- REQ-01 · El sistema deberá documentar en `docs/` el montaje, la API y el visor, con ejemplos que la CI ejecuta.
  prueba: tests/PrintThermal.Emulator.Tests/Documentacion/DocumentacionTests.cs · LosEjemplosDeLaDocumentacionCorren

## Casos borde

- Una ruta de la API que cambia y la documentación no.

## Fuera de alcance

- Integración con IA Layer (pedido 8).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Documentacion/DocumentacionTests.cs · LosEjemplosDeLaDocumentacionCorren

## Plan

Después de PAR-PTE-05: documenta la imagen publicada.

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
