# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #302
- **Repositorio:** https://github.com/aadonisblanco/Proyecto---Camp-Nou-Events
- **Estudiante:** Adonis Javier Blanco Pelegrin
- **Grupo:** C-122
- **Descripción declarada:** Organizador de Eventos - Camp Nou: aplicación de escritorio en Python que permite crear, visualizar, analizar y exportar eventos del estadio. Interfaz gráfica con pestañas para gestión, validación de datos, estadísticas automáticas y almacenamiento persistente en JSON, con opción de exportación a CSV.

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una GUI de escritorio construida con Tkinter/ttk (Main.py + ui.py + Controller.py + Events.py). Por tanto no se alimentó con `printf` por un menú `input()`. La evaluación se adaptó así:

1. **Lógica de negocio ejecutada de verdad y por separado.** La arquitectura del estudiante separa limpiamente la lógica (`Events.py`, clases `Evento` y `GestorEventos`) de la interfaz. Se ejecutó esa capa directamente con datos reales del repo: creación, conflicto de horario, estadísticas, búsquedas, export CSV, eliminación y recarga desde JSON. Todo verificado con valores concretos (abajo).
2. **Intento de arranque GUI headless.** Se intentó arrancar la GUI real con `xvfb-run`. Falló con `Aborted (core dumped)` **por el entorno, no por el código**: un `tk.Tk()` desnudo con un `Label` de texto plano bajo el mismo `xvfb` también aborta (`xcb_io.c:166 assertion`). Es un problema de X11/fuentes del host de revisión, no un bug del estudiante. La construcción de la GUI (root, `Notebook`, pestañas, widgets) sí progresa hasta el punto de renderizado.
3. **`py_compile`** de los 4 módulos: OK.

## Dimensión 1 — Qué hace el programa

Aplicación de escritorio para gestionar eventos del Camp Nou con cuatro pestañas (ui.py:62-81):

- **➕ Crear Evento** (ui.py:88-159): formulario de dos columnas con campos nombre, fecha, hora, duración, tipo, ubicación, capacidad y descripción. Botones Guardar / Limpiar / Salir.
- **📅 Ver Eventos** (ui.py:187-250): `Treeview` con búsqueda, "Mostrar Todos", "Ver Detalles" (abre ventana `Toplevel`, Controller.py:186-241) y "Eliminar Evento" con confirmación (Controller.py:243-277).
- **📊 Estadísticas** (ui.py:252-292): tarjetas con total de eventos, próximos, capacidad total y tipo más común, más una "distribución por tipo" en texto.
- **📆 Calendario** (ui.py:404-716): pestaña adicional no declarada en el Readme — grid mensual navegable que resalta días con eventos y lista los eventos del día seleccionado. Es un extra genuino y funcional.

El flujo de guardado: `guardar_evento` (Controller.py:40) → `get_form_data` (ui.py:316) → `validar_datos_evento` (Controller.py:64) → `gestor.crear_evento` (Events.py:200) → persistencia automática en `events_data.json`.

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** Arquitectura de tres capas explícita y bien ejecutada para un primer año:

- `Events.py` — modelo y lógica pura (clase `Evento` Events.py:9, clase `GestorEventos` Events.py:152). **No importa Tkinter**: es lógica de negocio genuinamente aislada, lo que permitió ejecutarla de forma independiente.
- `Controller.py` — capa intermedia (`ControladorEventos` Controller.py:7) que conecta botones de la UI con métodos del gestor (`conectar_eventos` Controller.py:20).
- `ui.py` — solo la vista (`EventOrganizerUI` ui.py:8).
- `Main.py` — punto de entrada que ensambla las tres piezas (Main.py:12-24).

`Evento` usa `@property` para campos derivados (`hora_fin` Events.py:52, `fecha_hora_inicio` Events.py:62, `es_proximo` Events.py:81), `to_dict`/`from_dict` (Events.py:122-143) para serialización, y `__str__`/`__repr__`. Nombres descriptivos y consistentes en español. Docstrings en casi todos los métodos. Este nivel de modularidad y uso de propiedades está por encima de lo típico en primer año.

