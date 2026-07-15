# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #293
- **Repositorio:** https://github.com/yudieraldaysal-cmd/Proyecto_beta_de_programacion
- **Estudiante:** Yudier Alday Salgado Calzada
- **Grupo:** C111
- **Descripción declarada:** Gestor de eventos tipo "torre de control" de aviación, con 13 opciones de consola en Python. Simula despegues, aterrizajes y mantenimiento, gestionando aviones, pilotos y pistas con persistencia en JSON.

---

## Nota metodológica importante

El proyecto es **híbrido**: ofrece una interfaz de consola (`ui/menu.py`, punto de entrada por defecto) y una GUI Tkinter (`ui/gui.py`, invocable con `python main.py --gui`). La verificación se hizo así:

- La **consola** se ejecutó de verdad alimentando el menú con `printf '...' | uv run python main.py`, recorriendo flujos válidos e inválidos.
- La **GUI** no se pudo arrancar por falta de display X11 en el entorno headless (`_tkinter.TclError: couldn't connect to display ""`) — esto es una limitación del entorno, no un fallo del código. Se verificó que `import tkinter` y todos los imports de `ui/gui.py` resuelven sin error, y que la GUI consume **exactamente la misma lógica de negocio** que la consola (mismas funciones de `models/*`), por lo que la corrección de la lógica se validó ejecutando esas funciones directamente en modo headless.
- No hay dependencias externas: `requirements.txt` documenta que todo es biblioteca estándar. `uv venv --python 3.12` sin instalar nada fue suficiente.

## Dimensión 1 — Qué hace el programa

Al arrancar (`main.py:20`, rama sin `--gui`), carga los cuatro JSON de `data/` (aviones, pilotos, pistas, eventos) y muestra un menú de 13 opciones (`ui/menu.py:50-67`):

1. Avanzar el tiempo (`avanzar_tiempo`, events.py:319) — suma una cantidad al reloj global y reporta eventos que caen en el intervalo.
2. Establecer tiempo específico (`establecer_tiempo_actual`, events.py:311).
3. Agregar evento (`ui/menu.py:96-185`) — el flujo más rico: pide tiempo, ID, tipo; valida existencia y disponibilidad de pista/avión/piloto, comprueba compatibilidad de tipo piloto↔avión, calcula prioridad por tipo, crea el evento y marca los recursos como ocupados.
4-6. Submenús de gestión de aviones/pilotos/pistas (`utils/utils.py:25,79,135`) — agregar/ver/guardar.
7. Ver eventos (`view_events_list`, events.py:115).
8-11. Simulación: siguiente evento (events.py:370), varios eventos, por tiempo máximo (events.py:221), calendario completo (events.py:151).
12. Reiniciar el reloj a 0 (`reiniciar_tiempo`, events.py:345).
13. Salir guardando todo (`ui/menu.py:242-250`).

Ejecuté y **confirmé funcionando**: creación de un evento válido, simulación del siguiente evento, simulación por tiempo, gestión de aviones, y salida con guardado.

## Dimensión 2 — Organización del código

Es la **fortaleza central del proyecto**. La separación por paquetes es limpia y coherente para un primer año:

- `models/` — una dataclass por entidad (`Airplane`, `Pilot`, `Airstrip`, `Evento`) con su CRUD y persistencia JSON propia.
- `ui/` — `menu.py` (consola) y `gui.py` (Tkinter), ambas consumiendo `models/`.
- `utils/` — `utils.py` (submenús de gestión) y `time.py` (errores y validaciones compartidas).
- `data/` — los cuatro JSON.

El uso de `@dataclass` con `to_dict()`/`from_dict()` (p.ej. events.py:44-68) es un patrón acertado y lo aplica de forma consistente. Los nombres de funciones son descriptivos (`marcar_pista_ocupada`, `piloto_ocupado`, `simular_por_tiempo`).

Debilidades de organización:

- **Código muerto/duplicado**: `core/manager.py` reimplementa `simular_calendario` y `delete_events_by_time`, pero nadie lo importa; además contiene llamadas rotas como `Airstrip.liberar(evento.pista_id)` (manager.py:53 — llama un método de instancia sobre la clase pasándole un string). Ese archivo está roto y debería borrarse. La versión buena vive en `events.py`.
- **Mezcla de responsabilidades**: `models/events.py` (408 líneas) contiene el modelo, el estado global del tiempo y toda la lógica de simulación. El propio estudiante lo reconoce en el informe (report.md:221).
- **Idioma mezclado**: nombres en inglés (`airplanes_map`, `add_pilot`) y español (`marcar_avion_ocupado`, `gestion_pistas`) conviven; menor, pero conviene unificar.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Qué corrí y qué observé:**

1. **Menú + entrada inválida** — `printf 'abc\n\n13\n'`: el menú se dibuja, entrada no numérica se maneja con `"Debe ingresar un número."` sin reventar (ui/menu.py:76-79). ✅
2. **Crear evento válido** — tiempo 10.0, ID `EV01`, `ATERRIZAJE`, pista `P-01`, avión `AL2-01` (privado), piloto `PIL-01` (privado). El evento se persistió correctamente en `data/events.json` con `prioridad: 300`, y `avion.json`/`piloto.json`/`pista.json` quedaron con los recursos marcados `ocupado`/`ocupada`. ✅
3. **Simular siguiente evento (opt 8)** — avanzó el reloj de 0.00 a 10.00 y ejecutó el evento 10.00 → 12.00. ✅
4. **Simular por tiempo (opt 10, lógica headless)** — con dos eventos (t=5 dur=2, t=10 dur=3) hasta t=20: ejecutó `[5→7]` y `[10→13]` en orden de tiempo, tiempo global final 13.0. ✅
5. **Flujos inválidos**: pista inexistente → `"La pista no existe"` (✅); avión privado + piloto militar → `"Este piloto no puede volar este tipo de avión"` (✅ — la validación de compatibilidad de tipos funciona).
6. **Gestión de aviones** — agregar `TEST01` tipo COMERCIAL, verlo en la lista, guardar: ✅.
7. `py_compile` de los 12 módulos: **todos compilan** (rc=0).

**Bugs del estudiante encontrados (no del entorno):**

- **BUG 1 — Crash con `Traceback` en opción 3.** La primera entrada de "Agregar evento", `tiempo = float(input(...))` (ui/menu.py:98), **no** está envuelta en `try/except`, a diferencia de las opciones 1 y 2. Confirmado: `printf '3\nabc\n...'` produce `ValueError: could not convert string to float: 'abc'` y **termina el programa entero**. Lo mismo aplica a `duracion` (ui/menu.py:115,172). Es el defecto funcional más serio: una entrada no numérica mata la aplicación.
- **BUG 2 — Prioridad especial 1000 nunca se activa.** En ui/menu.py:152 se comprueba `if avion_id in ("AL2_01", "AL2_02")` (con guion **bajo**), pero los IDs reales en `data/avion.json` usan guion **medio** (`AL2-01`). Verificado: al crear un evento con `AL2-01`, la prioridad resultó **300** (privado), no 1000. Esto contradice directamente lo que afirma el informe (report.md:203).
- **BUG 3 (latente) — `Airstrip.from_dict` está roto.** `add_airstrip` guarda la clave `"ID"` en mayúsculas (airstrip.py:64), pero la dataclass tiene el campo `id`. `Airstrip.from_dict(d)` hace `Airstrip(**d)` y lanza `TypeError: got an unexpected keyword argument 'ID'` (verificado). No revienta en el uso normal porque la consola nunca reconstruye objetos `Airstrip` (accede al dict directamente vía `pista_ocupada`, etc.), pero el modelo queda inconsistente y cualquier código que use `from_dict` para pistas fallará.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Aciertos: dataclasses, type hints razonables (`Optional[str]`, `Dict[str, dict]`), separación en módulos, uso de `os.path.join` y `encoding="utf-8"` en la persistencia.

Mejorables (menores para 1er año):

