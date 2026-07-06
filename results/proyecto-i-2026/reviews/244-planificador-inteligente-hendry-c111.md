# Revisión detallada — Proyecto I (1er año)

- **Alumno:** Hendry Quintana Ruiz
- **Grupo:** C111
- **Issue:** #244
- **Repo:** https://github.com/Takasu25c/Planificador-Inteligente-Hendry-C111
- **Descripción del issue:** "taller de reparación de motherboards, diseñado para que el usuario tenga muy pocas probabilidades de equivocarse".

> ⚠️ Nota importante para el profesor: **no es una app de consola.** El proyecto es una aplicación web **Streamlit** (`streamlit run streamlit_app.py`), no un menú de `input()`. La rúbrica asume consola; aquí la "ejecución dinámica" se hizo con el harness oficial `streamlit.testing.v1.AppTest`, que ejecuta el script real y captura excepciones y widgets programáticamente (equivalente a recorrer la interfaz). Es un nivel de ambición notablemente por encima del promedio de 1er año.

---

## 1. Qué hace el programa

Es un **planificador de turnos para un taller de reparación de motherboards** ("TERAX"). El punto de entrada es `streamlit_app.py:250-262`, que arma la interfaz: título, selector de tipo de reparación (`streamlit_app.py:252`, alimentado por `eventos.py:1` `lista_de_eventos`), campo de nombre, selector de fecha/hora (`st.datetime_input`, `streamlit_app.py:254`), y luego invoca `iniciar_evento`, `registrar_evento` y `busqueda_automatica`.

El dominio está bien modelado. Cada tipo de reparación tiene una **duración fija** (`streamlit_app.py:32-46`, un `dict` de `timedelta`) y un **conjunto de recursos compatibles** (`Recursos_y_Personal.py:3-15`). El flujo real es: (1) el usuario elige un servicio; (2) `iniciar_evento` (`streamlit_app.py:183-249`) solo deja agregar recursos compatibles con ese servicio; (3) `verificar_restricciones` (`streamlit_app.py:68-84`) impide herramientas mutuamente excluyentes (p.ej. Osciloscopio vs Multímetro); (4) `verificar_inclusiones` (`streamlit_app.py:85-109`) exige combinaciones obligatorias (p.ej. Osciloscopio ⇒ Monitor con BoardView); (5) al registrar, `registrar_evento` (`streamlit_app.py:121-161`) comprueba que ningún recurso esté **ocupado en el horario solapado** por otro evento ya agendado; (6) `busqueda_automatica` (`streamlit_app.py:162-182`) busca un "hueco" libre en la agenda. El estado se persiste en `salva.json` vía `planner.py`.

La idea central del issue ("que el usuario se equivoque poco") **se cumple de verdad**: hay tres capas de validación (compatibilidad servicio↔recurso, exclusiones, inclusiones) más detección de conflictos temporales.

## 2. Organización del código

**Buena para 1er año.** El código está **dividido en 4 módulos** con responsabilidades claras, en vez de un `main.py` gigante:

- `eventos.py` — catálogo de servicios.
- `Recursos_y_Personal.py` — catálogo de recursos y qué recursos aplican a cada servicio.
- `planner.py` — persistencia (`guardar_estado`/`cargar_estado`, `planner.py:5-25`).
- `streamlit_app.py` — UI y lógica.

Uso correcto de **funciones** para no repetir (`agregar_recurso`, `registro`, `verificar_restricciones`, etc.). Nombres en general descriptivos (`verificar_inclusiones`, `duracion_eventos`, `recursos_en_uso`).

Debilidades de organización:
- **`iniciar_evento` (`streamlit_app.py:183-249`) es un `if/elif` de 13 ramas casi idénticas** que solo difieren en el catálogo de recursos consultado. Todo eso se reduciría a un `dict` que mapee `evento -> catálogo` y un par de líneas. Es el punto donde más se repite código.
- La **indentación mezcla 1, 2 y 3 espacios** de forma inconsistente (compárese `streamlit_app.py:47-67` con `:110-120`). Python lo tolera mientras sea consistente por bloque, pero dificulta la lectura.
- Nombres puntuales flojos: `eventico`, `nevento`, variable `echo` inexistente; typo `st.session_state.recursos_en_uso` vs. la key `"Resource Index"`.

