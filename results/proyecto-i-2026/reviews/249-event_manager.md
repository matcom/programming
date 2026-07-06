# Reporte de Evaluación — Proyecto I

- **Issue:** #249
- **Repositorio:** https://github.com/AlvaroBesadaFerrer/Event_manager
- **Estudiante:** Alvaro Vladimir Besada Ferrer
- **Grupo:** C111
- **Descripción declarada:** "Planificador de eventos para un taller mecánico, con adición de eventos automáticos y con IA".

---

## Resumen de ejecución

Clonado con éxito. La aplicación **no es de consola**: es una app web **Streamlit** multipágina
(`main.py` + `pages/`). Se detectó el punto de entrada correcto (`streamlit run main.py`) y se
levantó un entorno aislado con `uv` (Python 3.12) instalando `requirements.txt`.

Lo que se corrió y observó:

1. **Arranque del servidor** — `streamlit run main.py --server.headless true` arrancó sin errores
   de import; `/_stcore/health` devolvió `ok` y la raíz HTTP devolvió `200`.
2. **Página principal (`main.py`)** — vía el harness `AppTest` de Streamlit renderiza la línea de
   tiempo con los 11 eventos de ejemplo **sin excepción**, siempre que exista `pkg_resources`
   (ver hallazgo de dependencias abajo).
3. **Página "Agregar evento" (`pages/1_Agregar_evento.py`)** — renderiza sin excepción: toggle de
   planificador automático, 2 selectbox, 1 multiselect, 7 checkbox de herramientas y botón de envío.
4. **Página "Ver detalles por recurso"** — renderiza sin excepción.
5. **Lógica de negocio (probada directamente, sin widgets)** — se ejerció `schedule_event_helper`
   con 8 escenarios; **todos correctos** (ver "Corrección funcional").

No se pudo ejercer la página de IA (`3_Agregar_evento_con_AI.py`) end-to-end porque requiere
`GEMINI_API_KEY` y llamadas reales a la API de Google Gemini, que no puedo realizar. Se revisó por
lectura.

---

## 1. Qué hace el programa

Planificador de eventos para un **taller mecánico** de autos. El dominio (`domain/`) modela cuatro
tipos de recurso (`domain/resource.py:4-9`): áreas de trabajo, tipos de evento, trabajadores y
herramientas, con 26 recursos concretos en `domain/resources_data.py:4-31`. Un `Event`
(`domain/event.py:1-21`) agrupa un espacio, un tipo, trabajadores, herramientas y un intervalo
de tiempo. El motor central resuelve conflictos de intervalos (`Event.intersection`,
`domain/event.py:23-30`) y disponibilidad de recursos (`check_resources_availability`,
`domain/event.py:32-45`), y aplica restricciones configurables de co-requisito y exclusión mutua
(`domain/restrictions.py`).

El punto de entrada es `streamlit run main.py`. La interfaz es Streamlit multipágina: `main.py`
muestra la línea de tiempo y permite ver detalles / eliminar; `pages/1` agrega eventos (manual o
auto-agendado); `pages/2` muestra la agenda por recurso; `pages/3` agrega eventos por lenguaje
natural con Gemini. La persistencia es un archivo `event_data.json` (`json_storage/save_load_data.py`).

Cumple y **excede** la especificación (`Proyecto 1.md`): implementa el dominio pedido, los dos
tipos de restricción requeridos, la detección de colisiones por intervalos, y añade tres extras no
exigidos (GUI web, auto-planificador, IA).

## 2. Organización del código

Sobresaliente para primer año. El código está **modularizado en capas coherentes**, no en un
`main.py` gigante:

- `domain/` — modelo puro (eventos, recursos, restricciones). `Event`, `Resource`, `Restriction`
  con subclases `MutualExclusion`/`CoRequisite` (`domain/restrictions.py:13-38`).
- `schedule_events/` — validación y planificación desacopladas de la UI
  (`schedule.py`, `validators.py`, `scheduling_helper.py`).
