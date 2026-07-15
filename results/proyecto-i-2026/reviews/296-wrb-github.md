# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #296
- **Repositorio:** https://github.com/laxxuzxlr8/WRB-GitHub.git
- **Estudiante:** Adrian Yunior Muñoz Obregón
- **Grupo:** C-111
- **Descripción declarada:** World Robot Boxing (WRB) — aplicación web (Streamlit) para planificar combates entre robots, eligiendo fecha, arena, modo, equipos, armas y patrocinador, con validación de recursos y persistencia en JSON.

---

## Nota metodológica importante

No es una app de consola: es una **aplicación web multipágina de Streamlit** (`main.py` define la navegación con `st.navigation`/`st.Page`). No tiene `input()` ni menú de terminal, así que no se puede alimentar con `printf`. La ejecuté en tres frentes:

1. **`py_compile`** de los 7 módulos con Python 3.12 → todos compilan.
2. **Lógica de negocio headless**: extraje y ejecuté las funciones puras (`core.py` completo + `validar_robot`, `validar_patrocinador`, `validar_combate`, `recomendar_fecha` de `organizar_combate.py`) con un stub de `streamlit` y los datos reales de `data/inventario.json` y `data/combates.json`, cubriendo flujos válidos e inválidos.
3. **Arranque real de la GUI**: `streamlit run main.py --server.headless true` → el servidor levanta y responde `HTTP 200` en `/` y en `/_stcore/health`, sin `Traceback`.

Un detalle de entorno relevante: el proyecto **requiere Python 3.12**, no "3.8+" como declara el badge del README. El código usa f-strings con comillas anidadas del mismo tipo (p. ej. `f"⚡️{st.session_state.inventario["robots"][robot]}"` en `organizar_combate.py:612`), sintaxis PEP 701 que solo es válida a partir de 3.12.

## Dimensión 1 — Qué hace el programa

Aplicación web con cinco páginas organizadas en un sidebar (`main.py:53-57`):

- **Acerca de WRB** (`pages/acerca_de.py`): página informativa. Además, al abrir la web por primera vez ejecuta una limpieza automática: recorre los combates guardados y elimina de la base de datos los que ya tienen fecha pasada (`acerca_de.py:13-27`).
- **Organizar combate** (`pages/organizar_combate.py`, 1026 líneas): el núcleo. Flujo guiado por la **fecha**: hasta elegir una fecha válida, todo lo demás está bloqueado (`organizar_combate.py:642, 905`). Al fijar fecha, el sistema calcula qué **arenas, robots, armas y células de energía** siguen disponibles descontando lo consumido por combates ya programados ese mismo día (`organizar_combate.py:311-353`). Luego se eligen modo (1vs1 o 3vs3), equipos con dos armas por robot, tipo de control y patrocinador; al confirmar se valida todo y se persiste (`organizar_combate.py:1001-1020`).
- **Combates programados** (`pages/combates_programados.py`): lista los combates guardados, muestra el detalle del seleccionado y permite eliminarlos (`combates_programados.py:138-152`).
- **Robots** y **Armas** (`pages/robots.py`, `pages/armas.py`): catálogos visuales.

## Dimensión 2 — Organización del código

Fortalezas:

- **Separación limpia de la capa de datos** en `core.py`: `guardar_combates`, `cargar_combates`, `cargar_inventario` concentran toda la I/O de JSON (`core.py:12-45`). Esto es exactamente lo correcto y se reutiliza desde varias páginas.
- **Arquitectura multipágina** bien aprovechada (`main.py` como único punto de entrada, cada página en su archivo).
- **Funciones de validación separadas** de la construcción de la UI (`validar_robot`, `validar_patrocinador`, `validar_combate`, `recomendar_fecha`), cada una con una responsabilidad única.
- Comentarios de sección abundantes y consistentes que hacen el archivo largo navegable.

Debilidades:

- `organizar_combate.py` es muy grande (1026 líneas) y mezcla lógica de negocio con mucho `st.*` de layout. La parte de gestión de equipos (Equipo A líneas 540-668 y Equipo B líneas 670-799) está **duplicada casi literalmente**; podría factorizarse en una función `render_equipo(equipo)` parametrizada. Lo mismo ocurre dentro de `validar_robot`, donde las ramas `equipo == "a"` y `equipo == "b"` (`organizar_combate.py:82-118`) son idénticas salvo el nombre de la clave.
- Hay una dependencia fuerte del `st.session_state` con ~15 variables de control (`reset_fecha`, `fecha_anterior`, `len_anterior`, `modo_anterior`, `robot_seleccionado`, etc.). Funciona, pero es difícil de seguir; es el costo natural de manejar estado en Streamlit.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente se ejecutó con datos reales del repo. Distingo bugs del estudiante de fallos de entorno.

