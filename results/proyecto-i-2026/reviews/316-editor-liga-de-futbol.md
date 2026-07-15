# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #316
- **Repositorio:** https://github.com/Alec-As/Editor_Liga_de_Futbol
- **Estudiante:** Alec Quiñones Romero
- **Grupo:** C-122
- **Descripción declarada:** Editor/planificador de una liga de fútbol. Automatiza la creación de partidos y viajes validando restricciones reales: recursos limitados (vehículos, árbitros, médicos, seguridad), descansos obligatorios entre tareas, capacidad de estadios y reglas de competición (local obligatorio, control de enfrentamientos repetidos).

---

## Nota metodológica importante

Esto **no es una aplicación de consola**: es una GUI de escritorio en Tkinter (`GUI.py`, `main.py:1-4`). El punto de entrada `main.py` importa `main` desde `GUI.py`, que abre una ventana con `tk.Tk()` y entra en `root.mainloop()` (`GUI.py:449-452`). No hay `input()` en ninguna parte, así que alimentarla con `printf` no aplica.

Adaptación de la ejecución:

1. **Intento de arranque headless de la GUI**: probé `python main.py` directamente y bajo `xvfb-run`. En ambos casos aborta con `[xcb] Aborting … Assertion !xcb_xlib_unknown_seq_number failed` / `Aborted (core dumped)`. Es el **mismo fallo de entorno** que reportó la verificación automática del issue (`root = tk.Tk()`), un problema de X11/xcb del entorno de ejecución, **no un bug del estudiante**. El código de construcción de la interfaz compila limpio y usa widgets estándar de Tkinter.
2. **Ejecución real de la lógica de negocio**: como la lógica está bien separada de la GUI (toda vive en `LeagueManager`, `Manager.py`), la instancié directamente y ejercité los flujos completos con datos reales del repo: creación de partidos/viajes válidos e inválidos, descansos, capacidad, recursos, aritmética de fechas y persistencia. Todo lo que reporto en la Dimensión 3 proviene de esa ejecución real.
3. `py_compile` de los 7 módulos: **todos compilan sin error**.

Que la lógica sea separable y ejecutable sin la GUI es un mérito de diseño del estudiante, no un accidente.

## Dimensión 1 — Qué hace el programa

El programa gestiona una liga fija de 10 equipos y 10 estadios (`Manager.py:10-36`), con una matriz de distancias 10×10 (`Manager.py:38-49`) y un catálogo de recursos: 3 vehículos con alcance (Avión/Autobús/Van) y 5 instrumentos-personal (Ambulancia, Árbitro, Médico, Seguridad, Cámaras TV) (`Manager.py:51-61`).

El usuario, desde la barra lateral (`GUI.py:35-44`), puede:

- **Agregar Partido** (`create_match`, `Manager.py:334-366`): valida que no sea el mismo equipo, que ambos equipos estén físicamente en el estadio local del equipo 1, que respeten el descanso de 3 días entre partidos, que el enfrentamiento no se haya jugado ya en esa dirección, y que haya recursos-instrumento suficientes ese día. Si todo pasa, registra el partido.
- **Agregar Viaje** (`create_travel`, `Manager.py:368-399`): valida que el destino sea distinto al estadio actual, el descanso de viaje, la capacidad del estadio destino, la disponibilidad de vehículos según la distancia, y que no haya tareas posteriores ya planificadas que el viaje invalidaría.
- **Listar Tareas** ordenadas por fecha (`Manager.py:401-411`), **Ver Detalles** por índice, **Eliminar Tarea** (`Manager.py:419-447`), y **Guardar/Cargar** el estado en `league_state.json`.

La ubicación de cada equipo se calcula dinámicamente buscando su viaje más reciente (`_search_current_location_for_team`, `Manager.py:224-241`); si no ha viajado, está en su estadio de casa. Esta es la idea central del sistema y está bien pensada.

## Dimensión 2 — Organización del código

**Fortalezas notables para un primer año:**

