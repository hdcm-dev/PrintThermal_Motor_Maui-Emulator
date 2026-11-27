---
id: PAR-PTE-04
titulo: API de tickets con OpenAPI y Scalar, visor Blazor y cliente tipado
version: 1.1
estado: lista
velocidad: completa
base_commit: 9dec7ce0bdb56b1ee990cc97cba6015cf79c0afb
sprint: SPR-01
epica: REQ-EPI-Acceso@v1
historias:
  - REQ-HIS-Api-De-Tickets@v1
  - REQ-HIS-Visor-Web@v1
  - REQ-HIS-Cliente-Tipado@v2
lee:
  - VIS@v1
  - REQ-EPI-Acceso@v1
  - ADR-0001@v1
  - ADR-0002@v1
  - ADR-0003@v1
  - REQ-HIS-Almacen-De-Tickets@v2
modifica:
  - REQ-HIS-Api-De-Tickets@v1
  - REQ-HIS-Visor-Web@v1
  - REQ-HIS-Cliente-Tipado@v2
  - REQ-HIS-Compilacion-Verificada@v2
appetite: dos días
pendientes:
decisiones:
  - {id: D1, tema: "versiones fijas de Microsoft.AspNetCore.OpenApi 10.0.14, Scalar.AspNetCore 3.3.0 y Microsoft.AspNetCore.Mvc.Testing 10.0.14 (nuget.org, 2026-11-27; 10.0.14 es el runtime ASP.NET Core del SDK 10.0.403)", clase: K1}
  - {id: D2, tema: "cliente HTTP tipado propio en PrintThermal.Emulator.Client, que usan el visor y las pruebas (opción A de ADR-0003)", clase: K2, adr: ADR-0003}
  - {id: D3, tema: "contrato de la API: rutas, esquema y códigos de plan.md §1 v2, con los cambios de la mesa ciega (lo concreto que ADR-0001 deja a la épica de acceso)", clase: K2, adr: ADR-0001}
  - {id: D4, tema: "almacén en memoria y fuente simulada detrás de interfaces del host (Api/), sin Bluetooth, porque PAR-PTE-02 y PAR-PTE-03 no están cerradas", clase: K1}
  - {id: D5, tema: "re-planificación: base en main 9dec7ce, REQ-HIS-Cliente-Tipado@v2, REQ-HIS-Compilacion-Verificada@v2 en modifica por Directory.Packages.props, y rutas de pruebas en las tres historias", clase: K1}
  - {id: D6, tema: "visor a escala: 384 puntos = 48 mm de impresión en un papel de 58 mm (NO VERIFICADO para la 58HB6, Especificacion-58HB6-Desde-La-Libreria §5)", clase: K1}
  - {id: D7, tema: "sin dirección configurada, el host escucha sólo en http://127.0.0.1:5080", clase: K0}
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
- REQ-05 · Cuando el cliente tipado pide el último ticket, el sistema deberá devolver el mismo PNG que la API.
  prueba: tests/PrintThermal.Emulator.Tests/Cliente/ClienteTests.cs · ElClienteDevuelveElMismoPngQueLaApi
- REQ-06 · Si no se configura una dirección de escucha, el sistema deberá escuchar sólo en la interfaz local.
  prueba: tests/PrintThermal.Emulator.Tests/Api/EscuchaTests.cs · SinConfiguracionEscuchaSoloEnLocal

## Casos borde

- Carpeta de salida vacía; id inexistente (404); borrar todos los tickets.
- Tiempo máximo de espera fuera de rango (400).

## Fuera de alcance

- Usuarios y autenticación (A3).
- Exposición fuera de local sin configuración explícita.
- Almacén en disco y carpeta de salida montable (REQ-HIS-Almacen-De-Tickets, PAR-PTE-02) y medio Bluetooth (PAR-PTE-03): esta parcela trabaja con un almacén en memoria y una fuente simulada (D4).
- Contenedor e imagen (PAR-PTE-05).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · ElPngLlegaConLaMismaHuella
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · LaEsperaVenceSinColgarse
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Api/OpenApiTests.cs · OpenApiYScalarResponden
- Criterio REQ-04 → tests/PrintThermal.Emulator.Tests/Visor/VisorTests.cs · ElVisorMuestraElUltimoTicket
- Criterio REQ-05 → tests/PrintThermal.Emulator.Tests/Cliente/ClienteTests.cs · ElClienteDevuelveElMismoPngQueLaApi
- Criterio REQ-06 → tests/PrintThermal.Emulator.Tests/Api/EscuchaTests.cs · SinConfiguracionEscuchaSoloEnLocal

## Plan

En `plan.md`. Incluye la etapa T6: recorrido del visor en navegador real (`recorrido/`, `Rules-Decisiones.md` §7), por `interfaz: n2` de REQ-HIS-Visor-Web.

## Tareas

En `tasks.md`. Pruebas primero: cada `prueba:` en rojo antes del código.

## Clarify

Esta corrida es un paso de un flujo de trabajo sin persona disponible para preguntar (2026-11-27). Cada duda se responde con una cita del corpus o queda como `supuesto:`; ninguna respuesta se atribuye a la persona.

1. ¿El visor lee el almacén o llama a la API? Respuesta del corpus: llama a la API por HTTP (ADR-0002 opción A; `Idea-Inicial.md` §7 A6, «el visor llame a la api, es un punto de testeo»).
2. ¿La API o el visor llevan autenticación? Respuesta del corpus: no (ADR-0001, opción C descartada; ADR-0002 §3, «sin usuarios»).
3. ¿Qué es el «tiempo máximo» de la espera? El corpus no lo fija. `supuesto:` parámetro `segundos` entre 1 y 60, 30 si falta; al vencer, `204`. Es parte del contrato (D3, K2): la mesa ciega lo aceptó (`evidencia/2026-11-27-mesa.md`, punto c).
4. ¿A qué escala se dibuja el ticket? El corpus no la fija para la 58HB6. `supuesto:` 384 puntos ocupan 48 mm dentro de un papel de 58 mm (D6); la especificación lo marca NO VERIFICADO.

## Complexity Tracking

## Cierre

Pendiente.
