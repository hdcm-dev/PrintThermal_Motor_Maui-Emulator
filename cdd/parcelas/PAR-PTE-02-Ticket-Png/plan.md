# Plan · PAR-PTE-02

> Sólo velocidad completa. Lo que va acá no se repite en `parcela.md`.

## Índice

1. [Enfoque técnico](#1-enfoque-técnico)
2. [Superficies](#2-superficies)
3. [Riesgos](#3-riesgos)

## 1. Enfoque técnico

- `Interprete` (Core/EscPos): máquina incremental. Cada trozo se agrega a un búfer pendiente; un comando se ejecuta sólo cuando sus bytes están completos, y las cargas (GS v 0, GS ( k, GS ( L, GS 8 L, FS q) se miden por longitud, nunca buscando LF (Especificacion-58HB6 §4.3). Entrega cada ticket por el evento `TicketTerminado` y las respuestas por `Respuesta`.
- Límites de un ticket: abre con ESC @ o con el primer comando que imprime; cierra con GS V, con un ESC @ después de contenido o con `Finalizar` (fin de la conexión, que llamará PAR-PTE-03). Las consultas fuera de un ticket no son de ninguno.
- Dibujo: lienzo de 384 puntos que crece hacia abajo; fuente A de 12 x 24 (Terminus Font, OFL 1.1, normal y negrita); ESC a, ESC !, ESC E, ESC -, GS !, ESC t (437, 850, 858 y otras), raster GS v 0 con sus modos, QR (codificador propio en modo byte, probado contra QRCoder 1.8.0), EAN-13 (GS k 2 y 67), ESC d, ESC J, GS V.
- PNG propio en escala de grises de 1 bit (zlib del runtime, CRC32 propio): ningún paquete nuevo, así `Directory.Packages.props` (REQ-HIS-Compilacion-Verificada) no se toca.
- `AlmacenDeTickets` (Core/Almacen): `{id}.bin` y `{id}.png` por ticket; id `AAAAMMDDTHHMMSSmmm-NNNN` validado con una expresión regular antes de tocar el disco; escritura por temporal y renombre.
- Vectores dorados: los `.bin` salen de ejecutar los renderers de la librería (D1); los `.png`, de la primera corrida en verde revisada a ojo (D3), con dos oráculos independientes del dorado: los bits de GS v 0 aparecen tal cual en el PNG y la matriz QR coincide con la de QRCoder.

## 2. Superficies

Una sola: la biblioteca Core (sin API, web ni app). PAR-PTE-03 llamará `Alimentar`/`Finalizar` y PAR-PTE-04 leerá el almacén.

## 3. Riesgos

- Fidelidad: fuente, interlineado (30 puntos), margen, alto de barras (80) y ancho de módulo (2) son supuestos sin medir (intake §6); quedan en `PerfilDeImpresora` para calibrarlos en la segunda etapa.
- Selección de máscara QR: la 58HB6 puede elegir otra máscara que QRCoder; el QR sigue siendo legible, pero el PNG no sería idéntico al de la impresora.
- FS q: la longitud se mide con la fórmula de Epson; la librería podría emitir otra (Especificacion-58HB6 §4.2, NO VERIFICADO). La librería no lo usa en el camino de la app de campo.
