# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #283
- **Repositorio:** https://github.com/milagro07/Mila-StudioPhoto
- **Estudiante:** Monica Curbelo Orduñez
- **Grupo:** C 121
- **Descripción declarada:** Aplicación en Python con Streamlit para la gestión de citas en un estudio de fotografía para quinceañeras.

---

## Nota metodológica importante

Este proyecto **no es una aplicación de consola**: es una app web construida con **Streamlit** (`home.py` como punto de entrada, navegación multipágina vía `st.Page` / `st.navigation`). No usa `input()`. Por tanto no se probó con `printf`, sino que la evalué de tres formas:

1. **Lógica de negocio pura** (`core.py`) ejecutada directamente con los datos reales del repo (`data/proyectos.json`, `data/inventario.json`).
2. **Arranque headless real**: `streamlit run home.py --server.headless true`. La app levantó correctamente (Uvicorn en el puerto, `HTTP 200`, `/_stcore/health → ok`).
3. **Ejecución dinámica de cada página** mediante el harness oficial `streamlit.testing.v1.AppTest`, simulando clics, selección de fechas y escritura en campos — flujos válidos e inválidos.

Entorno: `uv venv --python 3.12`, Streamlit 1.59.2, pandas 3.0.3. La app usa APIs muy recientes de Streamlit (`text_alignment`, `st.container(horizontal_alignment=...)`, `width="stretch"`); verifiqué que todas existen en 1.59.2 (con versiones más viejas la app no correría). También usa f-strings con comillas anidadas del mismo tipo (`planificar_evento.py:289`), válido solo en Python ≥ 3.12.

## Dimensión 1 — Qué hace el programa

Es un gestor de reservas para un estudio fotográfico, con cuatro páginas:

- **Sobre nosotros** (`page/sobre_nosotros.py`): presentación del estudio con texto e imagen.
- **Planificar sesión** (`page/planificar_evento.py`): el corazón de la app. Formulario progresivo que pide fecha (solo futura, `date_input` con `min_value = hoy + 1 día`, línea 70), nombre y apellido (validados), espacio, iluminación (autoasignada según el espacio), cámara, fotógrafo (autoasignado a "Mila"), auxiliar y cantidad de vestuarios. Al confirmar (`planificar_evento.py:276-292`) valida que no falten campos y persiste la sesión.
- **Sesiones programadas** (`page/sesiones_programadas.py`): tabla (`pd.DataFrame`) con las sesiones y opción de cancelar una (`selectbox` + botón "Borrar", líneas 25-42).
- **Otros trabajos** (`page/otros_trabajos.py`): galería de imágenes.

Verificado en ejecución: la app aplica correctamente las reglas del dominio. Espacio "Interior" → asigna iluminación "Focos" (`planificar_evento.py:200-202`); "Exterior" → "Natural" (203-205); seleccionar cámara autoasigna a Mila como fotógrafa (221-223).

## Dimensión 2 — Organización del código

**Fortalezas:**
- Separación limpia entre **lógica de negocio** (`core.py`: `load_projects`, `load_inventory`, `save_events`) y **presentación** (las páginas). Esto es notable para primer año: `core.py` no importa Streamlit y es testeable de forma aislada — de hecho pude ejecutarlo directamente sin levantar la GUI.
- Navegación multipágina bien estructurada (`home.py:4-31`), con páginas en su propia carpeta `page/`.
- Los datos (inventario y sesiones) viven en `data/*.json`, fuera del código.

**Debilidades:**
- `planificar_evento.py` tiene 291 líneas con mucha repetición: los diccionarios de `st.session_state.inventario` y `st.session_state.sesión` se escriben literalmente cuatro veces (líneas 16-34, 27-45, 87-94). Podrían factorizarse en una función `estructura_vacia()`.
- Hay archivos vacíos sin propósito: `__main__.py`, `page/__main__.py`, `views/__init__.py`, y `data/__main.__py` (nótese el nombre mal formado, con puntos fuera de lugar). Conviene eliminarlos.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Numerado por lo que **corrí y observé**:

