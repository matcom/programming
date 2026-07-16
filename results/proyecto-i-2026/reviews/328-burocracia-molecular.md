# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #328
- **Repositorio:** https://github.com/yaeladeltoro/Burocracia-Molecular
- **Estudiante:** Yaela Del Toro Sosa
- **Grupo:** C122
- **Descripción declarada:** BIO-GENIX — sistema CLI de gestión y planificación de experimentos de laboratorio: administra recursos (equipos y personal), valida reglas de seguridad/co-requisito, previene solapamientos de tiempo y recursos, y persiste el estado en JSON.

---

## Nota metodológica importante

Es una **aplicación de consola** (`input()`/`print()`), no GUI. Se ejecutó recorriendo el menú con `printf '...' | python main.py`. Se probaron flujos válidos e inválidos, y se hizo `py_compile` de todos los módulos. Antes de cada corrida mutante se respaldó `datos_laboratorio.json` y se restauró después, para no ensuciar el estado del repo.

Detalle de estructura relevante: el repo contiene **cuatro** archivos `.py`, pero solo tres forman el programa real (`main.py` → `visual.py` → `planificacion.py`). El cuarto, `import json.py`, es una **copia antigua y huérfana** de `planificacion.py`: incluye su propio `menu()` y un método extra `sugerir_huecos` que nunca se usa. No se importa desde ningún sitio. Su nombre con espacio (`import json.py`) delata que fue un guardado accidental. No afecta la ejecución, pero es ruido que conviene borrar.

## Dimensión 1 — Qué hace el programa

El punto de entrada `main.py:5-8` instancia `Visual`, imprime los "eventos del día" y entra en el menú.

- **`eventos_del_dia`** (visual.py:49-63, lógica en planificacion.py:129-145): al arrancar, clasifica los eventos guardados en "comienzan hoy" y "en proceso" comparando fechas contra `datetime.now().date()`. Verificado: con el JSON del repo (eventos de febrero-abril 2026) imprime "No hay eventos hoy".
- **Opción 1 — Listar** (visual.py:21-24): recorre `self.lab.eventos` y muestra índice, nombre, intervalo y recursos. Verificado con los 4 eventos del JSON.
- **Opción 2 — Planificar** (visual.py:26-32 → planificacion.py:69-101): pide nombre, inicio, fin y recursos; valida formato de fecha, que el inicio sea futuro, que inicio < fin, las reglas de dominio y los solapamientos; si todo pasa, agrega el evento y persiste.
- **Opción 3 — Buscar hueco** (visual.py:34-37 → planificacion.py:103-127): dada una duración y unos recursos, barre hora a hora los próximos 7 días y devuelve el primer intervalo libre que respete las reglas.
- **Opción 4 — Eliminar** (visual.py:39-44): elimina por índice si está en rango y persiste.
- **Opción 5 — Salir** (visual.py:46-47).

Todo el flujo declarado en `report.md` existe y funciona.

## Dimensión 2 — Organización del código

**Fortalezas.** Buena separación en tres capas: `planificacion.py` es la lógica de negocio pura (una clase `PlanificadorLaboratorio` con métodos bien nombrados), `visual.py` es la capa de presentación (clase `Visual` con el menú), y `main.py` es un lanzador mínimo. Es exactamente la modularización que se pide a este nivel: la lógica es reutilizable y testeable sin la CLI. Los nombres de métodos (`validar_restricciones`, `hay_solapamiento`, `planificar_evento`, `buscar_hueco`) son descriptivos y en español consistente. Los comentarios y docstrings son abundantes y claros.

**Debilidades.**
- El archivo huérfano `import json.py` (copia entera de la lógica con un `menu()` y un `sugerir_huecos` sin usar) debería borrarse: confunde sobre cuál es el código vivo.
- El `pass` en visual.py:7 tras la asignación del constructor es innecesario.
- `import datetime` en visual.py:2 no se usa (la conversión de fechas ocurre en `planificacion.py`); es un import muerto.
- Un evento es un `dict` con claves mágicas (`'inicio'`, `'fin'`, `'recursos'`) repetidas por todo el código. Una pequeña clase `Evento` o el uso de esas claves centralizado reduciría errores de tipeo; para 1er año el `dict` es aceptable, es solo una observación de crecimiento.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Se ejecutaron los siguientes casos (menú por `printf | python main.py`):

