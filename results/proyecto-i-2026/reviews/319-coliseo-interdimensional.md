# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #319
- **Repositorio:** https://github.com/diegosct11/Coliseo-Interdimensional.git
- **Estudiante:** Diego Alejandro Santa Cruz Torres
- **Grupo:** C121
- **Descripción declarada:** Aplicación web (Streamlit) para explorar un catálogo de héroes capturados por el villano Mojo y planificar combates en un "Coliseo Interdimensional", respetando reglas de categoría (con/sin poderes), co-requisito de arma, exclusión de armas para superpoderosos y disponibilidad por fecha/sala.

---

## Nota metodológica importante

**Esto NO es una aplicación de consola.** Es una app web multipágina de **Streamlit 1.59** (`main.py` define `st.Page` + `st.navigation`, `pages/*.py` son las páginas). No tiene `input()`; alimentarla con `printf` no tendría sentido.

Cómo adapté la ejecución:

1. `uv venv --python 3.12` + `uv pip install streamlit pandas` (declaradas en `requirements.txt`; instaló Streamlit 1.59.2).
2. **`py_compile`** de los 7 módulos: todos compilan.
3. **Arranque real headless**: `streamlit run main.py --server.headless true` levantó el servidor Uvicorn sin errores de importación ni de API, y `curl` a la raíz devolvió **HTTP 200** (health `/healthz` también 200). El árbol de navegación se construye correctamente.
4. **Lógica de negocio ejecutada de verdad**: importé `core.py` y probé `cargar_recursos`, `cargar_combates`, `guardar_combates` con los datos reales del repo (roundtrip de guardado/recarga verificado).
5. **Ejecución de cada página con `streamlit.testing.v1.AppTest`** (motor oficial de Streamlit que corre el script de la página de verdad y captura excepciones), incluyendo simulación de selección de fecha y cambio de categoría.

Aclaración sobre un falso positivo: al ejecutar `Info_EL_Coliseo.py` y `Listado_de_Combates.py` **aisladas** con AppTest, ambas lanzan `KeyError: 'url_pathname'` en la línea del `st.page_link(...)`. Esto **no es un bug del estudiante**: `st.page_link` requiere el contexto de `st.navigation` que solo existe cuando se entra por `main.py`, y AppTest carga la página suelta. En el flujo real (arranque por `main.py`, HTTP 200) esas páginas funcionan. Lo distingo explícitamente en la Dimensión 3.

## Dimensión 1 — Qué hace el programa

Cuatro páginas navegables desde `main.py:15-41`:

- **Info - El Coliseo** (`pages/Info_EL_Coliseo.py`): página narrativa/estática con texto e imágenes (`mojo.jpg`, `coliseo.jpg`).
- **Planificar Combate** (`pages/Planificar_Combate.py`, 520 líneas — el corazón del proyecto): flujo guiado para armar un combate. Selecciona **fecha** (`date_input`, `min_value="today"`, línea 119), **sala**, **categoría** (radio con/sin poderes, 261), **combatiente A y B** con su **arma** (dos `st.form`, 298-435), y **patrocinador** (457). Al confirmar (502) valida que todos los campos estén llenos, persiste el combate en `data/combates.json` y resetea el formulario.
- **Listado de Combates** (`pages/Listado_de_Combates.py`): muestra los combates planificados en un `st.dataframe` (11) y permite **borrar** uno por patrocinador (26-44), persistiendo el cambio.
- **Combatientes** (`pages/Combatientes.py`): catálogo visual de los 32 héroes en dos pestañas (con/sin poderes) con imágenes.

La lógica de disponibilidad es la parte más lograda: al elegir una fecha, `Planificar_Combate.py:166-206` recorre los combates ya guardados ese día y calcula salas/combatientes/armas **ocupados**, restándolos de los **libres**; si las 3 salas están tomadas, bloquea el día (187-198). Verifiqué en ejecución que con `combates.json` vacío una fecha futura libera correctamente las 3 salas, 16 con-poderes + 16 sin-poderes y las 20 armas.