1. **Arranque headless: OK.** `streamlit run home.py` levantó sin traza; `curl` al root devolvió `200` y el health-check `ok`.
2. **`home.py` (entrypoint con navegación) vía AppTest: sin excepciones.** Título renderizado.
3. **`core.load_projects()`: OK.** Devolvió las 2 sesiones reales (`2026-02-20` Yalena, `2026-02-21` Mariana).
4. **`core.load_inventory()`: OK.** Devolvió cámaras, espacios, auxiliares y `Vestuario: 10`.
5. **`core.save_events()` round-trip: OK.** Agregué una sesión de prueba, guardó y releyó correctamente; restauré el JSON original.
6. **Página "Sesiones programadas": OK.** Renderizó el dataframe con las 2 sesiones, el selectbox y el botón. **Flujo de borrado probado con clic real**: seleccioné "2026-02-20 / Yalena", pulsé "Borrar" → eliminó la entrada del JSON y mostró "Ha eliminado la sesión del día 2026-02-20". Restauré el JSON.
7. **Página "Planificar sesión": OK en el flujo feliz.** Cargó los widgets; los campos de nombre aparecen deshabilitados hasta elegir fecha (buen detalle de UX). Elegí una fecha futura (+30 días) → "Fecha Válida".
8. **BUG confirmado en ejecución — nombres compuestos rechazados.** Al escribir el nombre **"Maria Jose"** (con espacio), la app respondió con error: *"Nombre debe contener solo letras, espacios o guiones."* — irónico, porque el propio mensaje promete que los espacios son válidos. Un nombre de una sola palabra ("Ana") sí se acepta. **Causa** (`planificar_evento.py:9`): el regex es `r'^[A-Za-z\\s-]+$'` con **doble barra invertida**; dentro de una cadena `r'...'`, `\\s` significa "una barra literal o la letra s", **no** un espacio en blanco. El fix es `r'^[A-Za-z\s-]+$'` (una sola barra). Reproduje esto tanto aislando `validate_name` como con clics reales en la página.
9. **Limitación menor derivada del mismo regex:** nombres con tildes ("José") también se rechazan porque `[A-Za-z]` no cubre acentos. Aceptable para 1er año, pero relevante en un contexto cubano/español.
10. **BUG latente — `load_projects()` puede devolver `None`.** En `core.py:9-10`, si el archivo no existe hace `return` (devuelve `None`); y `data.get("sesiones")` (línea 14) devuelve `None` si el JSON no tiene esa clave. Luego `len(st.session_state.eventos_programados)` (`planificar_evento.py:76`, `sesiones_programadas.py:11`) revienta con `TypeError: object of type 'NoneType' has no len()`. Verifiqué que `len(None)` lanza esa excepción. No se dispara en el camino feliz porque el repo trae un JSON válido, pero un checkout sin datos válidos rompería la app. Debería devolverse `{}` en vez de `None`.

Todos los módulos pasan `py_compile` sin errores.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores:** `core.py` usa `try/except` al cargar JSON (bien), pero el `except` solo imprime y devuelve `None` implícito — de ahí el bug latente (#10). Mejor devolver un valor por defecto seguro (`{}`).
- **Legibilidad:** buenos nombres de variables y funciones en español consistente. El código de las páginas es lineal y fácil de seguir.
- **Repetición:** los diccionarios "vacíos" duplicados (ver Dimensión 2) son el punto más mejorable.
- **Detalle idiomático:** el bucle `while True` con `break` para buscar la próxima fecha libre (`planificar_evento.py:106-116`) funciona (lo verifiqué: devolvió `2026-02-22` con los dos días ocupados), pero la lógica de la bandera `disponible` es algo enrevesada; un `while dia in fechas_ocupadas: dia += 1 día` sería más claro.
- Archivos vacíos con nombres raros (`data/__main.__py`) deberían borrarse.

## Dimensión 5 — Datos y persistencia

- Modelo simple y correcto: dos JSON, uno para el **inventario** (recursos del estudio, semilla de solo lectura) y otro para las **sesiones** (diccionario indexado por fecha `YYYY-MM-DD`).
- Usar la fecha como clave es una decisión acertada: garantiza que no haya dos sesiones el mismo día (la app además valida colisiones, `planificar_evento.py:82-95`, verificado).
- Serialización con `json.dump(..., indent=4)` (`core.py:35`), legible. El round-trip guardar→cargar funciona (verificado).
- pandas se usa solo para renderizar la tabla (`sesiones_programadas.py:12`), uso mínimo pero apropiado.

## Dimensión 6 — Informe (`report.md`)

El informe es claro, bien estructurado y **coincide en lo esencial con el código**. Describe las cuatro páginas, las reglas del dominio (co-requisito espacio↔iluminación, dependencia cámara↔fotógrafo, exclusión mutua del auxiliar, campos obligatorios excepto vestuario) — todas ellas **verificadas en ejecución**.

Discrepancias menores:
- El informe describe el proyecto como "sesiones de fotos de 15" (quinceañeras), pero el código es genérico (no hay nada específico de 15 años). Coherente con la descripción del issue, solo es un matiz de encuadre.
- El informe **no menciona** el bug de validación de nombres con espacios; presenta la validación como funcional. No es exageración deliberada — la estudiante seguramente probó solo nombres de una palabra.
- Prerrequisitos dicen "Python 3.7 o superior", pero el código usa f-strings con comillas anidadas (`planificar_evento.py:289`) que **requieren Python ≥ 3.12**. Con 3.7 no compilaría.

El informe **no abusa** de "demuestra"/"prueba"; es honesto en su tono.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y ambicioso** para primer año. La estudiante eligió Streamlit (framework no trivial, fuera de lo enseñado en consola), lo dominó lo suficiente para construir una app multipágina que **realmente levanta y funciona**, con una separación limpia entre lógica de negocio y presentación que muchos estudiantes más avanzados no logran. Las reglas del dominio están bien pensadas y correctamente implementadas (lo confirmé ejecutando cada rama). El flujo de crear y borrar sesiones funciona de punta a punta con persistencia real en JSON. El único defecto funcional relevante es el regex de validación de nombres, que por una barra invertida de más rechaza nombres compuestos — un error sutil y muy común, fácil de corregir.

- **Principal fortaleza:** arquitectura limpia (lógica de negocio en `core.py` desacoplada de la GUI, testeable en aislamiento) sobre un framework moderno que la estudiante manejó con soltura; la app funciona de verdad end-to-end.
- **Principal área de mejora:** el bug de validación de nombres (`planificar_evento.py:9`, `\\s` → `\s`), que en la práctica impide reservar con un nombre de dos palabras; y proteger `load_projects()` para que devuelva `{}` en vez de `None` (evita un `TypeError` latente).

**Veredicto: sólido.**
