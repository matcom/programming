# Reporte detallado — Issue #239 · Event_Planificator

- **Estudiante:** Dayan Rodríguez Pérez
- **Grupo:** C-121
- **Repositorio:** https://github.com/Daroz73/Event_Planificator
- **Clonado en:** `/home/apiad/Workspace/.playground/proyecto1-eval/repos/239-event_planificator`
- **Tipo de proyecto:** Aplicación de escritorio con GUI (Flet), **no** consola.

---

## Resumen de ejecución

- **Entorno:** `uv venv` con Python 3.13.1; `flet==0.28.3` + `flet-web==0.28.3`
  instalados de forma aislada (el `flet-desktop` del `requirements.txt` no es
  usable headless, así que se arrancó en modo web browser).
- **Arranque:** el servidor Flet levantó correctamente en `http://localhost:8555`
  y respondió `HTTP 200` sirviendo la app-shell. `Domain()` cargó los 3 JSON sin
  error (1 evento, 18 trabajadores, 20 recursos) y el hilo de fondo
  `backgound_update` (`main.py:14`) arrancó sin lanzar excepción.
- Como el cliente es Flutter/web, la interacción real con los widgets no se puede
  ejercitar por HTTP. Por eso además se ejercitó **directamente la lógica de
  negocio** que disparan los botones, contra una copia temporal de los datos.

**Resultados de los caminos ejercitados:**

| Camino | Resultado |
|---|---|
| Boot del servidor Flet | ✅ HTTP 200, sin Traceback |
| `Domain()` + carga de los 3 JSON | ✅ (deserializa fechas ISO y entidades) |
| `find_next_avialable_slot()` | ✅ devuelve `(2027-06-14 01:43, 02:43)`, evitando el evento e1 |
| `show_details("e1")` | ✅ texto formateado correcto |
| `update_events()` (hilo de fondo) | ✅ sin error |
| `save_pending()` (crear + guardar evento) | ✅ persiste, JSON pasa de 1→3 eventos |
| `Events_Planificator.add_resource` / `Agg_Event` | ✅ asigna `w1`/`r1` al evento |
| **Crear Worker/Resource con `co_requested` = "laptop"** | ❌ **KeyError** (ver Dimensión 3) |

---

## Dimensión 1 — Qué hace el programa

Sistema de planificación de eventos hospitalarios con interfaz gráfica Flet
(`main.py:83` → `ft.app(main)`). El `AppBar` (`main.py:31-80`) ofrece tres menús —
Events, Workers, Resources — cada uno con crear y visualizar. El dominio
(`core/domain.py`) mantiene listas en memoria de `Event`, `Worker` y `Resource`,
con persistencia en tres JSON (`data/events.json`, `personal.json`,
`resources.json`) vía `Data_saved_loader`. Un hilo daemon
(`main.py:14-21`, `backgound_update`) llama a `Domain.update_events()` cada 60 s
para borrar automáticamente eventos ya finalizados.

La pieza central es `find_next_avialable_slot` (`core/domain.py:394-420`): dado un
diccionario de personal, uno de recursos y una duración, avanza en pasos de 15
minutos desde `datetime.now()` hasta hallar un intervalo sin solapamiento de
recursos/personal. El planificador de asignación vive en
`core/events_planificator.py` y valida restricciones de dominio (especialista
encargado presente, co-requisitos, cantidades disponibles) antes de agendar.

## Dimensión 2 — Organización del código

**Muy por encima del nivel esperado en 1er año.** El código está modularizado en
dos paquetes (`core/` para lógica, `GUI/` para presentación) con separación de
responsabilidades real:

