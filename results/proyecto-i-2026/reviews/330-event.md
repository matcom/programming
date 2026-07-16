# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #330
- **Repositorio:** https://github.com/Drakend-Zero/Event
- **Estudiante:** Antoine Hernández Apezteguía
- **Grupo:** C-111
- **Descripción declarada:** Planificador de eventos de Calistenia. Se introducen las cantidades generales de "inventario" para satisfacer las necesidades de cada subevento según la modalidad. Si la cantidad de participantes supera lo disponible, el planificador lo avisa; también gestiona la relación entre elementos del inventario para su uso individual o conjunto, organizando el evento general y los subeventos.

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una app **Streamlit** (`import streamlit as st`, `Back_processes.py:2`). El punto de entrada real es `Main.py`, que instancia `Pantallas()` y llama `Control_de_pantalla()`. La verificación automática del issue reportó `ModuleNotFoundError: streamlit` — eso es un fallo de entorno (falta la dependencia declarada), no un error del código.

Adapté la ejecución así:
1. `uv venv --python 3.12` + `uv pip install streamlit` (no hay `requirements.txt` ni `pyproject.toml`; deduje la única dependencia del `import`).
2. `py_compile` de los dos módulos.
3. Arranque **headless** del servidor Streamlit (`streamlit run Main.py --server.headless true`).
4. Ejecución directa de la **lógica de negocio** (SQLite y métodos de las clases) con los datos reales del repo (`Evento.db`), reproduciendo en aislamiento las mismas sentencias que ejecuta la GUI, para distinguir bugs del estudiante de fallos del entorno.

## Dimensión 1 — Qué hace el programa

La app organiza pantallas mediante `st.session_state.pantalla` como máquina de estados (`Back_processes.py:58-80`). Hay cuatro pantallas: `inicio`, `nuevo`, `gestor`, `sesion`, más una barra lateral con botones de navegación (`Back_processes.py:72-80`).

- **`pantalla_inicio`** (`Back_processes.py:5-9`): título "Eventos", llama a `Show().show()` y `Show().Fecha()`.
- **`Show.show`** (`Back_processes.py:127-153`): lista las tablas de `Evento.db` como "sesiones" en un `selectbox`, cuenta sus filas y, si hay, ofrece un segundo `selectbox` de "competencias creadas".
- **`iniciar_sesion_nueva`** (`Back_processes.py:45-53`): pide un nombre y crea una tabla SQLite nueva con ese nombre (`crear_tabla`).
- **`pantalla_nueva`** (`Back_processes.py:11-33`): formulario para una competencia (nombre, capacidad, duración) que debería insertar una fila.
- **`pantalla_gestion`** (`Back_processes.py:36-43`) + **`Recorrido`** (`Back_processes.py:165-233`): recorre las tablas, permite seleccionar evento/competencia y ofrece botones Eliminar / Cambiar nombre / Cancelar.

El **modelo mental** es: cada "sesión/evento" es una **tabla** SQLite; cada "competencia" es una **fila** de esa tabla. Ojo: **nada del "inventario"** descrito en el README (barras fijas, paralelas, dependencias entre elementos, control de capacidad restante) existe en el código. Lo implementado es un CRUD parcial de eventos, no el planificador de inventario prometido.

## Dimensión 2 — Organización del código

Fortalezas:
- Separación en clases por responsabilidad: `Pantallas` (routing/UI), `Manejo_de_datos` (escritura), `Show` (lectura para inicio), `Recorrido` (lectura/edición para gestión). Es una intención de arquitectura razonable para un principiante (`Back_processes.py:4,82,125,165`).
- La máquina de estados por `session_state` es un patrón correcto en Streamlit (`Back_processes.py:58-80`).

Debilidades:
- **Conexiones a SQLite duplicadas y descoordinadas.** `Manejo_de_datos` abre una conexión a nivel de clase (`Back_processes.py:83-84`) y otra dentro de `crear_tabla` (`Back_processes.py:89-90`); `Show`, `Fecha` y `Recorrido` abren la suya cada vez. No hay una capa única de acceso a datos; el estado de commits/cierres queda inconsistente (p. ej. `creador_competencia` cierra la conexión de clase en `Back_processes.py:123`, dejándola inutilizable para llamadas posteriores).
- **Acoplamiento por atributos de instancia no inicializados.** `self.sesiones`, `self.revision` sólo existen si `show()` corrió antes con datos (`Back_processes.py:140,146`). Otros métodos (`Fecha` línea 162, `creador_competencia` línea 115, `Recorrido` líneas 228/233) crean un `Show()` **nuevo** y leen esos atributos, que no existen → `AttributeError`. Es el defecto estructural central.
- Nombres inconsistentes (mezcla de español/inglés: `Show`, `Recorrido`, `Manejo_de_datos`, `Back_processes`).
- `delete_table` (`Back_processes.py:101`) y `creador_competencia` mezclan firmas con/sin `self` de forma inconsistente.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Qué corrí y qué observé:**

