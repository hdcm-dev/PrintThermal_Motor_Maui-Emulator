---
cdd_version: 0.15.8
cdd_commit: b609a2a
producto: PTE
complementario: ../PrintThermal_Motor_Maui-Emulator.Documentacion
inputs: versionados
esqueleto: nuevo
conocimientos:
  - Logica-De-Decision
  - Cadena-De-Consecuencias
  - Estilo-Web
consulta:
  - DocsAsCode-Designing-Specification@IA.Knowledge.Base 1.5 fe51898
estilo: Estilo-Web
ajustes: []
estructura:
  - cdd/cdd.lock.md
  - AGENTS.md
  - ENTREGA.md
  - '*.sln'
  - '*.slnx'
  - '**/*.csproj'
  - Directory.Build.props
  - README.md
  - LICENSE
  - .gitignore
  - .github/workflows/cdd.yml
  - cdd/testimonios/**
requiere_maqueta: false
---
# Lock de la norma CDD

Fija la versión de la norma con la que se gobierna este producto. No contiene reglas.

- `cdd_commit`: `git -C <norma CDD> rev-parse --short HEAD` medido el 2026-11-27. La norma CDD es un repositorio privado.
- `consulta:`: commit de la base de conocimientos privada medido el 2026-11-27 (`git rev-parse --short HEAD`, en `origin/main`); la decisión está en `cdd/corpus/decisiones/ADR-0007-Documentacion-Como-Codigo-Para-Agentes.md`. Es el único lugar publicado que nombra la base: la compuerta `ancla` exige la forma literal `Alias@<nombre de la base> <versión> <commit>` en cada entrada de `consulta:` (DEC-006).
- `estilo:` y `requiere_maqueta:`: `cdd/corpus/decisiones/DEC.md`.
