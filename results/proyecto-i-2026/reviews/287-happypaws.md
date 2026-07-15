# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #287
- **Repositorio:** https://github.com/tgnd254/HappyPaws
- **Estudiante:** Estefanía Delgado Marqués
- **Grupo:** C122
- **Descripción declarada:** Planificador de Eventos en un Refugio de Animales llamado HappyPaws.

---

## Nota metodológica importante

Este **no es** un programa de consola: es una aplicación **GUI** construida con **Kivy 2.3.1 + KivyMD 1.2.0** (`main.py:13`, `main.py:62` heredando de `MDApp`). El punto de entrada `main.py` monta un `ScreenManager` con seis pantallas y arranca una ventana fija de 800×600 (`main.py:5-8`, `main.py:69-83`). No hay `input()` en ninguna parte; toda la interacción es por clic sobre imágenes/botones y por los selectores `MDDatePicker`/`MDTimePicker`.

Adapté la ejecución en tres capas:

1. **`py_compile`** de los 8 módulos → todos compilan sin error.
2. **Lógica de negocio directa** (lo importante): importé `utils.py` y ejercité `resources_available` y `validate_resources` con los datos reales del repo (`data/resources.json`, `data/events.json`) y con eventos sintéticos, cubriendo flujos válidos, colisiones, agotamiento de cantidades y basura.
3. **GUI headless real** bajo `xvfb-run` (framebuffer virtual 800×600). Aquí encontré un obstáculo del **entorno**: la rueda de `kivymd==1.2.0` instalada por pip/uv **no incluye los archivos `.kv`** (bug conocido de empaquetado de esa versión). El primer arranque falla en `kivymd/uix/label/label.py:548` con `FileNotFoundError: label.kv`. Esto **no es un fallo del código de la estudiante** — su pin `kivymd==1.2.0` es razonable. Restauré los `.kv` faltantes desde el código fuente oficial de KivyMD y, con eso, **la aplicación completa arranca y las seis pantallas renderizan su árbol de widgets completo (incluidos `on_pre_enter`, imágenes, fuentes y carga de JSON) sin una sola excepción**.

## Dimensión 1 — Qué hace el programa

Flujo completo de planificación de eventos en un refugio:

- **Home** (`main.py:23-59`): dos botones grandes — *Crear evento* → pantalla `place`; *Ver eventos creados* → pantalla `events`.
- **Lugar** (`screens/place.py:18-90`): grid de 8 áreas del refugio; al seleccionar una, muestra un popup de carga (`show_loading`, `place.py:81-90`) y pasa a `resources` guardando `manager.selected_place`.
- **Recursos** (`screens/resources.py:24-200`): lista scrolleable de los recursos disponibles **filtrados por lugar** (`resources.py:60`); cada tarjeta muestra imagen, nombre, descripción y cantidad. Al tocar se resalta en azul (`toggle_resource`, `resources.py:203-214`). Al continuar valida co-requisitos y exclusiones (`try_continue`, `resources.py:217-225`).
- **Fecha** (`screens/date.py:26-200`): título + inicio + fin vía `MDDatePicker`/`MDTimePicker` (`date.py:118-149`); aplica siete validaciones (`create_event`, `date.py:152-200`) y, si el horario está libre, pregunta si el evento es recurrente (`ask_recurrence`, `date.py:203-278`).
- **Recurrencia** (`screens/recurrence.py:26-222`): patrón diaria/semanal/mensual + fecha límite; genera todas las ocurrencias, valida cada una contra el calendario **y contra las ya generadas de la misma serie** (`process_recurrence`, `recurrence.py:158-213`), y las guarda con un `series_id` UUID compartido.
- **Eventos creados** (`screens/events.py:20-150`): lista de eventos, pasados en gris (`events.py:55-72`); botones *Ver detalles* (`show_details`, `events.py:267-381`) y *Eliminar* con opción de borrar una ocurrencia o toda la serie (`confirm_delete`, `events.py:153-264`).

El corazón funcional es `resources_available` en `utils.py:26-111`: un **barrido temporal (sweep line)** que, para el intervalo pedido, particiona el tiempo en sub-intervalos según los cambios (inicios/fines de eventos solapantes), cuenta el uso simultáneo de cada recurso y lo compara contra su `quantity`; si hay bloqueo, avanza al final del conflicto más cercano y reintenta, devolviendo el próximo hueco libre.

## Dimensión 2 — Organización del código

**Fortalezas notables** (por encima de lo esperable en 1er año):

- **Separación real GUI / lógica.** Toda la lógica pura vive en `utils.py` (I/O JSON, `resources_available`, `validate_resources`) sin ninguna dependencia de Kivy. Esto es exactamente lo que permitió testear el negocio sin display, y es una decisión de diseño madura.
- **Una pantalla por módulo** en `screens/`, con `__init__.py` reexportando las clases (`screens/__init__.py`) — paquete Python bien formado.
- **Widgets reutilizables** en `widgets.py`: `ImageButton`, `RoundedButton`, `RoundedBox`, `show_message`, `show_loading`. Evita repetición y da coherencia visual.
- **Datos externos, no hardcodeados.** El inventario completo (44 recursos, con lugar/descripción/co-requisitos/exclusiones/cantidad) vive en `data/resources.json`; cambiar de refugio no toca código (`resources.py:20`, `utils.py:21-23`).

