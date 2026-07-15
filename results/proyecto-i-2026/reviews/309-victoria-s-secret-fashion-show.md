# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #309
- **Repositorio:** https://github.com/anacalzado335-sys/Victoria-s-Secret-Fashion-Show (rama `Branch_dev`)
- **Estudiante:** Ana Paula Martínez Calzado
- **Grupo:** C-122
- **Descripción declarada:** Software que automatiza y optimiza la gestión y planificación de desfiles de moda para la marca Victoria's Secret.

---

## Nota metodológica importante

**No es una app de consola** — es una aplicación web con interfaz gráfica sobre **Streamlit** (`main.py:1`, `streamlit run main.py`). La verificación automática del issue analizó la rama `main`, que contenía una versión anterior con archivos distintos (`clothes.py`, `resource.py`, `planner.py` sueltos); el trabajo real y completo está en la rama **`Branch_dev`**, que es la que evalué.

Por ser GUI, adapté la ejecución así:

1. **Compilación:** `py_compile` de los 5 módulos → todos OK.
2. **Lógica de negocio aislada:** instancié `Planner` con los datos reales de los JSON del repo y ejecuté directamente los métodos (`validate_co_requisite`, `validate_inclusion`, `is_available`, `find_next_gap`, `add_event`, `save_to_json`) con flujos válidos e inválidos.
3. **Arranque headless de la GUI:** `streamlit run main.py --server.headless true` → arrancó limpio, servidor Uvicorn levantado, **HTTP 200**, sin `Traceback`.
4. **Rama de carga de `database.json`** (la que usa `main.py` en arranque real): la repliqué con el `database.json` del repo → cargó 8 recursos, 13 prendas, 4 eventos correctamente.

## Dimensión 1 — Qué hace el programa

Es un planificador de desfiles con tres pestañas (`main.py:152`):

- **Calendario de Eventos** (`main.py:154-170`): lista los desfiles registrados en expanders, muestra recursos asignados y permite eliminar por ID.
- **Nuevo Evento** (barra lateral, `main.py:69-149`): formulario con nombre, fechas/horas de inicio y fin, y multiselección de modelos, lugares, ropa, calzado y accesorios. Al enviar valida co-requisitos, exclusiones y conflictos de horario antes de añadir y persistir.
- **Encontrar Disponibilidad** (`main.py:173-194`): dado un número de horas y unos recursos, calcula el próximo hueco libre con `find_next_gap`.
- **Recursos** (`main.py:197-239`): selecciona una modelo/lugar y muestra su agenda detallada y con quién comparte pasarela.

La persistencia se unifica en `database.json`; si existe se carga de ahí, si no se reconstruye desde los tres JSON fuente (`main.py:26-59`).

## Dimensión 2 — Organización del código

**Fortaleza destacada.** El código está bien modularizado por entidad del dominio, con separación real de responsabilidades:

- `models.py` → clase `Event` (`models.py:4`) + loader.
- `clothing.py` → clase `Clothes` (`clothing.py:3`) + loader.
- `resources_manager.py` → clase `Resource` (`resources_manager.py:4`) + loader.
- `planner.py` → clase `Planner` (`planner.py:8`), el núcleo lógico.
- `main.py` → solo la capa de presentación Streamlit.

La decisión de que `Planner` reciba las colecciones ya instanciadas en el constructor (`planner.py:9`) en lugar de leer archivos por su cuenta desacopla la lógica de negocio de la GUI. Esto lo verifiqué en la práctica: pude ejercitar toda la lógica **sin arrancar Streamlit**, justamente porque está desacoplada. Para primer año, esto es maduro.

Debilidades menores: la conversión de fechas ocurre dentro del constructor de `Event` (`models.py:8-9`), lo que hace que un formato inválido reviente en la construcción (ver Dimensión 3); nombres con mezcla de idiomas e inconsistencias (`events_calendary`, `find_event_by_Id`); y la lógica de arranque de `main.py:26-59` duplica parte del trabajo de los loaders.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo que corrí funcionó correctamente. Numerado:

1. **`validate_co_requisite`** — con solo una modelo → `(False, "ERROR DE CO-REQUISITO ...")`; con modelo + lugar → `(True, "Validación Exitosa")`. **Correcto.**
2. **`validate_inclusion`** — con Naomi Cambell + Grand Palais → `(False, "ERROR DE EXCLUSION ...")`; con Adriana + Grand Palais → `(True, ...)`. **Correcto.**
3. **`is_available`** — Adriana asignada a un evento del 24 al 27 de sept: un evento nuevo el 26 → `False` (conflicto detectado); un evento en diciembre → `True`. El solapamiento (`planner.py:77`, `begin < end and begin < end`) está bien implementado. **Correcto.**
4. **`add_event` + `save_to_json`** (`planner.py:14`, `planner.py:29`) — añadí un evento y lo persistí; el JSON reconstruido tenía 5 eventos, con `assigned_resources` serializados como nombres y las tres claves (`events`, `resources`, `clothes`). **Correcto.**
5. **Loaders con archivo inexistente** — los tres imprimen su mensaje y devuelven `[]` sin reventar (`models.py:24`, `clothing.py:16`, `resources_manager.py:17`). **Manejo de errores correcto.**
6. **Arranque Streamlit headless** — HTTP 200, sin traceback.
7. **Carga desde `database.json`** — la rama real de `main.py` cargó 8/13/4 sin error.

