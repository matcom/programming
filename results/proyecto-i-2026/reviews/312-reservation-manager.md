# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #312
- **Repositorio:** https://github.com/lacuentadeadrian57-netizen/Reservation-Manager
- **Estudiante:** Adrian Martinez Borrego
- **Grupo:** C121
- **Descripción declarada:** Controlador de reservas. Interfaz para gestionar reservas de distintos locales con distintos recursos a lo largo del tiempo.

---

## Nota metodológica importante

**No es una app de consola.** Es una aplicación web **Streamlit** (`main.py:1`, `st.set_page_config` en `main.py:34`). No tiene `input()`; toda la interacción es vía widgets del navegador. Por tanto la ejecución dinámica se hizo en dos frentes:

1. **Lógica de negocio aislada** (`manager.py`, `loader.py`), que está limpiamente separada de la GUI y se puede instanciar y ejercitar sin Streamlit. Aquí concentré el grueso de las pruebas con datos reales de `save.json`.
2. **Arranque headless de la GUI** con `streamlit run main.py --server.headless true`. El servidor levantó correctamente (`HTTP 200`, Uvicorn en el puerto probado), sin `Traceback`. La GUI funciona.

Entorno: `uv venv --python 3.12` + `streamlit`, `pandas`. `py_compile` de los tres módulos: **OK** en 3.12 (ver Dimensión 3 para el matiz de versión).

## Dimensión 1 — Qué hace el programa

El sistema modela un dominio de reservas con tres entidades: **locales** (locations), **recursos** (resources) y **reservas** (reservations), gobernadas por restricciones. El panel lateral (`main.py:42-52`) da cinco vistas:

- **Make reservation** (`main.py:65-88`): elige fechas, local y opcionales; muestra los recursos por defecto del local y el precio total calculado; botón para añadir.
- **Search details** (`main.py:90-129`): tabla de reservas (pandas `DataFrame`), detalle por ID y borrado.
- **Edit Locals** (`main.py:131-197`): alta/baja de locales, edición de precio, y listado de requisitos+opcionales.
- **Edit Resources** (`main.py:199-257`): alta/baja de recursos, edición de precio y cantidad, y exclusiones.
- **Save and Load** (`main.py:259-276`): persistencia a JSON.

El corazón es `Manager.refresh()` (`manager.py:48-79`): un barrido tipo *sweep-line* que detecta solapamientos por local **y** por agotamiento de inventario de recursos compartidos, y **desplaza** la reserva conflictiva al siguiente hueco libre (`manager.py:72-76`).

## Dimensión 2 — Organización del código

**Separación de responsabilidades clara y por encima del promedio de primer año:**

- `loader.py` — E/S de JSON, con escritura atómica vía archivo temporal + `os.replace` (`loader.py:4-16`). Detalle idiomático y correcto.
- `manager.py` — toda la lógica de dominio en la clase `Manager`, sin ninguna dependencia de Streamlit. Esto es lo que permitió probar la lógica de forma aislada.
- `main.py` — solo presentación (clase `App`), con el estado en `st.session_state`.

Uso consistente de **anotaciones de tipo** en firmas y atributos (`manager.py:13-23`, `manager.py:81`). Nombres en inglés, claros. Funciones auxiliares `code_to_id`/`id_to_code` (`manager.py:5-9`) para IDs tipo `RES-001`.

**Debilidades menores:**
- `manager.py:37-46` (`insert`): la búsqueda binaria itera sobre `self.reservations` pero inserta en el parámetro `reservations`. Funciona porque en la práctica ambos coinciden en el uso principal, pero el parámetro queda como sombra confusa; es un olor a código (en el camino `insert(..., collitions, "end")` de `refresh` se busca contra la lista equivocada). No produjo fallo observable en las pruebas de cascada, pero es frágil.
- Errores de tipeo en nombres: `collitions`/`collition` (por *collisions*), `succeded` (por *succeeded*), `enviroment` en el README. Cosmético.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente se **ejecutó** contra `save.json` (4 locales, 14 recursos, 3 reservas precargadas).

