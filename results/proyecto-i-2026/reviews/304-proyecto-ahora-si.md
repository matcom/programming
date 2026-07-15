# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #304
- **Repositorio:** https://github.com/ianpuiglvlle/proyecto-ahora-si
- **Estudiante:** Ian Puig Levalle
- **Grupo:** C-121
- **Descripción declarada:** Organizador de Eventos sobre un estadio de fútbol con 2 campos principales, donde se reservan entrenamientos, partidos y torneos, usando Streamlit y Python.

---

## Nota metodológica importante

**No es una app de consola.** Es una aplicación web **Streamlit** (`main.py:1`, `streamlit run main.py`), sin `input()`. La verificación automática del bot falló solo porque no tenía Streamlit instalado (`ModuleNotFoundError: No module named 'streamlit'`), no por un error del código.

Adapté la ejecución en dos frentes:

1. **Lógica de negocio aislada de la GUI:** el diseño separa modelos (`models/`) y servicios (`servicios/`) de la interfaz (`interfaz*.py`), así que instancié directamente `Evento`, `EventoConRecursos`, `Calendario`, `GestorRecursos` y `GestorEventos` con datos reales, simulando `st.session_state` con un `dict` con acceso por atributo. Recorrí flujos completos de alta, detección de conflictos, persistencia y borrado.
2. **Arranque headless de la GUI:** `streamlit run main.py --server.headless true` levantó el servidor sin errores, sirvió `HTTP 200` en la raíz y `/_stcore/health` devolvió `ok`. El único "fallo" fue de mi entorno (matar el proceso con `timeout`/`pkill`), no del código.

Entorno: `uv venv --python 3.12`, `uv pip install streamlit pandas`. `py_compile` de los 10 módulos: **todos compilan sin error**.

## Dimensión 1 — Qué hace el programa

Aplicación web con menú lateral de 4 secciones (`main.py:58-69`):

- **Inicio** (`interfaz.py:296`): dos tablas semanales (pandas DataFrame) — una por Campo Principal — con los eventos de los próximos 7 días, indexadas por bloque horario (08:00–18:00 en saltos de 2h) y día de la semana (`models/calendario.py:119`).
- **Calendario** (`interfaz.py:265`): lista ordenada por fecha/hora de todos los eventos, con botón "Eliminar" por evento.
- **Reserva** (`interfaz.py:82`): formulario de alta. El usuario elige tipo, fecha, título, hora, campo y recursos adicionales; el sistema filtra campos y recursos ya ocupados, valida conflictos y restricciones antes de guardar.
- **Estadísticas** (`interfaz.py:241`): conteo por tipo de evento (`st.metric`) + botón "Limpiar todos los eventos".

El modelo de dominio es rico para 1er año: eventos con recursos, un gestor de recursos con **capacidad por recurso** (p.ej. 10 árbitros, 1 campo principal), **co-requisitos** (Campo Principal exige Árbitro + Marcador) y **exclusiones mutuas** (Campo Principal vs Auxiliar; Calentamiento vs Árbitro).

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** La separación en capas es real y coherente, poco común en 1er año:

- `models/` — entidades: `Evento`/`EventoConRecursos` (`models/evento.py`, herencia + `super()`), `Recurso` (`models/recursos.py`), `Restriccion` con subclases `RestriccionCoRequisito`/`RestriccionExclusion` (`models/restriccion.py`), y `Calendario` (`models/calendario.py`).
- `servicios/` — `GestorRecursos` (asignación/liberación/capacidad, `servicios/gestor_recursos.py`) y `GestorEventos` (validaciones de tipo, `servicios/gestor_eventos.py`).
- `utils/helpers.py` — serialización JSON y formateo de fechas.
- `interfaz.py` / `interfaz_recursos.py` — presentación Streamlit.

Buen uso de herencia (`EventoConRecursos(Evento)`, `models/evento.py:68`), polimorfismo en `Restriccion.validar()` (`models/restriccion.py:9,18,34`), type hints consistentes y nombres en español claros y descriptivos.

**Debilidades:**

