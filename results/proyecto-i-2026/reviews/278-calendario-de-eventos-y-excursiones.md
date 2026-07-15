# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #278
- **Repositorio:** https://github.com/Winterwolf270/Calendario-de-eventos-y-excursiones.git
- **Estudiante:** Bryan Etzel Blanco Galbán
- **Grupo:** C-122
- **Descripción declarada:** Un planificador de eventos y excursiones sencillo y de fácil manejo.

---

## Nota metodológica importante

**No es una aplicación de consola: es una GUI de Tkinter.** El punto de entrada `main.py`
crea una `tk.Tk()` a pantalla completa con cuatro botones (Guía, Agregar, Calendario, Salir),
y cada módulo abre ventanas `Toplevel` (`guia.py:13`, `agregar.py:14`, `calendario.py:36`).
No hay `input()` en ninguna parte, así que alimentarlo con `printf` no aplica.

Adapté la ejecución así:

1. **Compilación:** `py_compile` de los cinco módulos (`main`, `logica`, `guia`, `agregar`,
   `calendario`) — todos compilan sin error.
2. **GUI headless:** intenté arrancar las ventanas bajo `Xvfb` (display virtual). El intérprete
   aborta con `xcb_io.c:166: append_pending_request: Assertion !xcb_xlib_unknown_seq_number`
   incluso con un `Label` + `Toplevel` mínimos ajenos al proyecto. Es un **fallo del entorno**
   (combinación Tcl/Tk + Xvfb de esta máquina), **no del código del estudiante**: un `tk.Tk()`
   vacío sí se crea y se destruye bien; el crash aparece al añadir cualquier widget. Dejo
   constancia de que no pude renderizar la interfaz aquí.
3. **Lógica de negocio:** como la lógica está bien separada de la GUI, la ejecuté directamente
   — `logica.calcular_ocupacion` con datos reales, y una réplica fiel del flujo de guardado
   (`agregar.py:85-144`), de limpieza (`calendario.py:11-33`) y de borrado
   (`calendario.py:151-164`) sobre el `eventos.json` real del repo. Todo lo que reporto en la
   Dimensión 3 proviene de esa ejecución.

## Dimensión 1 — Qué hace el programa

Es un planificador de reservas de excursiones con transporte limitado. El flujo real:

- **Guía** (`guia.py:12`): lee el catálogo `eventos.json` y lista los 9 eventos con nombre,
  duración y costo por persona; al pulsar uno abre un `Toplevel` con detalles (tipo, horario y
  punto de recogida) — `guia.py:38`.
- **Agregar** (`agregar.py:13`): formulario con tres `Combobox` de solo lectura — evento, fecha
  de ida (fechas futuras pre-generadas, `agregar.py:37`) y número de personas (1-20). Un aviso
  dinámico muestra la ocupación en color (verde/naranja/rojo) al cambiar evento o fecha
  (`agregar.py:51-83`). Al guardar, valida capacidad y persiste en `reservas.json`.
- **Calendario** (`calendario.py:35`): pinta el mes con `calendar.monthcalendar`, muestra
  `ocupados/2` por día, colorea (blanco/verde/rojo), navega meses, y al pulsar un día con
  reservas abre el detalle con opción de eliminar (`calendario.py:108`).

El modelo central es una **capacidad de 2 "vehículos" por día** (`logica.py:3`,
`CAPACIDAD_DIARIA = 2`), que es lo que el programa realmente controla — no un cupo de personas.

## Dimensión 2 — Organización del código

**Fortalezas.** El estudiante separó el proyecto en módulos con responsabilidades claras:
`main.py` (menú), `guia.py` (catálogo), `agregar.py` (alta), `calendario.py` (vista + baja) y
`logica.py` (regla de ocupación aislada de toda GUI). Esa última separación es la mejor decisión
de diseño del proyecto: `calcular_ocupacion` (`logica.py:5`) es una función pura y testeable, y
es precisamente lo que me permitió evaluar la corrección sin interfaz. Buen uso de `json` para
persistir catálogo y reservas en archivos separados.

