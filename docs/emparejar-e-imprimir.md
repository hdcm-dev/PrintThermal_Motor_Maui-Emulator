---
doc_id: USO-04
doc_type: guia-practica
title: Emparejar la app con la impresora emulada e imprimir
description: Cómo encuentra Android a «58HB6», las dos formas de vincularla (conectando o desde Ajustes) con la conexión cifrada y cómo llega cada ticket.
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
3. [Vincular desde Ajustes y después conectar](#3-vincular-desde-ajustes-y-después-conectar)
4. [Imprimir](#4-imprimir)
5. [Leer el ticket](#5-leer-el-ticket)

## 1. Encontrar «58HB6»

Con el contenedor arriba ([Montaje](montaje.md)), la impresora emulada aparece en la búsqueda de Bluetooth del emulador de Android como «58HB6», con la dirección configurada (`04:00:00:00:00:01` por omisión) y clase de dispositivo impresora. Publica el perfil de puerto serie (SPP, UUID `00001101-0000-1000-8000-00805F9B34FB`) en el canal RFCOMM 1.

## 2. Vincular conectando

Es uno de los dos caminos medidos: la app abre una conexión RFCOMM al perfil SPP de «58HB6» **sin vincular antes** (en Android, `createRfcommSocketToServiceRecord` con el UUID de SPP y `connect()`). Android pide la vinculación dentro de esa conexión y muestra su diálogo de confirmación; la impresora emulada acepta sola (no tiene teclado ni pantalla).

- Hay que confirmar el diálogo de Android antes de que venza la conexión. En PAR-PTE-07, sin confirmar, la app mínima informó `read failed, socket might closed or timeout` a los 35 s (2 de 2); confirmando a los pocos segundos, se vinculó y cifró. En PAR-PTE-03, un intento con el toque a los 31 s quedó trabado; la causa medida en PAR-PTE-07 es otra (§3).
- En una prueba automática, el diálogo se confirmó tocando el botón con `adb shell input tap` en las coordenadas que da `adb shell uiautomator dump`. El texto del botón depende del idioma del sistema.
- Si es la primera conexión después de encender el Bluetooth de Android y Android tiene otros equipos vinculados, el primer intento puede vencer aunque se confirme el diálogo (la causa está en §3); la impresora hace entonces la conexión breve de §3 y el intento siguiente cifra (medido 1 de 1 en PAR-PTE-07).
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

## 3. Vincular desde Ajustes y después conectar

Es el camino de Ajustes → Bluetooth → «Vincular dispositivo nuevo» (Ajustes usa `createBond()`), y después la app conecta por RFCOMM a un equipo ya vinculado. Una app que llama `createBond()` y conecta enseguida, antes de que Android corte la conexión del vínculo, no se midió (LAZ-0016). Medido en PAR-PTE-07 contra el emulador de Android API 37: vinculado desde Ajustes (por interfaz, con `uiautomator`) y después la app de ejemplo de la librería cliente conectó con cifrado e imprimió 5 de 5.

En este camino Android vincula en una conexión propia (vínculo «dedicado»), cifra esa conexión y la corta. Con netsim, la primera conexión después de encender el Bluetooth de Android lleva el manejador 0, y si Android tiene otros equipos vinculados atribuye el fin de esa vinculación a **otro** de ellos: no cifra y deja este «autenticando» (sin otros equipos vinculados, o con otro manejador, cifra y no pasa nada de esto). Para que eso no trabe la conexión siguiente, la impresora emulada hace una conexión breve con el teléfono y la corta enseguida; así Android cierra el estado del equipo correcto (LAZ-0011, ADR-0009). En el registro:

<!-- ejemplo: registro -->
```text
seguridad: XX:XX:XX:XX:BB:BB declara IO 0x01, autenticación 0x03
seguridad: confirmación pedida por XX:XX:XX:XX:BB:BB; se acepta
seguridad: vínculo guardado con XX:XX:XX:XX:BB:BB (tipo de clave 4)
vínculo con XX:XX:XX:XX:BB:BB sin cifrar; reconexión breve
reconexión breve 0x001 con XX:XX:XX:XX:BB:BB; se corta
seguridad: clave de enlace guardada para XX:XX:XX:XX:BB:BB
cifrado en 0x002: estado 0x00, activo 1
```

Las dos últimas líneas son de la conexión de la app: la impresora entrega la clave guardada y el enlace queda cifrado.

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