**Debilidades:**
- Fuga de responsabilidad: `Controller.py:186-241` construye una ventana `Toplevel` completa con `tkinter` — código de UI dentro del controlador. Además está casi duplicado en `ui.py:652-701` (`_mostrar_detalles_evento`). Deberían unificarse en la capa de vista.
- `clear_form_fields` (ui.py:311-314) es un `pass`: el controlador llama a "Limpiar Formulario" (Controller.py:139) pero no limpia nada — placeholder olvidado (ver Dimensión 3).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Lo que se corrió con la capa de negocio (datos propios), y lo observado:

1. **Crear evento válido + persistencia**: `crear_evento("Barca vs Madrid", 20/12/2026, 21:00, dur 2, cap 80000, precio 100)` → id `barcavsmad_20122026_2100`, `hora_fin` = `23:00`, `obtener_ingresos_estimados` = `6400000.0` (80000×0.8×100). Se escribió `events_data.json`. **Correcto.**
2. **Conflicto de horario**: segundo evento `22:00–01:00` el mismo día solapa al anterior → `crear_evento` devolvió `None` y se imprimió el conflicto. **La detección de solapamiento funciona** (`verificar_conflicto_horario` Events.py:267).
3. **Evento sin conflicto** (otro día) → creado OK.
4. **Estadísticas**: `obtener_estadisticas` devolvió `total_eventos: 2`, `capacidad_total: 80500`, `tipo_mas_comun: "Partido de Liga"`, `ingresos_totales: 6400000.0`. **Correcto.**
5. **Búsquedas**: por nombre `"barca"` → `["Barca vs Madrid"]`; por tipo `"concierto"` → `[]`; por fecha → correcto.
6. **Entrada inválida (robustez)**: `Evento(fecha="99/99/9999", hora="99:99")` → `fecha_hora_inicio` = `None`, `hora_fin` = `""`. **No revienta** — los `try/except` de las propiedades absorben la fecha basura sin `Traceback`.
7. **Export CSV**: `exportar_a_csv` generó cabecera + filas correctas. **Correcto.**
8. **Eliminar + recarga**: eliminó por id, guardó, y una nueva instancia de `GestorEventos` recargó desde JSON con el estado correcto. **La persistencia round-trip funciona.**

**Bug confirmado por ejecución — la validación de capacidad está muerta.** `get_form_data` (ui.py:357-366) devuelve la clave `"capacidad"`, pero `validar_datos_evento` la lee como `datos.get('capacity', '0')` (Controller.py:117, clave en inglés que **nunca existe**). Se reprodujo la validación aislada: con `capacidad="999999999"` (muy sobre el límite de 99 354 del Camp Nou, Controller.py:122) → **0 errores**; con `capacidad="-500"` → **0 errores**. La validación siempre evalúa el default `'0'` en vez del valor real. Consecuencia práctica: se puede guardar un evento con capacidad absurda o negativa sin que el formulario lo rechace. Como `Evento.__init__` hace `int(capacidad)` (Events.py:36), una capacidad no numérica sí reventaría al crear — pero el mensaje de validación amable que el estudiante escribió nunca llega a mostrarse. **Corrección de una línea**: cambiar `'capacity'` por `'capacidad'` en Controller.py:117.

**Segundo problema funcional — "Limpiar Formulario" no limpia.** `clear_form_fields` es `pass` (ui.py:311-314). Al pulsar el botón (Controller.py:137-139), los campos no se vacían. Bug menor de UX.

