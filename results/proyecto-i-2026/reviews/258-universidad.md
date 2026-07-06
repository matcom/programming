# Revisión detallada — Issue #258

- **Estudiante:** Liber Chaos González
- **Grupo:** C-111
- **Repositorio:** https://github.com/Liibbeerr/Universidad
- **Descripción del issue:** organizador de eventos de un local de videojuegos: reservar para jugar en consolas, computadoras y juegos de mesa.
- **Clon local:** `.playground/proyecto1-eval/repos/258-universidad`

## Nota metodológica importante

El proyecto **no es una aplicación de consola** (`main.py` + menú `input()`), sino una **aplicación web con Streamlit**. La rúbrica asume consola, pero aquí el punto de entrada es `Main.py` y se ejecuta con `streamlit run Main.py`. Adapté la ejecución dinámica en consecuencia: arranqué el servidor Streamlit headless y, además, recorrí todas las páginas y ramas del formulario con el arnés oficial `streamlit.testing.v1.AppTest`, que ejecuta el script real y captura excepciones. Esto es el equivalente fiel a "recorrer el menú" para una app Streamlit.

## Ejecución dinámica realizada

Entorno aislado con `uv venv` (Python 3.11) + `uv pip install streamlit pandas` (streamlit 1.59.0, pandas 3.0.3). El `venv/` versionado en el repo es de Windows (`pyvenv.cfg` apunta a `C:\Users\Liber\...`) y no sirve en Linux; no hay `requirements.txt`.

Pasos y observaciones:

1. **Compilación** de los 7 módulos con `py_compile`: los 7 compilan sin errores de sintaxis.
2. **Arranque headless** `streamlit run Main.py --server.headless true`: el servidor levanta limpio, `GET /` → HTTP 200, `/_stcore/health` → HTTP 200. Sin Traceback en el log.
3. **Recorrido de páginas vía `AppTest`** (`Main.py:15` menú lateral):
   - **Inicio** (`Inicio.py:3`): renderiza sin excepciones.
   - **Calendario** (`Calendario.py:6`): renderiza la tabla desde `Fecha.json` con `pd.read_json` y muestra el formulario de eliminación. Sin excepciones.
   - **Reservar** (`Reservas.py:10`): renderiza el formulario. Probé las **tres ramas** del selector "¿Qué desea Jugar?":
     - Juegos de mesa 🎲 → `Juegos_mesa()` — OK, sin excepción.
     - Consolas 🎮 → `Consolas()` — OK, sin excepción.
     - Computadoras 💻 → `Computadora()` — OK, sin excepción.
4. **Submisión válida end-to-end** (mesa, nombre `prueba_valida`, contraseña `abcd`, 2 personas, 10:00–12:00, juego "Uno" en "Mesa para 4"): salieron los mensajes `Hora valida...`, `Reserva válida: Uno en Mesa para 4 con Ninguno` y `Reserva guardadada correctamente`; y **la reserva se persistió correctamente** en `Fecha.json` (pasó de 1 a 2 registros). Restauré el archivo tras la prueba.
5. **Submisión rechazada** (mismo formulario con el default D&D + 2 personas): el sistema mostró `Para jugar D&D tiene que haber un mínimo de 3 personas` y **no** guardó nada. La compuerta de validación funciona.
6. **Casos límite de `Ver_hora`** (`Validaciones.py:30`) probados directamente: `10:00–14:00`→válido; `10:00–10:30`→rechaza (<1h); `09:00–11:00`→rechaza (antes de apertura); `15:00–19:00`→rechaza (después de cierre); `14:00–12:00`→rechaza (inicio>fin); `10:00–10:00`→rechaza (iguales). **Bug encontrado:** con una cadena de hora sin cero a la izquierda (`"9:00:00"`) lanza `ValueError: invalid literal for int() with base 10: '9:'`. En el flujo normal de la UI no ocurre (el `time_input` de Streamlit siempre entrega `HH:MM:SS` con cero delante), pero el parseo por posiciones fijas es frágil.

