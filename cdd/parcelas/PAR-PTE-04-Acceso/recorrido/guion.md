# Guion del recorrido · PAR-PTE-04

> Recorrido del visor en navegador real, por `interfaz: n2` de REQ-HIS-Visor-Web (`Rules-Decisiones.md` §7).

## Índice

1. [Navegador y URL](#1-navegador-y-url)
2. [Anchos](#2-anchos)
3. [Pasos](#3-pasos)
4. [Capturas y consola](#4-capturas-y-consola)

## 1. Navegador y URL

- Navegador: Chrome 157 (contenedor del navegador de juicio del equipo de desarrollo), manejado con chrome-devtools-mcp. Fecha: 2026-11-27, de 14:59 a 15:01 UTC.
- Servidor: el proyecto `PrintThermal.Emulator.Api` publicado con `dotnet publish -c Release` del árbol de esta parcela (con `favicon.svg` y el rótulo «Sin conexión con la API»), en `mcr.microsoft.com/dotnet/sdk:10.0.403` con turno en el equipo de cómputo. Dos instancias:
  - `http://<emulador>/` con `Emulador:Simulacion:IntervaloSegundos=20` (fuente simulada, D4).
  - `http://<emulador-sin-api>/` con `Visor:UrlApi` apuntando a un puerto cerrado, para el estado de error.
- `<emulador>` es la dirección del puerto publicado en el equipo, alcanzada por un túnel SSH; no se publica.
- Las horas que muestra el visor son las del servidor (UTC en el contenedor).

## 2. Anchos

- 1440 × 900 px.
- 360 × 800 px, con emulación móvil y táctil.

## 3. Pasos

1. Abrir `/` antes del primer ticket. Se ve el estado vacío: «Todavía no llegó ningún ticket», el medio «simulado, conectado · 0 tickets» y ninguna acción primaria (`1440-vacio.png`, `data-estado="vacio"`).
2. Sin tocar nada, llega el primer ticket de la fuente simulada. El visor pasa solo a datos, por la espera larga de `/api/tickets/esperar`, sin recargar la página. Muestra la lista con la hora y el tamaño («37 bytes · 21.5 mm»), el ticket a escala y las dos descargas (`1440-datos-llega-el-primero.png`).
3. Siguen llegando tickets. La lista crece y el elegido sigue al más nuevo (`1440-datos-tres-tickets.png`).
4. Elegir el ticket más viejo de la lista. Cambian la imagen, el texto alternativo y los enlaces (`1440-datos-otro-ticket.png`). Medido en la página:
   - la impresión mide 181,4 px CSS, o sea 48,0 mm, y el papel 58,0 mm (D6);
   - el PNG tiene 384 puntos de ancho natural.
5. Descargar desde los enlaces del ticket elegido, con `fetch` del mismo `href`:
   - «Descargar PNG» responde 200 `image/png` con 345 bytes, y `download="ticket-<id>.png"`;
   - «Descargar bytes recibidos» responde 200 `application/octet-stream` con 37 bytes, y `download="ticket-<id>.bin"`.
6. «Limpiar» pide confirmación en la misma fila: «¿Borrar 3 tickets?», con «Borrar todo» y «Cancelar» (`1440-datos-confirmar-limpieza.png`).
7. «Borrar todo»: la lista se vacía. A los dos segundos la fuente simulada imprimió otro ticket, y el visor lo mostró solo. `/api/estado` informa 1 ticket (`1440-datos-tras-limpiar.png`). El estado vacío tras limpiar ya se había visto en la primera corrida, descartada.
8. Pasar a 360 px. Lista, papel y acciones quedan en una columna. No hay desborde horizontal (`scrollWidth` 360 = ancho), ningún botón ni enlace mide menos de 44 px de alto y la impresión sigue en 48 mm (`360-datos.png`, página completa).
9. Abrir la instancia sin API a 360 px. Se ve el estado de error: alerta «No se pudo leer la API del emulador. Connection refused (127.0.0.1:9)», el medio «Sin conexión con la API» y «Reintentar» (`360-error.png`). Al pulsar «Reintentar», la alerta se mantiene y la página no se rompe.
10. La misma instancia a 1440 px (`1440-error.png`).
11. `/scalar` en la instancia con la fuente simulada: la página de Scalar lista Estado y Tickets. Consola sin errores. La captura no se guarda porque muestra la dirección del servidor.

Estado «cargando»: el visor prerenderiza con los datos, así que en un servicio sano no se llega a ver. Su sección existe (esqueletos con `aria-busy`), pero este recorrido no la capturó.

## 4. Capturas y consola

- `1440-vacio.png`, `1440-datos-llega-el-primero.png`, `1440-datos-tres-tickets.png`, `1440-datos-otro-ticket.png`, `1440-datos-confirmar-limpieza.png`, `1440-datos-tras-limpiar.png`, `1440-error.png`.
- `360-datos.png`, `360-error.png`.
- `consola.txt`: consola de cada navegación, sin errores.
- La primera corrida (14:54 a 14:57 UTC) se descartó por dos motivos:
  - un 404 de `/favicon.ico` en la consola;
  - el rótulo «Consultando el estado…» seguía visible en el estado de error.

  Se corrigieron los dos y se repitió el recorrido completo.
