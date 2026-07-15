# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #310
- **Repositorio:** https://github.com/mmaacr1012-coder/-Proyecto-I-Centro_Recreativo
- **Estudiante:** Marco Antonio Camacho Ramos
- **Grupo:** C122
- **Descripción declarada:** Un gestor de eventos para un Centro Recreativo, permite además la creación de recursos para ser utilizados en dichos eventos.

---

## Nota metodológica importante

No es una app de consola: es una aplicación web con **Streamlit** (multipágina). No tiene `input()` ni menú por consola. Para evaluarla ejecuté dos cosas:

1. **Arranque real del servidor**: `streamlit run Home.py --server.headless true`. El servidor levantó limpio (sin `Traceback`) y respondió `ok` en `/_stcore/health`, confirmando que la app se sirve correctamente.
2. **Lógica de negocio aislada**: todas las validaciones viven en `page_core.py` y dependen de `st.session_state`. Construí un arnés que simula `st.session_state` con el `data.json` real del repo e invoca directamente las funciones (`valid_date`, `resource_restriction_valid`, `user_resource_valid`, `resource_worktime_valid`, `resource_copies_valid`, `coinciden`) con casos válidos e inválidos. Esto permite verificar la corrección funcional sin depender del navegador.

Entorno: `uv venv --python 3.12` + `streamlit==1.51.0`. `py_compile` de los 7 módulos: **todos OK** en 3.12. Advertencia importante: el código usa f-strings con comillas dobles anidadas (p.ej. `page_core.py:60` `f"...{y["open"][:5]}..."`), sintaxis que **solo es válida a partir de Python 3.12**. El repo declara `requires-python = ">=3.13"` en `pyproject.toml` y `.python-version = 3.13`, así que en la máquina objetivo compila; pero en cualquier intérprete ≤3.11 fallaría al importar.

## Dimensión 1 — Qué hace el programa

Gestor de eventos de un centro recreativo con tres páginas (`pages/`):

- **Home** (`Home.py`): carga `data.json` a `st.session_state` una sola vez, guarda la lista de eventos en un `backup` y vacía `eventos` para arrancar en limpio (`Home.py:6-8`). Presenta la introducción.
- **Agregar Evento** (`pages/1_Agregar Evento.py`): flujo secuencial — nombre no vacío (`:28`), fecha inicio/fin con `st.date_input`/`st.time_input`, validación de fecha (`:47`), selección múltiple de recursos con `st.pills` (`:50`), y **cuatro capas de validación** encadenadas (`:53-56`): restricciones fijas, restricciones de usuario, horarios de trabajo y disponibilidad de copias. Solo si todas pasan aparece el botón "Guardar" (`:67`), que serializa el evento a `data.json`.
- **Lista de Eventos** (`pages/2_Lista de Eventos.py`): muestra los eventos en un `st.dataframe` con columnas formateadas (`:22-40`), o un aviso si está vacío.
- **Recursos** (`pages/3_Recursos.py`): tabla de recursos + diálogo modal `Add_resource` (`:10`) para crear recursos con nombre, horario, cantidad y listas de exclusión/inclusión, persistidos en `data.json`.

Verificado en ejecución: el servidor arranca y sirve; las validaciones funcionan (ver Dimensión 3).

## Dimensión 2 — Organización del código

**Fortaleza destacable para 1er año**: separación real entre lógica y presentación. Toda la validación vive en `page_core.py` como funciones puras (o casi-puras) reutilizadas por las páginas. Las páginas importan lo que necesitan (`pages/1_Agregar Evento.py:4`). La estructura multipágina de Streamlit se usa idiomáticamente (`pages/` numerado).

