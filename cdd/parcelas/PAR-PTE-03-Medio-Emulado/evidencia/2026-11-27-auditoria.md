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

## Ronda 2

auditor: claude -p, claude-opus-5-5, otro contexto sin la ejecución (mismo modelo: garantía reducida)
VEREDICTO: refutado

### Hallazgos de la ronda 1

| # | Estado | Ancla |
|---|---|---|
| 1 · Entrega al almacén sin dueño | resuelto | `parcela.md:24` → LAZ-0006, que trata exactamente ese problema y propone una prueba de extremo a extremo para PAR-PTE-04. La traza de `REQ-HIS-Recepcion-Integra.md:31` ya no dice que la conexión exista. |
| 2 · Vínculo trabado sin LAZ | resuelto (con un matiz: ver N3) | `parcela.md:25` → LAZ-0007 abierto. Cubre la causa, la recuperación, la confirmación automática y la medida de CE-2. |
| 3 · D1 sin mesa terminada | resuelto | `02-Mesa-TS2-TS3/informes/04-refutador.md` tiene 90 líneas y hay cinco jurados con `VOTO: P1=A P2=A`. El dictamen §2-§3 coincide con `parcela.md:98` (h1-h7 5/5, h8 4/5). |
| 4 · Cuenta de pruebas | resuelto | 22 = 13 del rojo + 5 posteriores + 4 del esqueleto. `pruebas.txt:61` dice «Total tests: 22» y el TRX trae `Counters total="22" passed="22"`. |
| 5 · REQ-01 tautológica | resuelto | `SdpTests.cs` ahora busca bytes escritos a mano para 0x0001, 0x0004 y 0x0100. Eso detecta un error simétrico del codificador en esos atributos. |
| 6 · REQ-02 inyecta el éxito | aceptado como deuda declarada (P2 de la mesa, dictamen §4 «clave sin autenticar, tipo 4») | `SeguridadTests.cs` no cambió en ade3907. «Los menores 4 a 9 se corrigieron» (`parcela.md:110`) es falso para el 6: ver N2. |
| 7 · Opciones L2CAP desconocidas | resuelto | `MultiplexorL2cap.cs:192-202` responde 0x0003 con los tipos rechazados y no marca `ConfigRecibida`. `HciTests.UnaOpcionL2capDesconocidaQueNoEsSugerenciaSeRechaza` prueba una opción sin bit de sugerencia (0x20) y otra con sugerencia (0xA0). |
| 8 · SDP ServiceSearch | resuelto en parte | Cambió el código (`ServidorSdp.cs:65-69`, `:117-120`), pero ninguna prueba ni mutación lo cubre. La continuación de ServiceSearch sigue ignorada (`:64`). Ahora, si el MTU trunca, Total puede quedar mayor que Current sin estado de continuación, lo que antes no pasaba. Con un solo registro no tiene efecto. |
| 9 · Evidencia real poco reproducible | resuelto en parte | El commit medido está declarado (`prueba-real.txt:11`) y la transferencia de 64 KB está explicada (`:78-82`). El receptor sigue «no versionado» (`:4`), pero lo dice. |
| 10 · Orden de los pasos | **no resuelto** | `estado: cerrada` sigue en `parcela.md:5` y en `SPR-01.md:23` antes de esta ronda. 04 §2 lo pone en el paso 6 (`04-Cerrar-Parcela.md:34`). `parcela.md:110` dice «se corrigió»: ver N2. |
| 11 · Sin keepalive | aceptado (conjetura) | La restricción «no se mandan pings» va al texto propuesto para ADR-0004 (`dictamen.md:61`). El riesgo de un flujo medio caído no quedó en «Lo aprendido», como dice `parcela.md:110`: `:107` sólo habla de los pings. |

Los dos «NO VERIFICADO» de la ronda 1 siguen sin verificar: el DM ante una PN a un DLCI sin servidor y el eco de la bandera de continuación en la configuración L2CAP. Nadie los retomó, y no los vuelvo a elevar.

### Hallazgos nuevos

**N1 · bloqueante · Un hallazgo procedente de la mesa queda escondido bajo «NO VERIFICADO».**
- **Qué pasa.** El consumo de tokens de la pila contra el umbral firmado de 2,5 M (R:15) figura sólo como «NO VERIFICADO … que no se asentó» (`parcela.md:104`), sin fila en `pendientes:` ni LAZ.
- **Lo que dice la mesa.** Lo votó procedente 5 de 5 (h2, `dictamen.md:34`), y el propio dictamen lo lista como «Deuda declarada: el consumo de tokens no se asentó (h2)» (`dictamen.md:69-70`). Costo/beneficio advierte que «el techo estimado coincide con el umbral y faltan dos parcelas de pila» (`jurado/03-costo-beneficio.md:34`).
- **Por qué bloquea.** Es un supuesto que cambia algo si es falso: si el consumo supera 2,5 M, D1 vuelve a la persona con B (`dictamen.md:56-57`). Según 04 §6 eso exige un LAZ abierto, y 04 §2 paso 5 nombra este caso: «un rótulo que esconde un pendiente». Es el mismo patrón que el hallazgo 2 de la ronda 1.
- **Agravante.** El dictamen dice «deuda declarada» y la parcela no la declara.
- **Arreglo.** Una fila en `pendientes:` con su LAZ (asentar el consumo de PAR-PTE-03 y de las parcelas de pila siguientes), o asentar el consumo ahora.

