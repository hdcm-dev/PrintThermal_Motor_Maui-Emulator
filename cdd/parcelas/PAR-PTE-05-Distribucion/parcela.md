---
id: PAR-PTE-05
titulo: Contenedor, release en ghcr.io y prueba de extremo a extremo en el emulador de Android API 37
version: 1.1
estado: cerrada
velocidad: completa
base_commit: 0b8c5b04dcf6a6ff9c4987b97edbd6f708ae34d4
sprint: SPR-01
epica: REQ-EPI-Distribucion@v1
historias:
  - REQ-HIS-Imagen-Publicada@v1
lee:
  - VIS@v1
  - REQ-EPI-Distribucion@v1
  - ADR-0001@v1
  - ADR-0006@v1
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v2
  - REQ-HIS-Recepcion-Integra@v2
  - REQ-HIS-Almacen-De-Tickets@v2
  - REQ-HIS-Ticket-A-Png@v2
modifica:
  - REQ-HIS-Imagen-Publicada@v1
  - REQ-HIS-Api-De-Tickets@v2
appetite: dos días
pendientes:
  - {que: "conectar el almacén en disco y el medio Bluetooth al host (avance en esta parcela, D2)", registro: LAZ-0006}
  - {que: "escucha de la imagen dentro del contenedor (avance en esta parcela, D3)", registro: LAZ-0008}
  - {que: "recepción RFCOMM conectada al almacén (avance en esta parcela, D2)", registro: LAZ-0010}
  - {que: "fin de un ticket sin corte con la conexión abierta: el cierre de la conexión lo termina; el plazo sigue sin decidir", registro: LAZ-0001}
decisiones:
  - {id: D1, tema: "imagen base mcr.microsoft.com/dotnet/aspnet:10.0.14-noble-chiseled por digest (runtime del SDK 10.0.403 de global.json) y etiqueta semántica v1.0.0 del release", clase: K1}
  - {id: D2, tema: "conexión del medio Bluetooth (PAR-PTE-03) y del almacén en disco (PAR-PTE-02) al host, detrás de IAlmacenDeTickets e IEstadoDelMedio, en src/PrintThermal.Emulator.Api/Integracion/ (LAZ-0006, LAZ-0010; el fin de conexión cierra el ticket, LAZ-0001)", clase: K1}
  - {id: D3, tema: "la imagen fija la escucha en http://0.0.0.0:5080 de su red de contenedor y vacía ASPNETCORE_HTTP_PORTS (LAZ-0008); no publica puertos", clase: K1}
  - {id: D4, tema: "modo de uso previsto aprobado por INFRA: en el equipo del emulador, --network container:<emulador Android>, dentro del turno y emu-slot del emulador, --cpus 1 --memory 512m --rm, y se baja con el emulador; netsim no se expone", clase: K0}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Una imagen de versión fija en `ghcr.io`, probada de punta a punta con una app de ejemplo de la librería cliente en el emulador de Android API 37.

## Escenarios

- Camino feliz: un release publica la imagen; con ella, la app de ejemplo imprime y la API entrega el PNG.
- No feliz: la prueba de extremo a extremo falla; el release no se publica.

## Requisitos EARS

- REQ-01 · Cuando se publica un release, el sistema deberá construir y publicar la imagen en `ghcr.io` con la versión del release.
  prueba: tests/PrintThermal.Emulator.Tests/Distribucion/ReleaseTests.cs · ElWorkflowDeReleasePublicaEnGhcr
- REQ-02 · Cuando la app de ejemplo imprime en el emulador de Android API 37, el sistema deberá entregar el PNG del ticket por la API.
  prueba: tests/PrintThermal.Emulator.Tests/Distribucion/ExtremoAExtremoTests.cs · LaAppDeEjemploImprimeYLaApiEntregaElPng
- REQ-03 · Cuando termina un ticket recibido por RFCOMM, el host deberá guardarlo en la carpeta de salida y entregarlo por la API.
  prueba: tests/PrintThermal.Emulator.Tests/Api/IntegracionTests.cs · ElTicketRecibidoPorRfcommLlegaALaApi
- REQ-04 · Cuando la imagen arranca sin otra configuración, el host deberá escuchar en el puerto 5080 de su red de contenedor y guardar en `/data`.
  prueba: tests/PrintThermal.Emulator.Tests/Distribucion/ReleaseTests.cs · LaImagenFijaEscuchaYSalida

