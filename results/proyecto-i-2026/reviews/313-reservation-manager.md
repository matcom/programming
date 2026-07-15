# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #313
- **Repositorio:** https://github.com/lacuentadeadrian57-netizen/Reservation-Manager
- **Estudiante:** Adrian Martinez Borrego
- **Grupo:** C121
- **Descripción declarada:** Una aplicación de manejo de reservas donde el usuario controla locales, recursos y actividades a lo largo de un período de tiempo.

---

## Nota metodológica importante

No es una app de consola: es una aplicación web **Streamlit** (`main.py:1`, `main.py:278`). La lógica de negocio está limpiamente separada de la interfaz en `manager.py` (clase `Manager`) y la persistencia en `loader.py`. Esta separación permitió **ejecutar la lógica real** de forma directa e independiente de la GUI: instancié `Manager`, cargué `save.json` y llamé sus métodos con datos reales. Además arranqué el servidor Streamlit en modo headless (`streamlit run main.py --server.headless true`) y arrancó sin errores ("You can now view your Streamlit app", sin `Traceback`).

## Dimensión 1 — Qué hace el programa

Gestiona reservas de locales con recursos asociados a lo largo del tiempo (granularidad de **día**, no de hora). El flujo:

- **Reservar** (`main.py:65-88`): eliges local, fechas y recursos opcionales; el sistema muestra los recursos por defecto (requisitos) del local, calcula el precio total (`manager.py:179-186`) y crea la reserva validando fechas y exclusiones (`manager.py:81-109`).
- **Inspeccionar** (`main.py:90-129`): tabla de todas las reservas + detalle por ID + botón para eliminar.
- **Editar locales** (`main.py:131-197`): añadir/eliminar/actualizar precio de locales.
- **Editar recursos** (`main.py:199-257`): añadir/eliminar/actualizar precio y cantidad de recursos.
- **Guardar/Cargar** (`main.py:259-276`): persistencia a JSON.

Lo más ambicioso y logrado es el **motor de resolución de colisiones** (`manager.py:48-79`): cuando una reserva nueva se solapa con otra —en el **mismo local**, o en locales distintos que compiten por un **recurso escaso**— el sistema la **reprograma automáticamente** al primer hueco libre, preservando su duración, en vez de rechazarla.

## Dimensión 2 — Organización del código

Muy buena para primer año. Tres módulos con responsabilidades claras:

- `loader.py` — E/S JSON pura, con escritura atómica vía archivo temporal + `os.replace` (`loader.py:4-16`). Un patrón sólido, poco común a este nivel.
- `manager.py` — toda la lógica de negocio en la clase `Manager`, sin dependencia de Streamlit. Excelente decisión de diseño: por eso pude testear la lógica en aislamiento.
- `main.py` — solo la capa de presentación (clase `App`) con estado en `st.session_state`.

Uso consistente de **type hints** (`manager.py:13-23`, `manager.py:81`), funciones auxiliares `code_to_id`/`id_to_code` para los identificadores `RES-NNN` (`manager.py:5-9`), y una **búsqueda binaria** propia para insertar reservas ordenadas por fecha (`manager.py:37-46`). Nombres en general claros.

Debilidades menores: los nombres de datos (`collitions`, `collide`, `_reserve`) tienen typos ("collitions" → "collisions") y algunos son crípticos; la lógica de `refresh` (`manager.py:48-79`) es densa y sin comentarios, difícil de seguir aun siendo correcta.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí la lógica de negocio directamente sobre `save.json` (3 reservas de ejemplo, 14 recursos, 4 locales). Resultados:

1. **`py_compile` de los 3 módulos: OK.** (Requiere Python ≥3.12 por f-strings con comillas anidadas en `manager.py:108` y `main.py:116-122`; con 3.12 compila sin problema.)
2. **Reserva válida:** `add_reservation(+5d, +7d, 'Conference Room', ['Whiteboard'])` → `(True, 'Reservation added!')`. Correcto.
3. **Cálculo de precio verificado:** para esa reserva el precio fue **1185**, que coincide exactamente con `(200 base + 30 Projector + 50 Screen + 100 Sound Tech + 15 Whiteboard) × 3 días`. La fórmula incluye base + requisitos + opcionales, multiplicado por (días + 1) (`manager.py:180-186`). Correcto.
4. **Flujos inválidos (todos rechazados con mensaje claro):**
   - inicio > fin → `Validation failed: the start date cannot be after the end date`.
   - inicio en el pasado → `Validation failed: the start date cannot be before the actual date`.
   - opcional inexistente → `Validation failed: the selected option NonExistent is invalid`.
   - par de opcionales excluyentes (Mirrors + Lighting controls) → `Validation failed: the selected option Lighting controls excludes Mirrors`. Correcto.
5. **Reprogramación por colisión (mismo local) — funciona.** Reserva A `[+1d, +3d]` y B `[+2d, +4d]` en 'Meeting Room': B se recolocó a `[+2d desplazado, ...]` empezando justo cuando A termina, conservando su duración de 2 días. Mensaje devuelto: `Reservation added in start: 2026-07-18, end: 2026-07-20`. Correcto.
6. **Reprogramación por recurso escaso (locales distintos) — funciona.** Con 'Ribbon Microphone' (cantidad 1), reservé 'Rehearsal Room' `[+1d,+3d]` (lo requiere) y un local nuevo que también lo requiere `[+2d,+4d]`: el segundo se reprogramó para no solaparse. Esta es la parte más difícil del proyecto y **está correcta en ejecución**.
7. **Guardar/cargar round-trip: OK.** Guardé a un archivo temporal y recargué en un `Manager` nuevo; fechas y datos idénticos.
8. **Eliminar reserva: OK.** `delete_reservation` limpia tanto `self.reservations` como `id_map` (`manager.py:111-117`).

