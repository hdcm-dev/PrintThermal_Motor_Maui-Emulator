# Auditoría del cierre de PAR-PTE-02

auditor: agente auditor Claude Opus 5.5 (Claude Code sin interfaz, `claude -p`), contexto nuevo que no vio la ejecución, sólo lectura · garantía reducida: es el mismo modelo que ejecutó la parcela; no había otra familia de modelos disponible.

## Índice

1. [Informe del auditor](#1-informe-del-auditor)
2. [Qué se hizo con cada hallazgo](#2-qué-se-hizo-con-cada-hallazgo)
3. [Segunda ronda](#3-segunda-ronda)
4. [Qué se hizo en la segunda ronda](#4-qué-se-hizo-en-la-segunda-ronda)
5. [Tercera ronda](#5-tercera-ronda)
6. [Qué se hizo en la tercera ronda](#6-qué-se-hizo-en-la-tercera-ronda)

## 1. Informe del auditor

Texto tal como lo devolvió, sobre la cabeza `8d7ce53` (los rebases sobre `main` cambian el SHA: el mismo árbol de `src/` y `tests/` es hoy el del commit «PAR-PTE-02: intérprete ESC/POS…» de la rama) más cambios sin commitear (el tope de carga, que se estaba escribiendo durante la auditoría):

> Hay dos hallazgos graves: el cierre no se sostiene tal como está en el árbol.
>
> **Graves**
>
> 1. **La evidencia no corresponde al código que se va a cerrar.**
>    - Mientras auditaba, alguien modificó `src/PrintThermal.Emulator.Core/EscPos/Interprete.cs:23,241,249,415` (agrega `CargaMaxima`/`Medida`) y `InterpreteTests.cs` (agrega `UnaCargaDesmesuradaSeDescartaSinRomperNiBloquear`). Ninguno de los dos cambios está commiteado.
>    - `evidencia/2026-11-27-pruebas.txt`, `.trx` y `mutacion.txt` son de antes de ese cambio. Dicen 34 de 34 y no incluyen la prueba nueva, que tampoco tiene corrida en rojo. Si se commitea así, lo que dice `## Cierre` sobre la evidencia es falso.
>    - Si no se commitea, HEAD (`8d7ce53`) queda con un defecto real. `1D 76 30 00 FF FF FF FF` desborda el `int` (65535 × 65535), el largo sale negativo y `b[..largo]` lanza `ArgumentOutOfRangeException` dentro de `Alimentar`. Como el búfer pendiente no se compacta, cada llamada siguiente vuelve a fallar con los mismos bytes: la conexión queda muerta. Lo mismo pasa con GS 8 L cuando p4 ≥ 0x80. Eso contradice REQ-02 («se registra y sigue»).
>
> 2. **REQ-03 no está implementado de punta a punta y el pendiente queda oculto.**
>    - REQ-03 dice «Cuando termina un ticket, el sistema deberá guardar…». Ningún código conecta `Interprete.TicketTerminado` con `AlmacenDeTickets.Guardar`.
>    - La prueba (`AlmacenTests.cs` · `CadaTicketGuardaBinYPng`) llama a `Guardar` a mano. Pasaría igual con un sistema que nunca guarda nada.
>    - El traspaso a PAR-PTE-03 en `## Cierre` (`parcela.md:92`) sólo pide `Finalizar` y `Respuesta`. La parcela PAR-PTE-03 no menciona el almacén y PAR-PTE-04 sólo lo lee.
>    - Tampoco está decidido qué pasa con un ticket de cola `feed` o `none` si la conexión sigue abierta: no se guarda hasta el próximo ESC @ o la desconexión, y no hay plazo de inactividad.
>    - Todo esto debería ser un LAZ. `pendientes: ninguno` no es cierto.
>
> **Menores**
>
> 3. **Los PNG dorados son circulares** (`InterpreteTests.cs:40-57`, D3). REQ-01 compara la salida del intérprete contra su propia salida revisada a ojo, y de esa revisión no queda registro.
>    - Los dos oráculos independientes cubren poco. El de raster sólo mira `03-imagen`, en la columna 0 y en cualquier fila. El de QR compara la matriz, pero no cómo queda dibujada en `04-qr.png`.
>    - Un interlineado, márgenes, saltos de línea, alto de barras o tablas R/G del EAN-13 equivocados habrían quedado fijados como dorados sin que ninguna prueba lo note.
>    - `02-acentos` sólo usa caracteres que valen lo mismo en las páginas de códigos 437 y 850, así que no distingue la tabla de caracteres.
>    - Revisé a mano las tablas L/G/R y de paridad del EAN-13: son correctas. Igual conviene un lector de QR/EAN-13 (ZXing) sobre los PNG de 04 y 05 como oráculo.
>
> 4. **El rótulo «Supuestos sin medir que no son pendientes» mezcla protocolo con fidelidad** (`parcela.md:90-91`).
>    - El formato de la respuesta a GS I 67 sin la cabecera `_`, el largo de FS q (que si está mal desalinea toda la lectura) y los bytes de DLE EOT no son fidelidad visual de la segunda etapa.
>    - Si son falsos, cambia lo que la librería cliente ve del emulador. Deberían ser LAZ, o quedar atados a la prueba de extremo a extremo de PAR-PTE-05.
>
> 5. **El alto del ticket no tiene límite** (`Lienzo.cs:135-138`, `Interprete.cs:499-503`).
>    - `ESC 3 255` y luego `ESC d 255` repetido suman unas 65 000 filas cada 6 bytes, alrededor de 25 MB por repetición. Un par de kilobytes de entrada agotan la memoria.
>    - Además, `APng` desborda `(porFila+1)*alto` pasadas unas 43,8 millones de filas.
>    - `CargaMaxima` no cubre este caso.
>
> 6. **Almacén: dos instancias sobre la misma carpeta pueden pisarse** (`AlmacenDeTickets.cs:36-42,98`).
>    - Cada instancia lleva su propio `_secuencia`, y la prueba misma usa dos. Con el mismo milisegundo y la misma secuencia, `File.Move(overwrite: true)` reemplaza un ticket sin avisar.
>    - `Fecha()` (`:101-102`) lanza `FormatException` con un archivo que tiene la forma de un id pero una fecha imposible (por ejemplo `20261399T…`). Como la carpeta es montable, eso rompe `Listar` y `Ultimo` para todos.
>
> 7. **Licencia de la fuente** (`PrintThermal.Emulator.Core.csproj`). La fuente va embebida en la DLL, pero `OFL.txt` no viaja con lo compilado ni con la imagen. La OFL §2 exige el aviso y la licencia en cada copia, así que hay que resolverlo antes de PAR-PTE-05.
>
> 8. **Nombres posiblemente privados** (`Vectores/LEEME.md:11`, `Generador.cs.txt:1-8`, `Generador.csproj.txt:10-13`).
>    - Aparecen «MotorDsl», sus espacios de nombres, rutas internas y `../lib-940fa7f`. El resto del corpus nombra a la librería sólo por su rol (DEC-006).
>    - Si ese repositorio es privado, esto viola P5 B. No pude confirmarlo: hay que verificarlo.
>    - No encontré IPs, MAC, credenciales ni nombres de equipos. El `computerName` del `.trx` es el id de un contenedor.
>
> 9. **Trazas del corpus incompletas.**
>    - Los criterios nuevos de v2 (troceo; id inválido) no tienen REQ-NN ni línea en «Criterio ↔ prueba».
>    - PAR-PTE-04 (`parcela.md:21`) sigue leyendo `REQ-HIS-Almacen-De-Tickets@v1`, que ahora es v2.
>
> **Notas**
>
> 10. El QR sólo se probó con corrección H (versiones 1, 3 y 10). Las filas L/M/Q de la tabla no tienen oráculo, y L es el valor por defecto cuando no llega fn 69.
> 11. GS r y GS a (consultas de estado) pasan por `Configura` y abren ticket (`Interprete.cs:260,308`), contra lo que dice D5. GS I con n ≠ 67 no responde.
> 12. La prueba de REQ-02 sólo ejercita un comando GS desconocido; los caminos ESC, FS y DLE no se prueban. Los 4 mutantes no tocan la validación de ids ni el QR. La corrida en rojo es coherente: 30 casos, todos `NotImplementedException`.
> 13. `parcela.md:89` cita `evidencia/2026-11-27-auditoria.md`, que todavía no existe.
> 14. `Alimentar` compacta el búfer en cada llamada: con trozos de 1 byte sobre una carga grande pendiente el costo es O(n²).
> 15. La regex de id usa `\d` (acepta dígitos Unicode) y `$` (acepta un `\n` final). No permite salir de la carpeta porque no admite separadores, pero conviene `[0-9]` y `\z`.
>
> VEREDICTO: refutado

## 2. Qué se hizo con cada hallazgo

| # | Hallazgo | Qué se hizo |
|---|---|---|
| 1 | Evidencia vieja; desborde de `int` en GS v 0 / GS 8 L que deja muerta la conexión | Tope `CargaMaxima` (4 MiB) con aviso, cálculo en `long`, prueba `UnaCargaDesmesuradaSeDescartaSinRomperNiBloquear`; FS q con el mismo tope. Evidencia rehecha sobre el árbol final: 49 de 49 (turno 251); el mutante M7 (sin tope) la pone en rojo |
| 2 | REQ-03 sin conexión de punta a punta; ticket sin corte con la conexión abierta | `AlmacenDeTickets.Escuchar(Interprete)`; `CadaTicketGuardaBinYPng` ahora alimenta el intérprete y no llama `Guardar` (mutante M3 en rojo). El plazo de inactividad y quién llama `Finalizar` quedan en LAZ-0001, declarado en `pendientes:` |
| 3 | Dorados circulares, oráculos débiles, página de códigos sin distinguir | Raster bit a bit en 03 y 06; QR del ticket 04 muestreado módulo a módulo contra la matriz de QRCoder; EAN-13 del ticket 05 fila a fila contra sus módulos; `EscTCambiaLaTablaDeCaracteres` (PC850 contra Windows-1252 y PC437). La revisión a ojo queda descrita en `Vectores/LEEME.md` §2. Un lector ZXing no se agregó: sería un paquete nuevo en `Directory.Packages.props`, fuera de `modifica` (D4); queda NO VERIFICADO que un lector lea los PNG |
| 4 | Supuestos de protocolo rotulados como fidelidad | GS I 67, DLE EOT y largo de FS q pasan a LAZ-0002, declarado en `pendientes:` |
| 5 | Alto del ticket sin límite | `PerfilDeImpresora.AltoMaximo` (20 000 puntos) con aviso; prueba `UnTicketQueNoTerminaDeAvanzarSeCortaEnElAltoMaximo` (mutante M8 en rojo) |
| 6 | Dos instancias pueden pisarse; fecha imposible rompe el listado | Renombre sin pisar y reintento con la secuencia siguiente (`DosInstanciasEnElMismoMilisegundoNoSePisan`); `EsId` exige fecha válida (`UnArchivoConFormaDeIdPeroFechaImposibleNoRompeElListado`) |
| 7 | `OFL.txt` no viaja con lo compilado | El `.csproj` de Core lo copia a la salida y a la publicación como `Licencias/TerminusFont-OFL.txt`. Que la imagen lo incluya se verifica en PAR-PTE-05 |
| 8 | «MotorDsl» quizá privado | Verificado público: `https://github.com/hdcm-dev/ThermalPrint.MotorDsl.Core` responde 200 y `https://api.nuget.org/v3-flatcontainer/motordsl.core/index.json` lista hasta 1.0.18. No se cambia |
| 9 | Criterios v2 sin REQ; PAR-PTE-04 lee Almacen@v1 | REQ-04 (troceo) y REQ-05 (id inválido) con su prueba y su línea en «Criterio ↔ prueba». PAR-PTE-04 se nombra en `## Cierre`: su `ready --contra main` pedirá subir la huella a v2 |
| 10 | QR sólo con H | Referencias `qr-4` (L), `qr-5` (M) y `qr-6` (Q) de QRCoder; pasan |
| 11 | GS r y GS a abren ticket | Pasan a consulta: no abren ticket y avisan. GS I con n ≠ 67 sigue sin respuesta, con aviso (LAZ-0002 cubre las respuestas sin medir) |
| 12 | REQ-02 sólo con GS; mutantes sin ids ni QR | REQ-02 con ESC, GS, FS y DLE desconocidos; mutantes M5 (id sin validar) y M6 (máscara QR fija) en rojo |
| 13 | Auditoría citada que no existía | Es este archivo |
| 14 | Compactación O(n²) | El búfer pendiente lleva un inicio y compacta sólo cuando falta lugar |
| 15 | `\d` y `$` en la regex | `^[0-9]{8}T[0-9]{9}-[0-9]{4}\z`; prueba con un id terminado en `\n` |

## 3. Segunda ronda

Mismo auditor, contexto nuevo, sobre la cabeza `e4a9316` (árbol limpio). Texto tal como lo devolvió:

> **Auditoría del cierre de PAR-PTE-02, segunda ronda** (cabeza `e4a9316`, árbol limpio)
>
> Las 15 correcciones de la primera ronda están en el código. Pero hay una prueba que no puede fallar, una afirmación de `## Cierre` que es falsa y pendientes sin registrar.
>
> ### Graves
>
> 1. **`## Cierre` afirma una verificación que no existe** (`parcela.md:97`, `auditoria.md:82`).
>    - `DosInstanciasEnElMismoMilisegundoNoSePisan` (`AlmacenTests.cs:76-86`) nunca provoca una colisión. Cada instancia toma `_secuencia` de forma perezosa en el primer `Guardar` (`AlmacenDeTickets.cs:39-40`), cuando el archivo de la anterior ya está escrito, así que sale 1, 2, 3 sin chocar. Un mutante con `overwrite: true` y sin reintento pasaría. La corrección #6 se da por probada con una prueba vacía. Para chocar haría falta intercalar: A guarda, B guarda, A guarda en el mismo milisegundo.
>    - «Ocho mutantes… incluidas las agregadas tras la auditoría» es falso:
>      - `EscTCambiaLaTablaDeCaracteres`, `ElEan13DelTicketSeDibujaConSusModulos`, `DosInstancias…` y `UnArchivoConFormaDeIdPeroFechaImposible…` no tienen ni rojo ni mutante.
>      - M2 sólo pone en rojo `1D 99` (`mutacion.txt:12-13`). Los casos ESC, FS y DLE de la prueba de REQ-02 nunca estuvieron en rojo, y el rojo (`rojo.txt`) es de cuando esa prueba era un `Fact`.
>
> ### Menores
>
> 2. **Pendiente oculto: licencia OFL en la imagen** (`auditoria.md:83`). «Que la imagen lo incluya se verifica en PAR-PTE-05» sólo está en esa tabla. PAR-PTE-05 no lo menciona, no hay LAZ, y ninguna prueba ni evidencia muestra que `Licencias/TerminusFont-OFL.txt` llegue a la salida de `publish`. Es la misma figura que el hallazgo 2 de la primera ronda.
> 3. **Regla de la especificación descartada sin rastro** (Especificacion §4.3, línea 127). Dice «responder GS I después de imprimir lo encolado; demora configurable». El emulador responde al instante (`Interprete.cs:312`) y `PerfilDeImpresora` no tiene esa demora. No figura en D6, ni en LAZ-0002, ni en PAR-PTE-03.
> 4. **LAZ-0002 no es coherente con su propio criterio.** Deja afuera la falta de respuesta a GS ( L fn 48 y fn 64 (D6), a GS a/GS r y a GS I con n ≠ 67. Si la 58HB6 sí responde, la librería toma otro camino (NV, §2 y §4.2). Es exactamente el criterio que llevó GS I 67 a LAZ-0002.
> 5. **La memoria sigue sin tope en un ticket sin fin** (`Interprete.cs:136-140, 476-480`).
>    - El arreglo #5 sólo acotó el lienzo; `_bytes` y `_avisos` siguen sin límite.
>    - Cada byte de control suelto genera un aviso de unos 100 bytes que queda en `_avisos` y se emite por `Aviso`: amplifica la entrada unas 100 veces.
> 6. **El aviso del alto máximo no dice la verdad** (`Interprete.cs:514-521`, `Lienzo.cs:32`). Con `_y` clavado en `AltoMaximo − Margen`, cada línea o imagen posterior se dibuja encima del margen inferior (filas 19 992 a 19 999). La prueba no lo mira.
> 7. **Si el almacén falla, se pierde el ticket** (`AlmacenDeTickets.cs:65`, `Interprete.cs:452-458`). Una `IOException` en `Guardar` (por ejemplo, carpeta montada sin permisos) sale por `Alimentar` hacia el transporte. El ticket se pierde (`_bytes` ya es null) y el corte se reprocesa como «corte suelto». No hay prueba, aviso ni LAZ.
> 8. **La prueba de la carga desmesurada tiene un oráculo débil** (`InterpreteTests.cs:286-295`). Sólo consume `GS v`; el resto de la cabecera (`30 …`) se dibuja como el texto «0». Por eso `PuntosNegros > 0` se cumple aunque «Texto» se pierda. Debería compararse contra una referencia, como hace REQ-02.
>
> ### Notas
>
> 9. **REQ-01:**
>    - El texto de 01, 02 y 07 (interlineado, negrita, márgenes) sólo tiene la revisión a ojo. Ninguna prueba distingue `Fuente.Negrita` de `Normal`.
>    - Los oráculos de raster, QR, EAN-13 y ESC t son reales.
>    - EAN-13 empieza en x=0 y el QR toca el texto, sin zonas de silencio; está declarado NO VERIFICADO.
> 10. **REQ-04** (`InterpreteTests.cs:64-73`) compara sólo el PNG, no `Bytes`, avisos ni respuestas. Los vectores no ejercitan la medición incremental de FS q, GS ( L, GS 8 L, ESC D ni DLE DC4.
> 11. **Comandos estándar que no están en la tabla de largos** (`Interprete.cs:159, 274`): ESC ?, ESC r, ESC T, ESC W, ESC &, GS b, GS \, GS $. Se consumen como 2 bytes y su parámetro se imprime como texto. La librería no los emite.
> 12. **Inconsistencias chicas:**
>     - Barras más anchas que 384 se recortan sin aviso (`Interprete.cs:697`), a diferencia de raster y QR.
>     - Un ticket vacío con corte se guarda en blanco; con `Finalizar` se descarta.
> 13. **Traza del audit:** `auditoria.md` audita `8d7ce53`, que no es ancestro de HEAD ni está en ninguna rama. Su árbol es igual al de `81b212c`; conviene citar ese.
> 14. **Licencias:**
>     - `CodigoQr.cs:6-16` dice «transcrito de QRCoder 1.8.0 (MIT)» sin el aviso MIT; es una tabla del estándar, riesgo bajo.
>     - La OFL de Terminus Font es correcta y la fuente no se modifica.
> 15. **Datos sensibles:** no hay IPs, MAC, credenciales ni nombres de equipos. `computerName` es el id de un contenedor. Que MotorDsl sea público sale de la primera ronda (afirma que la URL respondió 200 y que NuGet lista hasta 1.0.18); no lo volví a comprobar.
>
> ### Correcciones de la primera ronda que verifiqué como reales
>
> - #1: tope de carga y cálculo en `long`.
> - #2: `Escuchar`, la prueba de punta a punta y LAZ-0001.
> - #3: oráculos de raster, QR y EAN-13.
> - #9, #10, #11 (GS r/GS a como consultas), #14 (búfer con inicio) y #15 (regex).
>
> La evidencia de 49 de 49 en verde coincide con las pruebas del árbol.
>
> VEREDICTO: refutado

## 4. Qué se hizo en la segunda ronda

| # | Hallazgo | Qué se hizo |
|---|---|---|
| 1 | La prueba de dos instancias no choca; mutantes que no cubren las pruebas nuevas | La prueba intercala las instancias: la segunda escritura de la primera choca con la de la otra y salta a la secuencia 3 (ids exactos). Mutación rehecha con 18 mutantes, uno por prueba de guarda, incluidos ESC, FS y DLE desconocidos (M03), dos instancias (M10), ESC t (M11), EAN-13 dibujado (M12) y fecha imposible (M13): todos en rojo |
| 2 | La licencia en la imagen, sin registro | `LaLicenciaDeLaFuenteViajaConLoCompilado` prueba que llega a la salida de la compilación (mutante M18). La publicación de la imagen es trabajo de PAR-PTE-05 y se nombra en `## Cierre` |
| 3 | Demora de GS I descartada sin rastro | En LAZ-0002: Core responde al interpretar; la demora, si hace falta, es del transporte |
| 4 | LAZ-0002 incompleto | Suma GS ( L fn 48 y 64, GS a, GS r y GS I con n ≠ 67 |
| 5 | Memoria sin tope en `_bytes` y `_avisos` | Tope de 200 avisos por ticket con un aviso de resumen (`LosAvisosDeUnTicketTienenTope`, M15) y de 32 MiB por ticket, que lo cierra con aviso |
| 6 | El alto máximo dibujaba sobre el margen | El lienzo no dibuja desde `AltoMaximo − Margen`; la prueba exige el PNG en blanco después del tope (M16) |
| 7 | Un error del almacén salía por `Alimentar` | `Escuchar` captura los errores de disco y los informa por `ErrorAlGuardar`; el intérprete sigue (`UnErrorDeDiscoNoTumbaAlInterprete`, M14) |
| 8 | Oráculo débil de la carga desmesurada | Compara contra la referencia sin «1D 76» |
| 9 | Negrita sin prueba | `LaNegritaUsaOtraFuente` (M17). El interlineado y los márgenes quedan con la revisión a ojo (supuestos sin medir) |
| 10 | REQ-04 sólo compara el PNG | Compara también bytes y avisos, y suma un flujo con ESC D, DLE DC4, GS ( L, GS 8 L y FS q cortado en trozos |
| 11 | Comandos estándar sin largo | ESC ?, ESC r, ESC T, ESC W, GS b, GS \\ y GS $ con su largo; ESC & sigue sin soporte (la librería no lo emite) |
| 12 | Barras recortadas sin aviso; ticket vacío según cómo termina | Aviso al recortar barras. El ticket vacío con corte se guarda y sin corte se descarta: es D5 y queda escrito en `## Cierre` |
| 13 | Auditoría sobre un commit que no quedó en la rama | Se cita el commit de la rama por su mensaje: cada rebase cambia el SHA (ver §6, fila 7) |
| 14 | Aviso MIT de QRCoder | Copiado junto a la tabla en `CodigoQr.cs` desde `https://github.com/Shane32/QRCoder/blob/v1.8.0/LICENSE.txt` |
| 15 | MotorDsl público sin volver a comprobar | Sin cambio: la comprobación está en §2, fila 8 |

## 5. Tercera ronda

Mismo auditor, contexto nuevo, sobre la cabeza `b9ec3f5` (árbol limpio). Texto tal como lo devolvió:

> # Auditoría de PAR-PTE-02, tercera ronda (cabeza `b9ec3f5`, árbol limpio)
>
> Revisé las correcciones de la segunda ronda una por una. Casi todas están hechas de verdad; la excepción es la #13 (hallazgo 7). Igual encontré un contraejemplo de REQ-04 y una afirmación de `## Cierre` que no es cierta.
>
> Una aclaración sobre el encargo: la parcela no dice `pendientes: ninguno`. Declara LAZ-0001 y LAZ-0002, los dos existen y están abiertos.
>
> ## Graves
>
> **1. REQ-04 es falso: hay entradas donde el troceo cambia el PNG** (`Interprete.cs:408-413`, usado en `:166` para ESC D y en `:268` para GS k con m ≤ 6).
> - `HastaNul` busca el NUL en todo el búfer pendiente, no sólo en los primeros `tope` bytes.
> - Con los bytes enteros, un ESC D o un GS k sin NUL dentro de su tope se traga todo hasta el próximo `00`. Ese `00` aparece en casi cualquier cabecera `1D 76 30 00`.
> - Troceado, el mismo comando consume exactamente `tope` bytes.
> - Contraejemplo: `1B 40 1B 44` + 40×`41` + `0A` + `1D 76 30 00 01 00 01 00 FF` + `1D 56 01`.
>   - Con trozo 1 se dibujan 6 «A» y la imagen.
>   - Con los bytes enteros no se dibuja nada de eso.
> - Con los trozos de 256 de la librería, el resultado depende de dónde cae el corte.
> - `ElTroceoNoCambiaElResultado` (`InterpreteTests.cs:64-86`) no lo detecta porque sólo usa comandos bien formados. El tope existe justamente para la entrada mal formada, y ninguna prueba lo usa.
> - Corrección: buscar el NUL en `b[desde..Math.Min(b.Length, desde + tope)]`.
>
> **2. `## Cierre` afirma una cobertura de mutación que no existe** (`parcela.md:97`, «18 mutantes, uno por cada REQ y por cada guarda agregada en las auditorías»).
> - **Tope de 32 MiB por ticket** (`Interprete.cs:29,476-480`, auditoría §4 fila 5): sin prueba y sin mutante.
>   - Además tiene un defecto. Cuando el tope salta dentro de `Imprime`, el resto del comando sigue ejecutándose sobre un ticket ya cerrado.
>   - Una imagen o unas barras se dibujan en el lienzo que ya se exportó, y se pierden.
>   - Un carácter queda en `_linea` y pasa al ticket siguiente, aunque su byte quedó en el `.bin` anterior.
> - **Arreglo del grave #1 de la primera ronda para GS 8 L** (`:264`, el `(uint)`): ninguna prueba lo protege.
>   - Un mutante que quite el cast vuelve a lanzar `ArgumentOutOfRangeException` con p4 ≥ 0x80. Como el comando nunca se consume, la conexión queda muerta, y las 53 pruebas pasan igual.
>   - Lo mismo pasa con el tope de FS q (`:350-353`).
> - **Otras guardas de las auditorías sin prueba ni mutante:**
>   - GS a y GS r como consultas, que no abren ticket (primera ronda #11).
>   - Los largos nuevos de ESC ?, ESC r, ESC T, ESC W, GS b, GS \ y GS $ (segunda ronda #11).
>   - El aviso al recortar barras (segunda ronda #12).
> - `LaImagenRasterSeDibujaBitABit("06-escpos-feed")` se agregó después de la corrida roja y no tiene mutante, así que nunca estuvo en rojo. Eso contradice «su rojo es el de su mutante».
>
> ## Menores
>
> **3. Pendiente escondido en la prosa: la licencia OFL en la imagen** (`parcela.md:104`, «Para PAR-PTE-05…»).
> - `cdd/parcelas/PAR-PTE-05-Distribucion` no menciona OFL ni Terminus, y no hay LAZ.
> - La segunda ronda #2 pidió registrarlo. La respuesta sólo agregó la prueba sobre la salida de la compilación. Es una obligación de licencia (OFL §2) que hoy no verifica nadie.
>
> **4. Una excepción de un suscriptor deja el comando trabado** (`Interprete.cs:91-94`).
> - Si un suscriptor de `Respuesta`, `TicketTerminado` o `Aviso` lanza, `_inicio` no avanza y el comando se vuelve a ejecutar en el próximo `Alimentar`.
> - Caso típico en PAR-PTE-03: falla el envío por el socket dentro de `Respuesta`. El DLE EOT se vuelve a contestar en cada trozo siguiente.
> - Si el que falla es el `Guardar` del ticket, el corte se reprocesa como «corte suelto» y el ticket se pierde.
> - `Escuchar` sólo filtra `IOException` y `UnauthorizedAccessException`. No hay prueba, y LAZ-0001 y LAZ-0002 no lo dicen.
>
> **5. El almacén deja basura ante errores de disco** (`AlmacenDeTickets.cs:155-165,256`).
> - Si `WriteAllBytes` del temporal falla, el `.tmp` queda.
> - Si el `.png` falla por algo que no es un choque de id, el `.bin` queda huérfano.
> - `Ids()` sólo enumera `*.png`, así que ni `Listar` ni `BorrarTodo` ven ni limpian esos archivos.
>
> **6. Hay estilos de texto que no prueba ningún oráculo ni ningún dorado.**
> - ESC ! 0x10/0x20/0x80, GS ! y ESC - no aparecen en ningún vector.
> - Un mutante que ignore el doble alto, el doble ancho o el subrayado pasa todas las pruebas.
> - Si la librería los emite en la app de campo, REQ-01 no lo cubre.
> - Fuera de eso, los oráculos independientes (raster, QR, EAN-13, ESC t, negrita) son reales. El texto de 01, 02 y 07 sigue dependiendo sólo de la revisión a ojo, como ya está declarado.
>
> ## Notas
>
> **7. La corrección #13 de la segunda ronda no es real** (`auditoria.md:14,167`).
> - `81b212c` tampoco es ancestro de HEAD, y `git branch -a --contains` no lo encuentra en ninguna rama.
> - Es la misma situación que se quería corregir.
>
> **8. Diferencias de redacción entre parcela, corpus y LAZ.**
> - El corpus dice «trozos de 1, 7 o 256» (`REQ-HIS-Ticket-A-Png` §2). La parcela dice «cualquier tamaño» (REQ-04).
> - LAZ-0002 le asigna la demora de GS I al transporte, pero su tabla de propagación no nombra a PAR-PTE-03.
>
> **9. Un ticket que sólo avanza papel cuenta como ticket con contenido** (`Interprete.cs:532`).
> - Basta un `ESC @ ESC d 4` seguido de `Finalizar` para que se guarde un ticket en blanco.
> - D5 no lo distingue.
>
> **10. Licencia de la fuente.**
> - El texto de la OFL es correcto.
> - Los PSF «Uni2» de Debian son un subconjunto convertido desde BDF, o sea una «Modified Version» según la OFL. Esa licencia restringe usar el nombre reservado «Terminus Font» en versiones modificadas.
> - Riesgo bajo: Debian los distribuye así. Conviene dejarlo anotado en D2.
>
> **11. Datos sensibles.**
> - No hay IPs, MAC, credenciales ni nombres de equipos en el diff.
> - El `computerName` del `.trx` es el id de un contenedor (`ed003c9b7dcd`), y `/w/` es una ruta del contenedor.
> - No pude volver a comprobar que `hdcm-dev/ThermalPrint.MotorDsl.Core` sea público: me denegaron la consulta por red. Sigue valiendo lo que dice la primera ronda.
>
> ## Correcciones de la segunda ronda que verifiqué como reales
>
> - **#1:** la prueba de dos instancias ahora choca de verdad, y el mutante M10 la detecta.
> - **#5 (avisos):** el tope de 200 tiene su prueba.
> - **#6:** el lienzo ya no dibuja desde el margen inferior.
> - **#7:** `Escuchar` captura los errores de disco.
> - **#8:** la carga desmesurada se compara contra una referencia.
> - **#10:** el troceo compara bytes y avisos, y suma el flujo de cargas.
> - **#11:** los comandos nuevos tienen su largo.
> - **#12:** hay aviso al recortar barras.
> - **#14:** el aviso MIT de QRCoder está copiado.
> - **Evidencia:** el verde de 53 de 53 coincide con las pruebas del árbol. Las 18 líneas de mutación coinciden con las pruebas que nombran.
>
> VEREDICTO: refutado

## 6. Qué se hizo en la tercera ronda

| # | Hallazgo | Qué se hizo |
|---|---|---|
| 1 | `HastaNul` buscaba el NUL más allá del tope: el troceo cambiaba el resultado | Busca sólo dentro del tope. El contraejemplo del auditor entra en `ElTroceoNoCambiaElResultado` (mutante M19) |
| 2 | Cobertura de mutación declarada que no existía; tope de 32 MiB cerraba a mitad de un comando | Tope por ticket en `PerfilDeImpresora.BytesMaximosPorTicket`, medido entre comandos (`UnTicketQueSuperaSuTopeDeBytesSeCierraEntreComandos`, M20). Pruebas y mutantes para GS 8 L sin signo (M21), tope de FS q (M22), GS a y GS r como consultas (M23), largos nuevos (M24), aviso de barras (M25) y raster en 03 y 06 (M26). 29 mutantes, los 29 en rojo |
| 3 | Licencia OFL en la imagen sin registro | LAZ-0003, declarado en `pendientes:` |
| 4 | Un suscriptor que lanza repetía el comando | Cada suscriptor se llama aparte; su excepción queda como aviso y el comando no se repite (`UnSuscriptorQueFallaNoRepiteElComando`, M27) |
| 5 | Basura en disco ante errores | El temporal se borra siempre; si falla el `.png`, se borra el `.bin`. Sin prueba propia: no hay forma portátil de hacer fallar sólo la segunda escritura en el contenedor (corre como root) |
| 6 | Doble tamaño y subrayado sin prueba | `ElDobleTamanoYElSubrayadoSeDibujan` (M28 y M29). La librería no los emite en sus vectores |
| 7 | El SHA citado no está en la rama | Cada rebase sobre `main` reescribe los SHA de la rama; se cita el commit por su mensaje |
| 8 | Redacción distinta entre corpus, parcela y LAZ | El criterio de la historia dice «cualquier tamaño» y nombra lo probado; LAZ-0002 suma la fila de PAR-PTE-03 |
| 9 | Un ticket que sólo avanza papel se guarda | Es D5 (avanzar papel es imprimir); queda escrito en `## Cierre` |
| 10 | PSF convertidos desde BDF | Anotado en D2 |
| 11 | MotorDsl público | Sin cambio (§2, fila 8) |

No se corrió una cuarta ronda: el cierre queda con tres rondas «refutado» y todo lo señalado atendido o registrado. Garantía reducida: mismo modelo en otro contexto.
