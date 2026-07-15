# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #320
- **Repositorio:** https://github.com/RDR-03/EventsManagement
- **Estudiante:** Omar Alejandro Cuellar Ash
- **Grupo:** C111
- **Descripción declarada:** Herramienta para la organización logística de eventos hoteleros. Permite a un coordinador crear eventos, administrar un inventario de recursos y, principalmente, prevenir conflictos de disponibilidad horaria entre eventos que comparten recursos limitados.

---

## Nota metodológica importante

**No es una aplicación de consola: es una app web hecha con Streamlit** (`main.py:1`, `import streamlit as st`). El punto de entrada monta una navegación multipágina (`main.py:52-58`, `st.navigation([...])`) con cuatro páginas: Inicio, Crear Evento, Gestionar Inventario y Eventos Planificados. No hay `input()` ni menú de terminal.

Adapté la ejecución así:

1. **Lógica de negocio directa.** El diseño separa limpiamente el dominio (`core/`) de la interfaz (`UI/`), así que pude instanciar `planification` y `resource` y ejercitar `valid_event`, `resource_availability`, `find_space`, `remove_event`, la persistencia (`SaveData`/`LoadEvents`/`LoadResources`) y `valid_datetime` con datos reales, sin tocar Streamlit.
2. **Arranque headless de la GUI.** Ejecuté `streamlit run main.py --server.headless true`; el servidor levantó y respondió **HTTP 200** en `/`, sin trazas de error en el log. La app arranca correctamente.
3. Instalé `streamlit` y `pandas` en un venv 3.12. Ojo: `pandas` (usado en `UI/menus/see_schedule.py:2`) **no está declarado** en `pyproject.toml` — la instalación limpia con solo las dependencias declaradas fallaría al abrir esa página.

---

## Dimensión 1 — Qué hace el programa

El sistema modela un dominio hotelero con tres entidades:

- **`resource`** (`core/resources.py:1`): un activo con `name`, `total_cuantity`, `available`, `in_use`, más `dependencies` (dict recurso→cantidad) y `conflicts` (lista). Métodos para aumentar/disminuir cantidad, declarar dependencias (`dependant_on`, `resources.py:36`) y conflictos mutuos (`establish_conflict`, `resources.py:45`).
- **`event`** (`core/events.py:6`): tipo, `beginning`/`end` como `datetime`, `needed_resources` (dict recurso→cantidad) y descripción. Serializa a/desde dict.
- **`planification`** (`core/planification.py:6`): el núcleo. Mantiene `self.events` ordenada por fecha de inicio y alberga la validación.

El inventario base y las reglas del dominio se declaran en `core/_init_.py:6-32` (10 recursos con dependencias y un conflicto Tv↔Datashow).

Los flujos de usuario:
- **Crear Evento** (`UI/menus/event_creation.py`): elegir tipo, recursos y cantidades, fechas (con un *Asistente de Disponibilidad* que llama a `find_space`), validación en vivo de disponibilidad, e inserción ordenada por fecha (`event_creation.py:130-140`).
- **Gestionar Inventario** (`see_inventory.py`): aumentar/disminuir cantidades, con una verificación previa a la baja que detecta y bloquea reducciones que romperían eventos ya agendados (`see_inventory.py:88-120`).
- **Eventos Planificados** (`see_schedule.py`): tabla con `pandas`, y borrado con diálogo de confirmación (`@st.dialog`, `see_schedule.py:57`).

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** La separación dominio/UI es de las mejores que se ven a este nivel:

- `core/` contiene toda la lógica y **no importa Streamlit** en ninguna parte. Esto es lo que me permitió testear el motor sin GUI.
- Cada clase tiene responsabilidad única y clara. `planification` concentra las reglas; `resource` conoce sus dependencias y conflictos; `event` solo lleva datos + serialización.
- La UI está partida en una página por archivo bajo `UI/menus/`, cada una consumiendo el estado compartido de `st.session_state`.

