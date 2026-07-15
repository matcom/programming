# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #295
- **Repositorio:** https://github.com/Shelflogic/Proyecto_de_Pro
- **Estudiante:** Andres F. Bell Barcelo
- **Grupo:** C111
- **Descripción declarada:** Gestor Musical — organizar y gestionar la renta de equipos y personal para eventos (escenarios, consolas de sonido, micrófonos, luces, etc.), simulando el negocio de una productora, controlando que los recursos alcancen y que no se superpongan las fechas entre clientes.

---

## Nota metodológica importante

Es una **aplicación de consola** con `input()`/`print()`. Se ejecutó de verdad alimentando el menú por `stdin` (`printf`/`subprocess`) recorriendo las 8 opciones con datos reales del repo, y además se probaron funciones aisladas (`introducir_fechas`, `comprobar_asignacion_evento`) para validar la lógica de fechas y de asignación de números.

Entorno: `uv venv --python 3.12`. No hay dependencias externas — solo `json`, `datetime` y `copy` de la biblioteca estándar. `requirements.txt` existe pero está vacío. Los tres módulos pasan `py_compile` sin errores.

Aviso técnico: el código usa comillas dobles anidadas dentro de un f-string (`Funciones.py:103`), sintaxis válida solo desde **Python 3.11+** (PEP 701 se consolidó en 3.12). En Python ≤3.10 ese archivo ni siquiera compilaría. Con 3.12/3.13/3.14 corre bien.

## Dimensión 1 — Qué hace el programa

Menú de 8 opciones (`Menu.py:12`) sobre tres archivos JSON de persistencia:

1. **Añadir empresa** (`crear_inventario`, `Funciones.py:7`): pide nombre y cantidad de cada uno de los 16 recursos posibles (`Main.py:4-21`); descarta los recursos con cantidad ≤0 (`Funciones.py:29-30`) y la empresa entera si queda vacía (`Funciones.py:32-33`). Guarda en `inventarios.json`.
2. **Contratar empresa** (`guardar_evento_cliente`, `Funciones.py:60`): asigna un número de evento único, pide fechas, calcula disponibilidad según solapes con otros eventos, sugiere un hueco libre si no hay disponibilidad completa, aplica tres restricciones de negocio, y guarda el contrato en `clientes.json`.
3. **Ver empresas** (`Menu.py:116`): imprime el inventario.
4. **Ver eventos** (`Menu.py:150`): imprime todos los contratos.
5. **Cancelar evento** (`Menu.py:190`): elimina un evento por su número y libera el número en `lista_asignacion.json`.
6. **Eliminar inventario** (`Menu.py:237`): borra una empresa, **con guarda** que impide borrarla si algún cliente la tiene reservada.
7. **Detalle de un evento** (`Menu.py:291`): muestra un contrato concreto.
8. **Salir** (`Menu.py:103`): `raise SystemExit`.

Verificado en ejecución: las opciones 3, 4 y 7 imprimen correctamente el estado guardado (empresa `sas` con 2 de cada recurso; `evento_1` y `evento_2` con periodos 2010-10-02/03 y 2010-10-10/12).

## Dimensión 2 — Organización del código

**Fortalezas:**
- Separación real en tres archivos: `Main.py` (datos + arranque), `Menu.py` (bucle de menú + I/O de archivos), `Funciones.py` (lógica de negocio). Es una modularización más ambiciosa que la de muchos proyectos de este nivel (`Main.py:1`, `Menu.py:1-2`).
- Funciones con responsabilidad definida: `introducir_fechas` (`Funciones.py:190`), `comprobar_disponibilidad` (`Funciones.py:230`), `comprobar_asignacion_evento` (`Funciones.py:259`), `completa_disponibilidad` (`Funciones.py:282`). Cada una hace una cosa.
- Uso deliberado de `copy.deepcopy` (`Funciones.py:77-78`) para no mutar los datos originales al calcular disponibilidad — muestra conciencia del problema de aliasing, poco común en 1er año.

**Debilidades:**
- **La lectura/escritura de archivos está enredada en `Menu.py`** en vez de encapsularse. El mismo patrón `try: open(...) json.load ... except: open(...,'w')` se repite en las opciones 1 y 2 (`Menu.py:30-40`, `Menu.py:60-89`), con `except:` desnudos que ocultan cualquier error.
- **`menu()` se llama recursivamente al final de cada iteración** (`Menu.py:333`). Como cada opción hace `check=False`/`break` y luego re-invoca `menu()`, la profundidad de recursión crece con cada operación. Funciona en la práctica porque nadie hace miles de operaciones seguidas, pero es un antipatrón: un `while True` bastaría y evitaría el crecimiento del stack.
- Nombres inconsistentes/con typos: `azul` para el resultado de guardar (`Menu.py:32`), `disenaor de escenario` (`Main.py:19`), `evntos` (`Menu.py:297`). El código funciona pero cuesta leerlo.
- Bloques enormes de líneas en blanco (decenas entre `elif`) inflan los archivos sin aportar.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente se **ejecutó**:

