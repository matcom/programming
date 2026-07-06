# Revisión detallada — Proyecto I (256) OlymPlaner

- **Estudiante:** Ricardo Miguel Molano Dominguez
- **Grupo:** C121
- **Issue:** #256
- **Repositorio:** https://github.com/RMolanod7/OlymPlaner
- **Descripción del issue:** planificación de una Olimpiada de Centroamérica y el Caribe: gestionar recursos, programar actividades, evitar conflictos y generar un calendario final coherente.

---

## Ejecución (obligatoria) — qué corrí y qué observé

La app **no es de consola**: es una aplicación web **Streamlit** (`Main.py:1`, `Main.py:11 st.set_page_config`, `Main.py:569 # streamlit run Main.py`). No hay `requirements.txt` ni `pyproject.toml`; la única dependencia es `streamlit` (más stdlib: `datetime`, `json`, `collections`, `copy`, `pathlib`, `base64`).

Monté un entorno aislado con `uv` (Python 3.13) e instalé `streamlit==1.59.0`. Como es una app interactiva (no un menú `input()`), la recorrí con el framework oficial de pruebas de Streamlit, `streamlit.testing.v1.AppTest`, que ejecuta el script real y permite pulsar botones y rellenar widgets.

**Hallazgo crítico al arrancar (checkout limpio):**
Con el repositorio recién clonado, la app lanza `Traceback` inmediato:

```
AttributeError: st.session_state has no key "country".
  File "Main.py", line 43, in main
    idx = countrys.index(st.session_state.country)
```

Causa raíz confirmada: **desajuste en el nombre del fichero de persistencia**. El repo trae `data_olymplanner.json` (minúsculas, una sola `n` en "olymplanner"→"olymplaner"), pero `Persistence.py:8` y `Persistence.py:35` leen/escriben `Data_Olymplanner.json` (mayúscula inicial). Como ese fichero *no existe*, `load_data()` cae en el `except FileNotFoundError` y retorna sin poblar `st.session_state.country` (`Persistence.py:39-40`). Acto seguido, `Main.py:43` asume que `country` ya existe y revienta. El propio archivo del repo **no** salva el arranque porque su nombre no coincide con el que el código busca.

**Verificación:** copié `data_olymplanner.json` → `Data_Olymplanner.json` (el nombre que el código espera) y repetí. Con eso **la app arranca perfectamente** y todo el flujo funciona:

- Arranque OK: título `Planificador de Eventos`, headers de la olimpiada e inventario, selector de país sede, panel lateral con 25 `number_input` de inventario. Exceptions: `[]`.
- **Selección de país** (`Cuba`): OK, sin excepción.
- **Validación de días**: puse `2` días → mostró el error `La olimpiada debe durar como mínimo 5 dias y como máximo 30` (`Main.py:60-61`). Correcto.
- **Confirmar Recursos** (botón lateral): `st.session_state.blocked` pasó a `True` sin errores (inventario por defecto en 20 cumple los mínimos).
- **Avanzar**: `home_screen` pasó a `False`, se muestra la pantalla de funciones con los 6 botones del menú (Agregar / Listar / Eliminar / Detalles / Buscar Hueco / Calendario Final / Volver).
- **Agregar Eventos → Inaguración de la Olimpiada**: aparecen 12 `number_input` de recursos en dos columnas. Puse cantidades válidas (salón de eventos=1; coordinadores/expertos/guías/técnicos=3; el resto en 0). Pulsé "Añadir evento una vez" → `✅ Evento Inaguración de la Olimpiada añadido.` y el evento quedó registrado (`len(eve.events)==1`). La cadena de validaciones (recursos obligatorios/absurdos, inventario, colisiones, co-requisitos) se ejecutó sin `Traceback`.
- **Listar / Detalles / Buscar Hueco / Calendario Final** (sin eventos y con eventos): todos abren sin excepción. Calendario Final vacío muestra `No existen eventos agregados al calendario` (`Main.py:496-497`).

**Conclusión de ejecución:** la lógica de negocio está **viva y funcional** — el único obstáculo para que un evaluador la vea funcionar de primeras es el bug del nombre de fichero, que impide el arranque en un clon limpio. Es un fallo de una línea (renombrar el `.json` o corregir la cadena en `Persistence.py`), pero **bloqueante**.

---

## Dimensión 1 — Qué hace el programa

Aplicación web (Streamlit) para **planificar una Olimpiada de Centroamérica y el Caribe**. Punto de entrada `Main.py` (`main()` en `Main.py:10`, guardado por `if __name__ == "__main__"` en `Main.py:565`); se ejecuta con `streamlit run Main.py`. El flujo es de dos pantallas gobernadas por `st.session_state.home_screen`:

