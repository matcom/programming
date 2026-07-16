# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #327
- **Repositorio:** https://github.com/ErielMichel/Furvito-en-la-Polar.git
- **Estudiante:** Eriel Michel
- **Grupo:** C111
- **Descripción declarada:** Aplicación para gestionar eventos deportivos (partidos, entrenamientos y mantenimiento) que valida restricciones de recursos, evita conflictos de horario y sugiere automáticamente el próximo espacio disponible.

---

## Nota metodológica importante

Es una aplicación de **consola** con menú `input()` (`main.py:22-180`), no una GUI, pese a que `requirements.txt` declara `streamlit`, `pandas`, `numpy`, `altair`, etc. En la práctica el programa **solo usa la biblioteca estándar** (`sqlite3`, `datetime`); ninguna de las dependencias pesadas del `requirements.txt` se importa. Se ejecutó alimentando el menú con `printf '...' | python main.py` recorriendo las seis opciones con flujos válidos e inválidos, y además se invocó la lógica de negocio (`Scheduler`, `Database`, `validate_constraints`) directamente para aislar bugs. `py_compile` de todos los módulos: **OK**.

## Dimensión 1 — Qué hace el programa

Menú de consola (`main.py:6-16`) con seis opciones:

1. **Listar eventos** (`main.py:26-41`): lee `db.show_events()` y, por cada evento, sus recursos asignados vía `get_event_resources`. Verificado: muestra `[id] nombre (tipo)`, rango de fechas y recursos.
2. **Crear evento con validación** (`main.py:43-108`): pide nombre, tipo, fecha ISO, duración y una lista de recursos `nombre:cantidad`; valida restricciones, verifica disponibilidad y persiste. Verificado end-to-end.
3. **Eliminar evento** (`main.py:110-123`): libera recursos y borra (`delete_event_complete`).
4. **Listar recursos** (`main.py:125-130`): 18 recursos semilla (`database.py:47-66`).
5. **Buscar horario (Scheduler)** (`main.py:132-172`): `find_next_slot` busca en los próximos 7 días (8:00–19:00) el primer hueco donde todos los recursos estén disponibles; opcionalmente crea el evento.
6. **Salir**.

El modelo de datos vive en SQLite (`events/events.db`) con tres tablas: `resources`, `events`, `event_resources` (`database.py:16-45`).

## Dimensión 2 — Organización del código

Buena separación en paquete `events/`:

- `database.py` — capa de persistencia (SQLite), CRUD de eventos, asignación de recursos.
- `Constraints.py` — validación de restricciones por tipo de evento (función pura).
- `scheduler.py` — búsqueda de horario.
- `models.py` — clase `Resource` con `wear`/`release`.
- `main.py` — CLI.

**Fortalezas:** la lógica de negocio está separada de la interfaz, lo que permitió ejecutarla en aislamiento; `validate_constraints` (`Constraints.py:1`) es una función pura fácil de testear; nombres en general claros.

**Debilidades:**
- `models.py` (clase `Resource`) **está muerto**: no se importa en ningún módulo. Toda la gestión de cantidades se hace por SQL en `database.py`. Igual `event_templates.py` (`Templates_events`): definido pero nunca usado (main.py inline los tipos como texto).
- Acoplamiento cíclico latente: `scheduler.py:2` importa `Database`, y `database.py:2` importa `Constraints`; `Scheduler.__init__` crea su **propia** instancia de `Database` (`scheduler.py:6`), distinta de la que usa `main` — funciona porque ambas apuntan al mismo fichero, pero es frágil.
- Cada método de `Database` abre y cierra su propia conexión; para 1er año es aceptable, pero repite mucho `get_connection()/close()`.
- Nombre de fichero `Constraints.py` con mayúscula rompe la convención `snake_case` del resto.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Lo que se corrió y observó:

1. **Listar recursos y eventos (vacío):** correcto. Muestra los 18 recursos y "No hay eventos programados".
2. **Crear Official Match válido** (`referee:4, banderole:2, whistle:1, red card:1, yellow card:1, ball:3, pennant:4, soccer goal:2, goal net:2`, 2026-08-01 10:00): **OK**. Asigna los 9 recursos, crea el evento ID 1, y al listar aparece con sus recursos. Flujo completo funciona.
3. **Conflicto de horario:** un segundo Official Match solapado (11:00, mismos recursos) fue **rechazado y revertido** correctamente ("Evento ID 2 eliminado" → rollback). La detección de conflicto y el rollback en `create_complete_event` (`database.py:224-228`) funcionan.
4. **Restricciones violadas** (Official Match con `referee:1, ball:1`): lista los 9 mensajes de restricción y no crea el evento. Correcto.
5. **Fecha inválida** (`not-a-date`): capturada con mensaje amigable (`main.py:59-64`). Correcto.

**Bugs confirmados por ejecución:**

- **[BUG grave] `scheduler.py:14` — `ValueError: day is out of range for month`.** El cálculo del candidato usa `start_from.replace(day=start_from.day + day, ...)` con `day` de 1 a 7. Cuando `start_from.day + 7 > días_del_mes` (es decir, del 24 en adelante en un mes de 31 días, antes en meses cortos), `replace` **revienta con excepción no capturada**. Reproducido: `find_next_slot({'ball':1}, 2, start_from=datetime(2026,7,31,8,0))` → `ValueError`. El Scheduler está roto la última semana de todos los meses. La corrección idiomática es `candidate_start = (start_from + timedelta(days=day)).replace(hour=hour, minute=0, second=0, microsecond=0)`.

- **[BUG grave — diseño] Los recursos se descuentan globalmente, no por franja horaria.** `assign_resource_to_event` (`database.py:205-206`) hace `UPDATE resources SET available_quantity = available_quantity - ?`. Así, `available_quantity` es un contador **global permanente**, no una disponibilidad temporal. Reproducido: tras crear el Official Match del 1-ago que usa `soccer goal:2` (total 2), pedir disponibilidad de `soccer goal` para un evento del **2-ago** (sin solape) devuelve `False`. Consecuencia: un recurso escaso usado por *cualquier* evento queda inutilizable para *todos* los demás eventos, sin importar el tiempo — lo que **anula el propósito del scheduler basado en tiempo**. La comprobación de solape temporal en `check_resources_availability` (`database.py:149-157`) queda eclipsada por el descuento global de la línea 145. El diseño correcto es no descontar el pool global, sino sumar las cantidades reservadas por eventos *que solapan* en la franja consultada.

- **[BUG] Duración no numérica revienta el programa.** `float(input(...))` sin `try/except` en `main.py:66` (crear evento) y `main.py:135` (scheduler). Entrada `abc` → `ValueError: could not convert string to float` no capturado, el programa **termina abruptamente**. La fecha sí está protegida; la duración no.

- **[BUG] Restricción de `fence` en Training es código muerto.** `Constraints.py:45`: `if resources_dict.get("fence",0) < 0` — nunca es cierto (una cantidad nunca es negativa), pero el mensaje dice "needs at least 3 fences". Reproducido: un Training con `fence:0` **no** genera error. Debería ser `< 3`.

- **[Menor] EOF en el menú** (`main.py:24`) lanza `EOFError` sin capturar si la entrada se agota. Aceptable para 1er año, pero un `input()` mal alimentado tumba el programa.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- `except:` desnudos en `main.py:61, 82, 146` y en varios `except Exception as e` (`database.py:212, 291`). Para 1er año es un patrón común; conviene señalar que `except:` captura hasta `KeyboardInterrupt`.
- Mensajes de error mezclan español (UI) e inglés (capa de datos): "Evento creado" vs "assigned to event". Inconsistente pero no bloqueante.
- Errores tipográficos en mensajes de restricción: "need at least 1 yelow card" (`Constraints.py:26`), "needs at least 3 mini soccer goal" cuando el chequeo es `< 4` (`Constraints.py:57-58`).
- No usa `if __name__` en los módulos que crean `Database()` a nivel de import (`scheduler.py` lo hace en `__init__`, correcto).
- Buen uso de consultas parametrizadas SQL (`?`), evita inyección — punto a favor.