- **Muerto/no usado:** `recursos.json` existe pero **ningún módulo lo lee** — los recursos y restricciones están *hardcodeados* en `cargar_recursos_por_defecto()` (`servicios/gestor_recursos.py:16`) y `cargar_restricciones_por_defecto()` (`:36`). El archivo de datos es decorativo.
- **Métodos no usados:** `buscar_hueco_disponible()` (`servicios/gestor_recursos.py:142`), `interfaz_recursos.agregar_recursos_a_interfaz()` (`interfaz_recursos.py:7`) y `validar_hora_torneo()` (`utils/helpers.py:18`) no se invocan desde ningún flujo activo.
- **Lógica de negocio dentro de la vista:** `obtener_campos_disponibles`, `obtener_recursos_sugeridos`, etc. (`interfaz.py:19-80`) mezclan reglas de disponibilidad con la capa de UI; encajarían mejor en `GestorRecursos`.
- **Efecto colateral a nivel de módulo:** `interfaz.py:15` ejecuta `st.session_state.get(...)` en el *import*, un patrón frágil que solo funciona porque `main.py` inicializa el estado antes de importar. Cualquier import fuera de ese orden rompería.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente lo **ejecuté**, no lo leí:

1. **Validación de eventos** (`models/evento.py:34`): Torneo en día laborable → `(False, 'Los torneos solo pueden ser los fines de semana')`; Torneo en sábado → `(True, 'Evento válido')`; título en blanco `'   '` → `(False, 'El título no puede estar vacío')`. Correcto.
2. **Horario máximo**: evento a las 18:00 +2h (fin 20:00) → válido; la regla es `hora + duracion > 20` (`models/evento.py:43`), así que 20:00 es el límite exacto permitido. Coherente.
3. **Co-requisito**: `CAMPO_PRINCIPAL_1` sin árbitro/marcador → `(False, "El recurso 'CAMPO_PRINCIPAL_1' requiere 'MARCADOR'")`; con `MARCADOR`+`ARBITRO` → `(True, 'Recursos asignados exitosamente')`. Correcto.
4. **Exclusión**: `CALENTAMIENTO`+`ARBITRO` → `(False, "Los recursos 'CALENTAMIENTO' y 'ARBITRO' no pueden usarse juntos")`. Correcto.
5. **Detección de conflictos** (`models/calendario.py:69`): dos eventos con `CAMPO_PRINCIPAL_1` en el mismo bloque → `tiene_conflicto = True`; el segundo en `CAMPO_PRINCIPAL_2` mismo bloque → `False` (correcto, campos distintos no chocan). Solapamiento por intervalos calculado bien (`:87`).
6. **Capacidad por recurso**: con `EQUIPO_MEDICO` (capacidad 1 en código) ya ocupado, un segundo evento en el mismo bloque → `(False, "Recurso 'Equipo Médico' no disponible a las 10:00 (capacidad: 1)")`. Correcto.
7. **Persistencia**: alta → escribe en `eventos.json` con recursos serializados; `contar_eventos_por_tipo()` → `{'Partido Amistoso': 2, ...}`; `eliminar_evento(0)` → `True`, JSON reducido de 2 a 1 elementos. Ciclo completo funciona.
8. **Arranque GUI headless**: servidor OK, `HTTP 200`, health `ok`. No hay error de código.

**Bug real encontrado (desincronización memoria/JSON):** en `Calendario.agregar_evento` (`models/calendario.py:55-66`) el evento se **agrega a `self.eventos` (`:60`) antes** de intentar guardarlo. Si `agregar_evento_json` rechaza el guardado (p.ej. duplicado exacto de título+fecha+hora, `utils/helpers.py:125-130`), la función devuelve `(False, "Error al guardar el evento en JSON")` **pero el evento ya quedó en la lista en memoria**. Lo reproduje: tras un rechazo de guardado, `len(cal.eventos) == 2` mientras `eventos.json` tiene 1 solo. El orden correcto sería guardar primero y solo hacer `append` si el guardado tuvo éxito. En la práctica esto rara vez se dispara porque `tiene_conflicto` suele frenar antes, pero el camino existe.

**Robustez ante basura:** `obtener_hora_numero('basura')` → `0` (por el `try/except` de `models/evento.py:16`), y `es_valido` de ese evento devuelve `True`. Es decir, una hora inválida no se detecta y se trata como las 0:00. La GUI evita esto ofreciendo horas de un `selectbox` cerrado, así que no es explotable por el usuario final, pero la validación del modelo no es defensiva.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Bien:** type hints, `Tuple[bool, str]` como convención de retorno validación+mensaje, `super().__init__`, `__eq__`/`__str__` definidos, uso de `set`/`dict` para recursos y asignaciones.
- **`except:` desnudos** en `models/evento.py:18,24` y `utils/helpers.py:22`. Preferible `except (ValueError, IndexError):` para no tragar errores inesperados.
- **`import` dentro de funciones** repetido (`models/calendario.py:76,121,213`, `main.py:20,30,42`). Para evitar ciclos con Streamlit puede justificarse, pero abunda; conviene consolidar imports arriba donde no haya ciclo.
- **Comentarios con emojis y numeración inconsistente** (`interfaz.py:205,208`) — inofensivo, pero ruido.
- **`print()` para logging** en `helpers.py` y `calendario.py:52`. En una app web no se ven; un mecanismo de error más visible (o `st.error`) sería mejor.