- `gemini_scheduler/` — integración IA aislada.
- `json_storage/` + `utils/` — persistencia y utilidades (tiempo, color, filtros, formato).
- `pages/` + `main.py` — solo interfaz.

Uso correcto de clases donde el dominio lo pide (`Event`, `Resource`, jerarquía `Restriction`),
`Enum` para tipos de recurso (`domain/resource.py:4`), `__eq__`/`__repr__` en `Resource`
(`domain/resource.py:25-29`). Funciones pequeñas y con docstrings en español en casi todo el
código. Nombres claros y consistentes (`filter_resource_by_id`, `check_time_conflicts`,
`auto_schedule_event`). Las restricciones se generan por configuración declarativa
(`domain/restrictions_config.py` → `restrictions_data.py:7-31`), un patrón muy limpio que permite
cambiar reglas sin tocar lógica. Este nivel de separación y reutilización es atípicamente bueno
para un primer proyecto.

Observación menor: hay estado a nivel de módulo — `events = load_data()` en
`schedule_events/schedule.py:7` y `RESTRICTIONS = generate_restrictions()` en
`schedule_events/validators.py:5` se cargan al importar. Funciona en Streamlit (reejecuta el script
por interacción), pero es una variable global mutable que en otro contexto causaría estado obsoleto.

## 3. Corrección funcional (basada en ejecución real)

Se probó la lógica central llamando directamente a `schedule_event_helper` sobre una copia de
trabajo del JSON. Resultados observados:

