# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #315
- **Repositorio:** https://github.com/KEVINJuegos/Noemilator
- **Estudiante:** Kevin Y. García-Pola González
- **Grupo:** C122
- **Descripción declarada:** Noemilator es un programa de planificación de horarios semanales para centros educativos: organiza, crea y ofrece una vista general de los horarios de las diferentes sesiones/clases, relacionándolos con locales, personal docente, alumnos, materias y otros recursos.

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una GUI construida con **Flet 0.28.3** (`main.py:2`, `requirements.txt:1`), un framework Python sobre Flutter. No usa `input()`; la interacción es por ratón sobre widgets. Adapté la ejecución en tres capas:

1. **Compilación:** `py_compile` de los 9 módulos → todos compilan sin error.
2. **Lógica de negocio:** ejercité directamente la capa de datos (`data/temp_save.py`, `data/resources.py`) con datos reales, sin GUI. Es una capa perfectamente separable, lo cual es un acierto de diseño.
3. **Arranque real de la GUI:** lancé la app en modo web headless (`fl.app(..., view=WEB_BROWSER)`) tras instalar `flet[all]`. El servidor respondió **HTTP 200** sirviendo el índice Flet/Flutter. La app arranca de verdad; solo faltaba el display de escritorio (limitación del entorno, no del código).

---

## Dimensión 1 — Qué hace el programa

La app abre en un **menú principal** (`pages/main_menu.py:5`) con tres botones: `New` (navega a `/workspace`), `Load` (sin handler — botón muerto, `main_menu.py:14`) y `Exit` (`page.window.close()`).

El **workspace** (`pages/workspace.py:11`) es el corazón: una barra lateral con seis paneles conmutables (`workspace.py:118-123`) y una cabecera con nombre de proyecto editable + botón de guardado JSON (`workspace.py:19-28`). Los paneles:

- **Dashboard** (`pages/panels/dashboard.py`) — construye una tabla semanal (días × turnos). El panel izquierdo configura rango de días (dropdowns Inicio/Fin, `dashboard.py:106-139`) y turnos horarios editables (`dashboard.py:141-154`). Cada celda de la tabla abre un diálogo (`open_slot_dialog`, `dashboard.py:361`) que lista los eventos de ese `(día, turno)` y permite agregar/editar/eliminar. El formulario de evento (`open_event_form_dialog`, `dashboard.py:239`) presenta checkboxes de eventos, grupos, humanos, lugares y objetos para asociarlos al bloque.
- **Eventos, Grupos, Locales, Humanos, Objetos** (`events.py`, `grupos.py`, `places.py`, `humans.py`, `objects.py`) — CRUD de cada recurso: alta con nombre (y tipo/cantidad según el caso) y baja con botón papelera. Locales y Humanos asignan un tipo con color+icono (`places.py:6-26`, `humans.py:6-20`); Objetos lleva cantidad numérica editable en vivo (`objects.py:28-35`).

El botón de guardado (`temp_save.py:312`) abre un `FilePicker` y exporta el estado completo a JSON.

## Dimensión 2 — Organización del código

**Fortaleza clara para primer año.** La separación en capas es limpia y consistente:

- `data/resources.py` — modelos con `@dataclass` (`Event`, `ClassGroup`, `Place`, `Human`, `Object`, `TimeSlot`, `ScheduleEvent`, `Schedule`) más constantes de dominio (`DAYS`, `PLACE_TYPES`, `HUMAN_TYPES`, `COLORES`).
- `data/temp_save.py` — capa de estado y operaciones (add/get/remove/update por recurso) más export JSON. Es la "lógica de negocio" separable que pude probar sin GUI.
- `pages/` — vistas; `pages/panels/` — un módulo por panel, cada uno con la misma estructura (lista + inputs + header + carga de datos existentes). Esa repetición de patrón hace el código muy legible.

El patrón de **factory functions que devuelven handlers** (`fl_delete_evento(evento.id)` retorna un `handler`, `events.py:17-26`) es idiomático y bien resuelto: evita el clásico bug de closures sobre variable de bucle.