## Dimensión 5 — Datos y persistencia

Modelo relacional correcto: `resources`, `events`, `event_resources` con clave compuesta y `FOREIGN KEY ... ON DELETE CASCADE` (`database.py:42`). **Ojo:** el CASCADE no se activa porque SQLite requiere `PRAGMA foreign_keys = ON` por conexión, que no se establece; por eso la limpieza de `event_resources` al borrar un evento se hace manualmente en `delete_event_complete` (`database.py:278-287`), lo cual funciona. `INSERT OR IGNORE` para sembrar recursos idempotentemente (`database.py:70`) está bien pensado. Hay **dos** ficheros `.db` en el repo (`events/events.db` y `events/data/events.db`); solo el primero es el que usa el código (`database.py:5`), el segundo está huérfano/vacío y no debería commitearse.

## Dimensión 6 — Informe (`report.md`)

El informe está bien escrito y es extenso, pero **sobreestima lo implementado**:

- **§4.2 "Restricción de Exclusión Mutua"** (report:65-69): afirma que "un evento de Field Maintenance no puede usar escaleras por razones de seguridad". **No existe tal lógica** en `Constraints.py`: no hay ninguna restricción de exclusión, solo mínimos por tipo. Reclamo no respaldado por el código.
- **§4.3 "Límites por Tipo"** (report:71-73): afirma que "un entrenamiento no puede exceder los 25 balones". `Constraints.py` **solo valida mínimos** (`< 10`, `< 30`, ...), nunca máximos. No implementado.
- **§4.1 "Co-requisito"** (report:57-63): lo descrito como co-requisito es en realidad un chequeo de cantidad mínima por tipo de evento, no una dependencia entre recursos. La descripción es más ambiciosa que la implementación.
- **§6 "Flujo de trabajo"** (report:109) menciona que se verifica disponibilidad "en el horario solicitado" — cierto en intención, pero por el bug de descuento global (Dim. 3) la verificación temporal no funciona como se describe.
- La **Conclusión** (report:158) dice "El proyecto cumple con todos los requisitos" y "se han seguido las buenas prácticas"; matizado por los bugs y el código muerto anteriores.

El informe describe un sistema más completo (exclusión mutua, límites máximos) del que el código realmente implementa.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido en su núcleo pero con problemas verificados**. La arquitectura por capas es buena para 1er año, la persistencia en SQLite con consultas parametrizadas está bien hecha, y los flujos principales —crear evento con validación de restricciones, detectar conflictos de horario con rollback— funcionan de verdad al ejecutarlos. Se nota ambición y comprensión del dominio. Sin embargo, el Scheduler tiene dos fallos que comprometen su función declarada: revienta con excepción la última semana de cada mes, y el descuento global de recursos anula la lógica temporal que el propio informe presume. A eso se suman una restricción muerta (`fence < 0`), un crash por duración no numérica y código muerto (`models.py`, `event_templates.py`). El informe, además, describe restricciones (exclusión mutua, límites máximos) que no están en el código.

- **Principal fortaleza:** el flujo de creación de eventos con validación de restricciones y detección de conflictos + rollback funciona end-to-end sobre una capa de datos SQLite bien estructurada y parametrizada.
- **Principal área de mejora:** arreglar el Scheduler — usar `timedelta(days=...)` en vez de `replace(day=...)`, y reconsiderar el descuento global de `available_quantity` para que la disponibilidad sea temporal (solo cuenta recursos de eventos que solapan la franja), que es lo que el informe promete.