Debilidades menores:
- `core/core.py` está **vacío** (archivo muerto). El bot de verificación también reportó un `planification2.py` que no está en el repo actual — probablemente restos de iteraciones ya limpiados.
- El módulo de inicialización se llama `core/_init_.py` (un solo guion bajo a cada lado), no `__init__.py`. **No es un package real de Python**; funciona porque se importa explícitamente como módulo (`from core._init_ import ...`, `main.py:2`). Funciona, pero es un nombre confuso y no idiomático.
- Nombres de clase en minúscula (`resource`, `event`, `planification`) — la convención Python (PEP 8) es `PascalCase` para clases. El informe las llama `Resource`/`Event`/`Planification`, que es lo correcto pero no coincide con el código.
- Errata persistente en un atributo del dominio: `total_cuantity` (por `quantity`), presente en el JSON serializado.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí el motor con datos reales. Todo lo que sigue lo **ejecuté**, no lo leí:

1. **Evento vacío rechazado.** `valid_event(..., {})` → `"Debe asignar al evento al menos un recurso"`. ✅
2. **Dependencias respetadas.** Pedir `Datashows x2` (que dependen de Pantalla + Técnico AV) → evento válido. ✅
3. **Solapamiento temporal detectado.** Con un evento 10:00–12:00 usando los 2 datashows, un segundo evento solapado 11:00–13:00 pidiendo 1 datashow → `"No hay disponibilidad de Datashows en estas fechas"`. ✅ El motor de intersección de intervalos (`resource_availability`, `planification.py:53`, condición `event.beginning < end and beginning < event.end`) **funciona correctamente**.
4. **No-solapamiento aceptado.** El mismo datashow pedido en 2026-08-02 (sin solape) → evento válido. ✅
5. **Conflicto entre recursos.** Tv + Datashow en el mismo evento → `"No se pueden emplear Tv's cuando están utilizándose en el evento Datashows"`. ✅
6. **Exceso de cantidad.** `Salones x99` (total 5) → `"Introdujo una cantidad que supera la cantidad disponible de Salones"`. ✅
7. **Asistente de disponibilidad.** `find_space({Datashow:1}, ...)` con la franja 10–12 ya ocupada devolvió **12:00–14:00** — saltó correctamente al primer hueco libre en pasos de 1 hora. ✅
8. **Persistencia round-trip.** `SaveData` → `Events.json` con fechas ISO-8601 y recursos por nombre; `LoadEvents` reconstruyó el evento idéntico con sus `datetime`. ✅ `LoadResources` también restauró cantidades.
9. **`valid_datetime`** parsea 10 formatos de fecha y devuelve `False` ante basura. ✅ (pero ver Dim. 4 — es código muerto).

**Bug real encontrado (menor):** `valid_event` **no valida que `end >= start`**. Ejecuté `valid_event("Boda", 12:00, 10:00, {Salon:1})` y devolvió un evento válido con fin anterior al inicio. En la UI hay un `st.error` si `end < start` (`event_creation.py:106`), pero **ese chequeo no pone `valid_dates = False`** — la bandera `valid_dates` solo se mueve por disponibilidad (`event_creation.py:118-127`). Es decir, un usuario podría confirmar un evento con fin anterior al inicio. En la práctica los defaults del widget lo hacen improbable, pero el guard falta en ambas capas.

**Efecto secundario a vigilar:** `resource_availability` (`planification.py:47`) muta `resour.in_use` sobre el objeto compartido del inventario y lo resetea a 0 al final. Funciona en un solo hilo, pero es frágil: si el reset se saltara por una excepción, el inventario quedaría corrupto. Preferible calcular en una variable local sin tocar el objeto.

