---
id: REQ-HIS-Compilacion-Verificada
version: 1
epica: REQ-EPI-Distribucion
gobierno: parcela
tipo: referencia
rutas:
  - global.json
  - Directory.Packages.props
  - tests/PrintThermal.Emulator.Tests/Esqueleto*.cs
---
# REQ-HIS-Compilacion-Verificada

## Índice

1. [Enunciado](#1-enunciado)
2. [Criterios](#2-criterios)
3. [Trazas](#3-trazas)
4. [Historial](#4-historial)

## 1. Enunciado

La solución `PrintThermal.Emulator` (proyectos Core, Bluetooth, Api, Client y Tests) compila y se prueba en runners de GitHub en cada PR y en cada push a `main`, con el SDK y las versiones de los paquetes fijas (ADR-0006).

## 2. Criterios

- Dado un PR, cuando corre la CI, entonces compila la solución y corre sus pruebas en `ubuntu-latest` sin descargar la norma CDD.
- Dado un paquete NuGet de la solución, cuando se lo busca, entonces su versión está fija en `Directory.Packages.props` y ningún proyecto la declara.

## 3. Trazas

- Sube de: REQ-EPI-Distribucion
- Implementado en: todavía no hay código (siembra del 2026-11-27).

## 4. Historial

| Versión | Parcela | Cambio |
|---|---|---|
| v1 | siembra | Alta |
