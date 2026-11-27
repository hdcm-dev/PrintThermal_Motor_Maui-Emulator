# Auditoría refutadora · PAR-PTE-04

auditor: claude -p Claude Opus (mismo modelo que el ejecutor) en otro contexto que no vio la ejecución, sólo lectura (git log/diff/show, grep, lectura de archivos y capturas). Garantía reducida: mismo modelo.

## Ronda 1 (2026-11-27, sobre 8bb17cb)

Informe literal:

**Auditoría refutadora · PAR-PTE-04** (sólo lectura: git log/show, Grep, Read y las nueve capturas)

**Hallazgos**

1. **Bloqueante · hay un pendiente abierto sin LAZ.** `parcela.md:106` (Complexity Tracking) admite que se violó la precondición de olas de 03 §2 y dice: «La fusión queda en orden de olas, a cargo de otra etapa». Eso es una obligación abierta: no fusionar antes de PAR-PTE-02 (ola 2). No figura en `pendientes:` (`parcela.md:27-29`, sólo LAZ-0001 y LAZ-0002) ni tiene registro. «Lo que queda abierto» (`parcela.md:127`) tampoco la nombra. Además `cdd/sprints/SPR-01.md` ya marca PAR-PTE-04 como «cerrada» mientras PAR-PTE-02 sigue «lista», así que nada impide fusionar fuera de orden.
2. **Menor · la parcela figura cerrada antes de la auditoría.** `SPR-01.md` dice «cerrada», pero el frontmatter de `parcela.md:5` dice `estado: en-curso`. `tasks.md:15` marca T5 («auditoría, compuertas y PR») como `[x]`, y `evidencia/2026-11-27-auditoria.md`, citado en `parcela.md:131`, no existe. También `parcela.md:89` habla de una «etapa T6» que no está en `tasks.md`: el recorrido es T4.
3. **Menor · «los errores van en ProblemDetails» no se cumple en todos los casos.** Lo afirman `REQ-HIS-Api-De-Tickets.md:36` y el cambio 3 de la mesa. Los 400 y 404 escritos a mano sí son ProblemDetails (`EndpointsDeTickets.cs:130,143,177-181`). Pero `Program.cs` no llama a `AddProblemDetails()`. Por eso, en Production, `segundos=abc` y `limite=abc` (enlace de `int?`, `EndpointsDeTickets.cs:126,138`) probablemente responden un 400 con cuerpo vacío. Un id con punto o barra no llega a ninguna ruta y da un 404 plano. Las pruebas no lo detectan: `TicketsApiTests.cs:97,105` comprueba sólo el código de estado para `abc`. No lo ejecuté.
4. **Menor · la prueba de REQ-06 no cubre al host, y queda un efecto del contenedor sin declarar.** `EscuchaTests.cs:16-22` prueba sólo el ayudante `Direcciones.EscuchaSinConfigurar`. Si se borra `UseUrls` en `Program.cs:12-15`, la prueba sigue en verde, y el mutante M6 cambia el ayudante, no el host. La propia evidencia verde muestra `Now listening on: http://*:8080` (`2026-11-27-pruebas.txt:944,972`). La imagen oficial de .NET trae `ASPNETCORE_HTTP_PORTS=8080`, así que en cualquier contenedor oficial el valor por omisión de D7 nunca se aplica y el host escucha en todas las interfaces sin que nadie lo configure. Eso choca con «sólo en local salvo configuración» (ADR-0001) y le toca a PAR-PTE-05, pero no hay LAZ ni nota. El guion tampoco dice cómo escuchaba el servidor del recorrido.
5. **Menor · una condición de la mesa sobre D2 quedó a medias.** `mesa.md:11` pide «una prueba que recorra cada operación del cliente contra el host real». En `ClienteTests.cs`, `ObtenerAsync` sólo se prueba con un id inexistente (`:38`) y `ListarAsync(limite)` no se prueba nunca. `REQ-HIS-Cliente-Tipado.md:24` afirma que el cliente «cubre cada ruta», pero la cobertura no está probada.
6. **Menor · la mutación no es reproducible y el informe es un extracto.** `2026-11-27-mutacion.txt` registra cada mutante sólo como «1 file changed», sin el diff. En M6 (`:44-46`), «Error Message:» aparece vacío, a diferencia de los otros cinco. El archivo está filtrado, no es la salida literal.
7. **Menor · el recorrido no muestra todo lo que el Cierre da por visto.** El estado vacío tras limpiar «ya se había visto en la primera corrida, descartada» (`guion.md:38`), así que no hay captura válida. El estado «cargando» no se capturó (`guion.md:44`). Además, la explicación de que «con prerenderizado no se llega a ver» es dudosa: el circuito interactivo crea el componente de nuevo con `_tickets == null` (`Inicio.razor:116,125`), así que el esqueleto aparece un instante en cada carga. Es un parpadeo de datos a cargando a datos que no se midió. `REQ-HIS-Visor-Web.md:33` sigue afirmando cuatro estados.
8. **Observación · consola.** En `consola.txt:13-16` faltan los `msgid` 177 y 178 entre el bloque de 360 px y el de error, aunque el archivo se presenta como «copiado literal salvo la dirección». Que la consola esté sin errores es creíble: los errores del visor ocurren del lado del servidor, en Blazor Server. Pero ese hueco no tiene explicación.
9. **Observación · bucle del visor.** En `Inicio.razor:161-167`, si `/esperar` responde pero `CargarAsync` falla (por ejemplo, un 500 en `/api/estado`), `_tickets` no avanza. `despuesDe` queda fijo, la espera vuelve al instante con el mismo ticket y se repite sin pausa, porque la pausa de 3 s sólo se aplica a las excepciones de la espera.
10. **Observación · almacén.** No encontré pérdidas de aviso en la espera: `Guardar` cambia la señal bajo el mismo cerrojo con el que `EsperarAsync` revisa la lista (`AlmacenEnMemoria.cs:43-62,133-146`). `/api/estado` lee `Listar().Count` y `Ultimo()` bajo dos cerrojos distintos, así que puede devolver datos que no coinciden entre sí (`EndpointsDeTickets.cs:35-37`). Con más de 999 999 tickets en el mismo milisegundo, `D6` deja de ordenar (`AlmacenEnMemoria.cs:156`); no tiene consecuencias prácticas.
11. **Observación · la mesa pidió dejar escrito que los cambios del contrato serán sólo aditivos.** Está en `plan.md:15` y en un comentario del código, pero no en la historia `REQ-HIS-Api-De-Tickets` v2.
12. **Observación · higiene del repo público.** No encontré IPs fuera de loopback, MAC, credenciales ni rutas personales en lo agregado. La dirección del servidor está reemplazada por `<emulador>` y la captura de Scalar se omitió a propósito. El `.trx` lleva `computerName="26e17d74f23c"` (`pruebas.trx:2,8+`): parece el id efímero de un contenedor, no un equipo interno, pero conviene confirmarlo o quitarlo.

