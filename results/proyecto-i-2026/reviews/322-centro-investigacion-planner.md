# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #322
- **Repositorio:** https://github.com/SabrinaSocarras/centro-investigacion-planner
- **Estudiante:** Sabrina Socarras Arnaiz
- **Grupo:** C122
- **Descripción declarada:** Aplicación en Python con interfaz gráfica (Tkinter) para gestionar experimentos científicos en un laboratorio ficticio. Registra científicos, planifica experimentos, asigna recursos y aplica restricciones de seguridad y acceso automáticamente. Los datos persisten entre sesiones mediante archivos JSON.

---

## Nota metodológica importante

Esto **no es una app de consola**: es una GUI de Tkinter (`gui.py`, 933 líneas) cuyo punto de entrada real es `run_app.py` (aunque no se llama `main.py`, por lo que la verificación automática no lo detectó — 2/6). La verificación automática también reportó ausencia de `requirements.txt`/`pyproject.toml`, lo cual es correcto pero irrelevante: el proyecto solo usa la biblioteca estándar (`tkinter`, `json`, `datetime`, `pathlib`, `copy`), así que no hay dependencias que declarar.

Cómo adapté la ejecución:

1. **Compilación:** `py_compile` de los 10 módulos → **todos compilan** sin error de sintaxis.
2. **Arranque de la GUI headless (Xvfb):** falla en `gui.py:36`, `self.state("zoomed")` → `TclError: bad argument "zoomed": must be normal, iconic, or withdrawn`. **`"zoomed"` es un estado de ventana exclusivo de Windows**; en Linux/X11 no existe. No es un bug de lógica: sustituyendo esa línea por `self.geometry(...)` la ventana y todos los widgets de la pantalla de inicio se construyen sin error. En el entorno de la estudiante (Windows) arranca normalmente.
3. **Lógica de negocio:** al estar limpiamente separada de la GUI (clases `Planner`, `Experimento`, `Cientifico`, `Recurso` + módulos `sistema_*`), la ejecuté directamente con los datos reales del repo, cubriendo flujos válidos e inválidos. Aquí está el grueso de la evaluación.

---

## Dimensión 1 — Qué hace el programa

Sistema de planificación de experimentos de laboratorio con autenticación y control de acceso. Flujo real observado:

- **Carga inicial** (`gui.py:39-41`): lee `cientificos.json` (2 científicos), `recursos.json` (28 recursos) y `experimentos.json` (2 experimentos precargados) desde disco.
- **Autenticación** (`sistema_cientifico.py:55` `login_cientifico`): verifiqué login válido (`sabrina@123.com`/`SSS` → devuelve el objeto `Cientifico`) e inválido (password mala → `None`). Registro de nuevas cuentas con nivel 1/2/3 vía radio buttons (`gui.py:358-378`).
- **Creación de experimento** (`gui.py:575` `crear` → `Planner.agregar_exp`, `planner.py:13`): formulario con nombre, fecha (`YYYY-MM-DD HH:MM`), duración y checkboxes de recursos. Antes de aceptar, valida en cascada: conflicto de horario/recurso, nivel de acceso, exclusión mutua láser/vibración, co-requisito de radioactivos, y horario del centro.
- **Búsqueda de huecos** (`planner.py:126` `busca_hueco`, `planner.py:171` `buscar_varios_huecos`): si el rechazo es de horario, recorre el calendario en pasos de 30 min durante 30 días y sugiere hasta 3 fechas alternativas. Verifiqué que devuelve resultados correctos (ej. desde `2026-06-26 09:00` con un recurso ocupado → primer hueco `11:00`, y tres opciones `11:00/11:30/12:00`).
- **Eliminación** (`planner.py:115` `eliminar_experimento`): libera los recursos del experimento y reescribe los JSON. Verifiqué que `eliminar EXP-001` devuelve `True`, libera `CG-01` y deja `EXP-002`; borrar un id inexistente devuelve `False`.
- **Vistas por nivel** (`gui.py:458`): "Todos los experimentos" solo aparece para nivel 3.

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** La separación de responsabilidades es notablemente buena para primer año:

- **Modelo de dominio limpio:** `Cientifico` (`cientifico.py`), `Recurso` (`recurso.py`), `Experimento` (`experimento.py`) son clases pequeñas y cohesivas, cada una en su archivo.
- **Capa de persistencia aislada:** `sistema_cientifico.py`, `sistema_recursos.py`, `sistema_experimentos.py` encapsulan el `cargar_*`/`guardar_*` con `json`. Las rutas se resuelven con `Path(__file__).parent` (`sistema_recursos.py:5`), así que funcionan desde cualquier directorio de trabajo — buen detalle.
- **Lógica de negocio en `Planner`** (`planner.py`), completamente independiente de Tkinter. Esto es lo que me permitió ejecutar el núcleo sin GUI: es la marca de un buen diseño.
- **GUI factorizada:** helpers `crear_boton`/`crear_label`/`crear_entry` (`gui.py:61-109`) evitan repetición; `limpiar_pantalla` (`gui.py:50`) da un patrón consistente de navegación entre pantallas.

