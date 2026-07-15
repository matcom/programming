# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #279
- **Repositorio:** https://github.com/DanielVasallo-Matcom/proyecto-programacion2026
- **Estudiante:** Daniel Alejandro Vasallo Ramos
- **Grupo:** C122
- **Descripción declarada:** Gestión de horarios en un centro educativo (Planificador de clases: asigna aulas, profesores y horarios evitando conflictos).

---

## Nota metodológica importante

Es una aplicación de **consola** con `input()` y menú numerado. Se ejecutó de verdad
en un venv aislado (`uv venv --python 3.12`, Python 3.12.8), sin dependencias externas
(solo librería estándar: `json`, `datetime`). Se recorrió el menú con `printf '...' | python main.py`
en flujos válidos e inválidos, se probó la lógica de negocio directamente (`obtener_huecos`,
`hay_conflicto`) y se hizo `py_compile` de los 5 módulos. Todo compila sin errores.

## Dimensión 1 — Qué hace el programa

Planificador docente por consola con 6 opciones de menú (`main.py:14-37`):

1. **Listar asignaturas planificadas** (`consola.py:15-21`) — muestra los eventos guardados con horario.
2. **Agregar asignatura** (`consola.py:24-86`) — flujo guiado: elegir asignatura del catálogo fijo,
   elegir profesor, confirmar requisito, ver huecos disponibles, elegir hora de inicio (8/10/12/14/16),
   validación de conflicto de aula/profesor, y si todo pasa crea el evento (duración fija de 2 h).
3. **Eliminar asignatura** (`consola.py:89-97`) por índice.
4. **Ver detalles** (`consola.py:100-111`) de un evento por índice.
5. **Asignaturas disponibles** (`consola.py:6-12`) — catálogo de 10 asignaturas con aula, profesores y requisito.
6. **Guardar y salir** — persiste a `datos.json` (`persistencia.py:12-14`).

El catálogo de 10 asignaturas está fijo en `models.py:1-52`. Cada evento persistido guarda
`asignatura`, `inicio`, `fin`, `profesor`, `aula` (`models.py:54-61`).

**Ejecución observada (flujo válido):** agregar "Analisis Matematico" → profesor "Juan" → requisito "si"
→ hora 8 produjo `Clase agregada correctamente.` y el listado mostró `1. Analisis Matematico | 08:00 - 10:00`.
Ver detalles imprimió correctamente todos los campos. `datos.json` quedó bien formado con el evento.

## Dimensión 2 — Organización del código

Muy buena para primer año. El proyecto está **modularizado con criterio**:

- `main.py` — punto de entrada y bucle de menú.
- `consola.py` — interacción con el usuario (I/O).
- `models.py` — datos del dominio (catálogo) y factoría `crear_evento`.
- `validaciones.py` — reglas puras: `hora_valida`, `hay_conflicto`, `obtener_huecos`.
- `persistencia.py` — carga/guardado JSON.

La separación **lógica de negocio vs. I/O vs. persistencia** está bien lograda: las funciones de
`validaciones.py` son puras y testeables (de hecho las probé aisladas sin problema). Los nombres son
claros y en español consistente. Ausencia total de estado global mutable compartido: los eventos se
pasan como argumento (`eventos`) por todo el flujo. Esto es notablemente más limpio que el promedio.

Debilidad menor: la asignatura, con su aula/profesor/requisito, se modela como diccionarios anidados
(`models.py:1-52`) en vez de clases. Es una elección válida a este nivel, pero una clase `Asignatura`
y otra `Evento` harían el código más explícito. No es un defecto, solo margen de crecimiento.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Qué corrí y qué observé:**

1. **Listar vacío** (`1` sin datos) → `No hay asignaturas planificadas.` Correcto.
2. **Agregar válido** (Analisis/Juan/si/8) → `Clase agregada correctamente.`, persistido correctamente.
3. **Detección de conflicto** → dos veces la misma asignatura a las 8:00: la segunda dio
   `Conflicto de aula o profesor`. La lógica de solapamiento temporal (`validaciones.py:11`,
   `inicio < ef and fin > ei`) es correcta.
4. **Requisito "no"** → `No puedes entrar al aula sin el requisito.` Correcto.
5. **Asignatura inexistente** → `Asignatura invalida`. Correcto.
6. **Profesor fuera de rango** (99) → `Seleccion invalida` (capturado por `try/except`, `consola.py:39-44`). Correcto.
7. **Hora basura** ("abc") → `Hora invalida`; **hora no permitida** (9) → `Horario invalido`. Ambos correctos.
8. **Opción de menú basura** → `Opcion no valida.` Correcto.
9. **Eliminar índice inexistente** (99) → `Seleccion invalida`. Correcto.

**Ningún `Traceback` en ninguno de estos flujos.** El manejo de entradas inválidas es sólido y consistente.

**Bugs encontrados (reproducidos):**

- **B1 — `datos.json` corrupto revienta el programa al arrancar.** `persistencia.py:9` solo captura
  `FileNotFoundError`. Con un `datos.json` con contenido no-JSON, la ejecución muere con
  `json.decoder.JSONDecodeError` sin control (traza hasta `main.py:11 → persistencia.py:8`). Basta añadir
  `json.JSONDecodeError` al `except`.

- **B2 — Índice negativo elimina/consulta el elemento equivocado en silencio.** En eliminar
  (`consola.py:93`, `int(input(...)) - 1`) y ver detalles (`consola.py:104`), escribir `0` produce
  índice `-1`, que Python interpreta como *el último elemento*. Reproducido: con "Analisis" y "Logica"
  cargadas, elegir `0` en eliminar borró "Logica" (el último) e imprimió `Logica eliminada.` como si
  fuera válido. El `try/except` no ayuda porque `-1` es un índice legal en Python. Falta validar
  `indice >= 0` explícitamente.

