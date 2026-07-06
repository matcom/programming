# Reporte de evaluación — Issue #242

- **Estudiante:** Briell Quintana Hernández
- **Grupo:** C-122
- **Repositorio:** https://github.com/Briell06/Proyecto-I
- **Despliegue:** https://briell.pythonanywhere.com/ (verificado en línea, HTTP 200)
- **Tipo de proyecto:** Aplicación web Django (no es una app de consola). Django 5.2.7, Python 3.14.

> **Nota de calibración.** La rúbrica base asume una app de consola `main.py`. Esta entrega es un
> proyecto Django completo con ORM, vistas basadas en clases, formularios, plantillas, panel admin,
> migraciones y un pequeño suite de tests con pytest. El nivel técnico está muy por encima de lo
> esperable en un primer proyecto de 1er año. La evaluación se adapta al artefacto real.

---

## Ejecución dinámica (lo que se hizo y observó)

Entorno aislado con `uv` (`uv venv --python 3.14`, `uv pip install -r requirements.txt`). Todo lo
ejecutado corrió sin instalar nada en el sistema.

1. **`manage.py check`** → `System check identified no issues (0 silenced).` Sin errores.
2. **`manage.py migrate`** → aplica todas las migraciones (incluyendo `airline_app.0001/0002/0003`) sin
   fallo, creando `db.sqlite3` local.
3. **`pytest -q`** → `4 passed in 0.45s`. Los 4 tests (`test_flight_model.py`, `test_flight_views.py`)
   pasan. Cubren: `clean()` con fechas `None` sin romper, mapeo `personnel`→`pilot` en restricciones,
   creación de vuelo vía vista HTTP, y que una actualización inválida no produzca error 500.
4. **Servidor de desarrollo** (`runserver 127.0.0.1:8899`) levantó limpio. Recorrido HTTP de endpoints:

   | Endpoint | Código | Endpoint | Código |
   |---|---|---|---|
   | `/` | 200 | `/disponibilidad/` | 200 |
   | `/pistas/` | 200 | `/pistas/crear/` | 200 |
   | `/puertas/` | 200 | `/vuelos/crear/` | 200 |
   | `/personal/` | 200 | `/admin/` | 302 (redirige a login, correcto) |
   | `/aeronaves/` | 200 | `/pistas/999/` | 404 (correcto) |
   | `/vuelos/` | 200 | `/noexiste/` | 404 (correcto) |
   | `/restricciones/` | 200 | | |
   | `/buscar-horario/` | 200 | | |

5. **CRUD real por el navegador (POST con CSRF).** Obtuve el token CSRF de `/pistas/crear/` y envié un
   POST creando una pista → **302** (redirección de éxito) y la pista aparece luego en `/pistas/`. El
   ciclo formulario→validación→persistencia→listado funciona de punta a punta.
6. **Lógica de dominio ejercitada vía `manage.py shell`** (`/tmp/242_smoke.py`), resultados reales:
   - Vuelo válido creado: `get_duration()` = 3.0 h, `get_required_copilots()` = 1. ✔
   - Segundo vuelo con la **misma pista solapada** → rechazado: *"La pista seleccionada no está
     disponible durante el tiempo seleccionado."* ✔ (detección de conflictos funciona)
   - Vuelo con **origen == destino** → rechazado: *"El origen y el destino no pueden ser iguales."* ✔
   - Vuelo con **salida en el pasado** → rechazado. ✔
   - `Flight.find_next_available_slot(...)` → devolvió un dict con `departure_time`/`arrival_time`. ✔
   - `Runway(length_meters=100).full_clean()` → rechazado por regla 800–5000 m. ✔

**Conclusión de ejecución:** el programa arranca, migra, pasa sus tests, sirve todas sus páginas y
—lo más importante— la lógica de negocio (conflictos de recursos, validaciones, búsqueda de slot)
hace exactamente lo que el informe afirma. **No se observó ningún `Traceback` en ninguna prueba.**

---

## Dimensión 1 — Qué hace el programa

Sistema web de **gestión de operaciones aeroportuarias**. Modela cinco entidades de dominio y un
motor de restricciones:

- `Runway` (pistas), `Gate` (puertas), `Aircraft` (aeronaves), `Personnel` (pilotos/copilotos),
  `Flight` (vuelo, entidad transaccional central) y `ResourceConstraint` (reglas configurables).
  Todo en `airline_app/models.py`.
