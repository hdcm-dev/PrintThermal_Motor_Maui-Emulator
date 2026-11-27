# Auditoría de cierre · PAR-PTE-03 (dos rondas)

Informes verbatim del auditor refutador (04 §2 paso 5). La primera ronda auditó la cabeza 05789b1; la segunda, después de las correcciones.

## Ronda 1

auditor: claude -p, claude-opus-5-5, otro contexto sin la ejecución (mismo modelo: garantía reducida)
VEREDICTO: refutado

## Hallazgos

1. **bloqueante · E2 + E1 · Entrega «al almacén» sin dueño y sin declarar.** REQ-03 dice «deberá entregarlos al almacén sin cambios» (`parcela.md:49`) y la historia, «se entregan al almacén de tickets» (`REQ-HIS-Recepcion-Integra.md:22`). Su traza v2 afirma en presente que la entrega va por `IReceptorDeBytes`, «que el host del servicio conecta con el almacén» (`:31`). D6 (`parcela.md:30`) manda esa conexión a PAR-PTE-04. Sin embargo, `PAR-PTE-04-Acceso/parcela.md` no lee ni modifica REQ-HIS-Recepcion-Integra, y un grep de `Bluetooth|RFCOMM|Receptor|Recepcion|PAR-PTE-03` en PAR-PTE-04, 05 y 06 no devuelve nada. No se aplica la excepción «trabajo de una parcela ya planificada» (04 §6), porque ninguna parcela lo planifica. Falta una fila en `pendientes:` con su LAZ, o planificar la conexión en PAR-PTE-04. La prueba `LosBytesLleganIntegros` llega hasta `ReceptorEnMemoria`, no hasta el almacén.

2. **bloqueante · E2 + E4 · Un hallazgo abierto aparece como «NO VERIFICADO».** La prueba real registra un fallo de CE-2: el consentimiento llegó 31 s tarde, el vínculo quedó en state=12 pero en «AUTHENTICATING», y la conexión siguiente venció (`prueba-real.txt:44-47`; también 154-166, donde la dirección :01 no vuelve a abrir RFCOMM). El cierre lo deja como «NO VERIFICADO: la causa…» (`parcela.md:86`) y en «Lo aprendido» (`:87`), sin LAZ. Según 04 §6, una «duda o hallazgo sin respuesta» va en `pendientes:` con un LAZ abierto. LAZ-0005 trata otra cosa (el contenedor). Además, el caso puede repetirse en uso real (una persona que tarda en tocar «Pair»), y no está claro si el lado del emulador lo agrava al guardar la clave del par.

3. **bloqueante · E1 · D1 no tiene detrás una mesa terminada.** El cierre afirma que «D1 (K2) la decidió una mesa de otro contexto» (`parcela.md:85`), y T5 está marcada como hecha (`tasks.md:12`). El contrato de la mesa pide tres comisiones, después un refutador y después un jurado de cinco contextos (contrato §5). En el expediente del repositorio de documentación hay `01-requisitos`, `02-verificacion` y `03-lector`, pero `04-refutador.md` y `04-refutador.md.err` están vacíos y no hay ningún informe del jurado. Lo único que existe de P3 es el borrador del lector. Así, la decisión que va a la Review no está respaldada por su evidencia.

4. **menor · E1 · La cuenta de pruebas no cierra.** El cierre suma 13 pruebas en rojo, 4 del esqueleto y la de desconexión, que da 18, y declara 20 de 20. `pruebas.txt:1` habla de «16 nuevas». Las dos de `HciNetsimTests` (`ElTransporteHablaGrpcSobreHttp2ConNetsim` y `SinNetsimEscuchandoElServicioQuedaEnErrorYReintenta`) no aparecen en `rojo.txt`, nacieron en el commit verde (2cec4b8) y el cierre no lo dice. No es `prueba:` de ningún REQ, así que no rompe la regla de pruebas primero, pero la prosa no lo cuenta.

5. **menor · E2 · La prueba de REQ-01 es en parte tautológica.** `SdpTests.cs:80` arma el pedido byte a byte, pero decodifica la respuesta con `ElementoSdp.Decodificar`, el mismo código de producción que la codificó. Un error simétrico en los descriptores de tipo o tamaño pasaría sin detectarse, y M1 sólo cambia la constante 0x1101. Lo compensa la prueba real: Android lista `00001101-…` y abre RFCOMM en el canal 1 (`prueba-real.txt:68, 187`).

6. **menor · E2 · La prueba de REQ-02 inyecta el éxito.** `SeguridadTests.cs:44-62` comprueba bien cada respuesta HCI del emulador, pero el «se completa» llega como entrada inyectada (evento 0x36 con estado 0 y 0x18), no como resultado. El emparejamiento de extremo a extremo sólo lo prueba la evidencia real. Además, la clave tipo 4 está sin autenticar, sin MITM (`prueba-real.txt:60`): «seguro por confirmación» significa aquí consentimiento sobre Just Works. D3 y la P2 de la mesa lo cubren, siempre que la mesa termine (ver hallazgo 3).

7. **menor · E2 + E4 · L2CAP acepta opciones desconocidas que no son sugerencia.** `MultiplexorL2cap.cs:177-191` sólo trata MTU (0x01) y RFC (0x04). Cualquier otra opción con el bit de sugerencia en 0 se acepta en silencio con resultado 0. Core vol. 3, parte A, §5 (bit de sugerencia) pide rechazarla con «unknown options» (0x0003). Hoy no se dispara con Android (MTU 1024 y 1691 en la evidencia).

