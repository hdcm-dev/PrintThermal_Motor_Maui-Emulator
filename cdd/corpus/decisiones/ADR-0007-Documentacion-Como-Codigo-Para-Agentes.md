---
id: ADR-0007
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0007 · Documentación de uso como código, para que los agentes la consulten

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

Pedido 8 de la persona, junto con la firma del intake (`Idea-Inicial.md` §8, literal): «Si querés incluirle un      docascode para que los agentes consulten su uso. Tenemos conocimientos de docasdoc pero sin integración con ia layer». No está en el intake 1.2 (es posterior a la firma); entra por el arranque.

El conocimiento existente es `DocsAsCode-Designing-Specification` 1.5 (IA.Knowledge.Base `fe51898`, medido el 2026-11-27). Condición de carga, literal: «al arrancar la documentación de un proyecto, al adoptar la documentación como código en un proyecto que ya tiene documentos y al revisar un diseño documental existente; se carga por secciones según el mapa de carga de §0». Se cumple: este arranque abre la documentación del producto. `Hereda-de: —`. Su §0 lo rotula «Borrador ensayado, no probado» y pide usarlo «como guía con criterio, no como procedimiento validado».

## 2. Opciones

- A. Declararlo en `consulta:` del lock y diseñar con él la documentación de uso (montaje del contenedor, API, visor) dentro del repositorio principal, versionada con el código, sin integración con IA Layer (reversible: se retira de `consulta:` con un ADR).
- B. Sólo `README.md` y la página Scalar, sin diseño documental (más simple; no da a los agentes un lugar estable para consultar el uso).
- C. Postergarlo a la segunda etapa.

## 3. Decisión

A. La persona dejó la elección al agente («Si querés…», 2026-11-27); el arranque la toma y la épica entra en la visión, que la persona aprueba (pregunta P4 de `cdd/testimonios/Vision/OUTPUTs/01-preguntas.md`). Si la persona elige B o C, este ADR pasa a `reemplazado` y el conocimiento se retira de `consulta:`.

Alcance de lo adoptado: el mapa de carga de su §0 («Diseñar desde cero…: §2, §3 y §5.1; §6 para verificar») en la parcela de documentación, no en este arranque. Sin integración con IA Layer, por la palabra de la persona. Donde el conocimiento choque con la norma, manda la norma (`cdd.py ancla`, «sustituye» no se admite): en particular, la forma del lock nombra la base de conocimientos por su nombre, y lo que va al repositorio público pasa el barrido de su §3.6.

## 4. Consecuencias

- Épica de montaje y uso documentado (`vision.md` §4).
- `cdd/cdd.lock.md`: `consulta: DocsAsCode-Designing-Specification@IA.Knowledge.Base 1.5 fe51898`.