**Debilidades menores:**

- `screens/events.py:18` llama a `load_events()` a nivel de módulo (efecto secundario en el import) cuyo resultado se descarta — línea muerta.
- `resources.py:102` deja un `print(animal)` de depuración; también `resources.py` importa `json`, `Popup`, `ButtonBehavior` sin usarlos (imports muertos, comunes en varios `screens/*.py`).
- Los handlers de UI (p. ej. `on_pre_enter`) son largos porque construyen todo el layout imperativamente; en Kivy lo idiomático sería declarar el layout en `.kv`, pero para 1er año el enfoque imperativo es perfectamente válido y legible.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Lo que **corrí de verdad** (Python 3.12, venv con uv) y observé:

1. **`py_compile` de los 8 módulos** → OK, sin errores de sintaxis.
2. **Consistencia de datos** (JSON cargado real): 44 recursos, 5 eventos; los 8 lugares de `place.py` coinciden exactamente con los lugares referenciados en `resources.json` (ni sobran ni faltan); todos los recursos tienen las 6 claves requeridas. Sin desajustes.
3. **Hueco libre** (`resources_available`, fecha/lugar sin colisión): pedí `2026-09-01 10:00–12:00` con `['Escoba Industrial','Desinfectante']` → devolvió el intervalo pedido tal cual, `occupied=[]`. Correcto.
4. **Cantidad > 1 respetada:** el evento existente "Esterilización de Firulais" usa `Veterinario` (quantity=2). Pedí un segundo evento solapante con `Veterinario` → **lo permite** (queda 1 de 2 libre). Correcto y no trivial.
5. **Agotamiento de quantity=1:** "Adopción de Stuart Little" (2026-07-12 09:00–13:00) usa `Voluntario de Adopciones` (quantity=1). Pedí 10:00–11:00 solapando → **sugirió automáticamente 13:00–14:00**, justo cuando el recurso se libera. Correcto.
6. **Agotamiento de quantity=2** (eventos sintéticos): dos eventos simultáneos consumiendo las 2 unidades de `Anestesia General` → el tercer pedido fue bloqueado y sugirió el hueco tras la liberación. Correcto.
7. **Solapamiento parcial** (evento que empieza a mitad del intervalo): correctamente contado; sugiere el siguiente hueco. Correcto.
8. **Exclusiones** (`validate_resources`): `['Veterinario','Entrenador Certificado']` → devuelve el error de incompatibilidad. Correcto.
9. **Co-requisitos:** `['Veterinario']` solo → `"Veterinario requiere Kit Médico Básico"`; añadiendo el kit → sin errores. Correcto.
10. **Simulación de conflicto interno en recurrencia:** repliqué el bucle de `process_recurrence` con una recurrencia diaria de un recurso quantity=1 y un evento previo que bloquea una de las ocurrencias → la ocurrencia conflictiva fue detectada con su sugerencia. Correcto.
11. **Ciclo crear → persistir → recargar:** creé un evento válido, lo guardé con `save_events`, recargué (`load_events` pasó de 5 a 6) y restauré el original. La escritura produce JSON UTF-8 válido con acentos preservados (`ensure_ascii=False`, `utils.py:18`). Correcto.
12. **Arranque GUI real** (xvfb, tras restaurar los `.kv` de kivymd): las 6 pantallas renderizan el árbol de widgets completo, ejecutan sus `on_pre_enter`, cargan imágenes/fuentes/JSON → **cero excepciones**.

**Fallos distinguidos:**

