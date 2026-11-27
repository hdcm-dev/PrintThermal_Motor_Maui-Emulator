# Decisiones del agente (DEC-NNN)

> Decisiones de producto fuera de una parcela (las de una parcela van en su campo `decisiones:`). Una fila por decisión; nunca se borran. Cualquier parcela puede agregar filas: la compuerta no le pide dueño.

| DEC | Fecha | Parcela | Decisión | Clase |
|---|---|---|---|---|
| DEC-001 | 2026-11-27 | — | TS-2 y TS-3 («diferida: siembra», intake §11) reciben destino en ADR-0004, `propuesto`. Este arranque no convocó la mesa: las dos dependen del resultado juzgado de la prueba corta (PA-4, «diferida: prueba corta»). La mesa se convoca cuando ese resultado esté publicado, antes de la primera parcela de la épica del medio emulado | K0 |
| DEC-002 | 2026-11-27 | — | El lock declara `estilo: Estilo-Web` porque el producto tiene interfaz web (visor Blazor, ADR-0002); la plantilla del lock pide el alias de estilo «si hay web o maqueta» | K1 |
| DEC-003 | 2026-11-27 | — | `requiere_maqueta: false`: el intake no pide maqueta y el visor es «sencillo solo un visor sin usuarios» (`Idea-Inicial.md` §7 A3); los criterios de interfaz del visor se juzgan en navegador real (`Rules-Decisiones.md` §7) | K1 |
| DEC-004 | 2026-11-27 | — | CE-5 tenía «medida por fijar en la siembra» (intake §9): la visión 1 propone la medida y la persona la confirma en la pregunta P3 de su ronda K3 | K0 |
| DEC-005 | 2026-11-27 | — | La CI de CDD (`.github/workflows/cdd.yml`) es la plantilla de IA.CDD sin cambios, en `ubuntu-latest` (runners de GitHub, Q2:18). Trae la norma desde `vars.CDD_REPO_URL` con el secreto `CDD_READ_TOKEN` si la norma es privada; sin esa variable y ese secreto, los trabajos fallan al clonar. Se pregunta a la persona (P5) | K0 |