1. **`core.py` (I/O)**: `cargar_inventario()` devuelve 25 robots (Zeus = 550 C/E), 43 armas, 15 incompatibilidades, 2 arenas, 5000 células. `cargar_combates()` devuelve los 3 eventos de muestra (Lucy=2360 C/E, Elfo, Binn). `guardar_combates()` hace round-trip sin pérdida. ✔
2. **`validar_robot`**: campos vacíos → rechaza ("Debe llenar todos los campos…"); misma arma en ambos brazos → rechaza; combinación incompatible (Lanzallamas + Sensores ópticos avanzados) → rechaza con la razón exacta del inventario; robot válido → acepta; equipo lleno en 1vs1 → rechaza. Los 5 casos, correctos. ✔
3. **`validar_patrocinador`**: nombre con dígitos ("Ana123") → rechaza; 1 carácter → rechaza; nombre ya existente ("Lucy") → rechaza; "Carlos-Ruiz" → acepta. ✔
4. **`validar_combate`**: combate completo → válido; sin arena → devuelve `['Arena']`; con C/E requeridas (6000) > disponibles (5000) → devuelve `['C/E']`. ✔
5. **`recomendar_fecha`**: con ambas arenas ocupadas mañana → recomienda pasado mañana (2026-07-17); con agenda vacía → recomienda mañana (2026-07-16). El algoritmo de búsqueda de hueco funciona. ✔
6. **Arranque GUI headless**: `HTTP 200` en `/` y `/_stcore/health`, sin `Traceback`. ✔

**Único defecto funcional real encontrado (bug del estudiante, no de entorno):** la página **Robots** referencia las imágenes con ruta en minúscula `images/robots/...` (las 25 llamadas, `robots.py:28-199`), pero el directorio en disco es `images/Robots/` (R mayúscula). En Windows/macOS (sistemas de archivos insensibles a mayúsculas) esto funciona; en Linux (sensible a mayúsculas) **las 25 imágenes del catálogo de robots no cargan**. Es un fallo de portabilidad, no un crash: Streamlit muestra el placeholder de imagen rota pero la página no revienta. Notar que `armas.py` (`images/Armas/...`) y `organizar_combate.py` (`images/Gestionar Combate/...`) sí usan la capitalización correcta; el problema está aislado en `robots.py`.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores**: `cargar_*` envuelven la lectura en `try/except` con fallback razonable (`core.py:26-33, 39-45`). Bien para primer año.
- **Legibilidad**: nombres de variables en español, descriptivos y consistentes. Uso idiomático de comprensiones de lista para filtrar recursos disponibles (`organizar_combate.py:330-332, 565`).
- Puntos menores mejorables: comparaciones `== None` / `!= None` en vez de `is None`/`is not None` (`organizar_combate.py:263, 642`); el `except Exception as e` genérico con `print` (`core.py:31`) es aceptable aquí pero idealmente sería más específico. Nada de esto afecta la corrección.
- Pequeños typos que no afectan la ejecución pero conviene señalar: "Maza electromagn**á**tica" en el panel visual (`organizar_combate.py:507`) vs. "electromagnética" correcto en el inventario; "a sido cancelado" (`organizar_combate.py:1024`); "ofernsivo" en el report.

## Dimensión 5 — Datos y persistencia

- **Modelo de datos coherente y bien pensado.** `inventario.json` separa robots (dict nombre→C/E), armas (dict nombre→tipo), `combinaciones_no` (incompatibilidades con razón), arenas y un tope global de células. `combates.json` guarda cada combate indexado por patrocinador, con equipos como `{robot: [arma_izq, arma_der]}`. La estructura soporta directamente todas las validaciones sin transformaciones raras.
- **Serialización** con `json.dump(..., indent=4)` (`core.py:18`), legible. Los datos incluyen caracteres unicode escapados (`ó`, etc.), lo cual es correcto y se carga sin problemas.
- Detalle fino: el inventario usa guiones largos (en-dash `–`) en "Lanza–arpón" y "Lanza–chispas" mientras que los paneles de ayuda visual usan guion normal; como la validación real compara siempre contra los valores del inventario, esto no rompe la lógica, solo es una inconsistencia cosmética entre la tabla de ayuda y los datos.

## Dimensión 6 — Informe (`report.md`)

- **2113 palabras** (la verificación automática contó 1845; con el conteo actual supera el mínimo de 2000). El informe es **honesto y fiel al código**: describe el flujo guiado por fecha, la gestión de recursos por día, las funciones de validación y la persistencia, con fragmentos de código reales pegados que **coinciden** con lo que hay en los archivos.
- No exagera: no afirma "demuestra" ni "prueba" validaciones que no existan. Todo lo que describe está implementado y lo verifiqué ejecutando.
- Discrepancia menor: el informe incluye el fragmento de `robots.py` con la ruta `images/robots/...` (`report.md:297`) sin notar el problema de capitalización — coherente con que el estudiante desarrolló en un SO insensible a mayúsculas.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y notablemente ambicioso** para primer año. La pieza central —el algoritmo de asignación de recursos por fecha que evita colisiones de arenas, robots, armas y células entre combates del mismo día— está bien pensada y **verificada funcionando** con datos reales. La separación de la capa de datos en `core.py`, la arquitectura multipágina y el conjunto de funciones de validación (todas correctas en mis pruebas de flujos válidos e inválidos) reflejan buen criterio de diseño. El informe es honesto y fiel. El único defecto funcional real es la capitalización de rutas en `robots.py`, que rompe el catálogo de robots en Linux pero no en el entorno donde se desarrolló.

- **Principal fortaleza:** la lógica de negocio (gestión de recursos por fecha + validaciones), correcta y ejecutable, con una capa de datos limpia y bien separada.
- **Principal área de mejora:** corregir las 25 rutas `images/robots/` → `images/Robots/` en `robots.py` para que sea portable; y, de cara al futuro, reducir la duplicación Equipo A/Equipo B factorizándola en una sola función parametrizada.