**Debilidades (menores, propias del nivel):**

- Todo el estado vive en **globales de módulo** con `global` en cada función (`temp_save.py:9-21`, `39-43`, etc.). Funciona porque hay un solo proyecto en memoria, pero no escala a "cargar/cambiar de proyecto" y complica el testeo. Una clase `Project` que encapsule esas listas sería el siguiente paso natural.
- Los recursos se **identifican por nombre** (`remove_place(place_name)`, `temp_save.py:112`). Verifiqué que dos lugares con el mismo nombre coexisten y `remove` borra solo el primero — ambigüedad latente.
- `main.py:24` (`navigator_back`) usa `# type: ignore` y accede a `top_page.route`; funciona pero es frágil.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejercité la capa de datos con un centro educativo de ejemplo. **Todo lo que corrí funcionó sin `Traceback`:**

1. **Alta de recursos** — `add_event/grupo/place/human/object` produjeron los `__str__` correctos: `#1 Matemáticas`, `10-A`, `Aula 101 (Aula)`, `Prof. García (Profesor(a))`, `Proyector x3`. IDs de eventos autoincrementales correctos (`temp_save.py:41`).
2. **Rango de días** — `set_schedule_days("Lunes","Viernes")` → `['Lunes'..'Viernes']`. El **wrap-around** funciona: `("Viernes","Lunes")` → `['Viernes','Sábado','Domingo','Lunes']` (`resources.py:110-117`). Bien pensado.
3. **Días inválidos** — `set_schedule_days("Basura","Otra")` → `get_days()` cae correctamente a `DAYS[:5]` (Lun-Vie). Manejo de entrada inválida presente y correcto (`resources.py:111-112`).
4. **Turnos** — alta con numeración `max(...)+1` (`temp_save.py:214-216`), actualización de hora (`update_time_slot(1, start_time="07:30")` reflejado), eliminación.
5. **Eventos de horario** — alta con ID asignado, consulta por `(día, turno)`, actualización que reemplaza en su lista (`update_schedule_event`, `temp_save.py:263`).
6. **Cascada al borrar turno** — `remove_time_slot(2)` eliminó también los `schedule_events` de ese turno (`temp_save.py:231-236`). Verificado: 2 eventos → 1 tras borrar el turno. Esta consistencia referencial es un detalle maduro.
7. **Robustez ante operaciones nulas** — probé `update_object_quantity("inexistente")`, `remove_event(999)`, `remove_grupo("fantasma")`, `update_time_slot(999,...)`, `remove_schedule_event("Domingo",99,5)` (clave inexistente) y re-borrado de clave ya eliminada: **ninguno crashea**. Los bucles de búsqueda simplemente no encuentran y salen.
8. **Export JSON** — `export_to_json` generó un JSON bien formado, con `ensure_ascii=False` (acentos preservados: `"Matemáticas"`, `"Prof. García"`) e `indent=4`. Estructura completa y coherente con los modelos.
9. **Arranque GUI real** — servidor Flet respondió HTTP 200; los 6 paneles y las 2 vistas se construyen sin excepción usando un `page` stub.

**Bug funcional menor:** `Object` acepta cantidad negativa a nivel de modelo (`Object("X",-5)` → `X x-5`). En la GUI el `NumbersOnlyInputFilter` (`objects.py:72`) bloquea el signo, así que en la práctica no ocurre; pero la capa de datos no valida.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Legibilidad alta.** Nombres descriptivos, `@dataclass` bien usadas, comentarios de sección con banners. Uso correcto de `field(default_factory=list)` (`resources.py:92-96`), evitando el bug de mutable default.
- **Manejo de errores:** casi ausente pero **coherente con el diseño** — las operaciones son idempotentes y no revientan ante datos inexistentes (verificado). Faltaría validación de formato de hora (`"HH:MM"` no se valida; `dashboard.py:19` acepta cualquier texto).
- **Globales con `global`** en cada mutador es el punto menos idiomático, ya comentado.
- Pequeños detalles: `Object` sombrea el builtin `object` (`resources.py:71`) — funciona pero conviene evitarlo. Lambdas asignadas a nombre (`legend_chip`, `dashboard.py:756`) llevan `# noqa: E731`, señal de que ya pasó un linter.

