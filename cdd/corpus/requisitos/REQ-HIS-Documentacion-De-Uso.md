---
id: REQ-HIS-Documentacion-De-Uso
version: 3
epica: REQ-EPI-Montaje-Y-Uso
gobierno: parcela
tipo: referencia
rutas:
  - docs/**
  - tests/PrintThermal.Emulator.Tests/Documentacion/**
---
# REQ-HIS-Documentacion-De-Uso

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

Documentación de uso (montaje, API, visor) versionada con el código en el repositorio principal, para que los agentes la consulten, diseñada con el conocimiento de documentación como código declarado en `consulta:` del lock como guía, sin IA Layer (ADR-0007).

## 2. Criterios

- Dado un agente que no conoce el emulador, cuando sigue la documentación de uso, entonces levanta el contenedor e imprime un ticket.
- Dada una ruta, una clave de configuración o un ejemplo del cliente que cambia en el código y no en `docs/`, cuando corre la CI, entonces la prueba de la documentación falla.

## 3. Trazas

- Sube de: REQ-EPI-Montaje-Y-Uso
- Implementado en: `docs/` (índice en `docs/README.md`, ficha de diseño en `docs/diseno-documental.md`); prueba en `tests/PrintThermal.Emulator.Tests/Documentacion/` (PAR-PTE-06).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
| v2 | PAR-PTE-06 | `docs/` con montaje, emparejamiento, API, visor, diagnóstico y recetas; ejemplos que corren en la CI; criterio de deriva; `rutas:` suma las pruebas |
| v3 | PAR-PTE-07 | Emparejar desde Ajustes (§3 de `docs/emparejar-e-imprimir.md`), diagnóstico del vínculo sin cifrar y variables nuevas de `Emulador:Bluetooth` |
