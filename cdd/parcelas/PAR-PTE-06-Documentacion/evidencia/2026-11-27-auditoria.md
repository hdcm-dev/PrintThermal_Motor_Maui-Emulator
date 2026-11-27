# Auditoría del cierre de PAR-PTE-06 (2026-11-27)

auditor: agente refutador Claude (mismo modelo que la ejecución) en otro contexto, sin la conversación de la ejecución, sólo lectura — garantía reducida

## Ronda 1 (sobre 1740a58 más lo sin commitear)


Revisé `git diff origin/main...HEAD`, lo que está sin commitear (incluida la sección `## Cierre`, que se escribió mientras auditaba), docs/, las dos pruebas nuevas, la evidencia de PAR-PTE-06 y PAR-PTE-05, los LAZ citados y src/, el Dockerfile y los workflows. No ejecuté nada: todo sale de leer el código y la evidencia.

### Hallazgos que bloquean

**1. Falta la evidencia de verde que cita el cierre.**
- Dónde: `cdd/parcelas/PAR-PTE-06-Documentacion/parcela.md:80`.
- Qué pasa: el cierre cita `evidencia/2026-11-27-pruebas.txt` y `.trx`, pero en `evidencia/` sólo están `rojo.txt` y `mutacion.txt`, y esta última sin commitear. El único verde a la vista es la línea `== base` de la mutación, que corre sólo `DocumentacionTests` (2 de 2). No hay verde de la solución completa.
- Corrección: correr la solución completa sobre la cabeza, agregar los dos archivos y commitear también `mutacion.txt`.

**2. REQ-02 no se cumple para tres de las seis claves de configuración documentadas.**
- Dónde: `tests/.../DocumentacionTests.cs:200-201` y `:355-366`, frente a `docs/montaje.md:96-99`.
- Qué pasa: `VariableDeEntorno()` sólo mira los bloques de código y sólo cuando la clave va seguida de `=`. `Emulador__Salida`, `Emulador__Bluetooth__ArchivoDeClaves` y `Visor__UrlApi` aparecen únicamente en la tabla de §5, así que nadie las controla.
- Mutante de código que sobrevive: renombrar `Configuration["Visor:UrlApi"]` en `Program.cs:76`, o la propiedad `ArchivoDeClaves` en `MedioBluetooth.cs:19`. `docs/` queda desactualizada y las dos pruebas siguen en verde. Lo mismo pasa con `Emulador:Salida`, cambiando también el `ENV` del Dockerfile.
- Por qué la mutación no lo detecta: los 8 mutantes (`mutacion.txt:5-12`) cambian sólo `docs/`. Ninguno cambia el código, que es justo el caso que pide REQ-02.
- Corrección:
  - Extraer también las claves `X__Y` entre comillas invertidas de la prosa y las tablas, y compararlas con `conocidas`.
  - Sumar a la evidencia al menos un mutante de código por clase: ruta, clave y región del cliente.

### Hallazgos menores

**3. Estado de la parcela incoherente.**
- Dónde: `parcela.md:5` dice `estado: en-curso`, mientras `SPR-01.md` la marca «cerrada» y el cierre ya está escrito.
- Corrección: pasar el estado a cerrada, según la norma.

**4. Ejemplos ejecutables que no comprueban lo que la documentación afirma.**
- `docs/api.md:86-90`: el texto dice «el segundo imprime `404`», pero `-w '%{http_code}'` sin `-f` nunca falla. Si el código respondiera 204, la prueba pasaría igual. Corrección: `[ "$(curl … )" = 404 ]`.
- `docs/api.md:55-60`: si `ultimoId` se renombra, `sed` no encuentra nada, la espera va con `despuesDe=0` y devuelve un ticket viejo. El ejemplo sale con 0 y se rompe en silencio el patrón «sin perderlo». Corrección: asertar que `ID` es mayor que `ULTIMO`, o contrastar los nombres de campo citados (`ultimoId`, `medioConectado`, `bytesBin`…) con los esquemas de `/openapi/v1.json`.

