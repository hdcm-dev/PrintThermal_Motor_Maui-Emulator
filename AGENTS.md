# AGENTS

Este producto se gobierna con la **norma CDD** (Desarrollo Guiado por Corpus), que vive en un repositorio privado.

- Versión de la norma y conocimientos acoplados: `cdd/cdd.lock.md`.
- Corpus: `cdd/corpus/`. Parcelas: `cdd/parcelas/`. Sprints: `cdd/sprints/`.
- Documentación y prompts del PO: `PrintThermal_Motor_Maui-Emulator.Documentacion` (repositorio hermano).
- Solución: `PrintThermal.Emulator` (.NET 10). Prefijo de las parcelas: `PTE`.
- Entrega de los pull requests: `ENTREGA.md`.

Compuertas de CDD: se corren en local, con el `cdd.py` de la norma en el commit del lock (`cdd_commit`); su salida queda en la bitácora del repositorio de documentación. La CI pública (`.github/workflows/cdd.yml`) no descarga la norma: compila y prueba la solución .NET (decisión P5 B de la visión, `cdd/corpus/decisiones/DEC.md` DEC-006).

Para trabajar una parcela, con la norma CDD a mano:

```text
Leer y ejecutar <norma CDD>/PROMPTS/PROMPT-Parcela-CDD.md en el repositorio: /PrintThermal_Motor_Maui-Emulator [parcela: PAR-PTE-NN]
```

Las reglas viven en la norma CDD, no en este archivo.
