# Reporte de Evaluación — Proyecto I

- **Entrega:** Issue #251
- **Repositorio:** https://github.com/JulioEIppo/SchoolManagerProject1
- **Estudiante:** Julio E Gonzalez Alvarez
- **Grupo:** C-111
- **Descripción declarada:** gestión de eventos en una escuela: planifica un evento para un día y horario específico, con recursos específicos según una serie de reglas.

---

## Resultado de la ejecución (obligatorio)

**Nota de contexto:** la rúbrica asume una app de consola (`main.py` con menú `input()`). Este proyecto **no** es de consola: `main.py` es una aplicación web **Streamlit** (`import streamlit as st`, `st.tabs`, `st.session_state`, etc.). El informe lo declara honestamente, así que no hay engaño; simplemente adapté la ejecución dinámica a este stack.

Qué corrí y qué observé:

1. **Compilación** de los 7 `.py` con `python3 -m py_compile` → todos OK (Python 3.14; también probé con venv 3.12). No hay errores de sintaxis.
2. **Entorno aislado con `uv`**: `uv venv --python 3.12` + `uv pip install -r requirements.txt` (solo `streamlit==1.51.0` y sus deps). Instalación limpia.
3. **Arranque del servidor Streamlit**: `streamlit run main.py --server.headless true` → arranca correctamente, sirve `HTTP 200` en la raíz y `HTTP 200` en `/_stcore/health`. La app **levanta sin errores**.
4. **Ejercicio directo de la lógica de negocio** (importando `EventManager` y recorriéndola con datos, ya que el "menú" de una app Streamlit no se maneja por stdin). Probé, todo con resultado **correcto**:
   - Crear evento válido (aula + profesor + proyector) → sin errores, evento añadido.
   - Nombre duplicado → `['Event name already used']`.
   - Exclusión (Gym + profesor normal) → detecta co-requisito faltante **y** exclusión.
   - Co-requisito satisfecho (baloncesto + Gym + Gym Teacher Alberto) → válido.
   - Conflicto horario con recurso compartido → detectado.
   - Evento en el pasado / fin de semana → rechazado.
   - `find_available_slot` → devuelve un hueco futuro correcto (no propone tiempos ya pasados).
   - `save()` / `load()` roundtrip → 2 eventos guardados y recargados intactos.
   - Evento recurrente (3 repeticiones) → genera `Recur`, `Recur (day 2)`, `Recur (day 3)`.
   - Nombre vacío → rechazado; lugar que es staff → rechazado con mensaje claro.
5. **Carga de los `.json` versionados** en `src/saved_states/` y de archivos malformados:
   - `awerawerwasete.json` (56 eventos), `state_20260616_023319.json`, `stst.json` → cargan OK.
   - JSON malformado y JSON con claves faltantes → rechazados limpiamente (`load` devuelve `False`). Bien.
   - **`state.json` → CRASH con `Traceback`** (ver dimensión 3, hallazgo principal).

---

## 1. Qué hace el programa

`main.py` es una **aplicación web Streamlit** de gestión de eventos escolares. El punto de entrada real se ejecuta con `streamlit run main.py` (documentado en `report.md:85`), no con `python main.py`. La UI se organiza en cinco pestañas (`main.py:29`): General (métricas y uso de recursos), Create Event (formulario con sugerencia de horario, evento especial y recurrente), List (listado con filtros por fecha/lugar, ordenación y borrado), Config (guardar/cargar estado JSON, restaurar configuración por defecto) y Rules (gestión de co-requisitos y exclusiones).

La lógica de dominio vive separada de la UI en `src/core/`: `EventManager` orquesta eventos, recursos y reglas; `Event`/`Resource` (en `Classes.py`) son las entidades; `default_config.py` provee ~38 recursos iniciales (aulas, staff, inventario) con sus reglas; `FileManager` serializa/deserializa el estado a JSON. El flujo principal es: el usuario rellena el formulario → `EventManager.add_event` (`EventManager.py:128`) construye el `Event` (validaciones intrínsecas) y luego `validate_event` (`EventManager.py:95`) aplica reglas contextuales (existencia de recursos, conflictos horarios, co-requisitos, exclusiones, nombre único).

