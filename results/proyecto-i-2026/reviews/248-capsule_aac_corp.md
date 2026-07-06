# Reporte detallado — Proyecto I (Programación, 1er año)

- **Issue:** #248
- **Repositorio:** https://github.com/AAC2212/CAPSULE_AAC_Corp
- **Estudiante:** Andro Aguilera Cazanave
- **Grupo:** C-121
- **Descripción del issue:** planificar tareas en un centro de investigación: eventos que duran varios días, añadir recursos y verificar recursos disponibles.
- **Clonado:** OK (`--depth 1`).

> **Nota de calibración:** este NO es el proyecto de consola típico que asume la rúbrica. Es una **aplicación web con Streamlit** (multi-página). El punto de entrada es `main.py` (`streamlit run main.py`), no un menú `input()`. La "ejecución dinámica" se hizo lanzando el servidor Streamlit real y, sobre todo, ejecutando cada página y el flujo de planificación con el arnés `streamlit.testing.v1.AppTest` (que ejecuta el script de verdad, interactúa con widgets y reporta excepciones). Esto es más fiel que `printf | python main.py`, que no aplica a una app Streamlit.

---

## Resumen de la ejecución dinámica (lo que realmente corrí)

Entorno aislado con `uv venv --python 3.12` (los f-strings con comillas anidadas de `Second_page.py:L16` exigen Python 3.12+; el sistema tenía 3.14, pero se aisló en 3.12.8). Dependencias instaladas: `streamlit 1.59.0`, `pandas 3.0.3`, `streamlit-calendar 1.4.0`.

1. **Sintaxis:** `py_compile` de los 6 `.py` → todos OK, sin errores.
2. **Servidor real:** `streamlit run main.py --server.headless true` arrancó correctamente (uvicorn en el puerto, `/healthz` → 200).
3. **Renderizado de páginas (AppTest, con la raíz del repo en `sys.path`, como hace `streamlit run main.py`):**
   - `Main_Page.py` → renderiza sin excepción. Con `planes` vacío muestra 4 banners `st.info` de estado vacío (correcto).
   - `Second_page.py` → renderiza sin excepción (1 selectbox, 2 botones, 2 number_input).
   - `Third_Page.py` → renderiza sin excepción (2 selectbox, 5 number_input, 1 slider, 3 checkbox, 1 botón). Con el evento por defecto ("Control de Calidad") sin recursos, muestra correctamente las advertencias de restricción.
   - `Fourth_Page.py` → renderiza sin excepción (página estática de info).
4. **Flujo completo de planificación (extremo a extremo):**
   - Planifiqué "Reunión de planes" con lugar "Salón de reuniones" + Director del centro + Director de negocios → todas las advertencias se limpiaron, apareció `st.success("Se cumplen todas las restricciones...")` y al pulsar "Planificar evento" → `st.success("Se ha añadido el evento correctamente")`. **Persistencia verificada:** el evento quedó escrito en `DataBase.json` con `ID:1`, fechas coherentes (`fin = inicio + duración`) y recursos correctos.
   - **Detección de conflictos (el núcleo del proyecto):** planifiqué "Creación de medicamentos" (Lab 2, 10 días) → se añadió. Un segundo evento solapado en el mismo lugar fue **correctamente rechazado**: `"No se pudo planificar el nuevo evento debido a que el lugar 'Laboratorio 2' estará ocupado por el evento 'Creación de medicamentos' desde el 2026-07-07 hasta el 2026-07-17."` Esto es exactamente lo que pide el issue y **funciona**.
   - **Validación de entradas:** con IA=25 (por encima del tope de 15 por evento) el programa muestra la advertencia y bloquea con `st.error("No se cumplen las restricciones")` — la capa de validación funciona.
   - `find_hole()` con `planes` vacío devuelve una sugerencia de intervalo coherente.
5. **Bug reproducido al ejecutar (ver dimensión 3).** Todo se corrió sobre una copia de la DB; al terminar restauré `DataBase.json` a su estado limpio (`planes: []`, 6 tipos de evento) y detuve el servidor.

---

## Dimensión 1 — Qué hace el programa