1. **`py_compile` de los 4 módulos** → todos OK.
2. **Listar (opción 1)** con el JSON del repo → muestra los 4 eventos correctamente numerados (0-3). ✔
3. **Planificar válido** (`Secuenciacion ADN`, `2027-05-01 09:00`→`12:00`, `SEC01,BIO01`) → "¡Experimento planificado con éxito!" y aparece persistido como evento 4 al re-listar. ✔
4. **Co-requisito violado** (`SEC01` solo) → "ERROR: El Secuenciador requiere un Analista Bioinformático asignado." ✔
5. **Exclusión mutua** (`CRIO1,INC01`) → "ERROR: No se puede usar el Criostato y el Incubador en el mismo experimento." ✔
6. **Fecha en el pasado** (`2020-01-01`) → "Debe agendar eventos para fechas posteriores a la de hoy". ✔
7. **Formato de fecha inválido** (`ayer`/`manana`) → "Error: Formato de fecha incorrecto (Use YYYY-MM-DD HH:MM)." ✔
8. **`inicio >= fin`** invertido → "Error: La fecha de inicio debe ser anterior a la de fin." ✔
9. **Solapamiento de recursos** (dos experimentos sobre `MIC01` en horas superpuestas) → el segundo rechazado con "Error: Recursos {'MIC01'} ya están ocupados por el evento 'Exp1'." ✔
10. **Buscar hueco (opción 3)** (3h, `MIC01`) → "Sugerencia de horario: 2026-07-16 09:00". ✔
11. **Opción de menú inválida** (`99`) → re-muestra el menú sin romper. ✔

**Bugs detectados por la ejecución:**

- **[BUG-1] Recurso inexistente aceptado en silencio.** Planificar con un ID que no existe (`ZZZ`) devuelve "¡Experimento planificado con éxito!" en lugar de un error. Causa: `validar_restricciones` (planificacion.py:53) filtra los IDs desconocidos con `if rid in self.recursos`, de modo que un ID inválido simplemente no aporta ningún tipo y pasa toda validación. El evento queda guardado apuntando a un recurso que no existe (de hecho el propio `datos_laboratorio.json` del repo tiene eventos con recursos `"1"`, `"2"`, `"3"`, que no son IDs válidos — quedaron de pruebas previas). Debería rechazarse cualquier ID no presente en `self.recursos`.
- **[BUG-2] Crash con `Traceback` ante entrada no numérica en opción 3.** `float(input(...))` en visual.py:35 revienta con `ValueError: could not convert string to float: 'abc'` si el usuario escribe texto en la duración. El programa termina abruptamente.
- **[BUG-3] Crash con `Traceback` ante entrada no numérica en opción 4.** `int(input(...))` en visual.py:40 revienta con `ValueError: invalid literal for int() with base 10: 'xyz'` si el usuario escribe texto en el índice a eliminar.
- **[Menor] Retorno inconsistente de `planificar_evento`.** En el caso de fecha pasada, la función hace `raise Exception(...)` y lo captura devolviendo **el objeto Exception** (planificacion.py:81-82), no un string. Al imprimirse funciona por coincidencia (`str()` implícito), pero mezcla dos convenciones de retorno (string de error vs. objeto Exception) en la misma función. Conviene unificar a `return "Error: ..."`.
- **[Cosmético] `eventos_del_dia` imprime cabeceras vacías.** Aun cuando `alarms` está vacío e imprime "No hay eventos hoy", igual imprime a continuación "* Eventos que comienzan hoy:" y "* Eventos que aun estan en proceso:" (visual.py:53-63). Debería saltar esas secciones si no hay nada.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- El manejo de errores en la **lógica** (fechas, reglas) es cuidado y con mensajes claros — muy bien para 1er año. El punto débil es la **capa CLI**, que no protege las conversiones `float`/`int` (BUG-2, BUG-3); un `try/except ValueError` alrededor de cada `input()` numérico lo resolvería.
- `except:` desnudo en planificacion.py:73. Capturar excepciones específicas (`except ValueError`) es mejor práctica; de hecho la copia huérfana `import json.py` ya tenía esa versión mejor.
- Anotaciones de tipo puntuales y correctas (`app_visual: Visual`, `alarms: list[tuple]`) — detalle bonito y poco común a este nivel.
- Imports muertos (`import datetime` en visual.py:2) y `pass` sobrante (visual.py:7).
- No hay estado global mutable disperso: el estado vive dentro de la instancia `PlanificadorLaboratorio`. Correcto.