**5. Valores citados en docs/ que la prueba no contrasta con el código.**
- Valores por omisión y rangos: `04:00:00:00:00:01` (`montaje.md:95`), `segundos` de 1 a 60 con 30 por omisión (`api.md:51`), `limite` de 1 a 1000 (`api.md:79`). Un cambio en `OpcionesDelMedio` o en `EndpointsDeTickets` no hace fallar nada.
- Versión de la imagen: el patrón (`DocumentacionTests.cs:397`) acepta cualquier versión semántica, por ejemplo `9.9.9`, y «1.0.0» no se ata a la etiqueta `v1.0.0`.
- Puerto: `-p 127.0.0.1:5080:5080` y el «5080» de la prosa no se controlan (`:403-409`).
- Corrección: sumar constantes y valores por omisión por reflexión, y comprobar que la versión citada exista como etiqueta.

**6. Mensajes y estados de diagnóstico sin control, y una tabla incompleta.**
- `docs/diagnostico.md:43` y `:61`: `periférico: <motivo>; reintento en 2 s` y `conexión N: <aviso>` no van en un bloque `registro`, así que no se verifican.
- La tabla de `medio` (`diagnostico.md:36-41`) tampoco se verifica, y le faltan dos estados que existen en `EstadoPeriferico`: `Iniciando` (llegó a netsim y todavía no se anuncia) y `Detenido`.
- Corrección: agregar las filas y mover esas líneas a bloques `<!-- ejemplo: registro -->`.

**7. Acceso desde Android sin respaldo completo.**
- Dónde: `docs/montaje.md:86`.
- Qué pasa: dice «así se capturó el visor en PAR-PTE-05», pero esa evidencia (`extremo-a-extremo.md:65`) cuenta que se abrió un reenvío TCP temporal en la puerta de enlace del puente de Docker. Además, la página no da la dirección: un agente sin contexto no la sabe.
- Corrección: nombrar `10.0.2.2:5080`, que es pública en la documentación de Android, y marcar NO VERIFICADO sin el reenvío, o describir el reenvío.

**8. `-v <carpeta de salida>:/data` sin aclarar que va con ruta absoluta.**
- Dónde: `docs/montaje.md:57` y `recetas.md:47`.
- Qué pasa: con una ruta relativa, Docker crea un volumen con nombre y los tickets no aparecen en la carpeta del equipo.
- Corrección: decir «ruta absoluta».

**9. Los `«»` del campo `medio` probablemente llegan escapados.**
- Qué pasa: `System.Text.Json` escapa por omisión lo que no es ASCII, así que en la salida cruda de curl `medio` saldría como `Bluetooth \u00AB58HB6\u00BB …`. Un agente que haga `grep «58HB6»` no lo encuentra. Lo deduzco del código; no lo medí.
- Corrección: advertirlo en `api.md` §2 y `diagnostico.md` §2, y recomendar `jq`.

**10. LAZ-0013 dice algo que docs/ no cumple.**
- Dónde: `cdd/lazo/LAZ-0013…md:35` afirma «`docs/` marca cada uno NO VERIFICADO».
- Qué pasa: el `docker pull` y el `docker run` con la imagen publicada (`montaje.md:38`, `:56-60`) no llevan la marca. Al contrario, `:64` presenta como medido lo que se midió con una imagen local de `5066fa3`.
- Corrección: marcar NO VERIFICADO con la imagen publicada `1.0.0`.

**11. Cosas abiertas del cierre sin LAZ en `pendientes:`.**
- «Otras API de Android que la 37» (`parcela.md:84`, `que-es-y-limites.md:43`).
- El PIN `0000` del emparejamiento heredado, sin ejercitar (`que-es-y-limites.md:36`).
- El desborde del visor a 540 px, que figura como «hallazgo abierto» (`visor-y-scalar.md:27`) pero no tiene LAZ en ningún lado.
- La fidelidad frente a la 58HB6 física: ya existe LAZ-0002, pero no se cita.
- La imagen privada: hacerla pública lo decide la persona (plan de PAR-PTE-05). El camino principal de `montaje.md` no funciona sin credenciales, y LAZ-0013 sólo trata la alternativa con `docker build`.
- Corrección: citar LAZ-0002 y abrir o citar LAZ para el resto, o llevarlo como K3 a la Review.