1. **Carga:** `m.load('save.json')` → `True`; locales, recursos y reservas `RES-001..RES-003` cargados; `last_id` recuperado correctamente. **OK.**
2. **Cálculo de precio:** Conference Room, 1 día, +Whiteboard+Coffee → **420** = (200+30+50+100+15+25)×1. Verificado a mano. **Correcto.**
3. **Alta válida:** reserva futura en Meeting Room → `(True, 'Reservation added!')`, `last_id`→`RES-004`. **OK.**
4. **Fecha inicio > fin:** rechazada con mensaje `'...the start date cannot be after the end date'`. **OK** (`manager.py:82-83`).
5. **Fecha en el pasado:** `2020-01-01` rechazada con `'...cannot be before the actual date'`. **OK** (`manager.py:84-85`).
6. **Opcional inválido para el local:** Mirrors en Meeting Room → `'...the selected option Mirrors is invalid'`. **OK** (`manager.py:91-92`).
7. **Exclusión mutua:** Mirrors + Lighting controls en Rehearsal Room → `'...the selected option Lighting controls excludes Mirrors'`. **OK** (`manager.py:88-90`).
8. **Colisión mismo local:** dos reservas solapadas en Conference Room → la segunda se desplaza automáticamente para empezar cuando termina la primera, **preservando la duración**. Mensaje `'Reservation added in start: ..., end: ...'`. **OK** (`refresh`, `manager.py:72-76`).
9. **Contención de recurso entre locales:** con `Screen` reducido a cantidad 1, dos reservas en locales distintos (Conference y Main Auditorium, ambos requieren Screen) que solapan → la segunda se desplaza. El control de inventario compartido **funciona** (`manager.py:60-71`).
10. **Cascada de 3 solapamientos:** tres reservas idénticas en Meeting Room → serializadas en tres huecos consecutivos sin solapamientos (verificado programáticamente). **OK.**
11. **Persistencia round-trip:** `save()` a archivo temporal + `load()` en un `Manager` nuevo → reservas idénticas. **OK** (fechas serializadas con `isoformat`, `manager.py:212-216`).
12. **Alta de recurso con exclusión:** `add_resource('Laser',...,['Projector'])` establece el back-link bidireccional (`Projector` ↔ `Laser`). **OK** (`manager.py:123-125`). Duplicado rechazado.
13. **GUI headless:** el servidor Streamlit levantó (`HTTP 200`), sin `Traceback`.

**Bugs / fragilidades reales encontrados (fallos del estudiante, no del entorno):**

- **B1 — Requiere Python ≥ 3.12, pero el informe declara "3.8+".** Las f-strings de `manager.py:108` y `main.py:116` anidan el mismo tipo de comilla dentro de la expresión (`f"...{_reserve["start"]}..."`), sintaxis válida **solo desde Python 3.12** (PEP 701). Compilado bajo **Python 3.11 → `SyntaxError: f-string: unmatched '['`** (verificado). En 3.8-3.11 el programa **no arranca**. Discrepancia con el informe.
- **B2 — `delete_resource` deja referencias colgantes en exclusiones ajenas.** Al borrar `Mirrors`, la lista `exclusions['Lighting controls']` **sigue conteniendo `'Mirrors'`** (verificado). No limpia los back-links en otros recursos. Latente: no rompió los flujos probados porque `Mirrors` también desaparece de los opcionales, pero deja estado inconsistente.
- **B3 — `update_quantity` tiene una rama muerta.** `manager.py:192-196`: asigna `self.quantity[name] = value` **antes** de comparar `value < self.quantity[name]`, con lo que la comparación siempre es falsa y `refresh()` nunca se dispara al bajar una cantidad. El re-chequeo de disponibilidad tras reducir stock **no ocurre**.
- **B4 — `add_location` con un requisito inexistente crashea.** `manager.py:159` accede a `self.exclusions[requisite]`; con un requisito que no es recurso → **`KeyError`** (verificado). En la GUI está protegido porque los requisitos vienen de un `multiselect` limitado a recursos existentes (`main.py:145-148`), así que **no es alcanzable por el usuario**, pero la lógica no es defensiva.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **A favor:** anotaciones de tipo consistentes; separación GUI/lógica ejemplar; escritura atómica de archivos; búsqueda binaria propia para inserción ordenada.
- **A mejorar:**
  - `loader.py:11` y `loader.py:14` usan `except:` desnudo (captura todo, incluido `KeyboardInterrupt`). Preferir `except Exception:` o, mejor, `except (OSError, json.JSONDecodeError):`.
  - `load_data` (`loader.py:19-23`) no protege el `json.load` contra JSON corrupto: un archivo malformado propagaría `JSONDecodeError` sin manejo. La escritura sí es robusta, la lectura no.
  - Los tipeos en nombres (`collitions`, `succeded`) restan legibilidad.