1. **Opciones 3/4/7 (lectura):** correctas. Imprimen inventario y eventos con los datos reales del repo.
2. **Contrato completo exitoso (opción 2):** rentando la empresa `sas` para 2011-01-05/06 (sin solape), rellenando las 14 cantidades y confirmando "si" → RC 0, se guardó `evento_3` con todos los recursos y su `periodo`, y `lista_asignacion.json` pasó a `[1,2,3]`. **Funciona.**
3. **Cálculo de disponibilidad por solape (núcleo del proyecto):** correcto. Rentando `sas` para 2010-10-02/03 (que solapa `evento_1`, el cual ya reserva 1 de cada recurso sobre un stock de 2), la disponibilidad mostrada fue exactamente 1 para los recursos que `evento_1` ocupa y 2 para `consola de luces inteligente`/`luces con efectos` (que `evento_1` no reservó). **La lógica de intervalos funciona.**
4. **Sugerencia de hueco libre (`completa_disponibilidad`):** correcto. En el mismo caso, propuso "desde 2010-10-03 hasta 2010-10-10" — el hueco real entre `evento_1` (fin 10-03) y `evento_2` (inicio 10-10). **Funciona.**
5. **Restricción co-requisito (consola de sonido ⇒ ingeniero):** verificada. Al rentar una consola de sonido con 0 ingenieros, imprime "Debe rentar al menos un ingeniero de sonido" y reprompt (`Funciones.py:150-154`).
6. **Restricciones de exclusión de luces:** verificadas indirectamente — al rentar consola de luces tradicionales, los prompts de `consola de luces inteligente` y `luces con efectos` se saltan correctamente (14 prompts en vez de 16, `Funciones.py:128-133`).
7. **Validación de fechas (`introducir_fechas`):** correcta en aislamiento. `31-13-2020` → "Fecha invalida" y reprompt; inicio posterior a fin → "la fecha de inicio es posterior a la fecha de fin" y reprompt (`Funciones.py:198-207`).
8. **Empresa inexistente en contrato:** correcto — "El nombre de esa empresa no fue encotrado" y reprompt (`Funciones.py:164-166`).
9. **Cantidad no numérica / cantidad > disponible:** correcto — reprompt sin reventar (`Funciones.py:141-147`).
10. **Crear empresa con cantidades 0 y una no numérica:** correcto — la "x" se rechaza, solo se guarda el recurso >0, la empresa vacía se descarta (`Funciones.py:29-33`). También verificado que empresa-toda-cero + responder "no" NO produce `KeyError` (recupera y re-pregunta el nombre).
11. **Opción 5 (cancelar evento single-digit):** correcto — `evento_2` eliminado, `clientes.json` quedó con `evento_1`, `lista_asignacion` volvió a `[1]`.
12. **Opción 6 (guarda de eliminación de inventario):** correcto en los tres casos: bloquea si hay evento que la referencia ("No es posible eliminar este inventario…"), permite si `clientes.json` está vacío ("inventario sas eliminado correctamente"), y avisa si la empresa no existe.
13. **`comprobar_asignacion_evento`:** verificado en aislamiento — asigna el menor número libre y **reutiliza huecos** (lista `[2,3]` → asigna 1). Buen diseño.

**Bugs reales encontrados (ejecutados, no leídos):**

- **B1 — Fuga de números de evento en contratos abortados.** El número se escribe en `lista_asignacion.json` al *inicio* de `guardar_evento_cliente` (`Funciones.py:63-73`), antes de confirmar. Si el usuario aborta (o el flujo se interrumpe), el número queda quemado permanentemente en la lista aunque no se guarde ningún evento en `clientes.json`. Reproducido: tras abortar contratos, `lista_asignacion.json` creció a `[1,2,3,4]` sin que existieran los eventos 3 y 4.
- **B2 — Cancelación rota para números de evento ≥10.** En `Menu.py:218`, `lista.remove(int(ver_evento_cliente[-1::]))` toma **solo el último carácter** del identificador. Para `evento_12` intentaría `lista.remove(2)` — elimina el número equivocado y corrompe `lista_asignacion.json` (o lanza `ValueError` si el 2 no está). Lo correcto sería `ver_evento_cliente.split("_")[1]`. El mismo patrón `cliente[-1::]` aparece en `Funciones.py:66` (ahí solo afecta el mensaje que se imprime, es cosmético).
- **B3 — Truncado de `clientes.json` ante error a media escritura.** En `Menu.py:82-88`, el archivo se abre en modo `'w'` (lo trunca de inmediato) y *después* se ejecuta `guardar_evento_cliente`, que pide input. Si algo falla ahí, el `except` de `Menu.py:86` re-invoca la función con un dict vacío, dejando `clientes.json` en 0 bytes. Reproducido al interrumpir el input a media contratación: `clientes.json` quedó vacío. En uso interactivo normal no se dispara, pero es frágil.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **`except:` desnudos** por todos lados (`Menu.py:37`, `Menu.py:86`, `Funciones.py:70`, etc.). Ocultan errores reales y son la causa raíz de B3. Recomendación: capturar `FileNotFoundError`/`json.JSONDecodeError` explícitamente — de hecho el estudiante ya lo hace bien en `Menu.py:74` y `Funciones.py:27`, solo falta generalizarlo.
- **Comparaciones redundantes:** `if x is None`, `if x == {}`, `if len(x) == 0` repetidas seguidas (`Menu.py:67-72`, `119-126`). Con un simple `if not x:` se cubren los tres casos.
- **I/O de archivos repetida a mano** en cada opción del menú. Dos funciones `cargar(path)` / `guardar(path, data)` limpiarían muchísimo `Menu.py`.
- **Recursión en `menu()`** (ver Dimensión 2): sustituir por bucle.
- Puntos positivos idiomáticos: uso correcto de `with open(...)`, `json.dump(..., ensure_ascii=False, indent=2)`, `datetime.strptime`, y `deepcopy`.