- Punto de entrada real: proyecto Django estándar. `manage.py` arranca el servidor; `main.py:8` es un
  *bootstrapper* casero que crea el venv, instala deps, corre `makemigrations`/`migrate` y lanza
  `runserver`. El proyecto Django vive en `config/` (`config/settings.py`, `config/urls.py`).
- Flujo: el usuario crea recursos (pistas, puertas, personal, aeronaves), opcionalmente define
  restricciones, y luego programa vuelos. Al guardar un vuelo, `Flight.clean()`
  (`models.py:459`) valida fechas, disponibilidad de cada recurso en la ventana temporal, y
  restricciones de negocio; `validate_copilots()` (`models.py:608`) exige el mínimo de copilotos
  según duración. `find_next_available_slot()` (`models.py:655`) busca el próximo hueco libre en
  incrementos de 1 hora hasta 30 días.

El dominio no es trivial: incluye ventana de mantenimiento de 24 h para aeronaves (`models.py:329`),
copilotos escalonados por duración (`models.py:441`) y dos tipos de restricción (co-requisito /
exclusión mutua) resueltas dinámicamente (`models.py:552`).

## Dimensión 2 — Organización del código

Excelente para el nivel. Separación en capas idiomática de Django:

- **Modelos** con lógica de negocio encapsulada como métodos (`is_available`, `get_duration`,
  `clean`, `validate_resource_constraints`) — `models.py`.
- **Vistas** basadas en clases genéricas (`ListView`/`CreateView`/`UpdateView`/`DeleteView`/
  `DetailView`) más dos vistas función (`home`, `check_availability`, `find_slot`) — `views.py`.
  Reutilización clara: el patrón CRUD se repite 6 veces sin duplicar lógica de negocio.
- **Formularios** con `ModelForm`, widgets, labels y validación cruzada en `clean()` — `forms.py`.
- **URLs** limpias y nombradas en español — `urls.py`.
- **Admin** configurado con `list_display`, `list_filter`, `fieldsets` — `admin.py`.
- Nombres de variables, funciones y clases claros y consistentes; docstrings en casi todos los
  métodos. Constantes de dominio como `choices` (`FLIGHT_STATUS`, `CONSTRAINT_TYPES`).

Uso de clases donde el dominio lo pide (cada entidad es una clase de modelo). Esto está muy por
encima del "todo en un `main.py`" que la rúbrica contempla como escenario típico.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ver la sección "Ejecución dinámica" arriba. Resumen:

- Arranca, migra, pasa sus 4 tests, sirve las 15 rutas probadas con los códigos HTTP correctos.
- CRUD verificado por POST HTTP real (creación de pista → 302 → aparece en listado).
- Validaciones de negocio confirmadas por ejecución: conflicto de pista, origen==destino, salida en
  el pasado, longitud de pista fuera de rango, y `find_next_available_slot` devolviendo un slot.
- **Detalle bien resuelto:** hay un comentario explícito en `models.py:508` explicando que
  `clean()` evita hacer queries con `None` (`departure_time__lt=None`) — hay incluso un test dedicado
  a esa regresión (`test_flight_model.py:9`). Señal de que el estudiante depuró casos borde reales.
- **Observación menor (no verificada como fallo, potencial):** `home()` (`views.py:38`) usa
  `datetime.now()` (naive) para comparar `departure_time__gte`, mientras el proyecto tiene
  `USE_TZ = True`. En Django esto suele generar un `RuntimeWarning` por comparar naive vs aware; no
  llegó a romper en las pruebas, pero conviene usar `django.utils.timezone.now()` como en el resto
  del código.
- **Observación menor:** el manejo de copilotos en la vista (`FlightCreateView.form_valid`,
  `views.py:374`) guarda el vuelo y *después* llama a `validate_copilots()`, que puede lanzar
  `ValidationError` tras la persistencia. En la práctica queda un vuelo guardado sin copilotos
  suficientes si la validación posterior falla y no se revierte. No es un crash (el test
  `test_flight_update_view_does_not_500` confirma que no da 500), pero la atomicidad no es perfecta.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy sólido:

- Legibilidad e indentación consistentes; código formateado (usa `black`, listado en deps).
- f-strings idiomáticas, bucles claros, sin variables globales innecesarias.
- Manejo de errores con `try/except ValidationError` donde corresponde (`views.py:381`,
  `admin.py:122`).