1. **`py_compile` de ambos módulos** → OK. No hay errores de sintaxis.
2. **Arranque headless de Streamlit** → el servidor levanta correctamente: `Uvicorn server started on :::8531`, `curl /` devuelve **HTTP 200** y `/_stcore/health` devuelve **`ok`**. La app arranca; los errores son de **lógica en tiempo de ejecución**, no de arranque.
3. **Inspección de `Evento.db`** → dos tablas: `intento` e `intento2`, ambas **vacías**, con columnas `(nombre, capacidad, duracion, fecha)`.
4. **BUG — `SELECT name` contra columna inexistente.** El esquema real usa la columna `nombre`, pero el código consulta `name`:
   - `Back_processes.py:145` `SELECT name FROM {self.sesiones}`
   - `Back_processes.py:149` `SELECT name FROM eventos WHERE name = ?`
   - `Back_processes.py:210` `SELECT name FROM {tabla}`
   Reproducido: `sqlite3.OperationalError: no such column: name`. En cuanto una sesión/tabla tenga al menos una fila, la pantalla de inicio y la de gestión **revientan**.
5. **BUG — tabla `eventos` inexistente.** `Back_processes.py:149` consulta `FROM eventos`, tabla que nunca se crea. Reproducido: `sqlite3.OperationalError: no such table: eventos`.
6. **BUG — `INSERT` malformado.** `Back_processes.py:117`: `INSERT INTO {S.sesiones} VALUES (name,capacidad,duracion)` pasa **nombres de columna** como si fueran valores literales, además con 3 placeholders inexistentes. Reproducido en tabla equivalente: `sqlite3.OperationalError: no such column: name`. **Crear una competencia nunca funciona.**
7. **BUG — `creador_competencia` inalcanzable.** `Back_processes.py:114-115`: crea `S = Show()` y evalúa `if S.revision:`. En un `Show` recién creado el atributo `revision` no existe. Reproducido: `AttributeError: 'Show' object has no attribute 'revision'`. Todo el flujo de "Finalizar" en `pantalla_nueva` (`Back_processes.py:30-33`) aborta aquí.
8. **BUG — `fecha.strftime()` sin formato.** `Back_processes.py:161`: `fecha.strftime()` sin argumento. Reproducido: `TypeError: strftime() missing required argument 'format'`. Además el resultado se descarta (no se asigna) y luego se guarda `fecha` cruda. Marcar fecha revienta.
9. **BUG — `Recorrido` en Eliminar/Modificar competencia.** `Back_processes.py:227-233`: `ses = Show()` y luego `ses.sesiones` — atributo inexistente en instancia fresca → `AttributeError`. Además `DELETE {ses.sesiones} WHERE...` (falta `FROM`) y `UPDATE {ses.sesiones} WHERE...` (falta `SET`) son SQL malformado. Botones Eliminar/Modificar de competencia no funcionan.
10. **BUG — `ALTER TABLE {self.see}` incompleto.** `Back_processes.py:201`: `ALTER TABLE {see}` sin la cláusula de renombrado → error de sintaxis SQL. Renombrar evento no funciona.

**Fallos de entorno (no del estudiante):** el `ModuleNotFoundError: streamlit` del verificador automático es sólo la dependencia no instalada. En sí, el arranque de la GUI es correcto.

