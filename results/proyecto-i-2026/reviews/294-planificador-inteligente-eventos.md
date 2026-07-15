# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #294
- **Repositorio:** https://github.com/Muzkatanke/planificador-inteligente-eventos.git
- **Estudiante:** Antonio Milian Corrales
- **Grupo:** C122
- **Descripción declarada:** Gestor de eventos basado en una empresa de servicios multifuncional (ACUSE.S.A.). Aplicación de consola en Python que planifica eventos con recursos limitados, reglas de inclusión/exclusión entre recursos, detección de solapamientos y reprogramación automática.

---

## Nota metodológica importante

Es una **app de consola real** con `input()`, punto de entrada `Main.py`. No tiene dependencias externas: usa solo la biblioteca estándar (`os`, `json`, `datetime`, `pathlib`). Se ejecutó de verdad alimentando el menú con `printf`. Un detalle de entorno: usa `os.system("cls")` (comando de Windows) en cada pantalla; en Linux imprime `sh: 1: cls: not found` pero **no interrumpe** el flujo — es solo un problema de portabilidad, no un bug funcional. Además, para aislar la lógica de negocio (reprogramación, activación) se ejecutaron directamente las funciones de `logic/Evento.py` y `logic/Planificador.py` con datos construidos a mano.

## Dimensión 1 — Qué hace el programa

Menú principal (`Main.py:5-38`) con cuatro opciones: agregar, eliminar, ver, salir. En cada iteración del bucle, antes de mostrar el menú, se llaman `activate_events` y `desactivate_events` (`Main.py:15-16`) que recorren los eventos guardados y cambian su estado `activated` según la hora actual (`logic/Planificador.py:4-22`).

El flujo estrella es **agregar evento** (`logic/Manejo_Eventos.py:16-184`):
1. Elige uno de 6 tipos de evento predefinidos (`logic/Evento.py:16-21`).
2. Pide fecha de inicio y fin en formato `dd/mm/aaaa hh:mm`, validando formato, que fin > inicio y que no sea en el pasado (`Manejo_Eventos.py:42-52`).
3. Asignación interactiva de recursos con validación en vivo: por cada recurso muestra la disponibilidad **en el intervalo del evento** (`Manejo_Eventos.py:66-72`), marca `[EXCLUIDO]` los recursos incompatibles con la selección actual, y `[NO DISPONIBLE]` los agotados.
4. Aplica reglas de **inclusión obligatoria** (ej. Electricista → incluye automáticamente Accesorios de electricidad) y **exclusión** (ej. Electricista excluye Plomero) definidas en `logic/Datos.py:17-31`.
5. Si un recurso está copado en el intervalo, ofrece **reprogramar automáticamente** buscando el primer hueco libre tras los eventos solapados (`logic/Evento.py:32-67`).
6. Persiste en `storage/eventos.json` (`logic/Persistencia.py`).

**Eliminar** (`Manejo_Eventos.py:186-208`) y **ver** (`Manejo_Eventos.py:210-221`) funcionan sobre la lista cargada.

## Dimensión 2 — Organización del código

Muy buena para primer año. El proyecto está **modularizado por responsabilidad** en un paquete `logic/`:
- `Datos.py` — configuración (recursos y reglas), separada del código.
- `Evento.py` — clase `Event` + utilidades de fecha y planificación.
- `Recursos.py` — motor de reglas inclusión/exclusión.
- `Planificador.py` — máquina de estados activado/finalizado.
- `Manejo_Eventos.py` — casos de uso (agregar/eliminar/ver).
- `Persistencia.py` — serialización JSON.
- `Main.py` — solo interfaz de menú.

Esta separación entre datos, lógica y presentación es exactamente lo que se busca enseñar. Nombres claros y en su mayoría descriptivos.

