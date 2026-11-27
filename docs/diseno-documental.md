---
doc_id: FIC-01
doc_type: ficha
title: Ficha de diseño documental del emulador
description: Decisiones con las que se diseñó la documentación de uso de docs/, según el conocimiento de documentación como código.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [desarrollo, agente]
traces: [REQ-HIS-Documentacion-De-Uso, ADR-0007]
---
# Ficha de diseño documental

Aplicación del conocimiento `DocsAsCode-Designing-Specification` 1.5 (`fe51898`, declarado en `consulta:` de `cdd/cdd.lock.md`; ADR-0007) con su mapa de carga «diseñar desde cero» (§2, §3 y §5.1; §6 para verificar). El conocimiento se rotula «borrador ensayado, no probado» y pide usarlo como guía con criterio: esta ficha sigue su numeración, en forma condensada, y anota lo que no se aplicó. Donde choca con la norma CDD, manda la norma.

| Campo | Valor |
|---|---|
| Proyecto y repositorio leído | este repositorio, rama de PAR-PTE-06 sobre `main` `dd71ee5` |
| Carpetas excluidas | `.git` |
| Fecha | 2026-11-27 |
| Quién aplica | agente de la parcela PAR-PTE-06 |
| Conocimiento aplicado | `DocsAsCode-Designing-Specification` 1.5, `fe51898` |

## Índice

