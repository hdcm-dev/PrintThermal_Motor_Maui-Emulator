# Guion del recorrido · PAR-PTE-07

> REQ-HIS-Visor-Web tiene `interfaz: n2`. Recorrido del criterio de LAZ-0015 (REQ-03 de la parcela).

## Índice

1. [Navegador y URL](#1-navegador-y-url)
2. [Anchos](#2-anchos)
3. [Pasos](#3-pasos)
4. [Capturas y consola](#4-capturas-y-consola)

## 1. Navegador y URL

Chrome 157 por el MCP de herramientas del agente, con emulación de ancho (`emulate`, modo móvil y táctil). URL: el visor (`/`) del contenedor de la imagen de la rama, en la red del emulador de Android, con los tickets del camino (a) de la evidencia de extremo a extremo (estado «datos», el último ticket elegido).

## 2. Anchos

540, 480, 412, 360, 320, 280, 240 y 205 px de viewport. 205 px con escala 2,625 es la pantalla de 540 px del emulador de Android donde se vio el desborde en PAR-PTE-05.

## 3. Pasos

1. Abrir el visor con 540 px: estado «datos», ticket a escala dentro de su tarjeta.
2. Para cada ancho, cargar la página en un marco de ese ancho y medir con `getBoundingClientRect`: el ticket (`.papel__impresion`) dentro de la tarjeta (`.papel`), la tarjeta y el texto dentro de la pantalla, y `scrollWidth` del documento contra su `clientWidth`. Dos modos: «antes» (las reglas de `visor.css` de `main` repuestas en el marco) y «después» (la hoja de la rama).
3. Emular 205 px con escala 2,625, llevar la tarjeta a la vista y capturar: con la hoja de la rama y con las reglas de `main`.

Resultado (`medicion.json`):

| Ancho | Antes: desborde de la página | Antes: tarjeta en pantalla | Después: desborde | Después: ticket en tarjeta y tarjeta en pantalla |
|---|---|---|---|---|
| 540 a 240 | no | sí | no | sí |
| 205 | sí (`scrollWidth` 235 de 205) | no (tarjeta de 16 a 235) | no | sí (tarjeta de 16 a 189, ticket de 16 a 189) |

Con la hoja de la rama, a 205 px el ticket se achica al ancho de la tarjeta (173 px en lugar de 48 mm); de 240 px para arriba mantiene la escala de 48 mm.

## 4. Capturas y consola

- `205-datos-antes.png`: reglas de `main`; la tarjeta y el texto se cortan a la derecha.
- `205-datos.png`: hoja de la rama; tarjeta, ticket y texto dentro de la pantalla.
- `medicion.json`: las dieciséis mediciones.
- `consola.txt`: consola del navegador.
- `540-android-datos-ticket.png` y `540-android-datos-pie.png`: el mismo visor en el Chrome del emulador de Android (pantalla de 540 px, la del hallazgo de PAR-PTE-05), con un ticket de 384 × 1118 puntos: la tarjeta va de 42 a 498 px de pantalla (`uiautomator`: imagen `[42,243][498,1200]`, texto `[42,726][498,926]`), sin cortes a la derecha.
