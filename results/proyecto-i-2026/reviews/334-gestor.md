# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #334
- **Repositorio:** https://github.com/betancourtarlenisroxana-code/Gestor
- **Estudiante:** Arlenis Roxana Betancourt
- **Grupo:** C111
- **Descripción declarada:** Proyecto para organizar bodas (Gestor de Bodas): registrar cliente, comprobar capacidad del local frente al número de invitados, clasificar invitados (familiares/amigos), gestionar bebidas, con interfaz gráfica en Tkinter.

---

## Nota metodológica importante

El repositorio no es un único programa, sino **tres desarrollos paralelos** en distinto grado de madurez, todos bajo `programacion/`:

1. **Versión de consola madura** — `programacion/Gestor de Bodas/` (paquete modular con `models/`, `manager/`, `persistence/`, `utils/`, `main.py`). Es una app de **consola con `input()`**, no la GUI que anuncia el informe. Se ejecutó alimentando el menú con `printf`.
2. **Interfaz gráfica Tkinter** — `programacion/Gestor de Bodas/interfaz/ventana.py`. Es un script **independiente y aislado**: redefine sus propias clases `Cliente/Local/Invitado/Boda` (`ventana.py:6-27`) y no importa nada del paquete `models/`. `main.py` **nunca la lanza**. Se probó en headless (X11) y por lógica de negocio directa.
3. **Prototipo temprano de consola** — `programacion/main.py` + `programacion/menu.py`. Borrador con código muerto comentado, `os.system("cls")` (solo Windows) y un `Manager.agregar_boda` roto. Es claramente material de trabajo previo, no el entregable.

La evaluación se centra en la **versión 1 (paquete de consola)**, que es la funcional y completa, y comenta las otras dos como contexto.

## Dimensión 1 — Qué hace el programa

La app de consola (`Gestor de Bodas/main.py`) presenta un menú de 4 opciones (`utils/menu.py:1-8`):

1. **Crear local** — pide nombre y capacidad, instancia `Local` (`main.py:14-19`).
2. **Crear boda** — exige que exista al menos un local, pide datos del cliente, deja elegir local de una lista numerada, parsea fechas ISO de inicio/fin y crea la `Boda` (`main.py:21-40`).
3. **Ver bodas** — lista las bodas registradas (`main.py:42-43` → `manager.listar_bodas`).
4. **Salir** — persiste todo a JSON y termina (`main.py:45-48`).

El `Manager` guarda una comprobación real de valor: al añadir una boda detecta **solapamiento de fechas en el mismo local** y la rechaza (`manager/manager.py:16-25`). El modelo `Boda` valida `inicio < fin` (`models/boda.py:3-4`) y ofrece `agregar_invitado`, `asignar_asiento` (con control de asiento ocupado y rango) y `pedir_bebida` con descuento de inventario (`models/boda.py:13-48`), aunque el menú de consola **no expone** estos tres métodos: quedan como capacidades del modelo sin punto de entrada en la interfaz.

## Dimensión 2 — Organización del código

La versión de consola está **bien modularizada** para primer año: un paquete por responsabilidad (`models/`, `manager/`, `persistence/`, `utils/`), cada clase en su archivo, `__str__` definido en todos los modelos (`local.py:17-18`, `boda.py:58-59`, `cliente.py:6-7`, `invitado.py:6-7`). La separación modelo/persistencia/orquestación es correcta y demuestra comprensión de la organización de proyectos.

Debilidades concretas:

- **`Manager` con dos `__init__`** (`manager/manager.py:9-14` y `manager/manager.py:31-33`). Python conserva solo el segundo; el primero es código muerto. Funciona por casualidad porque el segundo `__init__` inicializa tanto `locales` como `bodas` vía `cargar_*`. Debe fusionarse en un único `__init__`.
- **Tres proyectos en el mismo repo** sin un README que indique cuál es el entregable. `programacion/main.py`/`menu.py` deberían borrarse o archivarse.
- La **GUI está desconectada** del paquete: duplica clases en vez de reutilizar `models/` (`interfaz/ventana.py:6-27`). Es esencialmente un cuarto proyecto.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Entorno: `uv venv --python 3.12` (CPython 3.12.8). Sin dependencias externas (Tkinter es stdlib). `py_compile` de **los 12 módulos del alumno: todos OK**.

