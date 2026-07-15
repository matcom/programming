# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #301
- **Repositorio:** https://github.com/GuillermoRR06/Management-System--CC-.git
- **Estudiante:** Guillermo Rodriguez Rodriguez
- **Grupo:** C121
- **Descripción declarada:** Gestor inteligente de eventos de un cine-teatro

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una aplicación web construida con **Streamlit**
(multipágina: `Inicio.py` + tres páginas bajo `App/pages/`), con persistencia en un único
`Data/data.json`. No se puede alimentar con `printf` por un menú de `input()`.

Para evaluar la corrección real adapté la ejecución así:

1. **Compilación estática** de los 9 módulos con `py_compile` (todos compilan).
2. **Lógica de negocio aislada:** stubbeé `streamlit` (los módulos leen
   `st.session_state["Recursos"]` / `["Eventos"]` en el nivel de módulo) y ejecuté
   directamente las funciones núcleo con los datos reales del repo: `Disponibility`,
   `AddEvent`, `Check_MC`, `Check_Evs`, `Check_Places`, `Review_*`, y el auto-agendador
   `Intellisense.Funcs`.
3. **Arranque headless de la GUI:** `streamlit run App/Inicio.py --server.headless true`.
   El servidor levanta (HTTP 200 en `/_stcore/health`), pero el *script* de la app solo
   corre cuando un navegador se conecta por websocket, así que la comprobación decisiva la
   hice llamando a `Save_Data.GetData()` sobre un **clon recién hecho** (ver Dimensión 3,
   hallazgo 1).

---

## Dimensión 1 — Qué hace el programa

Gestor de eventos para un cine-teatro ficticio ("La Cuarta Pared") con tres tipos de evento
y reglas de recursos:

- **Inicio** (`App/Inicio.py`): carga `Data/data.json` a `st.session_state`, purga fechas
  pasadas (`Inicio.py:11-23`) y presenta la portada con imágenes.
- **Agregar Eventos** (`App/pages/➕ Agregar Eventos.py`): flujo guiado por tipo. Selecciona
  fecha (rango de 30 días, `:31`), hora, duración; valida colisiones de sala y personal según
  asistencia y sala; asigna personal; y si no cabe, invoca la búsqueda automática de horario.
- **Lista de Eventos** (`App/pages/📅 Lista de Eventos.py`): lista por día concreto o los 30
  días; permite ver detalles y eliminar (borrado lógico, `activo=False`).
- **Informacion de Recursos** (`App/pages/📋 Informacion de Recursos.py`): tabla de las 6
  salas y los 5 grupos de empleados leídos de `data.json`.

Las reglas de dominio están implementadas y son verificables: sala con escenario modular
para teatro/concierto (`RevResources.py:199-207`, salas 4-6), exclusividad temporal del
concierto (`Check_Evs`/`Check_MC`), cuota de limpieza/seguridad por asistencia
(`Review_PersCapacity`, `RevResources.py:154-168`), y un colchón de 30 min post-evento para
limpieza y liberación de sala (`Disponibility`, `RevResources.py:47-55`) — un detalle de
diseño muy razonable.

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** La separación de responsabilidades es clara y madura
para un primer año:

- `App/Functions/` — lógica de negocio: `AuxFuncs` (búsqueda/orden de fechas), `EventsFuncs`
  (alta/detalle/baja), `RevResources` (todas las validaciones de recursos), `Save_Data`
  (persistencia).
- `App/Intellisense/Funcs.py` — el agendador automático, aislado de la UI.
- `App/pages/` — solo capa de presentación Streamlit.

Los nombres son descriptivos, hay docstrings en casi todas las funciones, y las funciones de
validación aceptan un flag `k` para decidir si emiten `st.error` — así se reutilizan tanto en
la UI (con mensajes) como en el agendador silencioso (`k=False`). Es un patrón inteligente.

**Debilidades:**

- **Estado global por import.** `EventsFuncs.py:6-7`, `RevResources.py:6-7` e
  `Intellisense/Funcs.py:5` hacen `res = st.session_state[...]` / `evens = st.session_state[...]`
  **en el nivel de módulo**. Esto acopla los módulos al runtime de Streamlit (no se pueden
  importar sin él) y crea aliases globales frágiles. Sería más limpio pasar `events`/`res` como
  parámetros (que en `AddEvent` ya se hace parcialmente: recibe `events` pero por dentro usa el
  global `evens`, `EventsFuncs.py:10` vs `:25`).
- **Duplicación masiva en `Agregar Eventos.py`.** Los tres bloques `if selection == tiposEventos[i]`
  (líneas 26, 127, 229) repiten casi literalmente la construcción de columnas de fecha/hora/duración
  y el diccionario `necesidades`. Podría factorizarse.
- Bloques `else` con `necesidades` que referencian variables no definidas para ese tipo (p. ej.
  `opProyec` en el bloque de teatro, `Agregar Eventos.py:205`) — funciona solo porque hay valores
  por defecto globales al inicio (`:24`), pero es confuso.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