Aplicación **web** en Python (Streamlit) que simula un planificador de eventos para un "Centro de Investigación AAC" ficticio (medicamentos). Entrada: `main.py:L4-L11` define 4 páginas con `st.Page` y las enlaza con `st.navigation(...).run()`. Se ejecuta con `streamlit run main.py` y se usa en el navegador (`report.md:L11-L13`).

Flujo principal:
- **Eventos planificados** (`pages/Main_Page.py`): calendario interactivo (`streamlit_calendar`) + tabla (`pandas.DataFrame`) de los eventos ya planificados; permite eliminar por ID o eliminar todos los eventos ya terminados (`Main_Page.py:L58-L103`).
- **Eventos y recursos** (`pages/Second_page.py`): lista los tipos de evento y los recursos (personal), permite añadir un tipo de evento nuevo y aumentar la cantidad de un recurso (`Second_page.py:L30-L66`).
- **Planificar eventos** (`pages/Third_Page.py`): página central. El usuario elige tipo de evento, fecha, duración, lugar y recursos; el programa valida ~7 bloques de restricciones por tipo de evento y, si todo cumple, comprueba choque de fecha/lugar/recursos contra lo ya planificado antes de persistir (`Third_Page.py:L67-L289`).
- **Info** (`pages/Fourth_Page.py`): página estática descriptiva.

La lógica de negocio vive en `Funciones.py` (clase `Eventos` + funciones de I/O JSON). Persistencia en `DataBase.json` con tres claves: `eventos` (tipos), `recursos` (inventario de personal), `planes` (eventos concretos planificados).

**Veredicto:** hace lo que el issue describe. El dominio, la duración multi-día y la verificación de recursos/solapamientos están implementados y funcionan al ejecutar.

## Dimensión 2 — Organización del código

Buen nivel para 1er año:
- **Separación por capas:** backend (`Funciones.py`) vs. presentación (`pages/*.py`). El README documenta la estructura (`report.md:L15-L23`). Esto es más de lo que se espera de un principiante.
- **Uso de clases cuando el dominio lo pide:** `class Eventos` (`Funciones.py:L24-L103`) encapsula un evento y sus operaciones (`choque_fecha`, `choque_lugar`, `choque_recurso`, `check_resources`, `find_hole`, `detalles`). El estudiante explica en el informe que llegó a la clase razonando el dominio (tipos de evento vs. eventos planificados) — buena señal de comprensión, no de copia.
- **Funciones reutilizables** para el I/O: `cargar_eventos`, `planes`, `cargar_recursos`, `añadir_evento`, `añadir_plan`, `aumentar_recurso`, `delete_by_ID` (`Funciones.py:L4-L159`).
- **Nombres mayormente claros** en español (`choque_fecha`, `añadir_plan`, `proximo_id`), aunque hay abreviaturas crípticas: `b_s` (búsqueda binaria) y sus parámetros `l,r` (`Funciones.py:L144`), `able`/`dispo` (`Third_Page.py:L5-L6`), `res_necesarios`. Comentarios en español ayudan.

Puntos débiles de organización:
- **Duplicación de I/O:** cada getter reabre `DataBase.json` por separado; `añadir_evento`/`añadir_plan`/`aumentar_recurso`/`delete_by_ID` repiten el patrón leer-todo → mutar → reescribir-todo (`Funciones.py:L108-L159`). Una única función `cargar_db()`/`guardar_db(data)` evitaría abrir el fichero 3-4 veces por render.
- **Toda la validación de restricciones es una escalera gigante de `if`** en `Third_Page.py:L67-L217` (~150 líneas), con mucha repetición entre tipos de evento (el patrón "duración mínima / IP / IA / máx asistentes / IP==0 and IA!=0" se repite casi idéntico 6 veces). Una tabla de reglas por tipo de evento reduciría esto a ~1/5.
- **Estado global a nivel de módulo en `Funciones.py:L20-L21`** (`recursos = cargar_recursos()`, `planeados = planes()`) se lee **una sola vez al importar**; si el JSON cambia en runtime, esas globales quedan desactualizadas frente a las relecturas por página. Funciona por casualidad porque Streamlit reejecuta el proceso, pero es frágil.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Lo que funciona (ejecutado y verificado):** las 4 páginas renderizan sin `Traceback`; el flujo de planificar → validar → detectar choque de lugar → persistir funciona de punta a punta; la validación de restricciones bloquea entradas inválidas; la eliminación por ID (para IDs presentes) funciona; `find_hole` devuelve sugerencias. **Ninguna página lanzó excepción al ejecutarse correctamente.**