**N2 · menor · La prosa del cierre afirma correcciones que no ocurrieron.**
- `parcela.md:110` dice que «los menores 4 a 9 se corrigieron», que «el 10 se corrigió con el orden de los pasos» y que «el 11 … queda en "Lo aprendido"».
- El 6 no se tocó: se acepta por la mesa.
- El 8 se corrigió sin prueba.
- El 10 sigue igual (`parcela.md:5`).
- El 11 no está en «Lo aprendido».

Hay que decir lo que pasó con cada uno o corregirlo. En particular, `estado: cerrada` se pone después de esta ronda.

**N3 · menor · LAZ-0007 y «Lo aprendido» atribuyen la causa al toque tardío, pero la evidencia tiene una variable confundida.**
- **El binario era otro.** El intento fallido (:01, de 15:08:32 a 15:10:30) cae dentro de la primera corrida. Esa corrida usaba el binario que mandaba pings de HTTP/2: los cortes de `prueba-real.txt:166` y de 15:12:04 lo enmarcan, y §6 dice «Se quitaron los pings (ese binario es el de arriba)». No era el código de 2cec4b8, que ya traía «Sin pings».
- **Cambiaron tres cosas a la vez.** El reintento exitoso cambió el binario, la dirección y el tiempo del toque al mismo tiempo.
- **Un dato que el registro no recoge.** En el intento fallido el emulador nunca registra «cifrado en …», y sí lo hace en el exitoso (`:68`, `:192`).

Lo que no se sostiene:
- «Lo aprendido: el toque … tiene que llegar enseguida» (`parcela.md:108`) presenta como aprendido algo que la evidencia no separa.
- El título de LAZ-0007 («si el consentimiento llega tarde…») y la frase «Código medido: 2cec4b8» (`prueba-real.txt:11`) no cubren §3, primer intento.

Arreglo: anotar en LAZ-0007 el binario con pings y la falta del cifrado como hipótesis competidoras, y bajar «Lo aprendido» a hipótesis.

**N4 · menor · «No recorren el camino que se midió» es inexacto para el mínimo de SDP.**
- El chequeo `maximo < 7` está en `Trozo` (`ServidorSdp.cs:117`). `Trozo` atiende ServiceAttribute y ServiceSearchAttribute, y este último es justamente el que usó Android. Por eso el código nuevo sí está en el camino medido, aunque sin efecto con máximos grandes.
- `prueba-real.txt:13-14` dice además «en ServiceSearch/ServiceAttribute», y ServiceSearch no pasa por `Trozo`.
- Hay que corregir la frase en `parcela.md:95` y en la evidencia.

### Lo que revisé y está bien

- **Mesa:** el contrato, los cuatro informes y el jurado de cinco están completos. Los votos individuales coinciden con el dictamen (P1=A y P2=A en los cinco). El texto para ADR-0004 respeta `gobierno: humano` (`dictamen.md:44`, `:67`), y h1 va a la Review atado a LAZ-0007, que propone la medida endurecida de CE-2.
- **LAZ-0006 y LAZ-0007:** están `abierto`, con forma de plantilla, y cada uno trata el problema que nombra su fila en `pendientes:`.
- **L2CAP:** el rechazo con 0x0003 lleva sólo los octetos de tipo, que es lo que pide el Core; se aceptan los tipos conocidos 0x02-0x07, y con la falla el canal no queda configurado.
- **SDP:** el mínimo de MaximumAttributeByteCount da 0x0003 (sintaxis inválida), y TotalServiceRecordCount ya no supera MaximumServiceRecordCount.
- **Pings:** la prueba `ElTransporteNoMandaPingsDeHttp2` no es tautológica en lo que importa. Fija la configuración de la fábrica que usa el transporte (`TransporteNetsim.cs:26`), y M11 la pone en rojo. El valor explícito coincide con el valor por defecto de .NET, y la evidencia lo dice.
- **Mutaciones:** de M7 a M11, cada una tiene su prueba en rojo, y M1 se repitió como control.
- **Rojo antes que el verde:** se mantiene (27867a9 → 2cec4b8). Las cinco pruebas posteriores están declaradas como posteriores y no son `prueba:` de ningún REQ.
- **Exposición:** los archivos nuevos y cambiados no traen rutas de usuario, nombres de equipo ni IPs fuera de loopback. Las rutas son `/w/` o del contenedor. Las direcciones son sintéticas, la de Android va enmascarada y el puerto gRPC sale como `<puerto gRPC>`.
