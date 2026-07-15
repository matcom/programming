# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #291
- **Repositorio:** https://github.com/johanpauleg/The_Panama_Canal
- **Estudiante:** Johan Paul Echevarría González
- **Grupo:** C122
- **Descripción declarada:** Gestor de eventos asociado al funcionamiento del Canal de Panamá. Usa Streamlit para la interfaz; `app.py` (interfaz e interacción) y `scheduler.py` (lógica de validación); archivos `.json` para eventos, recursos y restricciones.

---

## Nota metodológica importante

Esto **no es una app de consola**: es una aplicación web con Streamlit (`app.py:1`, `import streamlit as st`; `app.py:454-460`, navegación multipágina con `st.Page`/`st.navigation`/`pg.run`). Por tanto no se puede alimentar con `printf`. Adapté la evaluación así:

1. **Lógica de negocio en aislamiento** — `scheduler.py` está limpiamente desacoplado de la GUI (no importa `streamlit`), así que instancié la clase `Event` con datos reales del repo y ejecuté todos sus métodos de validación directamente, con flujos válidos e inválidos.
2. **Persistencia** — repliqué exactamente `save_event`/`delete_event` (`app.py:13-49`) y verifiqué el ciclo guardar/expandir-recurrencias/borrar contra `events.json`.
3. **Arranque headless de la GUI** — lancé `streamlit run app.py --server.headless true` en un puerto de prueba y confirmé arranque limpio (HTTP 200, `/_stcore/health` = `ok`, sin `Traceback`).
4. `py_compile` de ambos módulos bajo Python 3.12.

**Requisito de versión:** el código usa comillas dobles anidadas dentro de f-strings de comillas dobles (p. ej. `scheduler.py:95`, `f"...{restrictions_data["lock maintenance"]...}"`; también `app.py:407`, `app.py:450`). Esto es **PEP 701, exclusivo de Python 3.12+**. Bajo 3.10/3.11 el proyecto ni siquiera compila (`SyntaxError`), pese a que `requirements.txt` declara `Python>=3.10`. Con 3.12 compila y corre sin problemas.

## Dimensión 1 — Qué hace el programa

Planificador de eventos del Canal de Panamá con restricciones de dominio realistas. El canal se modela como 3 carriles (Pacífico/Culebra/Atlántico), cada uno con 3 esclusas: `resources.json` lista `A1..A3, C1..C3, P1..P3` más recursos cuantitativos (2 pilotos junior, 2 senior, 6 remolcadores, 3 equipos de mantenimiento).

Flujo (todo en `app.py`):
- **Home** (`home`, `app.py:51-72`): descripción del sistema.
- **Add events** (`add`, `app.py:74-392`): el usuario elige tipo (único/recurrente), fechas/horas, subtipo (Tránsito/Mantenimiento), tamaño de embarcación si es tránsito, y recursos + esclusas. Al pulsar "Schedule event" se construye un dict y se instancia `Event` (`app.py:335`), corriendo las validaciones.
- **Scheduled events** (`schedule`, `app.py:394-452`): lista los eventos guardados con botón de borrado por evento.

La pieza más ambiciosa es el **sugeridor de próximo horario disponible** (`scheduler.py:223-243`): si el evento choca con recursos ocupados, avanza hora a hora hasta 60 días buscando una franja libre y la ofrece al usuario para reprogramar (`app.py:307-331`).

## Dimensión 2 — Organización del código

**Fortalezas notables para 1er año:**
- **Separación GUI / dominio muy limpia.** `scheduler.py` no importa `streamlit`; toda la lógica de reglas vive en la clase `Event`. Esto es exactamente lo que permitió evaluar la lógica de negocio en aislamiento y es una decisión de diseño madura.
- **Validaciones descompuestas por responsabilidad** (`scheduler.py`): `validate_datetime_logic` (30), `validate_duration_restrictions` (46), `validate_resources_logic` (75), `validate_resources_restrictions` (91), `validate_resources_availability` (132), `validate_recurrence_limits` (182). Componen bien: `static_validations` (129) encadena las estáticas; `static_validations_recurrences` (190) añade la de recurrencia.
- **Datos externalizados en JSON** (`resources.json`, `restrictions.json`): las reglas no están hard-codeadas, se leen de configuración. Muy buen instinto.
- **Acumulación de mensajes de error** en `self.error_messages` que la GUI muestra en bloque; buena UX.