## Dimensión 2 — Organización del código

**Fortalezas:**
- Separación de páginas por responsabilidad (`pages/`), idiomática en Streamlit.
- Persistencia aislada en `core.py` (guardar/cargar recursos y combates), buen instinto de no mezclar I/O con UI.
- Datos externos en `data/*.json`, no hardcodeados en el código — muy correcto.
- Nombres de funciones descriptivos (`validar_combatiente`, `validar_patrocinador`, `validar_contienda`, `reset`).

**Debilidades:**
- `Planificar_Combate.py` es un script de 520 líneas con mucha lógica repetida. Los bloques de reinicialización de `st.session_state.libres`/`ocupados` aparecen **cuatro veces casi idénticos** (líneas 54-68, 90-105, 132-159, 272-278). Extraer una función `estado_inicial_recursos()` reduciría el archivo a la mitad.
- La rama A y la rama B (col1 líneas 292-363, col2 líneas 364-435) son **copia-pega** con "A"/"B" cambiados; una sola función parametrizada por combatiente eliminaría la duplicación.
- Detalle menor: la etiqueta del arma del Combatiente B dice "Combatiente A" (líneas 395 y 323 reutilizadas) — copy-paste sin corregir.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Qué corrí y qué observé:**

1. **`py_compile` de los 7 módulos** → todos OK.
2. **`streamlit run main.py` headless** → Uvicorn arranca limpio, **HTTP 200** en `/` y `/healthz`. Sin errores de importación ni de parámetros de API (usa API muy nueva: `text_alignment`, `horizontal_alignment` en contenedores, `st.Page`, `st.navigation` — todo soportado por 1.59.2).
3. **`core.py` roundtrip** → `cargar_recursos()` devuelve 32 combatientes / 20 armas / 3 salas; `guardar_combates({...})` + `cargar_combates()` recupera exactamente lo guardado. Persistencia correcta.
4. **`AppTest` de `Planificar_Combate.py`** → renderiza sin excepción; 1 date_input, 5 selectbox, 1 radio, 4 botones. Simulé elegir una fecha futura: `pelea["Fecha"]` se fija, salas/combatientes/armas libres se computan bien, mensaje "El Coliseo está libre ese día". Cambié la categoría a "Sin Poderes": el estado se reinicia sin reventar. **Esta página, que es el núcleo del proyecto, funciona.**

**Bugs reales del estudiante encontrados:**

5. **`Combatientes.py` CRASHEA por rutas de imagen con mayúsculas incorrectas** (bug real, no de entorno). AppTest de esa página muere en `Combatientes.py:13` con `MediaFileStorageError: Error opening 'images/con poderes/Flash.jpg'`. En disco el archivo es `flash.jpg` (minúscula). **11 de las 32 imágenes** referenciadas no existen por mayúsculas/minúsculas: `Flash`, `Invincible`, `Wonder Woman`, `Superman`, `Wolverine`, `John Wick`, `Espartaco`, `Toni Stark`, `Napoleon`, `Punisher`, `Elpidio`. En Linux/macOS (sistema de archivos sensible a mayúsculas) **la página del catálogo se rompe con `st.image`**. En Windows probablemente "funcionó" porque su FS no distingue mayúsculas — por eso el estudiante no lo notó. Es el defecto más importante.