**Bug real reproducido — ID duplicado (`Third_Page.py:L286` vs `L12`):** hay **dos** cálculos de ID distintos. `Third_Page.py:L12` calcula `proximo_id = max(lista_id)+1` (correcto, "el siguiente al más alto", justo como el estudiante describe haber aprendido en el informe). Pero el evento realmente persistido se crea con `Funciones.Eventos(len(planes)+1, ...)` (`Third_Page.py:L286`), es decir, usa **la longitud de la lista + 1**, no `proximo_id`. Secuencia reproducida al ejecutar:
1. Planifiqué 3 eventos → IDs `1, 2, 3`.
2. Borré el ID `2` → quedaron `1, 3`.
3. Planifiqué otro → `len(planes)+1 = 2+1 = 3` → **quedó `[1, 3, 3]` (ID 3 duplicado).**

El estudiante afirma en el informe (`report.md:L83`) haber resuelto esto justamente con "el ID siguiente al más alto ya existente". La variable correcta (`proximo_id`) existe, pero **no se usa** en la línea de persistencia — se usa `len(planes)+1`. Sugerencia concreta: en `Third_Page.py:L286` reemplazar `len(planes)+1` por `proximo_id`.

**Fragilidad latente — `b_s` (búsqueda binaria) sin caso "no encontrado" (`Funciones.py:L144-L151`):** al ejecutar `b_s` con un ID inexistente lanza `IndexError: list index out of range` (o recursión infinita según los límites). En la UI está **protegido** porque `Main_Page.py:L85` verifica `if id not in lista_id` antes de llamar a `eliminar`, así que en el uso normal no explota; pero si con el bug de ID duplicado se llegara a llamar `delete_by_ID` sobre un ID repetido, `b_s` puede eliminar el elemento equivocado. Sugerencia: añadir la condición de parada `if l >= r: return -1` (o lanzar un error controlado) y manejar el `-1`.

**Inconsistencia de datos — `añadir_evento` escribe un tipo de evento con esquema distinto (`Funciones.py:L108-L114`):** los tipos preexistentes en `DataBase.json` tienen las claves `Nombre/Duracion/Lugar/Recursos necesarios`, pero `añadir_evento` escribe solo `{"Nombre","Duracion","Lugar"}` (sin "Recursos necesarios"). No rompió al ejecutar porque `Second_page.py` solo lee `Nombre/Duracion/Lugar`, pero es una inconsistencia de esquema que podría morder más adelante.

**Detalle menor — mensaje de "no hay hueco" (`Funciones.py:L95`):** el texto dice `"...en los próximos {max_search} días"` pero `max_search` es una **fecha** (`self.inicio + 30 días`), no un número de días, así que el mensaje sale raro. No se disparó en mis pruebas (siempre encontró hueco), pero es un bug de presentación.

**Discrepancia informe↔código en restricciones:** `Third_Page.py:L143-L144` avisa "Se necesitan al menos 3 encargados de almacén" pero la condición es `if EA<1` (o sea, exige al menos 1, no 3). El texto y la condición no concuerdan.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Positivo:
- Indentación consistente, uso natural de f-strings, comprensiones de lista simples (`Third_Page.py:L14`), bucles claros.
- Comentarios explicativos en español a lo largo de `Funciones.py` y `Third_Page.py` — el estudiante dice en el informe haber aprendido "por las malas" a comentar, y se nota el esfuerzo.
- Type hints ligeros en varias firmas (`cargar_eventos()->list`, `añadir_evento(nombre:str,...)`), que ni siquiera se le exigen a 1er año — bonus.

