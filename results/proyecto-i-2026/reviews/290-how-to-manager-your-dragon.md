# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #290
- **Repositorio:** https://github.com/Sourcecamp06/How-to-manager-your-dragon
- **Estudiante:** Damiam David Fuentes Campos
- **Grupo:** C121
- **Descripción declarada:** Organizador de eventos ambientado en la película *How to Train Your Dragon*.

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una app web construida con **Streamlit** (`requirements.txt`: `streamlit==1.32.0`). El punto de entrada `main.py` renderiza un menú y navega entre dos páginas (`UI/page/form.py`, `UI/page/events.py`). Ejecutarla con `python main.py` produce el warning esperado de Streamlit y no es la forma correcta de correrla.

Adapté la ejecución así:
1. **Lógica de negocio en aislado.** Los modelos (`models/`) están limpiamente separados de la GUI, así que instancié `GestorEventos`, creé objetos `Evento` reales y ejecuté las 9 reglas de validación con datos concretos, sin arrancar Streamlit.
2. **Persistencia real.** Ejercité el ciclo crear → `guardar_en_json` → recargar en un gestor nuevo (`cargar_desde_json` → `compilar_participacion_diaria`).
3. **Arranque headless de la GUI.** `streamlit run main.py --server.headless true --server.port 8899` levantó correctamente (`HTTP 200`, sin traceback del código del estudiante).
4. `py_compile` sobre los 9 módulos → **compilan todos**.

---

## Dimensión 1 — Qué hace el programa

Sistema para planificar eventos de la Isla de Berk respetando reglas de negocio y controlando inventario de recursos.

- `main.py:141-155` — menú principal con dos botones ("Añadir evento", "Eventos activos") que conmutan `st.session_state.page` y hacen `st.rerun()`.
- `UI/page/form.py:202-464` — formulario dentro de `st.form`: título, tipo de evento (`gestor.type_of_events`, 6 tipos), arena (5 arenas), fechas/horas, y selección de guerreros/dragones de franquicia (checkbox con imagen), guerreros "random" y dragones libres (por cantidad), armas, armaduras y ovejas.
- `form.py:342-425` — al enviar, aplica una batería secuencial de validaciones: fecha futura con ≥1 día de antelación (`form.py:348`), fecha inicial ≤ final (`form.py:352`), nombre no vacío (`form.py:358`), nombre no duplicado (`form.py:362`), duración > 0 (`form.py:368`), y **9 reglas de negocio** (`form.py:372-425`).
- `form.py:427-464` — si todo pasa, construye el `Evento`, lo agrega a `gestor.eventos`, **descuenta los recursos del almacén** y persiste a JSON.
- `UI/page/events.py:78-212` — lista los eventos activos en expanders con imágenes de guerreros/dragones/armas/armaduras/ovejas, y permite **eliminar** un evento devolviendo los recursos al inventario (`events.py:190-210`).

Verificado por ejecución: el flujo completo de reglas, la creación de eventos y la persistencia funcionan de verdad.

## Dimensión 2 — Organización del código

**Fortaleza destacada para 1er año.** El proyecto tiene una separación de capas real y bien pensada:

- `models/creador_de_eventos.py` — clase `Evento` (datos + validación de fechas en el constructor + `to_dict`/`from_dict`).
- `models/gestor_de_recursos.py` — `GestorEventos`, el "almacén" e inventario, disponibilidad de arena, persistencia JSON, registro de participación diaria.
- `models/validador_de_reglas.py` — las 9 reglas como **funciones puras** (`gestor, ...) -> (bool, str)`), un patrón muy limpio y testeable.
- `UI/` — sólo presentación (Streamlit), sin lógica de negocio embebida.

Esta modularidad es lo mejor del proyecto: pude probar toda la lógica sin tocar la GUI precisamente porque está bien desacoplada.