Los tres LAZ que sí se citan tratan su tema: LAZ-0011 cubre el vínculo trabado y quién confirma el diálogo, LAZ-0001 el ticket sin corte y LAZ-0013 los pasos con Docker, salvo lo dicho en el hallazgo 10.

**12. La ficha dice que se llega a docs/ desde `AGENTS.md`, pero no hay enlace.**
- Dónde: `docs/diseno-documental.md:65` (D-12).
- Qué pasa: `AGENTS.md` no enlaza `docs/`.
- Corrección: agregar el enlace o corregir D-12.

**13. D-09 y D3 sin testimonio y con clase contradictoria.**
- Dónde: `diseno-documental.md:63,86` y `parcela.md:31,82`.
- Qué pasa: D-09 dice «decidida por delegación de la persona» sin testimonio citado. Además, la ficha la trata como de clase H (para una persona), mientras la parcela registra D3 como K1.
- Corrección: citar dónde consta la delegación y alinear la clase.

**14. Huecos estructurales de la prueba.**
- `Enlace()` (`DocumentacionTests.cs:215`) no valida anclas (`#…`) ni las referencias «§N».
- REQ-01 no comprueba que existan las páginas de montaje y del visor. Se puede borrar `montaje.md`, junto con los enlaces que apuntan a ella, y todo queda en verde, porque `recetas.md` §2 cumple con el volumen y la variable.
- Corrección: validar las anclas contra los títulos y exigir la lista de páginas del índice.

**15. Cobertura para un agente sin contexto.**
- La receta de workflow (`recetas.md:84-92`) es una lista en prosa, sin un YAML.
- Para crear el vínculo con una app que sólo lista equipos vinculados (`emparejar-e-imprimir.md:55`) no hay ningún paso concreto: ni la «app mínima» ni un comando.
- Corrección: agregar un YAML mínimo y un paso concreto, o marcarlos como pendientes.

**16. Lo pedido está, con los huecos ya señalados.** Están qué es y sus límites (PIN, equipo físico, iOS), el montaje con versión fija y `--network container:`, las variables, el puerto y la carpeta, emparejar e imprimir, curl y el cliente tipado con la espera, el visor y Scalar, y el diagnóstico de los cuatro casos. La API no repite lo que ya documenta Scalar.

### Repositorio público

No encontré hosts, IP internas, rutas del equipo, nombres de sesiones ni credenciales en docs/, el README, las pruebas, la parcela ni la evidencia. La dirección del teléfono va enmascarada (`XX:XX:XX:XX:BB:BB`) y las de la impresora son sintéticas. `/w/docs` en `rojo.txt` es una ruta del contenedor.

Veredicto: refuta

## Respuesta a la ronda 1

