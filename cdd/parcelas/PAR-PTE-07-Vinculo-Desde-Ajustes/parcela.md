---
id: PAR-PTE-07
titulo: Vincular desde Ajustes de Android y conectar después, con cifrado; ticket del visor sin desborde en pantalla angosta
version: 1.0
estado: cerrada
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
  - ADR-0009@nuevo
appetite: un día
pendientes:
  - {que: "sin medir: vincular conectando como primera conexión después de encender el Bluetooth de Android con otros equipos vinculados; una app que llama createBond() y conecta enseguida; un teléfono real", registro: LAZ-0016}
decisiones:
  - {id: D1, tema: "tras un vínculo nuevo que terminó sin cifrar, la impresora hace una conexión ACL breve con el teléfono y la corta (opción Emulador:Bluetooth:ReconexionTrasVinculoSinCifrar, por omisión true)", clase: K2, adr: ADR-0009}
  - {id: D5, tema: "aprobar ADR-0009 (opción A, reconexión breve), gobierno humano", clase: K3, sobre: ADR-0009, decide: "responsable técnico del emulador, en rol por delegación del Product Owner (testimonio F1, registro privado) 2026-11-27 opción A"}
  - {id: D2, tema: "traza HCI opcional del periférico (Emulador:Bluetooth:TrazaHci, por omisión false): cada comando y evento salvo datos ACL y Number Of Completed Packets", clase: K1}
  - {id: D3, tema: "tarjeta del ticket del visor: ancho min(58 mm, 100 %), hijos de la grilla con min-width 0 y una sola columna mínima 0 en el detalle; el ticket conserva 48 mm mientras entra", clase: K1}
  - {id: D4, tema: "REQ-03 se verifica con recorrido en Chrome con emulación de ancho (antes con las reglas de main repuestas, después con la hoja de la rama), no con una prueba de la CI: la CI pública no tiene navegador", clase: K1}
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

## Clarify

- ¿La corrección puede cambiar el comportamiento de la impresora emulada respecto de la 58HB6 física? Sí, si es la única forma de que el camino de Ajustes cifre en el emulador de Android con netsim; se decide por ADR (D1, ADR-0009) y queda apagable.
- ¿Qué cuenta como «540 px»? La pantalla de 540 px del emulador de Android donde se vio el desborde (viewport de 205 px con escala 2,625) y los anchos de viewport de 540 px o menos.

## Plan

Ver `plan.md`.

## Tareas

Ver `tasks.md`.

## Cierre

- Versiones subidas: REQ-HIS-Descubrimiento-Y-Emparejamiento v3 → v4 (historia y `modifica`), REQ-HIS-Visor-Web v3 → v4, REQ-HIS-Api-De-Tickets v3 → v4 y REQ-HIS-Documentacion-De-Uso v2 → v3. Changelog: «PAR-PTE-07 · …».
- Diagnóstico (`evidencia/2026-11-27-diagnostico.md`): la pila propia contesta capacidades y confirmación y guarda la clave; Android con netsim atribuye el fin de la vinculación (manejador 0, primera conexión después de encender su Bluetooth) a otro equipo vinculado antes, deja el nuevo «AUTHENTICATING» y en la conexión siguiente no autentica ni cifra. Causa en la pila de Android con netsim, con cita de su código fuente oficial; la versión exacta de la imagen API 37: NO VERIFICADO.
- Corrección (D1, ADR-0009) en `Hci/PerifericoBluetooth.cs`: reconexión breve tras un vínculo nuevo sin cifrar.
- Evidencia (corridas con SDK 10.0.403 en contenedor con turno en el equipo de cómputo):
  - `evidencia/2026-11-27-rojo.txt`: la prueba de REQ-01 en rojo sin la corrección (el host no envía Create Connection); las de REQ-02 y del caso sin vínculo nuevo pasan también sin la corrección: su poder lo mide la mutación.
  - `evidencia/2026-11-27-pruebas.txt` y `.trx`: 143 de 143 en verde sobre `17bedd6`.
  - `evidencia/2026-11-27-mutacion.txt`: 10 mutantes, 10 en rojo (REQ-01, REQ-02, casos borde, traza, opciones del medio; uno con la reacción diferida 500 ms).
  - `evidencia/2026-11-27-extremo-a-extremo.md`: emulador de Android API 37 y la imagen de la rama en su red, con turno. Camino (a), vínculo desde Ajustes por interfaz como primera conexión con otros equipos vinculados: 3 de 3 cifrados al primer intento de la app (`…:31`, `…:33` y, con la imagen final, `…:41`), con series de 5 impresiones de 5 IGUAL. Camino (b), vincular conectando: sin reconexión breve, 5 de 5 IGUAL dos veces. Controles sin la condición (manejador distinto de 0 con la corrección apagada, y sin otros equipos vinculados): Android cifra en la conexión del vínculo y la reconexión breve no se dispara.
  - `recorrido/`: visor en Chrome de escritorio con emulación de ancho; con las reglas de `main`, a 205 px (la pantalla de 540 px del emulador) la página desborda 30 px; con la rama, ningún desborde de 540 a 205 px. No se repitió en el Chrome del emulador de Android.
- Documentación de uso: `docs/emparejar-e-imprimir.md` §3 (camino de Ajustes), `docs/diagnostico.md` §3 y `docs/montaje.md` §5 (variables nuevas).
- Divergencia corpus/código: ninguna que contradiga un ADR. ADR-0009 registra que la impresora emulada inicia una conexión que la 58HB6 física no inicia en este flujo.
- Nota: LAZ-0011 §1 (re-planificación, PR #8) atribuye al Product Owner la frase «emparejar desde Ajustes y después conectar por RFCOMM tiene que funcionar con cifrado, igual que con la impresora real»: es el encargo de esta etapa tal como llegó al agente, no una cita literal del Product Owner.
- NO VERIFICADO: la imagen publicada con este cambio (sale con el próximo release); el camino (b) como primera conexión después de encender el Bluetooth de Android, una app que llama `createBond()` y conecta enseguida, y un teléfono real (LAZ-0016); que Android reutilice la conexión breve para la app (la impresora la corta apenas se completa); el visor en el Chrome del emulador de Android.
- Auditoría (`evidencia/2026-11-27-auditoria.md`): la ronda 1 refutó el cierre; cada hallazgo tiene respuesta en código, pruebas, mediciones nuevas o texto.
- Efecto colateral de la medición: la configuración de vínculos de Android del emulador de pruebas se borró para el control sin otros equipos (copia dentro del emulador).
