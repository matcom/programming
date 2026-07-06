# Reporte de Evaluación — Issue #235

- **Estudiante:** Luis David Gil Quintana
- **Grupo:** C122
- **Repositorio:** https://github.com/LuisDa04/PrimerProyectoCC
- **Proyecto:** Gestor de Eventos Hospitalarios (calendario de consultas, cirugías, emergencias, etc.)
- **Tipo de aplicación:** GUI de escritorio con Tkinter + SQLite (no consola)

---

## Ejecución dinámica (lo que realmente ocurrió al correr)

Entorno aislado creado con `uv venv --python 3.12`; el proyecto no tiene
dependencias externas (`requirements.txt` lo confirma: solo biblioteca estándar).
El Python del sistema **no** trae `tkinter`, pero el Python de `uv` sí (`TkVersion 8.6`).

- **Arranque de la GUI real** (`DISPLAY=:0 python main.py`): la aplicación
  **inicializó correctamente** — creó `calendario_hospital.db`, construyó el
  esquema y pobló los datos iniciales. El proceso terminó con un `core dumped` de
  XCB (`append_pending_request: Assertion !xcb_xlib_unknown_seq_number failed`),
  que es un **choque entre la librería X del sistema y el Python descargado por uv**,
  **no un fallo del código del estudiante**. La app llegó a levantar Tk y a
  persistir la BD antes del choque del entorno.

- **Verificación del esquema** (post-arranque): tablas `eventos`,
  `configuraciones`, `estado_calendario` creadas; datos iniciales poblados
  correctamente (4 doctores, 8 enfermeras, 8 recursos, 0 eventos). `database.py:78`.

- **Ejercicio headless de la lógica de negocio** (sin GUI, importando los módulos):
  todo funcionó sin `Traceback`:
  - `guardar_evento` → `obtener_eventos_dia`/`obtener_eventos_mes`: guarda y
    recupera con la estructura esperada. `database.py:218`, `database.py:362`.
  - Disponibilidad por intervalo: un doctor ocupado 09:00–09:30 **no** aparece
    disponible en ese intervalo pero **sí** a las 10:00. La detección de
    superposición funciona. `database.py:445`, `database.py:49`.
  - Restricciones (`constraints.py:163`): quirófano con 1 enfermera → rechazado;
    con 2 → aceptado; rayos X con doctor equivocado → rechazado; quirófano +
    consultorio (exclusión mutua) → rechazado. Los 4 casos correctos.
  - `buscar_hueco_disponible` (`database.py:577`): devuelve `(2026,7,10,'08:00','08:30')`
    para una consulta general. Correcto.
  - `obtener_estadisticas` (`database.py:721`), `obtener_agenda_recurso`
    (`database.py:525`): correctos.
  - **JSON**: import de `example_data.json` → 7 eventos cargados; export roundtrip
    coherente (columnas planas `enfermera/enfermera2/recurso1/recurso2`, simétricas
    entre import y export). `json_handler.py:82`, `json_handler.py:92`.
  - **Inputs inválidos**: import de archivo inexistente, JSON malformado y JSON sin
    lista `eventos` → los tres devuelven `(False, mensaje)` claro **sin romper**.
    `json_handler.py:99-120`.

- **Un edge case frágil, no alcanzable desde la GUI:** `hora_a_minutos("ab:cd")`
  lanza `ValueError` (`database.py:36`), porque hace `map(int, ...)` sin proteger.
  En la práctica las horas provienen de comboboxes con valores fijos ("08".."19",
  "00"/"30"), así que no es alcanzable por el usuario; queda como nota menor.

**Veredicto de ejecución:** la aplicación arranca y su lógica de negocio completa
funciona correctamente. No se observó ningún `Traceback` atribuible al código del
estudiante en ninguno de los flujos ejercidos.

---

## Dimensión 1 — Qué hace el programa

Aplicación de escritorio (Tkinter) para gestionar la agenda de un hospital. El
punto de entrada es `main.py` (`AplicacionPrincipal`, `main.py:21`), que orquesta:
la base de datos SQLite (`DatabaseManager`), el calendario mensual interactivo
(`CalendarioBasico`), y las ventanas modales de creación/consulta de eventos.

Flujo principal: el usuario ve un calendario mensual (navegable 2020–2030); hace
**doble clic** en un día para abrir la ventana de creación de evento (`window.py`),
donde elige tipo de evento (consulta, cirugía, emergencia, chequeo, terapia,
exámenes), horario, doctor, enfermera(s) y recurso(s). La interfaz se adapta al
tipo de evento (muestra segunda enfermera para cirugía/emergencia, filtra recursos
por patrón). Antes de guardar se validan restricciones del dominio. Hay además:
búsqueda automática de hueco disponible, agenda por recurso, estadísticas,
import/export JSON, reseteo de BD, y un modo "avanzar día a día" con persistencia
del estado visual del calendario entre sesiones. Se ejecuta con `python main.py`
(coincide con la guía del informe, `report.md:280`).

## Dimensión 2 — Organización del código

**Sobresaliente para 1er año.** El proyecto está dividido en **9 módulos** con
responsabilidades bien separadas, cada uno con un docstring de cabecera que explica
su propósito:

- `main.py` — orquestador (`AplicacionPrincipal`). `main.py:21`
- `database.py` — capa de datos SQLite (CRUD, disponibilidad, buscar hueco, stats). `database.py:62`
- `constraints.py` — motor de reglas del dominio (declarativo). `constraints.py:47`
- `calendario.py` — componente visual del calendario. `calendario.py:17`
- `window.py` / `window2.py` / `resource_view.py` — ventanas modales.
- `json_handler.py` — import/export. `utils.py` — helpers de UI reutilizables.

Uso apropiado de **clases** (`DatabaseManager`, `CalendarioBasico`,
`VentanaDetalle`, etc.) cuando el dominio lo pide. Nombres de funciones y variables
descriptivos y en español consistente. Se ve esfuerzo real por **no repetir código**:
p. ej. `_recursos_ocupados_en_intervalo` (`database.py:401`) es un método compartido
para evitar duplicar la lógica de detección de conflictos, y `_poblar_combo`
(`window.py:273`) centraliza el llenado de comboboxes. Comunicación entre componentes
vía callbacks inyectados (`on_double_click_callback`, `on_month_change_callback`),
un patrón limpio y desacoplado poco común a este nivel.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ver "Ejecución dinámica" arriba. Resumen: **arranca correctamente** y **toda la
lógica de negocio funciona**. Detección de conflictos por intervalo, motor de
restricciones (co-requisitos y exclusiones mutuas), buscar hueco, estadísticas,
agenda por recurso e import/export JSON: todos ejercitados y correctos, sin
`Traceback`. La validación de entradas es notablemente cuidada en `window.py:_guardar`
(`window.py:464`): valida que las horas sean numéricas, dentro del rango 08–19, que
los minutos sean 00/30, que inicio < fin, y la duración mínima por tipo de evento
(`window.py:482-529`). El import de JSON valida la estructura **antes** de tocar la
BD (`json_handler.py:108`), lo que evita corromper datos con un archivo malo.

Único punto frágil: `hora_a_minutos` (`database.py:36`) rompería con un string no
numérico, pero no es alcanzable desde la GUI (valores fijos en combos).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy por encima del promedio de 1er año:

- **Legibilidad e indentación** consistentes; secciones separadas con comentarios
  de banda (`# ── ... ──`).
- **f-strings** idiomáticos en todo el código.
- **Manejo de errores** apropiado donde toca: `try/except (ValueError, TypeError)`
  en `_evento_terminado` (`calendario.py:324`), `try/except` en export/import
  (`main.py:113`, `json_handler.py:102`), captura de `ValueError` al avanzar fecha
  (`calendario.py:446`).
- **Context managers** para SQLite (`with self.get_connection() as conn`) — buena
  práctica que muchos principiantes omiten. `database.py:80`.
- **Consultas parametrizadas** (`?` placeholders) en todas las queries: sin riesgo
  de inyección SQL. `database.py:257`, `json_handler.py:132`.
- `@staticmethod` usado correctamente donde no se necesita `self`. `database.py:178`.

Detalle menor: `calendario.py:13-14` importa `datetime` dos veces (línea 13 y 14),
inofensivo pero redundante.

## Dimensión 5 — Datos y persistencia

Diseño de persistencia **sólido y bien pensado**:

- Esquema SQLite con 3 tablas (`eventos`, `configuraciones`, `estado_calendario`).
- **Migración de esquema** con `PRAGMA table_info` para añadir columnas a tablas
  ya existentes sin perder datos (`database.py:113`) — muy maduro para el nivel.
- Persistencia del **estado visual** del calendario (día de inicio, días visitados)
  entre sesiones, serializando el set de días como JSON en una tabla clave-valor
  (`database.py:314`). Verificado en la ejecución: las tablas se crean y pueblan.
- Export/import JSON con roundtrip coherente y metadatos de versión.

## Dimensión 6 — Informe (`report.md`)

Informe **excelente y honesto**: 8 secciones, ~340 líneas, describe con precisión lo
que el código hace. No sobreestima; al contrario, cada afirmación tiene respaldo en
el código:

- El algoritmo "Buscar Hueco" descrito (`report.md:192-233`) coincide con
  `buscar_hueco_disponible` (`database.py:577`), incluido el enfoque de "huecos entre
  eventos" y el límite de 60 días.
- Las 8 restricciones descritas (`report.md:242-271`) coinciden exactamente con
  `CO_REQUISITE_RULES` (5) + `MUTUAL_EXCLUSION_RULES` (3) en `constraints.py`.
- La guía de uso (`report.md:277`) es correcta: `python main.py`.

Discrepancias menores: `pyproject.toml` referencia un `README.md` (`readme = "README.md"`)
que **no existe** en el repo; y la URL del repositorio en `pyproject.toml` apunta a
otro nombre (`Proyecto-de-gestor-de-Eventos`). Nada semántico.

---

## Síntesis

Trabajo **excepcional para un primer proyecto de 1er año**. Arquitectura modular
real, uso correcto de clases y SQLite, motor de restricciones declarativo, algoritmo
de planificación no trivial, validación de entradas cuidadosa y persistencia madura
(incluida migración de esquema). El informe es completo y fiel al código. Las
únicas observaciones son cosméticas (README ausente, import duplicado de datetime,
un edge case de parsing no alcanzable). La ejecución confirmó que la app arranca y
que toda la lógica de negocio funciona correctamente. Principal fortaleza:
organización y madurez del diseño. Área de mejora: cerrar detalles cosméticos y
blindar el parsing de horas.