**Debilidades:**
- **Bloque de inicialización duplicado** en cada página (`Home.py:3-8`, `pages/1_Agregar Evento.py:6-11`, `pages/2_Lista de Eventos.py:6-11`, `pages/3_Recursos.py:6-8`): el patrón de abrir `data.json` + poblar `session_state` está copiado 4 veces. Debería ser una función única en `page_core.py`.
- **Reglas de negocio hardcodeadas** en `resource_restriction_valid` (`page_core.py:22-51`): las incompatibilidades entre recursos "propios" están escritas como una cadena de `if/elif` con nombres literales, mientras que las de recursos de usuario sí son data-driven (`user_resource_valid`, `:8-21`). Conviven dos sistemas para lo mismo. Es una decisión consciente (el informe la menciona), pero frágil.
- Nombres mezclados español/inglés (`crash`, `lista`, `Starts`/`Ends` vs `recursos`/`eventos`).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí las funciones con el `data.json` del repo. Resultados observados:

1. **`valid_date`** (`page_core.py:5-7`): `start<end → True`, `start==end → False`, `start>end → False`. Correcto.
2. **`resource_restriction_valid`**: verificado con 9 combinaciones. Correctos: `Piloto+Arbitro → False` ("se llevan mal"), `Avion` solo → False ("El piloto debe manejar el Avion"), `Avion+Piloto+Paracaidas → True`, `Patineta` sin protectores → False, `Patineta+Protectores → True`, `Centro de Computacion` sin Informatico → False, `Arbitro` solo → False, `Informatico+Pelota de futbol → False`, lista vacía → True.
3. **`resource_worktime_valid`** (`:53-63`): `Avion` (10:00–15:00) pedido 09:00–11:00 → False con mensaje correcto; 10:30–14:00 → True; `Arbitro` 07:00–09:00 → False con la rama "solo trabaja de..." bien seleccionada. Correcto.
4. **`user_resource_valid`** (`:8-21`): exclusiones/inclusiones desde `data.json` funcionan — `Trampolin+Avion → False` (excluidos), `Trampolin` sin Protectores → False (inclusión faltante), `Trampolin+Protectores → True`. Correcto.
5. **`coinciden`** (`:64-71`) + **`resource_copies_valid`** (`:76-98`): con `Avion` (amount 1) y un evento que lo ocupa en la ventana, la función detecta la saturación y devuelve False con sugerencia de fecha disponible. La lógica recursiva de "buscar el próximo hueco" **funciona** end-to-end.

### Bugs encontrados en ejecución

- **BUG — typo de dato que desactiva silenciosamente una regla** (`page_core.py:27`): la condición usa `"Pelota de tennis"` (dos n), pero `data.json` define el recurso como `"Pelota de tenis"` (una n). Verifiqué: `resource_restriction_valid(["Protectores","Pelota de tenis"])` devuelve **`(True,)`** — es decir, la regla "no puedes usar pelotas con Protectores" **nunca se dispara para la pelota de tenis**. En cambio `["Protectores","Pelota de futbol"]` sí devuelve False. Peor aún, otras líneas del mismo archivo (`:33`, `:39`) sí escriben `"Pelota de tenis"` (una n), así que el código convive con las dos ortografías. Bug real de corrección, fácil de arreglar.
- **BUG menor — off-by-one en la sugerencia de fecha** (`page_core.py:86-91`): con `Avion` reservado *solo* el 07-20, al pedirlo ese día la app sugiere "disponible desde el **2026-07-22**", cuando el primer día realmente libre es el **07-21**. La causa: `:87` fija `newsd = sd + 1 día` y luego `:89` suma otro día *antes* de comprobar, saltándose el 07-21. La sugerencia es orientativa, pero desorienta.
- **Observación de robustez** (`:59-61`): los mensajes usan comparación por igualdad de nombre literal (`x == "Arbitro"`) para decidir "trabaja" vs "se puede usar". Si el usuario crea un recurso llamado, digamos, "Guardia", nunca entrará en la rama humana. No es incorrecto, es un detalle de generalización.

