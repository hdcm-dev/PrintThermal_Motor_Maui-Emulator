---
id: ADR-0005
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0005 · Un PNG por ticket como salida

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

La persona: «la salida puede simplemente una imagen bitmap como si fuera la foto del ticket» (`Idea-Inicial.md` §3). El intake firmado fija el formato: PNG (intake §4, R:12) y la medida CE-4, un PNG por ticket (intake §9).

## 2. Opciones

- A. Un PNG por ticket, sin pérdida (reversible: otro formato se agrega como salida extra).
- B. BMP sin comprimir (más grande, sin ventaja para comparar huellas).

## 3. Decisión

A. Decidida por la persona al firmar el intake 1.2 (2026-11-27). Fidelidad sin calibrar hasta medir la impresora física en la segunda etapa (intake §6, §10).

## 4. Consecuencias

- Épica del ticket en imagen (`vision.md` §4); CE-4.
