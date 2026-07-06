# Reporte detallado — Proyecto I (1er año)

- **Issue:** #268
- **Repo:** https://github.com/josuetamayo05/Smart-Event-Planner
- **Estudiante:** Josué Alejandro Tamayo Ayrado
- **Grupo:** C122
- **Descripción (issue):** aplicación de escritorio con Python y el framework Flet para planificar eventos clínicos en hospitales (cirugías, consultas, procedimientos y otros eventos médicos).

> Nota de calibración: es un proyecto de **primer año**, pero éste es un trabajo netamente por encima de la media esperada a ese nivel: app de escritorio con GUI real (Flet), arquitectura en capas, motor de reglas no trivial y hashing de contraseñas serio. La evaluación es exigente en los detalles precisamente porque el resto está muy bien.

---

## Resumen de la ejecución (dinámica, OBLIGATORIO)

Entorno aislado montado con `uv` (Python 3.12.8) + `flet==0.80.2` (+ `flet-desktop`, `flet-web`, `pip`). Como es Flet (GUI) y el entorno es headless, hice dos cosas: (a) ejercité la **lógica de negocio** importando los módulos y llamando funciones con escenarios reales; (b) **arranqué la app entera** en modo web para verificar que el arranque no explota.

**(a) Lógica de negocio — todo lo que probé funcionó:**

- Import de `models.*` + `utils.auth_manager` sin errores.
- `DatabaseManager("database.json")` carga **17 recursos y 9 eventos** correctamente (`database_manager.py:25`).
- Autenticación (`auth_manager.py:105`): `josue/admin123` → devuelve rol `admin`; contraseña mala → `None`; `javier/123456` → rol `staff`. PBKDF2 verificado.
- Corequisitos de quirófano (`scheduler.py:196`): quirófano solo → `OR_COREQUISITES` "requiere cirujano x1, anestesiologo x1, enfermera x2". Correcto.
- Cruce de medianoche (`scheduler.py:59`): evento 23:00→01:00 → `CROSSES_MIDNIGHT`. Correcto.
- Equipo completo, lunes 09–11, dentro de disponibilidad → **sin violaciones** (`[]`). Correcto.
- Mismo equipo 17:00–18:00 (fuera de la ventana 08–16 de los médicos) → 4× `OUTSIDE_AVAILABILITY` (`scheduler.py:98`). Correcto.
- Cirugía cardíaca sin cardiólogo ni CEC → `CARDIO_SURGEON_MISSING` + `CEC_MISSING` (`scheduler.py:214`). Correcto.
- Exclusión mutua tomógrafo/radioterapia solapados → `CT_VS_RADIOTHERAPY` (`scheduler.py:263`). Correcto.
- Capacidad: dos eventos solapados sobre `OR1` (quantity=1) → `RESOURCE_CAPACITY_EXCEEDED` (`scheduler.py:169`). Correcto.
- `find_next_slots({"roles":["cardiologo"]}, 30min, ...)` → devolvió 2 huecos válidos con paso de 15 min (`scheduler.py:289`). Correcto.
- Entrada inválida: `parse_dt("2026-99-99","25:99")` → `ValueError` (esperado; el que lo llama debe capturarlo).

**(b) App completa (`app.py`) en modo web:** lancé `ft.run(main, view=WEB_BROWSER, port=...)`. El servidor levantó sin traceback y `curl http://127.0.0.1:PORT/` devolvió **HTTP 200**. La vista de login se construye en el arranque sin excepciones. No pude interactuar con la GUI (headless, sin navegador con sesión), pero el arranque, el cableado de vistas y el servidor son sanos.

Ningún `Traceback` en todo lo ejecutado.

---

## 1. Qué hace el programa

Aplicación de **escritorio** (Flet) para planificar **eventos clínicos hospitalarios**: cirugías, consultas, diagnósticos y terapias, con asignación de recursos (quirófanos, médicos, enfermeras, equipos) y **detección automática de conflictos**. Punto de entrada `app.py` (`app.py:209`, `ft.run(main)`), que primero muestra un **login** (`app.py:206`) y, tras autenticar, arma la pantalla principal con barra lateral de navegación y seis vistas: Dashboard, Eventos, Nuevo evento, Calendario diario, Recursos y Búsqueda (`app.py:141-159`). El estado se persiste en `database.json` y los usuarios en `users.json`.

