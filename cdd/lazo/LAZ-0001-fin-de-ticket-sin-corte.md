---
id: LAZ-0001
fecha: 2026-11-27
origen: review
clase: aclaracion
estado: abierto
alcance: corpus
ids:
deuda:
normalizado:
parcela:
---
# LAZ-0001 · Fin de un ticket sin corte con la conexión abierta

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

Auditoría del cierre de PAR-PTE-02 (2026-11-27, `cdd/parcelas/PAR-PTE-02-Ticket-Png/evidencia/2026-11-27-auditoria.md`, hallazgo 2): con la cola `end_of_ticket = feed` o `none` la librería cliente no manda corte (Especificacion-58HB6 §4.1). El intérprete cierra ese ticket con el próximo ESC @ o con `Finalizar` (fin de la conexión). Si la conexión queda abierta y no llega otro ticket, el ticket no se guarda: no hay plazo de inactividad decidido. Tampoco está escrito en PAR-PTE-03 que el transporte deba llamar `Finalizar` y conectar el almacén (`AlmacenDeTickets.Escuchar`).

## 2. Clasificación

Aclaración (L1): no cambia el alcance de CE-4; falta decidir el plazo y quién lo aplica (transporte de PAR-PTE-03 o API de PAR-PTE-04).

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| PAR-PTE-03 | No nombra `Finalizar`, `Respuesta` ni el almacén | Por decidir |

## 4. Decisión

Pendiente.

**Avance en PAR-PTE-05 (2026-11-27).**

Con PAR-PTE-05 (D2) el host termina el ticket en curso cuando se cierra la conexión RFCOMM (`ReceptorAlAlmacen.Cerrada` → `Interprete.Finalizar`); prueba `UnTicketSinCorteTerminaAlCerrarLaConexion`. Con la conexión abierta y sin corte, el ticket sigue abierto: el plazo sigue sin decidir.
