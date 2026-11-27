# PAR-PTE-03 · Plan

## Índice

1. [Enfoque](#1-enfoque)
2. [Capas](#2-capas)
3. [Pruebas](#3-pruebas)
4. [Prueba real](#4-prueba-real)
5. [Riesgos](#5-riesgos)

## 1. Enfoque

Opción A de ADR-0004 (la del intake firmado, §8): un host Bluetooth Classic propio en C# que habla HCI con netsim por su gRPC (`PacketStreamer/StreamPackets`) y registra el chip con dirección fija. Reemplaza al periférico de Bumble de la prueba corta. La decisión de TS-2 y TS-3 es D1 (K2): la toma una mesa de otro contexto sobre el resultado juzgado de la prueba corta y de esta parcela, y el PO la aprueba en lote en la Review (`Rules-Decisiones.md` §2). ADR-0004 tiene `gobierno: humano` y esta parcela no lo toca.

## 2. Capas

| Carpeta | Qué hace |
|---|---|
| `Hci/` | Paquetes, dirección (orden invertido para netsim, S6b de la prueba corta), cola de comandos con `Num_HCI_Command_Packets`, ACL con buffers del controlador, periférico (inicialización, conexiones) y servicio con reintento |
| `Hci/Netsim/` | Transporte gRPC sin paquetes: HTTP/2 sin TLS y protobuf a mano (D2) |
| `L2cap/` | Señalización (conexión, configuración, desconexión, información, eco) y canales en modo básico |
| `Sdp/` | Elementos de datos, servidor (0x02, 0x04, 0x06, con continuación) y registro SPP (D4) |
| `Seguridad/` | SSP por confirmación, claves de enlace en archivo, PIN heredado (D3) |
| `Rfcomm/` | Multiplexor TS 07.10 del lado que responde, PN con créditos, MSC, entrega por `IReceptorDeBytes` (D6) |

## 3. Pruebas

Pruebas primero (`evidencia/2026-11-27-rojo.txt`): un controlador HCI falso y un Android simulado escritos aparte del código de producción (FCS bit a bit, tramas armadas byte a byte). REQ-01 por el servidor SDP; REQ-02 por la secuencia de eventos SSP y el reinicio con el mismo archivo de claves; REQ-03 por L2CAP + RFCOMM con fragmentación ACL, créditos y SHA-256, cinco envíos. El transporte gRPC se prueba contra un servidor HTTP/2 local que imita a netsim.

## 4. Prueba real

Emulador Android API 37 de la prueba corta (contenedor con turno, 2 vCPU / 6 GB) y la app Java mínima (`createRfcommSocketToServiceRecord` con 0x1101): descubrir 3 de 3, emparejar, SHA-256 íntegro 5 de 5 y reconexión sin reemparejar tras reiniciar el periférico. El periférico va en el equipo de cómputo si llega al netsim por red; si no, se mide y corre dentro del turno del emulador.

## 5. Riesgos

- Interoperar con la pila de Android (créditos RFCOMM, configuración L2CAP): lo cubre la prueba real.
- Cómo empareja la 58HB6 real y su registro SDP real siguen sin medir (Especificación, «[MEDIR]»).