El corazón real del proyecto es `models/scheduler.py`: un motor de reglas que valida capacidad de recursos, corequisitos por tipo de evento (p.ej. el quirófano exige 1 cirujano + 1 anestesiólogo + 2 enfermeras; la cirugía cardíaca exige cardiólogo + equipo CEC), exclusiones mutuas (quirófano infeccioso vs. trasplante el mismo día; tomógrafo vs. radioterapia solapados), ventanas de disponibilidad semanal y *blackouts* por recurso. Además hay búsqueda inteligente de huecos libres (`find_next_slots`, `find_next_slots_autofill`).

## 2. Organización del código

Muy buena para 1er año. **Separación en capas real:** `models/` (dominio + persistencia), `ui/` (vistas Flet, estado, diseño, catálogos), `utils/` (auth). Nada de "todo en un `main.py` gigante".

- Modelos como `@dataclass` con `from_dict`/`to_dict` (`event.py:8`, `resource.py:5`, `constraint.py:3`) — patrón limpio y consistente.
- `DatabaseManager` con lock (`RLock`), guardado atómico vía `os.replace` sobre un `.tmp`, y `backup/export/import` (`database_manager.py:40-74`). Esto es notablemente maduro.
- Vistas como clases (`LoginView`, `EventsView`, etc.) con un patrón uniforme `self.view` + `refresh()` (`app.py:56-65`).
- Nombres claros en general (`_check_resource_capacity`, `is_resource_free`, `find_next_slots`).

Puntos flojos de organización:

- **`test.py` (855 líneas)** es una versión **monolítica previa** de toda la app (login+vistas+scheduler en un solo `main`), que quedó en el repo tras refactorizar a `ui/`+`models/`. Es código muerto que confunde: parece "el proyecto" por su tamaño pero **no es el que corre** (`app.py` es el bueno). Debería borrarse o moverse a un histórico.
- `ui/time_utils.py:9` `sum_one_day` está **a medio escribir** (cuerpo vacío tras `temp=date_str.split("-")`), y el `if __name__=="__main__": print(datetime)` (`time_utils.py:14`) imprime la *clase*, no algo útil — restos de depuración.
- `utils/create_users.py:1` importa `from auth_manager import AuthManager` (import plano) en vez de `from utils.auth_manager import ...`; sólo corre si se ejecuta desde dentro de `utils/`. Script auxiliar, no crítico.
- En `app.py` hay bastante **código comentado** (funciones `apply_nav_style`/`refresh_nav`, `app.py:98-116`) que no se llegó a activar: el resaltado del ítem de menú seleccionado quedó desconectado.

## 3. Corrección funcional (basada en ejecución real)

Ver "Resumen de la ejecución" arriba: **arranca** (HTTP 200 en modo web) y **toda la lógica de negocio que ejercité respondió correctamente**, incluidos los ocho tipos de violación y la búsqueda de huecos. La app hace lo que promete el issue/README.

Hallazgos concretos de corrección:

- **Bug latente (dead branch): `_auto_requisites` no existe.** En `scheduler.py:144` `validate_event(event, description=None)` tiene una rama `if description is not None: violations.extend(self._auto_requisites())` (`scheduler.py:148-150`), pero **`_auto_requisites` no está definido en ninguna parte** (confirmado por grep). No revienta hoy porque **ningún llamador pasa `description`** (todas las llamadas en `new_event.py`, `events.py` y dentro del propio `scheduler` usan un solo argumento). Es una trampa: el día que alguien llame `validate_event(ev, "algo")` obtendrá `AttributeError`. Sugerencia: eliminar esa rama muerta (y el parámetro `description`) o implementar el método.
- **`parse_dt` importado y no usado** en `scheduler.py:10` (`from ui.time_utils import parse_dt`) — import huérfano; además crea una dependencia `models → ui` que ensucia la separación de capas (los modelos idealmente no deberían importar de `ui`).
- **Inconsistencia de `event_type` entre datos y reglas.** El catálogo (`ui/catalogs/event_types.py`) usa `cirugia_cardiaca`, pero en `database.json` hay eventos con `event_type` `"cirugia_cardiovascular"` y `"examen"`. Las reglas de cirugía cardíaca (`scheduler.py:214`) sólo disparan con el string exacto `"cirugia_cardiaca"`, así que un evento etiquetado `cirugia_cardiovascular` **no** activaría la exigencia de cardiólogo/CEC. Sugerencia: normalizar los códigos de tipo o centralizarlos en una constante.
- **Validación de formularios:** en la capa modelo `parse_dt` lanza `ValueError` con fecha/hora mal formadas; la responsabilidad de capturarlo recae en las vistas. No pude confirmar por GUI que el mensaje al usuario sea amable, pero el patrón `try/except` sí aparece en varias vistas.

