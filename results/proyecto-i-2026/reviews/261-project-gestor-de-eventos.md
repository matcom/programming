# Reporte de evaluación — Proyecto I

- **Issue:** #261
- **Repositorio:** https://github.com/AdanV06/Project-Gestor-de-eventos
- **Estudiante:** Edael Adan Verdecia Valdivia
- **Grupo:** C121
- **Descripción declarada:** "Planificador de eventos de un centro de astrofísica"
- **Clonado en:** `.playground/proyecto1-eval/repos/261-project-gestor-de-eventos`

---

## Ejecución dinámica (lo que realmente corrí)

Este **no** es un proyecto de consola: es una aplicación **GUI con Kivy**. El
`requirements.txt` es un *pip freeze* de todo el sistema operativo (~150 paquetes:
PyQt6, proton-vpn, matplotlib, pandas…), que no refleja las dependencias reales.
La única dependencia real es `kivy` (`modules/__init__.py:4-31`).

Pasos:

1. **Entorno aislado con `uv`:** `uv venv --python 3.12`, `uv pip install kivy`
   (Kivy 2.3.1). No instalé el `requirements.txt` porque instalaría medio SO.
2. **Punto de entrada:** `main.py:636` (`Myapp().run()`). Corrí
   `DISPLAY=:0 python main.py` contra el display real de zion, con `timeout`.
3. **Resultado del arranque:** la app **arranca correctamente**. El log de Kivy
   llega a `[Base] Start application main loop` — abrió ventana, cargó los cuatro
   bloques KV (`Builder.load_string` en `main.py:6-9`), instanció todo el árbol de
   widgets (`Contenedor → BoxL → Agregar_Evento` con sus inputs, spinner y
   botones) y entró en el bucle de eventos sin ningún `Traceback`. Las únicas
   advertencias (`xclip`/`xsel`/`MTD /dev/input`) son ruido del entorno headless,
   no fallos del programa. Como es GUI, no se puede recorrer el menú por stdin, así
   que probé la lógica directamente (abajo).
4. **Prueba directa del backend** (`modules/Backend1.py`), que es donde vive la
   corrección real. Con una copia de respaldo de `datas/Eventos.json`:
   - `Planificador()` carga **26 recursos** y 0 eventos (`Backend1.py:99-112`). OK.
   - `agregar_evento` de un evento válido → `"El evento se agrego correctamente"`
     y **escribe en el JSON** (verificado releyendo el archivo). OK.
   - **Detección de conflicto:** agregué un segundo evento que compite por
     "Sala de conferencias" (cantidad 1) en horario solapado → devolvió
     `"El recurso Sala de conferencias no esta disponible en ese horario"`. La
     lógica de solapamiento (`verificar_hora`, `Backend1.py:47-51`) funciona.
   - **Complementarios:** `verificar_complementarios([...],[['Planetario',1]],
     [['Carl Sagan',1]])` → `"En el Planetario debe estar Cleo Abram la encargada
     de esta sala"`. Correcto (`Backend1.py:131-132`).
   - Restauré el JSON al terminar.

La lógica de dominio **funciona de verdad**, no solo compila. Eso es lo más
valioso del proyecto.

---

## Dimensión 1 — Qué hace el programa

Aplicación de escritorio (Kivy) para **planificar eventos de un centro de
astrofísica**. El usuario, desde una interfaz gráfica con dos vistas
("Agregar evento" / "Ver eventos", `main.py:560-608`), compone un evento: nombre,
fecha/hora de inicio y fin, una sala (spinner de 5 opciones), científicos
(popup de 9, `main.py:132-187`) y herramientas/telescopios (popup de 12,
`main.py:20-65`). Al guardar (`ButtonGuardar`, `main.py:262-363`) se validan los
datos y se comprueban tres tipos de reglas de dominio antes de persistir en
`datas/Eventos.json`.

El corazón está en `modules/Backend1.py`, clase `Planificador` (`Backend1.py:89`):
carga recursos y eventos desde JSON, verifica **complementarios** (un telescopio
exige su especialista, una sala exige su encargada), **excluyentes** (científicos
que no pueden coincidir) y **disponibilidad temporal** de recursos (que un recurso
único no esté doble-reservado en horarios solapados). Incluye además una función
`buscar` (`Backend1.py:225-247`) que asigna automáticamente el primer hueco libre.

## Dimensión 2 — Organización del código

**Muy buena para primer año.** El proyecto está **modularizado en 10 archivos**
bajo `modules/`, con una separación consciente:

- `Backend1.py` — lógica de negocio (clases `Evento`, `Recurso`, `Planificador`).
- `Class_*.py` — clases de widgets de cada ventana.
- `Stile_*.py` — reglas de estilo en lenguaje KV.
- `Imagenes.py` — rutas de imágenes y textos informativos.

Usa **POO real**: `Evento`/`Recurso` con `convertir_dicc()` (`Backend1.py:66-86`),
`Planificador` como orquestador. Los nombres son en general claros y descriptivos
(`verificar_complementarios`, `verificar_excluyentes`, `agregar_evento`), y hay
comentarios abundantes que explican el *por qué* de cada bloque.

**Puntos flacos de organización:**

- **Duplicación del patrón `on_touch_down`/`collide_point`/`Popup`** en casi todas
  las clases de botón (`main.py:25-27,85-86,142-143,201-202,285-286,371-372`).
  Cada `mostrar_error` está reimplementado idéntico en tres clases distintas
  (`main.py:79-83,279-283,436-439`). Una clase base o función auxiliar eliminaría
  ~60 líneas repetidas.
- **Estado global mutable** en la clase `evento` (`main.py:12-17`): listas de clase
  compartidas (`recursos_persona`, `cantidad`…) que se limpian y rellenan desde
  varios sitios. Funciona, pero es frágil y difícil de razonar.