Lo que se corrió sobre la versión de consola:

1. **Flujo completo válido** — crear local "Salon Real" (cap. 100) → crear boda para "Maria Lopez" con fechas `2026-08-15 18:00`/`23:00` → listar → salir. Resultado: `Local creado`, `✅ Boda añadida correctamente`, `1. Boda de Maria Lopez en Salon Real`, `💾 Datos guardados. Saliendo...`. **Sin errores.**
2. **Persistencia round-trip** — tras salir, `data/locales.json` y `data/bodas.json` quedaron correctamente serializados (incluida la lista de 100 asientos `null`). Al **relanzar**, la boda persistida se cargó y apareció en "Ver bodas". La persistencia funciona en ambos sentidos.
3. **Detección de solapamiento** — segunda boda en el mismo local con fechas `19:00`/`22:00` (dentro del rango de la primera): `❌ El local ya está ocupado en esas fechas`. **Correcto.**
4. **JSON vacío `{}`** — los `data/*.json` versionados contienen `{}` (dict vacío). No rompe: `for l in data` itera cero claves y devuelve lista vacía (`persistence/storage.py:66`, `84`). Benigno por suerte, no por diseño.

Bugs del estudiante (fallos reales, no del entorno) — todos por **falta de manejo de errores en `main.py`**:

5. **Fecha mal formada → Traceback.** Entrada `fecha-mala` en inicio: `ValueError: Invalid isoformat string: 'fecha-mala'` (`main.py:36`). El programa **revienta** en vez de reintentar.
6. **Capacidad no numérica → Traceback.** Entrada `abc` en capacidad: `ValueError: invalid literal for int() with base 10: 'abc'` (`main.py:16`).
7. **Capacidad ≤ 0 → Traceback no capturado.** `Local.__init__` hace `raise ValueError` (`models/local.py:3-4`) pero `main.py:17` no lo envuelve en `try/except`, así que el programa cae.
8. **Índice de local fuera de rango** — un número de local inválido en `main.py:33-34` provocaría `IndexError` (no se atrapa). No forzado en la corrida, pero el patrón es idéntico a los anteriores.

Robustez que **sí** funcionó: opción de menú no numérica (`x`) → `Opción inválida` sin caer, porque el menú compara *strings* (`main.py:14,21,42,45,50`), no `int(input())`.

Sobre la GUI (`interfaz/ventana.py`): no arranca en este entorno por un fallo de X11/XCB (`Assertion !xcb_xlib_unknown_seq_number failed`, core dump) — es **problema del entorno headless, no del código**. La lógica de negocio verificada directamente: el parseo de fechas de `guardar_fechas` (`ventana.py:159-167`) acepta `2026-08-15` y rechaza basura con un `messagebox` de error (buen manejo, gracias al `try/except`). La GUI **no** valida `fin > inicio` ni capacidad vs. número de invitados, pese a que el informe presenta esa comprobación como característica central.

Prototipo `menu.py`: arranca pero `os.system("cls")` falla en Linux (`sh: 1: cls: not found`, inocuo); opciones "Ver locales" y "Gestión de bodas" son stubs (`pass`); `Manager.agregar_boda` está roto (`min()` sin argumentos y variable `boda` indefinida, `programacion/main.py:99-101`).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Positivo: nombres claros en español, `dict.get(bebida, 0)` idiomático (`models/local.py:15`), comprehensions para serializar asientos e invitados (`persistence/storage.py:41-51`), `next(...)` con default para buscar local por nombre (`storage.py:91-94`), `try/except FileNotFoundError` en la carga (`storage.py:71`, `120`).

Mejorable:

- **Ninguna entrada numérica en `main.py` está protegida.** `int(input(...))` y `datetime.fromisoformat(input(...))` deben ir en `try/except ValueError` con reintento (raíz de los bugs 5-8).
- En la GUI, `except:` desnudo (`ventana.py:166`) atrapa todo; mejor `except ValueError`.
- El menú de consola no da acceso a `agregar_invitado`/`pedir_bebida`/`asignar_asiento`: funcionalidad implementada pero inalcanzable para el usuario.

