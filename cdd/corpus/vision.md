---
id: VIS
version: 1
gobierno: humano
tipo: explicacion
aprobado: "Fernando 2026-11-27 versión: 1 sha256: f5f7f411ec88647d4fcfcb33ddbfdacd3a84b11a1f064331855b000f4acfab1c testimonio: cdd/testimonios/Vision/OUTPUTs/01-respuestas.md"
procedencia: humano
rutas: []
---
# Visión del producto

## Índice

1. [Problema](#1-problema)
2. [Visión](#2-visión)
3. [Product Goal](#3-product-goal)
4. [Épicas](#4-épicas)
5. [Fuentes](#5-fuentes)

## 1. Problema

La persona no dispone de un emulador de la impresora térmica 58HB6 para sus pruebas (intake §2). Tiene que reproducir el caso del equipo con Android 17 (intake §2). Lo necesita quien prueba la impresión en el emulador de Android, en local o en pruebas de extremo a extremo (intake §3).

## 2. Visión

Una impresora 58HB6 emulada que el emulador de Android descubre en «Dispositivos cercanos» de Bluetooth y empareja, y que entrega cada ticket como imagen PNG (intake §4). La imagen se obtiene por una API HTTP de tickets con OpenAPI y página Scalar, y se ve en un visor web sin usuarios integrado al mismo servicio, que llama a la API por HTTP (intake §4). Se publica como release de GitHub con imagen de contenedor de versión fija, deja un conocimiento de montaje para los otros agentes (intake §4) y una documentación de uso que los agentes consultan (pedido 8, `Idea-Inicial.md` §8; ADR-0007).

Fuera de alcance: NFC, con la puerta abierta, e iOS (intake §6). Fuera de la primera etapa: inyección de fallos y calibración con la impresora física (intake §6).

## 3. Product Goal

Verificar con la prueba corta S0-S8 que el medio emulado permite descubrir, emparejar e imprimir y, si pasa, imprimir un ticket y obtener su imagen (intake §5). Se da por alcanzado cuando se cumplen los criterios del intake §9:

| ID | Criterio | Medida |
|---|---|---|
| CE-1 | Descubrimiento desde Android | aparece en 30 s o menos, 3 de 3 (intake §9) |
| CE-2 | Emparejamiento | la impresora figura vinculada en Android (intake §9) |
| CE-3 | Integridad de lo impreso | huella recibida igual a la enviada, 5 de 5 (intake §9) |
| CE-4 | Salida | un PNG por ticket (intake §9) |
| CE-5 | Acceso | propuesta (DEC-004): el PNG de un ticket se pide por la API HTTP y llega como `image/png` con la misma huella SHA-256 que el generado, 5 de 5; y el visor lo muestra en un recorrido en navegador real (intake §9 dejó la medida para la siembra) |

## 4. Épicas

IDs propuestos; los archivos se siembran después de la firma de esta visión.

| Épica | Qué cubre | Criterios | Decisiones |
|---|---|---|---|
| `REQ-EPI-Medio-Emulado` | Descubrir, emparejar e imprimir por Bluetooth Classic desde el emulador de Android | CE-1, CE-2, CE-3 | ADR-0004 |
| `REQ-EPI-Ticket-Png` | Interpretar lo recibido y producir un PNG por ticket | CE-4 | ADR-0005 |
| `REQ-EPI-Acceso` | API HTTP de tickets con OpenAPI y Scalar, y visor Blazor que la consume | CE-5 | ADR-0001, ADR-0002, ADR-0003 |
| `REQ-EPI-Distribucion` | Release con imagen de contenedor de versión fija en `ghcr.io`, compilada en runners de GitHub | — | ADR-0006 |
| `REQ-EPI-Montaje-Y-Uso` | Conocimiento de montaje para los otros agentes y documentación de uso como código, sin integración con IA Layer | — | ADR-0007, ADR-0008 |

El Product Goal se alcanza con las tres primeras; las dos últimas completan el producto imaginado (intake §4) y su orden lo fija el Sprint Planning.

## 5. Fuentes

Raíz: repositorio `PrintThermal_Motor_Maui-Emulator.Documentacion`.

- `PROMPTs/intake.md`, versión 1.2, firmado el 2026-11-27 (sha256 `86b40e98947511c1d7c4c260f7b944b104e3f85748ddcd46e52ef2e0ca80e442`).
- `PROMPTs/SDD/01-Analisis-Idea-Inicial/Idea-Inicial.md` §7 (pedido 7) y §8 (pedido 8), 2026-11-27.
- Decisiones: `cdd/corpus/decisiones/` de este repositorio.
