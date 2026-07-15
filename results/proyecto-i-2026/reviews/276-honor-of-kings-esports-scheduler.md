# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #276
- **Repositorio:** https://github.com/KyleFullbuster/Honor-of-Kings-Esports-Scheduler
- **Estudiante:** Alejandro Miguel González Velez
- **Grupo:** C-122
- **Descripción declarada:** Sistema de gestión integral para equipos competitivos de Honor of Kings. Gestiona scrims, prácticas, análisis de draft y asignación de recursos profesionales.

---

## Nota metodológica importante

**No es una app de consola.** Es una aplicación web **Streamlit** con seis pestañas
(`Proyecto/main.py:200` — `st.tabs([...])`). El verificador automático la marcó como
"sin punto de entrada" porque no hay `main.py`/`app.py` en la raíz; el punto de entrada
real es `streamlit run Proyecto/main.py` y hay que ejecutarlo **desde dentro de
`Proyecto/`**, porque el scheduler abre `data.json` y `heroes.json` por ruta relativa
(`scheduler.py:38`, `scheduler.py:258`).

Cómo adapté la ejecución:

1. `uv venv --python 3.12` + `uv pip install streamlit==1.52.2 pandas python-dateutil`.
2. `py_compile` de los 12 módulos → **compilan todos sin error**.
3. Ejecuté la **lógica de negocio** (`Scheduler` de `scheduler.py`) directamente, en modo
   headless, alimentándola con flujos válidos e inválidos y observando qué excepciones
   levanta.
4. Arranqué además la GUI real (`streamlit run main.py --server.headless true`): sirvió
   **HTTP 200** sin `Traceback` en el log. La cadena de imports que hace `main.py`
   (scheduler + styles + las 6 tabs + utils.helpers) resuelve correctamente.

## Dimensión 1 — Qué hace el programa

Es un planificador de eventos para un equipo de eSports. Un evento tiene nombre, inicio,
duración y una lista de **recursos** (jugadores, héroes, instalaciones/dispositivos). El
núcleo funcional vive en `scheduler.py`:

- **Crear** (`add_event`, `scheduler.py:618`): valida formato de fecha, que no sea en el
  pasado, duración entre 10 y 240 min, recursos existentes, restricciones y conflictos de
  horario; luego persiste en `data.json`.
- **Restricciones** (`check_constraints`, `scheduler.py:516`): recursos mínimos, nombre
  único, co-requisitos (p. ej. `Héroe: Lam → Jugador2`), exclusiones mutuas (p. ej.
  `Lam ≠ Li Bai`), una sola sala por evento, y una regla de posición para eventos
  "importantes" (scrim/torneo): cada jugador debe tener al menos un héroe de su posición.
- **Conflictos de horario** (`check_conflicts`, `scheduler.py:595`): solapamiento temporal
  que comparta algún recurso.
- **Buscar hueco** (`find_next_slot`, `scheduler.py:676`): recorre en pasos de 15 min,
  horario laboral 9:00–22:00, hasta 7 días.
- **Eliminar / detalles / estadísticas** (`delete_event:745`, `get_event_details:757`,
  `get_statistics:789`).

La GUI (`main.py` + `tabs/tab1..tab6`) envuelve todo eso: un sidebar con dashboard en vivo
y pestañas de listar / agregar / buscar / eliminar / detalles / sistema.

## Dimensión 2 — Organización del código

**Fortaleza destacada.** El proyecto está **bien modularizado** para ser de primer año:
la lógica de negocio (`scheduler.py`) está limpiamente separada de la presentación
(`main.py` + `tabs/*.py`), el CSS está aislado en `styles.py`, los datos en JSON, y hay
un paquete `utils/` con helpers. Cada pestaña es una función `show_*_tab(scheduler)`
(p. ej. `tabs/tab2_add.py:11`). Esta separación permitió evaluar el motor sin tocar la GUI.

**Debilidades:**

1. **Nombre de archivo con espacio**: `Proyecto/utils/__init__ .py` (nótese el espacio
   antes de `.py`). No es un `__init__.py` válido, así que `utils` funciona por
   *namespace package* implícito (verificado: `import utils` deja `utils.__file__ == None`).
   Funciona por casualidad; debería renombrarse a `__init__.py`.