1. **BUG de portabilidad — la app no arranca en Linux/macOS.** `Save_Data.py:8,13` abren
   `"Data\data.json"` con separador de Windows. En Python la secuencia `\d` es una
   `SyntaxWarning: invalid escape sequence` y el nombre queda como el literal `Data\data.json`
   (un solo backslash), que en un sistema tipo Unix es un **nombre de archivo con backslash**,
   no una ruta a subcarpeta. Verificado sobre un **clon prístino**:
   ```
   >>> Save_Data.GetData()
   FileNotFoundError: [Errno 2] No such file or directory: 'Data\\data.json'
   ```
   Lo mismo afecta a las imágenes de portada (`Inicio.py:34,47-49`: `"App\Gallery\Logo.png"`,
   etc., con `SyntaxWarning: invalid escape sequence '\G'`) y al `st.switch_page("pages\...")`
   (`Agregar Eventos.py:305`). En Windows funciona; fuera de Windows la app cae al inicio.
   *Fix trivial:* usar `/` o `os.path.join("Data", "data.json")`.

2. **BUG de orden cronológico en `AddEvent`.** La inserción ordenada (`EventsFuncs.py:41-50`)
   está mal: la condición `if hora < tm_Init: index = i; break` inserta el evento **antes** del
   primer evento cuya hora es *menor* que la nueva. Ejecutado: partiendo de tres películas a las
   10:00 y agregando una a las 08:00, el resultado fue
   `[('F1','10:00'), ('F2','10:00'), ('F3','10:00'), ('Early','08:00')]` — la de las 08:00 quedó
   **al final**, no ordenada. La segunda rama `elif hora == tm_Init and hora.minute < tm_Init.minute`
   (`:47`) es además lógicamente muerta: si `hora == tm_Init` (objetos `time`) sus minutos ya son
   iguales, la comparación nunca es cierta. La lista solo queda "ordenada" porque la UI la muestra
   con `reversed(...)` (`Lista de Eventos.py:41`), lo que enmascara el defecto en el caso simple
   pero no en general.

3. **BUG lógico en `FindNewDay`** (`Intellisense/Funcs.py:149-159`). Para sugerir días libres hace
   `j = BS_Date(evens, newD)` y luego `if len(evens[j]) < 6`. Dos problemas verificados:
   (a) `BS_Date` devuelve `-1` cuando el día aún no existe en el calendario (lo normal para días
   futuros vacíos), y `evens[-1]` apunta al **último día existente**, no a `newD`;
   (b) `len(evens[j])` mide el **número de claves del dict del día** (`id`, `Lista_Eventos`,
   `In_Time` → siempre 3), no la cantidad de eventos, así que `< 6` es **siempre verdadero**.
   Resultado: sugiere los próximos 3 días **sin comprobar realmente** si están libres; y sobre un
   calendario vacío `evens[-1]` levantaría `IndexError`. En la ejecución no reventó porque siempre
   había al menos un día en `evens`, pero la recomendación de días es esencialmente ciega.

4. **Lo que SÍ funciona (verificado):**
   - `Disponibility` descuenta recursos correctamente: tras 3 películas 10-12 usando 2 técnicos
     de sonido c/u, quedó `sonido=0` y `salas=[False,False,False,True,True,True]`. Correcto.
   - `Check_MC` (bloqueo por concierto) y `Check_Evs` (exclusividad del concierto): tras agregar
     un concierto 15-17, `Check_MC(15:30-16:00)→False` y `Check_MC(10-11)→True`; con películas
     presentes `Check_Evs(10:30-11:00)→False` y `Check_Evs(20-21)→True`. Todo correcto, incluido
     el colchón de 30 min.
   - `AddEvent` de un evento válido, `Review_Capacity`, `Review_Scene`, `Review_PersCapacity`:
     todos devuelven lo esperado en flujos válidos e inválidos.
   - `Check_Places` (que a primera vista parecía sospechoso por `if ok not in salas` con `ok=True`)
     es en realidad **correcto**: `Check_Places([True]*6)→True`, `[False]*6→False`,
     `[True,False,...]→True`.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **f-strings con comillas anidadas iguales** en todos los `st.markdown(f"...{event["descripcion"]}...")`
  (p. ej. `EventsFuncs.py:60-69`, todas las páginas). Solo compila en Python ≥ 3.12 (PEP 701).
  En 3.11 o anterior esto es un `SyntaxError`. Conviene usar comillas simples dentro o distintas.
- `from datetime import *` (varios archivos) — el import con `*` es desaconsejado; mejor explícito.
- Recreación de objetos `time` con aritmética manual de horas/minutos repetida por todas partes
  (`Disponibility`, `Funcs`, `Agregar Eventos`). `datetime + timedelta` haría esto en una línea y
  evitaría errores de acarreo (de hecho hay un caso de acarreo de hora >23 tratado ad hoc).
