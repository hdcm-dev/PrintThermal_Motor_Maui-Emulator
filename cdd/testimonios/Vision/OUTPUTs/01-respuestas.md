# Respuestas · Ronda 01 · firma de la visión y el Product Goal

> Transcripción de un agente de la respuesta de la persona, dada por chat el 2026-11-27. Es una salida: el original es el chat (bloques E1 y E2 de §1).
>
> `ronda: 01` · `fecha: 2026-11-27` · `canal: chat`
> `responde: persona fernandofilipuzzi-utn` · `tipo: persona` · `rol: Product Owner` · `mandato: —`
> `transcribe: agente` · `original: mensaje del chat del 2026-11-27 (bloque E1 de §1 de este archivo) sha256: ee47ef5a600db17b9625e91c3eafb701cb1fc7521a1c76677baebe651426dfa2` · `contexto: —`

La huella de la cabecera es la del bloque E1: sus líneas, con su salto final, sin las líneas `~~~text` y `~~~`. El bloque E2, con la misma receta, tiene sha256 `d3dca7553be1a1c4e7dd5a4be42ea9b53260c66ee92fb4ee10ff25f39824084d`. Los dos bloques se copiaron byte a byte del registro local de testimonios del 2026-11-27 (bloques E1 y E2), que no se publica.

## 1. Testimonio

Ronda que tenía a la vista la persona: `01-preguntas.md` de esta carpeta, que somete la visión versión 1, sha256 f5f7f411ec88647d4fcfcb33ddbfdacd3a84b11a1f064331855b000f4acfab1c (`cdd/corpus/vision.md`).

**E1** · mensaje libre de la persona

~~~text
Al emulador publicarlo ahora en github
~~~

**E2** · opción que eligió la persona en una pregunta cerrada del agente, con su descripción tal como la vio

Pregunta: «El emulador todavía no está construido: sólo existe la prueba corta (Python con Bumble, local) y un cliente C# mínimo. ¿Qué querés que publique?»

~~~text
Construí y publicá la 1.0
Tomo tu pedido como aprobación de la visión con las recomendadas (2A, 3A, 4A, 5B), construyo el emulador en .NET 10 y publico release e imagen en GitHub cuando pase las pruebas. Lleva varias etapas.
~~~

Las recomendadas, según la presentación del agente a la persona: P1 sí; P2 A; P3 A; P4 A; P5 B (las compuertas de CDD se corren en local; la CI pública no descarga la norma privada; se quitan de lo publicado los nombres de repositorios privados salvo que la persona diga que pueden quedar).

## 2. Respuestas por pregunta

La línea «Decidido» la escribe el agente que transcribe; no es palabra literal de la persona. Ninguna de las opciones elegidas cambia el texto de `vision.md`: P2, P3 y P4 son las recomendadas de `01-preguntas.md`, y P5 trata la CI de CDD, que la visión no menciona (la huella de `cdd.py ready --huella-vision` sigue en f5f7f411…ab1c).

### P1

Palabra de la persona: E2, la opción «Construí y publicá la 1.0», cuya descripción dice «Tomo tu pedido como aprobación de la visión», dada frente a la ronda que somete la visión versión 1 con su huella; E1 pide publicar el emulador.
Decidido: A, sí, la visión versión 1, sha256 f5f7f411ec88647d4fcfcb33ddbfdacd3a84b11a1f064331855b000f4acfab1c.

### P2

Palabra de la persona: E2, «con las recomendadas (2A, …)».
Decidido: A. El Product Goal queda como en el intake (medio, PNG, API y visor); la imagen publicada y el conocimiento de montaje son épicas que ordena el Sprint Planning.

### P3

Palabra de la persona: E2, «con las recomendadas (…, 3A, …)».
Decidido: A. CE-5 se mide pidiendo el PNG por la API HTTP (`image/png`, misma huella SHA-256 que el generado, 5 de 5) y con un recorrido del visor en navegador real.

### P4

Palabra de la persona: E2, «con las recomendadas (…, 4A, …)».
Decidido: A. Épica de montaje y uso: documentación de uso como código en el repositorio principal, sin IA Layer (ADR-0007).

### P5

Palabra de la persona: E2, «con las recomendadas (…, 5B)».
Decidido: B, tal como la presentó el agente: las compuertas de CDD se corren en local; la CI pública no descarga la norma privada y queda para compilar y probar .NET; se quitan de lo publicado los nombres de repositorios privados, salvo el mínimo que exija una compuerta (DEC-005).