**Veredicto de ejecución: la aplicación funciona.** Arranca, las tres páginas renderizan sin fallos, las tres ramas de reserva operan, la validación rechaza casos inválidos y una reserva válida se guarda de verdad en el JSON. El único error real observado es el de `Ver_hora` con horas de un dígito, inalcanzable desde la UI.

## Dimensión 1 — Qué hace el programa

Aplicación web (Streamlit) para gestionar reservas de un salón de juegos con tres áreas: juegos de mesa, consolas y computadoras. Punto de entrada `Main.py:27` (`main()`), que arma un menú lateral con tres páginas (`Main.py:17`): **Inicio** (bienvenida y horario, `Inicio.py`), **Reservar** (formulario de reserva, `Reservas.py`) y **Calendario** (tabla de reservas + eliminación, `Calendario.py`).

El flujo principal (`Reservas.py:24-66`): el usuario elige tipo de actividad, rellena nombre, contraseña, fecha, hora inicio/fin y número de personas, selecciona los recursos concretos según la rama (`Juegos.py`) y pulsa "Reservar". Se validan nombre/contraseña (`Validar_nombre`), horas (`Ver_hora`) y reglas específicas del recurso (`Recursos_jm`/`Recursos_con`/`Recursos_pc`); si todo pasa, la reserva se serializa a `Fecha.json`. El calendario lee ese mismo JSON y permite borrar una reserva autenticando con contraseña del usuario + contraseña maestra del local (`"1234"`, `Eliminar_evento.py:18`).

## Dimensión 2 — Organización del código

Buena para primer año. El código está **repartido en 7 módulos** con responsabilidades claras, no en un `Main.py` monolítico:

- `Main.py` — enrutado del menú.
- `Inicio.py` — página de bienvenida.
- `Reservas.py` — orquestación del formulario y guardado.
- `Juegos.py` — funciones que devuelven un dict de recursos por rama (`Juegos_mesa`, `Consolas`, `Computadora`).
- `Validaciones.py` — validaciones centralizadas (`Validar_nombre`, `Ver_hora`, `Recursos_jm`, `Recursos_con`, `Recursos_pc`).
- `Calendario.py` — tabla + delega en `Eliminar`.
- `Eliminar_evento.py` — borrado autenticado.

El patrón "cada función de `Juegos.py` devuelve un dict que `Reservas.py` compone" (`Juegos.py:11`, `Reservas.py:43`) es un buen instinto de diseño. Nombres de funciones descriptivos. Puntos mejorables: alias de import poco legibles (`re_jm`, `re_con`, `re_pc`, `vh`, `vn` en `Reservas.py:6`); el bloque de apertura de `Fecha.json` con `try/except` se repite en 5 sitios (`Main.py:9`, `Reservas.py:11`, `Eliminar_evento.py:5`, `Validaciones.py:5,61,136,206`) y podría ser una sola función auxiliar; y `data` en `Main.py:9-13` se carga pero no se usa (es código muerto, la carga real está dentro de `Reservas()`).

## Dimensión 3 — Corrección funcional (ejecución real)

Ver "Ejecución dinámica". Resumen: **arranca y funciona**. Las tres páginas renderizan sin excepción; las tres ramas de reserva operan; validación de horario, capacidad, exclusividad de juegos por consola (`Validaciones.py:167-180`) y compatibilidad juego↔gama de PC (`Validaciones.py:247-262`) están implementadas y se disparan; una reserva válida persiste y una inválida se bloquea. Coincide con lo que promete el issue.