**Debilidades.** No hay clases — todo es funciones y estado a nivel de módulo. El diccionario
global `eventos = {}` en `calendario.py:9` es un caché en memoria que se mantiene sincronizado a
mano con `reservas.json`, y `agregar.py:137-140` lo escribe directamente desde otro módulo; eso
acopla `agregar` a un detalle interno de `calendario` y es frágil (si el calendario no se ha
abierto, ese caché puede estar vacío o desfasado respecto al JSON). El nombre `agregar` se reusa
como variable local para la ventana (`agregar.py:14`) sombreando el propio módulo — confuso.
La función `cargar_eventos` está duplicada en `guia.py:4` y `agregar.py:8`.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Qué se corrió** (réplica fiel de la lógica sobre `eventos.json` real):

1. **`calcular_ocupacion` con lista vacía** → `(0, 2)`. Correcto: día libre, 2 cupos.
2. **Dos reservas de 1 día el mismo día** → día X: `(2, 0)`. Correcto: se llena.
3. **Tercera reserva el mismo día** → el guardado devuelve `ERROR "sin transporte"`. Correcto:
   la validación de capacidad funciona (`agregar.py:113-116`).
4. **Cálculo de costo** → Buceo $50 × 4 = **$200**, Escalada $70 × 2 = **$140**. Correcto
   (`agregar.py:104`).
5. **Limpieza automática de fechas pasadas** (`calendario.py:21-25`): con una reserva a −5 días
   y otra a +5 días, conserva solo la futura (1 de 2). Correcto.
6. **Fechas/personas inválidas**: `strptime("32/13/2026")` e `int("abc")` lanzan `ValueError`,
   capturado por el `try/except` de `guardar_evento` (`agregar.py:149`). Además, en la GUI real
   los tres campos son `Combobox` de solo lectura, así que la entrada basura es estructuralmente
   imposible — buena defensa por diseño.

**Problemas encontrados (bugs del estudiante, no del entorno):**

7. **Semántica de "arrastre" invertida** (`logica.py:17-19`). La regla suma 1 ocupación al día
   *siguiente* por cada reserva cuya `duracion` empiece por `"1 día"`. Pero en `eventos.json`
   **todos** los eventos son `duracion: 1` (un solo día). Ejecutado: dos eventos de 1 día el
   10/08 dejan el **11/08 en `(2, 0)` — bloqueado**, aunque esos eventos terminan el mismo día
   y no deberían consumir transporte del día siguiente. Al revés, un evento de `duracion: 2`
   (de un día para otro) da día X `(1, 1)` y día X+1 `(0, 2)` — el **segundo día no se cuenta**.
   Es decir, la ocupación de dos días se aplica a los eventos equivocados. El informe describe
   la intención correcta ("eventos de un día para otro ocupan dos días"), pero la implementación
   hace lo contrario porque compara contra `"1 día"` en vez de contra la duración real >1.
8. **Borrado por coincidencia (nombre, fecha)** (`calendario.py:151-154`). El filtro elimina
   *todas* las reservas que coincidan en nombre y fecha de ida. Como el modelo permite 2
   reservas del mismo evento el mismo día (capacidad = 2), ejecutado: borrar una de dos reservas
   "Buceo / 25/07" elimina **ambas** del JSON. Bug de identidad de baja frecuencia pero real.
9. **`reservas.json` inicial vacío** (`{"reservas": []}`): confirmado — al revisar el proyecto
   no hay reservas cargadas (esperado, dado el borrado automático de fechas pasadas). El informe
   ya lo advierte honestamente.

Ningún flujo produjo un `Traceback` no capturado en la lógica de negocio.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- `except Exception:` a secas en `actualizar_aviso` (`agregar.py:78`) traga cualquier error y
  lo reduce al mensaje por defecto — cómodo para la UX pero oculta fallos reales durante el
  desarrollo. En `guardar_evento` sí se muestra el mensaje (`agregar.py:149`), mejor.