**Debilidades menores:**
- Nomenclatura mezcla español e inglés (`franquicia_warriors`, `finish_date`, `type_of_events`) — inconsistente pero legible.
- `models/gestor_de_recursos.py:106-107` — `verificar_recursos_disponibles` es un método con `pass`: código muerto.
- `UI/page/events.py:68-74` — función interna `cargar_eventos` con `Evento(**evento)` nunca se llama (los eventos vienen de `gestor.eventos`); es código muerto que además compararía fechas como strings.
- CSS masivo repetido inline en las tres páginas (`main.py`, `form.py`, `events.py`) — funciona, pero difícil de mantener.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente fue **ejecutado**, no leído:

1. **Validación de fechas del `Evento`** (`creador_de_eventos.py:7-10`): `finish < start` → `ValueError("La fecha de fin no puede ser anterior...")`; mismo día con `finish_time <= start_time` → `ValueError` correcto. ✅
2. **`to_dict`/`from_dict` roundtrip** — ida y vuelta idéntica. ✅
3. **`duration`** (`gestor:125`) — 10:00→12:00 devuelve `120.0` min. ✅
4. **Regla 1 — disponibilidad de arena** (`gestor:110`): solapamiento en misma arena → `(False, "La arena no esta disponible...")`; arena distinta → `(True, "")`. ✅
5. **Regla 3 — dragón con su guerrero** (`validador:36`): Astrid sin Tormenta → rechaza con `"El Astrid no puede montar si no es en Tormenta"`; Astrid con Tormenta → acepta; sólo Brutacio sin Brutilda → `"Los hermanos Brutacio y Brutilda deben ir juntos"`. ✅
6. **Regla 4 — balance/cremallerus** (`validador:55`): 1 guerrero + 1 dragón → acepta; Cremallerus con 2 guerreros → acepta (cuenta el dragón como 2). ✅
7. **Regla 5 — ovejas** (`validador:85`): en Playa acepta; fuera de Playa, sin ovejas, y ovejas en evento no-oveja → los tres mensajes correctos. ✅
8. **Regla 6 — excursión** (`validador:99`): fuera de "Guarida de dragones" → rechaza. ✅
9. **Reglas 7/8 — dragones obligatorios/prohibidos** (`validador:109,120`): evento montado sin dragones → rechaza; "Pelea entre vikingos" con dragón → rechaza. ✅
10. **Regla 9 — colisiones** (`validador:131`): Hippo+Patán y Bocón+Estoico → rechazan; par normal → acepta. ✅
11. **Persistencia**: crear evento multi-recurso → `guardar_en_json` → recargar en gestor nuevo restauró eventos, recursos y participación diaria correctamente. ✅
12. **Arranque headless Streamlit** → `HTTP 200`, sin traceback del código del estudiante (los `SyntaxWarning` visibles provienen de la librería Streamlit, no del proyecto). ✅

**Bug latente (no alcanzable en el flujo real):** `gestor.recomendar_fecha` (`gestor:133-150`) crashea con `IndexError` si se le pasa una arena sin eventos (accede a `events_list[len-1]` con lista vacía). En la práctica sólo se invoca cuando la arena **no** está disponible (`form.py:376-377`), es decir cuando ya hay ≥1 evento solapado, por lo que la lista nunca está vacía en el flujo real. Confirmado por ejecución: con 1 y 2 eventos devuelve recomendación sin crashear; sólo con lista vacía revienta. Recomendable blindarlo igualmente.

