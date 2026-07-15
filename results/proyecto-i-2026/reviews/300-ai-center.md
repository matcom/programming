# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #300
- **Repositorio:** https://github.com/RosyFdezT07/AI-Center
- **Estudiante:** Rosmary Fernández Tamayo
- **Grupo:** C121
- **Descripción declarada:** Planificador inteligente de eventos. Centro de Investigación en IA.

---

## Nota metodológica importante

El proyecto **no es una app de consola**: la interfaz es una aplicación web **Streamlit** (`app.py`, 1586 líneas). El `main.py` de la raíz es un script puente que sólo hace `os.system("streamlit run app.py")` (`main.py:11`), pensado para satisfacer al bot evaluador que busca un `main.py`. Por eso alimentarlo con `printf` por stdin no tiene sentido.

Adapté la ejecución en dos frentes:

1. **Lógica de negocio directa.** La arquitectura separa limpiamente la GUI del dominio, así que instancié `Planificador` y ejercité todos los flujos (planificar, restricciones, conflictos, huecos, persistencia) con datos reales del repo, sin tocar Streamlit.
2. **Arranque headless de la GUI.** Levanté `streamlit run app.py --server.headless=true`. El servidor arranca sin `Traceback` y el endpoint `/_stcore/health` responde `200 ok`. La GUI es funcional; sólo no puedo hacer clics sin navegador.

Entorno: `uv venv --python 3.12` + `streamlit pandas plotly numpy python-dateutil`. `py_compile` de los 12 módulos: **OK, todos compilan**.

## Dimensión 1 — Qué hace el programa

Sistema de planificación de eventos para un centro de investigación en IA con recursos limitados. El núcleo (`aplicacion/planificador.py`) recibe un evento (nombre, ventana temporal, recursos con cantidades, tipo, prioridad) y lo valida en cascada (`planificador.py:43-172`):

1. Validaciones temporales básicas: inicio < fin, duración ≤ 7 días, no en el pasado, año razonable (`planificador.py:71-94`).
2. Existencia de recursos y cantidad ≤ capacidad del pool (`planificador.py:99-107`).
3. Restricciones de dominio: co-requisitos, exclusiones mutuas, capacidad por tipo (`planificador.py:125`).
4. Conflictos temporales por recurso mediante **barrido de línea** (`planificador.py:174-236`).
5. Si hay conflicto y se pidió, busca hueco automático en los próximos 7 días con saltos de 10 minutos (`planificador.py:238-308`).

La GUI ofrece dashboard con métricas, gestión de eventos/recursos, formulario de nuevo evento, búsqueda de huecos, y gestión de datos con backups (`app.py`).

## Dimensión 2 — Organización del código

**Fortaleza destacada.** La modularidad es la mejor cualidad del proyecto y está muy por encima de lo esperado en primer año. Arquitectura por capas real:

- `core/` — tipos e interfaces (`Protocol`) para desacoplar (`core/interfaces.py`).
- `dominio/` — `Recurso`/`GestorRecursos` (`recursos.py`), `Evento`/`GestorEventos` (`eventos.py`), jerarquía de `Restriccion` con clase abstracta + tres subclases concretas (`restricciones.py:15-93`).
- `aplicacion/` — `Planificador` orquestador (`planificador.py`).
- `infraestructura/` — `Persistencia` con serialización, backups y migración (`persistencia.py`).

Uso correcto de `@dataclass`, `@property` (`eventos.py:110-135`), `@staticmethod`, `@classmethod` `from_dict`/`to_dict`, `ABC`/`@abstractmethod`, `__hash__`/`__eq__`/`__str__`/`__repr__`. El patrón Strategy en las restricciones (cada restricción implementa `es_valida`/`mensaje_error`) es un acierto de diseño.