## 3. Corrección funcional (basada en ejecución real)

**Ejecuté el programa** con `streamlit.testing.v1.AppTest`, cargando el directorio del proyecto en `sys.path` (equivale a `streamlit run` desde la carpeta). Streamlit instalado en venv aislado con `uv` (v1.59.0, Python 3.12).

**Arranca sin errores.** Carga inicial: 0 excepciones, título "TERAX" renderizado, 2 selectbox, 5 botones, 2 number_input presentes.

Recorrido de opciones (todo con salva.json respaldado y restaurado):

| Flujo probado | Resultado observado |
|---|---|
| Seleccionar "Defectacion" → recurso "Osciloscopio" → **Agregar Recurso** | ✅ `success: "Recurso Agregado"` |
| Agregar Osciloscopio + Monitor con BoardView → **Registrar Evento** (agenda vacía) | ✅ `success: "Registro añadido"`, escribió correctamente a `salva.json` |
| Registrar Osciloscopio **sin** Monitor con BoardView | ✅ Bloqueado: `error: "No se agrego Monitor con Boardview"` (inclusión funciona) |
| **Registrar evento con Osciloscopio en horario solapado** a otro ya agendado | ✅ Bloqueado: `error: "El/La Osciloscopio no esta disponible en ese horario"` (detección de conflicto funciona) |
| **Busqueda de Huecos** con 1 evento en agenda + recursos seleccionados | ✅ `success: "Registro añadido"` — agendó el nuevo evento a +15 min del fin del anterior (12:35 → 12:50) |
| **Eliminar recurso** con lista vacía | ✅ `error: "No hay elementos que eliminar"` (manejado) |
| **Eliminar evento** con lista vacía | ✅ `error: "No hay eventos en la lista"` (manejado) |

**Bug funcional real encontrado (silencioso):** registrar **"Mantenimiento: Bajo"** con solo "Pasta termica". El recurso se agrega (`success: "Recurso Agregado"`), pero al pulsar **Registrar Evento** → `success: []`, `errors: []`, `exc: 0`: **no pasa nada, sin ningún mensaje al usuario.** Causa: `verificar_inclusiones` (`streamlit_app.py:85-109`) es una cadena `if/elif` que **no cubre todos los casos y devuelve `None`** cuando el conjunto de recursos no cae en ninguna rama (aquí, "Pasta termica" sola). Luego `registrar_evento` hace `if verificar_inclusiones(...) == True:` (`streamlit_app.py:137`), que con `None` es falso, así que **omite el registro sin avisar**. Para un servicio de mantenimiento válido, el usuario queda sin feedback ni evento registrado. Se arregla haciendo que `verificar_inclusiones` termine con un `return True` por defecto (o cambiando la condición a `if verificar_inclusiones(...):` y devolviendo siempre bool).

**Otras fragilidades detectadas leyendo + ejecutando:**
- `registrar_evento:139` — `if st.session_state.eventos_en_curso == False:` **nunca es True** (una lista vacía es `[]`, no `False`; la comparación con `False` no da True para `[]`). La rama "agenda vacía" real llega igual al `registro` final de `:160`, así que el efecto neto funciona, pero la intención está mal expresada.
- Los number_input usan `max_value=len(...)` (`streamlit_app.py:52`, `:126`), lo que permite un índice igual a la longitud (fuera de rango). En la práctica el `del ...[numero]` no llegó a romperse en mis pruebas porque las cotas del widget acotan, pero es un off-by-one latente (debería ser `len-1` o guardar contra lista vacía).
- `busqueda_automatica` (`streamlit_app.py:162-182`) resta `timedelta` directamente sobre `horarios` asumiendo que son `datetime`; funcionó porque `cargar_estado` los convierte, pero es sensible al orden de conversión.