Ningún flujo probado produjo `Traceback`. La app es robusta ante los casos que probé.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- `valid_date` (`:5-7`) puede ser `return sd < ed` en una línea; el `if/return True` es redundante.
- Uso de `st.session_state` con `try/except AttributeError` implícito está bien manejado, pero el patrón `if "x" not in st.session_state` repetido en cada archivo pide una función de inicialización.
- `open` como nombre de variable (`pages/3_Recursos.py:16`) **sombrea el builtin `open`**. Justo en ese archivo se usa `open("data.json")` arriba (`:6`), así que funciona por orden, pero es una trampa esperando a activarse. Renombrar a `apertura`.
- `type(amount) == int` (`pages/3_Recursos.py:28`): idiomático sería `isinstance(amount, int)`.
- Comparación de fechas serializando a ISO y re-parseando en `coinciden`/`resource_copies_valid`: se podrían guardar objetos, pero para persistir a JSON el ISO es razonable.
- Manejo de errores: no hay `try/except` alrededor de la lectura de `data.json`; si el archivo faltara o estuviera corrupto, la app reventaría al arrancar. Para el alcance del proyecto es aceptable.

## Dimensión 5 — Datos y persistencia

Modelo simple y coherente en `data.json`: `recursos` (lista de dicts con `name/open/close/amount`), `user_restrictions` (`exclusions`/`inclusions` como dicts nombre→lista), y `eventos` (lista de dicts con `Name/Starts/Ends/Resources`). Serialización con `json.dump(..., indent=4)` (`page_core.py:104-105`).

**Punto de diseño discutible pero honestamente documentado**: al arrancar, cada página vacía `eventos` y guarda el original en `backup` (`Home.py:6-8`). Si guardas un evento sin haber pulsado antes "Cargar eventos", `save_event` **sobrescribe `data.json` con solo ese evento**, perdiendo los previos. El informe lo advierte explícitamente ("al guardar un evento sobreescribirá la base de datos"). Es un comportamiento sorprendente para un gestor, pero está declarado, no oculto. Los recursos sí se cargan siempre (`pages/3_Recursos.py:6-8` no vacía `recursos`), lo cual es la elección correcta.

## Dimensión 6 — Informe (`report.md`)

652 palabras (por debajo del mínimo de 2000 que marca la verificación automática). Aun corto, es **honesto y preciso**: describe el flujo real, distingue recursos propios (reglas hardcodeadas) de recursos de usuario (reglas en datos) — lo cual coincide con `resource_restriction_valid` vs `user_resource_valid`. No exagera ni afirma "demostrado/probado" sobre validaciones que no ejecutó. Reconoce abiertamente el comportamiento de sobrescritura de la "base de datos", que es justo el punto más delicado del diseño.

Discrepancias/omisiones: el informe no menciona el bug del typo `"Pelota de tennis"` (esperable, no lo detectó), ni la lógica recursiva de sugerencia de fechas disponibles (que sí implementó y es de lo más ambicioso del proyecto — merecía mención). El `pyproject.toml` referencia un `README.md` que no existe en el repo.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido** y notablemente ambicioso para primer año. La elección de Streamlit está bien ejecutada, la separación lógica/presentación es real, y el sistema de validación en cuatro capas (restricciones fijas, restricciones de usuario data-driven, horarios y disponibilidad de copias con búsqueda recursiva del próximo hueco) demuestra un dominio que va más allá de lo esperado. La app arranca sin errores y todas las validaciones que probé se comportan correctamente, salvo dos bugs concretos: un typo de datos (`"Pelota de tennis"` vs `"Pelota de tenis"`) que desactiva silenciosamente una regla, y un off-by-one en la sugerencia de fecha. El informe es corto pero honesto y bien alineado con el código.

- **Principal fortaleza:** el motor de validación en `page_core.py` — reglas encadenadas + búsqueda recursiva del próximo horario disponible, todo funcionando en ejecución real. Ambición y ejecución por encima del nivel de primer año.
- **Principal área de mejora:** el bug del typo `"Pelota de tennis"` (`page_core.py:27`), porque una regla de negocio nunca se cumple sin que nada lo avise; y de fondo, eliminar la duplicación del bloque de inicialización de `session_state` en las 4 páginas, extrayéndolo a una función.
