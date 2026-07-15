# Revisión detallada — Proyecto I (270)

- **Estudiante:** Brayan Miguel Rivero Horta
- **Grupo:** C121
- **Issue:** #270
- **Repositorio:** https://github.com/braiiann1/Music-Studio-Appointment-Manager
- **Descripción del issue:** aplicación de escritorio en Python con interfaz gráfica CustomTkinter para la agenda de turnos en un estudio musical.

---

## 0. Ejecución (obligatoria)

**Entorno:** venv aislado con `uv` (Python 3.12.8). Instalé `customtkinter==6.0.0` y
`pillow==12.3.0` (el `requirements.txt` del repo es un volcado completo del entorno
del sistema del estudiante —~130 paquetes: dnf, blivet, cockpit, streamlit, kivy…—
así que instalé solo las dependencias reales del proyecto).

**Punto de entrada:** `gui.py`. No usa `if __name__ == "__main__"`; construye todos
los widgets a nivel de módulo y termina con `start()` + `window.mainloop()`
(`gui.py:429-431`).

**GUI en headless:** no renderiza en este entorno (sin servidor X). Traceback real
al arrancar:

```
_tkinter.TclError: couldn't connect to display ""
   en gui.py:363  ->  window = ctk.CTk(fg_color="#1D1313")
```

Esto es esperado en un headless sin Xvfb y **no es un defecto del proyecto**.

**Ejercicio de la lógica (importando módulos):** como la capa de reglas está separada
en `validation.py` y `classes.py`, la ejercité directamente. Resultados observados:

| Caso probado | Resultado real |
|---|---|
| Import de `classes`/`validation`, `meses()`, `save_json()` | OK. Carga 6 eventos de `eventos.json` |
| Evento `single` válido (Sala A + micrófono, día libre) | `(True, 'Evento anadido con exito')` ✅ |
| `single` sin micrófono | `(False, 'Microfono Profesional es imprescindible para Grabacion de single')` ✅ |
| `single` en Sala B | `(False, 'Grabacion de single solo es posible en Sala A - Singular')` ✅ |
| Hora invertida (14:00→12:00, mismo día) | rechazado ✅ |
| Día inicio > día fin (9→5) | `(False, 'Dia inicial no puede ser mayor a dia de fin')` ✅ |
| Hora vacía `('','')` | `(False, 'Debe asignar la hora de incio y culminacion')` ✅ |
| Hora sin `:` (`1200`) | `(False, 'Formato incorrecto para la hora')` ✅ |
| Hora fuera de rango (`25:00`) | `(False, 'Formato de hora incorrecto')` ✅ |
| **Hora no numérica (`ab:cd`)** | **`ValueError: invalid literal for int() with base 10: 'ab'`** ⚠️ |
| **Día no numérico (`x`)** | **`ValueError: invalid literal for int() with base 10: 'x'`** ⚠️ |
| Colisión de recurso (Sintetizador 808, qty=1, solapado) | dispara `buscar_hueco`, devuelve tupla `forbello` con sugerencia `('10:01','16:02')` día `('13','13')` ✅ |

**Veredicto de ejecución:** el motor de validación funciona y es sorprendentemente
completo para 1er año: reglas por evento, por sala, dependencias, exclusiones,
solapamiento de horarios y agotamiento de inventario, más un mecanismo de *sugerencia
de hueco alternativo*. Los dos únicos `Traceback` reales aparecen con entradas no
numéricas — que en la GUI están mitigadas por los `validatecommand` y los
`CTkOptionMenu` (día/mes/año se eligen de listas, no se teclean), así que son difíciles
de alcanzar en uso normal, pero la función `validation()` no es defensiva por sí sola.

---

## 1. Qué hace el programa

Aplicación de escritorio (CustomTkinter) para gestionar reservas de sesiones de
grabación en un estudio musical ficticio ("Botaos Gang"). Modela tres entidades de
dominio como clases con instancias fijas en código: eventos/servicios (`classes.py:3-16`,
p.ej. "Grabacion de single", "Grabacion de Album"…), salas (`classes.py:18-30`) y
equipos de audio con un *pool* de cantidades (`classes.py:32-51`).

