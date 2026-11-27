# Diagnóstico de LAZ-0011 (PAR-PTE-07, 2026-11-27)

## Índice

1. [Montaje](#1-montaje)
2. [Lado de la impresora emulada](#2-lado-de-la-impresora-emulada)
3. [Lado de Android](#3-lado-de-android)
4. [Causa](#4-causa)
5. [Comprobaciones aisladas](#5-comprobaciones-aisladas)
6. [Límites](#6-límites)

## 1. Montaje

- Emulador de Android 17 (API 37, google_apis x86_64), 1 CPU / 5,5 GB, con turno y puesto de emulador; netsim con captura HCI por equipo (`-netsim-args --pcap`).
- Imagen construida desde la rama con la traza HCI opcional (`Emulador__Bluetooth__TrazaHci=true`) y **sin** la corrección; contenedor con `--network container:<emulador>`, 1 CPU / 512 MB, en el mismo turno. Direcciones sintéticas `04:00:00:00:00:21`, `…:22` y `…:23`.
- Android ya tenía otros equipos «58HB6» vinculados de corridas anteriores (direcciones `…:00`, `…:01`, `…:02`, `…:05`, `…:06`, `…:07`).
- Vínculo desde Ajustes → Bluetooth → «Pair new device» → «58HB6» → «Pair», por interfaz con `uiautomator`; después la app de ejemplo de la librería cliente («Escanear», elegir, «Conectar»).
- Direcciones del teléfono enmascaradas (`XX:XX:XX:XX:BB:BB`; en la traza hexadecimal, `BBBBBB000002` es esa dirección en orden HCI).

## 2. Lado de la impresora emulada

Traza HCI propia (`HCI <` evento, `HCI >` comando) del vínculo desde Ajustes, dirección `…:21`:

```text
HCI < 3209BBBBBB000002010003                                  IO Capability Response: Android IO 0x01, autenticación 0x03 (dedicado, MITM)
HCI < 3106BBBBBB000002                                        IO Capability Request
HCI > 2B0409BBBBBB000002030004                                IO Capability Request Reply: sin E/S, bonding general
HCI < 330ABBBBBB0000021B000000                                User Confirmation Request
HCI > 2C0406BBBBBB000002                                      User Confirmation Request Reply
HCI < 360700BBBBBB000002                                      Simple Pairing Complete, estado 0
HCI < 1817BBBBBB000002<clave de 16 bytes>04                   Link Key Notification, tipo 4 (se guarda)
L2CAP: canal abierto PSM 0x0001 …                             SDP de Android
HCI < 050400000013                                            Disconnection Complete 0x000, motivo 0x13
```

En esa conexión no hubo Encryption Change. En la conexión siguiente (la app):

```text
pedido de conexión ACL de XX:XX:XX:XX:BB:BB; se acepta
conexión ACL 0x001 con XX:XX:XX:XX:BB:BB
L2CAP: canal abierto PSM 0x0001 CID 0x0040, MTU remoto 1024
```

y nada más: no llegó Link Key Request (Android no pidió autenticación) ni el canal L2CAP de RFCOMM (PSM 3). La pila propia cumplió su parte: contestó capacidades y confirmación, guardó la clave y la habría entregado (en el camino que funciona, la línea `seguridad: clave de enlace guardada para …` aparece justo después del SDP).

## 3. Lado de Android

Captura HCI del controlador de Android en netsim (segundos desde el arranque de la captura):

```text
238.406 EVT Connection Complete       handle 0x000, dirección …:21
238.496 CMD Authentication Requested  handle 0x000
238.506 EVT Link Key Request          …:21  →  238.509 CMD Link Key Request Negative Reply
238.596 EVT IO Capability Request     →  238.600 CMD IO Capability Request Reply (IO 0x01, autenticación 0x03)
238.676 EVT IO Capability Response    (impresora: IO 0x03, autenticación 0x04)
239.326 EVT User Confirmation Request →  245.772 CMD User Confirmation Request Reply
245.865 EVT Simple Pairing Complete   estado 0
245.976 EVT Link Key Notification     tipo 4
245.976 EVT Authentication Complete   estado 0, handle 0x000
251.162 CMD Disconnect                handle 0x000, motivo 0x13
370.265 CMD Create Connection         …:21   (la app)
371.295 EVT Connection Complete       handle 0x001
                                      (SDP; ni Authentication Requested ni Set Connection Encryption)
```

Registro de la pila de Bluetooth de Android (con `log.tag.bluetooth` en VERBOSE), en la conexión de la app:

```text
btm_sec.cc:4640 btm_sec_execute_procedure: No immediate action taken in busy state: le_link=tSECURITY_STATE::IDLE(0x0) classic_link=tSECURITY_STATE::AUTHENTICATING(0x1)
```

Y en el vínculo desde Ajustes (dirección `…:23`, con el registro `…:00` quitado de la configuración de Android):

```text
btm_sec.cc:3000 btm_sec_auth_complete: Security Manager: in state: WAIT_AUTH_COMPLETE, handle: 0, status: 0, dev->sec_rec.classic_link:tSECURITY_STATE::IDLE(0x0), bda: xx:xx:xx:xx:00:01, RName: 58HB6
btm_sec.cc:3951 btm_sec_disconnected: device:xx:xx:xx:xx:00:01 name:58HB6 state:WAIT_AUTH_COMPLETE reason:HCI_ERR_CONN_CAUSE_LOCAL_HOST flag:0x98 bond_type:BOND_TYPE_PERSISTENT sec_req:0x0
```

El Authentication Complete y el corte del manejador 0 se atribuyeron a `…:01` (otro equipo vinculado antes), no a `…:23`. Con `…:00` en la configuración, lo mismo ocurrió con `…:00` (dirección `…:22`).

## 4. Causa

El enlace queda sin cifrar por la pila de Android con netsim, no por la pila propia:

1. netsim da el manejador 0 a la primera conexión ACL después de encender el Bluetooth de Android (Connection Complete `0000…` en 238.406; la siguiente, `0001…`).
2. Android busca el equipo de cada evento por manejador y compara también el de LE. Los equipos que carga de su configuración nacen con ese manejador en 0. Fuente oficial, AOSP `packages/modules/Bluetooth`, rama `main` leída el 2026-11-27:
   - `system/stack/btm/btm_dev.cc`, `btm_sec_allocate_dev_rec`: «`p_dev_rec = static_cast<tBTM_SEC_DEV_REC*>(osi_calloc(sizeof(tBTM_SEC_DEV_REC)));`»; `BTM_SecAddDevice` («Caching new record from config file») sólo fija «`p_dev_rec->hci_handle = get_btm_client_interface().peer.BTM_GetHCIConnHandle(bd_addr, BT_TRANSPORT_BR_EDR);`».
   - mismo archivo, la comparación de `btm_find_dev_by_handle`: «`if (p_dev_rec->hci_handle == *handle || p_dev_rec->ble_hci_handle == *handle) {`».
   - URL: https://android.googlesource.com/platform/packages/modules/Bluetooth/+/refs/heads/main/system/stack/btm/btm_dev.cc
3. `btm_sec_auth_complete` sólo vuelve a reposo el equipo que encontró por manejador («`if (p_dev_rec->sec_rec.classic_link == tSECURITY_STATE::AUTHENTICATING) { p_dev_rec->sec_rec.classic_link = tSECURITY_STATE::IDLE;`»), y `btm_sec_execute_procedure` no hace nada mientras el estado no sea reposo («`No immediate action taken in busy state`»). URL: https://android.googlesource.com/platform/packages/modules/Bluetooth/+/refs/heads/main/system/stack/btm/btm_sec.cc
4. El equipo nuevo queda «AUTHENTICATING»: en la conexión siguiente Android no autentica ni cifra, y el RFCOMM vence (`read failed, socket might closed or timeout`).

Vincular conectando funcionó en PAR-PTE-05 y en esta parcela porque esa conexión no fue la primera después de encender el Bluetooth (manejador distinto de 0). El código de `main` puede no ser idéntico al de la imagen API 37; lo que se afirma de esa imagen sale de sus registros, que coinciden con ese código.

## 5. Comprobaciones aisladas

| Prueba | Resultado |
|---|---|
| Vínculo desde Ajustes como primera conexión, con otros equipos vinculados, sin corrección | `…:21` y `…:23`: 2 de 2 sin cifrado en la conexión de la app (`…:21` con el estado «AUTHENTICATING» en el registro); `…:22` y `…:23`: Authentication Complete atribuido a otro equipo (registro VERBOSE; en `…:22` no se probó la app) |
| Tras la conexión vencida (corte con manejador 0x001, atribuido bien), otra conexión de la app, sin corrección (`…:23`) | cifró y abrió RFCOMM (1 de 1) |
| Vínculo desde Ajustes con la corrección (ADR-0009), `…:31` y `…:33` | 2 de 2: corte de la conexión breve atribuido al equipo correcto (`btm_sec_disconnected: device:xx:xx:xx:xx:00:31 … state:IDLE`), la app cifró al primer intento |

## 6. Límites

- La versión exacta del código de Bluetooth de la imagen API 37 no se leyó: NO VERIFICADO que sea igual a `main`.
- No se midió con la 58HB6 física ni con un teléfono real (LAZ-0002).
- Vincular conectando como primera conexión después de encender el Bluetooth, con otros equipos vinculados: sin medir (LAZ-0016).
