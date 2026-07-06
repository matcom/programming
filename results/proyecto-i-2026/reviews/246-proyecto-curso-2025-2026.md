# Reporte de evaluación — Proyecto I (1er año CC)

- **Estudiante:** Eviell Armenteros Martinez
- **Grupo:** C-122
- **Issue:** #246
- **Repositorio:** https://github.com/eviellthepolvora-eng/proyecto-curso-2025-2026
- **Descripción del issue:** "sistema de gestión integral que controla el proceso desde la venta hasta la entrega: gestión de pedidos/facturas y control de inventario".

---

## Resumen de ejecución

Se clonó el repo, se copió a un directorio de trabajo aislado (para no ensuciar los
JSON del original) y se ejecutó con Python 3.14 (sin dependencias externas: solo
`json` y `datetime` de la stdlib). Los 6 módulos compilan sin errores de sintaxis.

**El programa arranca y su núcleo funciona de punta a punta.** Se recorrieron todas
las opciones del menú alimentando inputs por stdin. Funcionan: ver inventario,
stock bajo, actualizar inventario (persiste), listar y entregar pedidos del día
(asignación recursiva de transportistas + movimiento a `entregado.json`), crear una
factura completa (reduce stock y persiste), y eliminar factura (doble confirmación +
devuelve stock al almacén). **Tres flujos lanzan `Traceback` real con inputs
inválidos** (fecha mal tecleada al arrancar, y transportador no numérico), detallados
en la dimensión 3.

---

## Dimensión 1 — Qué hace el programa

Aplicación de consola que gestiona la distribución de motocicletas: inventario por
modelo, facturas/pedidos con datos de cliente, asignación de transportistas y
persistencia en tres archivos JSON. El dominio real coincide con la descripción del
issue (venta → factura → entrega).

- **Punto de entrada:** `main.py.py:123` (`if __name__ == '__main__': main()`). El
  nombre del archivo tiene doble extensión (`main.py.py`), lo que obliga a invocar
  `python3 'main.py.py'` — probablemente un descuido al guardar.
- **Cómo se ejecuta:** al arrancar (`main.py.py:95`) pide inmediatamente la fecha de
  "hoy" (día/mes/año) vía `fechinguiri().date()`, y luego entra al menú principal con
  4 opciones: (1) trabajos pendientes → submenú entregar/eliminar, (2) tomar nuevos
  pedidos → sugiere fecha + crea factura, (3) verificar disponibilidad → submenú de
  inventario, (4) salir.
- **Flujo principal:** el proyecto está repartido en 6 módulos con responsabilidades
  claras (`main.py.py` orquesta; `impor.py` eventos/persistencia; `factura_PERFECTA.py`
  creación de factura; `transporte.py` transportistas; `almacen_clases.py` inventario;
  `todo_relacionado_con_fecha.py` fechas). El estudiante logró que módulos separados
  colaboren mediante `import`, algo notable para un primer proyecto.

## Dimensión 2 — Organización del código

Muy por encima del promedio de un primer año en cuanto a **separación en módulos**:
5 archivos de dominio + 1 orquestador, cada uno con una clase con responsabilidad
acotada.

- **Uso de clases:** `eventoun` (`impor.py:3`), `factura` (`factura_PERFECTA.py:3`),
  `transportista` (`transporte.py:3`), `almacen` (`almacen_clases.py:3`),
  `fechinguiri` (`todo_relacionado_con_fecha.py:2`). Buen instinto para agrupar
  estado + comportamiento.
- **Funciones para no repetir:** `main.py.py` factoriza los menús en
  `mostrar_menu_principal/eventos/almacen` y la lógica de entrega en
  `procesar_pedidos_hoy` (`main.py.py:43`). Bien.
- **Nombres:** mezcla. Los métodos de dominio son razonables (`agregar_factura`,
  `reduccion_cantidad_modelo`, `carro_disponible`). Pero hay identificadores
  informales/confusos que restan legibilidad: la clase `fechinguiri`, la variable
  `banana` (`impor.py:47`), `z` (`impor.py:40`), el método `go()`, `brr` comentado.
  Sugerencia: nombres descriptivos (`gestor_fechas`, `nuevo_evento`).
- **Código muerto:** bloques grandes comentados en `impor.py:112-130`,
  `transporte.py:49-58`, `todo_relacionado_con_fecha.py:74-86`, y las dos líneas de
  prueba al final de `factura_PERFECTA.py:160`. Conviene borrarlos antes de entregar.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Lo que SÍ funciona al correr** (verificado con inputs por stdin):

- **Ver inventario** (`3 → 1`): imprime la tabla desde `almacen.json` correctamente
  (`almacen_clases.py:24`).