**Verificado sin objeciones**
- **REQ-01 a REQ-06:** las seis pruebas existen y fallan en `rojo.txt` por comportamiento (404, NotFound, `NotImplementedException`, falta de `data-estado`), no por compilación. Pasan las 37 de 37 en `pruebas.txt:997-998`, y cada mutante M1 a M6 queda en rojo.
- **Pruebas sin cambios:** las de 386c214 no se tocaron después, así que el rojo y el verde corresponden a las mismas pruebas.
- **EARS:** las pruebas los verifican. REQ-01 hace 5 de 5 con SHA-256, REQ-02 vence en 204 en menos de 10 s y REQ-04 registra los pedidos del visor.
- **Capturas:** las nueve coinciden con el guion en hora, cantidad, elegido, confirmación, error con `127.0.0.1:9` y 360 px sin desborde.
- **Contrato:** los cinco cambios de la mesa están implementados (espera sin `despuesDe`, ids ordenables con restricción propia, `limite`, descripciones de la espera y del borrado, 204 y corte por `RequestAborted`).

**Lo que quedó abierto y no está declarado:**
- El orden de fusión por olas: PAR-PTE-04 no debe entrar antes que PAR-PTE-02. Está en Complexity Tracking sin registro ni pendiente, y SPR-01 ya la da por «cerrada».
- En contenedores oficiales, `ASPNETCORE_HTTP_PORTS=8080` deja sin efecto la escucha local de D7 (ADR-0001). Hace falta un LAZ hacia PAR-PTE-05.
- ProblemDetails no cubre los errores de enlace de parámetros ni las rutas que no coinciden.
- La cobertura completa del cliente que pidió la mesa (D2).
- La evidencia válida del estado «cargando» y del vacío tras limpiar.
- La auditoría que el Cierre cita (`auditoria.md`) todavía no existe, aunque T5 figura `[x]`.

