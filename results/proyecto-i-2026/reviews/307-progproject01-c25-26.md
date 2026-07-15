# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #307
- **Repositorio:** https://github.com/cubangnocchi/ProgProject01-c25-26
- **Estudiante:** Marco Raymat Cateura
- **Grupo:** C122
- **Descripción declarada:** Aplicación de consola inspirada en *The Expanse*: gestor de eventos e inventario para una nave minera. Permite crear eventos que no contradigan un conjunto de restricciones sobre inventario, tripulación, ubicación y tipo de evento, con persistencia en JSON.

---

## Nota metodológica importante

Es una aplicación de consola genuina (`input()`/`print()`), sin dependencias externas: corre solo con la biblioteca estándar de Python. Se ejecutó de verdad alimentando el menú con `printf '...' | python main.py`. Se probaron todos los flujos del menú (listar eventos/items, crear evento, añadir tripulante), flujos válidos e inválidos, y además se ejercitó la lógica de negocio de forma directa (edades, solapamiento de intervalos, motor de restricciones) importando las clases. Antes de cada corrida mutante se respaldó `save01.json` y se restauró.

## Dimensión 1 — Qué hace el programa

Punto de entrada `main.py:4` → `manager.run()`. Al importarse `src/manager/manager.py:16-19` carga `save01.json`, lo convierte a objetos `Calendar` + `Inventory` y entra en `main_bucle()` (`manager.py:32`), un bucle de menú:

- **[1] list events** (`manager.py:87`) — imprime nombre, intervalo y nº de personas de cada evento. Verificado: lista los dos eventos de ejemplo correctamente.
- **[2] list items** (`manager.py:91`) — imprime clave, nombre, tipo y cantidad (o "unique"). Verificado.
- **[3] list crew** — **aparece en el menú (`menues.py:19`) pero NO tiene rama en `main_bucle`** (`manager.py:34-53`): pulsar `3` no hace nada. Bug confirmado en ejecución.
- **[4] add item** (`manager.py:55`) — menú de creación con nombre, tipo (seleccionable), expendable, cantidad. Persiste.
- **[5] create event** (`manager.py:59`) — flujo completo: nombre → intervalo (dos fechas) → lugar → tipo → items → personas → chequeo de restricciones. Si pasa, inserta ordenado y guarda; si no, muestra los errores. **Es el núcleo del proyecto y funciona.**
- **[6] add crew member** (`manager.py:83`) — nombre, especialidad, estatus, fecha de nacimiento. Persiste.
- **[x] exit** — sale y guarda.

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** La arquitectura por paquetes es muy superior a lo típico en 1er año:

- `src/calendar/` — `Calendar`, `Event`, `interval` (dominio temporal).
- `src/inventory/` — `Item`, `Human`, `Inventory` (dominio de recursos).
- `src/data_base/` — `data_management.py`: carga/guarda JSON y hace la conversión objeto↔dict.
- `src/manager/` — orquestación (`manager.py`), motor de reglas (`restrictions.py`), datos de reglas (`restrictions_data.py`), búsquedas auxiliares (`data_search.py`).
- `src/visual_interface/` — separación real de la capa de presentación: `menues.py`, `console_in_out.py`, `SelectionMenue`.

La clase `SelectionMenue` (`selection_menue.py`) es un acierto: encapsula un patrón de menú reutilizable con validación de opción, y varios métodos estáticos para construir menús numerados desde listas/dicts. Las clases de dominio tienen serialización simétrica `convert_to_dictionary` / `convert_dictionary_to_*` bien pensada. La separación presentación/lógica/persistencia es limpia y consistente.

Debilidades: nombres inconsistentes (`interval` en minúscula vs. clases en PascalCase; `date_cration_menue`, `menue`, `expendable` mal escritos de forma sistemática); estado global a nivel de módulo (`main_calendar`/`main_inventory` en `manager.py:18-19` **y** duplicado en `restrictions.py`… no, en `tester.py`); mezcla de inglés (código) y español (comentarios). Nada grave para el nivel.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Lo que **se corrió** y **se observó**:

1. **Listar eventos ([1])** — OK. Salida correcta con los 2 eventos de `save01.json`.
2. **Listar items ([2])** — OK. Muestra u235 (200), space suit (56), nutrient blocks (5000).
3. **Listar tripulación ([3])** — **no hace nada**: falta el `if(option_selected == "3")` en `manager.py`. El menú promete la opción pero está muerta.
4. **Crear evento inválido ([5])** — un evento `reparation` en `Nuclear Reactor` sin items ni personas produjo correctamente **5 mensajes de restricción** ("needs an item of the type ¨tool¨", "needs a person specialized in ¨reparation specialist¨", "needs an item ¨radio-hazard protection¨", "needs ¨electric engineer¨", "needs ¨nuclear engineer¨") y ofreció el menú de recuperación. **El motor de restricciones funciona.**
5. **Crear evento válido ([5])** — un evento `undeterminated` en `Crew Quarters` (sin requisitos) se guardó y se insertó **en orden** entre los eventos existentes (verificado en el JSON: fechas 07:30 → 08:00 → Jan 3). La **inserción binaria ordenada** (`calendar.py:80`) funciona correctamente.
6. **Añadir tripulante ([6])** — se guardó con clave "6" y `NEXT_KEY` avanzó a 7. OK.
7. **Entradas basura** — probé `abc` para día, mes `99`, día `45`: todas atrapadas sin `Traceback` ("invalid literal for int()", "month must be in 1..12"). El manejo de entrada es robusto para un humano en terminal.
8. **Opción de menú inválida** (`9`) — rechazada con "option not available, try again". OK.
9. **Lógica directa** — `get_age` da 24/19/26 correctamente; `is_it_overlaping` da True/False correcto en casos de prueba.

Bugs y limitaciones detectados:

- **[Bug real] Opción [3] muerta** — `manager.py:34-53` no maneja `"3"`.
- **[Bug latente] Falta `error_list.append`** en la dependencia lugar→estatus (`restrictions.py:87`): se construye `error_str` pero nunca se agrega. No se dispara con los datos actuales porque todas las listas de estatus por lugar están vacías (`restrictions_data.py:218-228`), pero el error existe.
- **[Incompleto — no bug] Motor de reglas parcial.** `check_event` (`restrictions.py:11`) **solo llama a `dependency_check`**. Las funciones `exclusion_check`, `resources_n_crew_concistency`, `place_related_restrictions`, `dynamic_restrictions` (`restrictions.py:100-129`) son *stubs* que devuelven `[]`. Los datos de exclusión y de consistencia de recursos existen en `restrictions_data.py` pero no se usan. El propio código lo marca con `#! te quedaste por aquí` (`restrictions.py:89-95`) y el historial de commits ("restriction logic works!!!! now just add all of them...") lo confirma. Por eso, un evento puede violar reglas de exclusión o agotar el inventario sin ser rechazado. `Interval.is_it_overlaping` funciona pero **nunca se invoca** desde el pipeline (los choques de horario no se comprueban).
- **[Robustez] Bucle infinito con EOF.** `input_int_bucle` (`console_in_out.py:19-27`) captura `Exception` genérica: al agotarse la entrada canalizada, `input()` lanza `EOFError`, que se atrapa e imprime "Input was no valid because: EOF", reintentando para siempre. En una terminal real no ocurre (el humano teclea), pero rompe cualquier ejecución automatizada por tubería.
- **[Menor] `event_error_menue`** (`menues.py:92-100`) tiene la condición invertida: si `len==0` intenta `error_list[0]` sobre lista vacía. Es código muerto (solo se llama con errores presentes), pero está al revés.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Positivo: uso correcto de `try/except` alrededor de la construcción de fechas, funciones pequeñas y con propósito único, *type hints* en muchas firmas, sin `Traceback` en flujos probados. `datetime` bien aprovechado.