Dentro de sus caminos previstos, **el programa hace lo que dice el issue** y las tres validaciones centrales funcionan de verdad. Los fallos son de caminos secundarios y de feedback, no del núcleo.

## 4. Buenas prácticas de Python (nivel principiante)

- ✅ Uso idiomático de `datetime`/`timedelta`, `dict`, `f-strings` (`streamlit_app.py:154`), list comprehensions implícitas y `.copy()` para no mutar por referencia (`:116`, `:138`).
- ✅ Manejo de errores donde importa: `try/except json.JSONDecodeError` en `cargar_estado` (`planner.py:16-23`), y chequeo de tamaño de archivo (`planner.py:14`).
- ✅ Serialización correcta de `datetime` ↔ ISO string para poder guardarlo en JSON (`planner.py:6-10`, `:19-20`).
- ⚠️ **Indentación inconsistente** (1/2/3 espacios) — el punto más flojo de estilo.
- ⚠️ **Duplicación evidente** en `iniciar_evento` (13 ramas) — reducible a un `dict`.
- ⚠️ `verificar_inclusiones` sin `return` por defecto (raíz del bug silencioso).
- No penalizado: ausencia de tests/type hints (correcto para 1er año).

## 5. Datos y persistencia

**Correcta y bien pensada.** `salva.json` guarda una lista de dicts con nombre, tipo, horarios (ISO) y recursos (`salva.json:1`). `guardar_estado`/`cargar_estado` (`planner.py`) hacen el round-trip `datetime ↔ str` de forma robusta, con manejo de archivo vacío y JSON corrupto. Verifiqué en ejecución que registrar un evento **escribe** correctamente a `salva.json` y que eliminar **actualiza** el archivo. La estructura de datos (lista de dicts) es razonable para el volumen del dominio. El único detalle: `Nombre` guarda el placeholder `"Nombre y apellidos"` si el usuario no cambia el campo de texto — no hay validación de nombre no-vacío.

## 6. Informe (`report.md` / `README.md`)

El informe (`report.md`) **describe fielmente el dominio**: lista los 13 servicios con sus duraciones, los 18 recursos, y — lo más valioso — **documenta explícitamente las restricciones de exclusión (`!=`) e inclusión (`=`) por servicio** (`report.md:38-49`). Esas restricciones **coinciden con lo que el código implementa** en `verificar_restricciones`/`verificar_inclusiones`. Es un informe honesto: **no sobreestima** — no afirma features inexistentes.

Omisiones menores:
- No menciona la funcionalidad de **detección de conflictos temporales** ni la **búsqueda automática de huecos**, que son de lo más sofisticado del código. El informe se queda corto respecto a lo que el programa realmente hace (lo contrario de sobreestimar).
- `README.md` dice "ejecutar streamlit run en la consola" pero **no da el comando completo** (`streamlit run streamlit_app.py`) — un evaluador sin experiencia podría no saber qué archivo correr.
- El `report.md` no describe el flujo de uso paso a paso ni las dificultades encontradas (la rúbrica lo pide).

---

## Síntesis para el profesor

Trabajo **claramente por encima del promedio de 1er año**. El estudiante eligió una tecnología (Streamlit) que no se enseña en el curso e implementó un modelo de dominio genuino con **tres capas de validación reales que funcionan al ejecutar**: compatibilidad servicio↔recurso, exclusiones/inclusiones, y **detección de conflictos temporales por recurso** — esto último es lo más difícil y **quedó funcional** (verificado en ejecución). La persistencia JSON es correcta y robusta.

Debilidades: (1) un **bug silencioso** en el registro de mantenimientos por un `return` faltante en `verificar_inclusiones`; (2) **duplicación grande** en `iniciar_evento`; (3) **indentación inconsistente**; (4) el informe se queda corto (no sobreestima, pero omite features y el flujo de uso).

Ninguna excepción no controlada apareció en el recorrido de la interfaz. Es un proyecto sólido y ambicioso; las correcciones son de pulido, no de rediseño.
