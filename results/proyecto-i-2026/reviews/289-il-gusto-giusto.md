# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #289
- **Repositorio:** https://github.com/joserafael0160/Il-Gusto-Giusto
- **Estudiante:** José Rafael Pérez Rivero
- **Grupo:** C-122
- **Descripción declarada:** Sistema de gestión de restaurante italiano. Planifica comandas como eventos que consumen recursos (mesas, chefs, ingredientes) respetando restricciones culinarias. Incluye control de inventario, gestión de personal, contabilidad y exportación/importación de datos. Python, Streamlit, Pandas y pytest.

---

## Nota metodológica importante

Esto **no es una aplicación de consola**: es una app **Streamlit** (`main.py:88` → `st.set_page_config`, navegación por `st.sidebar.button`). No tiene menú `input()`. La virtud del diseño es que la **lógica de negocio está completamente separada de la GUI**: el núcleo vive en `src/core/`, `src/models/`, `src/services/` y `src/persistence/`, sin ninguna dependencia de Streamlit. Eso permitió evaluarla de dos formas:

1. **Lógica de negocio directa** — cargué `data/default_config.json` con `JSONHandler.load`, instancié `EventScheduler` y ejecuté flujos válidos e inválidos con datos reales del repositorio.
2. **GUI headless** — `streamlit run main.py --server.headless true`. El servidor arrancó limpio (Uvicorn en el puerto, `HTTP 200` a `GET /`, sin `Traceback` en logs). El único warning es el estándar de Streamlit en modo *bare*.

Además corrí la suite de tests del estudiante y `py_compile` sobre los 25 módulos.

## Dimensión 1 — Qué hace el programa

Simula la operación de un restaurante italiano modelando **comandas como eventos** que consumen recursos compartidos en el tiempo. El corazón es `EventScheduler.schedule_order` (`src/core/scheduler.py:16`), que ejecuta una tubería de validación en 5 fases antes de confirmar un pedido:

1. Mesa existe y está libre en el intervalo (`scheduler.py:44`, `_is_resource_free:167`).
2. Restricciones de dominio: exclusión mutua y co-requisito (`scheduler.py:48`).
3. Stock de ingredientes suficiente, contando personalizaciones (`scheduler.py:53`, `_check_ingredients_stock:184`).
4. Chef disponible y con la especialidad requerida (`scheduler.py:58`, `_find_available_chef:175`).
5. Éxito: descuenta despensa, crea el `Event`, ocupa mesa y chef, y registra el ingreso (`scheduler.py:63-84`).

Complementa con: `cancel_event` (libera recursos, **reembolsa ingredientes exactos y revierte el ingreso económico**, `scheduler.py:98`), `find_next_available_slot` (busca en ventana de 24h en pasos de 5 min, `scheduler.py:86`), y la capa de servicios `RestaurantService` (contratación/despido, compras, CRUD de menú). La UI tiene 6 vistas: Dashboard, Menú, Staff, Compras, Contabilidad, Configuración (importar/exportar/reset).

## Dimensión 2 — Organización del código

Ejemplar para primer año. Arquitectura por capas real y consistente:

- `src/models/` — dataclasses del dominio (`Employee`, `Dish`, `Ingredient`, `Table`, `Order`, `Event`) más `Restaurant` y dos `Enum` (`EmployeeRole`, `ExperienceLevel`). `restaurant.py:1`, `events.py:1`.
- `src/core/` — `scheduler.py` (motor) + `constraints.py` (reglas).
- `src/services/` — lógica de negocio sin estado (`restaurant_service.py`).
- `src/persistence/` — `json_handler.py` (serialización) + `repository.py` (`Protocol`).
- `src/components/` — una vista Streamlit por archivo.

Puntos fuertes concretos:

- **Patrón Strategy real en las restricciones** (`constraints.py:6`): `Constraint(ABC)` con `CoRequirementConstraint` y `MutualExclusionConstraint`; agregar una regla es subclasificar, sin tocar el `ConstraintValidator`. Diseño extensible genuino, no decorativo.
- Nombres descriptivos, type hints en todas las firmas públicas, docstrings.
- Separación GUI/lógica que hizo la app testeable y evaluable headless.

Debilidad menor:

- **Duplicación de serialización**: `settings.py:22-70` reimplementa a mano casi la misma lógica que `JSONHandler.save` (`json_handler.py:12-58`) en lugar de reutilizarla. El informe afirma "se evita la duplicación" (`report.md:81`); aquí sí la hay. Es el único punto donde el DRY se rompe.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo que sigue lo **corrí de verdad** contra `data/default_config.json` (restaurante con 8 platos, 16 ingredientes, 3 empleados, 5 mesas, balance $15000).