2. **Dos sistemas de validación paralelos que se solapan.** `scheduler.check_constraints`
   (`scheduler.py:516`) es la fuente de verdad usada en `add_event`, pero `utils/helpers.py`
   define un **segundo** validador, `validate_event_resources` (`helpers.py:110`), con reglas
   distintas por tipo de evento (min/max jugadores, etc.). Este segundo validador solo se usa
   en la GUI y **puede divergir** del motor: p. ej. un scrim exige "min 5 jugadores" en
   `helpers.py` pero `check_constraints` solo exige 1 (`scheduler.py:244`). El mismo par de
   funciones `_extraer_posicion_*` está **duplicado** en `scheduler.py:492` y `helpers.py:88`.
3. **`Scheduler` es una clase monolítica** de 800 líneas que mezcla carga de datos,
   validación, búsqueda y persistencia. Aceptable a este nivel, pero es el candidato natural
   a dividir.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté el motor con datos concretos. Resultados (numerados):

1. **Init OK**: carga 111 héroes desde `heroes.json`, 5 jugadores, 11 instalaciones, y los
   4 eventos de `data.json`. `get_statistics()` devuelve valores coherentes.
2. **Alta válida OK**: `add_event("Practica Real X", <+4h>, 30, [Jugador2, Héroe: Lam,
   Sala de Práctica Individual, Dispositivo Android Pro])` → evento agregado, la lista pasó
   de 4 a 5 y persistió. **Verificado que el camino feliz funciona.**
3. **Fecha basura** (`"no-es-fecha"`) → `ValueError: 📅 Formato de fecha inválido`. ✅
4. **Fecha en el pasado** (`2020-01-01`) → `⌛ No se pueden programar eventos en el pasado`. ✅
5. **Sin jugador** → `🎮 Debe haber al menos 1 jugador`. ✅
6. **Co-requisito** (Lam sin Jugador2) → `🔗 Lam requiere Jugador2`. ✅
7. **Exclusión mutua** (Lam + Li Bai) → `⚡ Lam y Li Bai no pueden usarse juntos`. ✅
8. **Dos salas** → `🚫 Solo se puede seleccionar UNA sala por evento`. ✅
9. **Recurso inexistente** (`Héroe: Inexistente`) → `❌ Recursos inválidos`. ✅
10. **Duración < 10 min** → `⏱️ Duración mínima: 10 minutos`. ✅
11. **Nombre duplicado** → detectado por `check_constraints`. ✅
12. **Conflicto de horario** (mismo recurso, misma ventana) → `⏰ CONFLICTO con evento ...
    Recursos en conflicto: Dispositivo Android Pro`. ✅
13. **Regla de posición en evento importante**: un scrim con `Jugador3 (Mid)` + `Héroe: Lam
    (Jungle)` → `🎯 Jugador3 (Mid) DEBE usar al menos un héroe de su posición (Mid)`; con
    `Héroe: Daji (Mid)` la regla de posición pasa. ✅
14. **Co-req de instalación** (Coach Principal sin Android Pro) y **exclusión de dispositivos**
    (Pro + Elite juntos) → ambas se disparan correctamente. ✅

**En ninguno de los ~15 flujos inválidos hubo un `Traceback` no controlado**: todos los
errores salen como `ValueError` con mensaje legible, que la GUI captura. Esto es lo más
importante y está bien resuelto.

**Fallos del entorno (no del estudiante):** ninguno relevante. Streamlit arrancó headless
y sirvió HTTP 200.

**Matiz correcto, no bug:** la validación "las salas requieren dispositivos"
(`scheduler.py:484`) obliga a incluir un `Dispositivo` cada vez que se usa una `Sala`. Es
una regla intencional del estudiante, no un defecto; simplemente hay que recordarla al
construir un evento válido.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Positivo**: type hints (`Dict, List, Optional, Tuple`), docstrings en casi todos los
  métodos, `logging` configurado (`scheduler.py:28`) en vez de `print` sueltos, manejo de
  errores con `ValueError` + mensajes claros, y validación de entradas antes de mutar estado.
  Muy por encima de la media de primer año.
