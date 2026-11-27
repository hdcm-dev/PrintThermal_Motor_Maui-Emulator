---
id: PAR-PTE-07
titulo: Vincular desde Ajustes de Android y conectar después, con cifrado; ticket del visor sin desborde en pantalla angosta
version: 1.0
estado: lista
velocidad: completa
base_commit: 24e034a06927eca2c7892c2f6cfdaf4cdf669c64
sprint: SPR-01
epica: REQ-EPI-Medio-Emulado@v1
historias:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v3
lazo: [LAZ-0011, LAZ-0015]
lee:
  - VIS@v1
  - REQ-EPI-Medio-Emulado@v1
  - ADR-0004@v1
  - REQ-HIS-Recepcion-Integra@v2
  - REQ-HIS-Imagen-Publicada@v2
modifica:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v3
  - REQ-HIS-Visor-Web@v3
  - REQ-HIS-Api-De-Tickets@v3
  - REQ-HIS-Documentacion-De-Uso@v2
appetite: un día
pendientes:
decisiones:
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Que una app que se vincula con «58HB6» desde Ajustes de Android y conecta después por RFCOMM tenga la conexión cifrada e imprima, como con la impresora real, sin romper el camino de vincular conectando; y que el visor no desborde el ticket en pantallas de 540 px o menos.

## Escenarios

- Camino feliz (a): vínculo desde Ajustes (por interfaz), después la app de ejemplo conecta e imprime 5 de 5.
- Camino feliz (b): vincular conectando (camino de PAR-PTE-05), después la app de ejemplo imprime 5 de 5.
- No feliz: el vínculo dedicado termina sin cifrar y Android no pide la clave en la conexión siguiente.
- Visor: pantalla de 540 px o menos con un ticket elegido.

## Requisitos EARS

- REQ-01 · Cuando un vínculo nuevo termina sin que la conexión se haya cifrado, el sistema deberá dejar a Android en condiciones de cifrar la conexión siguiente con la clave guardada.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadVinculoSinCifrarTests.cs · TrasUnVinculoSinCifrarLaImpresoraReconectaYCorta
- REQ-02 · Cuando el vínculo se cifra en la misma conexión (vincular conectando), el sistema deberá seguir como antes, sin conexiones propias.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadVinculoSinCifrarTests.cs · UnVinculoCifradoNoProvocaReconexion
- REQ-03 · Mientras la pantalla tenga 540 px de ancho o menos, el visor deberá mostrar el ticket elegido dentro de su tarjeta, sin desborde horizontal de la página.
  prueba: cdd/parcelas/PAR-PTE-07-Vinculo-Desde-Ajustes/recorrido/guion.md · Recorrido

## Casos borde

- Android conecta mientras la impresora hace su propia conexión; la conexión propia no arranca (estado de error del controlador); una conexión sin vínculo nuevo que se cierra.

## Fuera de alcance

- Cambiar la pila Bluetooth de Android o de netsim; la recuperación de vínculos trabados de versiones anteriores del emulador.

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadVinculoSinCifrarTests.cs · TrasUnVinculoSinCifrarLaImpresoraReconectaYCorta
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadVinculoSinCifrarTests.cs · UnVinculoCifradoNoProvocaReconexion
- Criterio REQ-03 → cdd/parcelas/PAR-PTE-07-Vinculo-Desde-Ajustes/recorrido/guion.md · Recorrido

## Plan

Ver `plan.md`.

## Tareas

Ver `tasks.md`.
