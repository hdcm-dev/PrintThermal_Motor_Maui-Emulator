---
id: PAR-PTE-04
titulo: API de tickets con OpenAPI y Scalar, visor Blazor y cliente tipado
version: 1.0
estado: lista
velocidad: completa
base_commit: 07d938ce85db406f4d06fd0fe1b2706a6ed07854
sprint: SPR-01
epica: REQ-EPI-Acceso@v1
historias:
  - REQ-HIS-Api-De-Tickets@v1
  - REQ-HIS-Visor-Web@v1
  - REQ-HIS-Cliente-Tipado@v1
lee:
  - REQ-HIS-Compilacion-Verificada@v1
  - VIS@v1
  - REQ-EPI-Acceso@v1
  - ADR-0001@v1
  - ADR-0002@v1
  - ADR-0003@v1
  - REQ-HIS-Almacen-De-Tickets@v1
modifica:
  - REQ-HIS-Api-De-Tickets@v1
  - REQ-HIS-Visor-Web@v1
  - REQ-HIS-Cliente-Tipado@v1
appetite: dos días
pendientes:
decisiones:
  - {id: D1, tema: "versiones fijas de Microsoft.AspNetCore.OpenApi y Scalar.AspNetCore", clase: K1}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Que quien prueba obtenga el PNG de cada ticket por HTTP y lo vea en un navegador (CE-5).

## Escenarios

- Camino feliz: se pide el último ticket por la API y llega `image/png` con la huella del generado; el visor lo muestra.
- No feliz: se espera un ticket que no llega; la API responde al vencer el plazo.

## Requisitos EARS

- REQ-01 · Cuando se pide el PNG de un ticket, el sistema deberá responder `image/png` con la misma huella SHA-256 que el generado.
  prueba: tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · ElPngLlegaConLaMismaHuella
- REQ-02 · Mientras no llega un ticket, el sistema deberá responder la espera al vencer su tiempo máximo.
  prueba: tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · LaEsperaVenceSinColgarse
- REQ-03 · El sistema deberá publicar OpenAPI en `/openapi/v1.json` y la página Scalar en `/scalar`.
  prueba: tests/PrintThermal.Emulator.Tests/Api/OpenApiTests.cs · OpenApiYScalarResponden
- REQ-04 · Cuando se abre el visor, el sistema deberá mostrar los tickets pidiéndolos a la API con el cliente tipado.
  prueba: tests/PrintThermal.Emulator.Tests/Visor/VisorTests.cs · ElVisorMuestraElUltimoTicket

## Casos borde

- Carpeta de salida vacía; id inexistente (404); borrar todos los tickets.

## Fuera de alcance

- Usuarios y autenticación (A3).
- Exposición fuera de local sin configuración explícita.

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · ElPngLlegaConLaMismaHuella
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · LaEsperaVenceSinColgarse
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Api/OpenApiTests.cs · OpenApiYScalarResponden
- Criterio REQ-04 → tests/PrintThermal.Emulator.Tests/Visor/VisorTests.cs · ElVisorMuestraElUltimoTicket

## Plan

Incluye la etapa T6: recorrido del visor en navegador real (`recorrido/`, `Rules-Decisiones.md` §7), por `interfaz: n2` de REQ-HIS-Visor-Web.

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
