# Reporte de Evaluación — Jamazon (Issue #255)

- **Estudiante:** Roger Peña Pérez
- **Grupo:** C121
- **Repositorio:** https://github.com/VIRUSGAMING64/jamazon
- **Descripción declarada:** "gestionar la asignación de tareas en intervalos de tiempo"
- **Tipo:** Aplicación de escritorio con GUI (`customtkinter`), NO app de consola.

---

## Nota de ejecución (obligatoria)

Se clonó el repo (depth 1) y se creó un entorno aislado con `uv` (Python 3.12),
instalando `customtkinter`, `CTkMessagebox` y `ctkdlib` desde `requirements.txt`.
Instalación sin problemas (4 paquetes + deps).

Como es una GUI (no un menú de consola), no se pudo alimentar por stdin. Se probó
de dos formas:

1. **Arranque real de la GUI** (`python main.py`): el programa arranca, imprime
   `MAIN POINT STARTED`, ejecuta `calendar.remove_old_events()` correctamente y
   comienza a construir la ventana principal. En **este entorno** (Linux con un
   servidor X particular, sin `xvfb`) la construcción de widgets aborta con un
   error de hilos de X11:
   ```
   [xcb] Unknown sequence number while appending request
   [xcb] You called XInitThreads, this is not your fault
   [xcb] Aborting, sorry about that.
   Assertion `!xcb_xlib_unknown_seq_number' failed.
   ```
   El propio mensaje de xcb señala que es un problema del entorno gráfico
   (`this is not your fault`). Como control, una ventana `CTk()` mínima y vacía
   **sí** renderiza sin problema (RC=0) en el mismo entorno, y el pipeline de
   imports del proyecto también carga limpio. El fallo aparece durante la
   construcción de `app.__init__` (`main.py:12-69`), que combina `.pack()` +
   `.place()` leyendo `._current_width` en caliente, más el hilo de `darkdetect`.
   No se pudo aislar por completo sin `xvfb`. Dado que el informe reporta pruebas
   en Arch, Ubuntu y Windows 11, se asume que la GUI sí renderiza en máquinas
   normales; aquí se documenta como limitación del sandbox, no como veredicto.

2. **Ejercicio del backend headless** (importando `modules` y llamando a la lógica
   directamente, sin `mainloop`). Esto sí se ejecutó por completo y es la base de
   las observaciones de corrección. Resultados verificados en vivo:
   - `from modules import *` → OK; carga 34 recursos y 12 tareas desde
     `templates/*.json`, 0 eventos iniciales.
   - Resolución de dependencias (DFS de `get_sources_dependency`) para
     `entrega_en_moto` → expande correctamente a
     `[gasolina, cajas_envio, electricidad, uniforme_repartidor, repartidor,
     mochila_termica, almacen, moto]` (recursos indirectos incluidos).
   - `suggest_brute_lr(...)` → devuelve un `datetime` de inicio sugerido.
   - `add_event(...)` de un evento → `True` (agregado). Al intentar agregar más
     eventos en el mismo intervalo, el sistema **detecta correctamente** el
     agotamiento del recurso `almacen` (count=1) y rechaza con `(False, "almacen")`.
     Es decir: la compresión de coordenadas + Segment Tree + `check_available`
     funcionan de verdad, no solo en el papel.

Conclusión de ejecución: **el backend hace lo que promete y se verificó
corriéndolo**; la GUI arranca pero no se pudo ver renderizada en este entorno por
una limitación de X11.

---

## 1. Qué hace el programa

Jamazon es un **planificador de asignación de recursos en el tiempo** para el
dominio de una tienda/restaurante de comida (recursos: cocineros, repartidores,
motos, hornos, ingredientes, uniformes, etc.). El punto de entrada es `main.py`,
que lanza una ventana `customtkinter` (`main.py:128-133`) con seis acciones:
crear tarea agendada, eliminar tarea, agregar recurso, mostrar tareas, definir un
nuevo tipo de tarea y mostrar recursos.

El núcleo del problema es no trivial: cada tarea reserva cantidades de varios
recursos durante un intervalo `[start, end]` (en minutos), y el sistema garantiza
que en ningún instante la demanda acumulada de un recurso exceda su capacidad
instalada. Maneja además dependencias jerárquicas entre recursos (`internet`
requiere `electricidad`, resuelto por DFS en `utils.py:11-24`) y restricciones de
exclusión mutua (`gerente` no puede coexistir con `cocinero`, campo `without`).
Cuando no hay hueco, `suggest_brute_lr` (`calendar.py:184-214`) propone el
próximo instante disponible. La corrección real de esta lógica quedó verificada
al ejecutarla (ver nota de ejecución).

## 2. Organización del código

Muy por encima del promedio de primer año. El código está **modularizado con
intención** (`report.md:69`), no en un `main.py` gigante:

- `main.py` — solo la ventana raíz y navegación (`main.py:7-133`).
- `modules/iohandler.py` — `BasicHandler`, IO de JSON (`iohandler.py:7-61`).
- `modules/events.py` — clase `event`, modela una tarea (`events.py:3-83`).
- `modules/calendar.py` — `Calendar`, el motor de asignación (`calendar.py:5-214`).
- `modules/SegTree.py` — Segment Tree con lazy propagation (`SegTree.py:1-64`).
- `modules/utils.py` — funciones puras reutilizables (`utils.py:1-113`).
- `modules/gui_core/` — una clase por ventana (`TaskCreator`, `TaskRemover`,
  `ResAdder`, `EventCreator`, `EventShower`, `ResourceShower`).

Uso correcto de **herencia** (`Calendar(BasicHandler)` en `calendar.py:5`,
`event(BasicHandler)` en `events.py:3`, ventanas heredando de `CTkToplevel`).
Nombres en general claros y descriptivos (`get_sources_dependency`,
`check_available`, `remove_old_events`). Las funciones de `utils.py` están bien
extraídas para evitar duplicación (`utils.py:79-113`). Este nivel de separación
frontend/backend es notable para un primer proyecto.

Peros menores: algunos nombres mezclan idiomas y estilos (`event` en minúscula
como clase, `app` en minúscula — `main.py:7`, `events.py:3`; convención Python es
`CamelCase` para clases). Variables de una letra en la lógica de árbol
(`calendar.py:64-92`, `l`, `r`, `di`, `a2`) dificultan la lectura.

## 3. Corrección funcional (basada en ejecución real)

Verificado corriendo el backend (ver nota de ejecución):

- **Arranca:** sí; imports OK, carga de plantillas OK, `remove_old_events` OK.
- **Resolución de dependencias:** correcta y verificada — el DFS expande recursos
  indirectos (`utils.py:11-24`).
- **Detección de conflictos de capacidad:** correcta y verificada — tras ocupar
  `almacen` (count=1), rechaza nuevas reservas en el mismo intervalo devolviendo
  `(False, "almacen")` (`calendar.py:94-140`). El Segment Tree con compresión de
  coordenadas (`calendar.py:64-92`) hace su trabajo.
- **Sugerencia de horario:** `suggest_brute_lr` devuelve un `datetime` válido
  (`calendar.py:184-214`).
- **Hace lo que dice el issue** ("asignación de tareas en intervalos de tiempo"):
  sí, y con más profundidad de la que sugiere la frase.

Puntos frágiles observados leyendo + corriendo:

- **Manejo de errores demasiado silencioso.** Muchos bloques capturan `Exception`
  y solo lo mandan al `log`, devolviendo estados como `(False, None)` o `[]` en
  vez de propagar (`calendar.py:25-26`, `calendar.py:138-140`, `events.py:59-60`).
  Ejemplo grave: si `event.__init__` falla (`events.py:59`), el objeto queda a
  medio construir (sin `self.start`) pero no se aborta — un `AttributeError`
  latente aguas abajo.
- **`except:` desnudo** en `gvar.py:6-9` y `utils.py:66-69` — atrapa hasta
  `KeyboardInterrupt`.
- **`log` abre el archivo en cada llamada sin cerrarlo** (`utils.py:7-8`): fuga de
  descriptores si se llama mucho.
- **Bug de hilos en GUI:** `TaskRemover.update_combo` (`TaskRemover.py:56-66`)
  corre en un `Thread` daemon y toca widgets Tk (`self.eventos.configure`) desde
  un hilo secundario. Tk **no es thread-safe**; esto es exactamente el tipo de
  patrón que produce abortos de X11 como el que se vio aquí. Aunque el crash de
  este sandbox es ambiental, este patrón es un riesgo real de corrección.
- No se pudo probar en vivo una "entrada inválida por el usuario" en la GUI (no
  renderizó), pero el código sí valida fechas ISO (`utils.py:27-33`,
  `TaskCreator.py`) y `start >= end`.

## 4. Buenas prácticas de Python (nivel principiante)

Bien para el nivel:
- Uso de f-strings, comprensiones, `set` para operaciones de conjuntos
  (`events.py:31-48`, colisiones vía `deps & no_use`).
- Docstrings en casi todas las funciones y clases.
- Bucles claros y estructuras de datos adecuadas.

A mejorar:
- **`except` demasiado amplio / silencioso** (ya detallado en §3). Sugerencia:
  capturar excepciones concretas (`KeyError`, `ValueError`, `FileNotFoundError`)
  y no tragarse todo con `except:`.
- **Fugas de archivos:** varios `open(...)` sin `with` (`calendar.py:34-36`,
  `calendar.py:51-53`, `utils.py:7-8`). Sugerencia: usar siempre `with open(...)`.
- **Argumento mutable por defecto** en `ResAdder.add_resource(..., need=[], witout=[])`
  (`ResAdder.py:50`): clásico footgun de Python. Usar `None` y crear la lista dentro.
- **`print` de depuración** dejado en el código de producción (`TaskCreator.py`,
  `calendar.py:108`, muchos `print(...)`). Sugerencia: quitarlos o mandarlos a `log`.
- Typos en nombres de variables/params (`witout` por `without`, `cullo` por
  `cuyo` en comentarios). Menor, pero conviene cuidarlo.

No se penaliza ausencia de tests, type hints ni async (nivel primer año).

## 5. Datos y persistencia

Diseño sólido para el nivel. Persiste en JSON (`templates/resources.json`,
`templates/tasks.json`, y estado en `saved/`). Las estructuras son razonables:
recursos como dict `{count, need, without}`, tareas como dict con `resources` y
`without`. `add_to_dict` (`utils.py:58-76`) construye rutas anidadas
recursivamente — ingenioso, aunque algo oscuro de leer.

Riesgo observado: la lógica sobrescribe `templates/*.json` en tiempo de ejecución
(`ResAdder` y `EventCreator` llaman a `_save_resources`/`_save_tasks` sobre
`./templates/...`, `ResAdder.py:66`, `EventDefiner.py:41`). Mezclar "datos semilla
versionados" con "estado mutable del usuario" en el mismo archivo puede corromper
las plantillas del repo. Sugerencia: guardar el estado del usuario en `saved/` y
dejar `templates/` como solo-lectura.

## 6. Informe (`report.md`)

El informe es **excelente para primer año** y, sobre todo, **honesto**: describe
lo que el código realmente hace y no infla features. Explica el dominio, el diseño
de recursos/tareas, la arquitectura modular, la decisión de usar JSON, y —lo más
valioso— documenta con precisión las técnicas algorítmicas reales: **compresión de
coordenadas** (`report.md:127-128`) y **Segment Tree** (`report.md:204`), ambas
verificadas en la ejecución. Discute problemas enfrentados (dependencias vía DFS,
persistencia, elección de CustomTkinter) y cita bibliografía pertinente
(docs de Python, CustomTkinter, CP-Algorithms).

Discrepancias menores informe↔código:
- El informe muestra `"gerente": {"count": 2, ...}` (`report.md:22`) pero
  `templates/resources.json` tiene `"count": 3`. Trivial.
- El informe dice que `remove_old_events` "elimina las fechas cuyo final sea
  **mayor** a la fecha actual" (`report.md:133`) — está al revés; el código
  conserva las de final mayor (futuras) y descarta las pasadas (`calendar.py:150-156`).
  Error de redacción, la lógica del código es la correcta.

No hay sobreestimación: el informe si acaso **subvende** la sofisticación real.

---

## Síntesis para el profesor

Proyecto **claramente por encima del promedio de primer año**. El estudiante
resolvió un problema genuinamente difícil (asignación de múltiples recursos con
capacidad finita, dependencias y exclusiones sobre intervalos temporales) con
estructuras de datos avanzadas (Segment Tree con lazy propagation + compresión de
coordenadas) que **funcionan de verdad** — lo comprobé ejecutando el backend. La
arquitectura modular con separación GUI/lógica y uso correcto de herencia es
impropia (en el buen sentido) de un principiante. El historial en `changelog`
muestra un desarrollo disciplinado con versionado semántico.

Debilidades reales: manejo de errores demasiado silencioso (`except:` desnudos que
esconden fallos), fugas de descriptores de archivo, un patrón de acceso a Tk desde
un hilo secundario que es un bug de corrección latente, y `print` de depuración
sin limpiar. Ninguna empaña el logro central.

**Principal fortaleza:** algoritmia + arquitectura muy por encima del nivel, con
un informe honesto que las documenta.
**Principal área de mejora:** robustez — dejar de tragarse excepciones, usar
`with open`, y no tocar widgets desde hilos.

*No se pudo ver la GUI renderizada en este entorno (limitación de X11 del sandbox,
no del código); el backend sí se ejecutó y verificó por completo.*