1. **Pantalla de configuración** (`Main.py:37-131`): elegir país sede (cambia el fondo visual, `Paises.py:20`), fecha de inicio, número de días (5–30), y ajustar el inventario general por categorías en el panel lateral. Un botón "Confirmar Recursos" valida mínimos (comida ≥ días, ≥10 cocineros/voluntarios, no-ceros) y bloquea la config; "Avanzar" pasa a la segunda pantalla.
2. **Pantalla de funciones** (`Main.py:132-563`): menú con Agregar Eventos (individual y recurrente), Listar, Eliminar (uno o todos), Detalles (por evento o por recurso), Buscar Hueco (siguiente hueco temporal libre), y Mostrar Calendario Final (vista HTML/CSS agrupada por día).

El dominio se modela con un catálogo fijo de 9 tipos de evento, cada uno con su lista de recursos (`Events.py:17-27`), y un rico sistema de restricciones (temporales, estructurales, de inventario, co-requisitos y exclusión mutua/seguridad). Cumple lo que promete el issue.

## Dimensión 2 — Organización del código

**Muy buena para 1er año.** El proyecto está **modularizado en 6 archivos** con responsabilidades claras, no un `main.py` gigante:

- `Units.py` — clases `Resource` y `Event` (el modelo de dominio, con `__eq__`, restricciones de exclusión mutua y co-requisitos).
- `Events.py` — clase `Events` (colección + lógica: `add_events`, `check_overlap`, `restrictions`, `check_inventory`, `resources_for_event`, recursión `add_event_recursive`).
- `Inventory.py` — clase `Inventory` (catálogo de recursos por categoría).
- `Paises.py` — clase `Pais` + `set_bg` (fondo visual por país).
- `Persistence.py` — `save_data` / `load_data` (JSON).
- `Main.py` — toda la interfaz Streamlit.

Usa **clases donde el dominio lo pide** (`Units.py:3`, `Units.py:18`, `Events.py:15`) y **funciones** para no repetir (`Event.Dict_res`, `Event.Res_names`, `Events.check_overlap` reutilizada por Buscar Hueco y por la recursión). Nombres en general claros y descriptivos (`selected_resources`, `check_inventory`, `resources_for_event`).

Puntos mejorables:
- `Main.py` es largo (568 líneas) y la función `main()` concentra toda la UI en un solo bloque con muchas ramas `if/elif` sobre `st.session_state.options`. Extraer cada pantalla del menú a su propia función (`render_agregar()`, `render_listar()`, …) lo haría mucho más legible.
- Mezcla de idiomas/convenciones en nombres: métodos con mayúscula inicial (`Dict_res`, `Res_names`) conviven con `snake_case` (`add_events`, `check_overlap`). Python usa `snake_case` para métodos.
- `restriccions` (`Units.py:71`) y `restrictions` (`Events.py:68`) — dos métodos casi homónimos (uno con typo) que hacen cosas distintas; fácil de confundir.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ver la sección "Ejecución" arriba. Resumen:

- **Bloqueante:** no arranca en clon limpio por el desajuste `data_olymplanner.json` vs `Data_Olymplanner.json` (`Persistence.py:8`, `Persistence.py:35`) → `AttributeError` en `Main.py:43`.
- **Corregido el nombre, todo funciona:** país, validación de días, confirmar recursos, avanzar, agregar evento (con toda su cadena de validaciones), listar, detalles, eliminar, buscar hueco y calendario final — sin `Traceback` en ninguna opción probada.
- Las validaciones de negocio son **coherentes con el issue y con `Informacion.md`**: colisiones de horario (`Events.py:42`), unicidad de inauguración/despedida y tope de 2 exámenes (`Events.py:68`), recursos obligatorios/absurdos por evento (`Events.py:105`), exclusiones de seguridad (`Units.py:52`), co-requisitos (`Units.py:71`).
- **Nota funcional menor:** las imágenes de fondo de Panamá y República Dominicana están **cruzadas** — `self.name == "Panama"` carga `Dominicana_Olymplaner.jpg` y viceversa (`Paises.py:35-37`). No rompe nada, pero muestra el país equivocado.
- **Otra nota:** en `Main.py:503-506`, el mensaje del último evento dice "El primer evento debe ser la fiesta de despedida" cuando debería decir "el último". Es un texto, no un fallo lógico.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Bien:** indentación consistente; f-strings en todos lados; comprehensions razonables (`Main.py:151`, `Main.py:391`, `Main.py:416`); uso de `defaultdict` para agrupar por día (`Main.py:513`); `try/except FileNotFoundError` en `load_data` (`Persistence.py:39`) y `try/except ValueError` al eliminar (`Main.py:350-364`); `deepcopy` para no mutar el evento base en la recursión (`Events.py:215`).
- **`from X import *` en todos los módulos** (`Main.py:3-8`, `Events.py:1-3`, etc.). Funciona, pero contamina el espacio de nombres y oculta de dónde viene cada cosa. Sugerencia: importar explícito (`from Units import Event, Resource`).
- **Duplicación evidente en `resources_for_event`** (`Events.py:105-205`): los tres bloques de comida (Desayuno/Almuerzo/Cena, `Events.py:166-177`) son idénticos salvo el nombre; podrían unirse con `if event.name in ("Desayuno","Almuerzo","Cena")`. Igual, la larga cadena de `elif i == "técnicos" and j.name == ...` en `Units.py:100-114` repite el mismo patrón 5 veces — un bucle sobre una lista de nombres de material lo reduciría a 4 líneas.
- **Comparaciones con listas literales larguísimas** (`Main.py:99`, `Events.py:109`, `Events.py:126`): más legible con `in {…}`.
- `if x.reu == False` (`Main.py:282`, `Events.py:250`) → idiomático: `if not x.reu`.
- No penalizo ausencia de tests/type hints (correcto para 1er año); de hecho ya usa anotaciones de tipo en firmas (`Units.py:4`, `Units.py:19`), lo cual es un plus.

