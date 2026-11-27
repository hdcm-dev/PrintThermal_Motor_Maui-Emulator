# Entrega de los pull requests

## Índice

1. [Delegación del merge](#1-delegación-del-merge)
2. [Reservas](#2-reservas)

## 1. Delegación del merge

- **Quién decidió:** el Product Owner (`fernandofilipuzzi-utn`), 2026-10-25, como regla de todos sus repositorios: los pull requests los conforma el agente con el conocimiento de conformación automática del pull request (base de conocimientos privada).
- **Qué cubre:** abrir el PR de la unidad, publicar el informe con el enlace, fusionar con commit de merge atado a la cabeza revisada cuando la guarda pasa, borrar sólo la rama del propio PR y dejar `main` al día.
- **Guarda:** PR abierto con la cabeza revisada; `mergeable` y estado `clean`; al menos una corrida de controles para esa cabeza y todas en éxito; sin reserva. Se relee justo antes de fusionar.
- **Techo de espera:** 1800 s. **Intervalo de relectura:** 30 s.
- **Asentada:** 2026-11-27, en la primera unidad con CI (PAR-PTE-01).

## 2. Reservas

El merge vuelve a la persona si hay una decisión que no le corresponde al agente, un efecto que no se pudo medir, un cambio irreversible no verificado, una guarda que no pasa o un cuestionamiento particular de la persona sobre la unidad.
