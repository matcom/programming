# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #274
- **Repositorio:** https://github.com/itadrias/SuperHero_Manager
- **Estudiante:** Adrian Pacheco Rubio
- **Grupo:** C122
- **Descripción declarada:** Proyecto de gestión de eventos personalizados y predeterminados con temática de superhéroes.

---

## Nota metodológica importante

Este proyecto **no es una aplicación de consola**: es una GUI completa construida con **Kivy 2.3.1**, con 14 módulos Python (~2050 líneas) más un archivo de layout declarativo `main.kv` (202 líneas), 5 archivos JSON de datos, e imágenes/sonidos/fuentes propias. El punto de entrada `main.py` instancia la `App` de Kivy y arranca el bucle de eventos; toda la interacción es por mouse (clic izquierdo/derecho, hover). Alimentar `main.py` con `printf` no tiene sentido aquí.

Adapté la ejecución en tres capas:

1. **Lógica de negocio aislada** (`restrictions.py`, `checker.py`, `utils.py`): no dependen de Kivy, así que las importé y ejecuté directamente con datos reales del repo, probando flujos válidos e inválidos y casos borde.
2. **Arranque GUI headless**: primero con `SDL_VIDEODRIVER=dummy` (falló por falta de contexto OpenGL — **fallo del entorno, no del código**), y luego bajo **Xvfb** (`xvfb-run -s "-screen 0 1280x720x24"`), donde la app **arrancó limpiamente, llegó a `Start application main loop` y corrió sin ningún `Traceback` hasta que el timeout la cerró**.
3. **Screenshot de verificación**: capturé la ventana renderizada tras 3 s (menú principal *Patrullar / Luchar / Añadir / Ver Eventos* sobre el fondo de superhéroes) — evidencia visual de que la interfaz se construye y dibuja correctamente.

`py_compile` de los 14 módulos: **todos compilan sin error**.

## Dimensión 1 — Qué hace el programa

Es un planificador de misiones de una agencia de superhéroes con persistencia real. El flujo, verificado por lectura + ejecución:

- El menú principal (`buttons.py:20` `Main_Container`, opciones cargadas desde `json/options.json` en `buttons.py:72-74`) ofrece cuatro ramas: **Patrullar** (`id=1`), **Luchar** (`id=2`), **Añadir** evento personalizado (`id=3`) y **Ver Eventos** (`id=4`) — despacho en `buttons.py:280-346`.
- Al elegir una misión, se abre un **selector de héroes/ítems** (`hero_selector.py:183` `selection_matrix`, 12 héroes ids 17–28 en `hero_selector.py:161-163`, 12 ítems ids 29–40 en `hero_selector.py:179-181`) con un **gráfico de radar hexagonal** (`chart.py:46` `Chart`) que suma los atributos del equipo en tiempo real (`hero_selector.py:97-119`).
- Al "Aceptar" (`buttons.py:622-643`), se valida el equipo contra las **restricciones** (`restrictions.py:3` `check_restrictions`) y luego se abre el **selector de fechas** (`date_selector.py:96` `date_selector`), que comprueba **solapamientos de agenda** (`checker.py:20` `check_overlapping`) antes de persistir (`checker.py:109` `create_event` → `json/events.json`).
- **Ver Eventos** (`events_view.py:51`) lista lo persistido con detalle y permite eliminar (`events_view.py:250` → `checker.py:128` `delete_event`), con reordenamiento cronológico automático (`checker.py:87` `sort_events`).

## Dimensión 2 — Organización del código

**Fortaleza destacada.** La separación en `widgets/` es real y bien pensada para un primer año:

- **Lógica de negocio pura, sin Kivy**: `restrictions.py` (motor de reglas), `checker.py` (agenda/solapamiento) y `utils.py` (E/S JSON + búsqueda de widgets) no importan Kivy. Esto es lo que me permitió probarlos aislados — señal clara de buena separación de responsabilidades.
- **Datos fuera del código**: héroes, ítems, misiones, restricciones y parámetros viven en `json/` (`options.json`, `restrictions.json`, `events_parameters.json`, `info_eventos.json`), no *hardcodeados*. Añadir una restricción es editar un JSON, no tocar Python.
- **Layout declarativo** en `main.kv` separado de la lógica de los widgets.
- Docstrings consistentes y en español en casi todas las clases y funciones.

**Debilidades (menores para el nivel):**