## 2. Organización del código

**Muy por encima del nivel esperado en 1er año.** El proyecto está modularizado con criterio real:

- Separación limpia UI ↔ lógica: `main.py` (presentación) vs. `src/core/*` (dominio). Esto es lo que el informe llama arquitectura probable-independiente, y es cierto: pude ejercitar `EventManager` sin tocar Streamlit.
- `Classes.py:15` usa `@dataclass(frozen=True)` para `Resource`, con comentario justificando la inmutabilidad ("for using in sets") — decisión correcta, porque los recursos se usan como claves de dict y elementos de set en todo `EventManager`.
- `ResourceType` como `Enum` (`Classes.py:7`).
- Excepciones propias `EventError`/`ValidationError` (`Exceptions.py`) que transportan una **lista** de errores, permitiendo devolver varios mensajes a la UI de una vez (`EventManager.py:184`, `:190`).
- Nombres claros y consistentes (`available_resources`, `co_requirements`, `find_available_slot`, `validate_exclusions`). Métodos cortos y con una responsabilidad.
- `FileManager` con métodos estáticos bien separados de serialización/deserialización por tipo.

Observaciones menores: los nombres de archivo usan `PascalCase` (`Classes.py`, `EventManager.py`), cuando la convención Python es `snake_case` para módulos; es cosmético. Hay bloques grandes de código comentado muerto (`main.py:438-463`, `main.py:622-643`) que conviene borrar.

## 3. Corrección funcional (basada en ejecución real)

La lógica de dominio es **sólida y correcta** en todos los casos que ejercité (ver "Resultado de la ejecución"). Las reglas del enunciado —planificar un evento con día/horario y recursos, sujeto a reglas— están todas implementadas y funcionando: horario laboral 9-18h para eventos normales (`Classes.py:56`), no fines de semana (`Classes.py:58`), co-requisitos (`EventManager.py:56`), exclusiones bidireccionales (`EventManager.py:43`), conflictos por recurso compartido (`EventManager.py:113`), nombre único, no-eventos-en-el-pasado. El motor coincide con lo que el informe promete.

**Hallazgo principal (bug real, con `Traceback`):** al cargar `src/saved_states/state.json` el programa **crashea** en vez de manejar el error. Ese archivo usa la clave antigua `"personnel"` en lugar de `"staff"` (un evento con esquema legacy). El flujo es:

1. `deserialize_events` (`FileManager.py:162`) hace `event_dict["staff"]` → `KeyError: 'staff'`.
2. Ese `KeyError` cae en el `except` de `FileManager.py:187`, que **está pensado para reportar el error limpiamente**.
3. Pero el propio mensaje de error tiene una **f-string mal formada** (`FileManager.py:189`):
   ```python
   f"Error loading event {event_dict.get("name", "unknown") : {e}}"
   ```
   El `: {e}` dentro de las llaves se interpreta como *format-specifier*, lo que lanza `ValueError: Invalid format specifier`. Resultado: el manejador de errores **se rompe a sí mismo** y el `Traceback` escapa hasta `load_state`.

Es decir: la mala noticia es que un archivo de estado con esquema viejo revienta la carga; la buena es que el bug está en un **mensaje de error** (una línea), no en la lógica. Un archivo con esquema actual carga perfecto (verificado con 3 de los 5 `.json`). El mismo patrón defectuoso está también en `FileManager.py:158` (`f"Place not found for {event_dict["name"]}"` — este sí es válido en 3.12+, pero mezcla comillas dobles anidadas de forma frágil).

**Sugerencia accionable:** cambiar `FileManager.py:189` a comillas simples externas y sin `:` conflictivo, p.ej.
`errors.append(f"Error loading event {event_dict.get('name', 'unknown')}: {e}")`.
Con eso, `state.json` cargaría reportando "Person ... not found" o similar, sin crash.

El resto de casos de error (JSON malformado, claves faltantes) se manejan bien: `load` devuelve `False` sin excepción.

