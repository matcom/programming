# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #250
- **Repositorio:** https://github.com/niitse34/CM-Chess-Club
- **Estudiante:** Leonardo Córdova Rosas
- **Grupo:** C122
- **Descripción del issue:** Planificador de eventos para un club de ajedrez, GUI en Streamlit, lógica de no solapamiento.
- **Fecha de evaluación:** 2026-07-06

---

## Resumen de ejecución (lo que realmente corrí)

Se creó un entorno aislado con `uv venv --python 3.12` y se instaló `streamlit`
(1.59.0). El proyecto **no es una app de consola**: es una app web Streamlit
(`gui.py` como punto de entrada, `run.sh` → `streamlit run gui.py`). Se ejecutó
de tres formas:

1. **`streamlit.testing.v1.AppTest`** recorriendo las 6 páginas del menú
   (`Events`, `Add Event`, `Find Slot`, `Resources`, `Save/Load`, `Settings`).
   **Todas renderizan sin excepción.**
2. **Ejercicio directo de la lógica de dominio** (`main.ChessClub`) con 12 casos
   de prueba cubriendo validaciones, solapamiento, piezas de repuesto y
   persistencia. **Todos se comportaron correctamente** (detalle abajo).
3. **Servidor Streamlit real** (`streamlit run gui.py --server.headless true`):
   arrancó limpio y respondió **HTTP 200** en la raíz.

Resultado de los 12 casos de lógica (todos correctos):

| # | Caso | Resultado observado |
|---|------|---------------------|
| 1 | friendly_match válido (board + pieces) | `(True, 'Scheduled: Amistosa')` |
| 2 | friendly_match sin piezas | `(False, 'Missing required resources ...')` |
| 3 | reloj en una clase (exclusión) | `(False, 'Clocks only for tournaments and team matches')` |
| 4 | `end <= start` | `(False, 'Invalid time: end must be after start')` |
| 5 | evento hoy (pasado) | `(False, 'Events can only be scheduled from tomorrow onwards')` |
| 6 | evento en día bloqueado (Monday) | `(False, 'Events cannot be scheduled on Mondays')` |
| 7 | clase de 30 min (< 1h mínimo) | `(False, 'class requires minimum 1h duration')` |
| 8 | doble reserva del mismo recurso (solapamiento) | `(False, 'Unavailable or missing: Casual Board 1')` |
| 9 | recurso inexistente | `(False, 'Unavailable or missing: nope_id (not found)')` |
| 10 | `find_next_slot(1h, ['c_board_1'])` | devolvió `2026-07-07 09:00:00` |
| 11 | pool de piezas: 6 eventos × 10 piezas > 50/día | los 5 primeros OK, el 6º `(False, 'Not enough spare pieces ... 50 available, 60 needed')` |
| 12 | guardar + recargar JSON | 1 evento persistido y releído correctamente |

**La lógica central de no-solapamiento y validación funciona de verdad al
ejecutar, no solo en el papel.**

---

## 1. Qué hace el programa

