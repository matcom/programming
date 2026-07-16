# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #331
- **Repositorio:** https://github.com/gbrielass/proyecto-de-programacion-2026.git
- **Estudiante:** Maria Gabriela Avila Leyva
- **Grupo:** C111
- **Descripción declarada:** Ayuda a gestionar la organización de eventos.

---

## Nota metodológica importante

**No es una app de consola.** Es una aplicación **Streamlit** (GUI web): `main.py:1` importa `streamlit as st` y todo el flujo de usuario pasa por widgets (`st.text_input`, `st.selectbox`, `st.multiselect`, `st.date_input`, `st.time_input`, `st.button`). No hay `input()` en ninguna parte, así que alimentarla con `printf` no aplica.

Cómo adapté la ejecución:

1. Creé un entorno aislado (`uv venv --python 3.12`) e instalé `streamlit` y `pandas` (no hay `requirements.txt` ni `pyproject.toml`; deduje las dependencias de los `import`).
2. `py_compile` de los cinco módulos — todos compilan.
3. **Arranque headless real:** `streamlit run main.py --server.headless true` levantó el servidor y respondió **HTTP 200** en `localhost`. La GUI arranca sin errores.
4. **Lógica de negocio ejecutada directamente** con los datos reales del repo (`data/clientes.JSON`, `data/Empresas.JSON`), sustituyendo `streamlit` por un stub que captura los `st.error`/`st.success`, para poder recorrer flujos válidos e inválidos sin navegador. Sobre una copia temporal de los JSON probé guardar empresa, guardar evento, eliminar y las restricciones.

## Dimensión 1 — Qué hace el programa

Es un organizador de eventos con emparejamiento cliente↔empresa. Menú lateral con cuatro opciones (`main.py:20`): **Empresa**, **Cliente**, **Eventos del dia**, **Eliminar**.

- **Empresa** (`main.py:35-45`): registra una empresa proveedora con nombre, ID, inventario de recursos y de personal. Persiste en `data/Empresas.JSON` vía `Empresa.guardar_empresa` (`function_empresa.py:21`).
- **Cliente** (`main.py:24-33`): captura una solicitud de evento (nombre, ID, tipo, fecha, hora inicio/fin, recursos y personal deseados) y llama a `Evento.crear_eventos` (`funcion_client.py:99`). Ahí ocurre lo interesante: valida los campos, aplica restricciones de coherencia, **busca una empresa que tenga todo lo pedido** (`find_empresa`, `funcion_client.py:39`) y **que esté libre en ese horario sin chocar recursos/personal** (`find_time` + `seeker`, `funcion_client.py:68` y `:92`), y solo entonces guarda el evento asignándole `empresa_id`.
- **Eliminar** (`main.py:47-53`): borra una empresa o un evento por ID (`funcion_delete.py:5`).
- **"Eventos del dia"**: aparece en el menú pero **no tiene rama `elif` que la maneje** (`main.py:22-53`). Seleccionarla no hace nada — es una opción muerta.

El corazón del proyecto —el algoritmo de emparejamiento con detección de solapamiento horario y conflicto de recursos— **funciona correctamente** en mis pruebas, y es más ambicioso que un CRUD plano.

## Dimensión 2 — Organización del código

**Fortalezas:**
- Separación por archivos según responsabilidad: `funcion_client.py` (eventos + matching), `function_empresa.py` (empresas), `funciones_restricciones.py` (reglas de coherencia), `funcion_delete.py` (borrado), `main.py` (UI). Para primer año es una modularización razonable.
- Dos clases con `__init__` y método `convertir()` para serializar a dict (`funcion_client.py:7`, `function_empresa.py:6`) — buena intuición de modelo de datos.

**Debilidades:**
- **Métodos que deberían ser `@staticmethod` no lo son** y se llaman sin `self`: `find_empresa`, `find_time`, `seeker`, `crear_eventos` (`funcion_client.py:39,68,92,99`) están definidos como métodos de instancia pero reciben sus datos por parámetro y se invocan como `Evento.crear_eventos(...)`. Funciona porque se llaman por la clase, pero conceptualmente son funciones sueltas metidas dentro de la clase. Lo mismo con `Empresa.guardar_empresa` (`function_empresa.py:21`).
- **Lecturas de archivo en el cuerpo de la clase** (`funcion_client.py:32-36`): `ceo = json.load(...)` y `client = json.load(...)` se ejecutan **al importar el módulo**, no al usarlo. Esto hace el import frágil: verifiqué que importar `funcion_client` desde otro directorio de trabajo revienta con `FileNotFoundError: data/Empresas.JSON` (`funcion_client.py:32`). Además esas dos variables de clase **nunca se usan** — `crear_eventos` vuelve a abrir los mismos archivos localmente (`funcion_client.py:118-121`). Son código muerto que solo añade fragilidad.
- Rutas relativas (`"data/..."`) por todos lados: el programa solo corre si el CWD es la raíz del repo.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Numerado, distinguiendo bugs del estudiante de fallos del entorno.

