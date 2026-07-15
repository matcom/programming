# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #306
- **Repositorio:** https://github.com/cymir01/War-is-Coming.git
- **Estudiante:** Cynthia Moreno Miranda
- **Grupo:** C-122
- **Descripción declarada:** Planificador de eventos bélicos ambientado en *A Song of Ice and Fire*. Organiza campañas (asedios, batallas navales, asaltos, emboscadas, etc.) asignando recursos limitados (unidades de distintas casas, personajes clave, recursos especiales) respetando reglas de co‑requisito, exclusión mutua y enemistades entre casas. Valida conflictos horarios, sugiere el próximo hueco disponible y persiste todo en JSON, con interfaz de consola enriquecida vía `rich`.

---

## Nota metodológica importante

Es una aplicación de **consola interactiva** (usa `rich.Prompt`/`Confirm`/`console.input`), no GUI. Se ejecutó de dos formas complementarias:

1. **Flujos completos por consola** alimentando `main.py` con `printf` a través de todo el menú (agregar / listar / ver detalles / eliminar / salir), incluyendo entradas basura.
2. **Lógica de negocio aislada** importando `data_manager` y `planner` directamente para probar rutas de validación que la interfaz no expone cómodamente (co‑requisitos de tipo de recurso, Batalla naval con cuádruple co‑requisito).

Entorno: `uv venv --python 3.12` + `rich==15.0.0`. Los 10 módulos pasan `py_compile`. Nota: el estudiante commiteó un `.venv/` de Windows, una carpeta `.idea/` y `__pycache__/` — deberían estar en `.gitignore` (ver Dimensión 4).

## Dimensión 1 — Qué hace el programa

Arranca en `main.py:4` → `main_menu.main()` (`src/interface/main_menu.py:13`). Muestra un panel de bienvenida, pide el nombre y entra en un bucle de menú con cinco acciones (`main_menu.py:31-46`): agregar (`a`), listar (`l`), ver detalles (`v`), eliminar (`d`), salir (`s`).

- **Agregar** (`command_add_event.py:8`): asistente paso a paso — nombre, descripción opcional, tipo de evento (elegido de una lista fija, `command_add_event.py:55-57`), ubicación opcional, era (DC/AC), selección de recursos por ID (con re‑pregunta si hay IDs inválidos), fechas por componentes (año/mes/día/hora/minuto), y una búsqueda opcional del próximo hueco. Finalmente delega en `add_event` (`command_add_event.py:170`).
- **Listar** (`command_list_events.py:6`): tabla `rich` con ID, nombre, tipo, era, descripción, estado, inicio, fin, recursos.
- **Ver detalles** (`command_view_details.py:5`): submenú para ver un evento por ID (tabla campo/valor con duración calculada) o la agenda de un recurso (todos los eventos que lo usan).
- **Eliminar** (`command_delete_event.py:4`): lista los eventos y borra por ID, persistiendo el cambio.

**Ejecución observada:** al listar aparece el evento sembrado (id 1, "Event Test", Misión diplomática, recurso 128). Todo el ciclo agregar→listar→ver→eliminar→recargar funciona y persiste correctamente.

## Dimensión 2 — Organización del código

Arquitectura en tres capas, muy bien separada para 1er año:

- **Modelos** (`src/models/`): `Event` (`event.py:5`) y `Resource` (`resource.py:1`), con `event_to_dict`/`create_event_from_dict` y `robject_to_dict`/`create_robject_from_dict` para (de)serialización JSON. `Event.__lt__` (`event.py:36`) compara por `start`, lo que habilita `sort()` y `bisect.insort` sin `key`.
- **Servicios** (`src/services/`): `data_manager.py` mantiene el estado global (`EVENTS`, `RESOURCES`, `RESTRICTIONS`, `NEXT_EVENT_ID`) y las operaciones CRUD + persistencia; `planner.py` concentra el motor de validación (cinco funciones especializadas) y el buscador de huecos.
- **Interfaz** (`src/interface/`): un archivo por comando, más `main_menu.py`. Buena granularidad.

Fortalezas: separación de responsabilidades genuina, docstrings extensos en varios comandos, nombres descriptivos. Las restricciones viven en datos (`default_data.json`), no en código — decisión de diseño madura que hace el sistema extensible (`planner.py:16-20` las lee del diccionario).

Debilidades menores: estado global mutable en `data_manager.py:16-19` (aceptable a este nivel, pero acopla todo a variables de módulo); la carpeta `tests and resources/` contiene borradores comentados (`tests.py` está enteramente comentado, `calendar_function.py`, `resource.py` duplicado) que no son tests reales y ensucian el árbol.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente se **ejecutó** de verdad:

1. **Listar evento sembrado** (`printf 'x\nl\ns'`): tabla `rich` correcta con id 1. ✅
2. **Agregar Batalla campal válida** (Stark 89/91/93, año 300): "Evento 'Batalla del Norte' agregado con ID: 2". ✅
3. **Casas enemigas** (Stark 89/91 + Lannister 66, Batalla campal): rechazado con "error: la casa Stark no puede aliarse con la casa Lannister". ✅
4. **Co‑requisito por tipo de evento** (Misión diplomática sin Embajador): "Error: el evento tipo Misión diplomática requiere el tipo de recurso Embajador". ✅
5. **Fecha fin ≤ inicio** (Asalto): "La fecha final debe ser posterior a la inicial" y aborta. ✅
6. **Fecha basura** ("abc" en el año): "ups! error en la fecha. Inténtelo de nuevo" y re‑pregunta; luego agrega bien. ✅
7. **Conflicto de recurso + hueco siguiente**: dos Asalto con recursos 89/91 solapados; al segundo se solicitó hueco → "Hueco encontrado: 0300-10-01 14:00:00 - 0300-10-01 20:00:00" (justo después del primero). Preciso. ✅
8. **ID de recurso inexistente** (999): "Error: Los siguientes ids no existen: 999" y re‑pregunta. ✅
9. **ID de recurso no numérico** ("abc"): "ids inválidos (deben ser números...)" y re‑pregunta. ✅
10. **Comando de menú desconocido** ("z"): "Comando no reconocido. Inténtelo de nuevo :)". ✅
11. **Ver detalles / agenda de recurso**: tablas correctas (duración calculada 6:00:00). ✅
12. **Eliminar + persistencia**: borrado el id 2, al reiniciar solo quedó el id 3. ✅
13. **Co‑requisito de tipo de recurso** (Flota sin Almirante, vía API): "Error: el recurso tipo 'Flota' requiere el recurso tipo 'Almirante'". ✅
14. **Batalla naval con cuádruple co‑requisito** (Almirante 9 + Flota 12 + Fuego valyrio 133 + Piromante 134): agregada con éxito; quitando el Fuego valyrio, rechazada correctamente. ✅

**No se observó ningún `Traceback`** en ninguna ruta, válida o inválida. El motor de restricciones (cinco categorías: inclusión/exclusión de tipos de recurso, inclusión/exclusión por tipo de evento, enemistad de casas) funciona en todas las combinaciones probadas.

**Bug latente detectado (no se manifiesta en el uso normal):** `Event.__init__` usa un argumento por defecto mutable, `resources_ids: list = []` (`event.py:10`). Verificado: dos `Event` creados sin pasar `resources_ids` **comparten la misma lista** (`e1.resources_ids is e2.resources_ids` → `True`). No causa fallo en la práctica porque `add_event` siempre pasa una lista explícita (`data_manager.py:111`), pero es un pozo clásico de Python. Lo idiomático es `resources_ids: list = None` y dentro `if resources_ids is None: resources_ids = []`.

**Observación sobre el buscador de huecos:** `find_next_available_time_slot` fija `event_type = "Batalla campal"` por defecto cuando llega `None` con `restrictions` presentes (`planner.py:143-144`). En la interfaz siempre llega el tipo real (`command_add_event.py:159`), así que no afecta el flujo actual; pero es un valor mágico frágil si se reutilizara la función.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Sólidas para el nivel: `try/except` en parseo de fechas e IDs, bucles `while True` de validación, comprensiones de lista (`data_manager.py:162,167,178`), `bisect.insort` para inserción ordenada, `set` para deduplicar. Uso correcto de `datetime.isoformat`/`fromisoformat`. Docstrings amplios (aunque algunos, como en `command_list_events.py:15-32`, quedaron con secciones "Workflow: 1. 2. 3." vacías — copy‑paste de plantilla sin rellenar).

Puntos mejorables (menores):
- El argumento por defecto mutable ya citado (`event.py:10`).
- Cruft versionado: `.venv/` (binarios Windows), `.idea/`, `__pycache__/` están en git. Un `.gitignore` con estas entradas es la práctica estándar.
- `type` y `id` se usan como nombres de variable (`command_add_event.py:70,106`), sombreando *builtins*. Inocuo aquí, pero conviene evitarlo.
- Mezcla de idiomas en la API (funciones/docstrings en inglés, mensajes en español). Coherente, no es error.
- `update_event_status()` está definida vacía (`data_manager.py:151-152`) — feature anunciada como futura, honestamente marcada en el informe.

