# Reporte de evaluación — Proyecto I (#257)

- **Estudiante:** Eduardo Alexis Vazquez Menéndez
- **Grupo:** C111
- **Repo:** https://github.com/Sajajima10/Gestor_Eventos
- **Descripción del issue:** "gestor de eventos de un laboratorio en la terminal"
- **Clonado:** OK (`--depth 1`)
- **Ejecución:** OK — Python 3.13.1 en venv aislado con `uv`; sin dependencias externas (`requirements.txt` solo pide `Python >= 3.6`).

---

## 1. Qué hace el programa

Es un **Sistema de Planificación CIEA**: un gestor de consola para reservar eventos
en un laboratorio de investigación, asignándoles recursos (instalaciones, personal,
hardware, suministros) y validando que la reserva respete un conjunto amplio de
reglas de seguridad. El punto de entrada es `main.py:167` (`if __name__ == "__main__"`),
que se ejecuta con `python main.py` desde la raíz del repo. El flujo es un bucle de
menú (`main.py:21-166`) con 7 opciones: listar eventos, crear, buscar próximo hueco
disponible, eliminar, ver inventario, modificar y salir.

El estado inicial vive en `data/inventory.json` (50 recursos + 2 eventos precargados
`EV-02`, `EV-03`) y las reglas de negocio en `data/rules.json`. El motor de reglas
(`core/logic.py`) es sorprendentemente rico para un proyecto de primer año: valida
conflictos de horario, inclusiones (`required` / `required_any`), exclusiones mutuas,
mínimos por categoría, y "master rules" (duración > 4h exige sala de control; recursos
de riesgo "Crítico" exigen kit de emergencia). Todo eso parametrizado en JSON, sin
tocar el Python.

## 2. Organización del código

Muy por encima de lo esperable en primer año. El código está dividido en paquetes con
responsabilidad única:

- `core/models.py` — clases de datos `Event` (`models.py:3`) y `Resource` (`models.py:20`).
  `Resource` guarda atributos variables en un `**kwargs` → `self.attributes` (`models.py:21-26`),
  decisión limpia para no rigidizar el esquema.
- `core/persistence.py` — clase `Data_Manager` (`persistence.py:5`), único responsable de
  I/O JSON: `load_data` (`persistence.py:17`) y `save_all_data` (`persistence.py:38`).
- `core/logic.py` — clase `Scheduled` (`logic.py:8`), el "cerebro": `check_availability`
  (`logic.py:14`), `validate_rules` (`logic.py:29`), `add_event` (`logic.py:87`),
  `find_next_gap` (`logic.py:106`), `update_event` (`logic.py:133`).
- `utils/error.py` — jerarquía de excepciones con base común `CIEAPlannerError`
  (`error.py:1`) y cinco subclases específicas.
- `main.py` — solo interfaz de consola; la lógica no está mezclada con los `print`.

Nombres claros y en español coherente. Uso correcto de clases donde el dominio lo pide
(no clases forzadas). Separación presentación/lógica/persistencia bien lograda. La única
nota menor de higiene: `core/logic.py:4-5` importa `datetime` dos veces, y
`logic.py:1-2` importa `json`/`Resource` que no se usan en ese módulo.

## 3. Corrección funcional (basada en ejecución real)

Ejecuté el programa en un venv aislado y recorrí **todas** las opciones del menú con
inputs por stdin y `timeout`. Antes de cada prueba mutante respaldé
`data/inventory.json` y lo restauré después. Resultados:

- **Arranque + menú (opciones 1, 5, opción inválida, 7):** OK. Lista los 2 eventos
  ordenados por fecha con nombres de recursos resueltos (`main.py:33-53`), muestra los
  50 recursos del inventario (`main.py:101-104`), responde "Opción no válida" a un `99`,
  y sale limpio. Sin `Traceback`.
- **Crear evento válido** (`03,48,15`, opción 2): OK → `✅ ¡ÉXITO! Evento 'Experimento
  Vacio' creado con ID EV-04`. Se persistió al JSON conservando los 50 recursos.
- **Motor de reglas — inclusión:** creé `03,15,05` (el ejemplo que el informe declara
  exitoso) y **falló correctamente**: `⚠️ ERROR DE PLANIFICACIÓN: La Cámara de Vacío
  Cuántico requiere la activación de la Bomba de Vacío Industrial (48)`. Es decir, el
  código es *más* correcto que el informe (ver §6).
- **Conflicto de horario:** `01,13,11` solapando `EV-03` → detectó el conflicto de
  recurso `01` (`logic.py:14-27`). OK.
- **Exclusión / categoría / fecha inválida / fin<inicio / duración no numérica:** todos
  producen el mensaje de error esperado, **ninguno lanza `Traceback`**. Los `try/except`
  de `main.py` capturan `CIEAPlannerError` y `ValueError` por separado.
- **Buscar hueco (opción 3):** `2h` para el recurso `01` → `✅ Hueco encontrado:
  2026-07-06 11:44:00` (a partir de `datetime.now()`, `logic.py:113`). OK.
