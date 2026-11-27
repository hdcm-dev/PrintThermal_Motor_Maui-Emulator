# Auditoría del cierre de PAR-PTE-01

auditor: agente auditor Claude Opus (Claude Code sin interfaz, `claude -p`), contexto nuevo que no vio la ejecución, sólo lectura · garantía reducida: es el mismo modelo que ejecutó la parcela; no había otra familia de modelos disponible.

## Índice

1. [Informe del auditor](#1-informe-del-auditor)
2. [Qué se hizo con cada hallazgo](#2-qué-se-hizo-con-cada-hallazgo)

## 1. Informe del auditor

Texto tal como lo devolvió, sobre la cabeza anterior a las correcciones de §2:

> ## Auditoría de cierre de PAR-PTE-01
> 
> No logré refutar el cierre: las cuatro pruebas existen y la evidencia coincide con ellas. Quedan una prueba débil, tres afirmaciones de la prosa que no son ciertas y dos medidas que la prosa promete pero que todavía no están.
> 
> **1. Pruebas contra los REQ**
> - **menor · REQ-04 es la más débil.** Busca líneas sueltas de texto en `.github/workflows/cdd.yml` (`EsqueletoTests.cs:112-120`).
>   - No pide la línea `push:`. Si alguien borra `push:` de `cdd.yml:8`, `branches: [main]` queda bajo `pull_request` y la prueba sigue pasando, aunque ya no cumple «o se empuja a `main`».
>   - Tampoco detecta un `if: false`, un `continue-on-error: true` ni un `runs-on` que esté en otro trabajo.
> - **observación · REQ-01** (`:53-60`) sólo lee `Directory.Build.props` de la raíz. Un `Directory.Build.props` o `.targets` en `src/` podría cambiar el destino a otro framework sin que la prueba lo vea.
> - **observación · REQ-02** (`:80-88`) sólo revisa los `.csproj` que están en el `.slnx`. Un `PackageReference Version=` dentro de `Directory.Build.props` o `.targets` pasaría la prueba.
> - **observación · REQ-02 nunca se vio en rojo** (`rojo.txt:77`). La prosa lo declara, pero no hay ninguna demostración de que la prueba sea capaz de fallar.
> - **REQ-03** (`:98-99`) está bien.
> 
> **2. Evidencia de rojo y verde:** es coherente.
> - El rojo da 3 fallos y 1 aprobada. Los fallos apuntan a las líneas 47, 96 y 112, que son las del archivo actual.
> - El verde da 4 de 4 en `pruebas.txt:22-30`, y el `.trx` también (`total=4 passed=4`). Los nombres coinciden en los tres archivos.
> 
> **3. ## Cierre contra `pendientes: ninguno`**
> - **menor · Dos medidas que no existen.** `parcela.md:80` apunta a `evidencia/2026-11-27-auditoria.md`, y ese archivo no está en `evidencia/`. Además, `parcela.md:79` llama a la corrida de la CI del PR «segunda medida de REQ-04» y `:36` promete verde en `ubuntu-latest`, pero esa corrida no se hizo y no tiene evidencia. Es algo abierto bajo un rótulo. La guarda de `ENTREGA.md:61` lo cubre, pero no está declarado como pendiente.
> - **menor · Faltan decisiones en el resumen.** `parcela.md:80` dice «Decisiones: D1 a D4», pero el frontmatter tiene también D5 (`:27`).
> - **menor · Estados en contradicción.** La parcela dice `estado: en-curso` (`parcela.md:5`) y `SPR-01.md` ya la marca como «cerrada».
> 
> **4. README y AGENTS**
> - **menor · README describe funciones que no existen.**
>   - `README.md:3` dice «Entrega cada ticket como imagen PNG por una API HTTP (OpenAPI y Scalar) y un visor web», en presente. Antes decía «prevista» y «Todavía no hay código».
>   - La tabla de proyectos (`:11-14`) dice qué «hace» cada uno, pero los cinco están vacíos.
>   - Dice que Api es «Host ASP.NET Core», cuando hoy es una biblioteca sin `Program.cs` (D3).
> - AGENTS no afirma nada falso.
> 
> **5. Datos sensibles:** no encontré IPs, direcciones MAC, credenciales ni rutas `/home`.
> - Las rutas `/w/...` y el `computerName` `3ac3d9d81881` del `.trx` son del contenedor efímero, no de una máquina local.
> - **observación:** `ENTREGA.md:59` publica el usuario de GitHub `fernandofilipuzzi-utn`. Es un dato público, no una credencial.
> 
> **6. Versiones fijas**
> - Están fijos el SDK (`global.json`, `rollForward: disable`), los paquetes centrales y las acciones por SHA.
> - **observación · Los SHA de las acciones no los comprobé.** No me dieron permiso para consultar `gh api`, así que no sé si cada SHA corresponde a su etiqueta.
> - **observación · Quedan dos cosas sin fijar:**
>   - La imagen del runner usa `runs-on: ubuntu-latest`. El propio REQ lo pide así.
>   - Las dependencias transitivas no tienen lock: `CentralPackageTransitivePinningEnabled=false` y no hay `packages.lock.json`.
> 
> VEREDICTO: no refutado

## 2. Qué se hizo con cada hallazgo

- REQ-04 débil (menor): la prueba exige ahora `push:` con `branches: [main]` debajo, y rechaza `continue-on-error: true` e `if: false`. Medida: `2026-11-27-mutacion.txt` (sin `push:` la prueba falla).
- REQ-01 y REQ-02 con props anidados (observación): las dos pruebas rechazan `Directory.Build.props`, `Directory.Build.targets` o `Directory.Packages.props` dentro de `src/` o `tests/`, y REQ-02 rechaza un `PackageReference` en `Directory.Build.props`.
- REQ-02 nunca visto en rojo (observación): `2026-11-27-mutacion.txt` lo muestra fallando con `[2.9.3]`.
- Auditoría citada y ausente (menor): es este archivo.
- Corrida de la CI del PR (menor): no es evidencia de esta carpeta, porque se produce después del cierre; la mide la guarda del merge (G3 de `ENTREGA.md`) y queda en la bitácora del repositorio de documentación. `## Cierre` lo dice así.
- «D1 a D4» (menor): corregido a D1 a D5.
- Estado `en-curso` contra «cerrada» en el sprint (menor): la parcela pasa a `cerrada` en este commit.
- README en presente (menor): dice ahora «su meta es», «qué va a hacer» y que hoy hay sólo el esqueleto vacío; Api, «hoy, biblioteca vacía».
- Usuario de GitHub en `ENTREGA.md` (observación): es público y es quien decidió la delegación; queda.
- SHA de las acciones sin comprobar por el auditor (observación): los resolví en la API de GitHub el 2026-11-27 (`repos/<acción>/commits/<etiqueta>`): checkout v7.0.1 `3d3c42e5…`, setup-dotnet v6.1.0 `0f4a354d…`, upload-artifact v7.0.2 `cf430e03…`.
- `ubuntu-latest` y transitivas sin lock (observación): `ubuntu-latest` lo pide REQ-HIS-Compilacion-Verificada; fijar las transitivas no está en el alcance de esta parcela y no se declara como pendiente porque ningún requisito lo pide.
