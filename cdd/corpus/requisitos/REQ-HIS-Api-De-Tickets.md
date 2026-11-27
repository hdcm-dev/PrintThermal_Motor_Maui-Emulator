---
id: REQ-HIS-Api-De-Tickets
version: 2
epica: REQ-EPI-Acceso
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Api/Api/**
  - src/PrintThermal.Emulator.Api/Program.cs
  - tests/PrintThermal.Emulator.Tests/Api/**
---
# REQ-HIS-Api-De-Tickets

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un host ASP.NET Core expone `/api/...`: estado, lista de tickets, último, un ticket por id en `.png` y `.bin`, esperar un ticket con tiempo máximo y borrar; publica OpenAPI en `/openapi/v1.json` y la página Scalar en `/scalar`; escucha sólo en local salvo configuración (ADR-0001).

El contrato (PAR-PTE-04, D3 con los cambios de la mesa ciega; detalle en `cdd/parcelas/PAR-PTE-04-Acceso/plan.md` §1):

| Ruta | Qué devuelve |
|---|---|
| `GET /api/estado` | Versión, medio y si está conectado, cantidad de tickets, `ultimoId`, hora |
| `GET /api/tickets?limite=` | Tickets, el más nuevo primero |
| `GET /api/tickets/ultimo`, `/ultimo.png` | El más nuevo: datos o PNG; 404 si no hay |
| `GET /api/tickets/{id}`, `/{id}.png`, `/{id}.bin` | Un ticket: datos, PNG (`image/png`) o bytes recibidos; 404 si no existe |
| `GET /api/tickets/esperar?despuesDe=&segundos=` | El primer ticket con id mayor que `despuesDe`; 204 al vencer el plazo (1 a 60 s, 30 por omisión); 400 fuera de rango |
| `DELETE /api/tickets`, `/{id}` | Borra todos o uno; 204, o 404 si el id no existe |

- Los errores van en ProblemDetails.
- Los ids son ordenables, sin puntos y mayores que `0`.
- Sin `despuesDe`, la espera devuelve sólo un ticket posterior a los presentes. Para no perder un ticket: leer `ultimoId`, imprimir y esperar con ese id, o con `0` si no había ninguno.
- Sin dirección configurada (`urls`, `http_ports`, `https_ports` o `Kestrel:Endpoints`), el host escucha en `http://127.0.0.1:5080`.
- El almacén y el medio entran por `IAlmacenDeTickets` e `IEstadoDelMedio`. Esta versión trae un almacén en memoria y una fuente simulada (`Emulador:Simulacion:IntervaloSegundos`).

## 2. Criterios

- Dado un ticket impreso, cuando se pide su PNG por la API, entonces llega como `image/png` con la misma huella SHA-256 que el generado, 5 de 5 (CE-5).
- Dado que no llega ningún ticket, cuando se espera con tiempo máximo, entonces la API responde al vencer el plazo sin quedar colgada.

## 3. Trazas

- Sube de: REQ-EPI-Acceso
- Implementado en: `src/PrintThermal.Emulator.Api/Program.cs` y `src/PrintThermal.Emulator.Api/Api/` (`EndpointsDeTickets`, `AlmacenDeTickets`, `AlmacenEnMemoria`, `FuenteSimulada`, `Direcciones`, `Png`); pruebas en `tests/PrintThermal.Emulator.Tests/Api/`.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-04 | Contrato de rutas, esquema y códigos (D3, con los cambios de la mesa); OpenAPI y Scalar; escucha local por omisión; almacén en memoria y fuente simulada detrás de interfaces; rutas de pruebas |