VEREDICTO: refutado

### Respuesta a la ronda 1

Ver «## Cierre» de la parcela, punto «Auditoría». Cada hallazgo se corrigió en código o en pruebas, o quedó registrado en LAZ-0003 y LAZ-0004.

## Ronda 2 (2026-11-27, sobre 5ce52a4)

auditor: claude -p Claude Opus (mismo modelo) en otro contexto nuevo, que no vio la ejecución ni la ronda 1 salvo por este archivo; sólo lectura. Garantía reducida.

Informe literal:

**Auditoría refutadora · PAR-PTE-04 · ronda 2** (sólo lectura, sobre 5ce52a4)

**Hallazgos**

1. **Bloqueante · la respuesta dice que todos los hallazgos de la ronda 1 se resolvieron, pero el n.º 2 sigue igual.** `parcela.md:139` afirma: «Cada hallazgo se corrigió en código o en pruebas, o se registró». La lista de nueve puntos que sigue no menciona el n.º 2, y nada cambió:
   - `cdd/sprints/SPR-01.md:24` sigue dando PAR-PTE-04 por «cerrada»; 5ce52a4 no toca SPR-01.
   - `parcela.md:5` sigue en `estado: en-curso`.
   - `tasks.md:15` marca T5 («auditoría, compuertas y PR») `[x]`, con la auditoría todavía abierta.
   - Con LAZ-0004 abierto, el tablero sigue diciendo que la parcela terminó, que es justo lo que la ronda 1 señaló.
2. **Menor · el Cierre vuelve a citar algo que todavía no existe.** `parcela.md:150` dice «La segunda ronda está en el mismo archivo», pero `auditoria.md` sólo tiene la ronda 1 y la respuesta (`:43-45`). Es el mismo defecto que la ronda 1 marcó con `auditoria.md`.
3. **Menor · la mutación se rehízo, pero M3 sigue sin diff.** `mutacion.txt:27-29` trae los encabezados `---`/`+++` sin ninguna línea `-` ni `+`. Probablemente el filtro descartó `-app.MapScalarApiReference();` por no tener sangría. M3 no se puede reproducir, así que el n.º 6 de la ronda 1 queda resuelto sólo en parte. El archivo sigue siendo un extracto («con su prueba filtrada», `:1`).
4. **Menor · «ProblemDetails… con pruebas» se probó sólo en Development.** `pruebas.txt:45` muestra `Hosting environment: Development`. En `:164-173`, el `problem+json` de `segundos=abc` lo produce `DeveloperExceptionPageMiddleware`. La ronda 1 hablaba de Production (`ThrowOnBadRequest=false`), y ese camino depende sólo de `UseStatusCodePages` (`Program.cs:49`). Probablemente funciona, pero ninguna prueba ni evidencia lo cubre. `REQ-HIS-Api-De-Tickets.md:37` lo afirma igual.
5. **Menor · REQ-06 sigue sin una guarda automática sobre el host.**
   - `EscuchaTests.cs:16-22` prueba sólo `Direcciones`. Si se borra `UseUrls` en `Program.cs:12-15`, las 38 pruebas siguen en verde.
   - `escucha.txt` es una medición manual. No trae el comando, ni cómo se quitó `ASPNETCORE_HTTP_PORTS`, ni cómo se obtuvo la IP. No se puede repetir.
   - Está declarado en el Cierre, así que no es un pendiente oculto, pero queda una regresión posible que ninguna prueba detecta.
6. **Menor · la consola no cubre todas las navegaciones, aunque `guion.md:54` dice que sí.**
   - Los tickets llegaron a las 15:30:09 y 15:30:49 (`1440-datos-dos-tickets.png`). El primer bloque de `consola.txt:7-9` es de las 15:30:53, o sea, la recarga del paso 4.
   - Falta la navegación inicial de los pasos 1 a 3, que es donde se juzga la llegada en vivo.
   - El encabezado (`:4-5`) explica huecos en los `msgid` «por otras pestañas», pero ahora 222-231 son correlativos. La explicación quedó vieja y no tapa la navegación que falta.