Debilidades menores:
- `logic/Recursos.py:1-3` importa `resources, rules` **dos veces** (línea duplicada) — inofensivo pero descuidado.
- `event_actives = load_events()` es una variable **de módulo** en `Manejo_Eventos.py:8` que se muta como estado global compartido; funciona, pero acopla toda la lógica a ese singleton. Una clase `EventManager` sería el siguiente paso.
- `check_resources_inclusion` (`Recursos.py:5`) recibe `alert` y `available_amount_interval` como parámetros para evitar imports circulares — solución pragmática, aunque revela que la frontera entre módulos aún no está del todo limpia.
- `available_amount` (`Evento.py:70-79`) quedó definida pero no se usa en el flujo real (se usa siempre la variante `_interval`).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

1. **Salir** (`printf '4\n'`): imprime el menú y sale limpio. OK.
2. **Agregar evento completo** (evento 1, fechas `20/12/2026 10:00`→`12:00`, Electricista×1): creado correctamente. La regla de inclusión **auto-agregó** "Accesorios de electricidad" y la de exclusión marcó Plomero/Accesorios de fontanería como `[EXCLUIDO]`. Resumen final:
   `ID: 1 / Nombre: Instalación eléctrica en local / Recursos: {'Electricista': 1, 'Accesorios de electricidad': 1}`. Se guardó bien en `eventos.json`.
3. **Persistencia entre corridas**: cerré y reabrí el programa; **Ver eventos** mostró el evento guardado con sus fechas y recursos. El contador de IDs se restaura desde el máximo guardado (`Persistencia.py:47`). OK.
4. **Agregar → Eliminar → Ver**: eliminé el evento por índice; `eventos.json` volvió a `[]` y "Ver" reportó "No hay eventos activos que ver". OK.
5. **Menú inválido** (`abc`, `99`): ambos → "Introduce un número válido!!", sin crash. OK.
6. **Fecha en el pasado** (`01/01/2020`): rechazada con "No se pueden programar eventos en el pasado." OK.
7. **Formato de fecha inválido** (`notadate`, `32/13/2026 99:99`): re-pregunta con "Formato inválido...". OK.
8. **Lógica de reprogramación** (probada directamente): con el recurso copado en el intervalo, `find_available_start` desplaza el evento al primer hueco tras el último evento solapado (ej. `12:00→13:00`); si la demanda supera la capacidad total del recurso, lanza `ValueError` con mensaje claro. Comportamiento correcto en ambos casos.
9. **Máquina de estados** (probada directamente): un evento en curso pasó a `activated=True`; uno vencido pasó a `activated=False`. OK.

**Bug real encontrado — el archivo de ejemplo rompe la carga:**
`storage/eventos_ejemplo.json` guarda fechas en formato ISO `"2026-02-10 14:00"`, pero `dict_to_event` (`Persistencia.py:26`) las parsea con `"%d/%m/%Y %H:%M"`. Copié el ejemplo a `eventos.json` y el programa **revienta al arrancar** con un `Traceback` no capturado:
`ValueError: time data '2026-02-10 14:00' does not match format '%d/%m/%Y %H:%M'`.
Además, ese mismo archivo usa el recurso `"Operario"`, que **no existe** en `Datos.py` (los recursos válidos son "Obrero"/"Operador de camión de carga"). El ejemplo que el propio informe presenta como referencia del formato es, por tanto, inconsistente con el formato que el código realmente escribe y lee. El `eventos.json` generado por la propia app sí es coherente; el problema está solo en el archivo de ejemplo.

**Observaciones menores (no bugs de la corrida normal):**
- `activate_events` (`Planificador.py:11-12`) compara la demanda contra el **total global** del recurso, no contra lo ya consumido por otros eventos activos; y usa `return` (aborta activar el resto de eventos) donde correspondería `continue`. En la práctica no se dispara porque la validación al crear ya impide sobre-asignación en cualquier intervalo, pero la lógica de esa comprobación es frágil.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Aciertos:
- `try/except ValueError` consistente alrededor de cada `int(input(...))` — el programa no se cae por entradas basura del usuario.
- Uso idiomático de comprensiones (`{key: 0 for key in resources}`), `max(...)` con generador, `datetime.strptime`.
- Bucles de validación que re-preguntan hasta obtener entrada válida.

