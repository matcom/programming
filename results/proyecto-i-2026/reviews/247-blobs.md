# Reporte detallado — Proyecto I (247) · Blobs

- **Estudiante:** Karel Antonio González Zaldivar
- **Grupo:** C111
- **Issue:** #247
- **Repo:** https://github.com/karelantonio/Blobs
- **Descripción del issue:** "Administrador de tareas para Blobs (pequeñas criaturas). Info útil en la documentación del repo".
- **Naturaleza:** No es una app de consola. Es una **aplicación gráfica de escritorio** hecha con **Kivy + KivyMD** (Material Design). Nivel muy por encima del típico proyecto de consola de 1er año.

---

## Ejecución dinámica (lo que corrí de verdad)

Entorno aislado con `uv`:

1. `make kivymd` — clona KivyMD dentro del repo (el `wheel` de KivyMD no incluye sus `.kv`, así que el autor lo documenta y provee un `Makefile`; funcionó a la primera).
2. `uv sync` — instaló kivy 2.3.1, asynckivy, materialyoucolor, pillow, etc. (14 paquetes, EXIT 0).
3. `uv run python main.py` bajo `DISPLAY=:0` (había servidor X real).

**Resultado del arranque de la GUI:** la app **arranca correctamente**. El log llega a `Start application main loop` con **cero `Traceback`** (`grep -c Traceback` = 0). Construyó las cuatro pantallas (Start/Overview/About/NewEvent), cargó los recursos-imagen y entró en el bucle principal; la maté por timeout (EXIT 124, esperado en una GUI). El único ruido en el log es un `FileNotFoundError` de `xsel`/`xclip` — es la **sonda de proveedor de portapapeles de Kivy** (envuelta en `try/except` dentro de la librería, no del código del alumno) y es inofensiva.

**Ejercicio de la lógica del alumno (headless).** Como no puedo clicar botones, monté un driver que parchea `App.get_running_app()` con los eventos guardados y llama directamente a las funciones del estudiante (`core/coldetect.py`, `core/holefind.py`, `core/events.py`). Observado:

- `load_events_from_disk()` cargó **3 eventos** desde `save.json` correctamente.
- `check_exclusions` con **Fuel pipe + Laser Beam** activos → devolvió el error esperado. ✔
- `check_collisions` metiendo un evento que usa **10 Blob** en pleno solapamiento → devolvió "creates a collision (using more resources than the available)". ✔
- `find_hole(5, {Blob:2})` → devolvió un `datetime` válido (`2026-07-06 11:35:00`). ✔
- `save_events_to_disk` produce JSON válido y persiste. ✔

**Dos bugs reales reproducidos al ejecutar** (ver Dimensión 3):

- `check_exclusions` con **solo Fuel pipe** (Laser Beam en qty 0) → **falso positivo**: "Laser Beam must not be used at the same time as: Fuel pipe".
- `check_inclusions` con **Chlorine + Gloves** (correcto) → **falso positivo**: "Resource Anti-radiation clothing requires: Blob" (¡y ni siquiera se está usando ropa antirradiación!).

---

## Dimensión 1 — Qué hace el programa

Gestor de agenda/tareas para los "Blobs" (criaturas de un mundo distópico). El usuario crea eventos a partir de **plantillas predefinidas** (`Generate electricity`, `Visit neighbour`, `Water treatment` — `core/event_templates.py:18-50`), les asigna fecha/hora/duración y una cantidad de **recursos** (Blob, Fuel pipe, Voltimeter, Laser Beam, etc. — `core/resources.py:12-101`). El sistema valida el evento contra tres reglas antes de crearlo: **inclusiones** (un recurso exige otro), **exclusiones** (dos recursos no coexisten) y **colisiones** (en ningún instante puede usarse más de un recurso del que existe). Además tiene un "buscar hueco" que sugiere el primer instante libre para colocar el evento.

- **Punto de entrada:** `main.py:1-6` → `from core import MainApp; MainApp().run()`.
- **Ejecución:** `uv run main.py` (o venv manual + `make kivymd`), documentado en `README.md:85-104`.
- **Flujo:** `MainApp.build()` (`core/__init__.py:322-326`) carga `RootWidget` (ScreenManager con 4 pantallas), lee eventos de disco, y mantiene la lista `events` como propiedad reactiva que **auto-persiste y refresca la vista** cuando cambia (`events_changed`, `core/__init__.py:343-346`).