- **Del entorno (no del código):** el `FileNotFoundError: label.kv` inicial es el bug de empaquetado de `kivymd==1.2.0` por pip/uv (ruedas sin archivos `.kv`). Reproducible e independiente del código de la estudiante; se sortea restaurando los `.kv`.
- **De la estudiante, muy menor (caso borde):** en `date.py:193-198` y en `recurrence.py:180`, cuando `resources_available` devuelve `(None, None, ...)` — el caso "no existe ningún hueco futuro que libere el recurso" (`utils.py:110-111`) — el mensaje al usuario formatearía `de None a None`. En la práctica es un camino estrecho: casi siempre `conflicts_ends` encuentra un fin de evento y sugiere una fecha concreta (lo verifiqué: un bloqueo de 08:00–20:00 sí sugirió 20:00). Solo se alcanza si todos los eventos bloqueantes ya terminaron respecto a `current_start`. No rompe la app; solo muestra un texto poco claro.
- **Robustez frente a JSON corrupto de eventos:** `load_events` protege `FileNotFoundError` y `JSONDecodeError` (`utils.py:10-13`), pero si un evento tuviera una fecha mal formada, `resources_available` lanzaría `ValueError` en `strptime` (`utils.py:36-37`). Riesgo real bajo, porque la app siempre escribe fechas válidas.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Legibilidad alta:** nombres en español coherentes, comentarios que explican el *porqué* (especialmente el algoritmo de barrido), funciones cortas en `utils.py`.
- **Manejo de errores de usuario correcto:** todas las validaciones de entrada devuelven un `show_message` y `return` temprano en vez de reventar (`date.py:152-200`, `recurrence.py:129-155`). Muy bien para 1er año.
- **Mejorables menores:** el `except:` desnudo en `recurrence.py:131` debería ser `except ValueError`; `print(animal)` de debug en `resources.py:102`; imports muertos en varios `screens/*.py`; el `load_events()` huérfano en `events.py:18`.
- **`validate_resources`** (`utils.py:115-126`) reporta exclusiones de forma bidireccional, generando mensajes casi duplicados ("A no puede usarse junto a B" y "B no puede usarse junto a A"). Como solo se muestra `errors[0]` (`resources.py:220`), el usuario no lo nota, pero deduplicar sería más limpio.

## Dimensión 5 — Datos y persistencia

- **Modelo de datos claro y bien pensado.** `resources.json`: cada recurso con `name`, `place[]`, `description`, `associated[]` (co-requisitos), `exclusions[]`, `quantity`. `events.json`: `title`, `start`, `end`, `recurrence`, `until` (en recurrentes), `resources[]`, `place`, `series_id`. El `series_id` UUID como agrupador de series es una idea correcta y bien ejecutada (`recurrence.py:199`, `events.py:257-262`).
- **Serialización robusta:** `save_events` con `indent=4, ensure_ascii=False` → JSON legible con acentos (`utils.py:16-18`). `load_events` degrada a lista vacía ante archivo ausente o corrupto (`utils.py:6-13`).
- **Separación config/estado:** `resources.json` es inventario (config), `events.json` es estado mutable. Buena distinción.

## Dimensión 6 — Informe (`report.md`)

El informe (`report.md`, idéntico a `README.md`) es **excelente y en su enorme mayoría fiel al código**:

- La descripción del algoritmo de barrido temporal (`report.md:147-167`) **coincide con precisión** con `utils.py:26-111`, incluidos los casos difíciles (eventos envolventes, parciales, quantity>1) — que yo **verifiqué ejecutando** (tests 4-7 arriba).
- El modelo de datos documentado (`report.md:58-95`, `252-279`) coincide con los JSON reales.
- Las siete validaciones de fecha listadas (`report.md:223-230`) están todas presentes en `date.py:152-200`.
- La lógica de recurrencia y borrado por serie (`report.md:171-190`, `242-248`) coincide con `recurrence.py` y `events.py`.

**Discrepancias menores (honestidad):**

- El árbol de directorios del informe usa `Screens/` con mayúscula (`report.md:300`) pero el paquete real es `screens/` en minúscula. Cosmético.
- El informe no menciona el caso borde `(None, None)` del algoritmo ni la posibilidad de texto "None a None"; afirma que el sistema "sugiere automáticamente el siguiente hueco libre" (`report.md:16`), lo cual es cierto **salvo** ese camino estrecho.
- El informe no exagera de forma engañosa: cuando dice que el algoritmo "maneja correctamente todos los casos difíciles" (`report.md:167`), mi ejecución lo respalda en los casos que probé. No detecté afirmaciones de "demostrado/probado" que sobreestimen una validación manual inexistente.

---

## Valoración global (orientativa, sin nota numérica)

Un proyecto **sobresaliente para primer año**. La estudiante eligió un dominio con restricciones ricas (cantidades múltiples, co-requisitos, exclusiones, recurrencia con detección de conflictos internos) y lo resolvió con una arquitectura limpia que separa de verdad la lógica de negocio de la GUI — algo que muchos proyectos de este nivel no logran. El algoritmo de barrido temporal no es trivial y, sometido a ejecución real con datos y casos sintéticos (agotamiento de quantity=1 y quantity=2, solapamientos parciales, conflictos internos de series), **funcionó correctamente en todos los casos probados**. La persistencia es sólida y el informe es de los más rigurosos y fieles al código que se pueden pedir. Los defectos son menores: imports/print de depuración, un `except` desnudo, y un único caso borde donde un mensaje podría mostrar "None". El único obstáculo para arrancar la GUI fue un bug de empaquetado de `kivymd==1.2.0` ajeno a la estudiante; restaurados los `.kv`, la app arranca entera sin errores.

- **Principal fortaleza:** la separación GUI/lógica y la corrección del algoritmo de conflictos con cantidades múltiples, verificada por ejecución.
- **Principal área de mejora:** cubrir el caso borde `(None, None)` de `resources_available` para no mostrar "None a None", y limpiar los residuos de desarrollo (print de debug, imports muertos, `load_events()` huérfano).