- Las funciones `error1/error2/error3` (time.py) **imprimen** y devuelven `None`; luego `add_airplane` hace `return error2()` esperando un mensaje, pero devuelve `None`. Confuso: el mensaje se imprime, no se retorna. Conviene que las funciones de error **retornen** el string.
- Estado global mutable (`tiempo_actual_global`, los `*_map`) con `global` en muchas funciones — funciona, pero acopla todo. Para un primer proyecto es aceptable; a futuro, encapsular en una clase gestora.
- `mostrar_tiempo_actual` (events.py:352-358) interpreta la unidad de tiempo como minutos para el formato `HH:MM:SS`, pero el cálculo de segundos `(tiempo*60)%60` siempre da 0 para unidades enteras o medias — cosmético.
- `except ValueError as e: print(f"❌ Error: {e}")` (ui/menu.py:85) captura, pero muestra el mensaje crudo de Python al usuario; mejor un mensaje propio.

## Dimensión 5 — Datos y persistencia

Modelo sólido para el nivel: cada entidad se serializa como dict plano a un JSON en `data/`. `load_*_json` limpia el mapa in-place y lo repuebla; `save_*_json` vuelca el mapa completo. Verifiqué que los cambios persisten entre ejecuciones (el evento creado seguía en `events.json` tras salir). Los `data/*.json` son legibles y editables a mano.

Punto débil ya señalado (BUG 3): `pista.json` almacena la clave `"ID"` en vez de `"id"`, rompiendo el contrato con la dataclass `Airstrip`. Las otras tres entidades sí usan `"id"` en minúscula de forma consistente.

## Dimensión 6 — Informe (`report.md`)

El informe existe (el estudiante lo añadió tras la verificación automática, que no lo había encontrado), está bien escrito, es honesto en tono y **describe con precisión la arquitectura**. Tiene 1961 palabras (justo por debajo del umbral orientativo de 2000). Coincide con el código en lo esencial: estructura de paquetes, dataclasses, mapas en memoria, persistencia JSON, funciones de simulación.

Discrepancias detectadas:

- **Datos de ejemplo (report.md:198-201) NO coinciden con el repo.** El informe afirma que vienen aviones AL2-02/AL2-03, piloto PIL-12, pista P-19 y "dos eventos de despegue programados para tiempo 56.0". Los JSON reales solo traen 2 aviones (AL2-01, NOEL06), 2 pilotos (PIL-01, PIL-02), 2 pistas (P-01, A-01) y `events.json` está **vacío** (`{}`). Parece describir un estado de datos anterior.
- **Prioridad 1000 (report.md:203)**: dice que "AL2-01 y AL2-02 tienen prioridad especial 1000 hardcodeada". En la práctica **no ocurre** por el bug del guion bajo (BUG 2); AL2-01 recibió prioridad 300.
- Menor: el snippet de la dataclass `Airstrip` en el informe (report.md:83) muestra el campo `id`, correcto, pero el JSON serializado usa `ID` — el informe no refleja esa inconsistencia real.

El informe no exagera capacidades del programa (no dice "demuestra" ni "prueba" cosas que no hace); las discrepancias parecen desactualización, no inflado deliberado.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso** para un primer año, y explícitamente "el primero de toda mi vida". La arquitectura modular es limpia, el uso de dataclasses + JSON es idiomático, la mayoría de los 13 flujos de consola funcionan de verdad (creación de eventos, simulación, gestión de recursos, validación de compatibilidad piloto↔avión, persistencia). Que además incluya una GUI Tkinter alternativa muestra iniciativa por encima de lo pedido. Los defectos son reales pero acotados: un crash por `input()` sin proteger en la opción 3, una prioridad especial que no se activa por un typo guion-bajo/guion-medio, un `from_dict` de pistas roto por una clave mayúscula, y código muerto en `core/manager.py`. Ninguno hunde el proyecto; todos son corregibles en pocas líneas.

- **Principal fortaleza:** organización modular clara y consistente (paquetes `models`/`ui`/`utils`/`data`), con dataclasses y persistencia JSON bien aplicadas — el proyecto se ejecuta y hace lo que promete en los flujos principales.
- **Principal área de mejora:** robustez de las entradas: envolver **todos** los `float(input())` en `try/except` (la opción 3 revienta con basura), y corregir los tres bugs concretos (typo `AL2_01`, clave `ID`→`id` en pistas, borrar `core/manager.py`).