Defectos concretos:
- **Bug real (borde):** `Ver_hora` (`Validaciones.py:31-37`) parsea la hora por posiciones fijas (`h1[:2]`, `h1[3:5]`, `h1[6:]`); con una cadena no cero-rellenada (`"9:00:00"`) lanza `ValueError`. Inalcanzable desde la UI, pero frágil. Solución: `datetime.time.fromisoformat(h1)` o `map(int, h1.split(":"))`.
- **Chequeo de solapamiento parcial:** `Recursos_jm/con/pc` sólo detectan conflicto cuando la reserva existente **cubre completamente** el horario nuevo (`hora_j1 <= hora_h1 and hora_j2 >= hora_h2`, `Validaciones.py:84,161,230`). Dos reservas que se solapan parcialmente (p.ej. 10–12 y 11–13) **no** se detectan como conflicto. El propio informe reconoce esta simplificación; para 1er año es aceptable, pero conviene señalarla.
- **Falta advertencia bloqueante:** en `Recursos_jm:102-103`, cuando hay <2 personas para un juego que no es D&D se muestra un `warning` pero **no** se hace `return False`, así que no bloquea la reserva. Inconsistente con el resto.
- El informe menciona `st.experimental_rerun()` tras eliminar, pero **no aparece en el código** (`Eliminar_evento.py` no re-ejecuta); la tabla se refresca sólo al recargar.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Sólidas para el nivel. Indentación consistente; uso idiomático de `f-strings` (`Validaciones.py:57,103`); `try/except (FileNotFoundError, JSONDecodeError)` para lectura robusta del JSON; escritura con `indent=4` y `ensure_ascii=False` en el borrado (`Eliminar_evento.py:39`). Puntos a pulir:
- Duplicación evidente del bloque de carga de `Fecha.json` (5+ veces) — extraer a una función.
- Aliases crípticos en imports (`Reservas.py:6`).
- En `Eliminar_evento.py:40-42` se llama `st.success("Reserva eliminada correctamente")` **dos veces seguidas** (línea duplicada).
- Modificar la lista `data` mientras se itera sobre ella con `data.remove(i)` dentro del `for i in data` (`Eliminar_evento.py:33-37`) es un antipatrón; aquí no rompe porque hace `return` inmediatamente tras remover, pero es un hábito a corregir.
- Al guardar en `Reservas.py:64` no usa `ensure_ascii=False`, por eso `Fecha.json` guarda `ñ` escapado (mientras el borrado sí lo desescapa): inconsistencia menor de codificación.

## Dimensión 5 — Datos y persistencia

Razonable. Estado en una **lista de diccionarios** serializada a `Fecha.json` (`Reservas.py:64`), estructura adecuada para el dominio. Persiste correctamente: verifiqué en ejecución que una reserva nueva se añade y una eliminada desaparece del archivo. También usa `st.session_state` para conservar reservas entre recargas (`Reservas.py:17`). Detalle: la clave es la hora truncada a la hora entera (`int(h1[:2])`) para los chequeos de conflicto, perdiendo minutos; funciona para el modelo simple pero limita la precisión.

## Dimensión 6 — Informe (`report.md`)

El repo trae el informe en un archivo llamado `README` (sin extensión), no `report.md`. Es **extenso, bien redactado y en general fiel** al código: describe correctamente las tres áreas, las validaciones, el doble password de borrado y la persistencia en JSON. Discrepancias detectadas:
- **Nombre del entry point:** el informe dice ejecutar `streamlit run app.py`, pero el archivo se llama `Main.py`. Un profesor que siga el informe al pie no arrancaría la app.
- **`st.experimental_rerun()`** se describe como implementado tras eliminar, pero no está en el código.
- El informe presenta el chequeo de solapamiento como resuelto con lógica de intervalos `[inicio, fin]`, pero el código real sólo cubre el caso de contención total (más simple de lo narrado). Es una leve **sobreestimación**, aunque el propio texto luego admite la simplificación.

En conjunto el informe describe de verdad lo que hace el programa, explica diseño/uso/dificultades con honestidad y sólo sobreestima en detalles menores.

## Síntesis

Trabajo **notable para un primer proyecto de 1er año**. El estudiante fue más allá de la consola pedida y entregó una app web funcional, modular y con una capa de validación de reglas de negocio genuinamente rica (exclusivos por consola, gamas de PC, capacidades de mesa, D&D). Ejecutada de verdad, arranca y hace lo que promete. Las debilidades son de robustez y consistencia (parseo frágil de horas, solapamiento parcial no detectado, duplicaciones), todas normales al nivel y con arreglos claros. Principal fortaleza: alcance y organización. Principal mejora: robustez del parseo/validación y limpiar duplicaciones.