- **B3 — Inconsistencia huecos vs. conflicto (más UX que crash).** `obtener_huecos` (`validaciones.py:16-38`)
  calcula los huecos como un **único recurso de tiempo global**, ignorando aula y profesor. Pero
  `hay_conflicto` (`validaciones.py:6-14`) sí distingue por aula/profesor. Reproducido: tras reservar
  Analisis (Aula 1 / Juan) a las 8:00, `obtener_huecos` **deja de ofrecer las 8:00**, pero
  `hay_conflicto` confirma que Logica (Aula 2 / Maria) **sí podría** programarse a las 8:00
  (devolvió `False`). Es decir, los huecos sugeridos son más restrictivos de lo que las reglas
  realmente permiten: el usuario que se guía por los huecos se pierde slots válidos. No crashea,
  pero contradice el modelo multi-aula que el propio informe describe.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores:** buen uso de `try/except` alrededor de conversiones `int()`. Un matiz idiomático:
  `except Exception` (`consola.py:42, 61, 96, 110`) es demasiado amplio; capturar `ValueError`
  (o `(ValueError, IndexError)` en eliminar/detalles) sería más preciso y no ocultaría bugs.
- **Legibilidad:** muy buena. Nombres claros, funciones cortas, sin líneas enrevesadas.
- **Constante mágica:** `[8, 10, 12, 14, 16]` (`validaciones.py:4`) y el rango `while hora < 18`
  (`validaciones.py:27`) están duplicados como conocimiento del "día lectivo". Extraerlos a una constante
  con nombre evitaría inconsistencias futuras.
- **Duración fija de 2 h** (`consola.py:70`) — razonable como simplificación, bien documentado por el catálogo.
- Uso de `datetime.strptime`/`strftime` para formatear horas es correcto y limpio.

## Dimensión 5 — Datos y persistencia

- Modelo de datos: catálogo estático (`dict` de `dict`, `models.py`) + lista de eventos (`list` de `dict`).
  Serialización JSON con `indent=4, ensure_ascii=False` (`persistencia.py:13`) — buena elección, el
  archivo resultante es legible.
- **Discrepancia de nombre de archivo:** el repo incluye `data.json` con un evento de ejemplo, pero el
  código lee y escribe **`datos.json`** (`persistencia.py:3`). Por eso `data.json` nunca se usa: al
  arrancar sin `datos.json` el programa parte de lista vacía (verificado). No es un bug funcional, pero
  el `data.json` versionado es engañoso — o se renombra a `datos.json`, o se elimina.
- Persistencia solo al elegir "Guardar y salir" (opción 6). Si el usuario cierra con Ctrl-C se pierden
  los cambios; aceptable a este nivel, pero conviene saberlo.

## Dimensión 6 — Informe (`report.md`)

El informe es extenso, bien estructurado y en general **coincide con lo implementado**. Describe con
precisión el dominio, los eventos, recursos, restricciones (co-requisito y exclusión mutua),
funcionalidades y persistencia. Elogios: no inventa features que no existan.

Matices de honestidad:

- El informe habla de "restricciones de exclusión mutua" para recursos y de un sistema que impide
  compartir recursos entre eventos solapados. **Esto está implementado** solo para aula y profesor
  (`validaciones.py:12`); los "materiales educativos" (requisito) se validan por confirmación manual
  `si/no` del usuario (`consola.py:47-51`), no como recurso finito. El informe podría dar la impresión
  de que el material se gestiona como recurso limitado, cuando en realidad es una simple pregunta de
  confirmación. Discrepancia menor pero vale precisarla.
- El informe titula el proyecto "Planificador **Inteligente** de Eventos" y menciona que "sugiere los
  intervalos disponibles ... sin conflictos" (`report.md:90-91`). Como muestra B3, la sugerencia de
  huecos **no** respeta el modelo real de conflictos por aula/profesor, así que "sin conflictos" es
  optimista: sugiere de más restrictivamente y a la vez no garantiza ausencia de conflicto de recurso.
- El informe menciona `datos.json` como archivo de almacenamiento (`report.md:103, 124`), coherente con
  el código — pero entonces el `data.json` versionado en el repo sobra (ver Dimensión 5).

No hay exageraciones graves; el informe es honesto en lo esencial y el proyecto entregado hace lo que dice.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido** para primer año. La arquitectura está bien pensada: modularización real por
responsabilidades, lógica de negocio pura y testeable separada de la I/O, sin estado global, nombres
limpios y consistentes. Al ejecutarlo, todos los flujos válidos funcionan y — lo más difícil a este
nivel — casi todos los flujos inválidos se manejan sin reventar. Encontré tres defectos concretos: dos
menores y fáciles de corregir (crash con JSON corrupto, índice negativo que borra en silencio) y uno
conceptual más interesante (la sugerencia de huecos ignora aula/profesor y contradice el modelo de
conflictos que el propio código implementa bien en otra parte). Ninguno hunde el proyecto; el segundo
y el tercero muestran justamente el tipo de detalle que vale la pena discutir.

- **Principal fortaleza:** organización del código y separación de responsabilidades (validaciones puras
  vs. I/O vs. persistencia) — poco común de ver tan limpio en primer año.
- **Principal área de mejora:** alinear `obtener_huecos` con el modelo real de conflictos por
  aula/profesor (B3), y endurecer los dos casos borde de índice negativo (B2) y JSON corrupto (B1).