Todas son observaciones menores, esperables en 1er año.

## Dimensión 5 — Datos y persistencia

- Modelo en memoria: `Calendario.eventos: List[Evento]` + `GestorRecursos.asignaciones: Dict[str, Dict[(date, str), Set[str]]]` — estructura de asignaciones bien pensada (recurso → (fecha, bloque) → conjunto de títulos ocupantes).
- Serialización: `to_dict()` en `Evento`/`EventoConRecursos` con `fecha.isoformat()`; carga con `date.fromisoformat` (`models/calendario.py:21`). Idempotente en el round-trip (lo verifiqué escribiendo y releyendo `eventos.json`).
- **Inconsistencia de datos:** `EQUIPO_MEDICO` tiene **cantidad 1** en el código (`servicios/gestor_recursos.py:27`) pero **cantidad 2** en `recursos.json` y en el README. Como el JSON no se lee, gana el código: capacidad 1. Discrepancia real entre las tres fuentes.
- `agregar_evento_json` relee todo el archivo, deduplica y reescribe completo (`utils/helpers.py:97-133`) — O(n) por alta, aceptable a esta escala.
- El identificador de asignación usa `evento.titulo` como clave dentro del `Set` (`servicios/gestor_recursos.py:136`). **Dos eventos con el mismo título** en el mismo bloque colisionarían en el conjunto (se contaría como uno). No lo forcé a fallar, pero es una fragilidad de diseño: el título no es un identificador único.

## Dimensión 6 — Informe (`report.md`)

**No hay `report.md`.** El bot marcó ❌ correctamente. Existe un `README.txt` (no `.md`) que, aun así, es un informe **excelente y honesto**: describe archivo por archivo, el flujo de alta paso a paso, el modelo de datos, las reglas de negocio y una sección de "problemas conocidos". Lo verifiqué contra el código y coincide en lo esencial. Discrepancias detectadas:

- El README dice que `EQUIPO_MEDICO` tiene **cantidad 2** (tabla de recursos); el código usa **1** (`servicios/gestor_recursos.py:27`). Ver Dimensión 5.
- El README describe `recursos.json` como "configuración inicial" que define los recursos, dando a entender que la app la lee. En realidad **no se lee**: los recursos están hardcodeados. El README sobrevende ese archivo.
- El README menciona reversión transaccional ("si los recursos fueron asignados pero el calendario falló, los recursos se liberan"). Eso sí está implementado (`interfaz.py:235`), pero el bug de la Dimensión 3 muestra que la reversión de la *lista en memoria* no está cubierta.

No usa lenguaje de "demuestra"/"prueba" exagerado — el tono es descriptivo y sincero, incluyendo una sección de limitaciones. Muy bien para 1er año; solo penaliza que sea `.txt` y no `.md`, incumpliendo el requisito formal.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**. La arquitectura por capas (modelos / servicios / interfaz / utils), el uso genuino de herencia y polimorfismo, y un sistema de recursos con capacidad, co-requisitos y exclusiones están claramente por encima del promedio de 1er año. Todo lo que ejecuté funciona: validaciones, detección de conflictos por solapamiento y recurso compartido, capacidad, persistencia JSON y borrado. La GUI Streamlit arranca limpia. El README (pese a ser `.txt`) es de los informes más completos y honestos que se ven a este nivel.

Los defectos son reales pero acotados: un bug de desincronización memoria/JSON cuando el guardado se rechaza (`agregar_evento` agrega antes de guardar), `recursos.json` muerto con una capacidad de `EQUIPO_MEDICO` que contradice al código, el título usado como identificador único de recurso, y `except:` desnudos. Ninguno rompe el uso normal por la GUI, que restringe entradas.

- **Principal fortaleza:** diseño orientado a objetos con separación de responsabilidades real y un modelo de recursos (capacidad + restricciones) correcto y verificado en ejecución.
- **Principal área de mejora:** ordenar `agregar_evento` para persistir antes de mutar el estado en memoria (o revertir el `append` si el guardado falla), y decidir si `recursos.json` es la fuente de verdad (leerlo) o eliminarlo; hoy hay tres fuentes que discrepan.
