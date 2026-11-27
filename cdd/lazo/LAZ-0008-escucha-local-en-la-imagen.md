---
id: LAZ-0008
fecha: 2026-11-27
origen: review
clase: aclaracion
estado: abierto
alcance: corpus
ids:
deuda:
normalizado:
parcela:
---
# LAZ-0008 · La imagen oficial de .NET fija el puerto y anula la escucha local por omisión

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

Auditor de PAR-PTE-04, 2026-11-27. El host escucha sólo en `http://127.0.0.1:5080` si no hay dirección configurada (D7, ADR-0001). La prueba está en `evidencia/2026-11-27-escucha.txt` de PAR-PTE-04.

Pero las imágenes `mcr.microsoft.com/dotnet/*` traen `ASPNETCORE_HTTP_PORTS=8080`. En un contenedor de esa familia, el host escucha en todas las interfaces del contenedor sin que nadie lo haya pedido: la misma evidencia muestra `ASPNETCORE_HTTP_PORTS de la imagen: 8080`.

Pregunta para PAR-PTE-05 (imagen): ¿la imagen fija la escucha de forma explícita (`ASPNETCORE_URLS` hacia la red compartida con el emulador de Android) y vacía `ASPNETCORE_HTTP_PORTS`, o el host deja de tomar `http_ports` como configuración explícita?

## 2. Clasificación

`aclaracion` provisoria; el 08 la reclasifica (puede ser trabajo de PAR-PTE-05).

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| | | |

## 4. Decisión
