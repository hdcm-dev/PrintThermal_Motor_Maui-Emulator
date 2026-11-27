---
id: LAZ-0002
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
# LAZ-0002 · Respuestas y longitudes de protocolo sin medir en la 58HB6

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

Auditoría del cierre de PAR-PTE-02 (2026-11-27, hallazgo 4): tres supuestos del intérprete cambian lo que la librería cliente ve del emulador si son falsos, y no son fidelidad visual.

- Respuesta a GS I 67: el emulador manda «58HB6» + NUL sin cabecera; el formato oficial no se pudo abrir (Especificacion-58HB6 §2, NO VERIFICADO) y la respuesta real es [MEDIR] (§8, ítem 3).
- Bytes de DLE EOT 1, 2 y 4: los de la tabla de §4.3, derivados de las máscaras de la librería; los reales son [MEDIR] (§8, ítem 2).
- Largo de FS q: el emulador usa la fórmula de Epson (x · y · 8 por imagen); la librería podría emitir otra unidad (§4.2, NO VERIFICADO). Si difiere, desalinea la lectura de lo que sigue. La librería no lo usa en el camino de la app de campo.

## 2. Clasificación

Aclaración (L1): los valores son configurables (`PerfilDeImpresora`), salvo el largo de FS q. Se resuelven midiendo la impresora (intake §6) o con la prueba de extremo a extremo contra la librería (PAR-PTE-05).

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| `PerfilDeImpresora`, `Interprete` | Supuestos rotulados NO VERIFICADO | Por decidir con la medida |

## 4. Decisión

Pendiente.
