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
