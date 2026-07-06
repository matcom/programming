# Reporte de Evaluación — Issue #233

- **Estudiante:** Sergio Jorge Montero López
- **Grupo:** C122
- **Repositorio:** https://github.com/SJopez/Apocalypse-Event-Manager
- **Descripción del issue:** "Gestor de eventos con temática de apocalipsis zombie".
- **Clonado en:** `/home/apiad/Workspace/.playground/proyecto1-eval/repos/233-apocalypse-event-manager` (depth 1, 1 commit visible).

> **Nota de calibración muy importante.** Esto **no** es la típica app de consola
> `main.py` + `input()` que asume la rúbrica. Es una **aplicación gráfica de
> escritorio completa** construida con **Kivy + KivyMD**, ~3626 líneas de Python
> repartidas en 16 módulos, más 6 archivos `.kv` de estilos, gráficas con
> Matplotlib, y persistencia en JSON. Para primer año esto es un proyecto de
> ambición y volumen muy por encima de lo esperado. Toda la evaluación está
> calibrada teniendo esto en cuenta.

---

## Ejecución dinámica (lo que hice y observé)

**Entry point:** `main.py` (`Main().run()`, `main.py:288`). Es una `MDApp`
(`main.py:266`) con un `ScreenManager` de 4 pantallas: menú de inicio, selección
de recursos, configuración de evento y lista de eventos (`main.py:254-278`).

**Entorno aislado:** monté un venv con `uv` (Python 3.12, porque Kivy 2.3.1 aún
no soporta 3.14) e instalé `requirements.txt`.

**Primer intento (fallido) y su causa raíz:** `requirements.txt` **no incluye
`kivymd`**. Al instalar `kivymd` desde PyPI se obtiene la 1.2.0, y el programa
importa `MDTimePickerDialHorizontal` (`screens/event_configuration/configuration.py:10`),
que **no existe** en 1.2.0. Traceback real:

```
File ".../configuration.py", line 10, in <module>
    from kivymd.uix.pickers import MDTimePickerDialHorizontal
...
FileNotFoundError: [Errno 2] No such file or directory: '.../kivymd/uix/label/label.kv'
```

**Descubrimiento clave:** el `report.md`/`README.md` **sí documentan** (Sección 4,
`report.md:234-242`) que KivyMD debe instalarse por separado clonando la rama
`master` (KivyMD 2.0.0), donde `MDTimePickerDialHorizontal` sí existe. Es decir,
la omisión de `requirements.txt` está compensada por la documentación. Seguí esa
instrucción: instalé `kivymd==2.0.1.dev0` desde el master de GitHub
(`--no-deps`, porque la dependencia transitiva `pycairo`/`materialshapes` no
compila sin cabeceras `cairo` del sistema, algo ajeno al proyecto).

**Segundo intento (exitoso):** ejecuté la app contra el display real (`DISPLAY=:0`)
con `timeout`. Al ser GUI no acepta inputs por stdin; se "recorre" observando el
arranque completo. Resultado:

- La ventana OpenGL se creó correctamente (Mesa/Intel, `[GL] OpenGL version 4.6`).
- Se cargaron **todos** los archivos `.kv` sin error.
- **0 tracebacks** durante toda la ejecución.
- Log final: `[Base] Start application main loop` → `[Base] Leaving application
  in progress...`. Es decir, **la aplicación arrancó por completo, construyó las
  4 pantallas y entró en su bucle principal**; solo terminó porque mi `timeout`
  la mató.
- Únicos avisos: warnings benignos de `[Factory] Ignored class ... re-declaration`
  (clases definidas a la vez en `.py` y en `.kv` — cosmético, no rompe nada) y
  ruido de proveedores de portapapeles (`xsel`/`xclip` no instalados en el
  sistema — irrelevante para la lógica).

**Validez de datos:** los 7 archivos JSON (`data/static/*` y `data/dynamic/*`)
parsean correctamente. Byte-compilación (`py_compile`) de los 16 `.py`: OK, sin
errores de sintaxis.

**Conclusión de ejecución:** una vez seguida la propia guía de instalación del
estudiante, **el programa arranca y funciona**. No pude ejercitar clics
individuales del menú de forma automatizada (es GUI de ratón, no de teclado),
pero el arranque limpio de las 4 pantallas y la ausencia total de excepciones
son evidencia fuerte de que la app está operativa.