## Casos borde

- Etiqueta de release repetida; imagen sin la carpeta de salida montada.

## Fuera de alcance

- Conocimiento de montaje en el repositorio de escenas (REQ-HIS-Conocimiento-De-Montaje, sprint siguiente).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Distribucion/ReleaseTests.cs · ElWorkflowDeReleasePublicaEnGhcr
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Distribucion/ExtremoAExtremoTests.cs · LaAppDeEjemploImprimeYLaApiEntregaElPng
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Api/IntegracionTests.cs · ElTicketRecibidoPorRfcommLlegaALaApi
- Criterio REQ-04 → tests/PrintThermal.Emulator.Tests/Distribucion/ReleaseTests.cs · LaImagenFijaEscuchaYSalida

## Plan

Ver `plan.md`. El emulador de Android corre con su turno, en su contenedor propio, y el del emulador de impresora comparte su red dentro del mismo turno (D4).

## Tareas

Ver `tasks.md`.

## Cierre

- Versiones subidas: REQ-HIS-Imagen-Publicada v1 → v2 (historia y `modifica`; `rutas:` suma `tests/…/Distribucion/**`) y REQ-HIS-Api-De-Tickets v2 → v3 (`modifica`; `rutas:` suma `src/PrintThermal.Emulator.Api/Integracion/**`).
- Changelog: «PAR-PTE-05 · Imagen del emulador…» en `cdd/corpus/CHANGELOG.md`.
- Evidencia (corridas con SDK 10.0.403 en contenedor con turno en el equipo de cómputo):
  - `evidencia/2026-11-27-rojo.txt`: 9 de 123 en rojo sobre el esqueleto (las nuevas, por la causa esperada). Las cinco pruebas que sumó la auditoría se escribieron con el código ya hecho: no tienen rojo propio y su poder lo mide la mutación.
  - `evidencia/2026-11-27-pruebas.txt` y `.trx`: 131 de 131 en verde.
  - `evidencia/2026-11-27-mutacion.txt`: 10 mutantes, 9 en rojo y al menos uno por REQ; sobrevive el de la guarda de una carrera que las pruebas de un hilo no reproducen.
  - `evidencia/2026-11-27-extremo-a-extremo.md` y `capturas/`: emulador de Android API 37 (1 CPU / 5,5 GB) y la imagen construida con este `Dockerfile` desde `5066fa3` en su red (1 CPU / 512 MB), en un solo turno. `MotorDsl.SampleApp` de la librería cliente imprimió cinco veces por RFCOMM; la API entregó cada PNG con la misma huella SHA-256 que el guardado en la carpeta montada, 5 de 5, y los 341 bytes que informó la app. El visor se capturó con Chrome del emulador de Android.
- Decisiones: D1 a D3 son K1; D4 es K0 (modo de uso aprobado por INFRA, documentado en `README.md` y en el `Dockerfile`).
- Divergencia corpus/código: ninguna que contradiga un ADR. ADR-0006 pide imagen en `ghcr.io` y runners de GitHub: el release corre en `ubuntu-latest`.
- Auditoría (`evidencia/2026-11-27-auditoria.md`): las rondas 1 y 2 refutaron el cierre; cada hallazgo tiene respuesta en código, pruebas, texto o NO VERIFICADO. No hubo tercera ronda.
- NO VERIFICADO:
  - el release real y la imagen publicada: corren al empujar la etiqueta, después de fusionar; su resultado va a la bitácora del repositorio de documentación;
  - el extremo a extremo con la imagen publicada y con el receptor final: se usó la imagen construida en local desde `5066fa3`; el receptor cambió después por la auditoría y lo cubren las pruebas;
  - vincular con `createBond` y después conectar dejó a Android sin cifrado dos veces (LAZ-0011 reproducido); vincular conectando funcionó;
  - las respuestas a consultas (DLE EOT, GS I) no vuelven a la app: la frontera `IReceptorDeBytes` no tiene canal de vuelta;
  - el dueño de `/data` con un volumen con nombre;
  - la guarda de `ReceptorAlAlmacen` para bytes que llegan mientras se cierra la conexión (mutante 8);
  - la fidelidad contra la 58HB6 física (heredado de PAR-PTE-02 a PAR-PTE-04).
- Hallazgo fuera de alcance: a 540 px de pantalla el ticket desborda su tarjeta en el visor (REQ-HIS-Visor-Web).
