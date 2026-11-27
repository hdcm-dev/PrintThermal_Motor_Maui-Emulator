# SPR-01

## Índice

1. [Sprint Goal](#1-sprint-goal)
2. [Parcelas](#2-parcelas)
3. [Olas](#3-olas)
4. [Review](#4-review)
5. [Retrospectiva](#5-retrospectiva)

## 1. Sprint Goal

Construir el emulador 1.0 en .NET 10 y publicar su release e imagen en GitHub cuando pase las pruebas. Palabra de la persona: E2 de `cdd/testimonios/Vision/OUTPUTs/01-respuestas.md` («Construí y publicá la 1.0 … construyo el emulador en .NET 10 y publico release e imagen en GitHub cuando pase las pruebas. Lleva varias etapas.»), 2026-11-27. Selección de parcelas: DEC-007.

Por qué es valioso: cierra el Product Goal (CE-1 a CE-5, PAR-PTE-02 a PAR-PTE-04) y deja una imagen de versión fija que otros agentes pueden montar (PAR-PTE-05) y consultar (PAR-PTE-06).

## 2. Parcelas

| Parcela | Épica | Historias | Decisiones K3 | Estado |
|---|---|---|---|---|
| PAR-PTE-01 | REQ-EPI-Distribucion | REQ-HIS-Compilacion-Verificada | — | cerrada |
| PAR-PTE-02 | REQ-EPI-Ticket-Png | REQ-HIS-Ticket-A-Png, REQ-HIS-Almacen-De-Tickets | — | cerrada |
| PAR-PTE-03 | REQ-EPI-Medio-Emulado | REQ-HIS-Descubrimiento-Y-Emparejamiento, REQ-HIS-Recepcion-Integra | — (K2: mesa de TS-2 y TS-3, ADR-0004, al lote de la Review) | cerrada |
| PAR-PTE-04 | REQ-EPI-Acceso | REQ-HIS-Api-De-Tickets, REQ-HIS-Visor-Web, REQ-HIS-Cliente-Tipado | — (K2 D2 y D3 al lote de la Review) | cerrada |
| PAR-PTE-05 | REQ-EPI-Distribucion | REQ-HIS-Imagen-Publicada | — | cerrada |
| PAR-PTE-06 | REQ-EPI-Montaje-Y-Uso | REQ-HIS-Documentacion-De-Uso | — | cerrada |
| PAR-PTE-07 | REQ-EPI-Medio-Emulado | REQ-HIS-Descubrimiento-Y-Emparejamiento | — (LAZ-0011 y LAZ-0015) | cerrada |

La etapa T6 (recorrido del visor en navegador real) va dentro de PAR-PTE-04, por `interfaz: n2` de REQ-HIS-Visor-Web. Fuera del sprint: REQ-HIS-Conocimiento-De-Montaje (ADR-0008).

## 3. Olas

```text
Ola 1: PAR-PTE-01
Ola 2: PAR-PTE-02, PAR-PTE-03
Ola 3: PAR-PTE-04
Ola 4: PAR-PTE-05
Ola 5: PAR-PTE-06
Ola 6: PAR-PTE-07
  PAR-PTE-01 antes que PAR-PTE-02 por REQ-HIS-Compilacion-Verificada
  PAR-PTE-01 antes que PAR-PTE-03 por REQ-HIS-Compilacion-Verificada
  PAR-PTE-01 antes que PAR-PTE-04 por REQ-HIS-Cliente-Tipado, REQ-HIS-Compilacion-Verificada
  PAR-PTE-02 antes que PAR-PTE-04 por REQ-HIS-Almacen-De-Tickets
  PAR-PTE-03 antes que PAR-PTE-05 por REQ-HIS-Descubrimiento-Y-Emparejamiento
  PAR-PTE-04 antes que PAR-PTE-05 por REQ-HIS-Api-De-Tickets
  PAR-PTE-04 antes que PAR-PTE-06 por REQ-HIS-Api-De-Tickets
  PAR-PTE-05 antes que PAR-PTE-06 por REQ-HIS-Imagen-Publicada
  PAR-PTE-06 antes que PAR-PTE-07 por REQ-HIS-Documentacion-De-Uso
```

## 4. Review

<!-- K3: una línea «<persona> <AAAA-MM-DD> aceptado | sin aceptar» (la exige `cdd.py ready` del sprint siguiente) y qué se corrige (historias, Goal, Visión). -->

## 5. Retrospectiva

<!-- Propuestas de norma o conocimiento, cada una con su PR. -->