- **Separación GUI / lógica limpia.** Toda la lógica de negocio está en `LeagueManager` (`Manager.py`) y la GUI solo llama a métodos públicos (`create_match`, `create_travel`, `delete_task`, `save_state`, `load_state`) que devuelven diccionarios `{"success": bool, "message": str}`. Ese contrato uniforme (`Manager.py:341, 366, 399…`) es un patrón muy razonable, y es exactamente lo que me permitió ejecutar la lógica sin la ventana.
- **Jerarquía de clases sensata.** `Task` → `Match` / `Travel` (`Task.py:4-46`) y `Resource` → `Vehicle` / `Instrument_Personal` (`Resource.py:4-26`) usan herencia con propósito. `Team` y `Stadium` (`Teams_and_Stadiums.py`) son datos simples y claros.
- **Nombres descriptivos** en español coherente (`_can_play_match`, `_search_current_location_for_team`, `_days_between`).
- Uso de métodos "privados" con prefijo `_` para las comprobaciones internas — buena señal de intención de encapsulamiento.

**Debilidades:**

- **Instancia global del manager** (`Manager.py:630`: `manager = LeagueManager()`) importada por la GUI (`GUI.py:3`). Funciona, pero es una variable global mutable; sería más limpio que la GUI recibiera el manager por parámetro.
- `Match.__str__` (`Task.py:27-30`) muestra IDs crudos, mientras que `LeagueManager.task_to_str` (`Manager.py:323-332`) muestra nombres. Hay dos representaciones del mismo objeto y la GUI las mezcla: el listado usa `task_to_str` (`GUI.py:193`) pero el diálogo de eliminar usa `str(tarea)` directo (`GUI.py:378`), así que el índice mostrado ahí (`{i}.`) y el del listado (`{i+1}`) no coinciden visualmente. Confuso para el usuario, aunque el índice interno es correcto.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté `LeagueManager` directamente con múltiples escenarios. **Lo verificado que funciona:**

1. **Mismo equipo rechazado** — `create_match(0, 0, …)` → *"Un equipo no puede jugar contra sí mismo"*. ✔
2. **Localía / co-ubicación** — `create_match(0, 1, 1/1)` con el visitante en su propio estadio → *"El equipo Centuria FC no se encuentra en el mismo estadio que Atlético Capital"*. ✔ La regla "ambos deben estar en el estadio local del equipo 1" se aplica correctamente.
3. **Flujo viaje→partido** — `create_travel(1→estadio0, 1/1)` (éxito) y luego `create_match(0, 1, 5/1)` (éxito). El equipo 1 viaja a la casa del 0 y el partido queda registrado con ambos co-ubicados. ✔
4. **Enfrentamiento repetido** — tras registrar 0 vs 1, un segundo `create_match(0, 1, …)` → *"Estos equipos ya tienen un partido planificado…"* vía la matriz `played_against` (`Manager.py:298, 357`). ✔
5. **Descanso de 3 días entre partidos** — `create_match(0, 2, 6/1)` (1 día tras el partido del equipo 0) → rechazado *"mínimo 3 días entre partidos"*; a `9/1` (4 días) → aceptado. ✔
6. **Capacidad de estadio** — al Camp Nou (id 5, cap 2) permite 2 viajes entrantes y **rechaza el 3.º**: *"El estadio Estadio Camp Nou no puede alojar más equipos en esta fecha"*. La restricción **sí se dispara**. ✔ (Ver nota de modelado abajo.)
7. **Asignación de vehículos por distancia** — viaje de 500 km asignó `[('Van', 5)]` (5×100). ✔ en cuanto a satisfacer la distancia, pero es **subóptimo**: hay un Avión de alcance 500 (1 vehículo) que habría bastado. La heurística ordena por alcance ascendente (`Manager.py:169`) y llena con los más pequeños primero, agotando toda la flota de vans. No revienta, pero deja el sistema sin vans para otros viajes ese día.
8. **Fechas inválidas no revientan** — `Time(31, 2)` (31 de febrero) se corrige silenciosamente a `01/01/2024` (`Time.py:1-10`). No hay `Traceback`. ✔ (aunque corregir a 1/1 en vez de avisar es discutible; ver Dimensión 4).
9. **Aritmética de fechas robusta** — probé `next_day` en casos límite: `28/2/2024 +1 = 29/02` (bisiesto correcto), `+2 = 01/03`, `31/1 +1 = 01/02`, `15/6 −20 = 26/05` (cruce de mes hacia atrás correcto), y clamps en `31/12 +1` y `1/1 −1`. Todo correcto. ✔ Cálculo de fechas sólido.

**Bugs encontrados (del estudiante), en orden de gravedad:**