## Dimensión 5 — Datos y persistencia

Modelo sólido y bien pensado para 1er año. `recursos` es un `dict` `{ID: {nombre, tipo}}` y `eventos` una lista de `dict`. La persistencia (planificacion.py:37-49) resuelve correctamente el problema no trivial de serializar `datetime`: convierte a `isoformat()` al guardar y reconstruye con `datetime.fromisoformat()` al cargar (planificacion.py:23-25). El fallback a un inventario por defecto cuando el archivo no existe (planificacion.py:26-35) es una buena decisión de diseño. La guarda con `ensure_ascii=False` preserva los acentos en el JSON.

Único reparo: el `datos_laboratorio.json` versionado contiene datos de prueba con recursos inválidos (`"1"`,`"2"`,`"3"`) y un evento con nombre vacío — residuo que, combinado con BUG-1, muestra que el sistema no está validando la integridad referencial de los eventos guardados.

## Dimensión 6 — Informe (`report.md`)

El informe (471 palabras; el verificador automático marca el mínimo de 2000 como no cumplido) describe con fidelidad **casi todo** lo que hace el programa: POO, `datetime`, persistencia JSON, CLI, y las tres reglas de negocio (co-requisito, exclusión mutua, solapamiento) — todas verificadas como reales en la ejecución. No exagera funcionalidades: lo que promete, lo cumple.

Discrepancias menores:
- La sección "Estructura del Proyecto" (report.md:45-48) lista `planificador.py` y `README.md`, pero los archivos reales son `planificacion.py`, `visual.py`, `main.py`, `import json.py` y `report.md`. La estructura documentada no coincide con la real.
- El ejemplo de "Buscar hueco" (report.md:88) muestra una sugerencia en el pasado (`2026-02-03`), coherente con cuando se escribió el informe pero no con el comportamiento actual (siempre sugiere desde `now`).
- No menciona los bugs de entrada no numérica ni el hueco de validación de recursos inexistentes — normal, no se les pide autocrítica, pero conviene saberlo.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido** para primer año. La ambición está bien elegida: un planificador con reglas de dominio (co-requisito, exclusión mutua) y detección de solapamiento temporal es un problema con sustancia, y la estudiante lo resolvió con una arquitectura limpia de tres capas, persistencia JSON correcta con manejo de `datetime`, y validaciones que funcionan de verdad — lo comprobé caso por caso. Los defectos son acotados y de tipo "robustez de bordes": dos crashes por entrada no numérica en la CLI, un hueco de validación que acepta recursos inexistentes, y ruido de archivos/imports muertos. Ninguno compromete el núcleo funcional; todos son arreglables en pocas líneas.

- **Principal fortaleza:** separación de responsabilidades ejemplar (lógica de negocio reutilizable, aislada de la CLI) con reglas de dominio correctamente implementadas y verificadas.
- **Principal área de mejora:** robustez de la entrada del usuario — envolver las conversiones `float`/`int` de la CLI en `try/except` y rechazar IDs de recurso que no existan, para que el programa nunca reviente ni guarde datos inconsistentes.
