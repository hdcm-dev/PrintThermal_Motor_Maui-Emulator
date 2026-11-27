---
id: LAZ-0011
fecha: 2026-11-27
origen: parcela
clase: aclaracion
estado: especificado
alcance: corpus
ids: [REQ-HIS-Descubrimiento-Y-Emparejamiento@v3]
deuda:
normalizado:
parcela: PAR-PTE-07
---
# LAZ-0011 · Un vínculo quedó inservible en la prueba real: consentimiento tardío o binario con pings

## Índice

1. [Duda o hallazgo](#1-duda-o-hallazgo)
2. [Clasificación](#2-clasificación)
3. [Propagación](#3-propagación)
4. [Decisión](#4-decisión)

## 1. Duda o hallazgo

PAR-PTE-03, 2026-11-27, prueba real (`cdd/parcelas/PAR-PTE-03-Medio-Emulado/evidencia/2026-11-27-prueba-real.txt` §3).
El toque en «Pair» llegó 31 s después del pedido de confirmación. Android dejó el vínculo (state=12), pero su seguridad
quedó en «AUTHENTICATING» y en la conexión siguiente no pidió la clave de enlace: el RFCOMM venció por tiempo. Se
repitió con otra dirección y el toque a tiempo, y funcionó. La causa no está separada: el reintento cambió a la vez la
dirección, el tiempo del toque y el binario (el del intento fallido mandaba pings de HTTP/2 a netsim, que cortó el flujo
antes y después de ese intento). En el intento fallido el emulador no registró «cifrado», y en el exitoso sí. Hipótesis
competidoras: el temporizador de emparejamiento de Android, o el binario con pings (NO VERIFICADO). Sin resolver además:
cómo se recupera un vínculo trabado (olvidar el equipo en Android, otra
dirección, borrar el archivo de claves) y quién confirma el diálogo en una prueba automática de extremo a extremo.

PAR-PTE-05, 2026-11-27: vincular con `createBond` (como Ajustes → Bluetooth) y conectar después dejó la conexión sin
cifrar 2 de 2; vincular conectando funcionó. PO, 2026-11-27: emparejar desde Ajustes y después conectar por RFCOMM
tiene que funcionar con cifrado, igual que con la impresora real.

## 2. Clasificación

Aclaración: afecta al montaje y a la documentación de uso (PAR-PTE-06) y a CE-2, que hoy se cumple con «figura
vinculada» aunque el vínculo no sirva.

## 3. Propagación

| Documento | Antes | Después |
|---|---|---|
| REQ-HIS-Descubrimiento-Y-Emparejamiento | v2: CE-2 «figura vinculada» | v3: además, vinculada desde Ajustes, la conexión RFCOMM siguiente se cifra e imprime |

## 4. Decisión

Especificado el 2026-11-27: CE-2 se mide también por el camino de Ajustes («vinculada desde Ajustes y primer RFCOMM
cifrado e íntegro, sin reemparejar»), criterio nuevo de REQ-HIS-Descubrimiento-Y-Emparejamiento v3. Lo ejecuta
PAR-PTE-07: diagnóstico con la traza HCI de los dos lados, corrección en la pila C# con prueba automática y prueba real
en el emulador de Android API 37 por los dos caminos (Ajustes y vincular conectando).
