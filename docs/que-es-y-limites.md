---
doc_id: USO-02
doc_type: guia-practica
title: Qué es el emulador y cuáles son sus límites
description: Qué reproduce el emulador de la 58HB6, con qué medio, y qué no reproduce del equipo físico.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso]
---
# Qué es y límites

## Índice

1. [Qué es](#1-qué-es)
2. [Qué reproduce](#2-qué-reproduce)
3. [Qué no reproduce](#3-qué-no-reproduce)
4. [Dónde aplica](#4-dónde-aplica)

## 1. Qué es

Un servicio .NET 10 en contenedor que se hace pasar por una impresora térmica Bluetooth ESC/POS de 58 mm, la 58HB6, para probar apps sin el equipo físico. Se conecta al controlador Bluetooth virtual del emulador de Android (netsim), se anuncia como «58HB6», recibe los bytes por RFCOMM, los interpreta y guarda cada ticket como `.bin` (lo recibido) y `.png` (la imagen). Los tickets se leen por una API HTTP, un visor web y un cliente tipado de .NET.

## 2. Qué reproduce

- Descubrimiento por Bluetooth Classic con el nombre «58HB6», clase de dispositivo impresora y el perfil de puerto serie (SPP) en el canal RFCOMM 1.
- Emparejamiento seguro simple (SSP) por confirmación: la impresora emulada acepta sola; Android muestra su diálogo de vinculación ([Emparejar e imprimir](emparejar-e-imprimir.md)).
- ESC/POS de 58 mm a 384 puntos de ancho: texto, estilos, alineación, imágenes, QR y EAN-13. Un ticket termina con el corte (`GS V`), con un `ESC @` después de contenido o al cerrarse la conexión.
- Varios tickets por conexión y varias conexiones seguidas.

## 3. Qué no reproduce

- **El PIN de la 58HB6 física.** El emulador empareja por confirmación; la app no ve el pedido de PIN del equipo real. Si Android pidiera el emparejamiento heredado, el emulador responde `0000` (NO VERIFICADO: no se ejercitó).
- **Lo propio del equipo físico:** papel, cabezal, temperatura, batería, botones, luces, sonido, velocidad de impresión y errores de hardware.
- **Respuestas a consultas** (`DLE EOT`, `GS I`): el intérprete las reconoce, pero la respuesta no vuelve a la app (la recepción no tiene canal de vuelta).
- **Fidelidad del dibujo:** el PNG sale de la fuente y las reglas del emulador. NO VERIFICADO contra lo que imprime la 58HB6 física.

## 4. Dónde aplica

- **Android:** sí, con el emulador de Android que trae netsim. Medido con Android 17 (API 37) en la prueba de extremo a extremo de PAR-PTE-05; otras API, NO VERIFICADO.
- **iOS:** no aplica. El medio es el controlador Bluetooth virtual del emulador de Android; no hay camino hacia un simulador de iOS.
- **Sin Bluetooth:** con la fuente simulada el host genera tickets solos, para probar consumidores de la API sin emulador de Android ([Recetas](recetas.md) §2).