## Dimensión 5 — Datos y persistencia

Modelo de datos sólido y bien tipado con dataclasses. `ScheduleEvent` guarda **listas de referencias** (`event_ids`, `groups`, `humans`, `places`, `objects`) — los eventos referencian por id, el resto por nombre. `schedule_events` es un `dict` con clave `(día, turno)` a lista de eventos (`temp_save.py:19`), buena elección para acceso por celda.

Serialización con `asdict()` + `json.dump` (`temp_save.py:290-304`), correcta y probada. **Limitación real:** la **importación no existe** — `import_from_json` está solo como comentario (`temp_save.py:307`) y el botón `Load` no tiene handler (`main_menu.py:14`). El estado se puede exportar pero no volver a cargar; la persistencia es unidireccional. El nombre del módulo, `temp_save`, es honesto sobre su estado provisional.

## Dimensión 6 — Informe (`report.md`)

El informe está **bien presentado** (badges, índice, secciones), pero **sobreestima significativamente** lo implementado. Discrepancias concretas:

- **"Detección de conflictos: alertas automáticas cuando se detectan superposiciones… asignaciones duplicadas de locales o docentes"** (`report.md:50`, `72`, `81`) — **no existe en el código.** No hay ninguna lógica de detección de conflictos; grep de `conflict/superpos/deshabilit` no encuentra nada. Es la feature estrella declarada y está ausente.
- **"Guardado automático… persistencia… Importar: carga un archivo JSON"** (`report.md:51`, `112`) — el guardado es **manual** (botón), no automático; y **la importación no está implementada** (`temp_save.py:307`).
- **"Gestión de materias… código, duración por sesión, requisitos"** y **"Gestión de alumnos… asigna alumnos a grupos"** (`report.md:47-48`, `99-107`) — no hay entidad Materia ni Alumno; lo más cercano son `Event` (evento genérico) y `ClassGroup` (solo nombre). El informe describe campos (especialidad, capacidad, disponibilidad) que **ningún modelo tiene** (`resources.py`).
- **"muestra solo docentes disponibles… aulas ocupadas se muestran deshabilitadas"** (`report.md:78-79`) — no hay filtrado por disponibilidad; los checkboxes muestran todos los recursos siempre.
- El ejemplo práctico (`report.md:83-85`) describe un flujo con validación de conflictos que el programa no ejecuta.

El informe describe una **aplicación aspiracional**, no la entregada. La sección "Dificultades Encontradas" quedó **vacía** (`report.md:135-138`). En descargo: lo que sí existe (dashboard, CRUD de 5 recursos, export JSON) funciona bien.

---

## Valoración global (orientativa, sin nota numérica)

Un proyecto **sólido en lo que entrega**, con una arquitectura por capas notablemente limpia para primer año y una GUI Flet real que arranca y funciona. La lógica de negocio pasó todas mis pruebas de ejecución —flujos válidos, inválidos y de borde— sin un solo `Traceback`, incluida la consistencia referencial al borrar turnos y el manejo del rango de días con wrap-around. El estudiante claramente entendió dataclasses, closures y separación de responsabilidades. El punto débil no es el código sino la **brecha entre el informe y la implementación**: el informe promete detección de conflictos, importación JSON, gestión de materias/alumnos/docentes con disponibilidad — nada de eso existe. Corregir el informe para que describa lo realmente construido (que ya es un logro respetable) lo dejaría en muy buen lugar.

- **Principal fortaleza:** arquitectura por capas limpia y una capa de datos robusta que no crashea ante entradas inválidas ni operaciones sobre datos inexistentes — verificado ejecutando.
- **Principal área de mejora:** alinear el informe con el código (eliminar features no implementadas) y, en el código, completar la importación JSON y encapsular el estado global en un objeto `Project` en lugar de globales de módulo.
