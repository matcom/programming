# Reporte detallado — Proyecto I (271)

- **Estudiante:** Orlando Valdes Montejo
- **Grupo:** C121
- **Issue:** #271
- **Repo:** https://github.com/Orlih226/Proyecto_25-26_Archivos_De_Hallowneest
- **Descripción del issue:** "gestión de eventos y recursos a lo largo del tiempo"
- **Clonado:** OK (depth 1)

## Resultado de la ejecución dinámica

El proyecto **no es una app de consola**: es una **aplicación gráfica** con `customtkinter` + `tkcalendar`. El punto de entrada es `Main.py:897-899` (`r1 = tk.CTk(); r2 = SegundaVentana(r1); r1.mainloop()`).

Pasos realizados:

1. Entorno aislado con `uv venv` (Python 3.12) e `uv pip install -r requirements.txt`. Se instalaron `customtkinter==5.2.2`, `tkcalendar==1.6.1` y transitivas (babel, darkdetect, packaging). Sin errores de instalación.
2. **Lanzamiento GUI** (`python Main.py`): abortó con un fallo de **XCB/threading** del servidor X, no del código del alumno:
   ```
   [xcb] Unknown sequence number while appending request
   [xcb] You called XInitThreads, this is not your fault
   python: ../../src/xcb_io.c:166: append_pending_request: Assertion `!xcb_xlib_unknown_seq_number' failed.
   ```
   Esto ocurre porque el único display disponible en el entorno de evaluación (`:0`) está ocupado por otro cliente X; `customtkinter`/`tkcalendar` invocan `XInitThreads` y chocan. **No hay Xvfb** disponible y no se instaló (requeriría `sudo`). El traceback es 100% de la capa X, nunca del código del estudiante. Se repitió incluso construyendo los widgets sin `mainloop()` (mismo abort de xcb), confirmando que es un conflicto de entorno, no un bug del proyecto.
3. **Verificación de la lógica de negocio por ejecución real.** Como el motor de validación/colisión es Python puro e independiente de la GUI (`Trabajar.py`, `TrabajoConDateTime.py`, `TrabajoConJson.py`, más los métodos de procesamiento de datos de `Main.py`), se ejecutó directamente alimentando eventos con el mismo formato que produce la interfaz. Todo dentro del `.venv`. Resultados observados:

   | Test | Qué se probó | Resultado |
   |------|--------------|-----------|
   | 1 | Carga de `Inventario.json` y `EventosEjec.json` (`TrabajoConJson.py:3`) | OK — devuelve dicts correctos |
   | 2 | Detección de solapamiento horario (`TrabajoConDateTime.py:4`) | OK — `12-14 vs 13-15 → False` (colisiona), `12-13 vs 14-15 → True` (libre) |
   | 3 | Inserción de un evento válido en DB vacía (`Trabajar.py:8` `Entrada`) | OK — entra a `{"2025":{"1":[...]}}` con estructura anidada año→mes correcta |
   | 4 | Segundo evento que colisiona en recursos+hora | OK — se marca inválido y `Validar` (`Trabajar.py:36`) **lo reprograma recursivamente** de `12:30-13:30` a `13:00-14:00` (justo tras el evento en conflicto) |
   | 6 | Dos eventos mismo día/lugar/recurso sin choque de hora (`08-09`, `10-11`) | OK — ambos entran |
   | 7 | Evento con lista de días `[1,2,3]` | OK — se expande a 3 eventos individuales |
   | 9 | Hora malformada `99:99` | Lanza `ValueError` — pero en la GUI está envuelto en `try/except` (`Main.py:678-694`), así que no tumba la app: se registra en `self.errores` |

   La lógica central **funciona y hace lo que promete el issue**: gestiona eventos en el tiempo con recursos, detecta colisiones y reajusta horarios. Es trabajo genuinamente no trivial para 1er año.

## 1. Qué hace el programa

Aplicación de escritorio para **planificar eventos de un reino** (ambientación *Hollow Knight*), donde cada evento consume recursos (personajes/brigadas) en un lugar, con tipo y franja horaria. El flujo (`Main.py`):

- **Interfaz 1 (Menú):** calendario (`Main.py:32`); al seleccionar un día lista los eventos de ese día (`ListarEventos` `Main.py:281`) y permite borrar por día/mes/año (`Main.py:313-341`).
- **Interfaz 2 (Crear):** captura hora inicial/final, año, meses y días (con una gramática propia de correspondencia línea-mes ↔ línea-día), tipo (checkbox), personajes y lugar; valida datos, restricciones de inventario y colisiones (`crear_eventos` `Main.py:423`).
- **Interfaz 3 (Resolución de colisiones):** muestra válidos/inválidos/reajustados con un gráfico de pastel dibujado a mano en un Canvas (`hacer_Grafico_Pastel` `Main.py:755`).
- **Interfaz 4 (Inscripción selectiva):** el usuario elige por índice cuáles reajustes inscribir; usa **búsqueda binaria** (`BusquedaBinaria4` `Main.py:874`).

Persistencia en `EventosEjec.json` (base "activa", arranca vacía) e `Inventario.json` (restricciones de personajes/lugares, de solo lectura).

## 2. Organización del código

- Separación en módulos con responsabilidad clara: `TrabajoConJson.py` (I/O), `TrabajoConDateTime.py` (tiempo), `Trabajar.py` (motor de colisión), `Main.py` (GUI + orquestación). Buen instinto de arquitectura para 1er año.
- Uso de **clases**: `SegundaVentana` (`Main.py:10`), `RequisitosDeEventos` (`Trabajar.py:6`), `ManejoDeTiempo` (`TrabajoConDateTime.py:2`), `ExtraerJson` (`TrabajoConJson.py:2`). Cero variables `global`.
- **Puntos débiles:**
  - `Main.py:14` — `Interfaz()` mide ~220 líneas y monta las 4 interfaces de un tirón; convendría un método por interfaz.
  - Bloques enormes de widgets casi idénticos repetidos a mano: `opcion1..opcion22` y sus `.configure(command=lambda...)` (`Main.py:116-166`) podrían generarse con un bucle sobre una lista de nombres. Misma repetición en los 7 checkboxes de tipo (`Main.py:97-111`).
  - Métodos de `Trabajar.py` (`Entrada`, `Validar`, `Validacion`, `Comparador`) están definidos **sin `self` ni `@staticmethod`** (`Trabajar.py:8,36,101,138`): funcionan porque se llaman como `RequisitosDeEventos.X(...)`, pero técnicamente son métodos de instancia mal formados. Marcarlos `@staticmethod` sería lo correcto.
  - El evento es una lista posicional (`x[3]`, `x[4]`, `x[7]`…) por todo el código; una clase `Evento` o al menos constantes con nombres de índice reducirían mucho la carga cognitiva.

## 3. Corrección funcional (ejecución real)

Ver la tabla de la sección de ejecución. En resumen: la GUI no se pudo recorrer por el conflicto X del entorno, pero **el motor completo se ejecutó y pasó todos los casos probados** (carga JSON, detección de colisión horaria, inserción, reprogramación recursiva, expansión multi-día, entrada malformada). El comportamiento coincide con lo que describen `Readme.md` y `report.md`. Validación de entradas presente y razonable: horas malformadas, meses>12, días fuera del rango del mes (`ValidarFecha` `Main.py:559` usa `calendar.monthrange`), años <2025, dobles lugares/tipos — todo genera mensajes en `self.errores` en vez de romper.

Observaciones de riesgo (no verificadas por falta de recorrido GUI, pero visibles en lectura):
- `Trabajar.py:31` usa `condicion` **fuera** del bucle donde se define (`Trabajar.py:17`); si `lista_con_eventos` viene vacía o el último `condicion` no es lista, `Validar` podría recibir un valor inesperado. En los tests con datos válidos no se disparó.
- `inscribir_seleccionados` usa `eval(text[1])` (`Main.py:852`) sobre el contenido de una caja de texto de la propia app. En este contexto el texto lo genera el programa, pero `eval` es un hábito a erradicar.

## 4. Buenas prácticas de Python (nivel principiante)

- **Bien:** f-strings idiomáticos, `try/except` alrededor de conversiones de fecha/hora, `copy.deepcopy` para no mutar la DB "fantasma" (`Main.py:432`) — muestra que entendió el problema de aliasing de listas (lo menciona en `report.md`). Comentarios abundantes.
- **A mejorar:**
  - Indentación **inconsistente** (mezcla de 1, 2, 3 espacios) en casi todos los archivos; funciona pero dificulta la lectura. Un formateador (`black`) lo arreglaría de un tiro.
  - Múltiples `except:` desnudos (`Main.py:319,495,573`, etc.) que silencian cualquier error. Preferible `except (KeyError, ValueError):`.
  - Sentencias múltiples por línea con `;` (`Main.py:82-93`, `Trabajar.py:42-43`).
  - `Dias()` (`Main.py:468`) y `Meses()` (`Main.py:479`) computan una lista filtrada `l`/`l2` y luego **retornan la original sin filtrar** (`return dias` / `return mes`) — código muerto que sugiere un filtrado que no llegó a conectarse.

## 5. Datos y persistencia

- Estructura de datos **bien pensada**: diccionario anidado `año → mes → [eventos del día]` (`report.md:58-68`), justificado por acceso O(1) a un mes/año sin recorrer todo. Es una decisión de diseño madura.
- Persistencia con `json.dump`/`json.load` (`TrabajoConJson.py`). Se guarda al cambiar de interfaz y al cerrar (`cerrar` `Main.py:241`, `salir` `Main.py:364`). Verificado que `Entrada` construye la estructura anidada correcta (tests 3, 6, 7).
- Detalle: la clave interna del JSON es `"Eventos en ejecucion"` (sin acento) mientras `report.md:60` la escribe `"Eventos en Ejecucion"` — inconsistencia menor de documentación, el código usa la correcta.

## 6. Informe (`report.md` + `Readme.md`)

Informe **honesto y detallado**, de los mejores en cuanto a describir el porqué de las decisiones (formato de evento como lista, DB anidada, copia "fantasma" estilo Fallout, aliasing de listas, migración tkinter→customtkinter, pérdida de código por `pull` mal hechos). **No sobreestima**: todo lo que afirma se sostiene en el código verificado. La sección "Experiencias" (`report.md:73-78`) es especialmente valiosa como reflexión de aprendizaje. Única discrepancia trivial: la capitalización de la clave JSON citada arriba.

## Veredicto interno

Proyecto **notablemente por encima de la media de 1er año**: motor de colisión con reprogramación recursiva funcional, GUI de 4 interfaces, estructura de datos justificada, búsqueda binaria, y un informe reflexivo y honesto. Le pesa la legibilidad (indentación caótica, `Interfaz()` gigante, repetición masiva de widgets, `except` desnudos) y algún hábito riesgoso (`eval`). No se pudo recorrer la GUI por un conflicto X del entorno de evaluación (no imputable al estudiante), pero la lógica central se ejecutó y pasó todos los casos. Trabajo sólido y ambicioso.