- Docstrings descriptivos en español, con toques personales ("pequeño toque personal :)"
  en `models.py:275`).
- Importaciones ordenadas. Sin duplicación evidente.
- Único punto flojo estilístico: varios `from django.db.models import Q` *dentro* de cada método
  `is_available` (`models.py:130`, `177`, `253`, `323`) en vez de un import a nivel de módulo. No es
  un error, pero repite el import cuatro veces.

No se penaliza ausencia de type hints/async. De hecho **sí hay tests**, lo cual es un extra sobre lo
esperado.

## Dimensión 5 — Datos y persistencia

- Persistencia vía ORM de Django sobre SQLite en desarrollo y MySQL en producción
  (`dj-database-url` en `settings.py:80`), configurable por variable de entorno. Buena elección.
- Estructuras de datos razonables: relaciones `ForeignKey` con `on_delete=PROTECT` (impide borrar
  recursos en uso — decisión de integridad acertada, `models.py:392`), `ManyToManyField` para
  copilotos, `choices` para enumeraciones.
- Migraciones presentes y aplicables. **Detalle de higiene:** la migración `0002` borra
  `ResourceConstraint` y `0003` la vuelve a crear (`migrations/0002_delete_resourceconstraint.py`,
  `0003_resourceconstraint.py`). Funciona, pero es un historial de migraciones "sucio" fruto de
  idas y vueltas de diseño; en un proyecto real se aplanaría (`squashmigrations`). No afecta la
  ejecución.
- `db.sqlite3` no está versionado (correcto, en `.gitignore`).

## Dimensión 6 — Informe (`report.md`)

Muy completo y, sobre todo, **honesto respecto al código** — lo que afirma, el código lo cumple
(verificado por ejecución). Cubre introducción, características, tecnologías, instalación (auto y
manual), uso, reglas de negocio, modelos, endpoints y aprendizaje.

Discrepancias / sobreestimaciones detectadas (todas menores):

- Afirma **"previene condiciones de carrera (race conditions)"** y **"transacciones atómicas"**
  (`report.md:17`). El código **no** usa `transaction.atomic()` ni `select_for_update()` en ningún
  punto; la prevención de doble reserva es a nivel de validación lógica en `clean()`, no de bloqueo
  transaccional. Bajo concurrencia real dos requests simultáneas podrían pasar ambas la validación.
  Es la afirmación más sobredimensionada del informe.
- Menciona **`GenericForeignKey`** para las restricciones (`report.md:303`). En realidad usa
  `PositiveIntegerField` con `primary_resource_type`/`primary_resource_id` resueltos a mano
  (`models.py:64`), que es un patrón parecido pero no el `contenttypes.GenericForeignKey` de Django.
- Menciona **"except Exception as e" que registra en logs** (`report.md:285`); el `except` genérico
  existe en `admin.py:126` pero no hay logging real configurado.
- Dice **"Python 3.8+"** (`report.md:34`) pero `pyproject.toml` exige `>=3.14`. Inconsistencia menor.
- "Capacidad de aeronave: 1-700" en el informe (`report.md:271`) vs. el código valida 10–800
  (`models.py:344`). Discrepancia de números menor.

Nada de esto invalida el trabajo; son exageraciones de redacción típicas de querer "sonar
profesional". El núcleo del informe describe con fidelidad lo que el sistema hace.

---

## Valoración global (orientativa)

Trabajo **sobresaliente para un primer proyecto de 1er año**. Es un sistema Django funcional,
desplegado en línea, con lógica de dominio no trivial que **corre y valida correctamente todo lo que
promete** (verificado ejecutando, no solo leyendo). Organización de código idiomática, formularios y
vistas bien estructurados, panel admin, y hasta un suite de tests que pasa.

- **Principal fortaleza:** la lógica de negocio (detección de conflictos de recursos, restricciones
  configurables, búsqueda de slot, ventana de mantenimiento) está bien pensada y probada en
  ejecución.
- **Principal área de mejora:** ajustar el informe para no sobreestimar garantías que el código no
  da (atomicidad / race conditions), y pulir dos detalles técnicos (`datetime.now()` naive en
  `home()`, orden guardar→validar copilotos, historial de migraciones).

El estudiante declara experiencia previa con Django; eso explica el nivel. Aun así, el dominio del
framework, la atención a casos borde y la disciplina de escribir tests hablan de un trabajo serio y
bien ejecutado.