---

## Dimensión 1 — Qué hace el programa

Es un **gestor gráfico de eventos ("aventuras") para un refugio en un apocalipsis
zombie**. El usuario, en el rol de líder del refugio, planifica misiones sobre un
inventario de 24 recursos de cantidad limitada. El flujo principal es:

1. **Menú de inicio** (`screens/init_menu/face.py`) — permite empezar, o
   cargar/guardar una partida desde archivo (`file_selector.py`).
2. **Selección de recursos** (`main.py:172` `ResourceMenu`) — 24 recursos
   seleccionables con panel de información al hacer hover (`main.py:42-84`).
3. **Configuración del evento** (`screens/event_configuration/configuration.py`)
   — calendario propio (`calendar_widget.py`), selector de hora (KivyMD), y
   creación tanto de eventos predefinidos como personalizados
   (`editable_event.py`).
4. **Lista de eventos** (`screens/event_list/events.py`) con un **diagrama de
   Gantt** generado con Matplotlib (`plot.py:74` `createGraph`).

La lógica de dominio no es trivial: valida fechas (duración mínima 24 h,
`event_manager.py:64-101`), reglas de recursos **complementarios/excluyentes**
(`event_manager.py:145-198`), y **detección de solapamiento temporal** entre
eventos que comparten recursos, con **reprogramación automática** a un hueco
libre si no hay inventario suficiente (`event_manager.py:200-286` `interception`,
`verifyInterval`, `joinTime`, `createEvent`).

## Dimensión 2 — Organización del código

**Muy por encima del nivel de primer año.** El proyecto está modularizado con
criterio real:

- Separación por capas: `core/` (lógica de dominio), `modules/` (utilidades y UI
  comunes), `screens/` (una carpeta por pantalla, con sus `styles/` y `widgets/`).
- La lógica de negocio (validación, solapamiento, reprogramación) vive en
  `core/event_manager.py` y `core/event_creation.py`, **separada de la UI**. Esta
  separación es notable para un principiante.
- Funciones pequeñas, con **docstrings en español en prácticamente todas**
  (`utilities.py`, `event_manager.py`, `plot.py`…). El nivel de comentado es
  excelente.
- Nombres razonables y descriptivos (`validateEventInfo`, `verifyInterval`,
  `joinTime`, `mergeInformation`).

Puntos mejorables (no penalizadores a este nivel):

- **Estilo de nombres inconsistente:** mezcla `camelCase` (`readJson`,
  `validDate`, `setEvent`) con `snake_case` (`get_one`, `create_adventure`,
  `add_resource`). PEP 8 recomienda `snake_case`. No rompe nada, pero conviene
  unificar.
- **Estado global mediante clases-contenedor** (`CurrentScreen`, `Disable`,
  `Utils`, `finded`, `Success` en `utilities.py:141-171`). Funciona, pero es
  esencialmente estado global mutable; con el tiempo dificulta razonar sobre el
  programa.
- `finded.ans` como variable de módulo para devolver el resultado de
  `join_child` (`utilities.py:141-144,183-195`) es un patrón frágil (no
  reentrante). Se podría resolver con un valor de retorno normal recursivo.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

- **Arranca:** sí, tras seguir la guía de instalación (venv + `requirements.txt`
  + KivyMD master). Construyó las 4 pantallas y entró en el main loop sin una
  sola excepción.
- **`Traceback` en ejecución normal:** **ninguno** observado durante el arranque
  y bucle principal.
- **Hace lo que dice el issue/informe:** sí — dominio zombie, gestión de eventos
  y recursos, GUI, Gantt, cargar/guardar. Todo lo prometido está implementado en
  código real, no solo descrito.
- **Validación de entradas:** buena. Valida título/descripción no vacíos
  (`event_creation.py`/`event_manager.py:35-61`), fechas con `try/except`
  distinguiendo `IndexError`/`ValueError` de duración insuficiente
  (`event_manager.py:96-98`), disponibilidad de inventario, y complementarios/
  excluyentes. El guardado de archivo envuelve `shutil.copy` en `try/except` con
  mensaje de permisos (`file_selector.py:75-81`), y la carga valida el formato
  del JSON (`file_selector.py:57-61`).
