---
id: ADR-0003
version: 1
gobierno: humano
estado: propuesto
rutas: []
---
# ADR-0003 · Cliente HTTP tipado y reutilizable para el visor

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

El visor llama a la API por HTTP (ADR-0002). El intake deja el cliente como «supuesto del agente, a decidir en la siembra» (intake §8). La persona quiere el visor como «ejemplo para hacer mas elevado el dia de mañana» (`Idea-Inicial.md` §7 A6): un cliente que otros consumidores (por ejemplo, las pruebas de extremo a extremo) puedan reutilizar sirve a ese fin. supuesto: que haya otros consumidores en .NET; no está medido.

## 2. Opciones

- A. Biblioteca de cliente tipado en un proyecto propio de la solución, que usan el visor y las pruebas (reversible).
- B. Llamadas HTTP sin tipar dentro del visor (más simple; no deja pieza reutilizable).
- C. Cliente generado desde el documento OpenAPI en cada compilación (agrega una dependencia de generación).

## 3. Decisión

Propuesta: A. Es K2 (contrato y dependencia entre piezas): la decide una mesa con ADR y la persona la aprueba en lote en la Review (`Rules-Decisiones.md` §2). No hubo mesa en el arranque; queda `propuesto` hasta que la convoque la parcela que construya el visor.

## 4. Consecuencias

- Si se acepta, un proyecto `*.csproj` más en la solución, con su requisito de contrato en la épica de acceso.
