---
doc_id: USO-03
doc_type: operativa
title: Montaje del emulador con la imagen publicada
description: Cómo levantar la imagen de versión fija en la red del contenedor del emulador de Android, con su configuración.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso, REQ-HIS-Imagen-Publicada]
---
# Montaje

## Índice

1. [Qué hace falta](#1-qué-hace-falta)
2. [Imagen](#2-imagen)
3. [Levantar el contenedor](#3-levantar-el-contenedor)
4. [Puertos y direcciones](#4-puertos-y-direcciones)
5. [Configuración](#5-configuración)
6. [Carpeta de salida](#6-carpeta-de-salida)
7. [Bajarlo](#7-bajarlo)

## 1. Qué hace falta

- Docker en el mismo equipo que el emulador de Android.
- El emulador de Android corriendo **en un contenedor**, con netsim (el controlador Bluetooth virtual del emulador). El emulador de impresora se une a la red de ese contenedor; netsim escucha sólo en su loopback y no hace falta exponerlo.
- El puerto gRPC de netsim, que se lee dentro del contenedor del emulador de Android (§3).
- Una carpeta del equipo para los tickets, con ruta absoluta y escritura para el usuario de la imagen (§6).

## 2. Imagen

Cada etiqueta `v*` del repositorio publica `ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:<versión>` (workflow `release.yml`). Se usa siempre una versión fija, nunca `latest`:

```bash
docker pull ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:1.0.0
```

Al 2026-11-27 la imagen es privada: el pull sin credenciales responde «unauthorized». Qué hacer en ese caso: [Diagnóstico](diagnostico.md) §5. Esta página no se midió con la imagen publicada `1.0.0`: PAR-PTE-05 usó una imagen construida en local con el mismo `Dockerfile` (NO VERIFICADO con la publicada; LAZ-0013).

## 3. Levantar el contenedor

Primero, el puerto gRPC de netsim. Está en la línea `grpc.port=` de un archivo `.ini` de netsim dentro del contenedor del emulador de Android. En la prueba de PAR-PTE-03 era `/tmp/netsim.ini`; el nombre (`netsim.ini`, `netsimd.ini`) y la carpeta dependen de cómo arranca el emulador (NO VERIFICADO en otros montajes), así que conviene buscarlo:

```bash
EMU=<contenedor del emulador Android>
docker exec "$EMU" sh -c 'cat /tmp/*netsim*.ini /tmp/android-*/*netsim*.ini "$XDG_RUNTIME_DIR"/*netsim*.ini 2>/dev/null' | sed -n 's/^grpc\.port=//p'
```

Después, el emulador de impresora en la red de ese contenedor:

```bash
PUERTO_NETSIM=<puerto gRPC de netsim>
docker run -d --rm --name impresora-58hb6 --network container:"$EMU" \
  --cpus 1 --memory 512m -v <carpeta de salida>:/data \
  -e Emulador__Bluetooth__Netsim=http://127.0.0.1:"$PUERTO_NETSIM" \
  -e Emulador__Bluetooth__Direccion=04:00:00:00:00:01 \
  ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:1.0.0
```

- `--network container:` comparte la red del emulador de Android: el emulador de impresora llega a netsim por `127.0.0.1` y la API queda en el puerto 5080 de esa red.
- `--cpus 1 --memory 512m` alcanzó en la prueba de extremo a extremo de PAR-PTE-05, con la imagen local (el emulador de Android en 1 CPU / 5,5 GB).
- `<carpeta de salida>` va con ruta absoluta: con un nombre suelto, Docker crea un volumen con nombre y los tickets no quedan en la carpeta del equipo.
- `--rm`: el contenedor no queda detenido; los tickets y las claves sobreviven en la carpeta montada.

Comprobar que arrancó: en los registros (`docker logs impresora-58hb6`) aparece una línea como

<!-- ejemplo: registro -->
```text
anunciando «58HB6» en 04:00:00:00:00:01, canal RFCOMM 1
```

y `GET /api/estado` informa el medio «Bluetooth «58HB6» (Anunciando)» con `medioConectado: true` ([API](api.md) §2).

## 4. Puertos y direcciones

- La imagen escucha en `http://0.0.0.0:5080` **de la red del contenedor** y no publica puertos. Dentro de esa red, `$BASE` es `http://127.0.0.1:5080`.
- Para llegar desde otro contenedor, se une a la misma red, por ejemplo con una imagen que tenga curl:

```bash
docker run --rm --network container:"$EMU" <imagen con curl> curl -fsS http://127.0.0.1:5080/api/estado
```

- Desde el equipo, sin publicar puertos: por la dirección del contenedor del emulador de Android en su red de Docker (`docker inspect "$EMU"`), si es una red puente. NO VERIFICADO.
- Desde Android (Chrome del emulador): en PAR-PTE-05 el visor se abrió desde Chrome del emulador de Android, pero hizo falta un reenvío TCP temporal fuera del contenedor. El camino directo, NO VERIFICADO.

## 5. Configuración

Variables de entorno (el doble guion bajo separa secciones de la configuración de .NET):

| Variable | Qué hace | Por omisión |
|---|---|---|
| `Emulador__Bluetooth__Netsim` | URL del gRPC de netsim. Con ella corre el medio Bluetooth | sin medio Bluetooth |
| `Emulador__Bluetooth__Direccion` | Dirección Bluetooth fija de la impresora emulada | `04:00:00:00:00:01` |
| `Emulador__Bluetooth__ArchivoDeClaves` | Archivo de claves de emparejamiento | `claves-bluetooth.txt` en la carpeta de salida |
| `Emulador__Bluetooth__ReconexionTrasVinculoSinCifrar` | Tras un vínculo que terminó sin cifrar (con netsim, el de Ajustes cuando Android lo atribuye a otro equipo), una conexión breve con el teléfono ([Emparejar e imprimir](emparejar-e-imprimir.md) §3) | `true` |
| `Emulador__Bluetooth__TrazaHci` | Registra los comandos y eventos HCI de la impresora (`HCI >` y `HCI <`), sin datos ACL ni Number Of Completed Packets y con las claves de enlace tapadas (`<clave>`), para diagnosticar | `false` |
| `Emulador__Salida` | Carpeta de los tickets | `/data` en la imagen |
| `Emulador__Simulacion__IntervaloSegundos` | Sin netsim: un ticket simulado cada tantos segundos; `0` lo apaga | `0` |
| `Visor__UrlApi` | Dirección con la que el visor llama a la API | la del propio servidor |

- `Emulador__Bluetooth__Netsim` necesita `Emulador__Salida`; la imagen ya la trae.
- Una dirección Bluetooth distinta es, para Android, otro equipo. En las pruebas se cambió la dirección para repetir el emparejamiento desde cero; como arreglo de un vínculo trabado, NO VERIFICADO ([Diagnóstico](diagnostico.md) §3).
- Sin `Emulador__Bluetooth__Netsim` ni simulación, el host sirve la API y el visor sin medio («sin medio»).

## 6. Carpeta de salida

- Cada ticket queda como `{id}.bin` y `{id}.png`. El `id` es la hora UTC de llegada más un número de orden (por ejemplo `20261127T165932797-0002`): ordena por llegada.
- Las claves de emparejamiento quedan en `claves-bluetooth.txt`. Mientras la carpeta y la dirección sean las mismas, el vínculo con Android debería servir de nuevo al reiniciar el contenedor (NO VERIFICADO entre reinicios).
- La carpeta tiene que admitir escritura del usuario sin privilegios de la imagen (uid 1654). Por ejemplo, `install -d -m 0777 <carpeta de salida>`, o dueño uid 1654.

## 7. Bajarlo

El emulador de impresora se baja junto con el emulador de Android: no queda vivo solo.

```bash
docker stop impresora-58hb6
```

Con `--rm`, detenerlo lo borra. Si el contenedor del emulador de Android se detiene primero, el de la impresora pierde su red: hay que detenerlo igual.