- **Estado global mutable a nivel de módulo**: `sum` y `used` en `hero_selector.py:16-17`, y `record` en `buttons.py:18`, son variables globales compartidas. Funciona, pero acopla widgets de forma frágil (un `selection_matrix(clean=True)` en `hero_selector.py:196-199` tiene que acordarse de resetearlas manualmente). El informe lo llama "single source of truth"; en la práctica es un global mutable, que es lo contrario de encapsular.
- **Método `remove` duplicado** en `selection_matrix` (`hero_selector.py:236` y `hero_selector.py:247`): la segunda definición **sombrea** a la primera, dejando la primera (que además parece recursiva/errónea) como código muerto. No rompe nada porque la que gana es la correcta, pero delata un descuido.
- Nombres de clases en `snake_case` (`change_button`, `hero_button`, `date_box`) en vez de `CamelCase` — inconsistente con `Main_Container`, `Chart`, `InfoPanel`. Convención, no corrección.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté la lógica de negocio con datos reales del repo. Resultados concretos:

1. **`check_restrictions` — sin héroe**: `check_restrictions([29], [0]*6)` → `(False, "…Necesitas al menos 1 héroe…")`. ✅ Correcto.
2. **`check_restrictions` — misión con héroe requerido ausente**: misión 5 exige a Juggernaut (id 26); `check_restrictions([5,17,29], …)` → `(False, "…Juggernaut es indispensable.")`. ✅ Lee `restrictions.json` correctamente.
3. **`check_restrictions` — ítem requerido por un héroe**: `check_restrictions([5,26,29], …)` → `(False, "El inmenso metabolismo de Juggernaut exige la Entrega de Suministros…")`. ✅ La dependencia héroe→ítem funciona.
4. **`check_restrictions` — equipo válido**: misión 3 (requisitos 0) + héroe + ítem → `(True, "")`. ✅
5. **`check_overlapping` — conflicto de héroe**: creé evento con héroe 17 (10:00–12:00); un segundo intento con héroe 17 (10:30–11:30) → devuelve `2026-08-01 12:00` (fecha de liberación). ✅ Detecta el cruce.
6. **`check_overlapping` — sin conflicto**: héroe 18 distinto, fuera de rango → `None`. ✅
7. **`next_available`**: para el héroe 17 ocupado devolvió `(2026-08-01 12:00, 13:00)` — el hueco justo tras la liberación. ✅ La recursión de `checker.py:69-85` converge.
8. **Recursos limitados** (`checker.py:cant`): ítem 40 (capacidad 1) reutilizado en solape → conflicto; ítem 35 (capacidad 10) → sin conflicto. ✅ El conteo de disponibilidad de ítems funciona.
9. **CRUD + re-indexado**: creé dos eventos (keys `0`,`1`), borré la key `0` → el restante se re-indexa a `0` vía `sort_events`. ✅ Consistencia de IDs mantenida.
10. **GUI headless bajo Xvfb**: arranca hasta el main loop, **cero Traceback**, screenshot renderizado del menú principal. ✅ La construcción del árbol de widgets es correcta.

**Fallos del entorno (no del código):** con `SDL_VIDEODRIVER=dummy` no hay contexto OpenGL → `RuntimeError: OpenGL support … not available in current SDL video driver (dummy)`. Es limitación del entorno headless sin GPU/GL; bajo Xvfb desaparece.

**Bugs reales del estudiante (menores):**

- **`delete_event(id)` sin key existente lanza `KeyError`** (`checker.py:135`): `delete_event(99)` → excepción no controlada. En la práctica la key siempre viene de la UI (`events_view.py:155` pasa el índice de enumeración), así que no es alcanzable con uso normal, pero es una falta de robustez.
- **`SoundManager.play_sound` no protege contra archivo faltante** (`sound_manager.py:19-21`): si `SoundLoader.load` devuelve `None` (ruta mala), la siguiente línea `self.sound.play()` reventaría con `AttributeError`. No se disparó en mi corrida porque los assets existen, pero es un `Traceback` latente.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- ✅ Uso correcto de `with open(...)` para E/S de JSON (`utils.py:53`, `utils.py:63`), con `encoding='utf-8'`.
- ✅ `try/except` en `restrictions.py:14-17` para el `min(ids)` sin key.
- ⚠️ **`from … import *`** en casi todos los módulos (`buttons.py:8-16`, `hero_selector.py:10-14`, etc.). Funciona pero contamina el namespace y hace difícil rastrear de dónde viene cada nombre. Preferible importar explícito.
- ⚠️ Sombra de builtins: la variable global `sum` (`hero_selector.py:16`) **oculta el builtin `sum()`** — y de hecho en `chart.py:159` se usa `sum(...)` como builtin en otro módulo, lo que funciona solo porque el shadowing es local a `hero_selector`. Confuso; renombrar a `attr_totals` sería más claro.
- ⚠️ `except:` desnudos (`hero_selector.py:40`, `buttons.py:392`, `restrictions.py:16`) capturan cualquier cosa, incluidos errores que uno querría ver. Preferible `except (KeyError, TypeError):`.
- Detalle de estilo: `str(start)[:len(str(start))-3:]` en `checker.py:120` para recortar los segundos — funciona, pero `str(start)[:-3]` es idéntico y más legible.