**Debilidades:**
- **Duplicación en `validate_duration_restrictions`** (`scheduler.py:54-72`): las tres ramas Small/Medium/Large son idénticas salvo la clave. Un solo lookup `restrictions_data[f"{vessel_size.lower()} vessel transit"]` las colapsaría (como sí se hace en `validate_resources_restrictions:111`).
- **`app.py` es largo (461 líneas)** y la función `add` concentra casi todo (74-392), con la máquina de estados de `session_state` bastante entrelazada. Es funcional, pero cuesta seguir.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí 16 escenarios contra la lógica real. Numerados:

1. **Tránsito pequeño válido** (7h, 1 junior, 1 remolcador, carril `A1,C1,P1`) → `static_validations` = `True`, sin errores; disponibilidad `(True, None)`. ✅
2. **Duración inválida** (tránsito pequeño de 2h) → `False`, `"Small vessel transit duration must be between 6 and 8 hours."` ✅
3. **Fecha en el pasado** → `False`, `"Neither start nor end datetime can be in the past."` ✅
4. **Recursos incorrectos** (2 junior en un pequeño, requiere 1) → `False`, mensaje preciso con el valor requerido. ✅
5. **Esclusas incorrectas** (`A1,A2,A3` en vez de un carril) → `False`, `"A small vessel transit requires three locks in a row..."` ✅
6. **Solapamiento en el mismo carril** (dos tránsitos que comparten `A1,C1,P1`) → detecta `"Lock A1 is already in use at 2026-07-17 09:00 ."` (y C1, P1). ✅
7. **Sugerencia de próximo horario** → devuelve `(True, "The selected datetime is unavailable", 15:00, 22:00)`, justo después de que se libera el carril. ✅ Excelente.
8. **Otro carril, sin conflicto** (`A2,C2,P2`) → disponibilidad `True`. ✅
9. **Agotamiento de pilotos junior** (2 tránsitos concurrentes agotan los 2 junior; un tercero) → `"Not enough junior pilots available..."` hora a hora. ✅ La contabilidad acumulativa por hora funciona.
10. **Mantenimiento válido** (1 equipo, 1 esclusa `P1`, 4h) → `True`. ✅
11. **Mantenimiento con 2 esclusas** → `False`, `"A lock maintenance requires only one lock."` ✅
12. **Recurrente válido** (2 repeticiones, intervalo 7 días) → `static_validations_recurrences` = `True`, `validate_recurrences` = `True`. ✅
13. **Recurrente que excede 60 días** (30 rep × 5 días) → `False`, `"Recurring event limits have to be within the next 60 days."` ✅
14. **Persistencia**: guardar 1 único + 1 recurrente×3 → `events.json` = `['1', evento, evento×3, '5']`; fechas de recurrencia correctas (`07-18, 07-20, 07-22` con intervalo 2). Borrar id=2 → `['1','3','4','5']`. ✅ El contador de IDs (sentinela string al final de la lista) funciona.
15. **GUI headless**: arranca limpio, HTTP 200, health `ok`, sin `Traceback`.
16. `py_compile` de ambos módulos: OK bajo 3.12.

**Bug real detectado (del estudiante):**
- **`event.get("tugboat")` en la vista de eventos** (`app.py:433` y `app.py:444`): la clave guardada es `"tugboats"` (`app.py:286`), pero al mostrar tránsitos se lee `"tugboat"` (singular). Verifiqué: `event.get("tugboat")` devuelve `None`. **Todo tránsito programado mostrará "Tugboats: None"** en la página "Scheduled events". No revienta (es un `.get`), pero es un dato siempre incorrecto en la UI. Corrección trivial: `event.get("tugboats")`.