**Observación de diseño (no bug):** `verificar_conflicto_horario` (Events.py:267-277) no considera la **ubicación**: dos eventos simultáneos en zonas distintas del estadio (p. ej. un concierto en Gol Norte y una visita guiada en el Museo) se marcan en conflicto aunque físicamente no lo estén. Para un primer año es una simplificación razonable, pero vale mencionarla.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Bien**: uso de `@property`, `type hints` (`List`, `Dict`, `Optional`), `**kwargs` con `.get(default)`, `@classmethod` para `from_dict`, f-strings, docstrings extensos.
- **Mejorable**: los `except:` desnudos en las propiedades de `Evento` (Events.py:58, 67, 76, 89, 99) y en `Controller.py:642` silencian cualquier error. Para robustez está bien que no revienten, pero conviene capturar `ValueError`/`TypeError` específicamente para no ocultar bugs reales.
- El uso de `datos.get('capacity', ...)` con default silencioso (Controller.py:117) es precisamente lo que ocultó el bug de la clave — un buen ejemplo de por qué los defaults silenciosos son peligrosos.
- `display_events` (ui.py:376-383) tiene fallbacks a claves en inglés (`event.get("nombre", event.get("name", ""))`) que nunca se usan: rastro de un cambio de idioma a medias en el esquema de datos, coherente con el desajuste `capacity`/`capacidad`.

## Dimensión 5 — Datos y persistencia

Modelo sólido. `Evento.to_dict`/`from_dict` (Events.py:122-143) serializan todos los campos; `GestorEventos.guardar_eventos`/`cargar_eventos` (Events.py:160-186) usan `json.dump/load` con `ensure_ascii=False` e `indent=2` y guardan automáticamente tras cada operación mutante. El id se genera de forma determinista a partir de nombre+fecha+hora (Events.py:45-49). Round-trip verificado en ejecución (TEST 8-9). Export a CSV con `csv.writer` correcto. Un id determinista puede colisionar si dos eventos comparten nombre/fecha/hora, pero es un caso de borde aceptable.

## Dimensión 6 — Informe (`Readme.md`)

Coincide en lo esencial con el código y describe bien la arquitectura de tres capas. Discrepancias menores:

- Menciona **"tres pestañas"** (Readme líneas 15) pero la app tiene **cuatro**: falta documentar la pestaña Calendario (ui.py:404), que es de hecho una de las partes más trabajadas. El informe **subestima** el proyecto aquí.
- Lista **"ingresos estimados"** como una métrica de estadísticas (Readme línea 6). El cálculo existe (`obtener_ingresos_estimados` Events.py:117, `ingresos_totales` en `obtener_estadisticas` Events.py:299) y se verificó = 6 400 000.0, **pero el controlador no lo pasa a la UI**: `datos_stats` (Controller.py:285-291) no incluye ingresos, así que nunca se muestran en pantalla. Feature calculado pero no expuesto.
- Nombres de archivo en el Readme (`ui_visual.py`, `controller.py`, `events.py`, `main.py`) no coinciden con los reales (`ui.py`, `Controller.py`, `Events.py`, `Main.py`). Detalle menor pero puede confundir a quien clone.
- No menciona la validación de capacidad como funcionalidad "demostrada" — bien, porque justamente está rota; no hay sobreafirmación en ese punto.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido**. La mayor fortaleza es arquitectónica: una separación real y bien pensada entre modelo (`Events.py`), controlador y vista, con la lógica de negocio genuinamente desacoplada de Tkinter — algo que permitió ejecutarla y verificarla íntegramente, y que está por encima del promedio de primer año. La lógica corre correctamente en todos los flujos probados: creación con persistencia JSON, detección de conflictos de horario, estadísticas, búsquedas, export CSV, eliminación y recarga round-trip, todo sin `Traceback` ante entradas basura. Los defectos son acotados y no arquitectónicos: un desajuste de clave (`capacity` vs `capacidad`) que desactiva silenciosamente la validación de capacidad, un "Limpiar Formulario" sin implementar, y algo de código de UI filtrado al controlador.

- **Principal fortaleza:** arquitectura de tres capas con la lógica de negocio limpiamente aislada de la GUI; el modelo `Evento`/`GestorEventos` es correcto, robusto ante entradas inválidas y con persistencia round-trip verificada.
- **Principal área de mejora:** el bug de una línea `datos.get('capacity')` → `datos.get('capacidad')` en Controller.py:117 que anula toda la validación de capacidad que el propio estudiante escribió; corregirlo restaura una feature ya construida.
