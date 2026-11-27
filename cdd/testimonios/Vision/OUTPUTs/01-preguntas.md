# Ronda 01 · firma de la visión y el Product Goal

> `fecha: 2026-11-27` · `persona: fernandofilipuzzi-utn` · `entrevistador: agente [cdd-arranque, subagente de un flujo]` · `ronda: 01` · `clase: K3`
>
> La escribe el agente: es una salida. Si la persona deposita su respuesta, va literal y fechada en `../INPUTs/01-respuestas.md`; si un agente la transcribe del chat, en `01-respuestas.md` de esta carpeta, con «transcribe: agente», el canal y el original con su sha256 (plantilla `respuestas.md` de IA.CDD). Sin plazo ni default.

## Índice

1. [Objetivo de la ronda](#1-objetivo-de-la-ronda)
2. [Preguntas](#2-preguntas)
3. [Lo que preguntó la persona](#3-lo-que-preguntó-la-persona)

## 1. Objetivo de la ronda

Firmar la visión y el Product Goal (K3 del arranque). El texto que se firma es el cerrado «visión versión 1 sha256 f5f7f411ec88647d4fcfcb33ddbfdacd3a84b11a1f064331855b000f4acfab1c» (`cdd/corpus/vision.md`, huella de `cdd.py ready --huella-vision`). Sale del intake 1.2 que firmaste y del pedido 8. Las recomendaciones de P2 a P5 dejan el texto como está; otra opción cambia la visión, y la versión 2 vuelve a vos para un sí explícito.

## 2. Preguntas

### P1 · ¿Aprobás la visión versión 1?
Contexto: sha256 f5f7f411ec88647d4fcfcb33ddbfdacd3a84b11a1f064331855b000f4acfab1c. Problema y visión copian el intake 1.2 (§2 a §4); el Product Goal es su §5 con los criterios CE-1 a CE-5; suma la documentación de uso para agentes (pedido 8) y cinco épicas. Opciones: A, sí, la versión 1; B, no, con los cambios que pidas (vuelve una versión 2). Recomendación: A: no agrega nada que no hayas pedido o firmado, salvo la medida de CE-5 (P3) y la forma de la documentación (P4), que podés cambiar en esas preguntas.

### P2 · ¿Hasta dónde llega el Product Goal?
Contexto: el intake 1.2 dice que el objetivo de esta etapa es verificar con la prueba corta que se puede descubrir, emparejar e imprimir y, si pasa, imprimir un ticket y obtener su imagen (§5). Dijiste «Vos podes publicar el contenedor listo para usar». Opciones: A, el Goal queda como en el intake (medio, PNG, API y visor) y la imagen publicada y el conocimiento de montaje son las épicas siguientes, que ordena el Sprint Planning; se pierde que la imagen lista para usar sea parte de la meta de la etapa. B, sumar al Goal la imagen publicada en `ghcr.io` y el conocimiento de montaje; se pierde un Goal corto que se cierra en cuanto el medio y la imagen del ticket funcionan, y el texto pasa a versión 2. Las dos son reversibles. Recomendación: A: es lo firmado, y la publicación no se posterga: ya es épica de esta visión.

### P3 · ¿Cómo se mide CE-5 (acceso a la imagen)?
Contexto: el intake dejó la medida «por fijar en la siembra» (§9). Opciones: A, el PNG de un ticket se pide por la API HTTP y llega como `image/png` con la misma huella SHA-256 que el generado, 5 de 5, y el visor lo muestra en un recorrido en navegador real (con capturas y consola); B, sólo la API, 5 de 5, sin recorrido del visor; se pierde la prueba de que el visor funciona. C, fijarla en la primera parcela de acceso; se pierde un Goal medible desde hoy. Recomendación: A: mide lo que pediste en A2 y A3 del pedido 7 y es la forma en que la norma juzga una interfaz.

### P4 · ¿Cómo entra la documentación como código del pedido 8?
Contexto: «Si querés incluirle un docascode para que los agentes consulten su uso … sin integración con ia layer». Hay un conocimiento de diseño documental (DocsAsCode-Designing-Specification 1.5) que se rotula a sí mismo «borrador ensayado, no probado». Opciones: A, una épica de montaje y uso: documentación de uso (montaje, API, visor) versionada con el código en el repositorio principal, diseñada con ese conocimiento como guía, sin IA Layer (ADR-0007); B, sólo `README.md` y la página Scalar; se pierde un lugar estable y verificado por la CI para que los agentes consulten. C, dejarla para la segunda etapa; se pierde que el primer release ya traiga cómo usarlo. Recomendación: A: es lo que pediste y es reversible (el conocimiento se retira con un ADR).

### P5 · ¿Cómo accede la CI pública a la norma privada?
Contexto: la CI de CDD (`.github/workflows/cdd.yml`) corre sólo en runners de GitHub, como elegiste (Q2:18), y clona la norma desde la variable `CDD_REPO_URL`; como la norma es privada, necesita además el secreto `CDD_READ_TOKEN` (token fino, sólo lectura de contenido sobre la norma). Sin los dos, los trabajos de la CI fallan al clonar desde este push. No puedo crear ese token: es una credencial tuya. Opciones: A, creás la variable y el secreto en el repositorio; se pierde que el repositorio público no guarde ningún secreto. B, la CI de CDD queda sólo manual hasta decidir, y las compuertas se corren en local; se pierde el control en cada PR. C, publicar la norma (decisión aparte y mayor). Recomendación: A: el token es de sólo lectura y es la vía que prevé la plantilla de la norma.

## 3. Lo que preguntó la persona

En la ronda 02 preguntaste dónde queda publicada la imagen, si hacía falta un token de Docker Hub y si tenía costo (C1, C2 y C4 de `PROMPTs/00-Fase-Previa/01-Relevamiento/OUTPUTs/02-respuestas.md` del repositorio de documentación). Respuesta, con la documentación oficial de GitHub abierta el 2026-11-27 (ADR-0006):

- Dónde: en el registro de contenedores de GitHub, `ghcr.io`, junto al repositorio. «`registry`: Must be set to `ghcr.io`.» (https://docs.github.com/en/actions/tutorials/publish-packages/publish-docker-images).
- Token: no hace falta Docker Hub; el workflow publica con el token automático del propio workflow: «You can use the automatically-generated `GITHUB_TOKEN` secret for the password.» (misma página).
- Costo: «GitHub Packages usage is free for public packages. In addition, data transferred in from any source is free.» (https://docs.github.com/en/billing/concepts/product-billing/github-packages).
- Después la sesión que monta la scene descarga la imagen de versión fija desde `ghcr.io` (ADR-0006, ADR-0008).
