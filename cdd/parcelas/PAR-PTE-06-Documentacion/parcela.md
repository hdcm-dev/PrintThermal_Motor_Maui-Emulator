---
id: PAR-PTE-06
titulo: Documentación de uso como código para agentes
version: 1.1
estado: en-curso
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
  - {que: "pasos de docs/ que la CI no corre y no se midieron con la imagen publicada 1.0.0 (docker run del montaje, archivo del puerto de netsim, simulación con la imagen, acceso desde el equipo, docker build desde la etiqueta, vínculo tras reiniciar)", registro: LAZ-0013}
  - {que: "recuperación de un vínculo trabado y confirmación del diálogo de Android en una prueba automática: docs/ los marca NO VERIFICADO", registro: LAZ-0011}
  - {que: "la imagen publicada 1.0.0 es privada: el docker pull del montaje necesita credenciales; la visibilidad la decide la persona", registro: LAZ-0014}
  - {que: "fin de un ticket sin corte con la conexión abierta: docs/ lo documenta como límite", registro: LAZ-0001}
decisiones:
  - {id: D1, tema: "estructura de docs/ según el conocimiento de documentación como código declarado en consulta: del lock (mapa de carga de su §0: §2, §3 y §5.1; §6 para verificar), con una ficha de diseño documental en docs/", clase: K1}
  - {id: D2, tema: "los ejemplos de bash marcados como ejecutables corren en la CI con bash y curl contra el host en Kestrel con la fuente simulada; cada ruta citada debe estar en /openapi/v1.json; el ejemplo del cliente tipado es una región de un archivo de pruebas que compila y corre; las variables de docker run se contrastan con las claves de configuración del código", clase: K1}
  - {id: D3, tema: "no se adopta la compuerta de origen del conocimiento (dg.py) ni los artefactos A-02 a A-13: la prueba xUnit de la solución, en la CI pública existente, es la verificación de docs/ (D-09 de la ficha)", clase: K1}
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

- Re-planificación previa en su propio PR (#6, merge `dd71ee5`): base en `main` `7192bce`, `lee` vigentes, REQ-02 y D1-D2.
- Versiones subidas: REQ-HIS-Documentacion-De-Uso v1 → v2 (historia y `modifica`; criterio de deriva; `rutas:` suma `tests/PrintThermal.Emulator.Tests/Documentacion/**`).
- Changelog: «PAR-PTE-06 · Documentación de uso como código…» en `cdd/corpus/CHANGELOG.md`. Sprint: fila cerrada en `SPR-01.md`.
- Entregado: `docs/` con nueve páginas (índice, qué es y límites, montaje, emparejar e imprimir, API, visor y Scalar, diagnóstico, recetas y la ficha de diseño documental del conocimiento declarado en `consulta:`), frontmatter mínimo del conocimiento (`USO-nn`, `FIC-01`) y enlace desde `README.md`. El contrato de la API no se repite: remite a `/openapi/v1.json` y `/scalar`.
- Evidencia (SDK 10.0.403 en contenedor, con turno en el equipo de cómputo, perfil liviano):
  - `evidencia/2026-11-27-rojo.txt`: las 2 pruebas nuevas en rojo sobre `e37a483` (sin `docs/`), por la causa esperada; 131 de 131 las demás.
  - `evidencia/2026-11-27-pruebas.txt` y `.trx`: verde de la solución sobre la cabeza.
  - `evidencia/2026-11-27-mutacion.txt`: mutantes de `docs/` (ruta, variable, ejemplo del cliente, mensaje de registro, ejemplo ejecutable, parámetro, versión de la imagen, enlace, ancla, rango, campo JSON, página) y del código con `docs/` sin cambiar (clave de configuración, ruta, región del ejemplo, valor por omisión, plazo por omisión).
- Decisiones: D1 a D3 son K1. La ficha deja D-09/D-10 decididas por el responsable de las decisiones técnicas del producto, por delegación de la persona.
- Divergencia corpus/código: ninguna. ADR-0007 pide `docs/` sin IA Layer: no se integró.
- Auditoría (`evidencia/2026-11-27-auditoria.md`): la ronda 1 refutó (verde ausente y claves de la prosa sin control, más catorce menores); cada hallazgo tiene respuesta en pruebas, texto, LAZ o NO VERIFICADO.
- NO VERIFICADO (en `docs/` y en LAZ-0013): los pasos con Docker y el emulador de Android que la CI no corre, con la imagen publicada `1.0.0` (privada al 2026-11-27, LAZ-0014); el esqueleto de workflow; la recuperación de un vínculo trabado (LAZ-0011).
- Alcance que no es pendiente: `docs/` describe lo medido con Android API 37, la única medida; el PIN `0000` del emparejamiento heredado no se ejercitó (Android usó SSP); la fidelidad contra la 58HB6 física sigue sin medir (heredado de PAR-PTE-02 a PAR-PTE-05). La prueba no ata la versión citada a una etiqueta de git (la CI clona sin etiquetas): la ata a la que declara `docs/README.md`.
- Fin de sprint: PAR-PTE-06 es la última parcela de SPR-01; la Review (K3) es de la persona.