- Entidades como `@dataclass`: `Resource` (`core/resource.py:4`), `Worker`
  hereda de `Resource` (`core/worker.py:5`), `Event` (`core/events.py:5`). La
  herencia Worker←Resource es una decisión de diseño defendible ("un trabajador
  es un recurso humano").
- `Domain` (`core/domain.py:11`) como fachada central; `Events_Planificator` y
  `Data_saved_loader` como colaboradores especializados.
- La GUI está factorizada en `Create_Utils`, `Visual_Utils`, `Creation_Validate`,
  `Delete_Utils`, con helpers reutilizables (`_card` en `visual_utils.py:383`,
  `_agg_row` genérico y recursivo en `create_utils.py:544`).
- Nombres en general claros y comentarios en español abundantes.

Detalles menores: hay clases con métodos casi todos `@staticmethod` que
funcionan más como namespaces que como objetos con estado (patrón válido pero
podría ser módulos de funciones). Typo `slef` en la firma de
`sorted_events_by_begin` (`core/domain.py:149`) — funciona por posición pero es
confuso.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Lo que **funciona** al ejecutar (ver tabla arriba): boot, carga/persistencia
JSON, búsqueda de hueco, detalles de evento, planificación y guardado de eventos.
El round-trip de serialización id-only ↔ reconstrucción de relaciones
(`rebuild_relations`, `core/domain.py:31`) es correcto y es el logro técnico más
serio del proyecto.

**Bug confirmado por ejecución — KeyError al crear Worker/Resource:**
`_create_worker` (`create_utils.py:513`) y `_create_resource`
(`create_utils.py:532`) validan primero el campo contra `dom.get_resources()`
(57 nombres) y luego indexan `dom.get_specialities_by_resource()[co_requested]`
(mapa de 53 claves). **4 recursos válidos no son claves del mapa** —
`laptop`, `collarín cervical`, `equipos de visualización de venas`,
`máquinas de diálisis` — así que un input aceptado como válido lanza
`KeyError('laptop')`. Esto no es teórico: el worker `w3` de los datos de ejemplo
usa exactamente `co_requested: "laptop"`, o sea que recrear un trabajador
realista rompe la app. Reproducido directamente:

```
REPRODUCED KeyError in _create_worker path: KeyError('laptop')
```

**Sugerencia:** usar `dom.get_specialities_by_resource().get(co_requested, {"all"})`
o sincronizar ambos conjuntos (que `get_resources()` y las claves del mapa
coincidan). Es un fallo de un solo carácter conceptual (`[]` vs `.get`).

**Otros defectos detectados por lectura (código muerto o secundario, no rompen el
flujo principal):**

- `Resource._show_details` (`core/resource.py:26`): `details += fields[k]` añade
  el objeto lambda en vez de llamarlo (`fields[k]()`), y `_show_use_plan`
  (`core/resource.py:30`) retorna una **lista** en vez de un `str`. `View Use
  Plan` con argumentos concretos daría salida corrupta; sin argumentos el camino
  de `Event.show_details` no llega aquí, por eso no se disparó en la prueba.
- `Domain._overlaps` (`core/domain.py:424`) declarada sin `self` pero llamada como
  método (`self._overlaps(...)` en `_is_valid_slot`, línea 432) → sería
  `TypeError`. Pero `_is_valid_slot`/`_overlaps` son **código muerto**: ningún
  botón los invoca; el slot activo es `find_next_avialable_slot`, que sí sirve.
- `Data_saved_loader.pop_` (`data_saved_loader.py:55`) llama `elements.pop_()`
  (método inexistente en `list`) — también sin uso.
- `_safe_parse_date` (`data_saved_loader.py:151`) usa `default_date=datetime.now()`
  evaluado una sola vez al definir (antipatrón de default mutable), aunque el
  parámetro nunca se usa dentro.

Validación de entrada del usuario: **sí la hay y es cuidadosa** —
`validate_date` (`creation_validate.py:13`) con try/except, rango begin<end
(`create_utils.py:482`), disponibilidad de personal/recursos
(`create_utils.py:485-494`), y un "menú inteligente" de autocompletado
(`filter_options`, `creation_validate.py:55`). El único hueco es el KeyError de
arriba.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy sólido para el nivel: `@dataclass`, herencia, `field(default_factory=list)`,
f-strings, comprehensions, `next(... , None)`, `set` para lookups rápidos
(`ids_generator`, `core/domain.py:100`), separación de copias al iterar mientras
se borra (`update_events`, comentario en `core/domain.py:48`). Manejo de errores
con try/except donde toca (parseo de fechas, `append_` con `FileNotFoundError`).

A mejorar: nombres inconsistentes de idioma/casing (`Agg_Event`, `_agg_row`,
`backgound_update` con typo); algún `print(specialities)` de depuración olvidado
(`create_utils.py:810`); métodos estáticos con firma sin `self`/`slef`. Nada de
esto es penalizable en 1er año.

## Dimensión 5 — Datos y persistencia

Bien resuelto y es donde el estudiante claramente más aprendió. El problema de
**referencias circulares** al serializar objetos con relaciones bidireccionales
(Event↔Worker↔Event) se resolvió guardando **solo ids** y reconstruyendo el grafo
en memoria al cargar (`rebuild_relations`, `core/domain.py:31-42`). El round-trip
se verificó ejecutando `save_pending()`: el JSON pasó de 1 a 3 eventos
correctamente. Estructuras de datos razonables (`dict` para cantidades, `set`
para ids-a-borrar). Un patrón de "lista pendiente + guardar en lote" para no
reescribir el JSON completo en cada operación (`pending_events`, `save_pending`).

## Dimensión 6 — Informe (`report.md`)

Informe extenso, honesto y **bien alineado con el código**. Describe correctamente
la arquitectura modular, el algoritmo de `find_next_avialable_slot` (con el código
real inline), y dedica una sección larga y acertada al problema de referencias
circulares y su solución con ids — que es exactamente lo que hace el código. No se
detectó sobreestimación grave: las features que anuncia (crear/visualizar/eliminar,
JSON, autocompletado, búsqueda de hueco, borrado automático de eventos vencidos)
existen todas. Única discrepancia: el informe/README venden el sistema como
"funcional", y lo es en su flujo feliz, pero el KeyError al crear
worker/resource con ciertos recursos válidos queda sin mencionar. El README dice
"aplicación de consola" implícitamente en la rúbrica pero el proyecto es GUI —
correctamente documentado como Flet.

---

## Valoración global

Trabajo **muy fuerte para primer año**: dos paquetes modularizados, herencia con
sentido de dominio, GUI reactiva no trivial en Flet, persistencia JSON con
resolución real del problema de referencias circulares, y un algoritmo de
búsqueda de huecos que funciona. El principal defecto es un `KeyError` concreto y
fácil de arreglar en el alta de trabajadores/recursos, más varios métodos muertos
con bugs latentes que convendría limpiar. La ambición y la ejecución están muy por
encima de lo esperado.