## Dimensión 2 — Organización del código

**Muy buena para 1er año.** El código está limpiamente separado en un paquete `core/`:

- Lógica de dominio pura y aislada de la UI: `events.py` (dataclasses + persistencia), `resources.py`, `event_templates.py`, `coldetect.py` (validaciones), `holefind.py` (búsqueda de hueco).
- UI declarativa en `.kv` (`core/main.kv` + `core/ui/*.kv`), separada de la controladora en `core/__init__.py`. El propio autor admite en `README.md:114` que no siguió MVC/MVVM; aun así, la separación lógica/vista está lograda.
- **Dataclasses** bien usadas (`Resource`, `Event`, `EventTemplate`, `Requirement`) con `to_dict`/`from_dict` para serializar.
- Nombres claros y en inglés consistente. Diccionarios de índice (`RESOURCES_AS_DICT`, `EVENTS_AS_DICT`) para lookups O(1).

Detalles menores:
- `Resource.from_dict` / `Event.from_dict` (`core/events.py:16,42`) **no llevan `@staticmethod`**; funcionan solo porque se invocan como `Resource.from_dict(dct)` (sin instancia). Es frágil: si alguien hiciera `resource_instancia.from_dict(...)` explotaría. Sugerencia: añadir `@staticmethod` o `@classmethod`.
- Hay **dos clases `Resource` distintas** con el mismo nombre en `core/resources.py:4` (recurso-catálogo) y `core/events.py:6` (recurso-cantidad de un evento). No colisionan porque están en módulos distintos, pero confunde al leer. Sugerencia: renombrar una (p.ej. `ResourceDef` vs `ResourceUse`).
- El nombre del método `check_and_create_event_dammit` (`core/__init__.py:165`) delata frustración; recomendable un nombre neutro.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

La app **arranca y corre** sin excepciones (ver Ejecución dinámica). Persistencia, colisiones y búsqueda de hueco funcionan. Pero al ejercitar las validaciones aparecieron **dos bugs reales**, ambos reproducidos:

**BUG 1 — `check_exclusions` genera falsos positivos (`core/coldetect.py:6-14`).**
El bucle **interno** salta los recursos con `qty==0` (`coldetect.py:11-12`), pero el bucle **externo (`resd1`) no**. Entonces, cuando un recurso está a 0 pero su lista `excludes` menciona un recurso activo, se dispara el error. Reproducido con solo **Fuel pipe** activo (Laser Beam en 0): devolvió "Laser Beam must not be used at the same time as: Fuel pipe". El evento legítimo se bloquearía. **Fix:** añadir `if resd1["qty"]==0: continue` al inicio del bucle externo.

**BUG 2 — `check_inclusions` ignora la cantidad en el recurso "dueño" (`core/coldetect.py:16-22`).**
`names` se calcula bien (solo recursos con `qty>0`, `coldetect.py:17`), pero luego el bucle recorre **todos** los `r in rrss` (`coldetect.py:18`), incluidos los que están a 0. Así, un recurso inactivo con `includes` exige su dependencia. Reproducido con **Chlorine + Gloves** (que debería pasar): devolvió "Resource Anti-radiation clothing requires: Blob" — ropa antirradiación que ni se está usando. **Fix:** iterar solo sobre los recursos con `qty>0` (p.ej. `for r in rrss: if r["qty"]==0: continue`).

Validación de entradas (parte positiva, `core/__init__.py:165-251`): valida plantilla seleccionada, formato de fecha (regex `\d+/\d+/\d+`), hora, duración entera y positiva, y satisfacción de requisitos. Buen manejo defensivo para 1er año.