- **[Moderado] `save_state` solo guarda la ÚLTIMA fecha del calendario.** Reproducido: con 3 fechas en memoria (`{'01/01':1, '02/01':1, '06/01':1}`), el JSON guardado contiene **solo** `{'06/01': 1}` — se pierden 2 fechas. La causa es de **indentación** en `Manager.py:479-511`: las líneas `tasks_list.append(task_dict)` (509) y `state["schedule"][date_str] = tasks_list` (511) quedan **fuera** de sus bucles. `tasks_list.append` se ejecuta una sola vez por fecha (fuera del `for task`), y la asignación al `schedule` se ejecuta una sola vez en total (fuera del `for date_str`), con la última `date_str` y `tasks_list` que quedaron en las variables. El resultado es que Guardar→Cargar pierde casi todo el calendario **silenciosamente**. El propio `league_state.json` versionado en el repo lo evidencia: tiene una sola fecha (`01/01/2024`) pese a que su nombre sugiere un estado más rico.
- **[Menor, latente] `_days_between` ignora el año** (`Manager.py:262-269`): `_days_between(1/1/2024, 1/1/2025)` devuelve `0`. No afecta en la práctica porque todo el sistema usa el año 2024 por defecto y la GUI nunca pide año (`Time(dia, mes)`, `GUI.py:255, 330`), pero rompería en cuanto se planificara a través de un cambio de año.
- **[Nota de modelado] Capacidad de estadio no cuenta al equipo local residente.** `_can_host_team` inicia `current_teams_count = 1` (`Manager.py:114`) y solo cuenta viajes, no al equipo que vive en ese estadio. Efecto: permite `max_teams` equipos *visitantes* además del local. Es una decisión de modelado defendible, pero difiere de una lectura literal de "capacidad máxima de equipos alojados".

Ninguno de estos produce un `Traceback` en uso normal. El único crash posible sería un ID de equipo/estadio fuera de rango (`IndexError`), pero la GUI lo blinda con `try/except (ValueError, IndexError)` en los diálogos (`GUI.py:267-268, 342-343`), así que el usuario nunca lo ve.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Contrato de retorno uniforme** `{"success", "message"}` — excelente práctica, hace la lógica testeable y predecible.
- **`from X import *`** en varios módulos (`Task.py:1-2`, `Resource.py:1`, `Manager.py:2-5`) — funciona pero contamina el namespace; mejor importar nombres explícitos.
- **`task.__str__()` explícito** en vez de `str(task)` (`Manager.py:82, 100, 295…`) — funciona igual pero no es idiomático; `str(task)` es lo esperado en Python.
- **Anotación de tipo incorrecta pero inofensiva**: `list[(str, int)]` (`Task.py:9`, `Manager.py:141…`) no es una anotación válida de tupla (sería `list[tuple[str, int]]`). Python la ignora en runtime, así que no rompe nada.
- **Corrección silenciosa de fechas inválidas** (`Time.py:7-10`): en vez de avisar, `Time(31, 2)` se convierte en `1/1`. Para un editor que presume de "validar cada solicitud", sería más coherente rechazar la fecha y avisar al usuario, como hace con el resto de restricciones.
- Variable de bucle `_` usada además como contador con significado (`Manager.py:81, 88`) — `_` por convención es "valor descartado"; aquí se compara (`_ >= -1 and _ <= 1`), lo que confunde. Un nombre como `offset` sería más claro.
- Manejo de excepciones razonable en `load_state` (`Manager.py:619-627`): distingue `FileNotFoundError`, `JSONDecodeError` y genérico. Buen detalle.

Son detalles menores, esperables y perdonables en primer año.

## Dimensión 5 — Datos y persistencia

- **Modelo de datos claro**: `schedule` es un `dict[str_fecha → list[Match|Travel]]` (`Manager.py:63`), `played_against` es una matriz 10×10 de booleanos (`Manager.py:65`), y las distancias otra matriz 10×10. Estructuras adecuadas al problema.
- **Serialización a JSON** (`save_state`, `Manager.py:449-531`) con `ensure_ascii=False` (bien, conserva acentos) e `indent=2` (legible). El diseño de la serialización es correcto en intención: guarda equipos, estadios, recursos (incluyendo `reach` de vehículos vía `hasattr`), calendario, `played_against` y matriz de distancias.
- **PERO** la persistencia del calendario está rota por el bug de indentación descrito en Dimensión 3: en la práctica solo se guarda la última fecha. `load_state` (`Manager.py:533-627`) está bien escrito y reconstruye correctamente lo que reciba — el fallo está enteramente en el lado de guardar.
- Detalle: `save_state` en la GUI escribe a un `league_state.json` relativo al directorio de trabajo (`GUI.py:439`, `Manager.py:449`), así que dependerá de desde dónde se lance la app.

