---
id: PAR-PTE-06
titulo: Documentación de uso como código para agentes
version: 1.1
estado: lista
velocidad: rapida
base_commit: 7192bcee59cb8703548799b2f27562a9eceb98b4
sprint: SPR-01
epica: REQ-EPI-Montaje-Y-Uso@v1
historias:
  - REQ-HIS-Documentacion-De-Uso@v1
lee:
  - VIS@v1
  - REQ-EPI-Montaje-Y-Uso@v1
  - ADR-0007@v1
  - REQ-HIS-Api-De-Tickets@v3
  - REQ-HIS-Imagen-Publicada@v2
  - REQ-HIS-Cliente-Tipado@v3
  - REQ-HIS-Visor-Web@v2
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v2
modifica:
  - REQ-HIS-Documentacion-De-Uso@v1
appetite: medio día
pendientes:
decisiones:
  - {id: D1, tema: "estructura de docs/ según el conocimiento de documentación como código declarado en consulta: del lock (mapa de carga de su §0: §2, §3 y §5.1; §6 para verificar), con una ficha de diseño documental en docs/", clase: K1}
  - {id: D2, tema: "los ejemplos de bash marcados como ejecutables corren en la CI con bash y curl contra el host en Kestrel con la fuente simulada; cada ruta citada debe estar en /openapi/v1.json; el ejemplo del cliente tipado es una región de un archivo de pruebas que compila y corre; las variables de docker run se contrastan con las claves de configuración del código", clase: K1}
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
- REQ-02 · Si una ruta de la API, una clave de configuración o el ejemplo del cliente tipado cambian en el código y no en `docs/`, entonces la prueba de la documentación deberá fallar.
  prueba: tests/PrintThermal.Emulator.Tests/Documentacion/DocumentacionTests.cs · LaDocumentacionSigueAlCodigo

## Casos borde

- Una ruta de la API que cambia y la documentación no.

## Fuera de alcance

- Integración con IA Layer (pedido 8).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Documentacion/DocumentacionTests.cs · LosEjemplosDeLaDocumentacionCorren
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Documentacion/DocumentacionTests.cs · LaDocumentacionSigueAlCodigo

## Plan

Después de PAR-PTE-05: documenta la imagen publicada (`v1.0.0`). Re-planificada sobre `main` `7192bce`: `lee` sube a las versiones vigentes (REQ-HIS-Api-De-Tickets v3, REQ-HIS-Imagen-Publicada v2) y suma el cliente, el visor y el emparejamiento, que la documentación describe; REQ-02 cubre el caso borde.

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
