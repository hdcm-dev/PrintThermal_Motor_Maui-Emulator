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
  - {que: "conectar el almacén en disco (PAR-PTE-02) y el medio Bluetooth (PAR-PTE-03) al host detrás de IAlmacenDeTickets e IEstadoDelMedio: ninguna parcela de SPR-01 lo nombra", registro: LAZ-0001}
  - {que: "escala del visor con 8 puntos por mm, sin medir en la 58HB6 (D6)", registro: LAZ-0002}
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

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · ElPngLlegaConLaMismaHuella → [EV-2026-11-27-pruebas]
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Api/TicketsApiTests.cs · LaEsperaVenceSinColgarse → [EV-2026-11-27-pruebas]
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Api/OpenApiTests.cs · OpenApiYScalarResponden → [EV-2026-11-27-pruebas]
- Criterio REQ-04 → tests/PrintThermal.Emulator.Tests/Visor/VisorTests.cs · ElVisorMuestraElUltimoTicket → [EV-2026-11-27-pruebas]
- Criterio REQ-05 → tests/PrintThermal.Emulator.Tests/Cliente/ClienteTests.cs · ElClienteDevuelveElMismoPngQueLaApi → [EV-2026-11-27-pruebas]
- Criterio REQ-06 → tests/PrintThermal.Emulator.Tests/Api/EscuchaTests.cs · SinConfiguracionEscuchaSoloEnLocal → [EV-2026-11-27-pruebas]
- Criterio N2 de interfaz de REQ-HIS-Visor-Web → `recorrido/guion.md`, con las capturas y `recorrido/consola.txt` (`Rules-Decisiones.md` §7)

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

- Precondición de 03 §2 («las olas anteriores, cerradas»): esta parcela es de la ola 3 y corrió con PAR-PTE-02 y PAR-PTE-03 (ola 2) abiertas, porque el flujo de trabajo pidió trabajar contra un almacén en memoria y una fuente simulada. La dependencia «PAR-PTE-02 antes que PAR-PTE-04 por REQ-HIS-Almacen-De-Tickets» se resolvió con D4: la parcela no toca `src/PrintThermal.Emulator.Core/Almacen/**` y declara la conexión en LAZ-0001. La fusión queda en orden de olas, a cargo de otra etapa.

## Cierre

- Versiones subidas: REQ-HIS-Api-De-Tickets v1 → v2, REQ-HIS-Visor-Web v1 → v2 y REQ-HIS-Cliente-Tipado v2 → v3 (historias y `modifica`), y REQ-HIS-Compilacion-Verificada v2 → v3 (`modifica`, por `Directory.Packages.props`). Cada historia suma a sus `rutas:` la carpeta de sus pruebas.
- Changelog: «PAR-PTE-04 · API de tickets con OpenAPI y Scalar, visor Blazor y cliente tipado…» en `cdd/corpus/CHANGELOG.md`.
- Evidencia:
  - `evidencia/2026-11-27-rojo.txt`: 29 de 37 en rojo sobre el esqueleto, con las seis pruebas de REQ-01 a REQ-06 en rojo por falta de comportamiento.
  - `evidencia/2026-11-27-pruebas.txt` y `.trx`: 37 de 37 en verde con el código final.
  - `evidencia/2026-11-27-mutacion.txt`: seis mutantes, uno por REQ; los seis en rojo.
  - Todas las corridas: SDK 10.0.403 en contenedor con turno.
  - La corrida de la CI pública del PR la mide la guarda del merge y no es evidencia de esta carpeta.
- Recorrido en navegador real: `recorrido/guion.md`, nueve capturas a 1440 y 360 px y `recorrido/consola.txt`, sin errores. Se ven los estados vacío, datos (con llegada en vivo por la espera) y error, la elección de un ticket, las descargas y limpiar con confirmación. El estado «cargando» existe en el código, pero el recorrido no lo capturó: con prerenderizado no se llega a ver. La primera corrida del recorrido se descartó por un 404 de `/favicon.ico` y un rótulo equivocado en el estado de error; se corrigieron y el recorrido se repitió entero.
- Decisiones:
  - D1, D4, D5 y D6 son K1, y D7 es K0.
  - D2 (ADR-0003, opción A) y D3 (contrato de la API sobre ADR-0001) son K2. Las decidió una mesa ciega del mismo modelo en otro contexto (`evidencia/2026-11-27-mesa.md`; garantía reducida) y van al lote de K2 de la Review de SPR-01.
  - El contrato v1 cambió por la mesa en cinco puntos: espera sin `despuesDe`, ids ordenables con restricción de ruta, ProblemDetails, `limite` y documentación del patrón y del borrado.
  - ADR-0003 sigue `propuesto` en su archivo (`gobierno: humano`): no se toca sin la K3 de la persona.
- Divergencia corpus/código: ninguna que contradiga un ADR.
  - ADR-0001 deja el contrato a la épica de acceso y queda escrito en REQ-HIS-Api-De-Tickets v2.
  - `supuesto:` agregado al contrato: `despuesDe=0` («desde el principio»). Surgió al escribir la prueba del cliente: sin él, quien imprime con el almacén vacío pierde el ticket si llega antes de la espera.
- Lo que queda abierto: LAZ-0001 (conectar el almacén y el medio reales) y LAZ-0002 (escala sin medir), en `pendientes:`.
- NO VERIFICADO:
  - el visor y la API con tickets reales del medio Bluetooth (esta parcela usa la fuente simulada);
  - la escala de 8 puntos por mm en la 58HB6.
- Auditor: `evidencia/2026-11-27-auditoria.md`.
- Lo aprendido:
  - el navegador de juicio corre en un contenedor de la red puente: alcanza al servicio por la dirección del puente de Docker del equipo, no por su `127.0.0.1`;
  - una restricción `regex(...)` con corchetes escapados en la plantilla de ruta no enrutó, y una restricción propia (`IRouteConstraint`) sí.
