# Extremo a extremo de PAR-PTE-07 (2026-11-27)

## Índice

1. [Montaje](#1-montaje)
2. [Camino (a): vincular desde Ajustes y después conectar](#2-camino-a-vincular-desde-ajustes-y-después-conectar)
3. [Camino (b): vincular conectando](#3-camino-b-vincular-conectando)
4. [Ronda 1 de la auditoría: imagen final y controles](#4-ronda-1-de-la-auditoría-imagen-final-y-controles)
5. [Desvíos y límites](#5-desvíos-y-límites)

## 1. Montaje

- Emulador de Android 17 (API 37, google_apis x86_64) en su contenedor, con turno y puesto de emulador: 1 CPU / 5,5 GB.
- Imagen construida con el `Dockerfile` de la rama en el equipo de cómputo (con turno) desde `c50f02b`, el commit previo al rebase sobre `main`, con el mismo `src/` y `Dockerfile` que `028d4aa` de la rama (después cambiaron `MedioBluetooth.cs`, que suma la opción, `docs/` y `cdd/`). Contenedor con `--network container:<emulador>`, 1 CPU / 512 MB, `--rm`, en el mismo turno; netsim no se expuso.
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

## 4. Ronda 1 de la auditoría: imagen final y controles

Imagen construida desde `1c6e700` (mismo `src/` y `Dockerfile` que la cabeza de las pruebas, `17bedd6`), con la opción en `MedioBluetooth.cs`, las claves tapadas en la traza y los cambios de la auditoría. Otro turno, el mismo emulador de Android. Registro de la pila de Bluetooth de Android en VERBOSE en todas las corridas.

| Corrida | Condición | Android (registro VERBOSE) | Impresora | Resultado |
|---|---|---|---|---|
| (a) final, `…:41` | Ajustes, primera conexión tras reiniciar el Bluetooth, otros equipos vinculados; corrección prendida (por omisión) | `btm_sec_auth_complete: … handle: 0 … bda: xx:xx:xx:xx:00:01`; corte de la breve: `btm_sec_disconnected: device:xx:xx:xx:xx:00:41 … state:IDLE` | vínculo sin cifrar → reconexión breve; la app cifró al primer intento | 5 de 5 IGUAL |
| S1, `…:42` | Ajustes, conexión que no es la primera (manejador 3), otros equipos vinculados; **corrección apagada** (`Emulador__Bluetooth__ReconexionTrasVinculoSinCifrar=false`) | `btm_sec_auth_complete: … handle: 3 … estado:AUTHENTICATING … bda: xx:xx:xx:xx:00:42`; `Start encryption` | `cifrado en 0x000: … activo 1` en la conexión del vínculo; sin reconexión breve | app: 1 de 1 IGUAL |
| S2, `…:43` | como S1 (manejador 5), corrección prendida | `btm_sec_auth_complete: … handle: 5 … bda: xx:xx:xx:xx:00:43` | cifró en la conexión del vínculo; sin reconexión breve | app mínima: 4104 bytes, SHA-256 enviado = `.bin` de la carpeta = `.bin` de la API |
| (b) final, `…:44` | vincular conectando (no es la primera conexión) | — | vínculo y cifrado en la misma conexión; sin reconexión breve | app: 5 de 5 IGUAL |
| (b) como primera conexión, `…:46` | vincular conectando como primera conexión tras reiniciar el Bluetooth, otros equipos vinculados (configuración restaurada), corrección prendida | `btm_sec_auth_complete: … handle: 0 … bda: xx:xx:xx:xx:00:02`; corte de la breve: `btm_sec_disconnected: device:xx:xx:xx:xx:00:46 … state:IDLE` | primer intento: vínculo guardado, sin cifrar, la app mínima venció (`read failed…`); reconexión breve; segundo intento: `clave de enlace guardada`, `cifrado en 0x003: … activo 1` | segundo intento: 4104 bytes, SHA-256 enviado = `.bin` de la carpeta |
| S3, `…:45` | Ajustes, primera conexión tras reiniciar el Bluetooth, **sin otros equipos vinculados** (configuración de vínculos de Android borrada antes, con copia en el emulador) | `btm_sec_auth_complete: … handle: 0 … estado:AUTHENTICATING … bda: xx:xx:xx:xx:00:45` | cifró en la conexión del vínculo; sin reconexión breve | app: 1 de 1 IGUAL |

- Vincular conectando como primera conexión con otros equipos vinculados (LAZ-0016) falla en el primer intento por la misma causa del lado de Android; la reconexión breve deja bien el segundo (1 de 1).
- En un intento previo con `…:46`, sin confirmar el diálogo y como primera conexión, Android atribuyó la falla de la vinculación (estado 5) a `…:01` y **borró ese otro vínculo** (`BOND 04:00:00:00:00:01 state=10` en el registro de la app mínima): es de la pila de Android, no de la impresora.
- Registros de la impresora de S2 y S3 (sin reconexión breve):

```text
S2: conexión ACL 0x000 con XX:XX:XX:XX:BB:BB · cifrado en 0x000: estado 0x00, activo 1 · desconexión ACL 0x000 de XX:XX:XX:XX:BB:BB, motivo 0x13
S3: conexión ACL 0x000 con XX:XX:XX:XX:BB:BB · seguridad: vínculo guardado con XX:XX:XX:XX:BB:BB (tipo de clave 4) · cifrado en 0x000: estado 0x00, activo 1 · desconexión ACL 0x000 de XX:XX:XX:XX:BB:BB, motivo 0x13
```

- Sin confirmar, registro de la app mínima con `…:32` (primera corrida): `19:12:25 BOND …:32 state=11` → `19:13:00 ERROR java.io.IOException: read failed, socket might closed or timeout, read ret: -1`; y `19:13:58 BOND …:32 state=11` → `19:14:32 ERROR …` (mismo mensaje).
- Los controles S1 y S3 confirman la causa: cuando Android atribuye el fin de la vinculación al equipo correcto (manejador distinto de 0, o ningún otro equipo cargado de la configuración), cifra en la misma conexión y la impresora no hace nada distinto de antes. La reconexión breve sólo se dispara cuando la vinculación termina sin cifrar.
- Sin confirmar el diálogo de Android: con `…:32` (primera corrida), la app mínima informó `read failed, socket might closed or timeout` a los 35 s, sin vínculo, 2 de 2 (registro arriba); con `…:44`, el primer intento terminó sin vínculo y la impresora registró el corte de esa conexión (el registro de la app de ese intento no se guardó).

(a) final, serie de cinco impresiones:

```text
impresion 1 id=20261127T210320285-0001 bytes=341 espera=6.216186542s png_api=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 png_carpeta=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 bin_api=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 bin_carpeta=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 IGUAL
impresion 2 id=20261127T210332118-0002 bytes=341 espera=5.317302435s png_api=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 png_carpeta=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 bin_api=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 bin_carpeta=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 IGUAL
impresion 3 id=20261127T210344088-0003 bytes=341 espera=5.390051139s png_api=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 png_carpeta=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 bin_api=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 bin_carpeta=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 IGUAL
impresion 4 id=20261127T210356098-0004 bytes=341 espera=5.469479361s png_api=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 png_carpeta=82f1ba127d8360dacab711ef47f1ea29ae3bbe330cd4c144b11c1a0b369d3361 bin_api=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 bin_carpeta=4f29546bd00887781f7facf250e5ef227a46ebbe8c81f9ebcb87e8ca894b7fa1 IGUAL
impresion 5 id=20261127T210407918-0005 bytes=341 espera=5.230493388s png_api=89522ffad899ffb5b1ebf8a11484c1422b1f5704ccbbc525ebe6bab0ac8ad0fa png_carpeta=89522ffad899ffb5b1ebf8a11484c1422b1f5704ccbbc525ebe6bab0ac8ad0fa bin_api=06af13078544e477b60bc82926de4fc02329ff840cb693cd48814b2265fd44d0 bin_carpeta=06af13078544e477b60bc82926de4fc02329ff840cb693cd48814b2265fd44d0 IGUAL
```

(b) final, serie de cinco impresiones:

```text
impresion 1 id=20261127T211918410-0002 bytes=341 espera=5.060773445s png_api=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 png_carpeta=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 bin_api=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f bin_carpeta=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f IGUAL
impresion 2 id=20261127T211929568-0003 bytes=341 espera=4.692278252s png_api=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 png_carpeta=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 bin_api=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f bin_carpeta=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f IGUAL
impresion 3 id=20261127T211940888-0004 bytes=341 espera=4.883337347s png_api=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 png_carpeta=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 bin_api=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f bin_carpeta=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f IGUAL
impresion 4 id=20261127T211952079-0005 bytes=341 espera=4.687221431s png_api=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 png_carpeta=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 bin_api=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f bin_carpeta=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f IGUAL
impresion 5 id=20261127T212003308-0006 bytes=341 espera=4.786342341s png_api=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 png_carpeta=ad1a78245a81d6499bbffbf6f19a3403fed09000e391d3294c18109bfd3e02f0 bin_api=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f bin_carpeta=208cc754a1463966864d791b32e93f6d2d94d4c36811c739d6166522ccbf049f IGUAL
```

S1 y S3:

```text
impresion 1 id=20261127T210648404-0001 bytes=341 espera=5.690713990s png_api=b5fb58bbf46ce3c7334f2e3199489e2c32a934d59a5125288d2525f44d2b55d0 png_carpeta=b5fb58bbf46ce3c7334f2e3199489e2c32a934d59a5125288d2525f44d2b55d0 bin_api=ef4ca0a40ca4505366c545e14e81df6ab71cf941cb0ec5c6cd089c8a77113f33 bin_carpeta=ef4ca0a40ca4505366c545e14e81df6ab71cf941cb0ec5c6cd089c8a77113f33 IGUAL
impresion 1 id=20261127T212241409-0001 bytes=341 espera=5.427125868s png_api=fce06331c34a8a924c34d9a67d66eba5ba2c8633421761e0bc36abb9363a5ea2 png_carpeta=fce06331c34a8a924c34d9a67d66eba5ba2c8633421761e0bc36abb9363a5ea2 bin_api=0be5a3460f03848042340ead012406f7195f9e35290cbdb8ae63dfffdd9f9bea bin_carpeta=0be5a3460f03848042340ead012406f7195f9e35290cbdb8ae63dfffdd9f9bea IGUAL
```

## 5. Desvíos y límites

- Camino (b) como primera conexión con otros equipos vinculados: 1 de 1 con el primer intento vencido y el segundo bien (§4).
- La imagen de esta corrida tiene el `src/` de `028d4aa`, sin la opción `ReconexionTrasVinculoSinCifrar` en `MedioBluetooth.cs` (entró después, por omisión `true`) ni los cambios de la auditoría.
- No se midió con la 58HB6 física ni con un teléfono real.
