# Extremo a extremo de PAR-PTE-07 (2026-11-27)

## Índice

1. [Montaje](#1-montaje)
2. [Camino (a): vincular desde Ajustes y después conectar](#2-camino-a-vincular-desde-ajustes-y-después-conectar)
3. [Camino (b): vincular conectando](#3-camino-b-vincular-conectando)
4. [Desvíos y límites](#4-desvíos-y-límites)

## 1. Montaje

- Emulador de Android 17 (API 37, google_apis x86_64) en su contenedor, con turno y puesto de emulador: 1 CPU / 5,5 GB.
- Imagen construida con el `Dockerfile` de la rama en el equipo de cómputo (con turno) desde `c50f02b`, el commit previo al rebase sobre `main` con el mismo `src/` y `Dockerfile` que `86b7de9` (después cambiaron `MedioBluetooth.cs`, que suma la opción, `docs/` y `cdd/`). Contenedor con `--network container:<emulador>`, 1 CPU / 512 MB, `--rm`, en el mismo turno; netsim no se expuso.
- App: `MotorDsl.SampleApp` de la librería cliente, el mismo APK de PAR-PTE-05 (sha256 `134227ba…37f0`).
- Android con otros equipos «58HB6» vinculados de corridas anteriores, y el Bluetooth de Android recién reiniciado antes de cada vínculo desde Ajustes: la condición que hacía fallar el camino (a) (diagnóstico §4).
- Por cada impresión: «Imprimir Ticket» en la app, `GET /api/tickets/esperar?segundos=60&despuesDe=<anterior>`, después `{id}.png` y `{id}.bin` por la API, comparados por SHA-256 con los archivos de la carpeta montada.

## 2. Camino (a): vincular desde Ajustes y después conectar

Ajustes → Bluetooth → «Pair new device» → «58HB6» → «Pair» (por interfaz con `uiautomator`), y después la app: «Escanear», elegir «58HB6 (04:00:00:00:00:31)», «Conectar» (una conexión RFCOMM) e «Imprimir Ticket». Lado de la impresora:

```text
anunciando «58HB6» en 04:00:00:00:00:31, canal RFCOMM 1
pedido de conexión ACL de XX:XX:XX:XX:BB:BB; se acepta
conexión ACL 0x000 con XX:XX:XX:XX:BB:BB
seguridad: XX:XX:XX:XX:BB:BB declara IO 0x01, autenticación 0x03
seguridad: confirmación pedida por XX:XX:XX:XX:BB:BB; se acepta
seguridad: emparejamiento con XX:XX:XX:XX:BB:BB terminó con estado 0x00
seguridad: vínculo guardado con XX:XX:XX:XX:BB:BB (tipo de clave 4)
L2CAP: canal abierto PSM 0x0001 CID 0x0040, MTU remoto 1024
L2CAP: canal abierto PSM 0x0001 CID 0x0041, MTU remoto 1024
desconexión ACL 0x000 de XX:XX:XX:XX:BB:BB, motivo 0x13
vínculo con XX:XX:XX:XX:BB:BB sin cifrar; reconexión breve
reconexión breve 0x001 con XX:XX:XX:XX:BB:BB; se corta
pedido de conexión ACL de XX:XX:XX:XX:BB:BB; se acepta
conexión ACL 0x002 con XX:XX:XX:XX:BB:BB
L2CAP: canal abierto PSM 0x0001 CID 0x0040, MTU remoto 1024
seguridad: clave de enlace guardada para XX:XX:XX:XX:BB:BB
cifrado en 0x002: estado 0x00, activo 1
L2CAP: canal abierto PSM 0x0003 CID 0x0041, MTU remoto 1691
RFCOMM: abierto DLCI 2 (conexión 1, créditos sí, N1 990)
conexión 1 abierta desde XX:XX:XX:XX:BB:BB (canal 1)
ticket 20261127T190458491-0001: 341 bytes, 384x436
```

Serie de cinco impresiones (después de una primera, `…190458491-0001`, también IGUAL):

```text
impresion 1 id=20261127T190946403-0002 bytes=341 espera=5.089429821s png_api=32daf7f40f16564e850466c2642c8497e00abdd9caba0728bbb32a950abfd4ba png_carpeta=32daf7f40f16564e850466c2642c8497e00abdd9caba0728bbb32a950abfd4ba bin_api=417510122a1ec20ff3a78774398b5fa99644de0334c8e219cd23fe89f87fbf27 bin_carpeta=417510122a1ec20ff3a78774398b5fa99644de0334c8e219cd23fe89f87fbf27 IGUAL
impresion 2 id=20261127T190957793-0003 bytes=341 espera=4.928045415s png_api=32daf7f40f16564e850466c2642c8497e00abdd9caba0728bbb32a950abfd4ba png_carpeta=32daf7f40f16564e850466c2642c8497e00abdd9caba0728bbb32a950abfd4ba bin_api=417510122a1ec20ff3a78774398b5fa99644de0334c8e219cd23fe89f87fbf27 bin_carpeta=417510122a1ec20ff3a78774398b5fa99644de0334c8e219cd23fe89f87fbf27 IGUAL
impresion 3 id=20261127T191009413-0004 bytes=341 espera=4.802379261s png_api=9c20d7e69a36cb5d46ed7b1309200ccd74152aa6626cae3650cc4dff4c4cdf6a png_carpeta=9c20d7e69a36cb5d46ed7b1309200ccd74152aa6626cae3650cc4dff4c4cdf6a bin_api=57741e994894d1058e5aa11868423614aad426d2c5b97c835b610d2ab6608da2 bin_carpeta=57741e994894d1058e5aa11868423614aad426d2c5b97c835b610d2ab6608da2 IGUAL
impresion 4 id=20261127T191020563-0005 bytes=341 espera=4.716959596s png_api=9c20d7e69a36cb5d46ed7b1309200ccd74152aa6626cae3650cc4dff4c4cdf6a png_carpeta=9c20d7e69a36cb5d46ed7b1309200ccd74152aa6626cae3650cc4dff4c4cdf6a bin_api=57741e994894d1058e5aa11868423614aad426d2c5b97c835b610d2ab6608da2 bin_carpeta=57741e994894d1058e5aa11868423614aad426d2c5b97c835b610d2ab6608da2 IGUAL
impresion 5 id=20261127T191031793-0006 bytes=341 espera=4.807725817s png_api=9c20d7e69a36cb5d46ed7b1309200ccd74152aa6626cae3650cc4dff4c4cdf6a png_carpeta=9c20d7e69a36cb5d46ed7b1309200ccd74152aa6626cae3650cc4dff4c4cdf6a bin_api=57741e994894d1058e5aa11868423614aad426d2c5b97c835b610d2ab6608da2 bin_carpeta=57741e994894d1058e5aa11868423614aad426d2c5b97c835b610d2ab6608da2 IGUAL
```

- 5 de 5: misma huella del PNG y del `.bin` en la API y en la carpeta; la app informó 341 bytes y la API entregó 341.
- La huella cambia entre la 2.ª y la 3.ª porque el ticket lleva la hora en minutos.
- Repetido con la dirección `…:33` (Bluetooth de Android reiniciado otra vez): vínculo desde Ajustes, reconexión breve, la app cifró al primer intento e imprimió (1 de 1, IGUAL):

```text
impresion 1 id=20261127T193121999-0001 bytes=341 espera=5.219663025s png_api=add2073129097b62d15a81e41a51cd2163510753916c8de5c56d08f086843665 png_carpeta=add2073129097b62d15a81e41a51cd2163510753916c8de5c56d08f086843665 bin_api=afcfc139c5cce75009eca1740e42fbeee7b178e769fb1935a190c632b1c5f722 bin_carpeta=afcfc139c5cce75009eca1740e42fbeee7b178e769fb1935a190c632b1c5f722 IGUAL
```

## 3. Camino (b): vincular conectando

La app mínima de la prueba corta conecta por RFCOMM a «58HB6» (`…:32`) sin vincular; Android pide la confirmación dentro de la conexión (notificación «Pairing request» → «Pair & connect» → «Pair»). Se vinculó y cifró en la misma conexión, y la impresora **no** hizo reconexión breve. Después la app de ejemplo conectó con la clave guardada:

```text
conexión ACL 0x002 con XX:XX:XX:XX:BB:BB
L2CAP: canal abierto PSM 0x0001 CID 0x0040, MTU remoto 1024
seguridad: XX:XX:XX:XX:BB:BB declara IO 0x01, autenticación 0x00
seguridad: confirmación pedida por XX:XX:XX:XX:BB:BB; se acepta
seguridad: emparejamiento con XX:XX:XX:XX:BB:BB terminó con estado 0x00
seguridad: vínculo guardado con XX:XX:XX:XX:BB:BB (tipo de clave 4)
cifrado en 0x002: estado 0x00, activo 1
L2CAP: canal abierto PSM 0x0001 CID 0x0041, MTU remoto 1024
L2CAP: canal abierto PSM 0x0003 CID 0x0042, MTU remoto 1691
L2CAP: canal abierto PSM 0x0001 CID 0x0043, MTU remoto 1024
RFCOMM: abierto DLCI 2 (conexión 1, créditos sí, N1 990)
conexión 1 abierta desde XX:XX:XX:XX:BB:BB (canal 1)
RFCOMM: cerrado DLCI 2 (conexión 1)
ticket 20261127T191515457-0001: 4104 bytes, 384x1118
conexión 1 cerrada
desconexión ACL 0x002 de XX:XX:XX:XX:BB:BB, motivo 0x13
pedido de conexión ACL de XX:XX:XX:XX:BB:BB; se acepta
conexión ACL 0x003 con XX:XX:XX:XX:BB:BB
L2CAP: canal abierto PSM 0x0001 CID 0x0040, MTU remoto 1024
seguridad: clave de enlace guardada para XX:XX:XX:XX:BB:BB
cifrado en 0x003: estado 0x00, activo 1
L2CAP: canal abierto PSM 0x0003 CID 0x0041, MTU remoto 1691
RFCOMM: abierto DLCI 2 (conexión 2, créditos sí, N1 990)
conexión 2 abierta desde XX:XX:XX:XX:BB:BB (canal 1)
```

Serie de cinco impresiones:

```text
impresion 1 id=20261127T192811382-0005 bytes=341 espera=4.696604119s png_api=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 png_carpeta=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 bin_api=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 bin_carpeta=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 IGUAL
impresion 2 id=20261127T192822693-0006 bytes=341 espera=4.859341487s png_api=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 png_carpeta=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 bin_api=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 bin_carpeta=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 IGUAL
impresion 3 id=20261127T192833874-0007 bytes=341 espera=4.682573664s png_api=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 png_carpeta=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 bin_api=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 bin_carpeta=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 IGUAL
impresion 4 id=20261127T192845293-0008 bytes=341 espera=4.993922282s png_api=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 png_carpeta=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 bin_api=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 bin_carpeta=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 IGUAL
impresion 5 id=20261127T192856403-0009 bytes=341 espera=4.616437516s png_api=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 png_carpeta=2ad930bbb5a0d0155d846f0ae1529b6413ba9875ecc9bdb6e90159808feea851 bin_api=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 bin_carpeta=c0ea69ee9c4e2935fe4d15dd4a5b714d6f64db2a70b868f98146412b14297d83 IGUAL
```

- 5 de 5 IGUAL. Antes de esta serie hubo otra con dos toques fallidos de la automatización de la interfaz (no encontró el botón «Imprimir Ticket» en pantalla: no hubo impresión) y tres impresiones IGUAL.

## 4. Desvíos y límites

- Camino (b) medido en una conexión que no fue la primera después de encender el Bluetooth de Android (como en PAR-PTE-05). Como primera conexión con otros equipos vinculados: sin medir (LAZ-0016).
- La imagen de la corrida tiene el `src/` de `86b7de9`; la cabeza agrega la opción `ReconexionTrasVinculoSinCifrar` en `MedioBluetooth.cs` (por omisión `true`, el mismo comportamiento), cubierta por las pruebas.
- No se midió con la 58HB6 física ni con un teléfono real.
