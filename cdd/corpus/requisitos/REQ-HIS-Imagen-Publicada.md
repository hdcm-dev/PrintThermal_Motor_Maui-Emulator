---
id: REQ-HIS-Imagen-Publicada
version: 1
epica: REQ-EPI-Distribucion
gobierno: parcela
tipo: referencia
rutas:
  - Dockerfile
  - .dockerignore
  - .github/workflows/release.yml
---
# REQ-HIS-Imagen-Publicada

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un release etiquetado construye la imagen del contenedor del emulador y la publica en `ghcr.io` con versión fija; esa imagen, unida al espacio de red del contenedor del emulador de Android (API 37), pasa una prueba de extremo a extremo con una app de ejemplo de la librería cliente (ADR-0006).

## 2. Criterios

- Dado un release, cuando termina su workflow, entonces `ghcr.io` tiene la imagen con la versión del release.
- Dada la imagen publicada y el emulador de Android API 37, cuando la app de ejemplo imprime un ticket, entonces la API entrega su PNG.

## 3. Trazas

- Sube de: REQ-EPI-Distribucion
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