- `next(e for e in eventos if e["nombre"] == nombre)` (`agregar.py:95`) lanzaría `StopIteration`
  si el nombre no existe; aquí está a salvo porque el nombre viene de un `Combobox` cerrado, pero
  conviene saberlo.
- Los emojis en botones y mensajes son un detalle simpático y no afectan la ejecución.
- Buen uso idiomático de comprensiones (`agregar.py:37`, `logica.py:12`) y de f-strings.
- El estado global mutable (`eventos` en `calendario.py:9`) es el punto menos idiomático; con
  una clase o pasando la estructura como argumento se evitaría.

## Dimensión 5 — Datos y persistencia

Dos archivos JSON separados, decisión acertada: `eventos.json` (catálogo, solo lectura) y
`reservas.json` (estado mutable). El esquema es coherente y usa `ensure_ascii=False` +
`indent=4`, así que los archivos quedan legibles. Detalle de modelado: el catálogo mezcla dos
representaciones de duración — `duracion` (int, días) para casi todos y `duracion_horas` (int)
solo para Paracaidismo (`eventos.json:38`), y el código tiene que ramificar en varios sitios
(`guia.py:44`, `agregar.py:97-102`, `guia.py:30`) para manejar ambas. Funciona, pero un solo
campo `unidad`/`duracion` uniforme habría simplificado. La reserva persiste `duracion` como
**string** (`"1 día(s)"`), mientras el catálogo la guarda como **int** — esa asimetría es la
raíz del `startswith("1 día")` del bug 7.

## Dimensión 6 — Informe (`report.md`)

El informe es largo, honesto y en primera persona; se nota el recorrido real del estudiante
(empezó en consola, migró a Tkinter, reescribió el calendario). Puntos a favor: **no exagera** —
avisa explícitamente que probablemente no haya reservas al revisar (por el borrado automático),
reconoce código que puso y quitó, y describe con precisión el modelo de capacidad por transporte.

Discrepancias / matices:

- Describe la intención de que "eventos de un día para otro ocupan dos días" (párrafo de
  Problemas), pero la implementación real hace lo contrario (bug 7, `logica.py:17-19`). El
  informe cuenta el objetivo, no lo que el código termina haciendo.
- Los encabezados arrastran literales de plantilla — `**negrita**, *cursiva*` (p. ej.
  `report.md:3, 7, 19`) — que parecen restos de una guía de Markdown y no aportan; conviene
  quitarlos.
- Ortografía del título: "Excuriones" (`report.md:1`) y "Enseñansa" (`report.md:19`).
- No afirma en ningún lado haber "demostrado" o "probado" validaciones con rigor formal — el
  tono es sobrio y realista, lo cual está bien.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido** para primer año. La ambición está bien dosificada: una GUI multi-ventana
funcional, persistencia en JSON, limpieza automática de fechas pasadas y un modelo de capacidad
de transporte con retroalimentación visual por colores. La lógica compila limpiamente y, en la
ejecución real de su núcleo, las rutas principales (alta, validación de cupo, cálculo de costo,
limpieza) se comportan correctamente. Los dos bugs reales que encontré (arrastre de ocupación
aplicado a la categoría equivocada; borrado que elimina reservas gemelas) son de casos borde y
no rompen el uso normal, pero valen como aprendizaje sobre cómo la representación de los datos
(int vs. string en `duracion`) y la identidad de los registros condicionan la corrección.

- **Principal fortaleza:** la separación de la regla de negocio en `logica.py:5`
  (`calcular_ocupacion` como función pura y testeable), que demuestra buen instinto de diseño y
  hace el resto del sistema verificable.
- **Principal área de mejora:** unificar la representación de la duración (un solo tipo, mismo
  formato en catálogo y en reserva) y corregir la condición de arrastre para que refleje la
  duración real del evento; de paso, dar a cada reserva un identificador único para que el
  borrado sea inequívoco.