## Dimensión 5 — Datos y persistencia

Modelo de datos rico y bien pensado: `data/default_data.json` define **135 recursos** (12 tipos × ~10 casas + especiales sin casa como Fuego valyrio/Piromante) y **cinco categorías de restricciones** parametrizadas por *tipo de recurso*, no por ID — decisión que el informe justifica (permite añadir casas sin tocar reglas) y que verifiqué: las reglas se aplican genéricamente a cualquier recurso del tipo.

Persistencia en `data/war_planner.json`: `load_data` (`data_manager.py:21`) crea el archivo desde `default_data.json` si no existe, con *fallback* a datos vacíos si tampoco está el default. `save_data` (`data_manager.py:72`) serializa con `ensure_ascii=False` (tildes correctas) y `default=str`. Fechas van como ISO. Verifiqué el ciclo completo: agregar → escribir JSON → reiniciar → releer → estado idéntico. El diseño es autoconsistente: comprobé que todo tipo requerido por alguna restricción existe en los datos, así que todo tipo de evento es realizable.

Detalle: el repo trae un `war_planner.json` con un "Event Test" sembrado; no es problema, pero deja el estado "sucio" en la primera ejecución del evaluador.

## Dimensión 6 — Informe (`report.md`)

Informe de ~3.900 palabras, **excelente y notablemente honesto**. Coincide con el código con mucha fidelidad: describe correctamente las tres capas, las cinco categorías de restricciones, el buscador de huecos y la persistencia. Puntos a favor:

- **Documenta bugs reales que encontró y arregló**, no solo aciertos: el typo `type` vs `resource_type` en `create_robject_from_dict` (§5.9), la indentación que solo evaluaba el último evento y el typo `rreesource_...` (§5.4). Esta transparencia es justo lo que se quiere ver.
- Distingue con claridad lo hecho de lo futuro: `update_event_status`, recursos con cantidad, eventos recurrentes (§7) — coincide con el código, no exagera.
- Es explícita sobre asistencia de IA y fuentes (GeeksforGeeks, libro de Matthes) para `__lt__`, `isinstance`, `fromisoformat` (§6). Honestidad ejemplar.

Discrepancias/matices menores:
- El informe llama al buscador "robusto" y que "maneja todos los casos" (§2.4, §3.3). Es funcional y lo verifiqué en varios escenarios, pero el valor mágico `event_type="Batalla campal"` por defecto (`planner.py:143`) y la granularidad de 1 hora lo hacen correcto-pero-no-blindado; "robusto" es algo optimista, no falso.
- El ejemplo de salida en §4 muestra una tabla de listar **sin** columnas Descripción/Estado, pero el código sí las incluye (`command_list_events.py:44-46`). Discrepancia cosmética de un ejemplo redactado a mano.
- Menciona "usé el depurador" (§6, Depuración): en el flujo observado la depuración fue por `print`/prueba‑error, como el propio §5.11/§6 admite. Trivial.

Nada de esto sube al nivel de "el informe demuestra algo que el código no hace". La correspondencia informe↔código es de las mejores del lote.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**, muy por encima de lo esperable en un primer año. El dominio es rico (135 recursos, cinco categorías de restricciones interdependientes), la arquitectura en tres capas es real y no decorativa, y —lo más importante— **todo lo que probé funciona**: validaciones de casas enemigas, co‑requisitos de tipo de recurso y de tipo de evento, exclusiones, conflictos de recursos, sugerencia del próximo hueco (con hora exacta correcta tras un solapamiento), persistencia entre ejecuciones y manejo de basura en cada punto de entrada, todo sin un solo `Traceback`. El informe acompaña con honestidad poco común, documentando los bugs que la propia estudiante cazó y corrigió.

**Principal fortaleza:** un motor de restricciones genérico, parametrizado por datos, que funciona correctamente en todas las combinaciones probadas (incluida la Batalla naval con cuádruple co‑requisito) — respaldado por un informe transparente que coincide con el código.

**Principal área de mejora:** higiene del repositorio (dejar de versionar `.venv/`, `.idea/`, `__pycache__/` con un `.gitignore`) y corregir el argumento por defecto mutable en `Event.__init__` (`event.py:10`); en un nivel más fino, retirar los borradores de `tests and resources/` y rellenar (o quitar) los docstrings de plantilla que quedaron vacíos.