No observé ningún `Traceback` en los flujos ejercitados. `py_compile` de los 9 módulos: **OK**.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Argumento mutable por defecto:** `needed_resources: dict = {}` en `event.__init__` (`events.py:13`) y `valid_datetime` reutiliza `dict[resource:int]` como anotación — el `{}` por defecto es el clásico footgun de Python (se comparte entre instancias). Aquí no causa daño porque siempre se pasa un dict, pero conviene usar `None` + inicialización dentro.
- **Código muerto:** `valid_datetime` (`planification.py:111-131`) no se importa en ningún sitio (la UI usa widgets de fecha, no parseo de texto); `core/core.py` está vacío. Borrarlos aclara el proyecto.
- **Sombreado de variable:** en `valid_event` (`planification.py:37`) el bucle interno reusa el nombre `amount` que ya viene del bucle externo (`planification.py:26`), pisando el valor. Aquí no rompe porque el flujo no lo reutiliza después, pero es una trampa esperando a alguien.
- **Cadenas con continuación de línea (`\`)** dentro de f-strings de mensajes (`planification.py:31`, etc.) meten espacios en blanco espurios en el texto mostrado. Mejor `st.error` en varias líneas o strings normales.
- Nombres de clase en minúscula (ya mencionado). El resto de nombres son descriptivos y en buen español/inglés mixto consistente.

Nada de esto es grave para un primer año; son pulidos.

## Dimensión 5 — Datos y persistencia

Modelo sólido y bien pensado:
- **Recursos como grafo:** dependencias y conflictos entre recursos convierten el inventario en un pequeño grafo de restricciones — una idea ambiciosa y bien implementada para este nivel.
- **Serialización:** `event.to_dict`/`from_dict` (`events.py:22,36`) convierten `datetime` ↔ ISO-8601 y las llaves recurso↔nombre. `LoadEvents` (`_init_.py:63`) reconstruye las llaves-recurso resolviéndolas contra el `Inventory` global. Verifiqué el round-trip completo: funciona.
- **Detalle:** el `Events.json` de ejemplo del repo tiene un evento con `beginning == end` (evento de duración cero) — coherente con la falta de guard `end > start`.
- El informe afirma haber implementado "**un encoder y decoder personalizados**" para JSON (report.md §3.5). Esto **sobreestima**: no hay una subclase de `json.JSONEncoder` ni un `object_hook`; simplemente se usa `isoformat()`/`fromisoformat()` dentro de `to_dict`/`from_dict`. Es una solución correcta y suficiente, pero no es un "encoder personalizado" en el sentido técnico del término.

## Dimensión 6 — Informe (`report.md`)

El informe es de buena calidad: bien estructurado, en buen español técnico, con 2210 palabras (supera el mínimo; el bot vio una versión previa de 1654). Cubre dominio, diseño, aprendizajes, guía de uso y dificultades. La verificación de solapamiento (`inicio1 < fin2 and inicio2 < fin1`) descrita en §6.1 **coincide exactamente** con el código.

Discrepancias detectadas:
1. **"encoder y decoder personalizados" (§3.5)** — sobreestima; ver Dim. 5.
2. **Nombre "ÁGORA"** (§5, guía de uso): el informe llama a la app "la interfaz de ÁGORA", pero ese nombre **no aparece en ningún lugar del código** — la UI se titula "Gestor de Eventos" (`main.py:29`). Inconsistencia de branding.
3. **Nombres de clase** en el informe (`Resource`, `Event`, `Planification`) no coinciden con el código (minúsculas). Menor.
4. El informe declara "Python 3.13.5" y "Streamlit 1.53.1" (README) mientras `pyproject.toml` pide `streamlit>=1.50.0` y `requires-python>=3.10`. Ninguna contradicción grave.

En general el informe **describe fielmente** lo que hace el sistema; las exageraciones son puntuales y menores.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y por encima del promedio** para un primer año. El estudiante tomó la decisión, correcta y madura, de separar el dominio de la interfaz — y lo hizo de verdad: el motor de planificación es testeable de forma aislada, sin Streamlit, algo que verifiqué ejecutándolo. El corazón del proyecto (detección de solapamientos temporales, disponibilidad de recursos por franja horaria, dependencias y conflictos entre recursos, y una verificación de integridad al reducir inventario) **funciona correctamente en todos los escenarios que probé**, válidos e inválidos. La ambición del modelo de recursos-como-grafo es notable a este nivel. Los defectos son de pulido: un guard `end > start` faltante en la lógica, código muerto (`valid_datetime`, `core/core.py`), `pandas` no declarado como dependencia, y un par de exageraciones menores en el informe.

**Principal fortaleza:** la arquitectura limpia dominio/UI, con un motor de negocio correcto y testeable de forma independiente — el solapamiento temporal, las dependencias y los conflictos funcionan de verdad.

**Principal área de mejora:** cerrar los cabos sueltos de robustez — validar `end > start` en `valid_event` (y hacer que la UI bloquee de verdad esa condición), declarar `pandas` en `pyproject.toml`, y eliminar el código muerto para que el proyecto sea tan limpio por dentro como su diseño lo promete.