## Dimensión 6 — Informe (`report.md`)

El informe (833 palabras, por debajo del mínimo de 2000 que marca la verificación automática) es esencialmente un **manual de usuario / descripción de features**, no un informe técnico de diseño. Coincidencias y discrepancias con el código:

- **Coincide bien** en el catálogo: las tablas de recursos, equipos y estadios (`report.md:71-116`) reflejan fielmente los datos de `Manager.py:10-74`, incluyendo los recursos por estadio derivados de `stadium.id % 2` / `% 3` (`Manager.py:71-74`).
- **Coincide** en las reglas de descanso declaradas (3 días sin partidos, 1 sin viajes tras partido; `report.md:52-57`) con lo implementado en `_can_play_match` / `_can_travel`.
- **Ligera sobreafirmación**: "Los recursos se asignan de forma **inteligente**" (`report.md:48`). La asignación de vehículos funciona pero es una heurística greedy subóptima (usó 5 vans en vez de 1 avión para 500 km, ver Dim. 3.7); "inteligente" es generoso.
- **Afirmación no del todo sostenida por el código**: "Dos equipos solo pueden jugar dos veces, y al menos una vez cada equipo debe haber jugado de local" (`report.md:67`). El código controla que un enfrentamiento *en una dirección concreta* (local→visitante) no se repita vía `played_against[local][visitor]` (`Manager.py:357`), lo que sí permitiría el partido inverso; pero no encontré una comprobación explícita que **garantice** que se juegue el de vuelta ni que limite a exactamente dos. La regla enunciada es más fuerte que lo implementado.
- El informe **no menciona** que Guardar/Cargar pierde el calendario (el bug de `save_state`), lo cual es entendible: probablemente el estudiante no lo detectó.
- El informe no incluye una sección de diseño (por qué esas clases, cómo se calcula la ubicación de un equipo, qué invariantes mantiene el sistema), que es justo lo que elevaría el conteo de palabras y el valor técnico.

No hay exageraciones graves ni features inventadas: lo que el informe describe existe. Las discrepancias son de matiz (una regla enunciada más fuerte que la implementada) y de omisión (el bug de persistencia).

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y ambicioso** para un primer año. El estudiante modeló un dominio genuinamente complejo —planificación con restricciones acopladas de recursos, descansos, ubicación dinámica de equipos y capacidad— y lo implementó con una arquitectura limpia: lógica de negocio completamente separable de la GUI, jerarquías de clases con propósito y un contrato de retorno uniforme que hace el sistema testeable. Al ejecutar la lógica directamente, la gran mayoría de las restricciones funcionan tal como se anuncian: co-ubicación para la localía, descansos entre tareas, control de enfrentamientos repetidos, capacidad de estadio y una aritmética de fechas sorprendentemente robusta (incluyendo años bisiestos y cruces de mes en ambos sentidos). La GUI no pudo arrancar por un fallo de X11/xcb del **entorno**, no del código —el mismo que golpeó la verificación automática— y todos los módulos compilan sin error.

El defecto de mayor peso es un **bug de persistencia en `save_state`** (`Manager.py:509-511`): un desliz de indentación hace que Guardar solo conserve la última fecha del calendario, perdiendo el resto silenciosamente. Es un error fácil de cometer y fácil de arreglar (mover dos líneas dentro de sus bucles), pero tiene consecuencia real: destruye datos sin avisar. Lo acompañan un `_days_between` que ignora el año (latente mientras todo sea 2024) y una asignación de vehículos correcta pero subóptima.

**Principal fortaleza:** la arquitectura — separación GUI/lógica limpia y un modelo de restricciones acopladas que funciona de verdad al ejecutarlo, con validación exhaustiva y sin crashes en uso normal.

**Principal área de mejora:** corregir el bug de indentación en `save_state` (que rompe la persistencia del calendario completo) y, en general, hacer que las validaciones fallen ruidosamente en vez de "arreglar" silenciosamente entradas inválidas (fechas), para estar a la altura de la promesa de "validar cada solicitud".
