---
id: LAZ-0015
fecha: 2026-11-27
origen: parcela
clase: aclaracion
estado: especificado
alcance: corpus
ids: [REQ-HIS-Visor-Web@v3]
deuda:
normalizado:
parcela: PAR-PTE-07
---
# LAZ-0015 · El ticket desborda su tarjeta en el visor con pantalla angosta

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-05, 2026-11-27 (`cdd/parcelas/PAR-PTE-05-Distribucion/evidencia/capturas/visor-ticket.png`): en el Chrome del
emulador de Android, con pantalla de 540 px, el ticket desborda a la derecha de su tarjeta y el texto de la tarjeta se
corta. Quedó como hallazgo fuera de alcance de esa parcela.

## 2. Clasificación

Aclaración: el visor ya debía mostrar el ticket a escala dentro de su tarjeta; el criterio no decía el ancho mínimo.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| REQ-HIS-Visor-Web | v2: sin ancho mínimo | v3: con 540 px de pantalla o menos, el ticket no desborda su tarjeta ni la página |

## 4. Decisión

Especificado el 2026-11-27 en REQ-HIS-Visor-Web v3; lo ejecuta PAR-PTE-07 con medición en Chrome con emulación de
ancho.
