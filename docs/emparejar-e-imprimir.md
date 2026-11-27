---
doc_id: USO-04
doc_type: guia-practica
title: Emparejar la app con la impresora emulada e imprimir
description: Cómo encuentra Android a «58HB6», cómo se vincula sin dejar la conexión sin cifrado y cómo llega cada ticket.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso, REQ-HIS-Descubrimiento-Y-Emparejamiento]
---
# Emparejar e imprimir

## Índice

1. [Encontrar «58HB6»](#1-encontrar-58hb6)
2. [Vincular conectando](#2-vincular-conectando)
3. [Lo que no funcionó: vincular y después conectar](#3-lo-que-no-funcionó-vincular-y-después-conectar)
4. [Imprimir](#4-imprimir)
5. [Leer el ticket](#5-leer-el-ticket)

## 1. Encontrar «58HB6»

Con el contenedor arriba ([Montaje](montaje.md)), la impresora emulada aparece en la búsqueda de Bluetooth del emulador de Android como «58HB6», con la dirección configurada (`04:00:00:00:00:01` por omisión) y clase de dispositivo impresora. Publica el perfil de puerto serie (SPP, UUID `00001101-0000-1000-8000-00805F9B34FB`) en el canal RFCOMM 1.

## 2. Vincular conectando

Es el camino medido: la app abre una conexión RFCOMM al perfil SPP de «58HB6» **sin vincular antes** (en Android, `createRfcommSocketToServiceRecord` con el UUID de SPP y `connect()`). Android pide la vinculación dentro de esa conexión y muestra su diálogo de confirmación; la impresora emulada acepta sola (no tiene teclado ni pantalla).

- Hay que confirmar el diálogo de Android enseguida. En la prueba que falló, el toque llegó a los 31 s (LAZ-0011).
- En una prueba automática, el diálogo se confirmó tocando el botón con `adb shell input tap` en las coordenadas que da `adb shell uiautomator dump`. El texto del botón depende del idioma del sistema.
- La clave queda guardada en `claves-bluetooth.txt` de la carpeta de salida; las conexiones siguientes la usan sin volver a preguntar.

Registros de un emparejamiento correcto (`docker logs`; la dirección del teléfono, enmascarada):

<!-- ejemplo: registro -->
```text
seguridad: XX:XX:XX:XX:BB:BB declara IO 0x01, autenticación 0x00
seguridad: confirmación pedida por XX:XX:XX:XX:BB:BB; se acepta
seguridad: emparejamiento con XX:XX:XX:XX:BB:BB terminó con estado 0x00
seguridad: vínculo guardado con XX:XX:XX:XX:BB:BB (tipo de clave 4)
cifrado en 0x000: estado 0x00, activo 1
conexión 1 abierta desde XX:XX:XX:XX:BB:BB (canal 1)
```

La línea `cifrado …: activo 1` es la que importa: sin ella, el vínculo no sirve.

## 3. Lo que no funcionó: vincular y después conectar

Vincular primero con `createBond()` y conectar después dejó a Android con la vinculación a medias («AUTHENTICATING»), sin pedir la clave a la impresora y **sin cifrado**: 2 de 2 intentos en PAR-PTE-05, con toques a los 24 s y a los 9 s. La causa no está separada (LAZ-0011, abierto). Hasta que se resuelva:

- vincular conectando (§2);
- si una app sólo lista equipos ya vinculados (como la app de ejemplo de la librería cliente), crear el vínculo antes con una app o un paso que conecte por RFCOMM a «58HB6» sin vincular, y después usar la app.

## 4. Imprimir

La app escribe ESC/POS en la conexión RFCOMM. Cada ticket se cierra con:

- el corte (`GS V`), o
- un `ESC @` que llega después de contenido, o
- el cierre de la conexión.

Si la app no corta ni cierra, el ticket queda abierto y no aparece en la API (LAZ-0001: el plazo para cerrar un ticket sin corte no está decidido). Una conexión puede llevar varios tickets.

Por cada ticket guardado, el registro muestra:

<!-- ejemplo: registro -->
```text
ticket 20261127T165932797-0002: 341 bytes, 384x436
```

## 5. Leer el ticket

Para no perder el ticket, se lee el último id **antes** de imprimir y se espera el siguiente: [API](api.md) §3. El PNG también queda en la carpeta de salida como `{id}.png`, y el visor lo muestra solo ([Visor y Scalar](visor-y-scalar.md)).