Es un **planificador de eventos para un club de ajedrez ("Critical Mass Chess
Club")** con interfaz web construida en Streamlit. El punto de entrada es
`gui.py` (`gui.py:1-48`); se lanza con `bash run.sh` (`run.sh:2`) y abre en
`http://localhost:8501`. El modelo de dominio vive en `main.py` (clase
`ChessClub`, `main.py:8`), las entidades en `models.py` (`Resource`, `Event`) y
la persistencia en `file_processing.py`.

El flujo principal: el club gestiona **recursos** (salas, equipamiento —tableros,
piezas, relojes, proyector— y personal —entrenadores FIDE, árbitros,
comentaristas—, definidos en `resources.json`). El usuario agenda **eventos** de
seis tipos (torneo, clase, enfrentamiento, partida amistosa, análisis,
simultánea) desde la página "Add Event" (`gui.py:77-146`). Al agendar, el sistema
valida en cascada: tiempo válido, fecha futura, día no bloqueado, duración
mínima, horario del club, **disponibilidad (no solapamiento)** de cada recurso,
**correquisitos y exclusiones** declarados en JSON, y el **pool diario de piezas
de repuesto**. Además ofrece: búsqueda del próximo hueco libre
(`find_next_slot`, `main.py:171`), monitoreo de carga horaria de entrenadores con
sugerencia de alternativa (`get_coach_workload`/`suggest_alternative_coach`,
`main.py:222-254`), y un panel "Settings" para editar tipos de evento, recursos y
restricciones en caliente.

Es un proyecto **notablemente ambicioso para 1er año**: el dominio está bien
pensado y las reglas de negocio están externalizadas a configuración.

## 2. Organización del código

Muy buena para el nivel. El código está **repartido en cuatro módulos** con
responsabilidades claras, en vez de un `main.py` monolítico:

- `models.py` — entidades `Resource` y `Event` con `to_dict()` para serializar
  (`models.py:3-31`).
- `main.py` — clase `ChessClub` con toda la lógica de dominio, bien dividida en
  métodos cortos y de nombre expresivo (`search_resource`, `check_available`,
  `validate_restrictions`, `schedule_event`, `find_next_slot`, `delete_event`…).
- `file_processing.py` — funciones libres `read_json`/`write_json` y clase
  `FileProcessing` para guardar/cargar (`file_processing.py:5-52`).
- `gui.py` — capa de presentación Streamlit, separada de la lógica.

Los nombres de variables y funciones son claros y en inglés consistente. Hay
reutilización real: `check_available` se llama tanto desde `schedule_event`
como desde `find_next_slot`; `search_resource` centraliza la búsqueda. La
separación presentación/lógica es exactamente lo que se espera y rara vez se ve
tan limpio en un primer proyecto. `main.py:1` empieza con una línea en blanco
(cosmético, sin efecto).

## 3. Corrección funcional (basada en ejecución real)

**Arranca perfectamente** y las 6 páginas renderizan sin `Traceback` (ver
"Resumen de ejecución"). El servidor real devolvió HTTP 200.

La lógica hace **exactamente** lo que promete el issue y el informe. Verificado
al correr (`main.py:85-169` para `schedule_event`):

- **No solapamiento** (`check_available`, `main.py:25-34`): la condición
  `not (end <= event.start or start >= event.end)` es el algoritmo correcto de
  intersección de intervalos. El caso 8 confirmó que rechaza doble reserva del
  mismo tablero.
- **Correquisitos y exclusiones** (`validate_restrictions`, `main.py:59-83`):
  casos 2 y 3 confirmaron el rechazo con mensajes descriptivos.
- **Validaciones de tiempo/fecha/duración/día bloqueado**: casos 4-7 todos
  correctos.
- **Pool de piezas de repuesto** (`main.py:151-161`): caso 11 confirmó el
  rechazo del 6º evento al exceder 50 piezas/día.
- **Persistencia** (caso 12): round-trip de guardado/carga correcto.

Validación de entradas: **sólida**. Se manejan recursos inexistentes (caso 9),
formatos de hora mal en config con `try/except` (`main.py:127-128`), y valores
de política no numéricos (`main.py:50-57`). La GUI también valida campos vacíos
antes de agendar (`gui.py:129-134`).

**Discrepancia menor informe↔config:** el informe (`report.md:51`) dice "los
relojes solo pueden ser utilizados en torneos", pero la regla real en
`resources.json:145-147` los permite en `tournament` **y** `team_match`. El
mensaje de error del código ("Clocks only for tournaments and team matches") es
el correcto; el informe simplificó de más en ese punto.

No encontré ninguna opción que lanzara excepción durante el recorrido.

## 4. Buenas prácticas de Python (nivel principiante)

Muy por encima del nivel esperado:

- **Legibilidad e indentación**: consistentes en todo el proyecto.
- **`try/except` donde toca**: parseo de config (`main.py:50-57`, `116-128`),
  guardas defensivas alrededor del pool de piezas (`main.py:152-161`). El
  `except Exception: pass` de `main.py:160` es demasiado amplio (silencia
  cualquier error), pero es un desliz menor.
- **f-strings** usadas idiomáticamente en mensajes y en la GUI.
- **Type hints ligeros** en varias firmas (`main.py:18,25,85,214,222`) — no se
  exigen en 1er año, es un plus.
- **Comprehensions claras** (`main.py:64,154,157`) sin abusar de anidamiento.
- Sin variables globales problemáticas: el estado vive en la instancia
  `ChessClub` y en `st.session_state`.

Puntos a pulir: el `except Exception: pass` mencionado; y `type` se usa como
nombre de parámetro/variable en varios sitios (`main.py:47,85,106…`), lo que
sombrea la función incorporada `type()` — funciona, pero conviene evitarlo.

## 5. Datos y persistencia

Bien resuelto. **Dos archivos con responsabilidades separadas**:
`resources.json` (configuración: recursos, tipos de evento, restricciones,
`config`) y `CM_chess_club.json` (eventos agendados). Las estructuras son
razonables: listas de objetos en memoria, serializadas vía `Event.to_dict()`
(`models.py:22-31`) que guarda solo los **IDs** de los recursos y los rehidrata
al cargar buscándolos por ID (`file_processing.py:48-51`) — decisión correcta
que evita duplicar objetos. Las rutas se construyen con
`os.path.dirname(os.path.abspath(__file__))` (`file_processing.py:6`), así que
la app funciona sin importar el directorio de trabajo. El caso 12 confirmó el
round-trip. Guardado automático tras cada alta/baja en la GUI
(`gui.py:73,141`).

## 6. Informe (`report.md`)

El informe es **excelente y honesto**: describe con precisión lo que el código
hace, explica las decisiones de diseño (por qué Streamlit, por qué JSON, por qué
externalizar reglas), documenta las funcionalidades una por una y hasta incluye
una sección de "Lecciones asimiladas" madura (`report.md:114-116`). No infla:
casi todo lo que afirma está respaldado por el código ejecutado.

Dos matices:

- **Sobre-simplificación** en `report.md:51` (relojes "solo en torneos" cuando
  también valen para enfrentamientos) — ver dimensión 3.
- **Testing declarado pero no incluido**: el informe describe "pruebas
  sistemáticas" (`report.md:110-112`) y `pyproject.toml` declara pytest como
  dependencia opcional (`pyproject.toml:27-31`), pero `.gitignore` excluye
  `test_edge_cases.py` y `test_main.py`, así que **los tests no se subieron al
  repo**. No penaliza en 1er año (los tests no se exigen), pero conviene saber
  que el informe habla de pruebas que no están versionadas. La verificación
  dinámica que hice yo confirma que la lógica sí es correcta.

También hay un desajuste menor entre la sección "Estructura" del informe
(`report.md:86-94`, menciona `main.py` como "programa") y la realidad (el
programa se lanza por `gui.py`); es un residuo de una versión anterior.

---

## Valoración global (orientativa, sin nota)

Trabajo **sobresaliente para un primer proyecto de 1er año**. La arquitectura
modular, la separación presentación/lógica, la externalización de reglas a JSON
y la corrección real de la lógica de no-solapamiento y validaciones —todas
verificadas ejecutando— lo colocan claramente por encima del nivel esperado. La
principal fortaleza es que **funciona de verdad y está bien organizado**; las
áreas de mejora son cosméticas (un `except` demasiado amplio, sombrear `type`) y
de honestidad documental (tests mencionados pero no subidos, y una
simplificación de más en una regla). Nada de esto compromete la calidad del
entregable.