| Hallazgo | Respuesta |
|---|---|
| 1 Verde ausente | `2026-11-27-pruebas.txt` y `.trx` sobre la cabeza; `mutacion.txt` versionado |
| 2 Claves de la prosa sin control | La prueba lee también las claves `X__Y` entre comillas invertidas de la prosa y las tablas; mutantes de código C1 (clave), C2 (ruta), C3 (región) en `mutacion.txt` |
| 3 Estado | `estado: cerrada` en el commit de cierre |
| 4 Ejemplos que no comprueban | El 404 se comprueba; la espera exige un id posterior a `ultimoId`; los campos JSON citados en `docs/api.md` se contrastan con los esquemas de `/openapi/v1.json` |
| 5 Valores sin contraste | Dirección por omisión (por reflexión), rangos y plazo por omisión (constantes de `EndpointsDeTickets`) y puerto de `-p` bajo prueba. La versión se ata a la que declara `docs/README.md`, no a una etiqueta de git (la CI clona sin etiquetas): declarado en el cierre |
| 6 Mensajes y estados | Las dos líneas pasan a bloques de registro; filas «Iniciando» y «Detenido» en la tabla. La tabla de estados no está bajo prueba |
| 7 Acceso desde Android | Se dice que hizo falta un reenvío TCP y el camino directo queda NO VERIFICADO (LAZ-0013); no se cita una dirección que no se midió |
| 8 Ruta absoluta | Agregado en montaje, diagnóstico y convenciones |
| 9 Escape de «» | Advertido en `docs/api.md` §2 como posible (NO VERIFICADO con el medio Bluetooth) |
| 10 LAZ-0013 | `docs/montaje.md` §2 marca NO VERIFICADO el montaje con la imagen publicada |
| 11 Abiertos sin LAZ | LAZ-0014 (imagen privada). La API de Android, el PIN heredado y la fidelidad se nombran en el cierre como alcance, no como pendientes (no cambian `docs/` si son falsos). La mención del desborde del visor se quitó de `docs/` (es de REQ-HIS-Visor-Web, fuera de esta parcela) |
| 12 D-12 | Corregido: desde `README.md` |
| 13 D-09 y D3 | La ficha dice que la delegación consta fuera del repositorio y que en CDD es D3, K1 |
| 14 Anclas y páginas | La prueba valida anclas contra los títulos y exige las páginas de la estructura enlazadas desde el índice (mutantes M9 y M12) |
| 15 Workflow y vínculo | Esqueleto YAML ilustrativo y NO VERIFICADO (LAZ-0013); el paso para crear el vínculo dice qué hace la app de prueba y que la usada no está publicada |

## Ronda 2 (sobre fde0679, mismo auditor en un contexto nuevo)

### 1. Hallazgos de la ronda 1

| # | ¿Resuelto? | Dónde se ve |
|---|---|---|
| 1 Verde ausente | No cuenta (excluido por consigna). `parcela.md:81` cita `pruebas.txt`/`.trx`, que todavía no están en `evidencia/` | — |
| 2 Claves de la prosa | **Sí.** `DocumentacionTests.cs:369-381` lee las claves `X__Y` entre comillas invertidas. `Emulador:Salida` y `Visor:UrlApi` se toman de `Program.cs:21,76` y `ArchivoDeClaves` por reflexión (`:346-353`). Renombrar cualquiera en el código hace fallar la prueba | — |
| 3 Estado | **No en la cabeza.** `parcela.md:5` sigue en `estado: en-curso`, mientras `SPR-01.md` ya la da por cerrada. La respuesta lo deja para el commit de cierre | menor |
| 4 Ejemplos | **Sí.** El 404 se comprueba (`api.md:90-91`). Con `ultimoId` vacío, la comparación de `api.md:64` siempre da verdadero, pero el nombre del campo queda controlado por `CamposJson` (`DocumentacionTests.cs:414-467`, contra `api.md:47`) | — |
| 5 Valores | **Sí.** Dirección (`:391-395`), rangos y plazo (`:397-407`) y puerto de `-p` (`:514-520`). Que la versión quede atada a `docs/README.md:16` y no a una etiqueta de git está declarado en `parcela.md:87` | — |
| 6 Mensajes y estados | **Sí.** `diagnostico.md:48-51` y `:73-76` están en bloques de registro y coinciden con `ServicioPeriferico.cs:84` e `Interprete.cs:761`. La tabla `:37-44` coincide con `EstadoPeriferico` y `MedioBluetooth.cs:48`. Sin prueba ni el «2 s» ni la tabla, como se declaró | — |
| 7 Acceso desde Android | **Sí** (`montaje.md:87`) | — |
| 8 Ruta absoluta | **Sí** (`montaje.md:31,65`, `diagnostico.md:29`, `docs/README.md:42`) | — |
| 9 Escape de «» | **Sí** (`api.md:49`) | — |
| 10 LAZ-0013 | **Sí** (`montaje.md:41`) | — |
| 11 Abiertos | **Sí**, con LAZ-0014 y el alcance declarado en `parcela.md:87`. Queda sin citar LAZ-0002 (fidelidad), aunque el cierre lo da como heredado | — |
| 12 D-12 | **Sí** (`diseno-documental.md:65`) | — |
| 13 D-09 / D3 | **Sí**, en lo posible: `diseno-documental.md:86` declara que el testimonio está fuera del repositorio | — |
| 14 Anclas y páginas | **Sí** para las anclas (`:652-656`, `:684-702`) y las páginas (`:665-681`). Las referencias «§N» siguen sin prueba; las revisé a mano y todas apuntan bien | menor |
| 15 Workflow y vínculo | **Sí** (`recetas.md:92-113`, `emparejar-e-imprimir.md:55`) | — |

