# Auditoría del cierre de PAR-PTE-02

auditor: agente auditor Claude Opus 5.5 (Claude Code sin interfaz, `claude -p`), contexto nuevo que no vio la ejecución, sólo lectura · garantía reducida: es el mismo modelo que ejecutó la parcela; no había otra familia de modelos disponible.

## Índice

1. [Informe del auditor](#1-informe-del-auditor)
2. [Qué se hizo con cada hallazgo](#2-qué-se-hizo-con-cada-hallazgo)

## 1. Informe del auditor

Texto tal como lo devolvió, sobre la cabeza `8d7ce53` más cambios sin commitear (el tope de carga, que se estaba escribiendo durante la auditoría):

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

Tras las correcciones no se volvió a correr el auditor.
