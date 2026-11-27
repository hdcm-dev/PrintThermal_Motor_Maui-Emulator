# Extremo a extremo de PAR-PTE-05 (2026-11-27)

## Índice

1. [Montaje](#1-montaje)
2. [Emparejamiento](#2-emparejamiento)
3. [Cinco impresiones](#3-cinco-impresiones)
4. [Visor](#4-visor)
5. [Desvíos y límites](#5-desvíos-y-límites)

## 1. Montaje

- Emulador de Android 17 (API 37, google_apis x86_64) en su contenedor, con turno y puesto de emulador en el equipo del emulador: **1 CPU / 5,5 GB**.
- Imagen del emulador de impresora construida con el `Dockerfile` de la rama en el equipo de cómputo (con turno) y cargada en el equipo del emulador, desde el commit `5066fa3` de la rama (mismo `src/` que `735adef`): `sha256:aa5cd74678391c05280ab3b81c77ee9828bac403f7f20dcf18703c247dfe443d`, versión 1.0.0. Contenedor con `--network container:<emulador>`, **1 CPU / 512 MB**, `--rm`, dentro del mismo turno (total 2 CPU / 6 GB, D4). netsim no se expuso: la imagen llegó al gRPC por `127.0.0.1` de la red compartida.
- Configuración: `Emulador__Bluetooth__Netsim=http://127.0.0.1:<puerto gRPC>`, `Emulador__Bluetooth__Direccion=04:00:00:00:00:07` (sintética), carpeta de salida montada en `/data`.
- App: `MotorDsl.SampleApp` de la librería cliente (origin/main 940fa7f), compilada con turno en el equipo de cómputo para android-x64 (APK sha256 `134227baafe2cb4dca6cc516057705049935bdf3a8165203e7036a76778e37f0`). Usa `createRfcommSocketToServiceRecord(00001101-…)` y el motor DSL de la librería para generar el ESC/POS.
- `GET /api/estado` al arrancar: medio «Bluetooth «58HB6» (Anunciando)», conectado. `/openapi/v1.json` 200, `/scalar` 302 (redirige a la página).

## 2. Emparejamiento

La app de ejemplo sólo lista dispositivos ya vinculados, así que el vínculo se creó con la app mínima de la prueba corta, conectando por RFCOMM a un dispositivo sin vincular (Android pide la confirmación dentro de la conexión). Lado del emulador de impresora:

```text
anunciando «58HB6» en 04:00:00:00:00:07, canal RFCOMM 1
seguridad: XX:XX:XX:XX:BB:BB declara IO 0x01, autenticación 0x00
seguridad: confirmación pedida por XX:XX:XX:XX:BB:BB; se acepta
seguridad: emparejamiento con XX:XX:XX:XX:BB:BB terminó con estado 0x00
seguridad: vínculo guardado con XX:XX:XX:XX:BB:BB (tipo de clave 4)
cifrado en 0x000: estado 0x00, activo 1
RFCOMM: abierto DLCI 2 (conexión 1, créditos sí, N1 990)
conexión 1 abierta desde XX:XX:XX:XX:BB:BB (canal 1)
RFCOMM: cerrado DLCI 2 (conexión 1)
ticket 20261127T165729277-0001: 4104 bytes, 384x1118
conexión 1 cerrada
```

Ese primer ticket (4104 bytes, de la app mínima) se borró por la API antes de la serie.

## 3. Cinco impresiones

Con la app de ejemplo: «Escanear», elegir «58HB6 (04:00:00:00:00:07)», «Conectar» (una sola conexión RFCOMM, cifrada con la clave guardada) e «Imprimir Ticket» cinco veces. Por cada impresión, `GET /api/tickets/esperar?segundos=60&despuesDe=<anterior>`, después `{id}.png` y `{id}.bin` por la API, comparados con los archivos de la carpeta montada:

```text
impresion 1 http=200 id=20261127T165932797-0002 bytes=341 espera=8.029929601s png_api=aa0e480f60e9fa3fb7ad404fb71da913ea135af86d405734a50a47a2edb4aa17 png_carpeta=aa0e480f60e9fa3fb7ad404fb71da913ea135af86d405734a50a47a2edb4aa17 bin_api=43183a6d7fb3019231d7c254e7a16f099f3e5acad1fb4da7406b499e30701b44 bin_carpeta=43183a6d7fb3019231d7c254e7a16f099f3e5acad1fb4da7406b499e30701b44 IGUAL
impresion 2 http=200 id=20261127T165947737-0003 bytes=341 espera=7.427181124s png_api=aa0e480f60e9fa3fb7ad404fb71da913ea135af86d405734a50a47a2edb4aa17 png_carpeta=aa0e480f60e9fa3fb7ad404fb71da913ea135af86d405734a50a47a2edb4aa17 bin_api=43183a6d7fb3019231d7c254e7a16f099f3e5acad1fb4da7406b499e30701b44 bin_carpeta=43183a6d7fb3019231d7c254e7a16f099f3e5acad1fb4da7406b499e30701b44 IGUAL
impresion 3 http=200 id=20261127T170002866-0004 bytes=341 espera=7.535878373s png_api=aa0e480f60e9fa3fb7ad404fb71da913ea135af86d405734a50a47a2edb4aa17 png_carpeta=aa0e480f60e9fa3fb7ad404fb71da913ea135af86d405734a50a47a2edb4aa17 bin_api=43183a6d7fb3019231d7c254e7a16f099f3e5acad1fb4da7406b499e30701b44 bin_carpeta=43183a6d7fb3019231d7c254e7a16f099f3e5acad1fb4da7406b499e30701b44 IGUAL
impresion 4 http=200 id=20261127T170017607-0005 bytes=341 espera=7.267204470s png_api=f212c860a331d3b73147e62db4c321fa5fc1090d386c4b3af22146f7654e587b png_carpeta=f212c860a331d3b73147e62db4c321fa5fc1090d386c4b3af22146f7654e587b bin_api=c81b24506876bf192d3584b8014e8746c017b3762df84f292d3b1f1a73a5dc55 bin_carpeta=c81b24506876bf192d3584b8014e8746c017b3762df84f292d3b1f1a73a5dc55 IGUAL
impresion 5 http=200 id=20261127T170032186-0006 bytes=341 espera=7.128264889s png_api=f212c860a331d3b73147e62db4c321fa5fc1090d386c4b3af22146f7654e587b png_carpeta=f212c860a331d3b73147e62db4c321fa5fc1090d386c4b3af22146f7654e587b bin_api=c81b24506876bf192d3584b8014e8746c017b3762df84f292d3b1f1a73a5dc55 bin_carpeta=c81b24506876bf192d3584b8014e8746c017b3762df84f292d3b1f1a73a5dc55 IGUAL
```

- 5 de 5: el PNG que entrega la API tiene la misma huella SHA-256 que el guardado en la carpeta, y lo mismo el `.bin`.
- La app informó «Ticket impreso (341 bytes enviados)» y la API entregó 341 bytes en cada caso.
- Las huellas cambian entre la 3.ª y la 4.ª porque el ticket lleva la hora en minutos («27/11/2026 16:59» → «17:00»); dentro del mismo minuto, los cinco bytes y PNG coinciden.
- Lado del emulador de impresora: `ticket <id>: 341 bytes, 384x436` cinco veces, sin avisos del intérprete.
- Los cinco `.bin` quedaron como vectores (`tests/…/Distribucion/Captura/`) con la huella de su PNG: `LaAppDeEjemploImprimeYLaApiEntregaElPng` repite receptor → almacén → API y exige esa huella en la CI.

## 4. Visor

Capturas en `capturas/`: el visor en Chrome del propio emulador de Android (puerto 5080 de la dirección especial con la que el emulador de Android expone su red al invitado), con el medio «Anunciando, con conexión RFCOMM», 5 tickets y el último a escala; y el PNG de un ticket.

## 5. Desvíos y límites

- **Código medido:** la imagen salió de `5066fa3`. Después, por la auditoría, cambió `ReceptorAlAlmacen` (bytes tardíos, conexiones con id repetido, errores al guardar): esa versión final está cubierta por las pruebas y la mutación, no por esta corrida.

- **Navegador:** el navegador del servidor de herramientas del agente no llegó a la red de contenedores (`ERR_CONNECTION_REFUSED` también hacia su propia dirección local; sí salía a Internet). El visor se capturó con Chrome dentro del emulador de Android. Para eso se abrió un reenvío TCP temporal del visor en la puerta de enlace del puente de Docker durante unos minutos y se cortó; no tocó netsim.
- **LAZ-0011 reproducido:** vincular con la app mínima por `createBond` y después conectar dejó a Android con la seguridad en «AUTHENTICATING», sin pedir la clave al periférico y sin cifrado (dos veces, direcciones 05 y 06, con toques a los 24 s y a los 9 s). Vincular conectando (dirección 07) funcionó al primer intento. Causa sin separar; con el emulador en 1 CPU.
- **Respuestas a consultas:** la frontera `IReceptorDeBytes` no tiene canal de vuelta; la app de ejemplo no consulta estado, así que no se ejercitó.
- **Visor a 540 px de pantalla:** el ticket desborda a la derecha de su tarjeta (`capturas/visor-ticket.png`). Es de REQ-HIS-Visor-Web, fuera de esta parcela.
