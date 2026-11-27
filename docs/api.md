---
doc_id: USO-05
doc_type: guia-practica
title: API de tickets con curl y con el cliente tipado
description: Cómo leer el estado, esperar un ticket sin perderlo y bajar su PNG, con curl y con el cliente .NET.
status: borrador
origin: ia-generated
confidence: media
owner: responsable del emulador
last_review: 2026-11-27
audience: [agente, desarrollo, qa]
traces: [FIC-01, REQ-HIS-Documentacion-De-Uso, REQ-HIS-Api-De-Tickets, REQ-HIS-Cliente-Tipado]
---
# API

El contrato completo (esquemas, parámetros, códigos) está en `/openapi/v1.json` y se explora en `/scalar` ([Visor y Scalar](visor-y-scalar.md)). Esta página muestra cómo usarla; los ejemplos de bash de esta página corren en la CI contra el host real.

## Índice

1. [Rutas](#1-rutas)
2. [Estado](#2-estado)
3. [Esperar un ticket sin perderlo](#3-esperar-un-ticket-sin-perderlo)
4. [Listar, bajar y borrar](#4-listar-bajar-y-borrar)
5. [Cliente tipado de .NET](#5-cliente-tipado-de-net)
6. [Errores](#6-errores)

## 1. Rutas

| Para | Ruta |
|---|---|
| Estado del emulador y del medio | `GET /api/estado` |
| Esperar el próximo ticket | `GET /api/tickets/esperar` |
| Lista, el más nuevo primero | `GET /api/tickets` |
| El más nuevo y su PNG | `GET /api/tickets/ultimo`, `GET /api/tickets/ultimo.png` |
| Un ticket, su PNG y sus bytes | `GET /api/tickets/{id}`, `GET /api/tickets/{id}.png`, `GET /api/tickets/{id}.bin` |
| Borrar uno o todos | `DELETE /api/tickets/{id}`, `DELETE /api/tickets` |

No hay autenticación: cualquiera que llegue a la red del emulador lee y borra tickets.

## 2. Estado

<!-- ejemplo: ejecutable -->
```bash
curl -fsS "$BASE/api/estado"
```

Devuelve `version`, `medio` (por ejemplo «Bluetooth «58HB6» (Anunciando)», «simulado» o «sin medio»), `medioConectado`, `tickets` (cantidad), `ultimoId` (`null` si no hay) y `hora`. Con el medio Bluetooth, `medioConectado` es `true` cuando la impresora se está anunciando.

## 3. Esperar un ticket sin perderlo

`GET /api/tickets/esperar` responde con el primer ticket cuyo id es mayor que `despuesDe`, o `204` si vence el plazo (`segundos`, de 1 a 60; 30 si falta). El patrón: leer `ultimoId` **antes** de imprimir, imprimir y esperar con ese id (con `0` si no había tickets: todo id es mayor que `0`). Quien llama tiene que esperar la respuesta más que `segundos`.

<!-- ejemplo: ejecutable -->
```bash
ULTIMO=$(curl -fsS "$BASE/api/estado" | sed -n 's/.*"ultimoId":"\([^"]*\)".*/\1/p')

# Acá la app imprime en «58HB6».

TICKET=$(curl -fsS --max-time 75 "$BASE/api/tickets/esperar?segundos=60&despuesDe=${ULTIMO:-0}")
ID=$(printf '%s' "$TICKET" | sed -n 's/.*"id":"\([^"]*\)".*/\1/p')
[ -n "$ID" ] || { echo "no llegó ningún ticket en 60 s" >&2; exit 1; }

curl -fsS -o "ticket-$ID.png" "$BASE/api/tickets/$ID.png"
curl -fsS -o "ticket-$ID.bin" "$BASE/api/tickets/$ID.bin"
ls -l "ticket-$ID.png" "ticket-$ID.bin"
```

Sin `despuesDe`, la espera toma un ticket posterior a los que había al recibir el pedido: si la app imprime antes de que llegue el pedido, ese ticket se pierde. Con `jq`, `ultimoId` se lee con `jq -r '.ultimoId // "0"'`.

## 4. Listar, bajar y borrar

<!-- ejemplo: ejecutable -->
```bash
curl -fsS "$BASE/api/tickets?limite=5"
curl -fsS -o ultimo.png "$BASE/api/tickets/ultimo.png"
curl -fsS "$BASE/api/tickets/ultimo"
```

`limite` va de 1 a 1000. Cada ticket trae `id`, `recibido`, `bytesBin`, `bytesPng`, `anchoPx` y `altoPx`.

Borrar todos (también los que esperan otros consumidores del mismo emulador) y ver el código de una respuesta:

<!-- ejemplo: ejecutable -->
```bash
curl -fsS -X DELETE "$BASE/api/tickets"
ID=no-existe
curl -sS -o /dev/null -w '%{http_code}\n' -X DELETE "$BASE/api/tickets/$ID"
```

El segundo imprime `404`.

## 5. Cliente tipado de .NET

`ClienteDeTickets` (proyecto `src/PrintThermal.Emulator.Client`) envuelve la API. No se publica como paquete NuGet en la versión 1.0.0: se referencia el proyecto. Recibe un `HttpClient` con la dirección base del emulador, terminada en `/`.

<!-- ejemplo: tests/PrintThermal.Emulator.Tests/Documentacion/EjemplosDelCliente.cs#imprimir-y-esperar -->
```csharp
public static async Task<byte[]> ImprimirYObtenerPngAsync(Uri emulador, Func<Task> imprimir)
{
    // El tiempo de espera del HttpClient tiene que superar el de la espera (60 s como máximo).
    using var http = new HttpClient { BaseAddress = emulador, Timeout = TimeSpan.FromSeconds(75) };
    var cliente = new ClienteDeTickets(http);

    // 1. Leer el último id antes de imprimir: así no se pierde un ticket que llegue rápido.
    var antes = await cliente.ObtenerEstadoAsync();

    // 2. Imprimir desde la app (fuera del emulador de impresora).
    await imprimir();

    // 3. Esperar el primer ticket posterior; null si vence el plazo.
    var ticket = await cliente.EsperarAsync(
            antes.UltimoId ?? ClienteDeTickets.DesdeElPrincipio, TimeSpan.FromSeconds(60))
        ?? throw new TimeoutException("No llegó ningún ticket en 60 s.");

    // 4. El PNG tal como lo generó el emulador.
    return await cliente.ObtenerPngAsync(ticket.Id)
        ?? throw new InvalidOperationException($"El ticket {ticket.Id} ya no está.");
}
```

Otros métodos: `ListarAsync`, `ObtenerUltimoAsync`, `ObtenerAsync`, `ObtenerPngDelUltimoAsync`, `ObtenerBinAsync`, `BorrarAsync` y `BorrarTodosAsync`. Los que leen un ticket devuelven `null` si no existe; `EsperarAsync` devuelve `null` si vence el plazo y rechaza plazos fuera de 1 a 60 s.

## 6. Errores

- `400`: un parámetro fuera de rango (`limite`, `segundos`).
- `404`: el ticket no existe, o todavía no hay tickets (`/api/tickets/ultimo`).
- `204`: la espera venció sin ticket.

Los errores vienen como ProblemDetails (RFC 9457), con el motivo en `detail`.