- **Observación menor de robustez (no verificada dinámicamente por ser GUI):** en
  `validResources` (`event_manager.py:189-196`) se hace `int(child.cuantity.text)`
  sin `try/except`; si el usuario deja el campo de cantidad en un valor no
  numérico, podría lanzar `ValueError`. No pude reproducirlo por ser interacción
  de ratón, pero lo dejo anotado como el punto más probable de fallo.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy sólido para el nivel:

- **Legibilidad e indentación:** consistentes en todo el árbol.
- **f-strings** usadas idiomáticamente (`main.py:30`, `event_manager.py:315`,
  `file_selector.py:66`).
- **`try/except/else`** bien empleado, incluso con la cláusula `else`
  (`event_manager.py:84-101`, `file_selector.py:75-87`) — un uso avanzado.
- Helpers de E/S JSON (`readJson`/`writeJson`/`addToJson`, `utilities.py:65-95`)
  evitan duplicación — buena factorización.

A mejorar:

- `except:` desnudo en `file_selector.py:97` — mejor `except Exception`.
- `from ... import *` en varios módulos (`main.py:10,13`, `utilities.py:1`) —
  cómodo pero oscurece de dónde viene cada símbolo.
- Comparaciones `== False` / `!= False` (`event_manager.py:47,168`) — más
  idiomático `not getChar(...)`.

## Dimensión 5 — Datos y persistencia

Diseño de datos correcto y bien pensado:

- **Estáticos** (`data/static/events.json`, `resources.json`): catálogo de 18
  eventos y 24 recursos con esquema rico (tipo, complementario, excluyente,
  cantidad, peligro, ubicación). Coherente y completo.
- **Dinámicos** (`data/dynamic/`): separa el evento en configuración
  (`current_event.json`), los eventos activos (`running_events.json`) y las
  selecciones temporales de recursos. Se limpian al cerrar (`cleanJSON`,
  `utilities.py:212-217`, enganchado en `main.py:281`).
- **Persistencia real:** cargar/guardar partida a un archivo JSON elegido por el
  usuario mediante un `FileChooser` (`file_selector.py`), con validación de
  formato al cargar. Esto es persistencia de verdad, no solo estado en memoria.

Los 7 JSON parsean sin error.

## Dimensión 6 — Informe (`report.md`)

**Sobresaliente y honesto** (4690 palabras, con tabla de contenidos, árbol de
directorios, guía de instalación paso a paso, y guía de interfaz):

- **Coincide con el código:** cada funcionalidad descrita (Gantt con Matplotlib,
  pools de recursos, complementariedad/exclusión, cargar/guardar, eventos
  personalizados) está efectivamente implementada. **No detecté sobreestimación.**
- **Documenta la instalación de KivyMD** (Sección 4, `report.md:234-242`),
  que es exactamente lo que hacía falta para correr el proyecto — la
  reproducibilidad está cubierta por el informe aunque `requirements.txt` no
  pinne `kivymd`.
- Reconoce dificultades reales con criterio técnico (`report.md:437`): implementar
  su propio selector de calendario, y menciona el uso de DFS para recorrer el
  árbol de widgets (`join_child`).

Única fricción: la omisión de `kivymd` en `requirements.txt` obliga a leer la
Sección 4 del informe; sería ideal reflejarlo también en `requirements.txt` (con
un comentario) para que el `pip install -r` no dé falsa sensación de completitud.

---

## Síntesis para el profesor

Trabajo **excepcional para primer año**. No es una app de consola: es una GUI de
escritorio completa y funcional (Kivy + KivyMD), con lógica de dominio no trivial
(solapamiento temporal, reprogramación automática, reglas de recursos),
persistencia en JSON, gráfica de Gantt, y un informe extenso y fiel al código.
**Ejecución verificada:** arranca sin excepciones y entra en su bucle principal
tras seguir la guía de instalación del propio estudiante.

- **Principal fortaleza:** ambición, volumen y modularización real, con separación
  lógica/UI y docstrings en casi todo.
- **Principal área de mejora:** unificar convención de nombres (`camelCase` vs
  `snake_case`) y reducir el estado global; y pinnear/anotar `kivymd` en
  `requirements.txt`.
- **Robustez a vigilar:** `int(child.cuantity.text)` sin guarda en
  `validResources` (posible `ValueError` con entrada no numérica).

Nivel general: muy alto. Sin nota numérica (orientativo).
