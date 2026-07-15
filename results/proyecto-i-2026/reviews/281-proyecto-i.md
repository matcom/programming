# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #281
- **Repositorio:** https://github.com/Pablo-2205/Proyecto-I
- **Estudiante:** Pablo Rodriguez Curbelo
- **Grupo:** C122
- **Descripción declarada:** Planificador inteligente para un salón de videojuegos y cine. Gestiona la reserva de recursos (consolas, controles, TVs, PCs, audífonos, etc.) para distintos tipos de eventos, garantizando que no haya conflictos de horario ni violaciones de las reglas de negocio.

---

## Nota metodológica importante

Es una aplicación de **consola** (CLI) con `input()` y un menú de bucle infinito (`main.py:477`). No es GUI, así que la ejecución fue directa alimentando el menú con `printf '...' | python main.py`. Además probé la **lógica de negocio** de forma aislada (importando `Events`, `Resource`, `Planificator` en scripts dentro del propio directorio del repo, porque los módulos usan `import Resource as resource` con nombre absoluto y solo resuelven desde ahí).

Entorno: `uv venv --python 3.12` (Python 3.12.8). Sin dependencias externas, como declara el informe. `py_compile` de los cuatro módulos: **OK**.

## Dimensión 1 — Qué hace el programa

El programa gestiona reservas horarias de un salón de videojuegos/cine. El menú (`main.py:8-24`) ofrece: listar eventos, agregar, eliminar, buscar hueco disponible, guardar/cargar JSON, ver inventario de recursos y ver agenda de reservas.

El flujo central de "agregar evento" (`main.py:46-131` → `Planificator.AddEvents`, `Planificator.py:23-50`) hace, en orden:
1. Rechaza descripciones duplicadas (`Planificator.py:24`).
2. Rechaza **cualquier** solapamiento temporal con eventos existentes (`Planificator.py:28-30`, vía `_events_overlap`, `Planificator.py:52`).
3. Comprueba pares mutuamente excluyentes (`Planificator.py:34-39`).
4. Valida reglas del evento (`Events.validate`) y reserva recursos (`assign_resources`).

La búsqueda de huecos (`find_available_slot`, `Planificator.py:123-176`) recorre día a día, calcula intervalos libres por recurso (`Resource.get_free_intervals`, `Resource.py:62-110`) y busca un intervalo común a todos los recursos requeridos (`_find_common_slots`, `Planificator.py:178-208`).

Ejecución real observada: al agregar `ReservePS5` para `2027-01-15 10:00-12:00, 2 personas`, el evento aparece correctamente en el listado con tipo, horario, duración (2 horas) y estado "✓ Recursos reservados" (`main.py:36-44`).

## Dimensión 2 — Organización del código

Buena separación en cuatro módulos con responsabilidades claras:
- `Resource.py`: jerarquía `Resource` + 12 subclases (PS5, PS4, TV, PC…), cada una con `quantity` y `bookings` de clase. Lógica de disponibilidad por intervalos (`_time_overlap`, `get_available_in_interval`, `is_available`, `book`, `release`).
- `Events.py`: clase base `Events` + 7 subclases concretas. Cada una declara `get_required_resources()`, su `validate()` y sus `assign_resources`/`release_resources`. Uso correcto de herencia y `super().validate()`.
- `Planificator.py`: orquestación (lista de eventos, solapamiento, persistencia JSON, búsqueda de huecos).
- `main.py`: solo interfaz de consola.

Es una arquitectura ambiciosa y bien pensada para primer año: polimorfismo real (`event.__class__.__name__`, `get_required_resources` sobrescrito por clase), serialización por diccionario (`to_dict`/`create_event_from_dict`, `Events.py:25-47, 299-311`). El diseño con recursos como atributos de clase es una decisión discutible (ver Dimensión 5) pero coherente.

Debilidades menores de organización:
- El mensaje "agregado exitosamente" se imprime **dos veces**: una en `Planificator.AddEvents` (`Planificator.py:50`) y otra en `main.py:124`. Confirmado en ejecución. La capa de negocio no debería imprimir; eso es responsabilidad de `main.py`.
- Funciones muertas: `ver_recursos_por_horario` (`main.py:396`) sí se usa (submenú 7.3), pero `Resource.get_next_available_slot` (`Resource.py:112-130`) y `find_slot_for_event` (`Planificator.py:219-227`) nunca se invocan desde `main.py`.
- `newPlanification = Planificator()` en `Planificator.py:229` a nivel de módulo es una instancia global sin uso.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí los siguientes casos:

