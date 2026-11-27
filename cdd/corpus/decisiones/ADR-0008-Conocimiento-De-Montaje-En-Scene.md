---
id: ADR-0008
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0008 · Conocimiento de montaje del emulador en Ng.IA-Context.Scene

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

La persona: «[…] cuando termines el mulador, el montaje del emulador lo podes plasmar en un conocimiento , revisa […] y ve la mejor forma de crear un conocimiento ahí que permita ser llamados por los otros para montar el emulador de forma limpia» (`Idea-Inicial.md` §7 A1). Uso previsto: pruebas internas y la scene donde se reproducen casos (intake §8, Q2:20). El intake firmado lo incluye en el producto (intake §4).

## 2. Opciones

- A. Un conocimiento de montaje en esa base, que monta la imagen de versión fija (ADR-0006) y remite a la documentación de uso del producto (ADR-0007) (reversible).
- B. Sólo instrucciones en el `README.md` del producto (no lo encuentran los otros agentes por su índice de conocimientos).

## 3. Decisión

A. Decidida por la persona en el pedido 7 y al firmar el intake 1.2 (2026-11-27). Lo escribe la parcela final de la etapa, con el emulador terminado («cuando termines», A1). El conocimiento vive en esa base (privada) y respeta su forma; desde los repositorios públicos sólo se lo nombra.

## 4. Consecuencias

- Épica de montaje y uso documentado (`vision.md` §4).
