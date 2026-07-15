# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #314
- **Repositorio:** https://github.com/Stark040301/Hotel_Planner_Project
- **Estudiante:** Abel Rodríguez Puig
- **Grupo:** C-122
- **Descripción declarada:** Aplicación Python/CustomTkinter para gestionar inventario hotelero y planificar eventos con validación de recursos y persistencia en JSON. Arquitectura MVC.

---

## Nota metodológica importante

No es una aplicación de consola: es una **GUI de escritorio** construida con CustomTkinter, con arquitectura MVC real (modelo / vista / controlador separados). El arranque de la GUI (`python main.py`) falla en este entorno **por ausencia de servidor X11**, no por un fallo del código — el aborto es de la capa `xcb`/Tkinter (`Assertion !xcb_xlib_unknown_seq_number failed`), idéntico a lo que reportó la verificación automática. Es un fallo de entorno, no del estudiante.

Como la lógica de negocio está limpiamente separada de la vista, la evaluación de corrección se hizo **ejecutando el backend directamente** (modelos + `Scheduler` + `Controller`) con los datos reales del repo (`hotel_planner/data/default_data.json`, 81 recursos y 9 eventos precargados), replicando exactamente el flujo de carga que hace `app.py:65-110`. Además corrí la suite de tests del propio estudiante y compilé todos los módulos.

## Dimensión 1 — Qué hace el programa

El sistema gestiona un inventario hotelero de tres tipos de recurso (`Room`, `Employee`, `Item`, jerarquía en `resource.py:3-139`) y planifica eventos que consumen esos recursos, validando disponibilidad temporal y restricciones de compatibilidad.

Flujo de planificación (`scheduler.py`):
- `add_event` (`scheduler.py:149`) valida vía `_can_schedule` (`scheduler.py:96`): duración positiva, nombre único, restricciones co-requisito/exclusión (`validate_resource_constraints`, `resource.py:148`), y disponibilidad por solapamiento de intervalos.
- El conteo de reservas (`_count_reserved`, `scheduler.py:30`) suma cantidades de eventos que se solapan en el tiempo con el intervalo pedido, usando `max(start)/min(end)` para detectar overlap real.
- `find_next_available` (`scheduler.py:195`) barre la ventana temporal en pasos de 30 min creando eventos provisionales hasta encontrar un hueco válido.
- `resource_usage_intervals` (`scheduler.py:41`) implementa un barrido de eventos tipo *sweep line* (apertura +qty / cierre −qty) para reportar cuántas unidades de un recurso están en uso en cada segmento.

Persistencia: escritura atómica de eventos (`save_events`, `scheduler.py:224`, con `.tmp` + `os.replace`) e inventario (`inventory_store.py`). Los datos vivos se guardan en `~/.hotel_planner/data.json` (`app.py:49`).

## Dimensión 2 — Organización del código

**Muy por encima de lo esperado en 1er año.** 28 módulos, 5.429 líneas, con una separación MVC genuina:

- **Modelo:** `models/` (resource, event, inventory, stores). Polimorfismo real con `Room`/`Employee`/`Item` sobre `Resource`, cada uno con su `to_dict()` extendiendo el del padre (`resource.py:65-139`).
- **Núcleo:** `core/scheduler.py` — índices en memoria (`events_sorted`, `name_to_event`, `resource_index`) mantenidos ordenados con `bisect` (`scheduler.py:143`).
- **Controlador:** `ui/controller.py` — fachada delgada entre vista y backend, con `threading.Lock` y variantes async (`save_state_async`/`load_state_async`, `controller.py:288-303`).
- **Vista:** `ui/screens/` + `ui/components/` — pantallas y widgets reutilizables (tarjetas, toolbars, dashboards).

Nombres claros, docstrings en casi todo, tipado con `typing` en el controlador. La separación es lo bastante limpia como para que **el backend sea probable sin la GUI** — lo cual verifiqué. Fortaleza destacada.

Debilidad menor: hay código de carga de eventos triplicado (`load_events` y `load_events_from_list` en `scheduler.py:247-341` comparten casi todo el cuerpo; podrían compartir un helper). Y `app.py` mezcla varias rutas de carga (`app.py:48-110`, `241-290`) con `print("DEBUG ...")` de depuración que quedaron en el código.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí el backend con los datos reales del repo. Lo observado:

1. **`py_compile` de los 28 módulos: OK.** Todos compilan sin error.
2. **Suite de tests del estudiante (`tests/test_constraints.py`): 5 passed en 0.07s.** Cubren co-requisitos (éxito y fallo), exclusión mutua por nombre y por categoría, y co-requisitos múltiples. Tests reales, bien escritos.
3. **Carga de datos real:** 81 recursos y 9 eventos cargados sin duplicados ni tracebacks.
4. **Programación válida** (recurso libre + co-requisitos satisfechos): `add_event('Fiesta A', ..., [Zona de Piscina + salvavidas + chaleco salvavidas])` → `(True, None)`. Correcto.
5. **Rechazo de sobre-reserva** (misma sala q=1, intervalo solapado): → `(False, "El recurso 'zona de piscina' no tiene suficiente disponibilidad (libres: 0)")`. Correcto.
6. **No-solapamiento permitido:** mismo recurso, franja horaria distinta → `(True, None)`. La lógica de intervalos distingue bien solapamiento de contigüidad.
7. **Límite de cantidad:** `Camarero` (q=12): pedir 15 → rechazado con `(libres: 12)`; pedir 12 → `(True, None)`. Conteo exacto.
8. **Restricciones co-requisito:** pedir `Zona de Piscina` sin sus requeridos → `(False, {'constraint_error': {'missing_requires': {'Zona de Piscina': ['chaleco salvavidas', 'salvavidas']}}})`. Correcto y con detalle útil para la UI.
9. **`find_next_available`:** con dos eventos ocupando 10-12 y 14-16, pedir un hueco de 1h devolvió `(2026-03-01 12:00, 13:00)` — exactamente el hueco libre entre ambos. Correcto.
10. **Round-trip de persistencia:** `save_events` → `load_events(validate=False)` reconstruye la lista de eventos idéntica. Correcto.
11. **Entradas inválidas manejadas sin traceback:** JSON corrupto → `(False, 'Error reading JSON: ...')`; archivo inexistente → `(False, 'File not found: ...')`; `end <= start` en el constructor de `Event` → `ValueError` con mensaje claro; recurso inexistente → `(False, "El recurso 'unicornio' no existe en el inventario")`. Robusto.

**Bug real encontrado — Controller reporta fallo en operaciones que sí funcionan.** `Inventory.add_resource` (`inventory.py:22-38`) **no tiene `return`**, devuelve `None`. Pero el controlador asume que devuelve el recurso añadido: `added = self.scheduler.inventory.add_resource(room); return (True, added.to_dict())` (`controller.py:93-94`, y análogamente en `add_employee` `controller.py:122` y `add_item` `controller.py:150`). Resultado verificado ejecutando: `ctrl.add_room('Sala Test', capacity=50)` → `(False, "'NoneType' object has no attribute 'to_dict'")`, **pero el recurso SÍ se añade** (el inventario pasa de 81 a 82 y `find_by_name('Sala Test')` lo encuentra). Es decir: la operación tiene éxito de facto pero el controlador informa error, así que la UI mostraría un mensaje de fallo engañoso al añadir cualquier recurso. Impacto real en el flujo "Añadir Recurso".

**Segundo defecto lógico — `Inventory.add_resource` duplica al fusionar.** Cuando ya existe un recurso con el mismo nombre, el método fusiona la cantidad en la entrada existente (`inventory.py:27-37`) **pero luego igualmente hace `self.resources.append(resource)`** (`inventory.py:38`), dejando **dos** entradas con el mismo nombre. Verificado: añadir `Item('Silla', 10)` y luego `Item('Silla', 5)` deja dos filas (qty 15 y qty 5). `find_by_name` enmascara el problema (devuelve la primera), pero `get_resources_by_category`, `to_dict()` y el contador de la vista de inventario contarían la silla dos veces. En la práctica no dispara con los datos precargados (nombres únicos), por eso no se ve al arrancar, pero es una inconsistencia latente. El `append` debería estar dentro de un `else`.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Nivel notablemente alto para 1er año:
- Uso idiomático de `bisect` para inserción ordenada, `defaultdict`, `pathlib.Path`, `datetime`/`timedelta`, properties con validación (`resource.py:51-59`).
- Serialización `to_dict`/`from_dict` bien encapsulada.
- Escritura atómica de ficheros — patrón maduro poco común en un principiante.
- Manejo de errores con `try/except` y devolución de `(ok, motivo)` en vez de dejar propagar excepciones a la UI.

