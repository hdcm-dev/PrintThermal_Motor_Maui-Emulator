---
id: REQ-HIS-Ticket-A-Png
version: 2
epica: REQ-EPI-Ticket-Png
gobierno: parcela
tipo: referencia
rutas:
  - src/PrintThermal.Emulator.Core/EscPos/**
  - tests/PrintThermal.Emulator.Tests/EscPos/**
---
# REQ-HIS-Ticket-A-Png

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Un intérprete ESC/POS para papel de 58 mm (384 puntos de ancho) convierte los bytes de un ticket en una imagen PNG (ADR-0005). Los recibe en trozos de cualquier tamaño, entrega cada ticket al terminar (corte, nuevo ESC @ después de contenido o fin de la conexión) y responde las consultas de estado (DLE EOT 1, 2 y 4; GS I 67; GS ( L fn 48 sólo si se configura).

## 2. Criterios

- Dados los bytes que la librería cliente genera para un ticket, cuando se interpretan, entonces el PNG coincide con su vector dorado.
- Dado un comando ESC/POS que el intérprete no conoce, cuando aparece en el ticket, entonces se registra y el resto del ticket se interpreta igual.
- Dados los mismos bytes en trozos de 1, 7 o 256, cuando se interpretan, entonces el PNG es el mismo que con los bytes enteros.

## 3. Trazas

- Sube de: REQ-EPI-Ticket-Png
- Implementado en: `src/PrintThermal.Emulator.Core/EscPos/` (`Interprete`, `PerfilDeImpresora`, `CodigoQr`, `CodigoEan13`, fuente Terminus Font 24x12 en `Fuentes/`); pruebas y vectores dorados en `tests/PrintThermal.Emulator.Tests/EscPos/`.

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-02 | Intérprete incremental, límites del ticket, consultas con respuesta y criterio de troceo; `rutas:` suma sus pruebas y vectores |