Nada de esto impide que la app corra; son detalles que un revisor exigente marca.

## 4. Buenas prácticas de Python (nivel principiante)

Muy por encima del nivel:

- `@dataclass`, `from __future__ import annotations`, type hints razonables, `Optional`, `Counter`, `timedelta`. Uso idiomático real.
- f-strings para los mensajes de violación.
- `try/except` acotado donde toca (parseo de blackouts en `scheduler.py:111`, lectura de `client_storage` en `login.py:23`).
- **Seguridad de contraseñas seria:** PBKDF2-HMAC-SHA256 con salt por usuario, 180k iteraciones, `hmac.compare_digest`, y **migración automática** desde un hash SHA256 legacy (`auth_manager.py:49-127`). Esto es sobresaliente para 1er año.
- Guardado atómico y locking en la "base de datos" JSON.

Detalles menores: algún import huérfano (arriba), bloques comentados en `app.py`, y estilo de espaciado algo irregular (a veces `x=y` sin espacios). Nada penalizable a este nivel.

## 5. Datos y persistencia

Sólida. `database.json` con `resources`/`events`/`constraints` y `users.json` con hashes PBKDF2. `DatabaseManager` hace *upsert* por `id`, borrado, y **guardado atómico** (`os.replace`) para no corromper el archivo si el proceso muere a mitad (`database_manager.py:40`). Estructuras razonables: recursos con `availability.weekly` (ventanas por día) y `blackouts`, eventos con `resource_units` para reservar múltiples unidades del mismo recurso. Verifiqué en vivo que carga 17 recursos y 9 eventos y que las lecturas/escrituras funcionan (probé upsert+delete de eventos de prueba y restauré con `git checkout`).

Observación: los datos de `database.json` contienen recursos y eventos "de juguete" claramente creados probando la app (p.ej. recurso `JATA` con `role: "Developer"`, usuarios `Jos`/`JAV`), lo cual es normal en un entregable, pero conviene enviar una BD limpia/semilla.

## 6. Informe (README.md)

No hay `report.md`; el **README** hace de informe y es **excelente** (badges, capturas, tabla de estructura, guía de instalación/uso, funcionalidades). Coincide en lo esencial con el código: login por `users.json`, validación de conflictos en tiempo real, calendario con slots, búsqueda por tokens, recursos físicos/humanos.

Discrepancias/omisiones:

- Dice "aplicación de escritorio **exclusiva para Windows**". Es Flet, que es multiplataforma; corre igual en Linux (lo levanté aquí). No es un error grave, pero sobre-restringe.
- El README no menciona el **motor de reglas** con la profundidad que merece (corequisitos por tipo, exclusiones mutuas, disponibilidad, CEC) — irónicamente **subestima** lo más impresionante del proyecto.
- No documenta que `test.py` es una versión vieja/muerta ni menciona el bug latente `_auto_requisites`. No sobreestima features: todo lo que promete, el código lo tiene.

Las credenciales que documenta (`admin`/`staff` como roles) son orientativas; los usuarios reales del `users.json` son `josue` (admin) y `javier` (staff).

---

## Valoración interna

Trabajo **muy fuerte** para 1er año: app de escritorio con GUI real, arquitectura en capas limpia, motor de validación de conflictos genuinamente no trivial (y **correcto** en todos los escenarios que ejecuté), persistencia atómica y autenticación con hashing profesional. Los defectos son de pulido, no de fondo: un `test.py` monolítico muerto que debería borrarse, una rama muerta con un método inexistente (`_auto_requisites`), un import huérfano que acopla `models`→`ui`, y una inconsistencia de `event_type` entre datos y reglas. Principal fortaleza: el scheduler + la madurez de infraestructura. Principal mejora: limpiar el código muerto y cerrar el bug latente. La nota final la pone el profesor.
