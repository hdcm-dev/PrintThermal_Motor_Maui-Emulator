---
doc_id: USO-07
doc_type: operativa
title: Diagnóstico del emulador
description: Qué mirar cuando no aparece «58HB6», no empareja, no llega el ticket o la imagen no se puede bajar.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso]
---
# Diagnóstico

Las dos fuentes: `GET /api/estado` ([API](api.md) §2) y los registros del contenedor (`docker logs impresora-58hb6`).

## Índice

1. [No arranca o no responde la API](#1-no-arranca-o-no-responde-la-api)
2. [No aparece «58HB6»](#2-no-aparece-58hb6)
3. [No empareja o el vínculo no sirve](#3-no-empareja-o-el-vínculo-no-sirve)
4. [No llega el ticket](#4-no-llega-el-ticket)
5. [La imagen es privada](#5-la-imagen-es-privada)

## 1. No arranca o no responde la API

- **El contenedor se cae al empezar:** `docker logs` del intento. Con `--rm` el contenedor desaparece; para leer el error, repetir sin `-d`.
- **Los tickets no aparecen en la carpeta del equipo:** `-v` necesita una ruta absoluta; con un nombre suelto, Docker crea un volumen con nombre.
- **No hay escritura en la carpeta de salida:** el usuario de la imagen (uid 1654) no puede escribir en la carpeta montada en `/data` ([Montaje](montaje.md) §6).
- **`curl` no conecta:** la API escucha en el puerto 5080 de la red del contenedor del emulador de Android, no en la del equipo: el pedido tiene que salir de esa red ([Montaje](montaje.md) §4).

## 2. No aparece «58HB6»

Mirar `medio` y `medioConectado` en `GET /api/estado`:

| `medio` | Qué pasa | Qué hacer |
|---|---|---|
| `Bluetooth «58HB6» (Anunciando)` | Se anuncia | Buscar de nuevo desde Android |
| `Bluetooth «58HB6» (Anunciando, con conexión RFCOMM)` | Hay una app conectada | — |
| `Bluetooth «58HB6» (Iniciando)` | Llegó a netsim y está preparando el controlador | Esperar unos segundos |
| `Bluetooth «58HB6» (Detenido)` | El servicio todavía no arrancó o se está bajando | Esperar; si sigue, ver los registros |
| `Bluetooth «58HB6» (Conectando)` o `(Error)` | No llega a netsim; reintenta cada 2 s | Revisar `Emulador__Bluetooth__Netsim`: `http://127.0.0.1:<puerto gRPC>` y el contenedor unido con `--network container:` al del emulador de Android |
| `sin medio` o `simulado` | Arrancó sin `Emulador__Bluetooth__Netsim` | Agregar la variable ([Montaje](montaje.md) §5) |

Cada reintento deja en el registro una línea con el motivo:

<!-- ejemplo: registro -->
```text
periférico: <motivo del error>; reintento en 2 s
```

Si el emulador de Android se reinició, netsim pudo cambiar de puerto: releerlo ([Montaje](montaje.md) §3) y volver a levantar el contenedor.

## 3. No empareja o el vínculo no sirve

- **Android vincula pero la conexión no cifra** (en el registro no aparece `cifrado …: activo 1`): si se vinculó desde Ajustes, el registro debe mostrar `reconexión breve` después del vínculo ([Emparejar e imprimir](emparejar-e-imprimir.md) §3). Si no la muestra, revisar que `Emulador__Bluetooth__ReconexionTrasVinculoSinCifrar` no esté en `false`. Si la confirmación tardó, la conexión pudo vencer: volver a conectar. Para ver cada evento HCI de la impresora, `Emulador__Bluetooth__TrazaHci=true` ([Montaje](montaje.md) §5).
- **Recuperar un vínculo trabado:** una conexión que Android corta con el manejador correcto lo destraba: en PAR-PTE-07, después de una conexión vencida, la siguiente cifró e imprimió (1 de 1, sin la reconexión breve). Olvidar el equipo en Android, usar otra `Emulador__Bluetooth__Direccion` o borrar `claves-bluetooth.txt`: NO VERIFICADO como arreglo.
- **Android no muestra el diálogo:** comprobar que la app conecta por RFCOMM a «58HB6» (§2 de la página de emparejamiento) y que la impresora se anuncia (§2 de esta página).

## 4. No llega el ticket

En orden:

1. **¿Hubo conexión?** El registro dice `conexión N abierta desde …`. Si no, la app no conectó: ver §3.
2. **¿Se cerró el ticket?** Un ticket se guarda con el corte (`GS V`), un `ESC @` después de contenido o al cerrarse la conexión. Si la app no corta ni cierra, queda abierto (LAZ-0001).
3. **¿Se guardó?** El registro dice `ticket <id>: N bytes, AxB` y el ticket aparece en `GET /api/tickets`.
4. **¿Se esperó bien?** Con `esperar` sin `despuesDe`, o con un `despuesDe` leído después de imprimir, el ticket ya estaba y no cuenta: leer `ultimoId` antes de imprimir ([API](api.md) §3). Un `204` es plazo vencido, no error.
5. **¿Lo borró otro?** `DELETE /api/tickets` y «Limpiar» del visor borran para todos los que usan el mismo emulador.

Los avisos del intérprete (comandos desconocidos, recortes, cortes sueltos) salen en el registro con el número de conexión, por ejemplo:

<!-- ejemplo: registro -->
```text
conexión 1: corte 1D 56 00 sin ticket abierto: se ignora
```

## 5. La imagen es privada

Al 2026-11-27 la imagen de `ghcr.io` es privada: un `docker pull` sin credenciales responde «unauthorized». Dos salidas:

- Iniciar sesión en `ghcr.io` con una cuenta que tenga acceso al paquete y un token con permiso de lectura de paquetes (`read:packages`), y repetir el pull de la versión fija.
- Construir la imagen desde la etiqueta del repositorio público, con la misma versión:

```bash
git clone --depth 1 --branch v1.0.0 https://github.com/hdcm-dev/PrintThermal_Motor_Maui-Emulator.git
docker build --build-arg VERSION=1.0.0 -t ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:1.0.0 PrintThermal_Motor_Maui-Emulator
```

La imagen construida así no es byte a byte la publicada (NO VERIFICADO); sale del mismo `Dockerfile` y del mismo commit.
