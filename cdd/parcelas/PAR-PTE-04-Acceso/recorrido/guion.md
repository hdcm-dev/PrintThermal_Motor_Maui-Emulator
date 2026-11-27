# Guion del recorrido · PAR-PTE-04

> Recorrido del visor en navegador real, por `interfaz: n2` de REQ-HIS-Visor-Web (`Rules-Decisiones.md` §7). Es la etapa T4 de `tasks.md`; SPR-01 la llama T6.

## Índice

1. [Navegador y URL](#1-navegador-y-url)
2. [Anchos](#2-anchos)
3. [Pasos](#3-pasos)
4. [Capturas y consola](#4-capturas-y-consola)

## 1. Navegador y URL

- Navegador: Chrome 157, en el contenedor del navegador de juicio del equipo de desarrollo, manejado con chrome-devtools-mcp. Fecha: 2026-11-27, de 15:29 a 15:34 UTC.
- Servidor: `PrintThermal.Emulator.Api` publicado con `dotnet publish -c Release` desde el código final de esta parcela, en `mcr.microsoft.com/dotnet/sdk:10.0.403` con turno en el equipo de cómputo. Tres instancias, cada una con `--urls` explícito dentro del contenedor:
  - `http://<emulador>:18604/`, con `Emulador:Simulacion:IntervaloSegundos=40` (fuente simulada, D4);
  - `http://<emulador>:18605/`, con `Visor:UrlApi` hacia un puerto cerrado del propio contenedor (error inmediato);
  - `http://<emulador>:18606/`, con `Visor:UrlApi` hacia una dirección de documentación (RFC 5737) que no responde, para ver «cargando» y el error por plazo.
- `<emulador>` es la dirección del puerto publicado en el equipo, alcanzada por un túnel SSH; no se publica.
- Las horas que muestra el visor son las del servidor (UTC en el contenedor).
- Los cambios de estado se registraron con un `MutationObserver` sobre `main[data-estado]`, instalado antes de cargar cada página.

## 2. Anchos

- 1440 × 900 px.
- 360 × 800 px, con emulación móvil y táctil.

## 3. Pasos

1. Abrir `/` en 18604 antes del primer ticket. Se ve el estado vacío: «Todavía no llegó ningún ticket» y «Medio simulado, conectado · 0 tickets». No hay acción primaria (`1440-vacio.png`). Estados registrados: `cargando` (2 ms, primer envío del renderizado en flujo) y luego `vacio`.
2. Sin tocar nada, llega el primer ticket y el visor pasa solo a datos, sin recargar, por la espera larga de `/api/tickets/esperar`. Se ven la lista con hora y tamaño («37 bytes · 21.5 mm»), el ticket a escala y las dos descargas (`1440-datos-llega-el-primero.png`).
3. Llega el segundo. La lista crece y el elegido sigue al más nuevo (`1440-datos-dos-tickets.png`).
4. Recargar la página. Estados registrados: `cargando` (2 ms) y `datos` (26 ms). No hay una segunda vuelta a `cargando` al conectarse el circuito: el estado prerenderizado persiste.
5. Elegir el ticket más viejo. Cambian la imagen, el texto alternativo y los enlaces (`1440-datos-otro-ticket.png`). Medido en la página:
   - la impresión mide 181,4 px CSS, o sea 48,0 mm, y el papel 58,0 mm (D6);
   - el PNG tiene 384 puntos de ancho natural.
6. Descargar desde los enlaces del ticket elegido, con `fetch` del mismo `href`:
   - «Descargar PNG» responde 200 `image/png` con 334 bytes, y `download="ticket-<id>.png"`;
   - «Descargar bytes recibidos» responde 200 `application/octet-stream` con 37 bytes, y `download="ticket-<id>.bin"`.
7. «Limpiar» pide confirmación en la misma fila: «¿Borrar 2 tickets?», con «Borrar todo» y «Cancelar» (`1440-datos-confirmar-limpieza.png`).
8. «Borrar todo»: el visor queda vacío y el estado dice 0 tickets (`1440-vacio-tras-limpiar.png`).
9. Pasar a 360 px; la emulación recarga la página. Se ve el estado vacío (`360-vacio.png`). Llega un ticket y el visor pasa solo a datos: lista, papel y acciones en una columna (`360-datos.png`, página completa). Medido:
   - sin desborde horizontal (`scrollWidth` = 360);
   - ningún botón ni enlace mide menos de 44 px de alto;
   - la impresión sigue en 48 mm.
10. Abrir 18606 a 360 px. Se ve «cargando»: esqueletos con `aria-busy` y «Consultando el estado…» (`360-cargando.png`). A los 75 s vence el plazo del cliente y aparece el error: «No se pudo leer la API del emulador. The request was canceled due to the configured HttpClient.Timeout of 75 seconds elapsing.» y «Sin conexión con la API» (`360-error-por-plazo.png`). Estados registrados: `cargando` (3 ms) y `error` (74 989 ms). Ocho segundos después sigue en `error`, sin volver a `cargando`.
11. Abrir 18605 a 360 px. Se ve el error inmediato: «Connection refused (127.0.0.1:9)» y «Reintentar» (`360-error.png`). Al pulsar «Reintentar», sigue en error y la página no se rompe.
12. La misma instancia a 1440 px (`1440-error.png`).

## 4. Capturas y consola

- 1440 px: `1440-vacio.png`, `1440-datos-llega-el-primero.png`, `1440-datos-dos-tickets.png`, `1440-datos-otro-ticket.png`, `1440-datos-confirmar-limpieza.png`, `1440-vacio-tras-limpiar.png` y `1440-error.png`.
- 360 px: `360-vacio.png`, `360-datos.png`, `360-cargando.png`, `360-error-por-plazo.png` y `360-error.png`.
- `consola.txt`: la consola de cada navegación, sin errores ni avisos.
- Este es el tercer recorrido. Los dos anteriores se descartaron:
  - el primero, por un 404 de `/favicon.ico` en la consola y por el rótulo «Consultando el estado…» visible en el estado de error;
  - el segundo, porque tras un error en el prerenderizado el circuito volvía a «cargando» (error → cargando, medido con el mismo observador).

  Se corrigió el código (`favicon.svg`, el rótulo, y el estado y el error persistidos entre el prerenderizado y el circuito) y se repitió el recorrido completo.