**Debilidades menores:**
- `Recurso.es_compatible_con` (`recursos.py:50-55`) es **código muerto**: se define (y se declara en `core/interfaces.py:16`) pero no se invoca en ningún lado (`grep` confirma cero usos reales). Además su lógica es dudosa (sólo "computacional+humano" son compatibles).
- `app.py` con 1586 líneas es un monolito; algo esperable en Streamlit, pero mucha lógica de presentación podría factorizarse en funciones.
- Firma inconsistente de `RestriccionCapacidad.__init__(capacidad_maxima, tipo_recurso)` (`restricciones.py:78`) vs. las llamadas por keyword `tipo_recurso=..., capacidad_maxima=...` (`restricciones.py:153`). Funciona porque siempre se llama con keywords, pero el orden posicional invita a errores.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté un banco de pruebas contra la lógica de negocio cargando `datos/datos.json` (14 recursos, 5 eventos, 10 restricciones). Resultados observados:

1. **Carga de sistema.** `cargar_datos` reconstruye 14 recursos, 5 eventos y 10 restricciones sin error. ✅
2. **Flujo válido.** Evento con `cluster_gpu_a100` + `investigador_vision` → `success: True`, "Evento agregado exitosamente", `duracion_horas: 3.0`. ✅
3. **Co-requisito.** Cluster GPU sin investigador → `False`, "Para la utilización del recurso: cluster_gpu_a100, es necesario emplear también el recurso: investigador_vision". ✅
4. **Exclusión mutua.** `cluster_gpu_a100` + `cluster_gpu_v100` → `False`, "no se pueden utilizar juntos". ✅
5. **Capacidad por tipo.** 3 recursos computacionales (máx 2) → `False`, "Máximo 2 recursos de tipo 'computacional' permitidos por evento". ✅
6. **Fecha inicio ≥ fin** → `False`, "La fecha de inicio debe ser anterior a la de fin". ✅
7. **Fecha en el pasado** → `False`, mensaje correcto. ✅
8. **Tipo de evento inválido** ("fiesta") → `False`, lista los tipos válidos. ✅
9. **Recurso inexistente** → `False`, "Recurso recurso_inexistente no encontrado". ✅
10. **Conflicto temporal (barrido de línea).** Dos eventos con el mismo cluster (capacidad 1) en el mismo horario: el segundo → `False`, "Capacidad excedida para 'Cluster GPU A100': Se requieren 2 simultáneos, capacidad máxima 1." El algoritmo de barrido funciona. ✅
11. **Búsqueda de hueco automático.** Tras el conflicto, con `buscar_hueco_si_ocupado=True` → `True`, evento reubicado 2 h después ("hueco encontrado"). ✅
12. **Búsqueda de huecos (lista).** `buscar_hueco_disponible` devolvió 22 huecos en 2 días. ✅
13. **Persistencia round-trip.** `guardar_datos` → recargar en otro directorio → 14 recursos, 7 eventos, 10 restricciones intactos, referencias de recursos correctamente reconstruidas. ✅
14. **GUI headless.** Streamlit arranca sin `Traceback`; `/_stcore/health` → `200 ok`. ✅

No detecté ningún fallo del estudiante. Todos los fallos observados eran validaciones esperadas (flujos inválidos rechazados con mensajes claros). Los errores del entorno (imposibilidad de clic en GUI) no son del código.

**Observación de diseño (no es un bug):** en la prueba 12, un pool `estacion_trabajo` (capacidad 4) al pedir 4 unidades es rechazado por la restricción `RestriccionCapacidad("computacional", máx 2)`, porque el aplanamiento expande el pool a 4 objetos del tipo "computacional". Es coherente con cómo está definida la restricción, pero implica que **los pools grandes y el límite por tipo entran en tensión**: nunca podrás usar más de 2 unidades de cualquier recurso computacional aunque el pool tenga 4. Vale la pena que la estudiante lo tenga presente (¿debe la capacidad por tipo contar recursos físicos distintos, o unidades?).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy sólido para el nivel:
- Type hints consistentes en casi todo el código.
- Docstrings en la mayoría de métodos.
- Manejo de errores con `try/except` que devuelve un dict de resultado en vez de reventar (`planificador.py:169-170`) — buen patrón.
- Validaciones con `raise ValueError` en `__post_init__` (`recursos.py:20-27`, `eventos.py:32-55`).

