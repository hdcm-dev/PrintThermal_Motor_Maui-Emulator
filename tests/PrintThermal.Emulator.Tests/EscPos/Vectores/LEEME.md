# Vectores dorados de PAR-PTE-02

## Índice

1. [Bytes de la librería](#1-bytes-de-la-librería)
2. [PNG esperados](#2-png-esperados)
3. [Matrices QR de referencia](#3-matrices-qr-de-referencia)

## 1. Bytes de la librería

Los `.bin` son los bytes que emite la librería cliente MotorDsl (`origin/main` `940fa7f`), producidos el 2026-11-27 ejecutando sus renderers en un contenedor `mcr.microsoft.com/dotnet/sdk:10.0.403`. El generador está en `Generador.cs.txt` y `Generador.csproj.txt` (enlaza `BitmapEscPosRenderer.cs` y `SkiaSharpRasterizer.cs` como lo hacen las pruebas de la librería).

| Vector | Renderer y perfil | Qué ejercita |
|---|---|---|
| `01-texto` | `escpos-bitmap`, `bt_58hb6` (32 columnas, tope 320 px) | ESC @, ESC a, ESC ! 8, texto ASCII, LF, GS V 1 |
| `02-acentos` | ídem | ESC t 0 y texto en PC437 |
| `03-imagen` | ídem | GS v 0 de 160 x 80 puntos y LF |
| `04-qr` | ídem | GS ( k fn 67, 69, 80 y 81 |
| `05-ean13` | ídem | GS k 2 (EAN-13 terminado en NUL) |
| `06-escpos-feed` | `escpos`, `thermal_58mm`, `end_of_ticket=feed` | ESC a 1 + GS v 0 de 384 puntos, ESC d 4 |
| `07-escpos-corte-total` | `escpos`, `end_of_ticket=cut_full` | ESC a 2, GS V 0 |

## 2. PNG esperados

Cada `NN-nombre.png` es la salida del intérprete revisada a ojo el 2026-11-27 y fijada como dorada. Qué se miró: 01, texto centrado, en negrita, a la derecha y partido en tres líneas; 02, «Acta N°», «Infracción», «Ñandú», «¿Firma? ¡Sí!» con sus acentos; 03 y 06, el borde, el círculo y la banda de grises binarizada por la librería (sale como un cuadrado lleno), a la izquierda y a todo el ancho; 04, el QR centrado bajo el texto centrado; 05, las barras con sus guardas a la izquierda y sin texto legible; 07, «Ñandú» a la derecha.

Además del dorado, hay oráculos que no dependen de él: los bits de GS v 0 en el PNG (03 y 06), el QR de 04 módulo a módulo contra `qr-2.txt`, las barras de 05 contra la tabla de EAN-13 y la tabla de caracteres de ESC t contra las de `System.Text.Encoding.CodePages`. La fidelidad frente a la 58HB6 física no está medida (intake §6): fuente, interlineado, alto de barras y márgenes son supuestos del emulador.

## 3. Matrices QR de referencia

`qr-N.txt`: matrices de QRCoder 1.8.0 (la versión que usa la vista previa de la librería) en modo byte (`CreateQrCode(byte[])`, como codifica el emulador), sin zona de silencio; `#` es módulo oscuro. `qr-1` a `qr-3` con corrección H (la que pide la librería), `qr-4` con L (la de la impresora sin fn 69), `qr-5` con M y `qr-6` con Q. Son el oráculo independiente del codificador QR del emulador.