Mejorable (todo menor para 1er año):
- `except Exception` demasiado ancho (`console_in_out.py`) — atrapa también `EOFError`/`KeyboardInterrupt`; conviene `except ValueError`.
- `if (x == None)` → idiomático `if x is None` (`calendar.py:28`, `menues.py:32`).
- `self.day_born.__str__()` → `str(self.day_born)` (varios).
- Estado global a nivel de módulo dificulta el testeo; pasar `calendar`/`inventory` como parámetros (ya lo hace en `check_event`, buena señal).
- Muchos `#!` de "pendiente" y typos sistemáticos (`menue`, `bucle`, `expendable`, `concistency`).

## Dimensión 5 — Datos y persistencia

Sólida. Modelo bien pensado: eventos referencian personas/items por **clave** (no por copia), lo que evita duplicación. Serialización simétrica en cada clase (`get_as_dictionary`/`dictionary_to_event`, etc.). `data_management.py` maneja el caso de base de datos ausente con `default_empty_data()` (`data_management.py:30`), poblando nave y lugares por defecto. Fechas serializadas como string y reparseadas con `strptime`. Un detalle: `Item.convert_dictionary_to_item` compara `IS_EXPENDABLE == "1"` (string) mientras se guarda como int `1` (`item.py:36,50`), así que al recargar todo item queda `is_expendable=False`; no afecta la ejecución porque esa bandera aún no se usa en reglas. La inconsistencia de clave int/str en `add_human` (`inventory.py:37`) queda normalizada por JSON al guardar, pero podría causar un `KeyError` si se referencia el tripulante recién creado dentro de la misma sesión.

## Dimensión 6 — Informe (`report.md`)

El informe (idéntico a `README.md`, con `report.md` como copia limpia) describe con precisión y honestidad la arquitectura, con enlaces `archivo:línea` a los métodos reales — y coinciden con el código. **No exagera**: no afirma que el proyecto "demuestra" ni "prueba" nada, y describe el motor de restricciones como "un conjunto de restricciones que se han de cumplir" sin sobrevender su completitud. Puntos débiles del informe: está **inacabado** — secciones "Ejemplo de uso", "Flujo de ejecución" e "Historia y experiencia" solo contienen la palabra "texto" (placeholder); varios typos; el índice enlaza a un ancla mal escrita (`#diseño-del-proyrcto`). Queda `borrador_readme.docx` suelto en la raíz. El informe es honesto pero le falta terminar de redactarse.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**, claramente por encima del promedio de 1er año en diseño. El estudiante entendió y aplicó separación de responsabilidades por paquetes, una capa de presentación reutilizable (`SelectionMenue`), serialización objeto↔JSON simétrica, inserción ordenada con búsqueda binaria, y un motor de reglas dirigido por datos (`restrictions_data.py`) que es la parte más difícil y la que **sí funciona** en su primera categoría. Se ejecutó de verdad y crea/persiste eventos válidos, rechaza los inválidos con mensajes claros, y no revienta ante entrada basura. Las carencias son de *terminación*, no de comprensión: la mitad del motor de restricciones quedó como *stubs* (el propio código lo admite), la opción [3] del menú no está cableada, y el informe tiene secciones en blanco. Nada de esto contradice la calidad del núcleo; son cabos sueltos de un proyecto al que le faltó una última pasada.

- **Principal fortaleza:** arquitectura modular madura + motor de restricciones dirigido por datos que funciona en ejecución real (5 restricciones correctas verificadas), con persistencia JSON robusta e inserción ordenada.
- **Principal área de mejora:** terminar lo empezado — cablear la opción [3], implementar (o al menos invocar) las categorías de restricción que ya tienen datos (exclusión, consistencia de recursos, choques de horario vía `is_it_overlaping`), y completar las secciones del informe. Corregir el bucle infinito ante `EOF` acotando el `except` a `ValueError`.