El flujo principal (`gui.py`): la pantalla arranca (`start()`, `gui.py:9-21`) cargando
los turnos desde `eventos.json` y pintándolos en una lista desplazable
(`render_grid`, `gui.py:166-188`). El botón "+" abre un formulario (`anadir`,
`gui.py:31-71`) con menús para evento/sala, entradas de hora, selectores de día/mes/año
y *checkboxes* de equipos. Al confirmar (`confirm`, `gui.py:73-110`) se arma el evento
como una lista y se pasa por `validation()`. Si valida, se inserta y se persiste a JSON;
si el conflicto es de inventario/horario, se ofrece un horario alternativo sugerido. Hay
además un modo "Editar" que despliega botones de borrado por fila (`editar`/`eliminar`,
`gui.py:121-137`, `gui.py:255-262`) y una vista de detalle con precio estimado
(`check_inventory`, `gui.py:203-253`).

## 2. Organización del código

Buena para el nivel. El proyecto está **dividido en tres módulos** con
responsabilidades claras:

- `classes.py` — modelo de dominio (clases `Eventos`, `Salas`, `Equipos_Audio`) y
  carga de datos.
- `validation.py` — toda la lógica de reglas de negocio (271 líneas), separada de la
  UI. Este es el mayor acierto estructural: la capa de reglas es testeable de forma
  aislada (de hecho la ejercité sin GUI).
- `gui.py` — construcción de widgets y *callbacks*.

Puntos observados:
- Uso real de **clases** para el dominio (`classes.py:3, 18, 32`), apropiado.
- Nombres en general claros, aunque **mezcla español e inglés** (`event_start_hour`,
  `buscar_hueco`, `overlapping`, `salas_opciones`), lo que resta consistencia.
- `gui.py` construye todos los widgets a nivel de módulo (`gui.py:363-427`) — funciona,
  pero mezcla configuración y estado global; una función `build_ui()` ayudaría.
- Hay **código muerto/inconsistente**: `classes.py:58 resource_pool: dict` (anotación
  sin valor), `classes.py:60-61 revisar_cantidades(): pass` (redefinida de verdad en
  `validation.py:95`), `informacion_cerrar` (`gui.py:264-267`) referencia
  `label_event_info` que puede no existir en ese scope.

## 3. Corrección funcional (basada en ejecución real)

Ver la tabla de la sección 0. Resumen:

- El **núcleo de validación funciona** y cubre muchos casos: dependencias evento↔equipo
  (`validation.py:184-223`), restricciones de sala (`validation.py:226-243`),
  exclusiones e interdependencias entre equipos (`validation.py:247-265`), orden de
  días/horas (`validation.py:162-181`) y **colisión por cantidad de inventario**
  (`revisar_cantidades`, `validation.py:95-155`).
- El mecanismo de **sugerencia de hueco alternativo** (`buscar_hueco`,
  `validation.py:21-93`) realmente corre y devuelve una propuesta —ambicioso para 1er
  año. Eso sí, produce horarios con desfase de un minuto (`10:01`, `16:02`) por los
  `timedelta(minutes=1)` de `validation.py:29, 39`; funciona pero el resultado se ve
  raro.
- **Fragilidad ante entrada no numérica** (`validation.py:25-26, 174, 96`): `int()`
  sobre texto lanza `ValueError` sin `try/except`. En la GUI está mitigado por
  `validar_start`/`vcmd` (`gui.py:274-290, 365, 399-400`) y por los `OptionMenu`, pero
  la función de validación no se protege sola.
- **Bug de precio de sala** (`gui.py:244`): al calcular el coste, el precio de la sala
  se toma de `EVENTOS[i].price` en vez de `SALAS[i].price`. El total mostrado en la
  vista de detalle es incorrecto (suma dos precios de evento).
- La rama `if/else` de `confirm` (`gui.py:98-110`): cuando `is_validated[1] ==
  "forbello"` entra al `if`, pero el `else` cuelga del `if forbello`, no del `if
  is_validated[0]`; el flujo de éxito y el de mensaje conviven de forma algo enredada.
  Funciona en la práctica pero es frágil.