## Dimensión 5 — Datos y persistencia

Modelo simple y coherente. `json/events.json` es un diccionario indexado por string (`"0"`, `"1"`, …) donde cada evento es `{start, end, id, resources}`. Las fechas se serializan como `"YYYY-MM-DD HH:MM"` (string) y se re-parsean con `datetime.strptime` al comparar — decisión razonable. Los ids codifican semántica por rangos (1–16 misiones, 17–28 héroes, 29–40 ítems), lo que evita colisiones y permite los chequeos `if 17 <= j <= 28`. `sort_events` (`checker.py:87`) re-escribe el JSON ordenado tras cada mutación para mantener la línea temporal consistente — verificado en ejecución. La separación datos/código es una fortaleza genuina.

Nota: la capacidad de los ítems (`checker.py:cant`, líneas 5–18 y 32) está **duplicada** — un diccionario a nivel de módulo y una copia literal dentro de `check_overlapping`. El del módulo (`cant`) nunca se usa; solo se usa `disp` interno. Redundancia inofensiva pero es código muerto.

## Dimensión 6 — Informe (`report.md`)

El informe (~2500 palabras) es extenso, bien estructurado y en general **coincide con el código** — describe correctamente `check_overlapping`, `next_available` (recursiva, confirmado), el motor de restricciones required/forbidden, el gráfico de radar y la persistencia JSON. Las tablas de sinergias/prohibiciones/requisitos son consistentes con `restrictions.json`. Mérito real: el estudiante entiende su propia arquitectura y la explica bien.

**Discrepancias / exageraciones a marcar:**

- **"encriptados en archivos `.json`" / "Base de Datos Encriptada"** (`report.md:188`, `report.md:195`): falso. Los JSON son texto plano legible; no hay cifrado de ningún tipo. Es lenguaje de marketing, no técnico.
- **"validación antes de persistir … en cadena"** (`report.md:50`): correcto y verificado — no se escribe hasta pasar atributos → restricciones → fecha. Aquí el informe **no** exagera; lo confirmé en ejecución.
- **"single source of truth"** (`report.md:212`): generoso. Es un global mutable compartido (`sum`/`used`), que técnicamente centraliza el estado pero sin la encapsulación que el término sugiere.
- El informe no menciona ninguna limitación conocida (p. ej. el `KeyError` de `delete_event` o la falta de guarda en `SoundManager`), pero para 1er año no se le exige un apartado de bugs conocidos.

En conjunto el informe **describe lo que el código hace de verdad**; las inexactitudes son de vocabulario ("encriptado"), no de features inventadas.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **notablemente ambicioso y sólido** para primer año. El estudiante fue mucho más allá de una app de consola: construyó una GUI completa en Kivy con animaciones, un gráfico de radar dibujado a mano con trigonometría (`chart.py`), audio, y — lo que más importa para la evaluación — **una capa de lógica de negocio real, separada de la interfaz, que ejecuté de forma aislada y pasó todos los flujos válidos e inválidos que le tiré**: restricciones required/forbidden, requisitos de atributos, detección de solapamiento de agenda con liberación de recursos, cálculo del próximo hueco disponible, y CRUD con re-indexado consistente. La GUI arranca sin Traceback bajo Xvfb y renderiza correctamente. Los defectos son de estilo y robustez de borde (globals mutables, `import *`, un `except:` desnudo, un `KeyError` no alcanzable en uso normal, un método duplicado), todos menores frente al alcance logrado.

- **Principal fortaleza:** separación real entre lógica de negocio (probada aislada y correcta) y presentación, sobre datos externalizados en JSON — una arquitectura que normalmente no se ve a este nivel, y que hizo el código verificable de verdad.
- **Principal área de mejora:** reemplazar el estado global mutable (`sum`, `used`, `record`) por atributos encapsulados en un objeto de sesión/estado, y endurecer las funciones de borde (`delete_event` con guarda de key, `SoundManager` con guarda de `None`); además, limpiar el código muerto (método `remove` duplicado, diccionario `cant` sin uso) e importar explícito en vez de `import *`.
