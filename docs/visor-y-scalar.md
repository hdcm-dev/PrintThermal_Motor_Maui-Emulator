---
doc_id: USO-06
doc_type: guia-practica
title: Visor web y Scalar
description: Dónde están el visor de tickets, la referencia Scalar y el documento OpenAPI, y qué muestra cada uno.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso, REQ-HIS-Visor-Web]
---
# Visor y Scalar

## Índice

1. [Visor](#1-visor)
2. [Scalar y OpenAPI](#2-scalar-y-openapi)

## 1. Visor

En `$BASE/` (la raíz, puerto 5080 de la red del emulador). Muestra el estado del medio y la cantidad de tickets, la lista de tickets recibidos y el elegido a escala del papel de 58 mm, con botones para descargar el PNG y los bytes recibidos, y «Limpiar» (pide confirmación y borra todos). Se actualiza solo cuando llega un ticket: no hace falta recargar.

- El visor lee todo por la misma API; no tiene datos propios.
- Para mirarlo hace falta un navegador que llegue a la red del contenedor ([Montaje](montaje.md) §4). En PAR-PTE-05 se abrió con Chrome dentro del emulador de Android.
- A 540 px de ancho de pantalla el ticket desborda su tarjeta (hallazgo abierto del visor).

## 2. Scalar y OpenAPI

- `/scalar`: referencia interactiva de la API (puede responder con una redirección a su página).
- `/openapi/v1.json`: el documento OpenAPI que genera el host. Es la fuente del contrato para generar clientes o validar respuestas.

<!-- ejemplo: ejecutable -->
```bash
curl -fsS -o openapi.json "$BASE/openapi/v1.json"
curl -fsSL -o /dev/null -w '%{http_code}\n' "$BASE/scalar"
curl -fsS -o /dev/null "$BASE/"
```
