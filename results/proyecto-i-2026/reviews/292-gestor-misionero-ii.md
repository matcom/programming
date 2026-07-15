# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #292
- **Repositorio:** https://github.com/MaykolCarreras/Maykol-A.-Carreras-Mart-n---C121---Proyecto-1er-Semestre-Alternativo
- **Estudiante:** Maykol A. Carreras Martín
- **Grupo:** C121
- **Descripción declarada:** Segunda versión de un gestor de eventos y recursos para una organización misionera.

---

## Nota metodológica importante

Este proyecto **no es una aplicación de consola**: es una GUI de escritorio construida con `customtkinter` (`main.py:2`, punto de entrada `main.py:861`). El traceback de la verificación automática (`ctk.CTk()` fallando en `main.py:8`) es un fallo de **entorno**, no de código: en CI no hay servidor gráfico. Confirmé esto arrancando la app bajo `xvfb-run`: llega hasta la inicialización de Tk y aborta con un error de threading de xcb (`append_pending_request: Assertion !xcb_xlib_unknown_seq_number failed`), que también es del entorno headless, no un bug del estudiante.

Por eso adapté la ejecución: el proyecto está **bien modularizado**, con la lógica de negocio separable de la GUI (`control_center.py`, `methods/event_manager.py`, `tools/format_check.py`, `dbases/handler.py`). Importé esos módulos directamente y ejecuté sus funciones con los datos reales del repo (`dbases/events.json`, `dbases/resources.json`, `dbases/id_count.json`), recorriendo flujos válidos e inválidos. Todo lo que reporto en la Dimensión 3 fue **ejecutado**, no leído.

## Dimensión 1 — Qué hace el programa

Gestor de eventos de una organización misionera con calendario mensual, validación de reglas de negocio y persistencia en JSON. Tres funcionalidades:

1. **Listar** eventos de un mes/año (`main.py:41` `paint` → `control_center.py:6` `get_list_month` → `event_manager.py:5` `index_month`). Los eventos se indexan por `(año, mes)`; un evento que abarca varios meses aparece en cada mes activo, y `paint` los separa en tres secciones: los que **inician** ese mes, los que **terminan** ese mes (`main.py:141`) y los que están **activos durante** el mes (`main.py:236`).
2. **Eliminar** un evento por botón (`main.py:340` → `control_center.py:130` `eliminar`), con confirmación `messagebox`.
3. **Añadir** un evento: valida fechas y duración por tipo (`control_center.py:16` `validar_fechas`), luego abre una ventana de selección de recursos (`main.py:541`), valida reglas de exclusión/inclusión (`control_center.py:60` `validar_reglas`) y disponibilidad de recursos en el intervalo (`control_center.py:57` `validar_existencia`), y si no hay hueco ofrece un **buscador de hueco** iterativo (`main.py:824` `loop_new_interval`).

## Dimensión 2 — Organización del código

**Fortaleza destacada.** La modularización es la mejor parte del proyecto y está bien pensada, con responsabilidades claras:

- `main.py` — solo GUI y "pintado" de datos.
- `control_center.py` — orquestación: coordina handler, manager y format_check.
- `methods/event_manager.py` — algoritmos de dominio (indexado por mes, detección de colisiones, suficiencia de recursos, aritmética de fechas).
- `tools/format_check.py` — validación de formato de fechas/horas, autocontenido.
- `dbases/handler.py` — I/O de JSON, la única capa que toca disco.

El estudiante explica en `report.md` que rehízo la v1 (monolítica) desde cero para lograr esto, y el resultado justifica la decisión. Los tipos de evento y sus límites de duración están centralizados en un diccionario legible (`control_center.py:36`).

**Debilidades menores.** Nombres poco descriptivos en variables locales de la GUI (`fasd`, `aux`, `asd`, `a`/`b`/`c` para frames — `main.py:48`, `event_manager.py:86`, `main.py:555`). El bloque de "pintado" de eventos está **triplicado** casi textualmente en `paint` (`main.py:80-129`, `main.py:178-227`, `main.py:272-321`): podría extraerse a un método `_pintar_evento(ev, row)`. Hay `return` muerto en `handler.py:8` (tras un `with`).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté la lógica de negocio con los datos reales del repo. Resultados concretos:

1. **Listar (`get_list_month(2026,5)`)** → devuelve 7 eventos correctamente indexados, con `f1`/`f2` convertidas a `datetime` y `duracion` calculada (p.ej. `a_20` Inspección, 22 días). ✔
2. **Mes sin eventos (`get_list_month(2030,1)`)** → devuelve `[]` sin excepción (gracias a `defaultdict`), y la GUI muestra "No hay eventos en este intervalo" (`main.py:330`). ✔
3. **Validación de fechas por tipo** — Inspección 2026-06-01→06-20 (19 días, dentro de 14-28): válida. ✔ Inspección 06-01→06-05 (4 días): rechaza con *"han de durar almenos 14 días"*. ✔
4. **Fechas basura** (`"abcd-ef-gh"`, hora `"xx"`) → devuelve mensajes de error por componente, sin `Traceback`. ✔
5. **f1 > f2** → *"La primera fecha es mayor que la segunda"*. ✔
6. **`validar_fecha` (batería)**: `2026-13-01`→mes fuera de rango; `2026-02-30`→"febrero solo posee 28 días"; `2028-02-29`→aceptada (bisiesto); `2025-06-01`→"El año debe estar entre 2026 y 9999"; `2026-6-1`→normaliza a `2026-06-01`; `""`→"Componente vacío". Todos correctos. ✔
7. **`validar_hora`**: `"9"`→`09:00:00`; `"25"`→"Hora fuera de rango"; `"09:70:00"`→"Minutos fuera de rango". ✔
8. **Reglas de negocio (`validar_reglas`)** — ejecuté 9 casos: Inspección con material→rechaza; Envío de fondos sin USD→rechaza; Biblias sin Tratados→rechaza; Marco Casanabes en Conferencia→rechaza; Proceso de Traducción con persona no calificada→rechaza; Tim Nayoils sin Waleska→rechaza. **Todas las restricciones del `report.md` se cumplen en ejecución.** ✔
9. **Disponibilidad de recursos (`validar_existencia`)** — Paul Stone (ocupado en `a_1` mayo-julio) pedido en junio 2026→`(1, "Paul Stone no está disponible")`; el mismo en enero 2028→`(0, proceder)`. Materiales: 95 Biblias en junio con `a_7` usando 20 (stock 100)→`(1, "No hay suficientes unidades")`. Detección de colisión y resta de stock **correctas**. ✔
10. **Round-trip de persistencia** — `refresh_db` añade un evento con id `a_22`, incrementa `id_count` a 23; `eliminar("a_22")` lo remueve. La base vuelve a 14 eventos. ✔
11. **Buscador de hueco (`get_newdates` paso=31)** — `2026-05-01→05-20` se desplaza a `2026-06-01→06-20` conservando la duración. ✔

**Fragilidad latente encontrada (no un bug activo).** `check_sufficiency` (`event_manager.py:80`) hace `stock["personas"].remove(key)` para cada persona ocupada en el intervalo. Forcé el caso en que un evento referencia a una persona que **ya no está** en `resources.json`: lanza `ValueError: list.remove(x): x not in list`. Con los datos actuales del repo no se dispara (toda persona usada existe en el master), pero si algún día se borra una persona del catálogo sin depurar sus eventos, la app reventaría al listar disponibilidad. Un `if key in stock["personas"]:` lo blindaría.

