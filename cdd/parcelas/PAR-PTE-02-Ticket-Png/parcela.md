---
id: PAR-PTE-02
titulo: Core: intérprete ESC/POS de 58 mm a PNG y almacén de tickets
version: 1.0
estado: cerrada
velocidad: completa
base_commit: 9dec7ce0bdb56b1ee990cc97cba6015cf79c0afb
sprint: SPR-01
epica: REQ-EPI-Ticket-Png@v1
historias:
  - REQ-HIS-Ticket-A-Png@v1
  - REQ-HIS-Almacen-De-Tickets@v1
lee:
  - REQ-HIS-Compilacion-Verificada@v2
  - VIS@v1
  - REQ-EPI-Ticket-Png@v1
  - ADR-0005@v1
modifica:
  - REQ-HIS-Ticket-A-Png@v1
  - REQ-HIS-Almacen-De-Tickets@v1
appetite: un día
pendientes:
  - {que: "fin de un ticket sin corte (cola feed o none) con la conexión abierta: plazo de inactividad y quién llama Finalizar y conecta el almacén", registro: LAZ-0001}
  - {que: "respuesta a GS I 67, bytes de DLE EOT y largo de FS q sin medir en la 58HB6", registro: LAZ-0002}
decisiones:
  - {id: D1, tema: "vectores dorados generados ejecutando la librería cliente en contenedor con turno, o derivados de sus pruebas: se ejecutaron sus renderers (origin/main 940fa7f) con el generador de Vectores/Generador.cs.txt", clase: K1}
  - {id: D2, tema: "fuente A de 12 x 24: Terminus Font 24x12 normal y negrita (SIL OFL 1.1), los PSF de console-setup de Debian 12 sin cambios, con su aviso y licencia en EscPos/Fuentes/OFL.txt", clase: K1}
  - {id: D3, tema: "PNG dorados: la primera salida en verde revisada a ojo, más dos oráculos que no dependen del dorado (bits de GS v 0 en el PNG; matriz QR igual a la de QRCoder 1.8.0)", clase: K1}
  - {id: D4, tema: "sin paquetes nuevos: codificadores QR, EAN-13 y PNG propios, para no tocar Directory.Packages.props (REQ-HIS-Compilacion-Verificada)", clase: K1}
  - {id: D5, tema: "límites del ticket: abre con ESC @ o con el primer comando que imprime; cierra con GS V, con ESC @ después de contenido o con Finalizar; las consultas fuera de un ticket no son de ninguno", clase: K1}
  - {id: D6, tema: "respuestas por defecto: DLE EOT según Especificacion-58HB6 §4.3; GS I 67 = «58HB6» + NUL sin cabecera; GS ( L fn 48 sin respuesta; todas configurables en PerfilDeImpresora", clase: K1}
  - {id: D7, tema: "las rutas de REQ-HIS-Ticket-A-Png y REQ-HIS-Almacen-De-Tickets suman sus pruebas (tests/.../EscPos/** y tests/.../Almacen/**), como hizo PAR-PTE-01 con su historia", clase: K1}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Convertir los bytes de un ticket en un PNG de 384 puntos de ancho y guardarlo con su `.bin` (CE-4).

## Escenarios

- Camino feliz: un ticket de la librería cliente produce un PNG igual a su vector dorado.
- No feliz: un comando desconocido se registra y el resto del ticket se interpreta.

## Requisitos EARS

- REQ-01 · Cuando llegan los bytes de un ticket, el sistema deberá producir un PNG de 384 puntos de ancho igual a su vector dorado.
  prueba: tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · ElTicketDeLaLibreriaCoincideConSuVectorDorado
- REQ-02 · Si el ticket trae un comando ESC/POS desconocido, entonces el sistema deberá registrarlo e interpretar el resto del ticket.
  prueba: tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · UnComandoDesconocidoSeRegistraYSigue
- REQ-03 · Cuando termina un ticket, el sistema deberá guardar su `.bin` y su `.png` con un id en la carpeta de salida.
  prueba: tests/PrintThermal.Emulator.Tests/Almacen/AlmacenTests.cs · CadaTicketGuardaBinYPng
- REQ-04 · Cuando los bytes de un ticket llegan en trozos de cualquier tamaño, el sistema deberá producir el mismo PNG que con los bytes enteros.
  prueba: tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · ElTroceoNoCambiaElResultado
- REQ-05 · Si se pide, lee o borra un id sin la forma de un id, entonces el sistema deberá responder que no existe sin tocar archivos.
  prueba: tests/PrintThermal.Emulator.Tests/Almacen/AlmacenTests.cs · UnIdInvalidoNoSaleDeLaCarpeta

## Casos borde

- Ticket vacío; corte de papel sin contenido; imagen raster más ancha que 384 puntos.
- Carga declarada desmesurada (GS v 0, GS 8 L, FS q de más de 4 MiB) y un ticket que avanza más que el alto máximo (20 000 puntos): se avisa y se sigue.