**Conclusión de corrección:** El servidor arranca, pero **prácticamente todos los flujos que escriben o leen filas fallan** con `OperationalError`/`AttributeError`/`TypeError`. Lo único que funciona de punta a punta es crear una tabla vacía (`crear_tabla`, `Back_processes.py:87-98`) y navegar entre pantallas mientras las tablas estén vacías.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **f-strings en SQL con nombres de tabla** (`Back_processes.py:91,106,117,141,145,162,192,201,207,210`). Para 1er año es entendible, pero es la fuente de injection y de los errores de "no such table/column". Como los nombres de tabla no se pueden parametrizar con `?`, conviene al menos validar/normalizar el nombre.
- **Valores de retorno descartados**: `dr.isoformat()` (`Back_processes.py:28`) y `fecha.strftime()` (`Back_processes.py:161`) no se asignan a nada.
- **Atributos de instancia creados dentro de un método y usados desde otro** sin garantía de orden — el patrón que rompe la mitad de la app. Mejor pasar los datos como parámetros o guardarlos en `session_state`.
- **`except` ausente**: ninguna consulta está envuelta en `try/except`, así que cualquier error se propaga como `Traceback` en la UI en vez de un `st.error` amable.
- Comentarios honestos del propio autor ("Terminar de crear...", "Completar la implementación...") marcan que el proyecto quedó **a medias**, lo cual el README no reconoce.

## Dimensión 5 — Datos y persistencia

- Persistencia en **SQLite** (`Evento.db`), buena elección para el dominio y bien motivada en el README (migró de JSON a SQLite por facilidad de borrar/renombrar).
- Modelo: **una tabla por evento**, **una fila por competencia**. Esto es un antipatrón (usar el nombre del evento como nombre de tabla obliga a f-strings en SQL y rompe la parametrización), pero es una decisión comprensible para un principiante.
- El esquema (`Back_processes.py:91-96`) define `nombre, capacidad, duracion, fecha`, pero el resto del código consulta `name` — **el código de escritura y el de lectura no comparten el mismo nombre de columna**, de ahí los fallos. Es el desajuste que hunde el proyecto.
- **No hay ningún modelo de "inventario"** (barras, dependencias, capacidad restante). La persistencia sólo cubre eventos/competencias vacías.

## Dimensión 6 — Informe (`report.md`)

No hay `report.md`; el `README.md` cumple ese rol (unas ~450 palabras, por debajo del mínimo esperado).

Discrepancias con el código:
- El README describe un **planificador de inventario** con control de capacidad restante y dependencias entre elementos ("si tuvieras 20 barras fijas... 10 barras fijas y 10 barras paralelas... con las restantes de cada una"). **Nada de esto está implementado.** No hay tabla, campo ni lógica de inventario en `Back_processes.py`. Es la exageración principal: describe la app soñada, no la construida.
- Afirma que "todo dato es modificable en cualquier momento después de guardado". En la práctica, guardar (`creador_competencia`) y modificar (`Recorrido`) **fallan con excepción** (ver Dimensión 3, puntos 6-9).
- La sección *Proceso* (aprendizaje de BD, clases, Streamlit) es honesta y bien escrita, y es la parte más valiosa del informe.

---

## Valoración global (orientativa, sin nota numérica)

El proyecto muestra **buena ambición e intención arquitectónica** para 1er año: separación en clases por responsabilidad, una máquina de estados por `session_state` correcta, elección justificada de SQLite y un frontend Streamlit que **arranca sin problemas** (HTTP 200, health `ok`). El estudiante claramente aprendió conceptos nuevos (BD, clases, Streamlit) y lo cuenta con honestidad en el informe.

Sin embargo, el proyecto **quedó a medias y no es funcional en sus flujos centrales**. Al ejecutarlo de verdad, prácticamente toda operación que lee o escribe filas revienta: desajuste entre la columna `nombre` (esquema) y `name` (consultas), `INSERT` malformado, atributos `revision`/`sesiones` leídos sobre instancias `Show()` recién creadas, `strftime()` sin formato y sentencias SQL incompletas (`ALTER TABLE`, `DELETE`/`UPDATE` sin `FROM`/`SET`). Lo único que funciona de punta a punta es crear tablas vacías y navegar. Además, el "inventario" que es el corazón de la descripción **no existe en el código**.

- **Principal fortaleza:** intención arquitectónica sólida (clases por responsabilidad + máquina de estados Streamlit que arranca) y honestidad sobre el proceso de aprendizaje.
- **Principal área de mejora:** unificar el acceso a datos y cerrar el desajuste esquema↔consultas. Empezar por: (1) usar `nombre` consistentemente en todas las consultas, (2) arreglar el `INSERT` con placeholders `?` y valores reales, (3) dejar de leer atributos de un `Show()` recién creado — pasar los datos como parámetros o vía `session_state`. Con esos tres arreglos, la mitad de la app volvería a la vida.

**Veredicto: con problemas (no funcional en sus flujos centrales; arranca pero revienta al operar).**