8. **menor · E2 · Dos detalles de ServiceSearch en SDP.** En `ServidorSdp.cs:64-68`, TotalServiceRecordCount devuelve la cuenta ya truncada y se ignora la continuación. Además, se acepta un MaximumAttributeByteCount menor que 7 (`:117`). Con un solo registro no tiene efecto.

9. **menor · E2 · Evidencia real difícil de reproducir.** El ejecutable que calcula el SHA del lado receptor «no está versionado» (`prueba-real.txt:3-5`) y no se dice de qué commit sale el binario cuyo sha256 se publica. Además, hay una transferencia de 64 KB (`:190`, conexión 1, sha `29a3…`) sin su contraparte de envío y que no se informa. No contradice el «10 de 10», pero ese número sale de una selección.

10. **menor · E4 · Orden de los pasos de cierre.** `estado: cerrada` (`parcela.md:5`) y la fila «cerrada» de SPR-01 ya están puestas antes de este auditor, aunque el commit dice «cierre (borrador)». 04 §2 pone `estado: cerrada` en el paso 6, después del auditor del paso 5.

11. **C · Sin keepalive.** Se quitaron los pings HTTP/2 («Lo aprendido»). Sin ellos, un flujo gRPC medio caído podría no detectarse hasta el próximo envío. No está medido, y ADR-0004 no registra esta restricción.

NO VERIFICADO: si responder DM a un PN de un DLCI sin servidor (`SesionRfcomm.cs:255-258`) cumple TS 07.10 y RFCOMM, o si correspondía contestar con PN. Tampoco verifiqué si echar de vuelta la bandera de continuación en la respuesta de configuración L2CAP (`MultiplexorL2cap.cs:192`) es lo que pide la especificación.

## Lo que revisé y está bien

- **RFCOMM, bit C/R:** las respuestas UA y DM del lado que responde llevan C/R=1, y sus comandos UIH y DISC llevan C/R=0 (`SesionRfcomm.cs:287-297`). Es coherente con la tabla de TS 07.10 §5.2.1.2. El simulador también verifica C/R=1 en UA y C/R=0 en la PN de respuesta.
- **RFCOMM, FCS y créditos:** la FCS de UIH cubre sólo dirección y control, y la de las demás tramas incluye el largo (`:307`). La PN pide control por créditos con 0xF0 y responde 0xE0. K va en 3 bits (≤7) y N1 en little endian en los octetos 4-5; N1 ≤ MTU−6, con cabecera máxima de 6. Una trama de crédito sin datos no consume crédito (`:179-181`). MSC 0xE3/0xE1 con la dirección `(dlci<<2)|3`. Valores de control: SABM 0x3F, UA 0x73, DM 0x1F, DISC 0x53.
- **Independencia de la prueba de REQ-03:** el simulador calcula la FCS bit a bit, sin tabla, y lee tramas sin código de producción (`HciSimulacion.cs:276-336`). Los vectores 0x1C (SABM en DLCI 0) y 0xD7 (UA) son los conocidos de TS 07.10. La fragmentación ACL respeta PB=10 y 01 y los buffers del controlador. No es tautológica.
- **L2CAP:** la respuesta de conexión lleva DCID y SCID en ese orden, y PSM no admitido da 0x0002. La respuesta de configuración lleva el CID del otro lado. Un modo distinto del básico da 0x0001 con RFC básico. La respuesta de información anuncia funciones 0x80 y canales fijos 0x02. El MTU por defecto es 672.
- **SDP:** la continuación usa un desplazamiento de 2 bytes, tope MTU−10 y error 0x0005, y la prueba en trozos (máximo 17, MTU 48) devuelve lo mismo. Los atributos son correctos: 0x0006 en/UTF-8 con base 0x0100, 0x0009 SPP 0x0102, 0x0005 PublicBrowseRoot y 0x0004 L2CAP/RFCOMM con canal 1.
- **SSP:** son correctos los códigos y los formatos de 0x042B (con IO 0x03, OOB 0 y autenticación 0x04), 0x042C, 0x042F, 0x040B, 0x040C y 0x040D (23 bytes), y de los eventos 0x16, 0x17, 0x18, 0x31, 0x32, 0x33, 0x34 y 0x36 (en 0x36, el estado va antes de la dirección). La clave tipo 4 es coherente con Just Works y con la variante 3 de Android.
- **Anuncio:** CoD 0x040680 coincide con `class=40680`. El EIR trae nombre completo «58HB6» y la UUID 0x1101. Write_Local_Name va con 248 bytes.
- **Rojo y mutación:** el rojo va antes que el código (orden 27867a9 → 2cec4b8), con 13 pruebas en rojo y 4 en verde. Las seis mutaciones están en rojo (M3 compila).
- **Prueba real:** las cifras coinciden: 3 de 3 en unos 2 s, 5 + 5 iguales y 2 reinicios sin reemparejar.
- **Pendientes bien tratados:** LAZ-0005 trata exactamente el desvío del contenedor y su precondición de CE-1. La prueba con la librería cliente queda en PAR-PTE-05 (REQ-HIS-Imagen-Publicada), y la medición de la 58HB6 real, en la segunda etapa firmada. Ninguna de las dos es un pendiente.
- **Exposición (5):** no hay rutas de usuario (las de los registros son `/w/` del contenedor), no hay credenciales y sólo aparece loopback. Las direcciones son sintéticas (04:00:…, 02:00:…, 02:11:22:33:44:55) y la de Android va enmascarada. El `computerName` del TRX es el id efímero de un contenedor.