6. **Falso positivo aclarado:** `Info_EL_Coliseo.py` y `Listado_de_Combates.py` lanzan `KeyError: 'url_pathname'` en `st.page_link` **solo bajo AppTest aislado** (falta el contexto de `st.navigation`). En el arranque real por `main.py` no ocurre (HTTP 200). **No cuenta como bug.**

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Bug en la regex del patrocinador** (`Planificar_Combate.py:25`): `re.match(r'^[A-Za-z\\\\s-]+$', ...)`. El `\\\\` sobre-escapado hace que la clase de caracteres sea "letras + barra invertida + la letra s + guion", **no** `\s` (espacio). Verificado ejecutando: `"Ana Maria"` (con espacio) es **rechazado** aunque el mensaje dice "solo letras y guiones". Debería ser `r'^[A-Za-z\s-]+$'` (una sola barra) si quiere permitir espacios, o `r'^[A-Za-z-]+$'` si no.
- `except Exception as e: print(...)` en `core.py:22,32`: captura demasiado amplia y `print` no se ve en una app web; mejor `st.error(...)` o dejar propagar.
- `cargar_combates`/`cargar_recursos` hacen `return` implícito (`None`) si el archivo no existe (`core.py:16, 26`). Verifiqué que devuelven `None`; los llamadores luego hacen `len(...)`/`.keys()`/`.update(...)`, que crashearían. No se dispara porque los JSON existen en el repo, pero es frágil — convendría `return {}`.
- Comparaciones `== None` / `!= None` en vez de `is None` (varias en `Planificar_Combate.py`). Detalle idiomático menor.
- Sobra la coma vacía y líneas en blanco dentro del literal (línea 196). Cosmético.

## Dimensión 5 — Datos y persistencia

- Modelo de datos limpio: `recursos.json` = `{combatientes: {nombre: "Con/Sin Poderes"}, armas: [...], salas: [...]}`; `combates.json` = `{combates: {patrocinador: {...campos del combate...}}}`.
- Persistencia con `json.dump(..., indent=4)` y lectura con `json.load` — correcto y legible.
- Detalle: `guardar_combates` envuelve en `{"combates": ...}` y `cargar_combates` desenvuelve con `.get("combates")` — el contrato es coherente entre ambos (verificado en el roundtrip). Bien hecho.
- Usa el **patrocinador como clave única** del diccionario de combates; de ahí la validación "ya existe un patrocinador con ese nombre" (24). Decisión razonable, aunque limita a un combate por patrocinador.

## Dimensión 6 — Informe (`report.md`)

**No hay `report.md`** (la verificación automática ya lo marcó). Existe un `README.md` bien escrito que hace las veces de informe: describe el tema, las características, y sobre todo las **tres reglas del coliseo** (co-requisito arma, exclusión de armas para poderosos, no mezclar categorías).

Contraste README ↔ código:
- Las tres reglas están **efectivamente implementadas** en `Planificar_Combate.py`: la categoría filtra combatientes por poderes (303-312, 375-386), sin-poderes obliga a elegir arma real y con-poderes fuerza "Ninguna" deshabilitado (315-327, 387-399). El README **no exagera** en esto — el código respalda lo que declara. Muy bien.
- El README **no menciona** el catálogo roto de imágenes en Linux ni instrucciones de que las mayúsculas de los archivos importan.
- Instrucciones de instalación/ejecución correctas (`streamlit run main.py`).
- Falta la sección de informe formal requerida (≥2000 palabras): el README es más un "getting started" que un informe técnico del diseño y las decisiones.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso** para primer año. El estudiante eligió Streamlit (framework no trivial), lo estructuró en páginas, separó la persistencia en `core.py`, externalizó los datos a JSON, y — lo más meritorio — implementó de verdad una lógica de disponibilidad por fecha con cálculo de recursos libres/ocupados que **verifiqué funcionando en ejecución**. Las tres reglas del dominio están realmente codificadas, no solo prometidas en el README. El corazón del proyecto (planificar un combate) arranca y funciona.

Lo que lo baja: (1) el catálogo de combatientes **crashea en Linux/macOS** por 11 rutas de imagen con mayúsculas incorrectas — casi seguro invisible en Windows, pero real; (2) la regex del patrocinador tiene un escape roto que rechaza nombres con espacio; (3) mucha duplicación de código en la página de planificar; (4) falta el `report.md` formal.

- **Principal fortaleza:** la lógica de planificación con disponibilidad por fecha (salas/combatientes/armas ocupados) está bien pensada y ejecuta correctamente; las reglas del dominio están realmente implementadas.
- **Principal área de mejora:** corregir las 11 rutas de imagen con mayúsculas (portabilidad Linux/macOS) y la regex del patrocinador; luego, factorizar la duplicación A/B y de reinicialización de estado.