**Bugs encontrados (del estudiante, no del entorno):**

- **B1 — Rama muerta en `update_quantity` (`manager.py:192-196`).** El código asigna `self.quantity[name] = value` (línea 194) y *luego* compara `if value < self.quantity[name]` (línea 195), que tras la asignación **siempre es False**. La intención era: "si bajaste la cantidad, recalcula colisiones", pero como ya sobreescribió el valor, `refresh()` nunca se llama. Verificado: al bajar Projector de 5 a 2, la cantidad cambia pero no se dispara la revalidación. El orden correcto sería comparar antes de asignar.
- **B2 — `delete_resource` borra locales colateralmente (`manager.py:130-152`).** Al eliminar un recurso que es requisito de varios locales, el método **elimina también esos locales** (`del self.requisites[location]`, línea 147, y no los reañade a `locations`). Verificado: al borrar 'Projector' (requisito de Conference Room, Main Auditorium, Meeting Room), quedó **solo** 'Rehearsal Room'. Comportamiento probablemente no intencionado; borrar un recurso no debería eliminar los locales que lo usan.
- **B3 — `add_resource` con exclusión inexistente lanza `KeyError` (`manager.py:125`).** Verificado con `add_resource('NewThing', 3, 10, ['DoesNotExist'])` → `KeyError: 'DoesNotExist'`. **No es alcanzable desde la GUI** (las exclusiones vienen de un `multiselect` de recursos existentes, `main.py:214`), así que es un hueco de robustez de la API, no un fallo del flujo normal.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **`except:` desnudo** en `loader.py:11` y `loader.py:14` — atrapa *todo* (incluido `KeyboardInterrupt`); mejor `except OSError` o `except Exception`. Menor.
- Type hints y separación de capas: **muy por encima del promedio** de primer año.
- Uso idiomático de `os.replace` para escritura atómica: excelente.
- El motor `refresh` mezcla varios niveles de lógica en un solo método; partirlo en helpers con nombres explicativos ayudaría a la legibilidad y al debugging (fue justamente ahí donde viven B1/B2 sin ser evidentes).
- `insert` recibe un parámetro `reservations` pero indexa contra `self.reservations` (`manager.py:39-46`) — funciona en la práctica porque siempre se pasa `self.reservations` o una lista donde el orden coincide, pero es una inconsistencia sutil que conviene limpiar.

## Dimensión 5 — Datos y persistencia

Modelo de datos claro y coherente: diccionarios paralelos (`price`, `quantity`, `exclusions`, `requisites`, `optionals`) indexados por nombre, más listas `resources`/`locations` y `reservations`. Serialización a JSON con conversión explícita de `date` ↔ ISO string (`manager.py:209-217`, `manager.py:231-242`), lo cual es lo correcto (las fechas no son JSON-serializables directamente). Escritura atómica en `loader.py`. `id_map` da acceso O(1) por ID. Round-trip verificado sin pérdida. Sólido.

## Dimensión 6 — Informe (`report.md`)

Bien escrito y organizado, pero con problemas concretos:

1. **El informe está literalmente duplicado.** Las líneas 1-179 se repiten verbatim en 180-359 (`report.md:180` reinicia con el mismo título `# **Python Reservation Manager**`). Así es como alcanzó las ~2028 palabras; el contenido único es la mitad. Probablemente un error al pegar.
2. **Menciona "horarios de apertura" (opening hours)** como restricción y funcionalidad (`report.md:95`, `report.md:274`), pero el código **no implementa** ningún concepto de horarios. Feature declarado que no existe.
3. **El ejemplo de colisión usa horas** ("10:00-12:00", "12:00", `report.md:74-76`) pero el sistema opera a **granularidad de día** (`date`, no `datetime`; `manager.py:81`). La descripción sobreestima la resolución temporal real.
4. **Ejemplos genéricos que no coinciden con los datos reales**: el informe habla de "Mesa, Sillas", "Proyector excluye Pantalla Exterior", "Altavoces excluyen Modo Silencioso" — ninguno existe en `save.json` (los reales son Whiteboard, Coffee station, Mirrors/Lighting controls, etc.). Es aceptable como ilustración, pero desconecta el informe del proyecto entregado.
5. Sí describe correctamente, en cambio, la reprogramación por colisión y por capacidad de recursos, que **sí** están implementadas y funcionan.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido** con una idea ambiciosa bien ejecutada en su núcleo. La separación lógica/GUI/persistencia es ejemplar para primer año, y el motor de resolución de colisiones —tanto por solapamiento en el mismo local como por competencia sobre recursos escasos entre locales distintos— **funciona de verdad** en ejecución, que es lo más difícil del enunciado. El cálculo de precios y todas las validaciones de entrada (fechas, exclusiones, opcionales inválidos) responden correctamente. Los defectos son acotados: dos bugs reales en operaciones de edición (`update_quantity` con rama muerta, `delete_resource` que arrastra locales), un `KeyError` no alcanzable desde la UI, y un informe duplicado con algún feature declarado de más.

- **Principal fortaleza:** el motor de reprogramación automática por colisiones y por capacidad de recursos, correctamente separado de la GUI y verificado en ejecución — un logro notable para el nivel.
- **Principal área de mejora:** las operaciones de *edición* (borrado de recursos, cambio de cantidad) tienen bugs; conviene testearlas con casos límite. Y arreglar el informe: quitar la duplicación y alinear los features declarados con lo que el código realmente hace.