**Inconsistencia menor en participación multi-día:** al crear (`form.py:457-460`) sólo se registra la participación en `start_date`. En cambio `compilar_participacion_diaria` (`gestor:236-252`, llamado al recargar) sí expande el rango completo. Resultado: en la misma sesión un evento de varios días sólo bloquea su día inicial; tras recargar bloquea todos. No es un crash, pero la Regla 2 queda parcialmente aplicada dentro de una sesión.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Muy bien:** reglas como funciones puras con retorno `(bool, mensaje)`; uso de `datetime`/`date`/`time` correcto; `try/except` en la capa de persistencia (`gestor:155-182`, `184-220`).
- `try/except` desnudos (`except:`) en `events.py:98,122,145,164,186` — atrapan todo; mejor `except FileNotFoundError` o similar.
- `models/validador_de_reglas.py` importado con `from ... import *` en `form.py:8` — funciona pero oculta qué se usa.
- Código muerto ya señalado (`verificar_recursos_disponibles`, `cargar_eventos`).
- Cadena de docstring usada como comentario en `events.py:131` (`""" ... """` suelto) — inofensivo pero no idiomático.

Son detalles esperables y menores en un primer año.

## Dimensión 5 — Datos y persistencia

- Modelo de datos claro: cada `Evento` se serializa con `to_dict` (fechas/horas en ISO string) y se reconstruye con `from_dict` (`creador_de_eventos.py:30-67`).
- `guardar_en_json` (`gestor:153`) persiste eventos + estado del inventario (`randoms_warriors`, `free_dragons`, `weapons`, `armors`, `ovejas`) + `daily_participation` (con `set` convertidos a `list`).
- `cargar_desde_json` (`gestor:184`) restaura todo y reconstruye los `set` de participación; crea la carpeta `data/` si no existe (`gestor:101-102`).
- Decisión sensata: al eliminar un evento se **devuelven** los recursos al almacén (`events.py:190-210`), manteniendo el inventario coherente. Verificado en ejecución.

La persistencia es funcional y consistente; es una fortaleza real del proyecto.

## Dimensión 6 — Informe (`report.md`)

2.041 palabras. Bien redactado, con narrativa temática y estructura clara.

- **Coincide con el código** en lo esencial: los 6 tipos de evento, las 5 arenas, las 9 reglas y el inventario declarado corresponden a lo implementado en `gestor.__init__` y `validador_de_reglas.py`.
- **Discrepancias menores:**
  - El informe lista sólo 4 armaduras ("Cascos, Pecheras, Pantalones, Botas") pero el código define **5** (`gestor:77-83` incluye "Cinturon de cuero": 10).
  - El informe describe la Regla 2 como "un guerrero de la franquicia **y su dragón** solo pueden participar en un evento al día", pero la implementación (`validador:16-32`) sólo mira **participantes de franquicia** ya registrados ese día; además la registración al crear es sólo de `start_date` (ver Dimensión 3), así que la regla se cumple plenamente sólo tras recargar.
  - Estructura de carpetas del informe incluye `data/__init__.py`, que no existe en el repo (sólo `data/eventos.json` se genera en runtime).
- El informe usa "demuestra"/"garantiza" en las conclusiones; es coherente con lo que efectivamente ejecuté, aunque no hay evidencia de pruebas automatizadas — la validación es manual vía la GUI.

En conjunto el informe es honesto y refleja bien el proyecto; las discrepancias son cosméticas.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido**. La arquitectura por capas (modelos / validador / GUI) es notablemente limpia para un primer año: permitió ejecutar toda la lógica de negocio en aislado y comprobar que las 9 reglas, la disponibilidad de arena, la duración, la persistencia JSON y el arranque de la GUI **funcionan de verdad**. El proyecto es ambicioso (Streamlit con imágenes, inventario con devolución de recursos, persistencia) y lo ejecutó correctamente. Los defectos son menores: código muerto, dos `except` desnudos, un `IndexError` latente pero no alcanzable en el flujo real, y una registración de participación diaria que sólo cubre el día inicial dentro de una misma sesión.

- **Principal fortaleza:** separación de responsabilidades real y bien lograda — reglas de negocio como funciones puras testeables, totalmente desacopladas de la interfaz.
- **Principal área de mejora:** consistencia en la registración de participación multi-día (aplicar en creación el mismo `compilar_participacion_diaria` que usa la recarga) y limpiar el código muerto / los `except:` desnudos.
