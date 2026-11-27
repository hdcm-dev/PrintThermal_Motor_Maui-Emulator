---
id: ADR-0009
version: 1
gobierno: humano
estado: aceptado
rutas: []
---
# ADR-0009 · Reconexión breve de la impresora emulada tras un vínculo que terminó sin cifrar

## Índice

1. [Contexto](#1-contexto)
2. [Opciones](#2-opciones)
3. [Decisión](#3-decisión)
4. [Consecuencias](#4-consecuencias)

## 1. Contexto

LAZ-0011: vincular «58HB6» desde Ajustes de Android (vínculo dedicado, `createBond`) y conectar después dejaba la conexión sin cifrar. El diagnóstico de PAR-PTE-07 (`cdd/parcelas/PAR-PTE-07-Vinculo-Desde-Ajustes/evidencia/2026-11-27-diagnostico.md`) muestra que la pila propia hace su parte: responde el pedido de capacidades, confirma, recibe la clave (Link Key Notification) y, en la conexión siguiente, nunca recibe el pedido de clave porque Android no lo hace. Del lado de Android:

- netsim da el manejador 0 a la primera conexión después de encender el Bluetooth de Android;
- la pila de Android busca el equipo de cada evento por manejador, y los equipos que carga de su configuración nacen con el manejador de LE en 0 (`BTM_SecAddDevice` reserva el registro con `osi_calloc` y sólo fija el de BR/EDR; `btm_find_dev_by_handle` compara los dos);
- así, el fin de la vinculación (Authentication Complete) y el corte se atribuyen a **otro** equipo vinculado antes, y el nuestro queda «AUTHENTICATING»: en la conexión siguiente, `btm_sec_execute_procedure` no hace nada «in busy state» y el RFCOMM vence.

Un corte con un manejador que Android atribuye bien devuelve el equipo a reposo: lo mostró una conexión vencida seguida de otra que cifró.

## 2. Opciones

- A. La impresora, al ver que un vínculo nuevo terminó sin cifrar, hace una conexión breve con el teléfono (que tiene el escaneo de página activo) y la corta enseguida: Android la recibe con un manejador nuevo, atribuido al equipo correcto, y su corte lo deja en reposo (reversible: opción `ReconexionTrasVinculoSinCifrar`).
- B. Documentar el límite y pedir al usuario que vincule conectando o que reintente la conexión (no cumple LAZ-0011: el camino de Ajustes tiene que funcionar como con la impresora real).
- C. Que la impresora inicie autenticación o cifrado en la conexión de la app. Según el código fuente de `main` (no medido en la imagen API 37), Android cierra el estado al recibir el cifrado ajeno pero no reanuda la conexión L2CAP que espera (`btm_sec_encrypt_change` vuelve sin reenviar el pedido), y el RFCOMM vencería igual.

## 3. Decisión

A. Aprobada en rol por el responsable de las decisiones técnicas del emulador, por mandato del Product Owner del 2026-11-27 (testimonio F1, registro privado del producto), en PAR-PTE-07. Medida en PAR-PTE-07: camino de Ajustes con la condición presente, 3 de 3 con cifrado al primer intento de la app (series de 5 impresiones de 5 con la misma huella del PNG); controles sin la condición (manejador distinto de 0, o sin otros equipos vinculados): Android cifra en la conexión del vínculo y la reconexión breve no se dispara; vincular conectando, 5 de 5 sin conexión propia, dos veces.

## 4. Consecuencias

- La impresora emulada inicia una conexión ACL propia, cosa que una impresora real no hace en este flujo: es una adaptación al emulador de Android con netsim, no una imitación de la 58HB6.
- Sólo se dispara tras un vínculo nuevo sin cifrar; con `Emulador__Bluetooth__ReconexionTrasVinculoSinCifrar=false` se apaga.
- Sin medir: vincular conectando como primera conexión después de encender el Bluetooth de Android, con otros equipos vinculados (LAZ-0016).