- **Eliminar (opción 4):** borró `EV-02` y persistió; borrar `EV-99` (inexistente)
  responde amablemente sin romperse (`main.py:87-99`).
- **Modificar (opción 6):** renombrar `EV-03` dejando el resto en blanco → OK y persiste.
  Un cambio que viola una regla (`21,22,...`) devuelve el error **y hace rollback**: el
  evento original queda intacto en el JSON (`logic.py:147-157`). Excelente detalle.
- **ID tras vaciar el calendario:** borré todos los eventos y creé uno → generó `EV-01`
  (usa `max(..., default=0)+1`, `logic.py:95-97`), evitando el bug clásico de reusar IDs.

Comportamiento observado: **cero excepciones no controladas** en todos los caminos
probados. El programa hace lo que dice el issue y bastante más.

Bordes menores (no rompen, pero valen anotar):
- Input de recursos vacío en la opción 2 → `"".zfill(2)` produce `"00"`, y el sistema
  responde `El recurso con ID '00' no existe` (`main.py:61` + `logic.py:32-33`). No
  crashea, pero el mensaje podría ser más claro ("no ingresó recursos").
- El listado de eventos parte de fechas en memoria; no revalida reglas de eventos ya
  guardados (correcto: se validan al crear/modificar).

## 4. Buenas prácticas de Python (nivel principiante)

Muy sólido:
- Indentación consistente, `f-strings` en todos lados, bucles claros.
- Manejo de errores idiomático con jerarquía de excepciones propias
  (`error.py`) capturadas de forma centralizada (`main.py:69-72`, `82-85`).
- Sin variables globales; el estado vive en `Data_Manager`.
- `encoding='utf-8'` y `ensure_ascii=False` en el I/O JSON (`persistence.py:19,70`) —
  cuidado con acentos que muchos principiantes olvidan.
- Uso de `sorted(..., key=lambda ...)` y `divmod` para formatear duración (`main.py:33-37`).

Notas menores de estilo:
- Imports redundantes en `logic.py:1-6` (doble `datetime`, `json`/`Resource` sin usar).
- `models.py:16-18` define `__str__` para `Event` pero el `main.py` no lo usa (imprime
  campo a campo). No es un problema, solo código no aprovechado.
- Falta `utils/__init__.py` (`core/` y `data/` sí lo tienen). En Python 3.3+ funciona
  por namespace packages, pero por consistencia convendría añadirlo.

## 5. Datos y persistencia

Correcta. `Data_Manager` carga recursos como objetos `Resource`, eventos como `Event`
con fechas parseadas (`persistence.py:28-31`), y reglas como dict crudo. `save_all_data`
(`persistence.py:38-71`) reserializa todo el estado. **Verifiqué la persistencia
ejecutando**: tras crear/eliminar/modificar, el JSON refleja el cambio y —punto clave—
conserva los 50 recursos. El bug que el informe describe (atributos guardados fuera del
bucle, que reducía el inventario a 1 recurso) está efectivamente corregido: el `for` de
`persistence.py:47-55` incluye el `res_dict.update` dentro. Estructuras de datos
razonables: `dict` para recursos (acceso por ID O(1)), `list` para eventos.

## 6. Informe (`report.md`)

Muy completo y honesto: explica diseño, decisiones (JSON sobre MariaDB), uso con
ejemplos, y una sección de "dificultades" que documenta bugs reales que encontró y
arregló (el borrado de recursos, `required` vs `required_any`, IDs duplicados,
`forbidden_count > 1`, el typo `attribute`/`attributes`). Esa reflexión es excelente.

**Discrepancia informe↔código:** el ejemplo de la sección "Cómo se usa" (`report.md:88-96`)
afirma que crear un evento con recursos `03,15,05` da `✅ ¡ÉXITO!`. Al ejecutarlo,
**falla** con la regla "Vacío de Alta Potencia" (la Cámara 03 exige la Bomba 48). El
informe sobreestima ese caso concreto: el recurso `03` no puede reservarse sin el `48`.
Es una inconsistencia menor de documentación, no un fallo del código (de hecho el código
es correcto y el ejemplo del informe es el que está mal). Fuera de eso, el informe
describe fielmente lo que el programa hace.

---

## Síntesis

Trabajo **excepcional para primer año**. Arquitectura modular limpia (paquetes con
responsabilidad única, separación UI/lógica/datos), motor de reglas configurable por
JSON genuinamente no trivial, jerarquía de excepciones bien usada, persistencia correcta
y verificada, y —lo más difícil— **robustez real**: recorrí los 7 caminos del menú con
inputs válidos, inválidos y de borde, y no produje un solo `Traceback`. Incluso maneja
rollback en la modificación. El informe es reflexivo y honesto.

**Principal fortaleza:** diseño y robustez muy por encima del nivel esperado.
**Principal mejora:** limpiar imports muertos, añadir `utils/__init__.py`, y corregir el
ejemplo del informe (`03,15,05`) que no coincide con el comportamiento real.