- **Mejorable**:
  - `except:` desnudos en `_extraer_posicion_*` (`scheduler.py:501`, `helpers.py:94`). Están
    marcados con `# noqa: E722`, pero atrapar `Exception` concreto (o `IndexError`) sería más
    idiomático.
  - Parsing de recursos por *string* (`recurso.split(": ")[1]`, `heroe.split("(")[1]...`) es
    frágil: la posición del héroe se deduce de un formato textual embebido en el nombre. Un
    pequeño cambio en el texto rompería las reglas de posición. Una estructura de datos
    (diccionario con campos `nombre`, `rol`, `posicion`) sería más robusta.
  - El código de `main.py` mezcla mucho HTML/CSS inline (`unsafe_allow_html=True`) con la
    lógica de estado; para mantenimiento conviene el `styles.py` que ya existe.

## Dimensión 5 — Datos y persistencia

- Persistencia en **JSON** (`data.json`), con serialización de fechas vía `isoformat()`
  y deserialización vía `datetime.fromisoformat()` (`scheduler.py:360`, `:421`). Correcto.
- Los héroes se cargan de `heroes.json` (111 héroes con clasificación por rol y posición),
  con **fallback** a datos por defecto si el archivo falta (`_load_heroes_data:255`,
  `_create_default_heroes_data:270`). Buen detalle defensivo.
- `save_data` guarda además metadatos descriptivos (lista de restricciones implementadas,
  versión, correcciones) — útil como autodocumentación, aunque infla el JSON.
- **Observación menor**: `add_event` revierte en memoria si falla el guardado
  (`scheduler.py:672`), pero como `save_data` levanta `ValueError`, el evento ya fue añadido
  y ordenado antes; el rollback existe pero el flujo de error es algo enrevesado.

## Dimensión 6 — Informe (`report.md`)

**No hay `report.md`.** El repo trae un **`README.md`** (594 palabras) que hace las veces de
informe, más 7 capturas de pantalla en `screenshots/`. El verificador automático reporta
correctamente la ausencia de `report.md` con ≥2000 palabras.

Sobre el contenido del README frente al código:

- Las restricciones que enumera (co-requisitos Lam→Jugador2, Ao Yin→Jugador4, exclusiones
  Lam≠Li Bai, Daji≠Kongming, una sala por evento) **coinciden** con lo implementado en
  `scheduler.py:86-240`. **Honesto en esto.**
- Afirma "~30 héroes" en el texto de justificación, pero `heroes.json` trae **111**; la
  discrepancia es a favor del proyecto (hay más de lo que dice), pero conviene alinearlo.
- "Validación Automática de Conflictos: ✅ superposición temporal, recursos duplicados,
  formato de fechas, prevención de pasado" — **todo verificado en ejecución** (Dimensión 3),
  no exagera.
- El README no dice explícitamente "esto demuestra/prueba" con testing; presenta capturas.
  No hay sobreafirmación de validación formal.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**. El estudiante entregó un motor de planificación completo,
con CRUD, un sistema de restricciones de co-requisito/exclusión no trivial, búsqueda de
huecos y persistencia JSON, todo envuelto en una GUI Streamlit modular de seis pestañas.
Lo ejecuté de verdad: el camino feliz agrega eventos y persiste, los ~15 flujos inválidos
se rechazan con mensajes claros y **sin un solo `Traceback` no controlado**, y la GUI
arranca y sirve. La separación lógica/presentación es notablemente buena para primer año.

Los defectos son de organización y robustez, no de corrección: un `__init__ .py` con espacio
en el nombre, dos validadores paralelos que pueden divergir (`scheduler.check_constraints`
vs `helpers.validate_event_resources`), parsing de recursos por manipulación de strings, y
la ausencia del `report.md` requerido (el README lo suple parcialmente).

- **Principal fortaleza**: motor de restricciones real y funcional, verificado en ejecución,
  con separación limpia entre lógica de negocio y GUI.
- **Principal área de mejora**: unificar la validación en un solo lugar (el motor) y modelar
  los recursos como datos estructurados en vez de deducir posición/rol partiendo strings;
  además, entregar el `report.md` formal.
