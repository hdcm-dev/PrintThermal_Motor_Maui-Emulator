---
id: ADR-0001
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0001 · Servicio .NET 10 con API HTTP de tickets, OpenAPI y Scalar

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

La persona preguntó cómo se accedería a la imagen («le vas a generar algo server html o un endpoint para pillar la imagen», `Idea-Inicial.md` §7 A2) y pidió Scalar sobre esa API («y sumale scalar , antes era swagger a esa api», A5). El normalizado firmado lo recoge: API HTTP de tickets con OpenAPI y página Scalar, sin autenticación y publicada sólo en la red del contenedor de pruebas (intake §4, §8; TS-7 respondida en Q2:20). Producto en .NET 10 (intake §6, I:21).

## 2. Opciones

- A. API HTTP de tickets en .NET 10 con documento OpenAPI y página Scalar, sin autenticación, expuesta sólo en la red de pruebas (reversible: agregar autenticación más adelante no rompe el contrato de lectura).
- B. Servir sólo archivos PNG desde una carpeta compartida (sin contrato, descartada por A2 y A5).
- C. API con autenticación (descartada por la persona en la ronda 02, P4 A).

## 3. Decisión

A. Decidió la persona (K3) al firmar el intake 1.2 el 2026-11-27 (`PROMPTs/intake.md` §14; testimonio `PROMPTs/00-Fase-Previa/01-Relevamiento/OUTPUTs/03-respuestas.md`). Lo concreto del contrato (rutas, esquema, códigos) es K2 de las parcelas de la épica de acceso.

## 4. Consecuencias

- Épica de acceso de la visión (`vision.md` §4) y criterio CE-5.
- Riesgo anotado en intake §10: si la API se abre fuera de la red de pruebas, vuelve el riesgo de no tener autenticación; eso es una K3 nueva.