**Compilación:** `py_compile` de los 5 módulos → OK. No hubo ningún `Traceback` del código en los flujos de negocio probados.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Idiomático y correcto:** uso de `defaultdict` (`event_manager.py:6`), `datetime.fromisoformat` / `timedelta`, `enumerate`, f-strings, `with open(...)`, y el patrón `lambda k=(...)` para capturar el id en los botones (`main.py:125`) — este último es un acierto no trivial que el estudiante entendió bien y documentó en `report.md`.
- **Manejo de errores por retorno de valor:** las validaciones devuelven strings/tuplas en vez de lanzar excepciones. Es un estilo válido y coherente en todo el proyecto, aunque mezcla "valor de dato" con "mensaje de error" en la misma variable (`control_center.py:26` inspecciona `int(elemento[0])` para distinguir error de fecha válida) — funciona, pero es frágil; excepciones o un tipo resultado explícito serían más limpios.
- **`except:` desnudo** en varios sitios (`main.py:358`, `format_check.py:25`, `event_manager.py:64`): captura todo, incluido lo inesperado. Preferible `except ValueError:`. El truco `int("asd")` para forzar un error controlado (`main.py:357`, `main.py:846`) funciona pero es un anti-patrón; `raise ValueError(...)` es lo idiomático.
- **Sin globales mutables** ni estado compartido problemático — bien.

Son observaciones de estilo menores, esperables y aceptables en primer año.

## Dimensión 5 — Datos y persistencia

Modelo claro y bien diseñado. Tres JSON: `events.json` (lista de eventos con `id`, `tipo`, `info`, `f1`/`f2` en ISO 8601, `recursos` = `{personas: [...], materiales: {rec: cant}}`), `resources.json` (catálogo maestro), `id_count.json` (contador para ids únicos `a_N`). El uso de ISO 8601 para fechas es una buena elección (comparación lexicográfica válida, `control_center.py:33`). La capa `handler.py` aísla toda la I/O con `ensure_ascii=False, indent=4`. El diseño de "restar del stock los recursos ocupados en el intervalo, sobre una copia" (`event_manager.py:77-100`) es una solución elegante al problema de disponibilidad sin mantener un ledger mutable. Único detalle: `check_sufficiency` **muta** el dict `stock` que recibe, pero como proviene de una lectura fresca de disco en cada llamada, no acumula corrupción (lo verifiqué llamando `get_list_month` dos veces seguidas sin efecto).

## Dimensión 6 — Informe (`report.md`)

Informe **excelente y honesto** (3.221 palabras). Describe con precisión los módulos, las restricciones de negocio, tres ejemplos de uso detallados y una sección de "recorrido personal" madura. Cotejé sus afirmaciones contra el código ejecutado:

- Las restricciones enumeradas (co-requisitos, exclusión mutua, límites de duración) **coinciden exactamente** con `validar_reglas` (`control_center.py:60`) y `validar_fechas` (`control_center.py:36`). No exagera features.
- La "Nota" (`report.md:126`) sobre el bloque comentado de fechas de prueba (`main.py:510-514`) es real y honesta: reconoce un atajo de depuración dejado en el código.
- La explicación del algoritmo de disponibilidad (`report.md:141`) es fiel a la implementación.
- El estudiante no usa lenguaje de "demuestra"/"prueba" que sobreestime validación: describe el funcionamiento, no reclama garantías formales. Buen tono.

Una discrepancia menor de nomenclatura: el informe llama al botón "Seleccionar Recursos"/"Sección Añadir" pero el código dice "Elegir Recursos" (`main.py:502`) y "Sección añadir" (`main.py:410`). Trivial.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**, notablemente por encima del promedio de primer año. La lógica de negocio es correcta en todos los flujos que ejecuté —validación de fechas, reglas de exclusión/inclusión, disponibilidad de recursos con detección de colisiones, persistencia y buscador de hueco—, y ninguno reventó con entradas inválidas. La arquitectura modular está bien concebida y justificada, y el informe es de los más completos y honestos que he revisado. Los defectos son de estilo (nombres crípticos, `except:` desnudos, triplicación del bloque de pintado) y una fragilidad latente en `check_sufficiency` que no se dispara con los datos actuales.

- **Principal fortaleza:** la separación de responsabilidades entre GUI, orquestación, algoritmos, formato y persistencia — hecha con criterio y ejecutándose sin errores en toda la lógica de dominio.
- **Principal área de mejora:** eliminar la triplicación del código de pintado en `paint` extrayendo un helper, y blindar `check_sufficiency` (`event_manager.py:80`) contra personas ausentes del catálogo con un `if key in stock["personas"]`.