7. **Observación · el visor usa un plazo de 75 s para todos los pedidos.** `Program.cs:44` aplica `EsperaMaxima+15` también a `/api/estado` y a la lista. Con la API inalcanzable, la vista queda 75 s en «cargando». Mientras dura el renderizado en flujo no hay interactividad: `blazor.web.js` recién se conecta a las 15:32:53 (`consola.txt:18-19`). El recorrido lo muestra y la historia lo admite (`REQ-HIS-Visor-Web.md:35`), así que no hace falso ningún criterio.
8. **Observación · el visor lee su estado fuera del dispatcher.** `Inicio.razor:181-193` lee `Tickets` y `ErrorDeCarga` fuera de `InvokeAsync`. Además, `LimpiarAsync` y el bucle pueden intercalar dos `CargarAsync`. Una lista vieja después de limpiar se corrige en 25 s o menos. Es benigno.
9. **Observación · `despuesDe` no se valida.** `EndpointsDeTickets.cs:138` acepta cualquier texto. Con `despuesDe=abc` (las letras ordenan después de los dígitos), la espera nunca devuelve nada y siempre responde 204. El contrato dice «ids… mayores que 0», pero no hay 400.
10. **Observación · la UI muestra el texto crudo de la excepción, en inglés.** `Inicio.razor:168` lo pasa tal cual: «Connection refused (127.0.0.1:9)», «The request was canceled…». Con un `Visor:UrlApi` real, la página expondría el host interno.
11. **Observación · una promesa a otra parcela sin registro.** `plan.md:36` dice que el riesgo de DELETE «lo dirá la documentación de uso (PAR-PTE-06)». No hay LAZ ni pendiente. La descripción en el OpenAPI (`EndpointsDeTickets.cs:106`) ya cumple lo que pidió la mesa, así que es menor.

**Verificado sin objeciones**
- REQ-01 a REQ-06: las pruebas existen y fallan en `rojo.txt` por comportamiento (NotFound, `NotImplementedException`, falta de `data-estado`). Pasan en `pruebas.txt` (38 de 38), y M1 a M6 quedan en rojo.
- El refuerzo de REQ-05 (`ClienteTests.cs:24-30`) recorre ahora todas las operaciones del cliente, como pidió la mesa para D2.
- Ronda 1, resueltos:
  - n.º 1: LAZ-0004;
  - n.º 4: LAZ-0003 y `escucha.txt`;
  - n.º 5: cobertura del cliente;
  - n.º 7: capturas de «cargando» y del vacío tras limpiar, coherentes con el guion;
  - n.º 9: pausa tras fallar la recarga;
  - n.º 10: `/api/estado` con una sola lectura;
  - n.º 11: «sólo aditivo» escrito en `REQ-HIS-Api-De-Tickets.md:36`.
- El almacén no pierde avisos: `Guardar` cambia la señal bajo el mismo cerrojo con que la espera revisa la lista. Los cinco cambios de la mesa están implementados.
- Higiene: no hay IPs fuera de loopback, ni MAC, ni credenciales, ni rutas personales. `<emulador>` y `<ip del contenedor>` están enmascarados. El `computerName` del TRX es un id de contenedor (`7a5d830aff22`).

**Lo que quedó abierto y no está declarado:**
- SPR-01 da por «cerrada» una parcela `en-curso`, con auditoría abierta, y T5 figura `[x]`. El Cierre dice que todo lo de la ronda 1 quedó resuelto, y esto no lo está.
- La segunda ronda que cita `parcela.md:150` todavía no existe.
- M3 sigue sin diff.
- ProblemDetails en Production, para errores de enlace, no está medido.
- No hay una guarda automática de REQ-06 sobre el host.
- Falta la consola de la primera navegación del recorrido.
- La documentación del riesgo de DELETE se promete a PAR-PTE-06 sin registro.

VEREDICTO: refutado

### Respuesta a la ronda 2

Ver «## Cierre» de la parcela, punto «Auditoría»: lo corregido (estado, mutación con diff completo, ProblemDetails en Production, comandos de la escucha, consola declarada, trabajo de PAR-PTE-06 nombrado) y las observaciones aceptadas sin cambio. Lo que queda abierto está en `pendientes:` (LAZ-0001 a LAZ-0004).