- Números mágicos (`8`, `6`, `[True]*6`) para la plantilla de recursos en `Disponibility:22-29`
  en vez de leerlos de `data.json` (ver Dimensión 5).
- Manejo de errores por validaciones explícitas + `st.error`: adecuado y legible para el nivel.
  No hay `try/except` innecesario. Bien.

## Dimensión 5 — Datos y persistencia

Persistencia en un único `Data/data.json`, cumpliendo el requisito de "un solo archivo".
El modelo del **calendario** es bueno: lista de días `{id: (y,m,d), Lista_Eventos: [...], In_Time: bool}`
ordenada y consultada con **búsqueda binaria** (`BS_Date`) y **merge sort** propio (`Sort_Dates`) —
uso apropiado de estructuras vistas en clase, y ambición notable.

**Discrepancia de modelo detectada:** el bloque `Recursos.humanos` de `data.json` usa claves
**con tildes** (`"técnicos de sonido"`, `"operadores de proyección"`, `"personal de limpieza"`),
pero `Disponibility` (`RevResources.py:22-29`) **no lee ese bloque**: hardcodea su propia plantilla
con claves **sin tildes** (`"tecnicos de sonido"`, ...). Es decir, las cantidades de empleados que
la lógica de disponibilidad usa (6, 6, 6, 8, 8) están **duplicadas** en el código y desconectadas del
JSON: cambiar el inventario en `data.json` no cambiaría la validación. La página de recursos, en
cambio, sí lee el bloque `humanos` con tildes (`Informacion de Recursos.py`), así que la UI y la
lógica leen fuentes distintas. Funciona por coincidencia numérica, pero es una fragilidad real.

Detalle menor: en `AddEvent` el `id` del día se crea como **tupla** `(y,m,d)` (`EventsFuncs.py:30`),
pero tras `json.dump`/`load` vuelve como **lista**; el código lo maneja indexando `["id"][0..2]`, así
que no rompe, pero conviene ser consciente.

## Dimensión 6 — Informe (`Informe.pdf`)

Informe de **18 páginas**, en LaTeX, muy cuidado: resumen, restricciones, diseño con diagrama de
arquitectura, implementación archivo por archivo con listings, resultados (capturas) y conclusiones.
La redacción es honesta y con voz propia (la anécdota del cine Chaplin es un buen toque).

- **No sobrevende la validación:** los "Resultados" son capturas de la app en uso; **no** afirma
  pruebas automáticas ni "se demuestra/prueba" formalmente. Correcto.
- El resumen la llama "aplicación de software **robusta** que permite cumplir **todos** los
  objetivos". Es una ligera sobreestimación a la luz de (a) el bug de ruta que impide arrancar
  fuera de Windows y (b) el orden cronológico y la recomendación de días defectuosos — aunque en el
  entorno Windows del autor la experiencia sí es funcional.
- **Transparencia notable:** el Listing 1 del informe reproduce el propio código con el chequeo
  `"Events" not in st.session_state` — que es un **typo** (debería ser `"Eventos"`, la clave que sí
  se guarda, `Inicio.py:6` vs `:26`). El informe hasta explica *por qué* añadió ese guard (datos que
  no cargaban al cambiar de página). El typo hace que la condición sea siempre verdadera, lo que
  recarga el JSON en cada página; funciona, pero no por la razón que el autor cree.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**. La arquitectura es lo más fuerte: separación limpia
UI/lógica/persistencia/agendador, buenos nombres, docstrings, y estructuras de datos no triviales
(búsqueda binaria + merge sort propios sobre un calendario). Las reglas de dominio del cine-teatro
están correctamente implementadas y las verifiqué ejecutando la lógica con datos reales: colisiones
de sala/personal, exclusividad del concierto y cuotas por asistencia funcionan. El informe es
extenso, honesto y bien estructurado.

Lo que impide llamarlo redondo son tres bugs concretos hallados al ejecutar: (1) rutas con separador
de Windows que **impiden arrancar la app en Linux/macOS** (fix de una línea), (2) el orden cronológico
de eventos en `AddEvent` no ordena de verdad, y (3) `FindNewDay` recomienda días sin comprobar su
disponibilidad. Ninguno es de diseño profundo; son deslices de implementación muy corregibles.

- **Principal fortaleza:** organización del código y modelado de datos — separación de
  responsabilidades clara y uso apropiado de estructuras/algoritmos vistos en clase, con reglas de
  dominio correctamente implementadas y verificadas en ejecución.
- **Principal área de mejora:** la portabilidad de rutas (`Save_Data.py`, imágenes, `switch_page`)
  usando `/` u `os.path.join` para que la app arranque en cualquier SO — es el defecto de mayor
  impacto y el de arreglo más barato.

**Veredicto:** sólido.