1. **Arranque GUI: OK (entorno).** `streamlit run main.py` levanta y responde HTTP 200. El `ModuleNotFoundError: streamlit` de la verificación automática es solo la ausencia de la dependencia instalada, no un bug del código.

2. **Algoritmo de emparejamiento: CORRECTO.** Con una empresa `87654321` que dispone `Escenario, Luz de escenario, Mesas y sillas, Globos` / `tecnico de sonido, meseros`:
   - `find_empresa(["Escenario","Luz de escenario"], ["meseros"])` → `['87654321']` (la encuentra).
   - `find_time` sobre el evento existente del 2026-02-07 18:38–22:00 que usa "Escenario": pedir 19:00–21:00 el **mismo día** con "Escenario" → `False` (bloquea el choque). Pedir el **2026-03-01** → `True` (libre). Correcto.
   - `seeker` devuelve `87654321` en fecha libre y `"no hay"` en fecha en conflicto. Correcto.

3. **BUG — `guardar_empresa` no tiene `return` tras los errores de validación** (`function_empresa.py:22-30`). La validación es un `if/elif` **separado** del bloque de guardado (que empieza en `function_empresa.py:32` con un `if funciones_restricciones...` independiente). Al no cortar el flujo, **una empresa inválida se guarda igual**. Verificado: `guardar_empresa("", "x", [], [])` emitió a la vez `error "Escriba el nombre de la empresa"` **y** `success "Evento guardado correctamente"`, y el registro basura `{name:"", Id:"x"}` quedó persistido en `Empresas.JSON`. Este es el defecto funcional más serio.

4. **BUG — restricción `"Alfombra" and "Carpas"`** (`funciones_restricciones.py:20,22` y su gemela en clientes `:52,54`). La intención es "no puedes tener alfombra Y carpas a la vez". Pero `"Alfombra" and "Carpas" in recursos` Python lo evalúa como `"Alfombra" and ("Carpas" in recursos)`; como `"Alfombra"` es siempre verdadero, la condición se reduce a *"¿hay Carpas?"*. Verificado: registrar solo `["Carpas"]` (sin alfombra) **bloquea incorrectamente** con el mensaje de alfombras+carpas. Lo correcto sería `"Alfombra" in recursos and "Carpas" in recursos`.

5. **BUG — `("cantante" or "Banda") in personal`** (`funciones_restricciones.py:28` y `:60`). Mismo patrón: `("cantante" or "Banda")` se evalúa a `"cantante"`, así que la regla solo mira si hay `"cantante"` e **ignora `"Banda"`**. Verificado: `restricciones_empresa(["Mesas y sillas"], ["Banda"])` devolvió `True` (sin error) cuando debería exigir micrófono. Lo correcto: `("cantante" in personal or "Banda" in personal)`.

6. **Inconsistencia de mayúsculas en las reglas.** Varias condiciones comparan contra literales que **no coinciden con las opciones del menú**: `"Proyector"` (el menú ofrece `"proyector"`, `main.py:31`), `"Tecnico de proyeccion"` (menú: `"tecnico de proyeccion"`), `"Encargado del stand"` (menú: `"encargado del stand"`), `"Iluminacion ambiental"` (menú: `"iluminacion ambiental"`). Estas restricciones **nunca se disparan** porque el string exacto jamás llega. Solo funcionan las que sí cuadran (Escenario/Luz de escenario, Bocinas/tecnico de sonido).

7. **"Eventos del dia" no hace nada** (`main.py`): opción de menú sin handler.

8. **Validación de fecha pasada ausente.** `crear_eventos` valida duración ≤12h y horas distintas, pero nada impide agendar un evento en una fecha ya pasada.