## 4. Buenas prácticas de Python (nivel principiante)

Muy buenas para el nivel:

- Legibilidad e indentación consistentes en todos los módulos.
- `try/except` donde toca, con excepciones tipadas (`EventManager.py:182-191`, `FileManager.py:99-107`).
- f-strings idiomáticas en mensajes y formateo de fechas.
- Sin variables globales de estado mutable en la lógica (las "globales" de `default_config.py` son constantes de recursos, uso legítimo).
- Comprensiones de listas/sets claras (`main.py:45`, `EventManager.py:100`).

A mejorar (menor): quedan comentarios en spanglish mezclados dentro del código (`EventManager.py:77` "Para 3 o más", junto a variables `primeros`/`ultimo` en español mientras el resto es inglés) — mantener un idioma. Y el `except Exception` genérico de `FileManager.py:187` esconde el tipo real del fallo; ya vimos que ahí precisamente estaba el bug.

## 5. Datos y persistencia

Bien resuelto. El estado completo (recursos, co-requisitos, exclusiones, eventos) se serializa a JSON con un formato de cadena `"tipo:nombre"` por recurso (`Classes.py:21`, `FileManager.py:110`). La deserialización reconstruye un `resource_map` y valida referencias (`FileManager.py:144`), acumulando errores y descartando **toda** la carga si hay alguno (`FileManager.py:85-90`) — decisión correcta para no dejar el manager en estado inconsistente. Las estructuras elegidas son razonadas: `set[frozenset[Resource]]` para co-requisitos (permite "válido si cumple al menos un conjunto") y `dict[Resource, set[Resource]]` bidireccional para exclusiones. El roundtrip save→load lo verifiqué y preserva los eventos. `get_unique_filename` (`FileManager.py:250`) evita sobrescrituras al subir archivos externos. El único pero es el crash del §3 ante un `.json` con esquema legacy.

## 6. Informe (`report.md`)

**Excelente y honesto.** Describe con precisión lo que el código hace: identifica correctamente que es una app **Streamlit** (no de consola), explica la arquitectura módulo por módulo (`report.md:24-35`), documenta decisiones de diseño reales (inmutabilidad, `frozenset`, validación en dos etapas, recurrencia), y da instrucciones de ejecución correctas (`streamlit run main.py`). La sección de dificultades (`report.md:131-178`) es concreta y coincide con el código.

No detecté sobreestimación: cada feature que el informe afirma (sugerencia de horario, recurrencia con salto de fines de semana, exclusiones bidireccionales, persistencia externa) existe y funciona. Única discrepancia relevante: el informe (`report.md:164`) dice que si falta un recurso al deserializar "se omite el evento y se imprime una advertencia... evitando que la carga falle por completo" — pero en la práctica `load_state` descarta **toda** la carga si hay cualquier error (`FileManager.py:85`), que es lo correcto y lo que el propio §7.6 (`report.md:178`) describe bien; hay una pequeña contradicción interna entre §7.3 y §7.6 del informe. Y, por supuesto, el informe no menciona el bug de la f-string en el manejador de errores (que provoca crash en `state.json`).

---

## Síntesis para el profesor

Trabajo **claramente por encima del promedio de 1er año**. Modularización real, separación UI/lógica, uso correcto de dataclasses inmutables, enums, excepciones propias con lista de errores, y un modelo de reglas (co-requisitos como `set[frozenset]`, exclusiones bidireccionales) bien pensado. La app Streamlit arranca sin problemas y toda la lógica de dominio que ejercité es correcta. El informe es honesto, detallado y fiel al código.

El único defecto de corrección real es un bug de una línea: una f-string mal formada en el **manejador de errores** de `deserialize_events` (`FileManager.py:189`) que, ante un `.json` de esquema legacy (`state.json`, clave `personnel` en vez de `staff`), convierte lo que debía ser un mensaje de error en un `Traceback`. Trivial de arreglar y no afecta el camino feliz. Fortaleza principal: diseño y organización. Área de mejora principal: robustez del manejo de errores en la carga (y limpiar código comentado muerto).
