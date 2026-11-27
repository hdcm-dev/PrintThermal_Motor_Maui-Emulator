---
id: PAR-PTE-03
titulo: Bluetooth Classic propio en C# contra netsim: descubrimiento, emparejamiento y recepción por RFCOMM
version: 1.0
estado: cerrada
velocidad: completa
base_commit: 9dec7ce0bdb56b1ee990cc97cba6015cf79c0afb
sprint: SPR-01
epica: REQ-EPI-Medio-Emulado@v1
historias:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v1
  - REQ-HIS-Recepcion-Integra@v1
lee:
  - REQ-HIS-Compilacion-Verificada@v3
  - VIS@v1
  - REQ-EPI-Medio-Emulado@v1
  - ADR-0004@v1
modifica:
  - REQ-HIS-Descubrimiento-Y-Emparejamiento@v1
  - REQ-HIS-Recepcion-Integra@v1
appetite: tres días
pendientes:
  - {que: "el periférico en un contenedor propio con --network container: del emulador no está medido (la prueba real corrió dentro del contenedor del emulador)", registro: LAZ-0005}
  - {que: "la entrega de IReceptorDeBytes al almacén de tickets no tiene parcela que la conecte", registro: LAZ-0010}
  - {que: "un vínculo quedó inservible en la prueba real (causa sin separar: consentimiento tardío o binario con pings); falta recuperación y confirmación automática", registro: LAZ-0011}
  - {que: "el consumo de tokens de la pila no se asienta frente al umbral firmado de 2,5 M (R:15)", registro: LAZ-0012}
decisiones:
  - {id: D1, tema: "TS-2 y TS-3: pila propia frente a puente y emparejamiento emulado frente al real. Mesa de otro contexto (tres comisiones, refutador y jurado de cinco, mismo modelo): TS-3 = A y TS-2 = A por unanimidad; el texto para ADR-0004 va al lote de K2 de la Review", clase: K2, adr: ADR-0004}
  - {id: D2, tema: "gRPC de netsim sin paquetes: HTTP/2 sin TLS de HttpClient y protobuf escrito a mano para los cinco mensajes que se usan; no suma dependencias ni toca Directory.Packages.props", clase: K1}
  - {id: D3, tema: "SSP sin entrada ni salida (IO 0x03) y bonding general sin MITM (0x04): Android pide consentimiento y el emulador confirma; PIN 0000 sólo si Android usa emparejamiento heredado", clase: K1}
  - {id: D4, tema: "registro SDP: el de la prueba corta (handle, clase 0x1101, L2CAP + RFCOMM canal 1, navegación pública) más perfil SPP 1.2, base de idioma y nombre de servicio «58HB6»; el registro real de la 58HB6 sigue sin medir", clase: K1}
  - {id: D5, tema: "re-planificación: lee REQ-HIS-Compilacion-Verificada@v2 (I-3); ADR-0004 sale de modifica porque tiene gobierno humano y la parcela no lo toca; las historias suben a v2 para cubrir sus pruebas en rutas", clase: K1}
  - {id: D6, tema: "la frontera con el almacén de tickets es IReceptorDeBytes en Rfcomm (Core no está fusionado); el host del servicio la conecta en PAR-PTE-04", clase: K1}
sin_impacto_de_diseno: false
motivo_sin_impacto:
---
## Objetivo

Que el emulador de Android descubra, empareje e imprima en «58HB6» (CE-1, CE-2, CE-3).

## Escenarios

- Camino feliz: Android encuentra «58HB6», la empareja por confirmación y le imprime; los bytes llegan íntegros.
- No feliz: netsim no responde por gRPC; el servicio lo informa en su estado y reintenta.

## Requisitos EARS

- REQ-01 · Cuando Android busca dispositivos cercanos, el sistema deberá anunciarse como «58HB6» con el perfil de puerto serie (UUID 0x1101) en SDP.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/SdpTests.cs · ElRegistroSdpPublicaSppConNombre58HB6
- REQ-02 · Cuando Android pide emparejar, el sistema deberá aceptar el emparejamiento seguro por confirmación con su dirección fija.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadTests.cs · ElEmparejamientoPorConfirmacionSeCompleta
- REQ-03 · Cuando llegan datos por RFCOMM, el sistema deberá entregarlos al almacén sin cambios.
  prueba: tests/PrintThermal.Emulator.Tests/Bluetooth/RfcommTests.cs · LosBytesLleganIntegros

## Casos borde

- Reinicio del emulador de impresora con el vínculo guardado en Android; desconexión a mitad de un ticket.

## Fuera de alcance

- iOS y NFC (intake §6).
- Inyección de fallos (intake §6).

## Criterio ↔ prueba

- Criterio REQ-01 → tests/PrintThermal.Emulator.Tests/Bluetooth/SdpTests.cs · ElRegistroSdpPublicaSppConNombre58HB6
- Criterio REQ-02 → tests/PrintThermal.Emulator.Tests/Bluetooth/SeguridadTests.cs · ElEmparejamientoPorConfirmacionSeCompleta
- Criterio REQ-03 → tests/PrintThermal.Emulator.Tests/Bluetooth/RfcommTests.cs · LosBytesLleganIntegros

## Plan

`plan.md` y `tasks.md`. Opción A de ADR-0004 (la del intake firmado); D1 la decide una mesa de otro contexto con el resultado de la prueba real, y el PO la aprueba en lote en la Review (K2).

## Tareas

`tasks.md`. Pruebas primero: cada `prueba:` en rojo antes del código (`evidencia/2026-11-27-rojo.txt`).

## Clarify