A mejorar:
- **Casi nulo manejo de errores en el I/O.** Ningún `try/except` alrededor de `open("DataBase.json")` / `json.load` (`Funciones.py:L4-L18`). Si el archivo falta o está corrupto, cualquier página revienta con `FileNotFoundError`/`JSONDecodeError`. Como todo el I/O usa rutas relativas, la app **solo funciona si el proceso corre desde la raíz del repo**; ejecutarla desde otra carpeta la rompería. Sugerencia: rutas absolutas basadas en `__file__` y un `try/except` que muestre un `st.error` amable.
- **Reapertura repetida del JSON** y las globales de módulo desactualizables ya mencionadas.
- Nombres abreviados (`b_s`, `l`, `r`, `able`, `dispo`, `lsit_res` — este último con typo, `Second_page.py:L54`).
- Reuso de nombre de variable de bucle `e` anidado en `Third_Page.py:L243-L263` (el `for e in planes` externo y otro `for e in planes` interno usan el mismo nombre, pisando la variable), lo cual funciona aquí pero es confuso y propenso a error.

(No penalizo ausencia de tests, type hints exhaustivos ni async, según la rúbrica.)

## Dimensión 5 — Datos y persistencia

- Persistencia en `DataBase.json` con estructura razonable (tres colecciones: `eventos`, `recursos`, `planes`). Verificado al ejecutar: planificar un evento lo escribe correctamente; eliminar lo quita.
- Estructuras adecuadas: diccionarios para recursos (nombre→cantidad), listas de dicts para eventos/planes.
- **Se persiste de verdad** (no solo memoria): confirmado leyendo el JSON tras planificar.
- Punto débil: `json.dump(datos, file)` sin `ensure_ascii=False` ni `indent` (`Funciones.py:L114,L121,L130,L158`) → el archivo queda en una sola línea con acentos escapados (`ó`). Funciona, pero es ilegible; sugerencia: `json.dump(datos, file, ensure_ascii=False, indent=2)`.
- El bug de ID duplicado (dim. 3) es también un problema de integridad de datos: puede dejar la colección `planes` con IDs no únicos, lo que rompe la premisa de "identificar cada evento por ID".

## Dimensión 6 — Informe (`report.md`)

El informe (que hace las veces de README) es **notablemente completo y honesto** para 1er año: describe funcionalidad, estructura, uso paso a paso, restricciones, problemas de desarrollo, decisiones de diseño, aprendizajes y agradecimientos. Refleja bien lo que el código hace.

- **Coincide con el código** en lo esencial: 4 páginas, calendario+tabla, planificación con restricciones, persistencia JSON, clase `Eventos`.
- **Discrepancia concreta:** en `report.md:L83` el estudiante afirma haber resuelto el problema de los IDs asignando "el ID siguiente al más alto ya existente". El código **calcula** eso (`proximo_id`, `Third_Page.py:L12`) pero **no lo usa** al persistir (`len(planes)+1`, `Third_Page.py:L286`), y el bug de ID duplicado sigue presente (reproducido al ejecutar). Es una sobreestimación involuntaria: creyó haberlo arreglado, pero la corrección no llegó a la línea que persiste.
- El informe es transparente sobre haber usado ayuda (profesores, conocidos, YouTube, IA) — se agradece la honestidad; no hay señales de código copiado sin entender: las decisiones de diseño están razonadas en su voz.
- Menor: el informe se llama `report.md` pero varias referencias internas dicen `README.md` (`Fourth_Page.py:L10`, `report.md:L8`).

---

## Síntesis para el profesor (orientativa, sin nota)

Trabajo **por encima de la media para un primer proyecto de 1er año**: eligió una app web (Streamlit) en vez de la consola típica, separó backend/frontend, usó una clase con sentido de dominio, y — lo más importante — **el núcleo funcional (planificar eventos multi-día + detectar choques de lugar/recursos + persistir) funciona al ejecutarlo de verdad**. El informe es completo y honesto.

Principales cosas a corregir: (1) el bug de ID duplicado (`len(planes)+1` en vez de `proximo_id`, `Third_Page.py:L286`) — importante porque contradice lo que el informe afirma y rompe la unicidad de IDs; (2) la `b_s` sin caso de "no encontrado"; (3) ausencia total de manejo de errores en el I/O + rutas relativas frágiles; (4) la escalera de `if` de restricciones es muy repetitiva. Ninguno de estos empaña que el proyecto **cumple lo que promete el issue** y demuestra comprensión real.
