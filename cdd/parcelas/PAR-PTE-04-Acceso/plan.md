# Plan · PAR-PTE-04

> Sólo velocidad completa. Lo que va acá no se repite en `parcela.md`.

## Índice

1. [Enfoque técnico](#1-enfoque-técnico)
2. [Superficies](#2-superficies)
3. [Riesgos](#3-riesgos)

## 1. Enfoque técnico

Un solo host ASP.NET Core (`PrintThermal.Emulator.Api`, ahora ejecutable) sirve la API, el documento OpenAPI, la página Scalar y el visor Blazor Interactive Server. El visor llama a la API por HTTP con el cliente tipado de `PrintThermal.Emulator.Client` (ADR-0002 A, ADR-0003 A), como cualquier otro consumidor.

**Contrato de la API (D3, K2 sobre ADR-0001).** Sin autenticación. JSON con nombres en camelCase. Versión 2 de este plan: incorpora los cambios de la mesa ciega (`evidencia/2026-11-27-mesa.md`), marcados con (M). Los cambios futuros son sólo aditivos (M); los errores 400 y 404 salen como ProblemDetails, RFC 9457 (M).

| Método y ruta | Respuesta | Códigos |
|---|---|---|
| `GET /api/estado` | `EstadoDelEmulador`: versión, nombre del medio, si está conectado, cantidad de tickets, id del último, hora del servidor | 200 |
| `GET /api/tickets?limite={n}` | Lista de `TicketInfo`, el más nuevo primero; `limite` opcional (M) | 200; 400 si `limite` no está entre 1 y 1000 |
| `GET /api/tickets/ultimo` | `TicketInfo` del más nuevo | 200, 404 si no hay |
| `GET /api/tickets/ultimo.png` | PNG del más nuevo | 200 `image/png`, 404 |
| `GET /api/tickets/{id}` | `TicketInfo` | 200, 404 |
| `GET /api/tickets/{id}.png` | PNG tal como se generó | 200 `image/png`, 404 |
| `GET /api/tickets/{id}.bin` | Bytes recibidos | 200 `application/octet-stream` con nombre `{id}.bin`, 404 |
| `GET /api/tickets/esperar?despuesDe={id}&segundos={n}` | `TicketInfo` del primer ticket con id mayor que `despuesDe` | 200; 204 al vencer el plazo; 400 si `segundos` no es un entero entre 1 y 60 (30 si falta) |
| `DELETE /api/tickets` | Borra todos | 204 |
| `DELETE /api/tickets/{id}` | Borra uno | 204, 404 |

`TicketInfo`: `id`, `recibido` (ISO 8601 con desplazamiento), `bytesBin`, `bytesPng`, `anchoPx`, `altoPx`.

Id (M): sin puntos ni barras (restricción de ruta `[A-Za-z0-9_-]+`), ordenable en orden ordinal también tras un reinicio, y mayor que `0`. El almacén en memoria usa la hora UTC al milisegundo y una secuencia (`20261127T143005123-000001`).

Espera (M): devuelve el primer ticket con id mayor que `despuesDe`, aunque el de referencia ya no exista. Sin `despuesDe`, sólo un ticket posterior a los presentes al recibir el pedido (nunca uno viejo). Patrón para no perder un ticket: leer `ultimoId` en `/api/estado`, imprimir y esperar con `despuesDe=ultimoId`, o `despuesDe=0` si no había tickets. La espera se corta si el cliente se desconecta; el cliente necesita un tiempo de espera mayor que `segundos` (M).

Borrar todo (M): sin autenticación, vacía también lo que esperan otros consumidores del mismo emulador; lo dice la descripción de la operación y lo dirá la documentación de uso (PAR-PTE-06).

Versión 1 (descartada por la mesa): sin `despuesDe` devolvía el último si existía, y un id desconocido esperaba uno posterior a los presentes.

**Almacén y medio (D4).** `IAlmacenDeTickets` y `IEstadoDelMedio` viven en `Api/`. Esta parcela los cumple con `AlmacenEnMemoria` (capacidad acotada) y, si `Emulador:Simulacion:IntervaloSegundos` es mayor que 0, con `FuenteSimulada`, que genera un ticket sintético (PNG de 384 puntos de ancho y su `.bin` ESC/POS) cada tantos segundos. El almacén en disco de PAR-PTE-02 y el medio Bluetooth de PAR-PTE-03 se conectan detrás de esas interfaces.

**Escucha (D7).** Si no hay `urls`, `http_ports`, `https_ports` ni `Kestrel:Endpoints` configurados, `http://127.0.0.1:5080`. El visor arma la dirección base de su cliente con `Visor:UrlApi` o, si falta, con la primera dirección del servidor (un comodín se traduce a `localhost`).

**Visor.** Una página en `/`: cabecera con el estado del medio y la cantidad de tickets; lista con hora y tamaño; el ticket elegido a escala (D6: imagen de 48 mm en un papel de 58 mm, por CSS en milímetros); descarga del PNG y del `.bin`; limpiar con confirmación. Cuatro estados (cargando, vacío, datos, error). Se actualiza con un bucle de `GET /api/tickets/esperar` mientras el circuito vive. Sin CDN: CSS propio en `wwwroot/` y fuentes del sistema.

**Pruebas.** `WebApplicationFactory<Program>` con el almacén en memoria. El visor se prueba por su prerenderizado: en la prueba, el cliente del visor sale por el manejador del servidor de pruebas con un registro de pedidos, para comprobar que la lista llegó por la API.

## 2. Superficies

| Superficie | ID | Rutas |
|---|---|---|
| API HTTP | REQ-HIS-Api-De-Tickets | `src/PrintThermal.Emulator.Api/Api/**`, `Program.cs`, `tests/…/Api/**` |
| Web | REQ-HIS-Visor-Web | `src/PrintThermal.Emulator.Api/Visor/**`, `wwwroot/**`, `tests/…/Visor/**` |
| Biblioteca cliente | REQ-HIS-Cliente-Tipado | `src/PrintThermal.Emulator.Client/**/*.cs`, `tests/…/Cliente/**` |
| Paquetes | REQ-HIS-Compilacion-Verificada | `Directory.Packages.props` |

## 3. Riesgos

- El visor se llama a sí mismo por HTTP: si la dirección base no resuelve (comodín, contenedor), el visor queda en error. Mitigación: `Visor:UrlApi` y traducción de comodines; se mira en el recorrido.
- Las pruebas no ven el circuito interactivo (sólo el prerenderizado); la actualización por espera se juzga en el recorrido en navegador real.
- La escala de 8 puntos por mm no está medida en la 58HB6 (D6).