1. **`pytest` — 26/26 pasan** en 0.13s. Cubren planificación, colisiones mesa/chef, cancelación, reembolso exacto de ingredientes (incl. personalizados), restricciones, stock, persistencia y déficit de personal.
2. **`py_compile`** de los 25 módulos: sin errores.
3. **Pedido válido** (`d1` Pizza Margherita en `t1`): `True | Pedido agendado y cocinado exitosamente. | chef: e1`; balance $15000 → $15014.50.
4. **Colisión de mesa** (mismo `t1`, +5 min): `False | La mesa 1 ya está reservada u ocupada en ese intervalo.` — la detección de solape de intervalos (`start < evt.end_time and end > evt.start_time`, `scheduler.py:171`) es correcta.
5. **Mesa inválida:** `False | Mesa inválida`. **Plato inexistente:** `False | Plato ghost no existe`. **Cantidad cero:** `False | Debe seleccionar al menos un plato con cantidad mayor a cero.`
6. **Cancelación:** liberó mesa y chef, y el balance regresó exactamente de $15014.50 → $15000.00 (revierte ingreso, `scheduler.py:126`).
7. **Exclusión mutua** (seafood `d5` + cheese_heavy `d6`): rechazado con el mensaje de "Tradición Italiana (Mar & Queso)".
8. **Co-requisito** (trufa `d7` con `truffle_oil` a 0): rechazado con "Soporte de Trufa".
9. **Stock insuficiente** (mozzarella a 0.001): `False | Stock insuficiente del ingrediente: Mozzarella di Bufala.`
10. **Especialidad de chef:** `d7` requiere `truffle_specialty`; solo Giovanni (`e1`) la tiene → el motor le asignó `e1` correctamente. Verifica que `_find_available_chef` filtra por especialidad (`scheduler.py:178`).
11. **Servicios inválidos:** contratar sin saldo, comprar ingrediente inexistente y publicar plato sin nombre → los tres rechazados con mensajes claros.
12. **Persistencia roundtrip** con evento activo: balance, conteo de eventos y menú se conservan tras save→load.
13. **`find_next_available_slot`:** con `t1` ocupada, devolvió un hueco ≥ fin del primer evento; con `truffle_oil` a 0 devolvió `None`.
14. **GUI headless:** arrancó sin `Traceback`, `HTTP 200`.

**No encontré ningún bug del estudiante.** Todos los flujos inválidos se manejan con mensajes en español y sin excepciones. Los defectos observados son de entorno inexistentes (la app corre limpia).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy por encima del promedio de primer año:

- **Dataclasses** con `field(default_factory=dict)` bien usados (`events.py:13`).
- **Enums** para roles y experiencia en vez de strings mágicos.
- **`ABC` + `@abstractmethod`** para el contrato de restricciones.
- **`Protocol`** para el repositorio (`repository.py:6`) — concepto avanzado.
- Retorno consistente `Tuple[bool, str, ...]` para propagar errores sin excepciones, patrón limpio y predecible.
- Manejo defensivo de I/O: `load` atrapa `JSONDecodeError`/`IOError` y devuelve `(None, [])` (`json_handler.py:65-69`).

Mejorables (menores):

- La duplicación de serialización en `settings.py` ya mencionada.
- `find_next_available_slot` hace polling en pasos fijos de 5 min (`scheduler.py:95`); funciona, pero podría saltar directamente a `end_time` del evento en conflicto. Es una optimización, no un error.

## Dimensión 5 — Datos y persistencia

Sólida. `JSONHandler` (`json_handler.py`) serializa todo el estado a JSON con `indent=4, ensure_ascii=False` (respeta acentos). Maneja bien lo delicado: `datetime` → ISO, `Enum` → `.value`, y `asdict` para dataclasses simples. La carga (`_parse_data:82`) reconstruye los `Enum` desde string y los `datetime` desde ISO. El seed `data/default_config.json` es rico y realista (recetas italianas coherentes con las categorías de las restricciones). Ofrece además `loads`/exportación por string para el upload de la UI. El roundtrip que ejecuté preservó balance, eventos y menú sin pérdida.

## Dimensión 6 — Informe (`report.md`)

2341 palabras, bien estructurado (9 secciones), y en general **honesto y fiel al código**. Describe correctamente la tubería de validación, las restricciones, los patrones SOLID (con ejemplos verificables) y la suite de tests — su sección 8 lista exactamente lo que los tests cubren, sin exagerar. La sección de SOLID no es palabrería: el Strategy de restricciones y el `Protocol` de repositorio respaldan de verdad las letras O, L, I y D.

Discrepancia única: `report.md:81` afirma "se evita la duplicación", pero `settings.py:22-70` duplica la serialización de `json_handler.py`. Overclaim menor y aislado.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **excepcional** para primer año. El estudiante entendió el problema como uno de planificación de recursos con restricciones y lo modeló con una arquitectura por capas limpia, un patrón Strategy genuino para reglas extensibles, persistencia robusta y una suite de 26 tests que pasa completa. Lo ejecuté de múltiples formas —tests, lógica directa con datos reales, GUI headless— y **no hallé un solo bug**: flujos válidos e inválidos se comportan exactamente como se espera, con mensajes claros y sin excepciones. El informe acompaña con honestidad, salvo un overclaim trivial sobre duplicación.

- **Principal fortaleza:** el diseño. Separación GUI/lógica que hace el núcleo testeable, sistema de restricciones extensible por herencia, y corrección funcional verificada de punta a punta. Nivel de madurez arquitectónica infrecuente en primer año.
- **Principal área de mejora:** eliminar la duplicación de serialización de `settings.py` reutilizando `JSONHandler.save` — y ajustar la afirmación del informe en consecuencia.
