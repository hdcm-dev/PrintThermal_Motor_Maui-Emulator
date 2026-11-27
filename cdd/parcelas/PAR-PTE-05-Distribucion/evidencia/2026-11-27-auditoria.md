# Auditoría del cierre de PAR-PTE-05 (2026-11-27)

auditor: subagente Claude Opus (mismo modelo que el ejecutor) en otro contexto que no vio la ejecución, sólo lectura (git diff/log, grep, lectura de archivos). Garantía reducida: mismo modelo.

## Índice

1. [Ronda 1](#1-ronda-1)
2. [Respuesta](#2-respuesta)
3. [Ronda 2](#3-ronda-2)
4. [Respuesta a la ronda 2](#4-respuesta-a-la-ronda-2)

## 1. Ronda 1

Auditor refutador en otro contexto (mismo modelo, sólo lectura, sin la conclusión del autor). Veredicto: **refutado**. Hallazgos, en su orden:

1. La evidencia no estaba versionada en la rama.
2. Faltaban la corrida verde, la mutación y la corrida del release; la prueba de extremo a extremo usó una imagen construida en local, no la publicada.
3. `LaAppDeEjemploImprimeYLaApiEntregaElPng` afirmaba más de lo que prueba: repite bytes guardados por la API, el trozo de 990 bytes no troceaba nada, la huella sale del mismo intérprete y los cinco vectores son dos distintos.
4. Un receptor que acumule los bytes y sólo interprete al cerrar pasaba todas las pruebas; faltaban varios tickets en una conexión abierta y conexiones intercaladas.
5. Carrera en `ReceptorAlAlmacen.Recibidos` (indexador tras `Abierta`; bytes a un intérprete ya finalizado).
6. `Guardar` sólo atrapaba errores de disco: otra excepción cortaba el medio Bluetooth.
7. Las pruebas del workflow las pasaba un paso de prueba con `|| true`, `continue-on-error` o `if: false`.
8. `EsperarAsync` relee la carpeta cada segundo; con el reloj hacia atrás, un ticket nuevo queda bajo el umbral (del almacén de PAR-PTE-02).
9. `release.yml` sin defectos que lo hagan fallar; detalles: sin `concurrency`, no verifica que la etiqueta esté en main, la imagen nace privada.
10. `Dockerfile` correcto; sin probar el dueño de `/data` con un volumen con nombre.
11. Superficies cambiadas fuera de las que declaraba `plan.md`.
12. Sin datos no publicables; una captura mostraba otra dirección que la elegida.

## 2. Respuesta

| # | Respuesta |
|---|---|
| 1 | Versionada en este PR (`evidencia/`, `capturas/`). |
| 2 | Verde y mutación en `2026-11-27-pruebas.txt` y `2026-11-27-mutacion.txt`. El release corre al empujar la etiqueta, después de fusionar: su resultado y la imagen publicada quedan en la bitácora del repositorio de documentación. La imagen del extremo a extremo salió del mismo `Dockerfile` y del mismo código, construida en local: que la publicada se comporte igual queda NO VERIFICADO hasta repetir la prueba con ella. |
| 3 | Texto corregido: es un candado de regresión de la corrida real, no una prueba de fidelidad. Trozo de 37 bytes; `huellas.txt` aclara que hay dos tickets distintos. La prueba de conteo también exige que cada archivo exista. |
| 4 | Nuevas: `CadaTicketConCorteLlegaSinCerrarLaConexion` (cada ticket visible antes del cierre y la lista del más nuevo al más viejo), `DosConexionesIntercaladasNoSeMezclan` y `BytesDespuesDelCierreNoReabrenElTicket`. |
| 5 | `Recibidos` usa `GetOrAdd` y descarta bytes de una conexión ya cerrada bajo el mismo cerrojo. |
| 6 | `Guardar` registra cualquier excepción y no la propaga. |
| 7 | `ElWorkflowDeReleasePublicaEnGhcr` exige que no haya `continue-on-error`, `|| true` ni pasos con `if:`, y la línea exacta de la prueba. |
| 8 | Se mantiene: la relectura cubre otra instancia sobre la misma carpeta; el orden por reloj es del almacén de PAR-PTE-02 (NO VERIFICADO con el reloj hacia atrás). |
| 9 | Sin cambios en el workflow. La visibilidad de la imagen la decide la persona. |
| 10 | NO VERIFICADO con volumen con nombre; con carpeta montada, el dueño lo pone el equipo (documentado en README y Dockerfile). |
| 11 | `plan.md` §2 suma esas superficies. |
| 12 | Captura retirada. |

## 3. Ronda 2

El mismo auditor revisó la cabeza `9457d91`. Veredicto: **refutado**.

1. El extremo a extremo no corrió sobre el código entregado: la imagen es anterior a los cambios del receptor y la evidencia no declaraba su commit.
2. `BytesDespuesDelCierreNoReabrenElTicket` no mandaba bytes después del cierre. El mutante 8 moría por otra causa.
3. Un `Recibidos` posterior al cierre creaba una conexión nueva que nunca se terminaba. Además, los ids de conexión vuelven a empezar con cada periférico. Y el texto «(ya cerrada)» era código muerto.
4. Las pruebas que sumó la auditoría no tienen rojo propio.
5. Menores: la evidencia nombraba una captura retirada, y la salida de las compuertas no está en el repositorio.

Lo que dio por resuelto: la evidencia versionada, el verde y la mutación sobre la cabeza, el texto del candado de regresión, las conexiones intercaladas, las aserciones del workflow, la captura de errores al guardar y los datos publicables.

## 4. Respuesta a la ronda 2

| # | Respuesta |
|---|---|
| 1 | La evidencia declara que la imagen salió de `5066fa3` y que el receptor final lo cubren las pruebas. En la parcela queda NO VERIFICADO. La prueba con la imagen publicada se hace después del release y queda en la bitácora del repositorio de documentación. |
| 2 | La prueba manda bytes después del cierre y vuelve a cerrar. El mutante 9 (crear la conexión al recibir) la pone en rojo. |
| 3 | `Recibidos` ya no crea conexiones: sin `Abierta` previa, descarta. `Abierta` con un id que ya existe termina primero el ticket de la conexión anterior (prueba `UnaConexionNuevaConElMismoIdTerminaLaAnterior`, mutante 10). El texto muerto se quitó. La guarda de la carrera (mutante 8) sobrevive y queda NO VERIFICADO. |
| 4 | Declarado en el cierre: el poder de esas pruebas lo mide la mutación, 9 de 10. |
| 5 | Se corrigió la referencia a la captura. La salida de las compuertas va a la bitácora del repositorio de documentación, como en las parcelas anteriores. |

No se corrió una tercera ronda.