## Dimensión 5 — Datos y persistencia

Modelo sólido y el aspecto más maduro del proyecto. `Manager.guardar_todo` serializa locales y bodas a JSON con `indent=4` y `encoding="utf-8"` (`storage.py:25-26`, `53-54`). Las fechas se guardan con `isoformat()` y se recuperan con `fromisoformat()` (`storage.py:39`, `102`). La reconstrucción resuelve las **referencias entre bodas y locales por nombre** (`storage.py:91-94`) y reconstruye el mapa de asientos desde nombres (`storage.py:114-116`) — un manejo de relaciones notable para primer año. Verificado por round-trip real (Dimensión 3, punto 2). Limitación menor: los locales se identifican por nombre, así que dos locales homónimos colisionarían.

## Dimensión 6 — Informe (`README.md`)

El informe está bien redactado y estructurado, pero **describe un programa distinto al entregado**, con varias discrepancias:

- **"Interfaz gráfica conectada directamente con las clases del proyecto"** (README:191) — **falso**. `interfaz/ventana.py` redefine sus propias clases y no importa `models/` (`ventana.py:6-27`); `main.py` ni siquiera la lanza. La app que realmente funciona es de **consola**, no la GUI que el informe presenta como el eje del proyecto.
- **Clase `Bebidas`** — el informe le dedica una sección entera (README:111-127) diciendo que "está integrada dentro de `Local`". **No existe tal clase.** Las bebidas son un simple `dict` en `Local` (`models/local.py:8`), y ni siquiera hay un archivo `bebidas.py` pese a que la estructura del informe lo lista (README:47).
- **Estructura declarada** (README:38-53) no coincide: no hay `interfaz.py` en la raíz, sí hay `manager/` y `persistence/` (los más valiosos) que el informe omite por completo.
- **"El programa compara capacidad del local con el número de invitados"** (README:87-107, 233) — la comprobación estrella del informe **no se ejecuta en ninguna interfaz**: la consola no gestiona invitados y la GUI no compara nada. El método `agregar_invitado` sí limita por capacidad (`boda.py:14`), pero es inalcanzable.
- La conclusión afirma que el proyecto "**demuestra**" el uso de POO (README:265). Es fuerte: hay POO real y modular en el paquete de consola, pero el informe atribuye al conjunto capacidades (GUI integrada, clasificación de invitados operativa, chequeo de capacidad) que **no** están enlazadas en el producto ejecutable.

---

## Valoración global (orientativa, sin nota numérica)

Hay un núcleo de consola **genuinamente bueno** enterrado aquí: un paquete modular limpio, con separación de responsabilidades, persistencia JSON bidireccional que reconstruye relaciones entre objetos, detección de solapamiento de fechas y validaciones en los modelos. Ejecutado de verdad, el flujo principal (crear local → crear boda → listar → guardar → recargar) funciona sin fallos y la persistencia round-trip es correcta. Para primer año, la parte de `persistence/` y `manager/` está por encima del promedio.

Lo que baja el resultado es la **desconexión entre las piezas y el informe**: el repo mezcla tres proyectos sin señalar cuál es el entregable, la GUI que el informe pone en el centro está aislada y no integrada, la "clase Bebidas" y el chequeo de capacidad que el informe describe no existen o son inalcanzables, y `main.py` no protege ninguna entrada, por lo que cualquier dato inválido (fecha, capacidad no numérica) tira el programa con un `Traceback`.

- **Principal fortaleza:** el subsistema de persistencia y el `Manager` de la versión de consola — serialización JSON con reconstrucción de referencias boda↔local y detección de solapamiento de fechas, verificado por round-trip real.
- **Principal área de mejora:** unificar el proyecto (elegir la versión de consola, borrar el prototipo y decidir si integrar de verdad la GUI con `models/`), envolver **toda** entrada de `main.py` en `try/except` con reintento, y corregir el informe para que describa el programa que realmente se ejecuta.