Sin persona en esta corrida; no hubo huecos que reabrieran la parcela. Lo que el corpus no fijaba se decidió como K1 (D2 a D6).

## Cierre

- Versiones subidas: REQ-HIS-Descubrimiento-Y-Emparejamiento v1 → v2 y REQ-HIS-Recepcion-Integra v1 → v2 (`rutas:` con sus pruebas, trazas al código; D5). ADR-0004 no se tocó (gobierno humano).
- Changelog: «PAR-PTE-03 · Transporte Bluetooth Classic propio en C# …» en `cdd/corpus/CHANGELOG.md`. Sprint: fila de PAR-PTE-03 en `cdd/sprints/SPR-01.md`.
- Evidencia:
  - `evidencia/2026-11-27-rojo.txt`: las 13 pruebas escritas primero, en rojo; las 4 del esqueleto, en verde.
  - `evidencia/2026-11-27-pruebas.txt` y `.trx`: 22 de 22 en verde, 18 nuevas y 4 del esqueleto. Las 5 nuevas que no estuvieron en el rojo se escribieron después: transporte gRPC (2), sin pings, desconexión a mitad de ticket y opción L2CAP desconocida.
  - `evidencia/2026-11-27-mutacion.txt`: once mutaciones, todas en rojo. Cubren REQ-01, REQ-02, REQ-03, el transporte, el reintento, la desconexión y los pings.
  - `evidencia/2026-11-27-prueba-real.txt`, contra Android 17, API 37:
    - descubre 3 de 3 en unos 2 s;
    - empareja por SSP con consentimiento;
    - SHA-256 igual 10 de 10 (5 de 64 KB y 5 del ticket de 4104 B), más un envío previo también igual;
    - reconecta sin reemparejar tras 2 reinicios del emulador de impresora.

    Desde la segunda dirección la prueba midió el código del commit 2cec4b8; el descubrimiento con la primera dirección y el primer intento de emparejamiento corrieron con un binario anterior que mandaba pings de HTTP/2. Los cambios posteriores, por el auditor y la mesa, están declarados en ese archivo. El mínimo de MaximumAttributeByteCount de SDP queda en el camino que usó Android, pero sin efecto con los máximos que mandó.
- Desvío de la prueba real: el periférico corrió dentro del contenedor del emulador, en su turno, porque netsim publica su gRPC sólo en loopback y desde el equipo de cómputo no se llega (medido, §1 de la evidencia). El contenedor propio unido a la red del emulador sigue sin medir: LAZ-0005.
- Decisiones:
  - D1 (K2): la decidió una mesa de otro contexto. Expediente en el repositorio de documentación, `PROMPTs/SDD/02-Mesa-TS2-TS3/`: contrato, cuatro informes, jurado y dictamen. Resultado: TS-3 = A y TS-2 = A, 5 de 5. Los hallazgos h1 a h7 fueron procedentes 5 de 5 y h8, 4 de 5. Se aplicaron h6 (la transferencia previa explicada), h7 (el commit juzgado, en el dictamen) y h8 (la prueba sin pings y M11). h2 a h5 van al texto propuesto para ADR-0004. h1 cambia la medida de CE-2, que es de la persona, y va a la Review junto con LAZ-0011.
  - D2 a D6: K1.
- NO VERIFICADO:
  - cómo empareja y qué registro SDP publica la 58HB6 real (Especificación, «[MEDIR]», segunda etapa);
  - el PIN heredado contra Android: con SSP activo en los dos lados ese camino no se recorre, y RootCanal no valida el PIN, así que no tiene criterio en el medio emulado;
  - el periférico con la librería cliente compilada: se usó la app Java mínima, que hace la misma llamada;
  - la causa de LAZ-0011.
- Lo aprendido:
  - netsim corta el flujo gRPC si recibe pings de HTTP/2 cada 30 s sin datos: el transporte no los manda, y una prueba lo fija.
  - Hipótesis, sin separar (LAZ-0011): un consentimiento tardío o el binario con pings dejan el vínculo inservible.
  - Conjetura: sin pings, un flujo gRPC medio caído podría no notarse hasta el próximo envío.
  - Android guarda la clase 0x1F00 si se vincula una dirección sin descubrirla antes.
- Auditor: `evidencia/2026-11-27-auditoria.md`, en rondas.
  - Ronda 1, «refutado». Sus tres bloqueantes se resolvieron: la entrega al almacén (LAZ-0010), el vínculo trabado (LAZ-0011) y la mesa, que ya terminó. De los menores:
    - 4, 5 y 7: corregidos;
    - 6: aceptado por la mesa (TS-2 = A, clave sin autenticar);
    - 8: en parte; el mínimo tiene prueba, pero TotalServiceRecordCount no, y la continuación de ServiceSearch sigue sin atenderse (con un solo registro no tiene efecto);
    - 9: en parte; el commit medido y la transferencia previa están declarados, y el ejecutable receptor sigue sin versionar (lo dice la evidencia);
    - 10: `estado` volvió a `en-curso` hasta la última ronda;
    - 11: es conjetura y quedó en «Lo aprendido».
  - Ronda 2, «refutado». El consumo de tokens pasó a LAZ-0012 (N1). Se corrigió la prosa (N2), LAZ-0011 tiene ahora las dos hipótesis (N3) y se corrigió el alcance de los cambios posteriores (N4).
  - Ronda 3, «no refutado». Se aplicaron sus menores:
    - M1: la evidencia ya no da una sola causa;
    - M2: esta prosa;
    - M3: LAZ-0012 con `origen: parcela`.

    La línea del texto propuesto para ADR-0004 sobre el consentimiento tardío depende de LAZ-0011.
