# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #317
- **Repositorio:** https://github.com/charlieBrown046/Cinefilia
- **Estudiante:** Carlos R. Alvarez Quevedo
- **Grupo:** C-111
- **Descripción declarada:** Gestor de Festivales de cine con búsqueda de películas online.

---

## Nota metodológica importante

**Esto NO es una aplicación de consola ni un proyecto Python estándar.** Es un
**videojuego/aplicación Godot 3.5.3** cuyos scripts están escritos en Python
mediante el addon [godot-python (PythonScript) v0.50.9](https://github.com/touilleMan/godot-python).
Cada uno de los 16 archivos `.py` del estudiante (en `CinefiliaGodot/python_logic/`,
~1265 líneas propias) es una clase de nodo Godot: `from godot import *` +
`@exposed class X(Control)`. El código está **fuertemente acoplado al runtime de
Godot** (`get_node(...)`, `connect(...)`, `HTTPRequest`, `OS.get_datetime`,
`ImageTexture`, señales), por lo que **no es importable ni ejecutable bajo
CPython puro** — el módulo `godot` solo existe dentro del intérprete embebido
(Python 3.8) que trae el addon.

No hay `main.py`, `report.md`, `requirements.txt` ni `pyproject.toml` (el
`README.md` cumple la función de informe, ver Dimensión 6). El intérprete
declarado (uv, Python 3.12) no puede correr la GUI, y el binario del addon es
un intérprete Python 3.8 sin Godot editor, por lo que **arrancar la aplicación
completa headless no fue posible en este entorno** (falta el ejecutable Godot y
un display; la propia instalación indica ejecutar `windows_64_release.exe`).

**Cómo adapté la ejecución.** En lugar de asumir "no ejecutable", aislé la
**lógica de negocio separable** de la capa Godot y la ejecuté de verdad bajo
Python 3.12, además de validar la **capa de persistencia** contra los archivos
JSON reales versionados en el repo (`administratorSave.json`,
`eventHandlerData.json`). Ver Dimensión 3.

## Dimensión 1 — Qué hace el programa

Aplicación con tres secciones navegables mediante una barra superior
(`menu_SwitchButtons.py:8`, estados `popular` / `administration` / `festival`):

1. **Populares / Buscador** (`menu_findMovie.py`, `menu_PopularPoster.py`):
   consulta la API de TMDb (`https://api.themoviedb.org/3/...`) vía el nodo
   `HTTPRequest` de Godot y su señal `request_completed`. Muestra sinopsis,
   puntuación, fecha de estreno, duración y carga el póster como textura
   (`menu_findMovie.py:46-96`). Encadena tres requests: búsqueda → detalle
   (runtime) → póster.
2. **Administrar** (`administrator_scriptManager.py`, el módulo central, ~430
   líneas): gestiona tres tipos de recursos — **Cines** (nombre, imagen, días de
   servicio, horario apertura/cierre, capacidad), **Servicios de Comida** (tipos
   de producto y tiempos de reabastecimiento) y **Trabajadores** (limpiadores,
   venta de entradas, técnicos, dependientes). Persiste todo en
   `administratorSave.json` (`:195-290`) y lo recarga al iniciar (`:116-194`).
3. **Festivales** (`event_nameAndDate.py`, `event_dayManager.py`,
   `event_addCinema.py`): crea festivales con fecha inicial/final validadas y
   duración ≤30 días, genera la lista de días y los persiste en
   `eventHandlerData.json` (`event_nameAndDate.py:41-113`). Navegación día a día
   con botones adelante/atrás (`event_dayManager.py:52-73`). El propio README
   reconoce que esta sección **está incompleta** (falta el sistema de
   restricciones entre trabajadores/horarios/comida).

Un calendario mensual navegable se construye a partir de tiempos Unix
(`dateObj.py:52-98`) y una escena de intro reproduce una animación y cambia de
escena al terminar (`0_intro_Node2D.py`).

## Dimensión 2 — Organización del código

**Fortalezas.**
- **Convención de nombres por escena clara y consistente**: prefijos `menu_`,
  `administrator_`, `event_` que mapean 1:1 con las escenas de Godot. El README
  la documenta explícitamente. Facilita mucho la navegación de 16 archivos.
- **Modelado de dominio real** en `administrator_scriptManager.py:12-65`: clases
  internas `Movies`, `FoodService`, `Cinema` con `__init__` tipado y método
  `completeDescription()`. `Cinema.assignWorker()` (`:56-62`) usa el idiom
  `for/else` correctamente para validar que no falte ningún tipo de trabajador.
- **Separación razonable de responsabilidades por nodo**: cada script hace una
  cosa (el reloj rellena opciones, el switch cambia de estado, el manager
  serializa). Es la arquitectura idiomática de Godot y está bien aplicada.

**Debilidades.**
- **Imports basura por autocompletado del IDE** en casi todos los archivos:
  `from cProfile import label`, `from encodings.punycode import selective_find`,
  `from asyncio.windows_events import NULL`, `from operator import truediv`,
  `from pydoc import text`, `from sqlite3 import connect`… Ninguno se usa (salvo
  `NULL`, ver Dimensión 3). Son ~2-3 líneas muertas por archivo.
- **Encadenamiento frágil de `get_node()`** con rutas larguísimas y navegación
  por `get_parent().get_parent().get_parent()` (`administrator_addButton.py:14-24`,
  `administrator_scriptManager.py:91-113`). Cualquier cambio en el árbol de
  escenas rompe estas rutas silenciosamente. Es difícil de mantener, aunque es
  un mal común en proyectos Godot.
- **Números mágicos de nodos** ("0", "1"…"9" como nombres de campos de
  hora/capacidad) que obligan a aritmética manual de dígitos (`:137-146`).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Confirmé primero que **ningún módulo importa bajo CPython** (`ModuleNotFoundError:
No module named 'godot'`) — esperado. Luego:

1. **`py_compile` de los 16 módulos con Python 3.12: PASA** (34 `.pyc`
   generados, sin errores de sintaxis). El código es sintácticamente válido.
2. **Validación de fechas de festival (lógica separable de
   `event_nameAndDate.py:22-70`).** Reimplementé el árbol de decisión sin la capa
   Godot y lo corrí con 11 casos. Resultados **todos correctos**:
   - Rango de 5 días con nombre válido → `OK total_days=5`.
   - Nombre por defecto `"Nombre..."` → rechazado ("sin nombre").
   - Inicio > fin → rechazado ("inicio > fin").
   - Rango de 46 días → rechazado (">30 días").
   - `día 32`, `mes 13`, `29-feb-2025` (no bisiesto) → **rechazados como fecha
     inválida** (gracias a `datetime.strptime`, que valida el calendario).
   - `29-feb-2024` (bisiesto) → aceptado (`total_days=2`). ✔ correcto.
   - Frontera exacta: 30 días → OK; 31 días → rechazado. ✔ límite correcto.
   Esta lógica es **robusta ante entradas inválidas**: no revienta, delega la
   validación de calendario a `strptime` y devuelve `None` en `except ValueError`.
3. **Persistencia de festivales.** Reconstruí la generación de `days_list`
   (`:100-104`) para un festival de 3 días → produce
   `[{"01-01-2025": []}, {"02-01-2025": []}, {"03-01-2025": []}]`. Correcto.
4. **Navegación día a día (`event_dayManager.py:52-73`) contra el
   `eventHandlerData.json` real del repo.** Contiene dos festivales
   ("Festival de Cine Europeo": 2 días, "aasd": 3 días). Emulé el avance/retroceso:
   primer día y último día se leen correctamente (`08-03-2025`/`09-03-2025` y
   `01-02-2000`/`03-02-2000`). La lógica de límites (alerta en primer/último día)
   es correcta.
5. **Round-trip de la capa de administración contra `administratorSave.json`
   real.** El JSON versionado tiene 2 cines. La aritmética de
   codificación/decodificación de horas y capacidad round-trips exacto:
   `open=744 → 12:24`, `closed=1212 → 20:12`, `capacity=400 → dígitos 0-4-0-0`.
   El esquema serializado (`:275-289`) coincide campo por campo con el esquema
   deserializado (`:120-165`). **La persistencia funciona.**

**Bugs reales encontrados (del estudiante, no del entorno):**

- **B1 — Import Windows-only que rompe la portabilidad
  (`menu_findMovie.py:2`).** `from asyncio.windows_events import NULL` **falla al
  importar en Linux/macOS** (`ImportError: win32 only`, verificado). Además `NULL`
  se usa como valor por defecto de parámetro (`:46,79,90`). En Windows funciona
  por casualidad; fuera de Windows el módulo entero no carga. Debería ser `None`.
- **B2 — `_ready` del administrador sin manejo de archivo ausente
  (`administrator_scriptManager.py:117`).** Abre `administratorSave.json` sin
  `try/except`; en la **primera ejecución** (sin archivo de guardado) lanzaría
  `FileNotFoundError` y la escena no cargaría. Nótese que `event_nameAndDate.py`
  y `event_dayManager.py` **sí** guardan esa apertura con try/except — la
  inconsistencia sugiere que el estudiante conocía el patrón pero lo olvidó aquí.
- **B3 — `IndexError` en búsqueda sin resultados
  (`menu_findMovie.py:51`).** `data["results"][0]["overview"]` se accede sin
  comprobar que la lista no esté vacía. Si TMDb no devuelve películas (título
  inexistente), revienta. El acceso al póster (`:71`) sí está protegido con
  `try/except IndexError`, pero los tres campos anteriores no.
- **B4 — Off-by-one latente en la recarga (`administrator_scriptManager.py:126`
  y siguientes).** Se indexa con `data["cinemas"][child.get_index()-1]`. Para el
  primer hijo (índice 0) esto da `-1`, que en Python indexa el **último**
  elemento. Funciona en la práctica solo si el orden de creación compensa el
  desfase; es frágil y difícil de razonar.
- **B5 — Typo consistente `drinReplace`** (`:32,37,280`): el atributo se guarda
  como `drinReplace` (falta la 'k') pero la clave JSON es `drinkReplace`.
  **No** rompe porque el typo se lee de vuelta con el mismo nombre, así que
  round-trips; es solo inconsistencia de nombres.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores desigual**: bien en las fechas (`strptime` + `except
  ValueError`) y en dos aperturas de JSON, ausente en la tercera (B2) y en el
  parseo de la API (B3). El estudiante conoce el patrón; falta aplicarlo
  uniformemente.
- **Secretos hardcodeados** (`menu_findMovie.py:31,34`): la API key
  (`e77caec8b1c38da154069225c507ca08`) y un Bearer token JWT están commiteados en
  el repo. Para un proyecto docente es aceptable, pero conviene mencionar que en
  producción esto no debe versionarse.
- **Cadenas `if/elif` largas** para mapear día en inglés → español
  (`dateNew.py:33-49`): un diccionario `{"Monday": "Lunes", ...}` sería más corto
  y directo. Mismo patrón en `clockCinema.py:11-20`.
- **`print("Hola")` / `print("Hola2")`** de depuración dejados en callbacks
  (`dateObj.py:100,104`), y `print(2+2)` en la intro (`0_intro_Node2D.py:25`).
- **Comentarios honestos y útiles**: el código está bien comentado en español,
  incluyendo TODOs sinceros ("posible error de ejecución cíclico, luego lo
  resuelvo"). Se agradece la transparencia.

## Dimensión 5 — Datos y persistencia

Sólido para primer año. Dos JSON con esquema claro y coherente:

- **`administratorSave.json`**: `{cinemas: [...], foodServices: [...], workers:
  {cleaner|ticket|tech|shop: [count, [nombres]]}}`. Horas codificadas como
  minutos totales, capacidad como entero; la codificación/decodificación por
  dígitos round-trips exacto (verificado, Dimensión 3.5).
- **`eventHandlerData.json`**: `{Event: {nombre: {start_date, end_date,
  total_days, days: [{fecha: []}]}}}`. La lista de días se materializa una por
  fecha, lista para colgar películas.

El modelo mediante clases de dominio (`Cinema`, `FoodService`, `Movies`) y su
serialización con `json.dump(..., ensure_ascii=False, indent=4)` es correcto y
legible. La deserialización reconstruye la UI reemitiendo señales `button_up`,
enfoque ingenioso aunque acoplado.

## Dimensión 6 — Informe (`README.md`)

No hay `report.md`; el `README.md` (~1500 palabras) cumple la función de informe
y es de **buena calidad**: con índice, explicación de la elección tecnológica
(Godot + godot-python), documentación de la integración TMDb con ejemplo de
código real, y descripción sección por sección del workflow.

**Honestidad del informe: alta.** El estudiante **declara explícitamente que la
sección de Festivales está incompleta** ("Esta sección de proyecto aún no se
encuentra terminada… ¿Qué falta? Hay que agregar todo el sistema de limitaciones
entre trabajadores, horarios y servicios de comida", líneas 116-121). Esto
coincide con el código: las reglas de negocio de trabajadores descritas en el
README (limpiadores 10 min al final, venta de entradas 30 min, técnicos y
alcohol, dependientes en horario de almuerzo) **no están implementadas** en
ningún módulo — son especificación, no código. El informe **no exagera**: no usa
"demuestra"/"prueba" ni afirma validación que no exista. La descripción de las
tres secciones coincide con lo verificado.

Detalle menor: el bloque de Contacto conserva el placeholder "**Tu Nombre**"
(línea 132) en lugar del nombre real.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **ambicioso y sólido** para primer año, con un mérito poco común: en
vez de la típica app de consola, el estudiante levantó una **aplicación gráfica
completa en Godot escribiendo toda la lógica en Python** mediante un addon no
trivial, integró una **API externa real (TMDb)** con requests encadenados, y
construyó una **capa de persistencia JSON coherente** que verifiqué round-trip
contra los datos reales del repo. La lógica separable que pude ejecutar (validación
de fechas, generación y navegación de días de festival, codificación de
horas/capacidad) es **correcta y robusta ante entradas inválidas**. El informe es
honesto sobre lo que falta. Los defectos son de nivel principiante y en su
mayoría menores: imports basura del IDE, un import Windows-only que rompe la
portabilidad, manejo de errores desigual y un par de bugs latentes (IndexError en
búsqueda vacía, FileNotFoundError en primer arranque).

**Principal fortaleza:** ambición y ejecución técnica reales — GUI en Godot con
lógica Python, integración de API viva y persistencia JSON verificablemente
correcta, todo con una convención de nombres limpia.

**Principal área de mejora:** robustez y portabilidad — sustituir el import
`asyncio.windows_events.NULL` por `None`, envolver la lectura de
`administratorSave.json` en try/except, y proteger el acceso a
`data["results"][0]` cuando la búsqueda no devuelve resultados. Limpiar los
imports muertos que dejó el autocompletado.
