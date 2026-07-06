# Reporte detallado — Issue #238: SmartSport Scheduler

- **Estudiante:** Gerardo Javier Pujol Suárez
- **Grupo:** C-122
- **Repositorio:** https://github.com/AtlasOffMind/SmartSport-Scheduler
- **Descripción (issue):** gestionar un centro deportivo con espacios, implementos deportivos y personal que se pueden alquilar para planificar eventos (partidos, entrenamientos, torneos).

## Resumen de ejecución

- **Clonado:** OK (depth 1).
- **Punto de entrada:** `main.py:24` → construye una GUI de **Tkinter** (`Frontend/gui.py:PlannerGUI`). **No es una app de consola** — no hay menú por `input()`; toda la interacción es por ventana.
- **Dependencias:** ninguna externa; solo la librería estándar (`tkinter`, `json`, `datetime`, `calendar`). No hay `requirements.txt` ni `pyproject.toml`.
- **Python del sistema (3.14):** sin módulo `tkinter`. Usé el intérprete de `uv` `cpython-3.13.1` (tiene `tkinter`).
- **GUI headful:** el host tiene `DISPLAY=:0`, pero al instanciar `PlannerGUI` sobre el display en vivo de Alex Tk aborta con un error de threading de X11 (`[xcb] append_pending_request assertion`) — **problema del entorno de revisión (display compartido, sin Xvfb), no del código**. No hay Xvfb instalado para un display aislado.
- **Estrategia de verificación:** al no poder recorrer la ventana, ejercité **directamente la lógica de negocio** del backend (donde vive toda la corrección) con un script de humo, cargué los JSON reales incluidos y corrí la suite de tests del repo.

### Qué se ejecutó realmente y qué se observó

Script `smoke_backend.py` contra `backend/` (todo con `cpython-3.13.1`):

| Escenario | Resultado observado |
|---|---|
| `create_planner()` | 51 recursos cargados, 0 eventos. OK (`backend/utils/utils.py:6`) |
| Añadir evento válido (Cancha de Tenis + co-requisitos) | Añadido OK (`planner.py:add_events`) |
| Evento solapado que excede capacidad (Cancha=1) | Rechazado con `ValueError: That's not a valid event`. Correcto |
| Nombre duplicado | Lanza `DecisionRequired: This event already exist`. Correcto (`planner.py:227`) |
| Evento sin co-requisito (Pelota de Tenis sin Cancha/Raquetas) | Rechazado. Correcto (`requires` aplicado) |
| Evento fuera de horario (05:00, antes de 7am) | Rechazado. Correcto (`is_valid` valida 7:00–22:00) |
| `remove_event` de evento inexistente | Lanza `Exception: This event:'NoExiste' dosen't exist`. Correcto |
| `excludes` (Piscina Olímpica + Sacos de Boxeo) | `is_valid` → False. Exclusión respetada |
| `events_table()` | Renderiza tabla ASCII alineada. OK |
| `find_next_slot_step()` | Devuelve `(2025-12-28 12:00, 13:00)`. Funciona pero con **fecha hardcodeada** (ver Corrección) |
| `save_planner` / `load_planner` (ida y vuelta a `/tmp`) | Roundtrip OK: 51 recursos + 1 evento persistidos y recargados |
| Cargar `data/planner.json`, `data/New Try.json`, `data/Copia planner.json` | Los tres cargan OK (51 recursos; 3, 1 y 3 eventos respectivamente) |

**Conclusión de ejecución:** el motor de dominio (validación de horario, capacidad, co-requisitos, exclusiones, persistencia JSON) **funciona correctamente** en todos los caminos que probé. La GUI no pude recorrerla por limitación del display de revisión, pero su código de construcción importa sin errores (`from Frontend import PlannerGUI` → "imports OK").

---

## 1. Qué hace el programa

Aplicación de escritorio con interfaz gráfica (Tkinter) para **planificar el alquiler de un centro deportivo**. El dominio se modela con tres entidades: `Resource` (espacio, implemento o personal, con `name` y `amount` disponible), `Event` (actividad con nombre, `start`/`end` y el diccionario de recursos que consume) y `Planner` (motor que mantiene el inventario global de 51 recursos predefinidos, los eventos programados y las reglas `requires`/`excludes`). El usuario abre `main.py`, ve una lista de eventos, un calendario mensual con días coloreados (rojo=ocupado, verde=libre, azul=hoy) y una barra de botones para añadir/eliminar/ver eventos, guardar/cargar y buscar el próximo hueco libre. Al crear un evento se encadenan diálogos modales: nombre → fecha/hora inicio → fecha/hora fin → selección de recursos → resolución de co-requisitos obligatorios → cantidades. El motor valida cada evento en cuatro capas (temporal, disponibilidad, co-requisitos, exclusiones) antes de aceptarlo, y persiste el estado a JSON en `data/`.

Es un proyecto de **alcance claramente superior a la media de primer año**: separación backend/frontend en paquetes, 51 recursos con un grafo real de co-requisitos y exclusiones, calendario propio y persistencia atómica.

## 2. Organización del código

Excelente para el nivel. El código está dividido en paquetes con responsabilidades claras:
- `backend/models/` — clases de datos puras: `Resource` (`resource.py`), `Event` (`event.py`), `DecisionRequired` (`exceptions.py`) y el motor `Planner` (`planner.py`).
- `backend/utils/` — `create_planner()` con el catálogo de recursos (`utils.py`) y `save/load_planner` (`persistence.py`).
- `Frontend/gui.py` — la ventana principal `PlannerGUI`.
- `Frontend/dialogs/` — cinco diálogos modales reutilizables, uno por archivo.
- Los `__init__.py` reexportan símbolos con `__all__` explícito, lo que es muy pulcro (`backend/__init__.py:1`).

