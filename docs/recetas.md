---
doc_id: USO-08
doc_type: guia-practica
title: Recetas para pruebas automáticas y workflows
description: Patrones listos para usar el emulador en pruebas automáticas, guiones y workflows de CI, con y sin Bluetooth.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso]
---
# Recetas

## Índice

1. [Esperar a que el emulador esté listo](#1-esperar-a-que-el-emulador-esté-listo)
2. [Sin Bluetooth: fuente simulada](#2-sin-bluetooth-fuente-simulada)
3. [Prueba de extremo a extremo con una app](#3-prueba-de-extremo-a-extremo-con-una-app)
4. [Prueba en .NET con el cliente tipado](#4-prueba-en-net-con-el-cliente-tipado)
5. [Workflow de CI](#5-workflow-de-ci)

## 1. Esperar a que el emulador esté listo

Antes de imprimir: que la API responda y que el medio esté conectado (con Bluetooth, «Anunciando»).

<!-- ejemplo: ejecutable -->
```bash
for i in $(seq 1 30); do
  if curl -fsS "$BASE/api/estado" 2>/dev/null | grep -q '"medioConectado":true'; then
    echo "listo"; break
  fi
  [ "$i" -lt 30 ] || { echo "el emulador no quedó listo en 30 s" >&2; exit 1; }
  sleep 1
done
```

## 2. Sin Bluetooth: fuente simulada

Para probar un consumidor de la API (un visor, un paso de CI, un cliente) sin emulador de Android: sin `Emulador__Bluetooth__Netsim` y con `Emulador__Simulacion__IntervaloSegundos` mayor que 0, el host genera un ticket sintético de 384 puntos cada tantos segundos; `medio` es «simulado». Los ejemplos de esta documentación corren así en la CI (con el host en proceso, no con la imagen).

Con la imagen, fuera del montaje con el emulador de Android y para uso local:

```bash
docker run -d --rm --name impresora-simulada --cpus 1 --memory 512m -p 127.0.0.1:5080:5080 \
  -v <carpeta de salida>:/data -e Emulador__Simulacion__IntervaloSegundos=5 \
  ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:1.0.0
```

Con `-p 127.0.0.1:5080:5080` la API queda sólo en el loopback del equipo. La simulación con la imagen: NO VERIFICADO (la prueba la cubre con el host en proceso).

## 3. Prueba de extremo a extremo con una app

El orden que funcionó en PAR-PTE-05 (emulador de Android API 37, la app de ejemplo de la librería cliente):

1. Emulador de Android arriba; emulador de impresora en su red ([Montaje](montaje.md)); esperar «listo» (§1).
2. Instalar la app (`adb install`).
3. Vincular conectando y confirmar el diálogo de Android enseguida, por ejemplo con `adb shell uiautomator dump` y `adb shell input tap` ([Emparejar e imprimir](emparejar-e-imprimir.md) §2).
4. Por cada impresión: leer `ultimoId`, hacer imprimir a la app, esperar con `despuesDe` ([API](api.md) §3) y guardar el PNG y el `.bin`.
5. Comparar: la huella SHA-256 del PNG de la API coincidió con la del archivo de la carpeta de salida, 5 de 5.
6. Bajar el emulador de impresora junto con el emulador de Android ([Montaje](montaje.md) §7).

Para comparar contra un PNG esperado conviene fijar el contenido del ticket: en PAR-PTE-05 la huella cambió entre impresiones porque el ticket llevaba la hora.

## 4. Prueba en .NET con el cliente tipado

El método `ImprimirYObtenerPngAsync` de [API](api.md) §5, copiado en el proyecto de pruebas, sirve dentro de una prueba: `imprimir` dispara la impresión en la app (por ejemplo, un toque por `adb`) y el resultado es el PNG. En xUnit (ilustrativo):

```csharp
[Fact]
public async Task LaAppImprimeElTicket()
{
    var png = await ImprimirYObtenerPngAsync(
        new Uri("http://127.0.0.1:5080/"), () => TocarImprimirEnLaAppAsync());
    Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, png[..4]);
}
```

`TocarImprimirEnLaAppAsync` es de quien escribe la prueba. La prueba tiene que correr en la red del emulador ([Montaje](montaje.md) §4).

## 5. Workflow de CI

En un runner sin KVM no hay emulador de Android: el workflow prueba contra la fuente simulada (§2). Pasos:

1. Si la imagen es privada, `docker login ghcr.io` con un token de lectura de paquetes, o construirla desde la etiqueta ([Diagnóstico](diagnostico.md) §5).
2. Levantar la imagen con la simulación y el puerto en el loopback (§2).
3. Esperar «listo» (§1) con `BASE=http://127.0.0.1:5080`.
4. Correr las pruebas del consumidor; guardar los PNG como artefacto.
5. `docker stop` al final, también si las pruebas fallan (`if: always()` en GitHub Actions).

Un workflow que use el emulador de Android necesita un runner con KVM y Docker, y sigue §3. NO VERIFICADO en runners de GitHub.