### 2. Hallazgos nuevos

**1. `docs/recetas.md:43-51` y `:84-113` contradicen el uso aprobado de la imagen.** — **bloquea**
- Qué dice docs/: proponen correr la imagen sola, sin el emulador de Android, con `-p 127.0.0.1:5080:5080`, tanto en el equipo como en un runner de GitHub.
- Qué dice el repositorio:
  - El `Dockerfile:3-4` y el `README.md:36` dicen «Uso previsto (aprobado por INFRA) … dentro del mismo turno y puesto de emulador … nunca queda viva sola».
  - Ese uso es la decisión D4 de PAR-PTE-05, de clase K0 (`PAR-PTE-05-Distribucion/parcela.md:34`).
  - La propia `docs/montaje.md:114` dice «no queda vivo solo».
- Por qué bloquea: no hay decisión ni LAZ que amplíe el uso previsto. Un agente que siga §2 en el equipo del equipo de trabajo sale de lo aprobado.
- Corrección: elegir una de dos.
  - Acotar §2 y §5 a equipos y runners ajenos a ese uso, diciendo explícitamente que quedan fuera del uso aprobado.
  - Abrir un LAZ o una decisión para la persona y sumarlo a `pendientes:`.

**2. El changelog no registra LAZ-0014.** — **menor**
- `cdd/corpus/CHANGELOG.md:14` dice «LAZ-0013 abierto», pero LAZ-0014 también se abre en esta parcela.

**3. El índice de docs/ describe de menos lo que se controla.** — **menor**
- `docs/README.md:50` dice «cada variable `Emulador__…`», pero la prueba también controla `Visor__UrlApi`.

Datos privados: en lo nuevo (docs/, LAZ-0013, LAZ-0014, la parcela, la evidencia y las pruebas) no aparecen hosts, IP internas, rutas del equipo, nombres de sesiones ni credenciales. `GHCR_TOKEN` es el nombre de un secreto, no un valor, y la dirección del teléfono va enmascarada. El resto de lo que afirma docs/ coincide con src/, el `Dockerfile` y `release.yml`: claves, valores por omisión, `uid 1654`, `/data`, el puerto 5080, los estados y las plantillas de registro.

Veredicto: refuta

## Respuesta a la ronda 2

| Hallazgo | Respuesta |
|---|---|
| R1-3 Estado | `estado: cerrada` |
| R1-11 LAZ-0002 | No se cita: LAZ-0002 trata respuestas, demoras y longitudes de protocolo, no la fidelidad del dibujo; la fidelidad se nombra en el cierre como heredada |
| R1-14 «§N» | Sin prueba, revisadas a mano por el auditor; queda así |
| Nuevo 1 Imagen sola en las recetas | `docs/recetas.md` §2 y §5 la acotan como uso fuera del previsto, sólo para equipos propios o runners de CI efímeros, y remiten al montaje donde rige el uso previsto; decisión D4 de la parcela (K1); el uso previsto (D4 de PAR-PTE-05) no cambia |
| Nuevo 2 Changelog | Nombra LAZ-0013 y LAZ-0014 |
| Nuevo 3 Índice | Dice `Emulador__…` o `Visor__…` |

No se corrió una tercera ronda.