Uso apropiado de `@dataclass` para las entidades. Nombres de métodos mayormente claros (`is_valid`, `add_events`, `find_next_slot_step`). El uso de clases donde el dominio lo pide es correcto y natural.

Puntos menores: el paquete se llama `Frontend` con mayúscula (los demás en minúscula — inconsistencia de convención); hay comentarios `# todo` y `# Aprobado x Chayanne` dispersos (`gui.py:41`, `gui.py:101`) que deberían limpiarse antes de entregar; `add_events` es plural pero añade un solo evento.

## 3. Corrección funcional (ejecución real)

Ver la tabla de arriba. El backend **pasa todos los caminos probados**. Bugs y fragilidades detectados al leer y ejecutar:

- **`load_planner(None)` revienta.** `persistence.py:load_planner` hace `p = Path(path)` sin comprobar `None`. Confirmado en ejecución: `TypeError: argument should be a str ... not 'NoneType'`. En la GUI está cubierto porque `load_planner` recibe siempre `path.name` (`gui.py:249`), pero la firma admite `None` y ahí falla. Sugerencia: `if path is None: raise ValueError(...)` o resolver a la ruta por defecto como hace `save_planner`.
- **`find_next_slot_step` con fecha hardcodeada** (`planner.py:280`: `start_point = datetime(2025, 12, 28, 10, 0)`). Solo busca hueco ese día concreto; no parte de "hoy" ni recibe la fecha. Devuelve un slot correcto para ese día, pero la función no generaliza. El propio autor dejó un `# TODO` reconociéndolo (`planner.py:315`).
- **`is_valid` puede lanzar `AttributeError` con recurso desconocido.** En la rama de capacidad, `avail = self._resources.get(name).amount` (`planner.py:172`) haría `None.amount` si el nombre no estuviera en inventario. En la práctica el bucle previo ya rechaza recursos desconocidos (`planner.py:145`), así que no se alcanza — pero es una llamada frágil que convendría blindar.
- **Reglas `requires`/`excludes` asimétricas.** Se comprueban en una sola dirección (si pides A, ¿está B?), no bidireccional. Para el catálogo actual funciona; es una limitación de diseño, no un fallo de ejecución.
- **Validación de entradas de usuario:** el diálogo de fecha (`multi_input_dialog.py:_on_ok`) captura `ValueError` por campo y muestra los errores acumulados; fechas imposibles (ej. 31 de febrero) las atrapa el `datetime(...)`. Buen manejo básico. Nota: los `errors.append(ve)` guardan el objeto excepción, no `str(ve)`, así que el mensaje mostrado puede quedar algo técnico.

No observé ningún `Traceback` en los caminos normales del backend; los únicos crashes son los provocados adrede arriba.

## 4. Buenas prácticas de Python (nivel principiante)

Muy por encima del promedio: indentación consistente, f-strings, `@dataclass`, comprehensions legibles, `try/except` donde corresponde (diálogos, guardado/carga), escritura atómica con archivo `.tmp` (`persistence.py:save_planner`). Type hints presentes en muchas firmas (no exigidos, es un plus).

A mejorar: `except AttributeError as ex: pass` silencioso en `load_planner` de la GUI (`gui.py:250`) — traga errores sin avisar; `except Exception` genérico en varios sitios; los comentarios de trabajo (`# todo`, `# Aprobado x Chayanne`) deberían salir de la entrega final. `add_resources` puede devolver `None` (`gui.py:177`) que luego se pasa a `Event(...)` — camino frágil si el usuario cancela el selector.

## 5. Datos y persistencia

Correcta. `save_planner` serializa a JSON con escritura atómica (temporal + `replace`), `datetime` con `isoformat`, y `ensure_ascii=False` para acentos. `load_planner` reconstruye `Resource`/`Event` desde el dict. Verificado en ejecución: roundtrip fiel y los tres JSON incluidos en `data/` cargan sin problema. Estructuras (`dict[str, Resource]`) razonables para el dominio.

## 6. Informe (`README.md`)

Informe **extenso y en general fiel al código**: describe correctamente las clases, los 4 niveles de validación, la estructura de carpetas y los métodos clave, e incluye una honesta "historia" del desarrollo (reconoce apoyo fuerte de IA en el Frontend por no conocer Tkinter, y una idea abandonada de versión Django). Discrepancias a señalar:

- **Sobreestima con el `.exe`:** dice "Descarga `SmartSport Scheduler.exe` de la carpeta `dist/`" (README línea ~18), pero **no existe `dist/` ni ningún `.exe`** en el repo. `EJECUTABLE_README.md` refuerza esa afirmación. Feature anunciada que no está.
- **Tests presentados como funcionalidad**, pero `tests/test_planner.py` está **roto**: importa `from Proyecto import ...` desde una carpeta `Scripts/` que ya no existe (estructura antigua). Al correrlo: `ModuleNotFoundError: No module named 'Proyecto'`. Además las firmas usadas (`save_planner(p, None)`, `get_default_data_path()` sin args) no coinciden con el código actual.
- Quedan restos de esqueleto de redacción sin desarrollar (una lista "- como lo hice / - concepto de eventos ..." al inicio).

Fuera de eso, el README no inventa capacidades del motor: lo que describe de validación y persistencia **sí está y funciona**.

---

## Veredicto interno

Trabajo **sobresaliente para primer año**: arquitectura modular real, dominio rico, motor de validación multicapa que funciona en todos los caminos probados, GUI completa y persistencia atómica. Debilidades: un par de bugs latentes (`load_planner(None)`, fecha hardcodeada en `find_next_slot_step`), tests rotos, y un README que sobrevende un `.exe` inexistente y unos tests que no corren. Nada de esto empaña un proyecto claramente por encima del nivel esperado.