| # | Escenario | Resultado observado | Correcto |
|---|-----------|---------------------|----------|
| 1 | Evento manual válido (event_8, area_1, tool_2, worker) | `errors == []`, guardado | ✅ |
| 2 | Co-requisito violado (event_1 sin Juan) | `['**Reparaciones eléctricas** necesita estar con **Juan** en el evento']` | ✅ |
| 3 | Fuera de horario (06:00) | `['Hora de inicio o de fin fuera del horario de trabajo ...']` | ✅ |
| 4 | Sin trabajadores | `['Debe seleccionar al menos un trabajador.']` | ✅ |
| 5 | Fin antes que inicio | `['La **hora de fin** debe ser posterior a la **hora de inicio**.']` | ✅ |
| 6 | Auto-agendar (duración 60') | `errors == []`, encontró hueco y persistió | ✅ |
| 7 | Dos eventos solapados (mismo área+worker+tool) | detecta los 3 conflictos (espacio, Frank, Caja de herramientas) | ✅ |
| 8 | Exclusión mutua (Sofía+Juan) | `['**Sofía** no puede estar en un evento con **Juan**']` | ✅ |

La persistencia funciona: tras varias inserciones `load_data()` devolvió el conteo esperado (12 vs 11
iniciales tras cada alta). El programa hace **exactamente** lo que declara el issue y el informe.
Validación de entradas: robusta a nivel de dominio (horario, trabajadores, orden, conflictos,
restricciones). `load_data` maneja `FileNotFoundError`/`JSONDecodeError` y tipos inesperados
(`json_storage/save_load_data.py:20-26`).

**Hallazgo de dependencias (no es culpa del estudiante, pero afecta a la ejecución en limpio):**
La página principal lanza `ModuleNotFoundError: No module named 'pkg_resources'` en
`main.py:33` (dentro de `st_timeline`) cuando el entorno tiene `setuptools>=81` o Python moderno
sin `setuptools`. El componente `streamlit-vis-timeline==0.3.0` hace `import pkg_resources`
internamente. Con `setuptools<81` instalado, la página **renderiza perfectamente sin excepción**.
`requirements.txt` no fija `setuptools`, así que en una instalación limpia reciente la línea de
tiempo podría romperse. Sugerencia: añadir `setuptools<81` (o migrar a otra versión del componente)
a `requirements.txt`.

**Bug potencial en el flujo de IA con hora específica (por lectura de código, no verificado en
ejecución por falta de API key):** en `gemini_scheduler/ai_validators.py:38-46`,
`validate_ai_response` deja `event_data["start_time"]` y `["end_time"]` como **objetos `datetime`**.
Luego `pages/3_Agregar_evento_con_AI.py:228` llama `to_object(event_data)`, y
`utils/save_load_utils.py:48` hace `str_to_datetime(data["start_time"])`, que en
`utils/time_utils.py:7` invoca `datetime.strptime(<datetime>, ...)` → lanzaría `TypeError`. Ese
error se captura en `save_load_utils.py:64-66` y devuelve `None`, con lo que el evento mostraría
"Error interno: No se pudo procesar el evento" en lugar de crearse. El camino de auto-agendado por
IA (solo duración, sin start/end) **no** cae en esto porque `start_time`/`end_time` quedan `None`.
Habría que confirmarlo con una API key; lo reporto como sospecha fundada en el código.

## 4. Buenas prácticas de Python (nivel principiante)

Muy por encima del nivel esperado:

- Legibilidad e indentación consistentes en todo el repo.
- Docstrings en español en casi todas las funciones.
- f-strings idiomáticas (`domain/event.py:36`, `schedule_events/validators.py`).
- `try/except` acotado y correcto donde toca (`json_storage/save_load_data.py:20`,
  `utils/save_load_utils.py:61-66`, y el manejo por tipo de error de la API Gemini en
  `pages/3_...py:252-269`).
- `Enum` para tipos, comprehensions limpias (`utils/filter_utils.py:3`, `:30`).
- Type hints presentes en varios módulos (`gemini_scheduler/ai_validators.py`,
  `domain/restrictions.py:9`) — no exigidos, pero suma.

Detalles menores: `check_auto_schedule` (`ai_validators.py:227-230`) devuelve el resultado de
`duration and not ...`, que puede ser un `int`/`None` en vez de un `bool` estricto (funciona por
truthiness, pero conviene envolver en `bool(...)`). Comentarios "TODO" quedaron al final de
`pages/1_Agregar_evento.py:93-95` (recordatorios de desarrollo); conviene limpiarlos.

## 5. Datos y persistencia

Correcta y bien pensada. Los eventos se serializan a `event_data.json` guardando **IDs** de recurso
en lugar de objetos completos (`utils/save_load_utils.py:14-25`), y se rehidratan resolviendo los
IDs contra el catálogo (`to_object`, `:41-66`). Estructuras de datos razonables (listas de objetos,
diccionarios de configuración). Fechas normalizadas con zona horaria `America/Havana`
(`utils/time_utils.py`). La carga es defensiva ante archivo ausente/corrupto. El formato del JSON
guardado coincide con el documentado en el informe.

## 6. Informe (`report.md`)

Excelente y **honesto**: 470 líneas que describen dominio, diseño por capas, recursos, ambos tipos
de restricción, las 8 validaciones, las funcionalidades, guía de uso con ejemplos, estructura de
carpetas, instalación, notas técnicas, y secciones reflexivas ("Qué aprendí", "Dificultades"). Todo
lo que afirma **coincide con el código** — no detecté sobreestimación. La descripción de las
restricciones por recurso (`report.md:83-124`) concuerda con `restrictions_config.py`. La única
omisión de matiz: el informe no menciona el requisito implícito de `setuptools<81` para la línea de
tiempo ni el posible fallo del flujo IA con hora específica, pero eso es esperable. El `README.md`
es más breve pero suficiente y consistente.

---

## Valoración general (interna)

Trabajo **excepcional para primer año**. Arquitectura por capas limpia, uso correcto de POO y
`Enum`, restricciones configurables declarativamente, motor de conflictos por intervalos que
funciona correctamente en las 8 pruebas ejecutadas, persistencia sensata, GUI web funcional e
integración de IA — muy por encima de lo pedido. La lógica central es sólida y está bien probada por
ejecución directa. Los únicos peros son un pin de dependencia faltante (`setuptools<81`, ajeno a la
lógica del estudiante) y un probable bug en el sub-camino IA-con-hora-específica que no pude
verificar sin API key. Principal fortaleza: diseño y corrección del motor de planificación.
Principal mejora: fijar dependencias reproducibles y revisar el tipo de `start_time`/`end_time` que
recibe `to_object` desde el flujo de IA.