- **Stock bajo** (`3 → 3`): imprime el dict de modelos con ≤5 unidades
  (`almacen_clases.py:43`). Funciona.
- **Actualizar inventario** (`3 → 2`): recoge cantidades por modelo y persiste. Probado:
  pasó a `10/8/6/2/9` y quedó guardado en `almacen.json` (`almacen_clases.py:9`).
- **Entregar pedidos de hoy** (`1 → 1`): con la fecha de hoy = `(2026, 6, 17)` (la que
  el usuario teclea al arrancar), `filtrado` (`impor.py:105`) encontró los 3 eventos
  de `eventos.json`, asignó Pancho/Juan/Ernesto vía recursión
  (`transporte.py:29-48`), los movió a `entregado.json` (de 5 → 8 registros) y dejó
  `eventos.json` vacío. **Persistencia correcta.**
- **Crear factura completa** (`2`): recogió modelo (Yamaha ×2), dirección, nombre,
  teléfono, cuño; **redujo el stock de Yamaha de 4 → 2** (`almacen_clases.py:38`) y
  guardó el evento con fecha `(2026, 6, 20)` en `eventos.json`. Flujo completo OK.
- **Eliminar factura** (`1 → 2`): doble confirmación (`impor.py:53`), eliminó la
  factura de Honda ×7 y **devolvió el stock al almacén** (Honda 4 → 11). OK.

**Tracebacks reales observados (bugs que rompen el programa):**

1. **Crash al arrancar con fecha mal tecleada.** Input `abc` en "Dia:" →
   `entero()` (`todo_relacionado_con_fecha.py:5`) imprime "Introduce un numero
   valido" pero **retorna `None`** (falta un `return` en la rama else / no re-pide).
   Ese `None` llega a `date()` y explota:
   ```
   File "todo_relacionado_con_fecha.py", line 37, in date
       if mes % 2 == 0 and dia > 0 and dia <= 30 :
   TypeError: '>' not supported between instances of 'NoneType' and 'int'
   ```
   Como esto ocurre en la primerísima interacción (`main.py.py:95`), un solo typo al
   arrancar tumba todo el programa.

2. **Crash al arrancar con fecha imposible.** Input `31/2/2026` → tras "repita la
   fecha" hace `break` sin haber asignado la variable local `date`
   (`todo_relacionado_con_fecha.py:60`, dentro del `while`):
   ```
   File "todo_relacionado_con_fecha.py", line 62, in date
       return date
   UnboundLocalError: cannot access local variable 'date' where it is not associated with a value
   ```
   La rama de error debería re-pedir la fecha o devolver `None` de forma controlada,
   no caer al `return date`.

3. **Crash al escoger transportador no numérico.** En la entrega, input `xyz` en
   "Escoja el transportador":
   ```
   File "transporte.py", line 21, in carro_disponible
       self.select = int(input(f"Escoja el transportador\n"))
   ValueError: invalid literal for int() with base 10: 'xyz'
   ```
   `int(input(...))` sin `try/except` ni `.isdigit()`. Contrasta con el resto del
   código, que sí valida con `.isdigit()` en casi todos lados.

**Validación de entradas:** en general **buena** para ser 1er año — casi todos los
`input()` numéricos usan `.isdigit()` y bucles `while` que re-piden
(`factura_PERFECTA.py:26-57`, teléfono de 8 dígitos en `factura_PERFECTA.py:113`,
cantidad ≤ stock). Las tres grietas están justamente donde falta ese patrón
(`entero()`, `date()` en el caso imposible, y `carro_disponible`).

**Coincidencia con el issue/informe:** el programa hace lo que el issue promete
(venta → factura → inventario → entrega). Sí hay discrepancias con el `report.md`
(ver dimensión 6).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Positivo:** f-strings usadas con soltura; `try/except (FileNotFoundError,
  json.JSONDecodeError)` en toda la carga de JSON (`impor.py:24`,
  `almacen_clases.py:32`) — manejo de errores por encima del promedio del nivel;
  `enumerate(..., start=1)` para menús; `.strip()` en los inputs de menú.
