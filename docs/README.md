---
doc_id: USO-01
doc_type: indice
title: Documentación de uso del emulador de la impresora 58HB6
description: Punto de entrada para agentes y personas que montan el emulador, emparejan, imprimen y leen los tickets.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso]
---
# Documentación de uso

Escrita para un agente que no conoce el emulador: con estas páginas se levanta el contenedor, se empareja la app, se imprime y se obtiene el PNG de cada ticket sin leer el código. Vale para la versión `1.0.0` de la imagen.

## Índice

1. [Qué leer según la tarea](#1-qué-leer-según-la-tarea)
2. [Convenciones](#2-convenciones)
3. [Cómo se verifica esta documentación](#3-cómo-se-verifica-esta-documentación)

## 1. Qué leer según la tarea

| Tarea | Página |
|---|---|
| Saber qué hace el emulador y qué no | [Qué es y límites](que-es-y-limites.md) |
| Levantar el contenedor junto al emulador de Android | [Montaje](montaje.md) |
| Emparejar la app con «58HB6» e imprimir | [Emparejar e imprimir](emparejar-e-imprimir.md) |
| Leer tickets por HTTP (curl o el cliente tipado de .NET) | [API](api.md) |
| Mirar los tickets o explorar el contrato | [Visor y Scalar](visor-y-scalar.md) |
| Algo no anda | [Diagnóstico](diagnostico.md) |
| Usarlo en pruebas automáticas o en un workflow | [Recetas](recetas.md) |
| Saber cómo se diseñó esta documentación | [Ficha de diseño documental](diseno-documental.md) |

El contrato completo de la API (rutas, parámetros, esquemas y códigos) no se repite acá: lo publica el propio emulador en `/openapi/v1.json` y lo muestra en `/scalar`.

## 2. Convenciones

- `$BASE` es la dirección de la API del emulador, sin barra final; en el montaje previsto, `http://127.0.0.1:5080` dentro de la red del contenedor del emulador de Android ([Montaje](montaje.md) §4).
- `<entre ángulos>` es un valor que pone quien usa la página (nombre de un contenedor, una carpeta).
- «NO VERIFICADO» marca lo que no se midió; el resto sale del código de este repositorio o de la prueba de extremo a extremo de PAR-PTE-05 (`cdd/parcelas/PAR-PTE-05-Distribucion/evidencia/`).

## 3. Cómo se verifica esta documentación

La prueba `tests/PrintThermal.Emulator.Tests/Documentacion/DocumentacionTests.cs` corre en la CI de cada pull request:

- los bloques precedidos por `<!-- ejemplo: ejecutable -->` corren con bash y curl contra el host real, con la fuente simulada;
- cada ruta y cada parámetro citados tienen que estar en `/openapi/v1.json`, cada variable `Emulador__…` tiene que ser una clave de configuración del host, la imagen tiene que ser la que publica el release y el puerto el de la imagen;
- los mensajes de registro citados (`<!-- ejemplo: registro -->`) tienen que salir de una plantilla del código;
- el ejemplo del cliente tipado es una región de `tests/PrintThermal.Emulator.Tests/Documentacion/EjemplosDelCliente.cs`: compila con la solución y la prueba lo corre.

Lo que la CI no puede correr (Docker, el emulador de Android, el emparejamiento) se contrasta con el código y con la evidencia citada, y queda marcado donde no se midió.