Mejorables:
- `os.system("cls")` no es portable; `print("\n"*50)` o dejar de limpiar sería más limpio y multiplataforma.
- Usar `dict` como nombre de variable (`Persistencia.py:42`) **sombrea el built-in** `dict`; renombrar a `d` o `raw`.
- No hay `try/except` alrededor de `load_events()` (`Manejo_Eventos.py:8`): cualquier JSON malformado o inconsistente tumba el arranque con un traceback crudo (justo lo que ocurre con el ejemplo).
- El estado global mutable (`event_actives`) es cómodo pero dificulta razonar sobre el flujo.

## Dimensión 5 — Datos y persistencia

Modelo simple y adecuado: clase `Event` con `id/name/start/end/resources/activated` (`Evento.py:3-13`), contador de ID a nivel de clase auto-incremental que se re-sincroniza al cargar (`Persistencia.py:44-47`) — detalle bien pensado para no duplicar IDs entre sesiones. Serialización JSON legible (`indent=4`, `ensure_ascii` implícito produce `\u...` en los nombres, cosmético). Los recursos se modelan como `dict` recurso→cantidad, coherente con el catálogo de `Datos.py`. La única falla es la **incoherencia de formato de fecha** en el archivo de ejemplo descrita arriba.

## Dimensión 6 — Informe (`report.md`)

Buen informe: explica dominio, eventos, recursos, reglas, persistencia y estructura del código, y **coincide bien con lo implementado**. La tabla de recursos y las reglas descritas concuerdan con `Datos.py`. La descripción del flujo de agregar/reprogramar es fiel a lo que ejecuté.

Discrepancias a corregir:
- El informe presenta `eventos_ejemplo.json` (`report.md:146-157`) como "referencia para comprender el formato", pero ese archivo **no carga** con el código actual por el desajuste de formato de fecha y el recurso inexistente "Operario". El informe afirma implícitamente algo que no se sostiene al ejecutarlo.
- `report.md:88` dice "los recursos se liberan cuando finaliza o se elimina un evento activo". En el código, eliminar solo hace `pop` de la lista (`Manejo_Eventos.py:206`); no hay un descuento/reposición explícito de recursos porque el modelo calcula disponibilidad **por intervalo** sobre la marcha — así que la afirmación es correcta en efecto, pero no hay un paso de "liberación" como el texto sugiere. Detalle menor.
- Requisitos: dice "Python 3.10 o superior"; se probó en 3.12 sin problemas. OK.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso** para primer año. Va bastante más allá del CRUD típico: implementa un motor de reglas de inclusión/exclusión entre recursos, disponibilidad calculada por intervalo de tiempo, detección de solapamientos y **reprogramación automática** al primer hueco libre — y todo eso funciona de verdad al ejecutarlo. La arquitectura está limpiamente modularizada (datos / lógica / persistencia / presentación separadas), el manejo de entradas inválidas es robusto, y la persistencia con re-sincronización de IDs está bien resuelta. El informe es claro y en general fiel al código.

El defecto más concreto es que el **archivo de ejemplo `eventos_ejemplo.json` está roto** respecto al código (formato de fecha ISO en vez de `dd/mm/aaaa`, y recurso "Operario" inexistente): copiarlo a `eventos.json` tumba el arranque con un traceback. Se arregla en dos minutos regenerando el ejemplo desde la propia app o corrigiendo las tres fechas y el nombre del recurso, y conviene envolver `load_events()` en un `try/except` para degradar con gracia ante JSON inconsistente.

- **Principal fortaleza:** la lógica de negocio (reglas de recursos + reprogramación automática por intervalos), correcta y verificada al ejecutar, con una modularización muy por encima del promedio de primer año.
- **Principal área de mejora:** coherencia entre el archivo de ejemplo y el formato real, más robustez en la carga (`try/except` en `load_events`) para que un JSON malo no tumbe el programa.
