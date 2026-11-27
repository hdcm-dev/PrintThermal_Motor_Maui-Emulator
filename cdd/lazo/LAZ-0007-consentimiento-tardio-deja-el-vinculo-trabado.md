---
id: LAZ-0007
fecha: 2026-11-27
origen: parcela
clase: aclaracion
estado: abierto
alcance: corpus
ids:
deuda:
normalizado:
parcela:
---
# LAZ-0007 · Si el consentimiento de Android llega tarde, el vínculo queda inservible

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-03, 2026-11-27, prueba real (`cdd/parcelas/PAR-PTE-03-Medio-Emulado/evidencia/2026-11-27-prueba-real.txt` §3).
El toque en «Pair» llegó 31 s después del pedido de confirmación. Android dejó el vínculo (state=12), pero su seguridad
quedó en «AUTHENTICATING» y en la conexión siguiente no pidió la clave de enlace: el RFCOMM venció por tiempo. Se
repitió con otra dirección y el toque a tiempo, y funcionó. Sin resolver: la causa (probablemente el temporizador de
emparejamiento de Android, NO VERIFICADO), cómo se recupera un vínculo trabado (olvidar el equipo en Android, otra
dirección, borrar el archivo de claves) y quién confirma el diálogo en una prueba automática de extremo a extremo.

## 2. Clasificación

Aclaración: afecta al montaje y a la documentación de uso (PAR-PTE-06) y a CE-2, que hoy se cumple con «figura
vinculada» aunque el vínculo no sirva.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| — | — | — |

## 4. Decisión

Pendiente: reproducir (confirmar después de 30 s), medir la recuperación y decidir si CE-2 se mide como «vinculada y
primer RFCOMM íntegro sin reemparejar» (lo propone la mesa de TS-2 y TS-3).