- **A mejorar:**
  - Indentación inconsistente en un método clave: `guardar_entregado`
    (`impor.py:7`) tiene el cuerpo sangrado de más (8 espacios bajo el `def`);
    funciona pero es confuso. La lógica de `almacenado` (`almacen_clases.py:9-23`) usa
    un `for/else` con `return print(...)` mezclando efectos y valores.
  - `int(input())` sin proteger en `transporte.py:21` (fuente del crash #3) y
    `entero()` que retorna `None` implícito (crash #1). Envolver en `try/except` o
    validar antes.
  - `return print(...)` en `almacen_clases.py:23,48` devuelve `None` innecesariamente;
    mejor separar `print` del `return`.
  - Línea muerta sin efecto: `not cuño or cuño !="SI"` en `factura_PERFECTA.py:131`
    (evalúa una expresión y la descarta).

  No se penaliza ausencia de tests ni type hints (correcto para el nivel).

## Dimensión 5 — Datos y persistencia

**Bien resuelta.** Tres JSON (`almacen.json`, `eventos.json`, `entregado.json`) con
carga/guardado consistentes y `indent=4`. Verifiqué en ejecución que:
- crear factura reduce stock y persiste el evento,
- entregar mueve de `eventos.json` a `entregado.json`,
- eliminar devuelve stock y quita el evento.

Observaciones:

- **Fecha guardada como string de tupla** (`str((2026,6,20))` → `"(2026, 6, 20)"`) en
  vez de un formato estándar (`"2026-06-20"`). Funciona porque toda la comparación es
  string-vs-string (`impor.py:108`, `filtrado`), pero es frágil: cualquier cambio en
  el formato rompe el filtrado silenciosamente. Sugerencia: `datetime.date` o ISO.
- **Estructura anidada rara en `entregado.json`:** los registros guardan
  `factura → {fecha, factura}` (doble anidamiento, `transporte.py:41`
  `ev.guardar_entregado(primer, dispo)` donde `primer` es el evento completo). Es
  inconsistente con la estructura plana de `eventos.json`.
- **`obtener_cantidad_disponible` vs stock real:** el chequeo de stock al crear
  factura (`factura_PERFECTA.py:13`) lee de un `almacen()` recién instanciado; funciona
  porque se carga del JSON, pero coexisten varias instancias de `almacen` en memoria
  (una en `factura`, otra en `main`) sincronizadas solo vía disco. Para el nivel está
  bien, pero es una fuente potencial de estado inconsistente.

## Dimensión 6 — Informe (`report.md`)

Informe **muy extenso y bien estructurado** (dominio, componentes, restricciones,
operaciones, estructura de datos, ejemplos, sección honesta de "aprendizaje durante
el desarrollo"). Esa última sección, contando las trabas reales (condicionales →
bucles → métodos → clases → imports), es lo mejor del informe: refleja aprendizaje
genuino.

**Pero sobreestima y no coincide en varios puntos con el código ejecutado:**

- **Menú descrito ≠ menú real.** El informe (`report.md:422-433`) describe 7 opciones
  ("Listar/Agregar/Eliminar/Ver detalle/Buscar hueco/Ver inventario/Reducir entrega").
  El menú que corre tiene **4 opciones** con submenús distintos. "Ver detalle de
  evento" (opción 4 del informe) **no existe** en el código.
- **"Interfaz CLI con manejo de errores robusto"** (`report.md:13`): parcialmente
  falso — hay 3 `Traceback` reproducibles con inputs inválidos triviales (ver
  dimensión 3).
- **"Libera transportista después de completar entrega"** (`report.md:98,703`): el
  código hace `self.trans.remove(dispo)` (`transporte.py:37`) y **nunca lo vuelve a
  agregar**; los transportistas no se reciclan dentro de una sesión.
- **Nombre del modelo:** el informe dice "Trek" (`report.md:72,328`), el código y los
  datos usan "Treck" (`almacen_clases.py:8`).
- **Números de ejemplo inventados:** el inventario "actual" del informe
  (`report.md:67-76`, 5/3/2/4/1) no coincide con `almacen.json` real (4/4/4/1/4).
  Normal en un borrador, pero conviene alinearlo.
- **Formato de fecha:** el informe muestra `(15, 3, 2024)` (día, mes, año) pero el
  código guarda `(año, mes, día)` (`main.py.py:95` → `date()` retorna `(ano,mes,dia)`).

En síntesis: el informe describe una versión más ambiciosa/pulida que la que corre.
Documenta bien el diseño pero infla algunas features.

---

## Valoración global

Trabajo **sólido y por encima del promedio de un primer proyecto**, sobre todo por la
modularización real (6 módulos con clases que colaboran vía imports) y la persistencia
en JSON, que funcionan de verdad al ejecutar. El estudiante logró un flujo completo
venta → factura → inventario → entrega con validaciones no triviales.

**Principal fortaleza:** arquitectura en módulos/clases y persistencia funcional.
**Principal debilidad:** tres `Traceback` con inputs inválidos triviales (uno de ellos
tumba el programa en el primer prompt), más un informe que sobreestima features
inexistentes. Blindar los tres `int(input())`/`entero()` sin validar convertiría esto
en un programa robusto.