Mejorables menores:
- Varios `except Exception: pass` silenciosos (`scheduler.py:118-120`, `inventory.py:29-37`) que tragan errores; en depuración conviene al menos loguear.
- Los `print("DEBUG ...")` de `app.py` deberían quitarse o pasar a `logging`.
- Duplicación entre `load_events` y `load_events_from_list`.

## Dimensión 5 — Datos y persistencia

Modelo de datos bien pensado. El JSON separa `inventory.resources` y `events` (esquema anidado en `default_data.json`), y `app.py:65,70` lo reempaqueta al formato plano que esperan los stores — un puente correcto, aunque un poco frágil por acoplar el conocimiento del esquema a la capa de vista en vez de a un loader unificado. La escritura atómica (`.tmp` + `os.replace`) es un acierto real. `Event.to_dict`/`from_dict` e `Inventory.load_from_file` (con `TYPE_MAP` para reconstruir la subclase correcta, `inventory.py:10-14,93-122`) están bien resueltos.

## Dimensión 6 — Informe (`report.md`)

2.797 palabras, muy completo y en general **honesto**. Coincide con el código en lo esencial: MVC (real), escritura atómica (real, `scheduler.py:238-242`), eventos virtuales de Tkinter para sincronizar vistas, `~/.hotel_planner/data.json` como ruta de persistencia (`app.py:49`, exacto), tests del scheduler (existen y pasan). El estudiante es transparente sobre limitaciones (p.ej. que los eventos solo se crean/eliminan, no se editan — sección 7).

Discrepancias / matices a señalar:
- **Recurrencia sobrevalorada.** El informe la lista como funcionalidad ("ninguna, diaria, semanal, mensual, estacional, personalizada", sección 2.2). En el código la recurrencia es **solo un campo de metadato almacenado** (`event.py:22,119`); el scheduler **no expande** eventos recurrentes — lo dice él mismo en dos comentarios: `Nota: no gestiona recurrencias` (`scheduler.py:45`) y `TODO: añadir soporte para recurrencias` (`scheduler.py:92`). Un evento "diario" solo ocupa su intervalo único. El informe no aclara que la recurrencia es cosmética/no aplicada.
- El informe no menciona el bug del controlador al añadir recursos (`controller.py:93-94`) ni la duplicación en `inventory.py:38` — lógico, probablemente no los detectó porque la GUI y los tests no ejercitan ese camino exacto.
- Menciones de "accesibilidad WCAG", "responsividad" y "optimización de rendimiento" (secciones 2.5, 4.6) son afirmaciones de UI que no pude verificar sin display; no las tomo como demostradas, pero tampoco como falsas.

En conjunto el informe **describe con fidelidad** lo que el código hace, con la salvedad de la recurrencia.

---

## Valoración global (orientativa, sin nota numérica)

Trabajo **excepcional para primer año**. La ambición (GUI CustomTkinter, MVC genuino, 28 módulos), la calidad del núcleo de planificación (índices ordenados, conteo por intervalos, sweep line, búsqueda de huecos) y la existencia de tests unitarios propios que pasan colocan este proyecto muy por encima de la media. Ejecuté el backend con datos reales y el motor de scheduling es **correcto** en todos los flujos que probé: programación válida, rechazo de sobre-reserva, límites de cantidad, co-requisitos, no-solapamiento, gap-finding, persistencia round-trip y manejo de entradas inválidas sin reventar. La GUI no arrancó solo por falta de X11 en el entorno, no por fallo del código.

Los dos defectos encontrados son reales pero acotados: el del controlador (`add_resource` devuelve `None` → la UI reporta error al añadir un recurso que sí se añade) sí afecta el uso real y merece corregirse; el de duplicación en `Inventory.add_resource` es latente (no se dispara con nombres únicos). Ninguno invalida el núcleo.

- **Principal fortaleza:** motor de planificación correcto y bien diseñado (validación de disponibilidad por intervalos, restricciones co-requisito/exclusión, búsqueda de huecos), respaldado por una arquitectura MVC limpia y tests propios que pasan.
- **Principal área de mejora:** corregir el contrato de `Inventory.add_resource` (que devuelva el recurso y no duplique al fusionar) para que el controlador y la vista de "Añadir Recurso" funcionen sin falso error; y ser preciso en el informe sobre que la recurrencia se almacena pero no se aplica.