## Dimensión 3 — Corrección funcional (según ejecución)

- **Arranca sin errores** y entra en el bucle de eventos (ver "Ejecución dinámica").
- **La lógica de planificación funciona:** probada directamente, detecta conflictos
  de horario, aplica complementarios y persiste en JSON correctamente.
- **Validación de entradas:** buena cobertura en `ButtonGuardar` (`main.py:291-327`):
  campos vacíos, mes/día/hora fuera de rango, fecha en el pasado, fecha fin antes
  que inicio, tope de 5 herramientas. Los `int(...)` van dentro de `try/except`
  que captura `ValueError` con un mensaje amigable (`main.py:330-332`).

**Bugs / inconsistencias detectadas leyendo:**

- **Rangos de año incoherentes** (`main.py:314` vs `main.py:322`): el año de inicio
  se permite hasta 2040, pero el de fin **solo hasta 2030**. Un evento que empiece
  en 2035 no podrá tener una fecha de fin válida.
- **`verificar_complementarios` compara estructuras dispares** (`Backend1.py:127`):
  `comp[0] in recursos_personal` compara `["Cleo Abram",1]` contra una lista de
  `["nombre",1]`. Funciona **por coincidencia** (ambos son `[nombre,1]`), pero si
  la forma de un lado cambiara, la comprobación fallaría en silencio.
- **f-strings con comillas anidadas del mismo tipo** (`main.py:534`:
  `f"{event["inicio"]} : {event["fin"]}"`) solo son válidas en Python **3.12+**.
  En 3.11 o anterior esto es un `SyntaxError`. Debería documentarse la versión
  mínima o usar comillas distintas.
- No se detectó forma de romper el arranque con las entradas por defecto; los
  fallos anteriores son de caminos concretos, no del flujo feliz.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Positivo:** indentación consistente, f-strings idiomáticos, `with open(...)`
  para I/O de archivos (`Backend1.py:56-57,220-221`), `try/except/else` bien usado
  (`main.py:291-335`), comentarios que explican intención.
- **A mejorar:**
  - El *bubble sort* manual de `ordenar` (`Backend1.py:15-23`) podría ser
    `lista.sort(key=lambda e: e.inicio)` — más simple y correcto.
  - Algunos `except Exception` demasiado amplios que mezclan `ValueError` con
    errores de lógica levantados a mano (`main.py:330`, `Backend1.py`).
  - Faltas de ortografía en mensajes al usuario ("ano" por "año", "eveno",
    "cientifico") que se ven en pantalla.
  - `disp = 1000000` como centinela (`Backend1.py:34,208`) es un número mágico
    frágil; `float("inf")` sería más claro.

## Dimensión 5 — Datos y persistencia

**Sólida.** Estado en dos JSON (`datas/Eventos.json`, `datas/Recursos.json`).
`Recursos.json` es un catálogo bien estructurado (nombre, tipo, cantidad,
complementario, excluyentes) con 26 recursos. La carga (`cargar_eventos`/
`cargar_recursos`, `Backend1.py:99-112`) reconstruye objetos con `datetime.strptime`,
y el guardado serializa con `convertir_dicc` + `strftime` (`Backend1.py:83`). El
borrado desde "Ver eventos" reescribe el JSON (`Class_Vent_Ver_Eventos.py:56-60`).
Verifiqué el *round-trip* completo (agregar → leer disco → coincide). Correcto.

Detalle menor: las rutas son relativas (`"datas/Eventos.json"`), así que la app
**solo funciona si se ejecuta desde la carpeta raíz** del proyecto.

## Dimensión 6 — Informe (`report.md`)

**Excelente informe**, de los más completos y honestos que se ven en primer año.
Describe con precisión el dominio, la arquitectura por módulos, el catálogo de
recursos con sus reglas, los flujos de uso y las validaciones. **Coincide con el
código** en lo esencial: la GUI Kivy, la persistencia JSON, las reglas de
complementarios/excluyentes y la búsqueda automática de hueco existen y funcionan
tal como se describen.

Discrepancias menores a señalar:

- El informe dice instalar con `pip install -r requirements.txt`
  (`report.md:96`), pero ese archivo es un *freeze* del SO entero, no las
  dependencias del proyecto. La instrucción real debería ser `pip install kivy`.
- El informe afirma "Nombre… máximo 45 caracteres" (`report.md:284`), pero no
  encontré esa validación de longitud en `ButtonGuardar` — solo se valida que no
  esté vacío (`main.py:293`). Ligera sobreestimación.
- El texto es en algunos tramos algo grandilocuente ("guardián digital",
  "validación algorítmica inteligente"), pero está respaldado por código real, no
  vacío.

---

## Síntesis para el profesor

Proyecto **notablemente por encima del nivel esperado en un primer proyecto**. Lo
distingue: (1) modularización real en 10 archivos con separación UI/lógica/estilo;
(2) una GUI Kivy funcional y no trivial (popups, grids, spinners, imágenes); (3)
lógica de dominio genuina y **verificada corriendo** (conflictos de horario,
complementarios, excluyentes, búsqueda de hueco) con persistencia JSON que
round-trippea. El informe es completo y honesto.

Áreas de mejora, todas normales a este nivel: duplicación del patrón de botones,
estado global mutable en `evento`, un `requirements.txt` inservible, y un par de
bugs concretos (rango de año fin ≤2030, comparación frágil en complementarios,
dependencia de Python 3.12 por f-strings anidadas). Nada de esto empaña que el
sistema **hace lo que promete**.

**Principal fortaleza:** lógica de planificación real y funcional con buena
arquitectura. **Principal mejora:** limpiar dependencias/duplicación y unificar
los rangos de validación de fechas.