**Quirks de lógica (no crashes) observados:**

- **`find_next_gap` no es óptimo** (`planner.py:116-157`). Cuando un recurso está ocupado, avanza `gap_needed + 1 día` (`planner.py:151`) en vez de saltar al fin del evento que causa el conflicto. En una prueba con una modelo ocupada de +1h a +5h buscando 3h, devolvió un hueco a +27h en lugar del hueco válido justo tras el evento (+5h). Encuentra *un* hueco válido, pero no el primero. Para primer año es aceptable; conviene saberlo.
- **`Event` con fecha malformada** lanza `ValueError` en el constructor (`models.py:8`). En la GUI esto no ocurre porque las fechas vienen de widgets `date_input`/`time_input`, pero es un punto frágil si algún día se cargan datos externos con formato inválido.

No encontré ningún fallo del entorno; todo lo que falló, falló por diseño (mensajes controlados), no por bug.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Puntos fuertes: uso idiomático de comprensiones de lista, `datetime`/`timedelta` para aritmética temporal (`models.py:13`), `try/except FileNotFoundError` en los tres loaders, `json.dump(..., ensure_ascii=False)` para preservar tildes/eñes (`planner.py:65`). Las funciones de validación devuelven tuplas `(bool, str)` — patrón limpio y testeable.

Mejorables (menores): typos en strings de usuario (`"asiganr"` en `planner.py:88`, `"regristrados"` en `main.py:157`); indentación inconsistente en `find_next_gap` (`planner.py:153-154`); métodos como `add_event` hacen `return self.list.append(...)` (`planner.py:15`), que devuelve `None` innecesariamente; `find_event_by_Id` mezcla mayúsculas en el nombre. Nada de esto afecta el funcionamiento.

## Dimensión 5 — Datos y persistencia

Modelo de datos claro: tres JSON fuente (`resources.json`, `clothes.json`, `events.json`) más un `database.json` unificado que actúa como estado persistente. Los eventos se serializan con fechas como strings y recursos como lista de nombres (`planner.py:32-39`), reconstruyendo las referencias vía `res_map` al cargar (`main.py:41-52`). El diseño de consolidar en un solo archivo atómico está bien pensado y funciona: verifiqué el roundtrip completo add→save→reload sin pérdida de datos. Bien para el nivel.

Detalle: las prendas y accesorios se cargan y muestran en el formulario, pero **no se guardan en el evento** — `assigned_resources` solo recibe modelos y lugares (`main.py:115-118`, `main.py:145`), no la ropa seleccionada. La selección de ropa es visual pero no persiste en el desfile.

## Dimensión 6 — Informe (`report.md`)

El `report.md` **sí existe** en `Branch_dev` (la verificación automática no lo vio porque miró `main`). Está bien escrito y coincide en lo esencial con el código. Precisiones:

- **Coincide:** la arquitectura POO por módulos (`Resource`/`Clothes`/`Event`/`Planner`), el desacople del `Planner`, el mecanismo de `database.json` con `ensure_ascii=False`, y el arranque con `streamlit run main.py`. Todo verificable en el código.
- **Ligera sobreestimación de validación de campos:** el informe dice "se verifica que el campo lugar no se encuentre vacío" (§1). En realidad no hay un chequeo explícito de "lugar vacío"; lo que existe es `validate_co_requisite` (`planner.py:82`), que solo exige un lugar **si hay al menos una modelo**. Un evento sin modelos ni lugar pasaría esa validación. El efecto se acerca, pero la redacción es más fuerte que la implementación.
- **"Encontrar disponibilidad de modelos":** el informe (§1, §3) presenta `find_next_gap` como funcionalidad sólida. Funciona y devuelve un hueco válido, pero como noté en la Dimensión 3, no garantiza el *primer* hueco. El informe no menciona esta limitación (esperable, es sutil).
- No detecté exageraciones graves ni features inexistentes. El informe es honesto en general.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido** para primer año. La arquitectura está genuinamente bien pensada: separación por módulos, un `Planner` desacoplado de la interfaz que pude probar de forma aislada, y persistencia con roundtrip verificado. Todas las reglas de negocio que ejecuté (co-requisitos, exclusiones, conflictos de horario, guardado) funcionan correctamente, y la GUI Streamlit arranca sin errores. La ambición de pasar de consola a una web app y resolverlo leyendo documentación oficial habla muy bien del proceso. Los defectos son menores: `find_next_gap` no siempre da el hueco óptimo, la ropa seleccionada no persiste en el evento, y hay typos/inconsistencias de estilo.

- **Principal fortaleza:** modularización y desacople real entre lógica de negocio (`Planner`) e interfaz, que hizo la lógica testeable y verificable de forma independiente — con toda la validación de reglas funcionando correctamente al ejecutar.
- **Principal área de mejora:** afinar `find_next_gap` para que salte al fin del evento en conflicto (no `+1 día`) y así devuelva el primer hueco válido; y persistir la ropa/accesorios seleccionados dentro del evento, no solo mostrarlos.