1. **PS5 válido** (`10:00-12:00, 2 personas`, fecha futura): agregado y listado correctamente. ✓
2. **Descripción duplicada**: `✗ Error: Ya existe un evento con esa descripcion`. ✓
3. **Solapamiento temporal** (Peli 11:00-13:00 sobre Evento A 10:00-12:00): `✗ Error: El evento se solapa con 'Evento A' (10:00-12:00)`. ✓
4. **Formato de fecha inválido** (`fecha-mala`): `✗ Error: Formato de fecha/hora incorrecto`. No revienta. ✓
5. **Fuera de horario** (06:00): `✗ Error: Las Reservas deben ser entre 8 am y 10 pm`. ✓
6. **Fin antes de inicio** (14:00→10:00): `✗ Error: Reservación de Hora incorrecta`. ✓
7. **Fecha en el pasado** (2020): `✗ Error: No se pueden crear eventos en el pasado`. ✓
8. **Número de personas basura** (`abc`): capturado por `except ValueError` (`main.py:126`): `✗ Error: invalid literal for int()`. No revienta. ✓
9. **Opción de menú inválida** (`xyz`): `✗ Opción no válida`. ✓
10. **FIFA con 8 personas**: agregado OK. **FIFA con 5**: rechazado `Deben haber mínimo 8 personas` (`Events.py:184`). ✓
11. **Dota con 16**: OK. **Dota con 10**: rechazado `Solo puede realizarse si hay 16 personas` (`Events.py:159`). ✓
12. **CoD 16 personas edad 16**: OK. **CoD edad 14**: rechazado `Todos los participantes deben ser mayores de 16 años`. ✓
13. **Save/load round-trip**: agregué "Guardado1", guardé, y en un arranque nuevo el evento se recargó y listó correctamente. El JSON serializa `class_name`, `description`, `start`, `end`, `clients`, `minAge`. ✓
14. **Eliminar + liberar recursos**: agregué un evento, la agenda (opción 8) mostró PS5×1 y Control PS5×2 reservados; tras eliminar, la agenda mostró "No hay reservas activas". La liberación por descripción funciona. ✓
15. **Capacidad a nivel de recurso** (probado en aislamiento): `PS5Controller` (qty=10) se agota tras 5 reservas de 2 unidades en el mismo intervalo; la 6ª da "NO disponible". La máquina de disponibilidad es correcta. ✓
16. **find_available_slot** (ReservePS5, 2h, desde 2027-05-01): devolvió `(2027-05-01 08:00, 10:00)`. ✓

Ningún `Traceback` no controlado en flujos normales ni con basura.

**Bug de diseño (importante) — el chequeo de solapamiento global bloquea la capacidad múltiple.**
`AddEvents` rechaza *cualquier* par de eventos que se solape en el tiempo (`Planificator.py:28-30`), sin importar si usan recursos disjuntos. En ejecución confirmé que **no se pueden tener dos PS5 simultáneas** aunque hay 10 PS5 y 10 controles, ni una película (TV) a la vez que una sesión PS5. Consecuencia: toda la maquinaria de `is_available`/`amount_needed`/`quantity` **nunca se ejerce por el flujo normal** — el evento siempre muere antes en el chequeo de solapamiento. El salón se comporta como si tuviera capacidad 1 para todo. Lo correcto sería solapar solo cuando *coincidan recursos* y dejar que la capacidad por recurso (que ya existe y funciona) decida.

**Código muerto — `INCOMPATIBLE_PAIRS`.**
`INCOMPATIBLE_PAIRS = [("PS5","Xbox360"), ("PS4","XboxOne")]` (`Planificator.py:8-11`) nunca dispara: ningún evento pide simultáneamente PS5 y Xbox360 (ni PS4 y XboxOne) en su `get_required_resources()`. Verificado: `ReservePS5` solo pide `{'PS5':1,'PS5Controller':2}`. La regla de exclusión mutua es inalcanzable.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Fortalezas: manejo de errores con `try/except ValueError` en casi todos los puntos de entrada; validación temprana; nombres de método razonables; docstrings breves.