**Fragilidades ante entrada basura (contexto atenuante):**
- Un `Event` construido con `start_datetime="not-a-date"` lanza `ValueError` (`scheduler.py:17`), y con `maintenance_teams=None` lanza `TypeError` (`scheduler.py:86`, `None > int`). **Sin embargo**, en la app real estos valores no pueden producirse: los widgets `st.number_input`/`st.date_input`/`st.time_input` restringen tipo y rango en origen (`app.py:215-250`, `116-141`). No es un bug alcanzable por el usuario, solo una fragilidad si la lógica se reusara fuera de la GUI.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **`type == "..."` sobre la variable `type`** (`app.py:105`, `276`, `337`, etc.): usa `type` como nombre de variable, sombreando el builtin. Funciona, pero es mala práctica; mejor `event_type`.
- **`type(x) == str`** (`app.py:43`, `401`; `scheduler.py:137`) → idiomático sería `isinstance(x, str)`.
- **Nombres claros y consistentes** en general (`validate_*`, `used_junior_pilots`, etc.). Buen hábito.
- **Datos globales a nivel de módulo** (`scheduler.py:8-9`, `resources_data`/`restrictions_data` cargados al importar) — aceptable aquí, pero significa que un cambio en los JSON no se recarga sin reiniciar.
- **f-strings con comillas anidadas del mismo tipo** (`scheduler.py:95`) — legales en 3.12, pero conviene usar comillas simples dentro para máxima compatibilidad y legibilidad.

Nada de esto es grave para 1er año; son pulidos menores.

## Dimensión 5 — Datos y persistencia

- **Modelo:** eventos como dicts JSON con `id`, `type`, `start/end_datetime` (ISO), `subtype`, `vessel_size`, contadores de recursos y `locks`. `resources.json` (capacidad total) y `restrictions.json` (reglas por subtipo) como configuración externa.
- **Contador de IDs ingenioso pero frágil:** el último elemento de la lista `events.json` es un string con el próximo ID (`app.py:19`, `"next_id = int(events_data[-1])"`). Funciona (lo verifiqué), pero mezcla dos tipos en una misma lista, obligando a filtrar `type(x)==str` en cada recorrido (`app.py:401`, `scheduler.py:137`). Un dict `{"next_id": N, "events": [...]}` sería más limpio; el propio informe reconoce esta tensión (report.md:163).
- **Serialización correcta:** datetimes se guardan como ISO y se reparsean con `datetime.fromisoformat`. Sin problemas.
- El repo trae `events.json` vacío (`[]`), estado inicial correcto.

## Dimensión 6 — Informe (`report.md`)

2.710 palabras, bien redactado y **honesto**. Coincide con el código:
- Recursos (2/2/6/3 y 9 esclusas): exacto (report.md:39-46 vs `resources.json`).
- Duraciones por tipo (mantenimiento 4-5h; tránsito 6-8/8-11/10-14h): exactas (report.md:74-78 vs `restrictions.json`).
- Búsqueda hora a hora del próximo horario disponible: descrita fielmente (report.md:96 vs `scheduler.py:149-179`).
- **No exagera:** describe la validación como reglas implementadas, no afirma haberla "probado" ni incluye claims de testing inexistente. De hecho lista "añadir pruebas automatizadas" como mejora futura (report.md:179), reconociendo que no las hay. Muy correcto.

Única discrepancia menor con la realidad: el informe no menciona el bug `tugboat`/`tugboats`, lo cual es esperable (no lo detectó). Y `requirements.txt:1` declara `Python>=3.10` cuando el código exige 3.12 — inconsistencia que el informe no aborda.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y notablemente ambicioso** para 1er año. La separación GUI/dominio es de manual, las validaciones están bien descompuestas y —lo más difícil— el motor de disponibilidad hora a hora con contabilidad acumulativa de recursos y sugerencia del próximo horario libre **funciona de verdad**: lo verifiqué con solapamientos de esclusas, agotamiento de pilotos y expansión de recurrencias, todo correcto. La persistencia guarda, expande y borra bien. El único bug real que reproduje es cosmético (`tugboat` singular → muestra "None" en la lista), de corrección trivial. El informe es honesto y no infla resultados.

- **Principal fortaleza:** el diseño desacoplado + el motor de validación de disponibilidad de recursos por franja horaria, que ejecuté y se comporta correctamente en escenarios complejos (conflictos de esclusas, agotamiento de pilotos, recurrencias).
- **Principal área de mejora:** corregir el bug `event.get("tugboat")` → `"tugboats"` (`app.py:433,444`) y alinear `requirements.txt` con la versión de Python realmente requerida (3.12, no 3.10). Secundariamente, reducir la duplicación en `validate_duration_restrictions`.