## Fuera de alcance

- Fidelidad calibrada contra la impresora física (segunda etapa, intake §6).
- Transporte Bluetooth: PAR-PTE-03.

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · ElTicketDeLaLibreriaCoincideConSuVectorDorado → [EV-2026-11-27-pruebas]
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · UnComandoDesconocidoSeRegistraYSigue → [EV-2026-11-27-pruebas]
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Almacen/AlmacenTests.cs · CadaTicketGuardaBinYPng → [EV-2026-11-27-pruebas]
- Criterio REQ-04 → tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · ElTroceoNoCambiaElResultado → [EV-2026-11-27-pruebas]
- Criterio REQ-05 → tests/PrintThermal.Emulator.Tests/Almacen/AlmacenTests.cs · UnIdInvalidoNoSaleDeLaCarpeta → [EV-2026-11-27-pruebas]

## Clarify

Corrida de un flujo sin persona presente: ninguna pregunta se le hizo a la persona y ninguna respuesta se le atribuye. Las dudas se contestaron con el corpus y la especificación; lo que no tiene fuente queda como supuesto rotulado.

1. ¿Qué comandos hay que dibujar? Los que emite la librería en sus dos caminos, `escpos-bitmap` (el de la app de campo) y `escpos` (Especificacion-58HB6 §4.1, §4.2 y §6); los vectores los confirman ejecutándola.
2. ¿Qué es «el ticket»? Del ESC @ al corte; la cola de la librería es GS V 1, GS V 0, ESC d n o nada (§4.1), así que el fin de la conexión también cierra (D5).
3. ¿Fuente, interlineado, alto de barras, respuesta exacta a GS I? Sin medir en la 58HB6 (§8): supuestos del emulador en `PerfilDeImpresora`, fuera de alcance hasta la segunda etapa (intake §6).

## Plan

Plan y tareas en `plan.md` y `tasks.md` (velocidad completa).

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

- Versiones subidas: REQ-HIS-Ticket-A-Png v1 → v2 y REQ-HIS-Almacen-De-Tickets v1 → v2 (enunciado, criterios, trazas y `rutas:` con sus pruebas, D7). `lee` pasó a REQ-HIS-Compilacion-Verificada@v2 y `base_commit` a `9dec7ce` por re-planificación en `main` (`af03a45`), por el «I-3 anticipado» de `ready --contra main`.
- PAR-PTE-04 lee REQ-HIS-Almacen-De-Tickets@v1: al empezar, su `ready --contra main` pedirá subir la huella a v2 (I-3).
- Changelog: «PAR-PTE-02 · …» en `cdd/corpus/CHANGELOG.md`.
- Evidencia: `evidencia/2026-11-27-rojo.txt` (30 de 30 pruebas nuevas en rojo contra el API sin implementar; las 4 del esqueleto en verde), `evidencia/2026-11-27-pruebas.txt` y `.trx` (49 de 49 en verde tras la auditoría, SDK 10.0.403 en contenedor con turno), `evidencia/2026-11-27-mutacion.txt` (ocho mutantes; cada uno pone en rojo la prueba de su REQ o de su guarda, incluidas las agregadas tras la auditoría).
- Decisiones: D1 a D7, todas K1, sin K3 pendiente.
- Auditor: `evidencia/2026-11-27-auditoria.md` («refutado» sobre `8d7ce53`; los quince hallazgos se atendieron como dice su §2).
- Pendientes: LAZ-0001 (fin de un ticket sin corte con la conexión abierta) y LAZ-0002 (respuestas y longitudes de protocolo sin medir).
- Supuestos de fidelidad visual, fuera de alcance hasta la segunda etapa (intake §6), en `PerfilDeImpresora` o en el intérprete y NO VERIFICADO contra la 58HB6: fuente, interlineado de 30 puntos, márgenes de 8, alto de barras 80 y módulo 2, alto máximo de 20 000 puntos, QR sin zona de silencio, alineación aplicada a raster, QR y barras, avance de ESC d después de imprimir la línea, modo y máscara del QR, texto legible del EAN-13 sin dibujar.
- NO VERIFICADO además: que un lector de QR o de EAN-13 lea los PNG (se comparó con QRCoder y con la tabla de EAN-13, no con un lector).
- Para PAR-PTE-03: el transporte conecta `AlmacenDeTickets.Escuchar`, llama `Finalizar` al cerrarse la conexión y manda por el canal lo que emite `Respuesta` (LAZ-0001).
- Lo aprendido: la sobrecarga de texto de QRCoder elige modo alfanumérico o segmentos mixtos, así que la referencia del codificador propio se genera con `CreateQrCode(byte[])`; un `exit` dentro del comando del contenedor cortaba la devolución de resultados (el lanzador local lo corre en un subshell); y `cierre` lee `lee` de la parcela en la base, así que un «I-3 anticipado» se resuelve re-planificando en `main`, no en el PR.
