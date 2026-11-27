# Auditoría del cierre de PAR-PTE-07 (2026-11-27)

auditor: agente refutador Claude (mismo modelo que la ejecución) en otro contexto, sin la conversación de la ejecución, sólo lectura — garantía reducida

## Índice

1. [Ronda 1 (sobre a3e0901)](#ronda-1-sobre-a3e0901)
2. [Respuesta a la ronda 1](#respuesta-a-la-ronda-1)

## Ronda 1 (sobre a3e0901)

Encontré un problema bloqueante, siete importantes y siete menores. Los números de la evidencia cuadran (136 de 136 en el `.trx`, 4 mutantes en rojo, 2 de 2 y 5 de 5 en el extremo a extremo, 30 px y 16 mediciones del visor), pero el cierre deja fuera efectos que no se midieron y la evidencia tiene una afirmación falsa.

1. **Bloqueante.** La reconexión breve viene activada por omisión y se dispara en todo vínculo desde Ajustes que termina sin cifrar, que con Ajustes es siempre. También se dispara cuando Android no se equivoca de equipo, por ejemplo en un emulador sin otros vínculos (el primer uso típico) o cuando el manejador no es 0. Ese caso no se midió: toda la evidencia con la corrección (`…:31`, `…:33`) se tomó con otros «58HB6» vinculados y el Bluetooth recién reiniciado (`extremo-a-extremo.md:15`). Además, en esas corridas no hay registro de Android que muestre que el error de atribución ocurrió de verdad. `diagnostico.md:101` sólo muestra el corte de la conexión breve, así que «funcionó por la corrección» no se separa de «no se reprodujo la condición». No está en `pendientes:` ni en NO VERIFICADO (`parcela.md:26-28`, `:99`), y `ENTREGA.md` §2 devuelve el merge a la persona cuando un efecto no se midió.
   - **Lo corrige:** una corrida del camino (a) con la corrección sin otros vínculos (y otra con manejador distinto de 0), más el registro VERBOSE del vínculo de `…:31`/`…:33`. Si no, declararlo NO VERIFICADO, abrir un LAZ y dejar la reserva de merge.

2. **Importante.** `extremo-a-extremo.md:110` dice que la opción de `MedioBluetooth.cs` está «cubierta por las pruebas». Es falso: ninguna prueba nombra `ReconexionTrasVinculoSinCifrar`, `OpcionesDelMedio` ni `MedioBluetooth`. `DocumentacionTests.cs:346-353` sólo comprueba que la propiedad exista.
   - Quitar `= true` en `MedioBluetooth.cs:22` deja la corrección apagada en la imagen y todo sigue en verde.
   - La imagen medida (`c50f02b`, igual a `028d4aa`) no tenía ese cableado, que entró en `2c5f3d4`.
   - El camino `=false` que citan `docs/diagnostico.md:57` y `ADR-0009:40` nunca se ejecutó.
   - `parcela.md:94` habla de «la imagen de la rama» sin aclarar nada de esto.
   - **Lo corrige:** una prueba de la configuración (clave ausente da `true`; `false` no manda 0x0405), o retirar la afirmación y marcarlo NO VERIFICADO.

3. **Importante.** Las pruebas negativas sólo esperan 300 ms (`SeguridadVinculoSinCifrarTests.cs:54`, `:67`, `:83`). Una implementación que reconecta después de **cualquier** cierre con 0,5 s de retardo pasa las tres pruebas, así que REQ-02 («sin conexiones propias») no queda verificado frente a implementaciones asíncronas. La mutación sólo probó mutantes síncronos (M2, M4).
   - **Lo corrige:** afirmar recién después de un evento posterior cuya respuesta se espera, o sumar un mutante con retardo.

4. **Importante.** Dos casos borde de `parcela.md:60` tienen código pero ni prueba ni mutante:
   - Android conecta durante la reconexión: `PerifericoBluetooth.cs:213`.
   - La conexión propia no arranca: `:275-279`.
   - Borrar la línea 213 o la 278 sobrevive a toda la batería.
   - La variante más probable de la carrera no está manejada. Si Android reutiliza el ACL breve para el RFCOMM de la app, no hay Connection Request. La impresora descarta su L2CAP (el manejador breve no está en `_conexiones`, `:44-50`) y lo corta con 0x13 (`:228`).
   - **Lo corrige:** pruebas para esos dos casos, y declarar NO VERIFICADO la reutilización del ACL.

5. **Importante.** El criterio (`REQ-HIS-Descubrimiento-Y-Emparejamiento.md:34`) y `docs/emparejar-e-imprimir.md:52` igualan Ajustes con «`createBond()` en una app», que fue justamente el origen de LAZ-0011 en PAR-PTE-05. Sólo se midió por la interfaz de Ajustes, con la app abierta mucho después.
   - La corrección actúa recién en Disconnection Complete (`:249-264`), y Android mantiene el ACL del vínculo unos 5 s (`diagnostico.md:59-60`: de 245.976 a 251.162).
   - Una app que conecta apenas recibe BOND_BONDED viaja por ese ACL con el equipo todavía «AUTHENTICATING», así que es probable que su primer intento venza igual.
   - **Lo corrige:** medirlo, o declararlo y acotar el texto de docs.

6. **Importante.** El criterio pide «5 de 5». La parcela lo lee como un vínculo y 5 impresiones sobre una sola conexión (`parcela.md:44`). El vínculo desde Ajustes con cifrado se ejercitó 2 veces, tantas como los fallos de PAR-PTE-05, y las 5 impresiones no agregan nada sobre el vínculo.
   - **Lo corrige:** aclarar en el criterio que «5 de 5» son tickets, o medir 5 vínculos.

7. **Importante.** El diagnóstico afirma más de lo que sostiene su evidencia.
   - **Lo que sí sostiene:** el error de atribución está observado directamente (`btm_sec_auth_complete` con `…:01` en el vínculo `…:23`, y el corrimiento a `…:00` cuando ese equipo estaba en la configuración). La fila 2 de §5 muestra el mecanismo de la mitigación.
   - **Lo que no:**
     - `diagnostico.md:93` dice que vincular conectando funcionó en PAR-PTE-05 «porque» no era la primera conexión. No se cita el manejador de PAR-PTE-05, y ese contrafáctico es justo lo que LAZ-0016 deja sin medir.
     - Faltan los controles negativos que la hipótesis predice: Ajustes sin corrección con manejador distinto de 0, o sin otros vínculos, debería cifrar.
     - Las hipótesis competidoras de LAZ-0011 §1 (temporizador con el toque a los 31 s, binario con pings de HTTP/2) no se descartan, y `docs/emparejar` §2 borró el dato de los 31 s.

8. **Importante.** `ADR-0009:31` descarta la opción C con un comportamiento de Android («`btm_sec_encrypt_change` vuelve sin reenviar») que no aparece en ninguna evidencia. La única mención está en el propio ADR. Es una aprobación K3 sobre una premisa no medida.
   - Además, el ADR ya figuraba «aceptado» con el texto de aprobación en `17d85dd` (16:38), antes de que D5 se asentara en `a3e0901` (16:48). La aprobación remite a un registro privado que no se puede verificar desde este repositorio.

9. **Menor.** `parcela.md:28` registra la falta de medición con teléfono real en LAZ-0002, que trata de respuestas y demoras de ESC/POS (`LAZ-0002:13`), y LAZ-0002 no se modificó. Además, medir «la reconexión breve con la 58HB6 física» no tiene sentido: la impresora física no la hace.

10. **Menor.** El commit `2c5f3d4` dice que LAZ-0011 y LAZ-0015 «la nombran», pero `17d85dd` vuelve `parcela:` a vacío. Los dos quedan en `especificado`, y el CHANGELOG sólo menciona LAZ-0016. O falta la traza al LAZ, o el mensaje del commit es engañoso.

11. **Menor.** Trazabilidad de commits.
    - La evidencia cita commits que no están en la rama ni en `origin` (`rojo.txt:1` cita `6f79bd3`; `extremo-a-extremo.md:13` cita `c50f02b` y `86b7de9`). Comprobé que su contenido equivale a `3d1aa17` y a `028d4aa`; conviene citar esos.
    - `e3f5b21`, el commit «de la corrección», compara `c.Par == vinculado` por referencia (la clase de la dirección no define `operator==`). Ahí la corrección no hace nada y REQ-01 falla, lo que molesta para un bisect.
    - `2c5f3d4` mete código y `docs/` bajo un mensaje de «parcela».

12. **Menor.** En docs hay afirmaciones sin evidencia.
    - `docs/emparejar-e-imprimir.md:32`: «Sin confirmación, la conexión venció…». No hay ninguna corrida sin confirmación; el `read failed` del diagnóstico es del caso «AUTHENTICATING», con la confirmación dada.
    - `docs/diagnostico.md:58` dice «cifró e imprimió», pero el diagnóstico (§5) dice «cifró y abrió RFCOMM».
    - `docs/diagnostico.md:57`: «si la confirmación tardó, la conexión pudo vencer» no está medido.

13. **Menor.** `docs/montaje.md:99` dice «cada comando y evento», pero la traza omite Number Of Completed Packets (`PerifericoBluetooth.cs:86`). Tampoco advierte que escribe las claves de enlace en claro en el registro (evento 0x18 y comando 0x040B); la evidencia tuvo que taparlas a mano (`diagnostico.md:31`).

14. **Menor.** `REQ-HIS-Visor-Web.md:30` y `docs/visor-y-scalar.md:23` dicen «a escala» sin excepción. Con la hoja nueva, por debajo de unos 213 px el ticket se achica (173 px a 205 en `medicion.json`); D3 lo dice, pero el corpus y docs no. Además, REQ-03 se midió en Chrome de escritorio con emulación de ancho, no en el navegador del emulador de Android donde se vio LAZ-0015, y eso no está declarado como límite.

15. **Menor.** Tres detalles del código:
    - El caso de la conexión breve (`:222`) no comprueba el tipo de enlace (`p[9]`).
    - Si el Disconnect de la breve falla, el ACL queda vivo y no figura en `_conexiones`.
    - Un Disconnection Complete con estado distinto de 0 ahora además dispara un Create Connection con el ACL todavía vivo.

    Lo que descarté:
    - **Reintentos infinitos:** no hay; la conexión breve nunca entra en `_conexiones` ni en `_vinculadas`, así que su cierre no vuelve a disparar.
    - **Estado colgado:** el estado no sobrevive a una reconexión con netsim, porque `ServicioPeriferico` crea un periférico nuevo por sesión.
    - **`Clear()` en un Command Status correcto:** el `Clear()` de `:278` no corre con estado 0, porque `HostHci.cs:119-124` sólo reenvía los Command Status con error.
    - **Efecto sobre el camino que ya funcionaba:** vincular conectando con cifrado no reconecta (camino (b) del extremo a extremo y REQ-02).

**Repositorio público:** no encontré hosts, IPs privadas, rutas personales, credenciales ni datos personales. El `.trx` trae el nombre efímero del contenedor (`18586eef1de3`) y rutas `/w/…` del contenedor, y las capturas sólo muestran el ticket de ejemplo. Un detalle: la máscara del teléfono no es coherente. `BBBBBB000002` equivale a `02:00:00:BB:BB:BB`, mientras el texto usa `XX:XX:XX:XX:BB:BB`; deja ver el prefijo sintético de netsim, que no es un dato personal.

Veredicto: cierre refutado

## Respuesta a la ronda 1

| Hallazgo | Respuesta |
|---|---|
| 1 Efecto sin medir sin la condición | Medido con la imagen final (extremo a extremo §4): con manejador distinto de 0 y con la corrección apagada (S1), y sin otros equipos vinculados (S3), Android cifra en la conexión del vínculo y la reconexión breve no se dispara; con la corrección prendida y manejador distinto de 0 (S2), igual. La condición quedó registrada (VERBOSE) en las tres corridas con corrección del camino (a): `…:31`, `…:33` y `…:41` (diagnóstico §5) |
| 2 Opción de `MedioBluetooth.cs` sin prueba | `MedioBluetooth.OpcionesDelPeriferico` y `OpcionesDelMedioTests` (por omisión prendida; `false` y `TrazaHci` llegan); `ConLaOpcionApagadaNoHayReconexion`; el `false` se ejecutó de verdad en S1. La imagen final ya trae el cableado. El texto falso se corrigió |
| 3 Esperas de 300 ms | Las pruebas negativas esperan un punto de sincronía (un evento que el host contesta) y 1,5 s; mutante M5 (reacción diferida 500 ms) en rojo |
| 4 Casos borde sin prueba | `SiAndroidSeAdelantaLaConexionNoSeCorta` (M6), `SiLaReconexionNoArrancaLaConexionSiguienteNoSeCorta` (M7), `UnCorteFallidoNoProvocaReconexion` (M8). Reutilizar la conexión breve: la impresora la corta apenas se completa; NO VERIFICADO en el cierre |
| 5 `createBond()` con conexión inmediata | El criterio y docs ya no igualan Ajustes con «`createBond()` en una app»; el caso queda en LAZ-0016 |
| 6 «5 de 5» | El criterio dice «cinco tickets seguidos por esa conexión»; los vínculos desde Ajustes con la condición, 3 de 3, más los controles |
| 7 Diagnóstico más fuerte que la evidencia | Controles negativos medidos (S1, S3). El «porque» de PAR-PTE-05 se cambió por «coherente con», con LAZ-0016. Las hipótesis del toque tardío y de los pings no hacen falta: la falla se reprodujo con el toque a los 2 s y el binario sin pings |
| 8 Opción C sin evidencia | ADR-0009 la marca «según el código fuente de `main` (no medido)». Orden de commits: el texto de la ADR y su K3 (D5) entran en el mismo PR; la aprobación en rol remite al testimonio F1, privado por regla de los repositorios públicos |
| 9 LAZ-0002 | Quitado de `pendientes:`; el teléfono real va a LAZ-0016. La 58HB6 física no hace la reconexión: ADR-0009 §4 lo dice |
| 10 Mensaje de `2c5f3d4` | El mensaje quedó así (la rama no se reescribe); los LAZ no se tocan en este PR |
| 11 Commits citados | La evidencia cita `3d1aa17` (rojo) y `028d4aa` (imagen de la primera corrida), con el SHA previo al rebase; la imagen final, `1c6e700` |
| 12 Afirmaciones de docs | Sin confirmación: medido (2 de 2, `read failed…` a los 35 s); «cifró y abrió el canal RFCOMM»; el vencimiento del diálogo, con ese dato |
| 13 Traza | La traza tapa las claves (`<clave>`, prueba `LaTrazaHciNoMuestraLaClaveDeEnlace`, mutante M9); `docs/montaje.md` dice qué omite |
| 14 «A escala» | REQ-HIS-Visor-Web y `docs/visor-y-scalar.md` dicen que por debajo de unos 213 px el ticket se achica; Chrome de escritorio con emulación, declarado en el cierre |
| 15 Detalles del código | La conexión breve exige enlace ACL (`p[9]`); un Disconnection Complete con error ya no dispara la reconexión (M8). Si el Disconnect de la breve fallara, el ACL quedaría vivo sin tráfico: no se cubre |