## Dimensión 5 — Datos y persistencia

Modelo basado en **diccionarios paralelos** indexados por nombre (`price`, `quantity`, `exclusions`, `requisites`, `optionals`) más listas (`resources`, `locations`) y una lista de reservas ordenada. Es un modelo relacional "a mano" simple y funcional. La serialización a JSON convierte `date`↔`isoformat` correctamente en ambos sentidos (`manager.py:212-216`, `manager.py:234-235`). El `id_map` (`manager.py:23`, `manager.py:102`) da acceso O(1) por ID. Persistencia verificada con round-trip. Sólido para el alcance.

## Dimensión 6 — Informe (`report.md`)

- **Contenido duplicado:** el `report.md` (2028 palabras) es el mismo texto **repetido dos veces literalmente** (líneas 1-179 y 180-359, idénticas al `README.md`). El contenido único real es ~1014 palabras — coincide con la marca del chequeo automático. El conteo que supera el mínimo de 2000 es **artificial**.
- **Discrepancia de versión (B1):** el informe declara "Python 3.8+" (`report.md:81`, `report.md:260`) pero el código exige 3.12+. Es incorrecto y verificable.
- **Features exageradas / no implementadas:** el informe habla de reservas con **horas** ("10:00-12:00", `report.md:74-76`, `report.md:253-255`) y "horarios de apertura" (`report.md:95`, `report.md:274`), pero el sistema trabaja **solo con fechas** (`st.date_input`, granularidad de día). El ejemplo de colisión con horas no corresponde al código.
- **Nombres de ejemplo inventados:** "Mesa", "Sillas", "Pantalla Exterior", "Equipo de Modo Silencioso" (`report.md:27`, `report.md:41`, `report.md:58`) no existen en `save.json` (que usa Projector, Screen, Sound Technician, etc.). Son ilustrativos, no del sistema real.
- **A favor:** la sección de arquitectura describe correctamente la separación eventos/recursos/restricciones y las restricciones sí están implementadas (exclusiones, correquisitos, validación de fechas, capacidad, colisiones) — todo verificado en ejecución. El informe **no exagera** el mecanismo central (el sweep-line de colisiones existe y funciona), solo la granularidad temporal.

---

## Valoración global (orientativa, sin nota numérica)

Es un proyecto **sólido y ambicioso** para primer año. La arquitectura separa limpiamente la lógica de negocio de la GUI Streamlit, lo que habla de buen criterio de diseño, y el mecanismo estrella — la resolución de colisiones por barrido (*sweep-line*) que desplaza reservas al siguiente hueco libre considerando tanto solapamiento por local como agotamiento de inventario de recursos compartidos — **funciona de verdad**, verificado con cascadas de hasta tres reservas y contención cruzada de recursos. Las validaciones (fechas, exclusiones, opcionales inválidos) responden con mensajes claros. Los defectos son: una dependencia no declarada de Python 3.12 (el informe dice 3.8+ y el código no arranca por debajo de 3.12), dos bugs de mantenimiento de estado (`delete_resource` deja back-links colgantes, `update_quantity` tiene una rama muerta), un `KeyError` no alcanzable desde la GUI pero presente en la lógica, y un informe con contenido duplicado y features temporales (horas/horarios) que el código no implementa.

- **Principal fortaleza:** el motor de reservas con resolución automática de colisiones por barrido, considerando inventario de recursos compartidos entre locales — funciona correctamente en ejecución real y está bien separado de la interfaz.
- **Principal área de mejora:** honestidad y precisión del informe (eliminar la duplicación, corregir "Python 3.8+" → 3.12+, quitar las horas/horarios que no existen) y robustecer el manejo de estado en las bajas (`delete_resource`, `update_quantity`).