Detalles de corrección:
- **Mensaje de error copy-paste** (`core/__init__.py:196`): al validar la **hora** dice "Time does not look valid (must be DD/MM/YY)" — debería decir HH:MM.
- **Formato de fecha inconsistente entre serialización y parseo:** `Event.to_dict` guarda `%m/%d/%Y` (`core/events.py:35`) y `from_dict` lo lee igual (`events.py:45`), así que es coherente internamente; pero la UI **pide y muestra** `DD/MM/YYYY` (`core/__init__.py:198`, `new_event_screen.kv:141`). Como el JSON nunca se muestra crudo al usuario, no rompe, pero es una inconsistencia latente a documentar.
- `find_hole` tiene un `return nw` de "workaround" (`core/holefind.py:14`) con `TODO`; el propio autor sabe que en teoría siempre hay hueco, pero deja la red de seguridad. Honesto.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Por encima del nivel esperado:
- **Dataclasses**, **f-strings**, comprensiones de lista claras, `set` para lookups.
- **Manejo de errores** en I/O: `load/save_events_to_disk` envuelven en `try/except` y degradan a lista vacía en vez de crashear (`core/events.py:51-67`). Muy bien.
- Type hints ligeros (`-> str|None`, `list[Event]`) — no exigidos, suma.
- Indentación consistente, sin duplicación evidente grave.

A mejorar:
- `except:` desnudo en `core/__init__.py:207` y `:271` (captura todo). Preferible `except ValueError:`.
- `import *` implícito vía `from .resources import RESOURCES, ...` está bien, pero `from kivy.app import App` dentro de módulos de lógica (`coldetect.py:1`, `holefind.py:1`) **acopla la lógica de dominio a Kivy**: `check_collisions`/`find_hole` leen `App.get_running_app().events`. Sugerencia: pasar la lista de eventos como parámetro; así la lógica sería testeable sin arrancar la GUI (de hecho tuve que parchear `App` para probarla).
- Regex sin `re.fullmatch` ni `$`: `match("\\d+/\\d+/\\d+", date)` (`core/__init__.py:186`) acepta basura al final ("12/3/2026zzz"). Menor.

## Dimensión 5 — Datos y persistencia

Correcta. Estado en memoria como `list[Event]` (propiedad Kivy reactiva) que se **auto-serializa a `save.json`** en cada cambio vía el binding `events_changed` (`core/__init__.py:343-346`). Serialización manual con `to_dict`/`from_dict` y `json`. Reproducido: los 3 eventos de `save.json` cargan y se reconstruyen bien. Estructuras razonables (dataclasses + dicts de índice). No hay migración de esquema ni manejo de JSON corrupto más allá del `try/except` que lo trata como "sin eventos" — aceptable.

## Dimensión 6 — Informe (`README.md`)

**No hay `report.md`; el informe es el `README.md`**, y es **excelente y honesto**. Describe fielmente la arquitectura (árbol de archivos comentado archivo por archivo, `README.md:52-73`), explica y **justifica decisiones de diseño** (por qué Kivy, por qué KivyMD, por qué separar `.kv`), documenta el algoritmo de colisiones y de búsqueda de hueco (`README.md:120-126`), incluye galería con capturas y una **nota de transparencia sobre el uso de IA** (`README.md:138-140`, solo imágenes y síntesis de documentación).

- **No sobreestima:** al contrario, es autocrítico ("muy suciamente", "no seguí MVC", `TODO`s honestos). No afirma features que el código no tenga.
- **Discrepancia menor informe↔proyecto:** `pyproject.toml` **no declara `kivymd`** como dependencia; el `README` lo compensa documentando `make kivymd`. Funciona, pero un evaluador que solo haga `uv sync` sin leer el README fallaría. Sugerencia: mencionar `make kivymd` también en la sección "Ejecutar" con más énfasis, o pinnear KivyMD como dependencia git.
- El proyecto se autodescribe como "de consola" en el enunciado del issue, pero es GUI — el README aclara que es gráfico, así que no engaña.

---

## Síntesis

Trabajo **sobresaliente para primer año**. Arranca y corre limpio, con una GUI Material Design real, separación lógica/vista, dataclasses, persistencia automática y un README ejemplar y honesto. Los dos bugs de validación (`check_exclusions`/`check_inclusions` ignoran `qty==0` en el bucle externo) son **reales y reproducibles**, pero de **una línea de fix cada uno** y no impiden el uso general — solo bloquean falsamente ciertos eventos válidos. Principal fortaleza: ambición técnica y arquitectura. Principal mejora: corregir los dos falsos positivos y desacoplar la lógica de dominio de `App.get_running_app()` para poder testearla.
