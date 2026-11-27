---
id: PAR-PTE-02
titulo: Core: intérprete ESC/POS de 58 mm a PNG y almacén de tickets
version: 1.0
estado: lista
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
  - {id: D1, tema: "vectores dorados generados ejecutando la librería cliente en contenedor con turno, o derivados de sus pruebas", clase: K1}
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

## Plan

Plan y tareas en `plan.md` y `tasks.md` al empezar (velocidad completa).

## Tareas

- Pruebas primero: cada `prueba:` en rojo antes del código.

## Cierre

Pendiente.