## Dimensión 5 — Datos y persistencia

- **Estructuras de datos razonables y variadas** (justo lo que dice el informe): diccionarios para el catálogo de eventos→recursos (`Events.py:17`) y para el inventario (`Inventory.py:5`), `set` para nombres únicos (`Events.py:30`), listas para eventos, `defaultdict` para agrupar (`Main.py:513`).
- **Persistencia JSON** implementada de forma limpia (`Persistence.py`): serializa config + inventario + eventos (con formato de fecha explícito `%Y-%m-%d %H:%M`) y los reconstruye a objetos `Event`/`Resource` al cargar. Bien pensado.
- **Pero:** (a) el bug del nombre de fichero hace que la persistencia efectivamente **no cargue** en clon limpio; (b) el nombre está **hardcodeado por duplicado** en `save_data`/`load_data` — si se corrige, hay que tocarlo en dos sitios (o mejor, una constante).
- **Detalle sutil:** `Inventory.default_inventory` usa **`set`** para los recursos de cada categoría (`Inventory.py:6-39`). Al iterar sets para pintar el inventario (`Main.py:73-78`) el **orden no es determinista** entre ejecuciones; para una UI conviene una lista/tupla con orden fijo.

## Dimensión 6 — Informe (`Report.md` + `Informacion.md`)

**Excelente y honesto.** `Report.md` describe con fidelidad lo que el código hace (Streamlit, POO, `st.session_state`, modularización, recursión para eventos recurrentes, calendario HTML/CSS) — nada de features fantasma. Explica diseño, uso paso a paso, dificultades reales (colisiones, recursos reutilizables concurrentes, persistencia) y aprendizajes. `Informacion.md` documenta el sistema de restricciones con más detalle del que muchos proyectos de 1er año alcanzan, y **coincide con el código** (exclusiones de seguridad, co-requisitos, límites de cantidad).

Discrepancias informe↔código detectadas:
- El informe presume "persistencia mediante `st.session_state`" y JSON, pero **no menciona** que el arranque limpio falla por el nombre del fichero. No es sobreestimación consciente, es el bug no detectado.
- `Report.md` habla de "≥10 cocineros/voluntarios" en el informe general y el código sí lo valida (`Main.py:92`), pero en `restriccions` de `Units.py:81` el umbral por evento es `< 5`. Son dos chequeos distintos (inventario global vs. por evento); no es contradicción, pero conviene aclararlo.

En conjunto, el informe **no sobreestima**; si acaso, subestima la calidad de las validaciones implementadas.

---

## Síntesis para el profesor

Trabajo **notablemente por encima de la media de 1er año**: código modularizado en 6 archivos con POO real y bien usada, un motor de restricciones amplio y coherente con el dominio, persistencia JSON, recursión aplicada, e interfaz web con Streamlit (más ambicioso que la típica app de consola pedida). El informe es serio, honesto y detallado.

El **único fallo grave** es de una línea y bloqueante: el nombre del fichero de datos no coincide (`data_olymplanner.json` vs `Data_Olymplanner.json`), lo que impide el arranque en un clon limpio (`Traceback` en `Main.py:43`). Corregido eso (renombrar el `.json` o la cadena en `Persistence.py`), toda la aplicación funciona sin excepciones en el recorrido completo del menú. Defectos menores: imágenes Panamá/Dominicana cruzadas, duplicación en `resources_for_event`, `import *` generalizado, y sets con orden no determinista en el inventario.

**Fortaleza principal:** arquitectura y modelado del dominio. **Área de mejora principal:** robustez/consistencia (el bug de arranque debió detectarse probando desde cero).