1. [Inventario](#1-inventario)
2. [Decisiones](#2-decisiones)
3. [Catálogo activo](#3-catálogo-activo)
4. [Decisiones de clase H para una persona](#4-decisiones-de-clase-h-para-una-persona)
5. [Audiencias y entrega](#5-audiencias-y-entrega)
6. [Artefactos A-02 a A-13](#6-artefactos-a-02-a-a-13)
7. [Lo que el conocimiento no resolvió](#7-lo-que-el-conocimiento-no-resolvió)
8. [Verificación](#8-verificación)

## 1. Inventario

Comandos del bloque de §3.1 del conocimiento, corridos en el árbol de la rama el 2026-11-27.

| Patrón | Comando | Cuenta | Nota |
|---|---|---|---|
| ID declarado más de una vez | 1b (sin la compuerta de origen) | 5 | Aproximado: cortes del patrón en títulos del corpus CDD, no IDs repetidos; ninguno en `docs/` |
| Versión declarada dos veces | 2 | 0 | — |
| Estado «planificado» en la documentación técnica | 3 sobre `docs/` | 0 | Las coincidencias son «previsto» y «todos» en prosa |
| Traza de la interfaz a una unidad ajena | 4 | — | No hay ayuda al usuario final (CL-06 inactiva) |
| Puntero a un documento obsoleto | 5 | 0 | Sin CHANGELOG en la raíz; el del corpus vive en `cdd/corpus/` |
| Espacios de IDs en uso | 7 | REQ, PAR, LAZ, ADR, CE, TS, SPR, DEC… | Los de la norma CDD; ninguno choca con `USO` ni `FIC` |
| Plataformas | 8 | `net10.0` | Un solo destino; la CI corre en `ubuntu-latest` |
| Prefijos nuevos libres | 9 | `USO` 0, `FIC` 0 | Libres fuera de `docs/` |
| Barrido de lo privado | 12b sobre `docs/` y `README.md` | sólo loopback y `0.0.0.0` | Sin hosts, rutas del equipo ni direcciones internas |

## 2. Decisiones

| Id | Pregunta | Valor | Evidencia | Clase |
|---|---|---|---|---|
| D-01 | Escenario | Servicio con paquetes (imagen de contenedor y cliente .NET); sin sitio publicado; marco de especificación: CDD; topología: mismo repositorio | `README.md`, `cdd/cdd.lock.md` | P |
| D-02 | Clases activas | CL-01, CL-03, CL-05, CL-07, CL-13 (§3) | §3 | P |
| D-04 | Registro de IDs | El de la norma CDD para el corpus; `USO-nn` y `FIC-nn` para `docs/` | `cdd/corpus/` | P |
| D-05 | Dónde vive cada pieza | `docs/` del repositorio principal, versionada con el código | ADR-0007 | P |
| D-06 | Ancla | El commit; la página dice la versión de la imagen que documenta (`1.0.0`) | `docs/README.md` | P |
| D-08 | Frontmatter | El mínimo de §5.2 del conocimiento | cada página | P |
| D-09 | Cómo corre la compuerta | Una prueba xUnit de la solución, en la CI pública existente (`cdd.yml`); no se adopta la compuerta de origen (`dg.py`) | `tests/…/Documentacion/DocumentacionTests.cs` | H: decidida (§4) |
| D-10 | Qué bloquea | La prueba bloquea como cualquier otra de la solución | — | H: decidida (§4) |
| D-12 | Canales para agentes | Los mismos Markdown, desde `README.md`; sin índice para agentes ni IA Layer | ADR-0007 | H: decidida por la persona, «sin integración con ia layer» |
| D-14 | Qué es privado | Nada privado en `docs/`: el repositorio es público; hosts, rutas y nombres internos quedan fuera | barrido de §1 | P |
| D-16 | Idioma | Español, como el resto del producto | — | P |
| D-18 | Audiencias | Agente, desarrollo y QA; todo `publico` | §5 | P |

## 3. Catálogo activo

| Clase | Activa | Motivo | Dónde vive |
|---|---|---|---|
| CL-01 | sí | Esta ficha (A-01) y el corpus CDD | `docs/diseno-documental.md`, `cdd/corpus/` |
| CL-03 | sí | Ya existía | `AGENTS.md` |
| CL-05 | sí | Contrato OpenAPI emitido en ejecución por el host | `/openapi/v1.json`; la prueba lo levanta y lo contrasta con `docs/` |
| CL-07 | sí | Documentación técnica escrita del producto | `docs/` (no `docs/tecnica/`: es la única carpeta de documentación de uso) |
| CL-13 | sí | Ya existía | `README.md` (enlaza `docs/README.md`) |
| CL-04 | no | No se genera ni publica una referencia desde los comentarios del código | — |
| CL-02, CL-06, CL-08 a CL-12, CL-14 | no | Sin RRD aparte (el criterio vive en la historia), sin ayuda al usuario final, sin índice para agentes, sin cuerpo formativo, sin copia de conocimiento, sin sitio publicado | — |

## 4. Decisiones de clase H para una persona

| Id | Estado | Opciones | Default propuesto | Quién decide |
|---|---|---|---|---|
| D-09 / D-10 | decidida por el responsable de las decisiones técnicas del producto, por delegación de la persona (2026-11-27; el testimonio de la delegación no está en este repositorio). En CDD es la decisión D3 de PAR-PTE-06, K1 | Prueba xUnit en la CI existente / compuerta de origen con sus controles | Prueba xUnit | — |
| API `/openapi/v1.json` | decidida por la persona (exponer la API con OpenAPI y Scalar, ADR-0001) | — | — | — |
| D-12 | decidida por la persona: sin integración con IA Layer (ADR-0007 §1) | — | — | — |

## 5. Audiencias y entrega

Perfil mínimo. Las tres audiencias (agente, desarrollo, QA) leen las mismas páginas, de nivel `publico`, en el repositorio público: no hay acceso propio ni roles. Descartado: un nivel distinto para el agente, porque el agente lee con el acceso de quien lo usa.

## 6. Artefactos A-02 a A-13

No se producen: dependen de adoptar la compuerta de origen y su registro, que D-09 no adoptó. La prueba de la documentación cumple la parte verificable (enlaces, rutas, configuración, ejemplos que corren).

## 7. Lo que el conocimiento no resolvió

| Sección | Qué no resuelve | Qué quedó sin hacer |
|---|---|---|
| §0 | «Borrador ensayado, no probado» | Se aplicó condensado; los artefactos A-02 a A-13 no se produjeron |
| §3.1 | Los comandos suponen la compuerta de origen | El comando 1 no se corrió; se usó el 1b, aproximado |

## 8. Verificación

| Comprobación | Resultado |
|---|---|
| Prueba de la documentación (rojo, verde y mutantes de `docs/`) | Evidencia de PAR-PTE-06 en `cdd/parcelas/PAR-PTE-06-Documentacion/evidencia/` |
| Barrido de lo privado | §1, comando 12b |
| Par rojo/verde de la compuerta de origen | No aplica: no se adoptó (D-09) |
