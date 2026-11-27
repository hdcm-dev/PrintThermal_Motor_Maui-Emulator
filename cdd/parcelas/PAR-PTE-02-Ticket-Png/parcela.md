---
id: PAR-PTE-02
titulo: Core: intérprete ESC/POS de 58 mm a PNG y almacén de tickets
version: 1.0
estado: en-curso
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

## Casos borde

- Ticket vacío; corte de papel sin contenido; imagen raster más ancha que 384 puntos.

## Fuera de alcance

- Fidelidad calibrada contra la impresora física (segunda etapa, intake §6).
- Transporte Bluetooth: PAR-PTE-03.

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · ElTicketDeLaLibreriaCoincideConSuVectorDorado
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/EscPos/InterpreteTests.cs · UnComandoDesconocidoSeRegistraYSigue
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Almacen/AlmacenTests.cs · CadaTicketGuardaBinYPng

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

Pendiente.
