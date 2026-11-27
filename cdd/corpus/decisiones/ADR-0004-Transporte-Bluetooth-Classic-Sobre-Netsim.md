---
id: ADR-0004
version: 1
gobierno: humano
estado: propuesto
rutas: []
---
# ADR-0004 · Transporte Bluetooth Classic propio en C# sobre el gRPC de netsim, con dirección fija

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

- La librería que usa la impresora real habla Bluetooth Classic, RFCOMM, perfil de puerto serie, con socket seguro: el emulador tiene que publicar el registro SDP y admitir el emparejamiento (intake §8; X:7, X:8).
- Un contenedor aparte no llega al simulador de radio del emulador por IP; sí compartiendo el espacio de red del contenedor del emulador (X:10).
- Dirección del intake: Bumble sólo en la prueba corta; pila propia en C# después; sobre 2,5 millones de tokens se vuelve a la persona (intake §6, §8; R:15, respuesta de un agente en rol, incluida en el texto que la persona firmó).
- Fuente pública, abierta el 2026-11-27 (https://google.github.io/bumble/platforms/android.html): «The Android emulator's virtual Bluetooth controller is called Netsim.»; «Both ways are controlled via gRPC requests to the Android emulator controller and/or from the Android emulator.»; la misma página advierte: «Bluetooth support in the Android emulator is a recent feature that may still be evolving.»
- Dos tensiones siguen abiertas con dueño «mesa» y destino «diferida: siembra» (intake §11): TS-2 (emparejamiento emulado frente al real) y TS-3 (pila propia frente a puente). Las dos dependen de la prueba corta S0-S8 (PA-4, «diferida: prueba corta»). Hay registros locales de una corrida de la prueba corta fuera de los repositorios; no están publicados ni juzgados, y este ADR no los usa como evidencia.

## 2. Opciones

- A. Transporte propio en C#: un controlador virtual que se conecta al gRPC de netsim desde el espacio de red del contenedor del emulador, con dirección Bluetooth fija entre reinicios (para que el emparejamiento de Android siga valiendo), y encima RFCOMM/SPP y SDP propios.
- B. Puente: el producto en .NET recibe los bytes de un periférico hecho con Bumble (Python) que habla con netsim (reversible y más rápido; agrega Python al contenedor, contra «Producto en .NET 10», intake §6).
- C. Conectar por el puerto HCI de netsim en vez de gRPC (supuesto: mismo alcance de red; no evaluado).

## 3. Decisión

Propuesta: A, que es la dirección que el intake firmado ya trae (§8). Queda `propuesto`: TS-2 y TS-3 son de una mesa (K2, `Rules-Decisiones.md` §2) que lee el resultado juzgado de la prueba corta; este arranque no convocó mesa. Destino de TS-2 y TS-3: este ADR (`DEC.md` DEC-001). Si la prueba corta falla en S2, S3 o S5, el ADR pasa a `reemplazado` y el caso vuelve a la persona (intake §12).

## 4. Consecuencias

- Épica del medio emulado (`vision.md` §4); CE-1, CE-2, CE-3.
- Riesgos del intake §10: medio emulado no verificado de punta a punta; emparejamiento emulado distinto del real; fidelidad sin calibrar hasta la segunda etapa.