Debilidades menores:
- `Experimento.se_solapa_con` es método de instancia pero la comparación de recursos (`comparten_recursos`) vive en `Planner` — repartición razonable, sin problema.
- `recursos_iniciales.py:75` escribe en la ruta relativa `"src/recursos.json"` (no usa `Path(__file__)` como el resto), así que ese script solo funciona si se ejecuta desde la raíz del repo. Inconsistente con los demás módulos.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté la lógica de negocio con datos reales. **Todas las restricciones declaradas funcionan** salvo un caso límite:

1. ✅ **Experimento válido** (lunes 2026-07-20 10:00, 2h, recurso libre) → `True`, id `EXP-003`, recurso queda `disponible=False`.
2. ✅ **Nivel insuficiente** (nivel 1 + Láser certificado) → `False`, "No tiene nivel suficiente…".
3. ✅ **Exclusión mutua** (Láser + Fuente de Vibración) → `False`, "No se puede usar un láser junto con una fuente de vibración".
4. ✅ **Radioactivos sin co-requisito** → `False`, "…requieren sala blindada y dosímetro personal".
5. ✅ **Radioactivos con Sala Blindada + Dosímetro** → `True`.
6. ✅ **Domingo** (2026-07-19) → `False`, "No se permiten realizar experimentos los domingos".
7. ✅ **Inicio fuera de horario** (07:00 entre semana) → `False`, "El experimento esta fuera del horario del centro".
8. ✅ **Fin fuera de horario** (19:00 + 2h = 21:00) → `False` (correcto: confirma la afirmación del informe de que compara `time` con minutos, no solo la hora).
9. ✅ **Conflicto** (mismo recurso `CG-01`, horario solapado con `EXP-001`) → `False`, "Conflicto de horario con otro experimento".
10. ✅ **Sábado válido** (2026-07-18 10:00, dentro de 9–15) → `True`. **Nivel 2 con recurso certificado** → `False` (el límite `<3` es correcto).

**Bug real encontrado (defecto del estudiante, no del entorno):**

11. ⚠️ **Experimentos que cruzan la medianoche se aceptan indebidamente.** Un experimento lunes 18:00 con duración 10h (fin martes 04:00) → `agregar_exp` devuelve **`True`** y lo acepta, aunque el experimento corre toda la noche a través del periodo cerrado. Causa: en `planner.py:68`, la comprobación de cierre es `fin.time() > time(hora_cierre, 0)`. Cuando `fin` pasa de medianoche, `fin.time()` es una hora de madrugada (04:00) que resulta *menor* que 20:00, por lo que la validación no lo detecta. El chequeo compara solo la *hora del día* del fin, ignorando que cayó en el día siguiente. `se_solapa_con` sí usa `datetime` completos, pero la validación de horario solo mira `.time()`. Es un caso límite, pero rompe la garantía "el experimento no puede salirse del horario del centro".

**Observación sobre estabilidad de IDs (latente, benigno en uso normal):**

12. `Experimento.id` se asigna en el constructor desde un contador de clase (`experimento.py:13-14`). `cargar_experimentos` re-instancia los experimentos existentes en cada llamada, lo que **incrementa el contador** y reasigna IDs distintos (`EXP-003`, `EXP-004`…) si se recarga dos veces en el mismo proceso. El "arreglo" de `sistema_experimentos.py:52-56` ajusta el contador *después*, pero los objetos de la segunda carga ya llevan IDs equivocados. En la GUI real esto **no** se dispara porque los datos se cargan una sola vez al arrancar (`gui.py:39-41`). Sería más robusto leer el `id` desde el JSON en lugar de regenerarlo (el JSON ya lo guarda, `sistema_experimentos.py:69`, pero al cargar se ignora, `sistema_experimentos.py:36-42`).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Legibilidad:** excelente. Nombres en español consistentes, comentarios que explican el *porqué* (no solo el qué), código limpio. Muy por encima de la media de primer año.
- **Manejo de errores en la GUI:** los `try/except ValueError` en `gui.py:586-601` para fecha y duración están bien acotados y dan mensajes claros al usuario. Buen uso.
- **`ultimo_error` como estado:** el patrón de guardar el mensaje en `self.ultimo_error` (`planner.py:11`) y leerlo tras un `False` funciona, aunque su valor inicial "Ha habido un error" queda accesible si `agregar_exp` devuelve `True` sin haberlo tocado (inofensivo, pero un poco descuidado). Alternativa idiomática: devolver una tupla `(bool, mensaje)` o lanzar excepciones específicas.
- **Constructor de `Cientifico`** (`cientifico.py:6-7`): valida `nivel_acceso not in [1,2,3]` pero solo hace `print` de advertencia, no rechaza. En la GUI el nivel siempre viene de radio buttons [1,3], así que `textos_nivel[nivel]` (`gui.py:436-439`) es seguro en la práctica; pero un nivel fuera de rango cargado desde un JSON editado a mano provocaría un `IndexError`.
- **Inconsistencia menor:** el dedup de email por `.lower()` está en la GUI (`gui.py:398`) pero `registrar_cientifico` (`sistema_cientifico.py:44`) usa comparación exacta. La ruta real es la de la GUI, así que no afecta.