9. **Flujos válidos que sí funcionan (verificado):** guardar empresa válida → persiste bien; crear evento en fecha libre → persiste con `empresa_id` asignado; ID con menos de 8 dígitos → `error "El ID debe tener 8 digitos"`; duración >12h → `error "el evento debe durar maximo 12 horas"`; borrar empresa existente → se elimina; borrar ID inexistente → mensaje de error apropiado. Ninguno de estos revienta con `Traceback`.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Idiomática de comparaciones:** los bugs 4 y 5 vienen de un malentendido común (`x or y in coll`, `a and b in coll`). Merece la pena interiorizar que `in` solo aplica al operando inmediato.
- **`== True` / `== False`:** `if cumple_recursos == False`, `if encontrado == True` (`funcion_client.py:57,63,88`, `funcion_delete.py:18`). Más idiomático: `if not cumple_recursos`, `if encontrado`.
- **`try/except` demasiado amplio:** `except:` desnudo (`funcion_client.py:123`, `function_empresa.py:37`) captura cualquier cosa; mejor `except (FileNotFoundError, json.JSONDecodeError):`.
- **Nombres mezclados** (español/inglés, `name`/`Id`/`ceo`) y ligera inconsistencia de indentación (mezcla de 2 y 3 espacios en `funcion_client.py`). Menor, pero pasar todo a un estilo (p. ej. 4 espacios) ayuda a leer.
- **Sin manejo de fecha pasada** ni normalización de mayúsculas de las opciones — de ahí el bug 6.

Nada de esto es grave para primer año; son afinamientos.

## Dimensión 5 — Datos y persistencia

- Persistencia en dos JSON (`data/Empresas.JSON`, `data/clientes.JSON`) con estructura `{"Empresas": [...]}` / `{"Eventos": [...]}`. Modelo simple y correcto.
- `convertir()` serializa a dict con `str(fecha)`, `str(hora_inicio)`, etc. (`funcion_client.py:19`) — buen manejo de tipos no serializables.
- **`Empresas.JSON` viene vacío** (`{"Empresas": []}`) pero `clientes.JSON` referencia `empresa_id: "87654321"`: los datos de ejemplo son incoherentes entre sí (hay eventos asignados a una empresa que no existe en el archivo de empresas).
- Reescritura completa del archivo en cada guardado (leer todo → append → volcar). Correcto y suficiente a esta escala.
- **Sin unicidad de ID:** nada impide dos empresas o dos eventos con el mismo ID; `find`/`delete` operan sobre el primero que encuentran.

## Dimensión 6 — Informe (`report.md`)

**No hay `report.md` en el repositorio** — lo confirmó también la verificación automática. Falta el informe. Es un requisito del proyecto: documentar qué hace, cómo está organizado y qué decisiones se tomaron. Sin él no hay nada que contrastar contra el código, pero tampoco hay exageraciones que marcar.

---

## Valoración global (orientativa, sin nota numérica)

Este proyecto tiene un núcleo genuinamente bueno para primer año: el algoritmo que empareja una solicitud de cliente con una empresa que **tenga los recursos y esté libre sin solapar horario ni chocar recursos/personal ya comprometidos** está bien pensado y —lo verifiqué ejecutándolo con datos reales— **funciona**. Detectar conflictos de horario con intersección de intervalos y de recursos es más de lo que muchos proyectos intentan. La modularización en cinco archivos por responsabilidad y el uso de clases con serialización también muestran buen criterio. El costo lo pagan tres tipos de descuido: (a) un `return` faltante que deja pasar empresas inválidas, (b) dos reglas rotas por la precedencia de `and`/`or` con `in`, y (c) reglas que nunca se disparan por mayúsculas que no coinciden con el menú. Ninguno hace reventar el programa —no vi un solo `Traceback` en los flujos probados—, pero sí lo dejan validando menos de lo que aparenta. Sumado a la falta del `report.md` y a la opción de menú muerta, queda la sensación de un proyecto con muy buena idea central y ejecución a medio pulir.

- **Principal fortaleza:** el algoritmo de emparejamiento cliente↔empresa con detección de solapamiento horario y conflicto de recursos (`funcion_client.py:39-97`), que ejecuté y da resultados correctos.
- **Principal área de mejora:** cerrar los tres bugs de validación —el `return` faltante en `guardar_empresa` (`function_empresa.py:22-30`), y la precedencia de `and`/`or` con `in` en las restricciones (`funciones_restricciones.py:20,28,52,60`)— y escribir el `report.md`.