Mejorables (menores para 1er año):
- **`print` en la capa de negocio**: `AddEvents`, `RemoveEvents`, `save_to_file`, `load_from_file` imprimen directamente. La lógica debería *devolver* estado o *lanzar* excepciones y dejar la impresión a `main.py`. De ahí el mensaje duplicado observado.
- **`except Exception` amplio** (`main.py:128, 313; Planificator.py:120`): útil para no reventar, pero puede ocultar bugs. Para principiante es aceptable, solo conviene ser consciente.
- **Bug menor real:** `FifaTournament.__init__` (`Events.py:183-186`) llama `super().__init__(description, start_str, end_str, clients)` **sin pasar `minAge`**, así que el `minAge` recibido se descarta (verificado: pasar `minAge=18` deja `f.minAge=0`). Inofensivo porque FIFA no tiene regla de edad, pero es una fuga silenciosa.
- **Mezcla de idiomas** en nombres (`AddEvents`, `RemoveEvents`, `tags`, `clients` conviviendo con `agregar_evento_manual`, `descripcion`). Cosmético.
- `import sys` en `main.py:1` no se usa.

## Dimensión 5 — Datos y persistencia

Modelo de recursos: cada tipo es una **subclase** con `quantity` y `bookings` como **atributos de clase** (`Resource.py:132-215`). Las reservas se guardan como lista de diccionarios `{'event','start','end','amount'}`. La disponibilidad se calcula por solapamiento de intervalos (`get_available_in_interval`, `Resource.py:16-26`) — correcto y probado.

Implicación de usar estado de clase: el inventario es **global y compartido** por todo el proceso; no hay noción de instancias de recurso individuales (los `id`/`type` del `__init__` de `Resource` nunca se usan). Para este proyecto funciona, pero acopla el estado a las clases (dos `Planificator` compartirían las mismas reservas de recurso). Es una decisión de diseño, no un error.

Persistencia JSON (`save_to_file`/`load_from_file`, `Planificator.py:74-121`): sólida. Serializa por `to_dict`, reconstruye por `create_event_from_dict` (con fallback a `Events` base y aviso si la clase es desconocida). Maneja `FileNotFoundError` y `JSONDecodeError`. El round-trip funciona en ejecución. Nota: al cargar se re-reservan recursos, pero **no se re-chequea solapamiento entre eventos cargados** — coherente con que el JSON se asume válido.

## Dimensión 6 — Informe (`report.md`)

El informe es honesto y coincide bien con el código en lo general: describe correctamente los 7 tipos de evento, los recursos requeridos, la CLI y la estructura de módulos. No exagera con "demuestra/prueba".

Discrepancias detectadas:
- El informe dice "garantizando que no haya conflictos de horario ni violaciones de las reglas de negocio". En la práctica el chequeo de horario es **demasiado estricto** (rechaza incluso eventos con recursos disjuntos), lo opuesto a un bug de sub-validación: el sistema es más restrictivo de lo que el dominio permite (Dimensión 3).
- FIFA: el informe dice "4 PS5, 2 controles PS5 y 4 TVs" y el código coincide (`Events.py:190`), pero es una regla curiosa (4 consolas con solo 2 controles). `Structure.txt` la describe distinto ("2 play controllers, 8+ clientes, 1 TV y (PS5 o PS4)"). El código no implementa la alternativa "PS5 o PS4" ni el mínimo de 1 TV — implementa una versión fija con 4 PS5 + 4 TV. Discrepancia entre `Structure.txt` y la implementación.
- El informe no menciona el submenú de "disponibilidad por horario" (opción 7.3) que sí existe y funciona.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido** para primer año. La ejecución real es robusta: recorrí menús, flujos válidos e inválidos (fechas malas, basura numérica, opciones inválidas, reglas de torneos, save/load, borrado con liberación de recursos) y **nunca reventó con un `Traceback` no controlado**. La arquitectura demuestra comprensión real de POO: jerarquías de eventos y recursos, polimorfismo, serialización y una búsqueda de huecos que funciona. El manejo de errores es consistente.

El defecto que más pesa es de **diseño de negocio, no de robustez**: el chequeo de solapamiento global (`Planificator.py:28-30`) hace que toda la infraestructura de capacidad por recurso — que el estudiante escribió bien y que funciona en aislamiento — quede sin efecto en el flujo normal, comportándose como capacidad 1 para todo el salón. Junto con `INCOMPATIBLE_PAIRS` (código muerto) y el `minAge` perdido en FIFA, son señales de que la validación se construyó por capas que no llegaron a integrarse del todo.

- **Principal fortaleza:** ejecución robusta y arquitectura POO ambiciosa y bien separada, con capacidad por recurso, persistencia JSON y búsqueda de huecos que funcionan de verdad.
- **Principal área de mejora:** corregir el solapamiento para que compita por *recursos*, no por *tiempo global* — así la maquinaria de capacidad (ya escrita) haría su trabajo y el salón permitiría reservas simultáneas de recursos disjuntos.