Puntos mejorables menores:
- `except Exception as e` genérico en varios sitios (`planificador.py:169`, `458`, `493`) atrapa demasiado; para depurar conviene ser específico.
- `print` para diagnósticos (`persistencia.py:89`, `eventos.py:318`) — en un sistema con GUI, esos mensajes van a la consola del servidor y el usuario no los ve. Mejor propagar como advertencias estructuradas.
- Código muerto (`es_compatible_con`) y algún método declarado en la interfaz sin uso.

## Dimensión 5 — Datos y persistencia

Modelo bien pensado. `datos/datos.json` guarda `metadata`, `eventos`, `recursos` y `restricciones` serializadas por tipo con sus parámetros (`persistencia.py:113-143`). La deserialización reconstruye recursos primero y luego resuelve las referencias de los eventos **por ID**, evitando duplicar instancias del mismo recurso físico (`persistencia.py:61-105`) — decisión correcta y bien comentada. Backups con timestamp (`persistencia.py:183-197`) y listado ordenado (`persistencia.py:199-218`). El round-trip que ejecuté (prueba 13) confirma que no se pierde información.

Detalle: si un recurso referenciado por un evento no existe al cargar, `cargar_sistema` **omite** ese recurso con un `print` y continúa (`persistencia.py:88-93`), no aborta.

## Dimensión 6 — Informe (`report.md`)

Informe extenso, bien redactado y en general fiel al código: la descripción de la arquitectura por capas, del algoritmo de barrido de línea (`report.md:171-177`), del aplanamiento de cantidades (`report.md:94-97`) y de la reconstrucción de referencias (`report.md:99-103`) coincide con lo implementado.

Discrepancias a señalar (honestidad sobre el informe):

1. **Pseudocódigo idealizado.** El informe muestra `cargar_sistema` haciendo `raise ErrorIntegridad(f"Recurso {recurso_id} no existe")` ante una referencia rota (`report.md:219`). El código real **no lanza excepción**: imprime un aviso y omite el recurso (`persistencia.py:88-93`). El pseudocódigo describe un comportamiento más estricto del que existe.
2. **Narrativa de proceso no verificable.** "15 escenarios de uso" (`report.md:61`), iteraciones de "3 semanas / 2 semanas / 3 semanas" (`report.md:93,99,105`) y "retrospectivas después de cada hito" son afirmaciones de proceso que no se pueden comprobar desde el repo; leídas como relato están bien, pero conviene no presentarlas como hechos demostrados.
3. **Tono "profesional/desplegable".** Frases como "listo para ser desplegado en cualquier centro de investigación" (README) sobreestiman: es un prototipo académico correcto, no un producto. Es una exageración menor, común y perdonable.

Lo que el informe **acierta y no exagera**: la complejidad O(n log n) del barrido, la tensión pool vs. capacidad (que el propio informe reconoce en la Iteración 1), y la separación de responsabilidades — todo verificado al ejecutar.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**, claramente por encima de la media de primer año. La arquitectura por capas es real y funcional, no decorativa: pude ejercitar el dominio completo sin tocar la GUI justamente porque está bien separado. El motor de restricciones (patrón Strategy) y el algoritmo de barrido de línea para conflictos con pools de capacidad funcionan correctamente en todas las pruebas que ejecuté (14 escenarios, incluidos flujos inválidos y round-trip de persistencia). La GUI Streamlit arranca limpia. El informe es maduro, aunque con algo de narrativa de proceso no verificable y un pseudocódigo que idealiza un caso de error.

**Principal fortaleza:** diseño y modularidad excepcionales para el nivel — separación limpia dominio/aplicación/infraestructura, patrón Strategy en restricciones, y un algoritmo de detección de conflictos correcto y eficiente, todo verificado por ejecución.

**Principal área de mejora:** limpiar el código muerto (`es_compatible_con`), sustituir los `print` de diagnóstico por avisos que lleguen al usuario de la GUI, y alinear el informe con el comportamiento real del código (el `raise` que no existe). Como reflexión de diseño: resolver la tensión entre pools de capacidad grande y el límite de recursos por tipo.