## Dimensión 5 — Datos y persistencia

- Modelo de datos claro en tres JSON (`cientificos.json`, `recursos.json`, `experimentos.json`), con `ensure_ascii=False` e `indent=4` — legibles y con acentos correctos.
- **Normalización correcta:** los experimentos guardan el `email` del científico y los `id` de recursos (no objetos anidados), y al cargar se resuelven las referencias (`sistema_experimentos.py:28-47`). Es un patrón de "claves foráneas" que evita duplicar datos — muy bien pensado.
- Guardado transaccional razonable: cada `agregar_exp`/`eliminar_experimento` reescribe experimentos y recursos juntos (`planner.py:79-80, 121-122`), manteniendo consistente el estado `disponible` de los recursos.
- Se pierden atributos derivados no serializados (`estado` sí se guarda; `contador` se recalcula). Correcto para el alcance.

## Dimensión 6 — Informe (`report.md`)

El informe tiene **878 palabras** — por debajo del mínimo de 2000 que pide la asignatura. Es una carencia formal a señalar. Dicho eso, **el contenido es honesto y preciso**: no exagera ni inventa features.

- La sección "Restricciones implementadas" (líneas 50-72) describe exactamente lo que verifiqué ejecutando: las cinco restricciones existen y funcionan tal cual las narra.
- La afirmación de línea 72 ("compara el objeto `time` completo… para evitar que un experimento que termina a las 20:30 sea aceptado") es **verdadera** para el caso intra-día (mi test 8 lo confirma). Lo que el informe no menciona es el caso límite del cruce de medianoche (Dimensión 3, punto 11), donde esa misma comparación falla. No es deshonestidad — es un caso que la autora probablemente no consideró.
- Línea 78 ("El contador de IDs se inicializa al cargar para evitar colisiones") describe la *intención* de `sistema_experimentos.py:52-56`; el mecanismo tiene el matiz del punto 12, pero es fiel al diseño.
- El README y el informe coinciden con el código; no hay features declaradas que no existan.

---

## Valoración global (orientativa, sin nota numérica)

Un proyecto **sólido y bien diseñado** para primer año. Lo que más destaca es la arquitectura: la lógica de negocio está genuinamente separada de la GUI, con un modelo de dominio limpio y una capa de persistencia bien aislada — eso me permitió ejecutar y verificar todo el núcleo sin siquiera abrir la ventana. Las cinco restricciones de seguridad declaradas funcionan correctamente en ejecución real, la persistencia normaliza los datos con un patrón de referencias por clave, y la búsqueda de huecos alternativos entrega resultados válidos. El código es limpio y legible, por encima de la media del curso. Los defectos son acotados: un bug real de caso límite (experimentos que cruzan la medianoche se aceptan indebidamente), un problema latente de reasignación de IDs que no se manifiesta en el uso normal de la GUI, y el informe corto (878 palabras) frente al mínimo requerido. La app no arranca headless en Linux por `state("zoomed")` (exclusivo de Windows), pero eso es de entorno, no de lógica.

- **Principal fortaleza:** separación de responsabilidades ejemplar — la lógica de negocio (`Planner` + modelo + `sistema_*`) es totalmente independiente de la GUI y todas las restricciones de seguridad funcionan verificadas en ejecución.
- **Principal área de mejora:** el chequeo de horario de cierre (`planner.py:68`) compara solo `fin.time()` y no detecta experimentos que cruzan la medianoche; debería comparar el `datetime` completo del fin contra la hora de cierre del *día de inicio*. Y ampliar el informe hasta el mínimo de 2000 palabras.