## 4. Buenas prácticas de Python (nivel principiante)

- **A favor:** indentación consistente, f-strings idiomáticas, comprensión razonable de
  `timedelta`/`calendar` (`validation.py:2-3, 25, 69`), separación de capas, uso de
  `with open(...)` para I/O (`gui.py:91, 258, 314`; `classes.py:67`).
- **A mejorar:**
  - `from classes import *` y `from validation import *` (`validation.py:1`,
    `gui.py:3-4`) — el *wildcard import* dificulta rastrear de dónde viene cada nombre.
  - `print()` de depuración por toda la lógica (`validation.py:23, 30, 58-60, 87, 160`;
    `gui.py:100`) — deberían quitarse o pasar a *logging*.
  - Estado global mutable compartido (`Event_list.lista_eventos`,
    `last_suggested_event`, `inventario`, `radios`) — para 1er año es aceptable, pero
    conviene ir hacia funciones que reciban/retornen datos.
  - Falta `if __name__ == "__main__":` en `gui.py`; importar el módulo dispara la GUI.

## 5. Datos y persistencia

- Persiste en `eventos.json` con `json.dump(..., ensure_ascii=False, indent=2)`
  (`gui.py:91-92, 258-259, 314-315`), y recarga con `save_json` (`classes.py:66-68`).
  Funciona: verifiqué que carga 6 eventos correctamente.
- **Estructura por posición** (lista `[nombre, sala, (h_ini,h_fin), [inv], (d_ini,
  d_fin), mes, año]`). Es la decisión más discutible: acceder por índice mágico
  (`ev[2][1]`, `ev[4][0]`, `ev[6]`) es frágil y difícil de leer. Un `dict` con claves
  (`{"evento":..., "sala":...}`) o un `@dataclass` sería mucho más robusto — el propio
  informe lo reconoce como mejora futura.
- Detalle: `save_json` **nunca crea** `eventos.json` si falta (abre en modo `"r"`,
  `classes.py:67`); si el archivo no existe, la app fallaría al arrancar. Como el repo
  lo incluye, no se dispara, pero es una dependencia implícita.

## 6. Informe (`report.md`)

Informe **extenso y honesto**, y —punto importante— **no sobreestima de forma grave**:
casi todo lo que describe existe en el código (añadir/editar/eliminar/listar, botón de
info con precio, dependencias/exclusiones, *resource pool* con las cantidades exactas de
`classes.py`, formato de evento como lista posicional). Discrepancias menores:

- El ejemplo de evento del informe usa nombres con tilde ("Batería", "Micrófono") que
  **no** coinciden con los nombres sin tilde del código (`classes.py:40`); un evento así
  fallaría la validación de dependencias.
- La sección de estética habla de "Material UI", "FAB", "navigation drawer",
  "elevación", "accesibilidad para lectores de pantalla" — es más aspiracional que lo
  que CustomTkinter realmente entrega; no hay *drawer* ni foco accesible en el código.
- El informe no menciona el bug de precio ni la fragilidad ante entradas no numéricas
  (esperable — el estudiante no los conocía).

En conjunto el informe **describe fielmente** lo que el código hace y es transparente
(incluso sobre los nombres de los primeros commits y las condiciones en que se redactó).

---

## Valoración global (interna)

Trabajo **claramente por encima de la media para un primer proyecto**. La mayor
fortaleza es la **separación en capas** con un motor de reglas de negocio no trivial
(dependencias, exclusiones, solapamiento temporal, agotamiento de inventario y
sugerencia de hueco), que **verifiqué que funciona** ejercitándolo directamente. Los
puntos débiles son propios del nivel: fragilidad ante entradas no numéricas
(`ValueError` sin capturar), un bug de precio (`EVENTOS` en vez de `SALAS`), estructura
de datos posicional en vez de `dict`/`dataclass`, `print` de depuración y estado global.
Ninguno de estos rompe el uso normal por la GUI. La GUI no pude renderizarla en headless
(sin display), lo cual no es un defecto del proyecto.