## Dimensión 5 — Datos y persistencia

Modelo de tres archivos JSON con separación de conceptos, coherente con lo declarado:
- `inventarios.json`: `{empresa: {recurso: cantidad}}`.
- `clientes.json`: `{evento_N: {empresa: {recurso: cantidad, ..., periodo: {inicio, fin}}}}` — diccionarios anidados de 3-4 niveles, manejados correctamente.
- `lista_asignacion.json`: lista de números de evento en uso, que permite reutilizar huecos al cancelar. Es una idea de diseño madura.

Las fechas se serializan como `str(datetime)` → `"YYYY-MM-DD HH:MM:SS"` y se re-parsean con `strptime`. Funciona, aunque mezcla el formato de entrada del usuario (`DD-MM-YYYY`) con el de almacenamiento (`YYYY-MM-DD HH:MM:SS`), algo que hay que tener presente al leer el código. La debilidad de persistencia es B1/B3: la escritura no es transaccional (el número se compromete antes de confirmar; el archivo se trunca antes de tener el dato final).

## Dimensión 6 — Informe (`REPORT.md`)

Existe `REPORT.md` (1262 palabras; hay una copia idéntica `REPORT.txt`). La verificación automática lo marcó como ausente porque buscó `report.md` en minúsculas y pedía ≥2000 palabras — el archivo sí está, con nombre en mayúsculas.

Es un informe **honesto y bien escrito**. Coincide con el código real:
- Describe las tres restricciones de negocio tal como están implementadas (consola de sonido⇒ingeniero, exclusión de consolas de luces, luces con efectos⇒consola inteligente). **Verificado que las tres funcionan.**
- Describe correctamente el número de asignación único reutilizable y la sugerencia de hueco libre. **Ambos verificados.**
- La sección "problemas que enfrenté" (fechas como texto, ordenar intervalos, persistencia) es genuina y refleja el código.

No exagera: no dice "probé exhaustivamente" ni "demuestra". Es una descripción fiel de lo construido. La única discrepancia menor: el informe no menciona las limitaciones B1/B2/B3 (esperable, son bugs sutiles), y afirma que el número sirve "para buscar o borrar los datos sin alterar los eventos de los demás" (REPORT.md:45) — cierto salvo por B2 con números ≥10.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y ambicioso** para primer año. El corazón del problema — controlar disponibilidad de recursos por intervalos de fecha con solapes entre clientes, y sugerir el próximo hueco libre — está genuinamente resuelto y **verificado en ejecución**: la disponibilidad se recalcula bien, el hueco propuesto es correcto, y las tres restricciones de negocio funcionan. La separación en tres módulos y el uso deliberado de `deepcopy` y de una lista de asignación reutilizable muestran un nivel de diseño por encima del promedio del curso. El informe es honesto y coincide con el código.

Los defectos son de robustez y estilo, no de concepto: `except:` desnudos que ocultan errores (causa de un truncado real de `clientes.json`), la extracción del número de evento con `[-1::]` que rompe la cancelación a partir del evento 10, la fuga de números en contratos abortados, y la recursión innecesaria de `menu()`. Ninguno invalida el logro central, pero conviene corregirlos.

- **Principal fortaleza:** la lógica de disponibilidad por solape de fechas y la sugerencia automática de hueco libre — el núcleo difícil del proyecto — funciona de verdad, comprobado con datos reales.
- **Principal área de mejora:** robustez de la persistencia y del manejo de identificadores: sustituir `except:` desnudos por excepciones concretas, cambiar `ver_evento_cliente[-1::]` por `split("_")[1]`, y asignar/persistir el número de evento solo tras confirmar el contrato.
