---
id: ADR-0002
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0002 · Visor Blazor Interactive Server integrado al servicio, que consume la API por HTTP

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

Pedidos de la persona (`Idea-Inicial.md` §7): «podes agregarle un visor web en blazor interactive server para ver los tickets, sencillo solo un visor sin usuarios» (A3); «el visor blazor integrado al servicio de la api» (A4); «podes si queres que el visor llame a la api, es un punto de testeo y como ejemplo para hacer mas elevado el dia de mañana» (A6). Intake §4 y §8 (I:69, I:75, I:87).

## 2. Opciones

- A. Visor Blazor Interactive Server en el mismo proceso que la API, que la llama por HTTP como cualquier cliente (reversible: el visor puede separarse en otro proceso sin cambiar la API).
- B. Visor que lee el almacenamiento de tickets directamente, sin pasar por la API (descartada: A6 lo quiere como punto de prueba de la API).
- C. Visor en un servicio aparte (descartada por A4).

## 3. Decisión

A. Decidió la persona (K3) en el pedido 7 y al firmar el intake 1.2 (2026-11-27). El visor no tiene usuarios ni autenticación (intake §8, TS-7).

Rige `Estilo-Web` 0.4.0 (catálogo de IA.CDD, `b609a2a`): su condición de carga es «el lock declara `estilo: Estilo-Web` (producto con maqueta o panel web)», y el lock la declara porque el producto tiene interfaz web (`DEC.md` DEC-002). Hereda de `Template-HTML-Panel-De-Control` 2.0, según su cabecera. Rige también la marca `interfaz:` de `Rules-Decisiones.md` §7 para los requisitos cuyas rutas toquen `*.razor` o `wwwroot/**`.

### Conocimientos que no cargan

- `Panel-Estructura` (catálogo de IA.CDD): condición «el producto tiene un panel de administración con ingreso por cuenta (no alcanza con tener maqueta), o el lock declara este conocimiento en conocimientos:». Hecho que la incumple: el visor es «sin usuarios» (A3) y el lock no lo declara.

## 4. Consecuencias

- Épica de acceso de la visión (`vision.md` §4); CE-5.
- El cliente que usa el visor para llamar a la API se decide en ADR-0003.
