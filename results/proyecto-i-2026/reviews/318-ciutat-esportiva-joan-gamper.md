# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #318
- **Repositorio:** https://github.com/cala49/Ciutat-Esportiva-Joan-Gamper
- **Estudiante:** Carlos Alberto Marichal Cala
- **Grupo:** C121
- **Descripción declarada:** Un gestionador de eventos basado en el campo de entrenamiento del FC Barcelona (Ciutat Esportiva Joan Gamper), para partidos de fútbol, entrenamientos y eventos especiales, con gestión de recursos/equipamiento y restricciones entre ellos.

---

## Nota metodológica importante

Es una **aplicación de consola** con menú `input()` (no GUI), así que se ejecutó directamente alimentando el menú con `printf`. Además se probó la **lógica de negocio** de forma aislada (importando `persistence`, `calendary`, `contranins`, `event`) para llegar a rutas que el flujo interactivo protege por la validación de fecha (no permite eventos en el pasado y los datos de ejemplo están fechados en el pasado). No hay `requirements.txt`/`pyproject.toml`: el proyecto es **solo biblioteca estándar** (`datetime`, `json`, `typing`), corre sin instalar dependencias.

## Dimensión 1 — Qué hace el programa

`main.py` arranca `InterfazConsola.mostrar_menu_principal()` (`cli.py:45`), un menú de 10 opciones:

1. Ver eventos ordenados por fecha (`cli.py:91`, `calendary.py:61`).
2. Agregar evento: pide nombre, fecha `YYYY-MM-DD`, hora `HH:MM`, duración en minutos, y recursos por índice; valida formato, rechaza fechas pasadas, verifica conflicto de recursos y restricciones antes de insertar (`cli.py:112`).
3. Eliminar evento por número de lista (`cli.py:206`, `calendary.py:27`).
4. Buscar hueco disponible: barre desde ahora en pasos de 30 min buscando el primer intervalo sin conflicto de recursos (`cli.py:235`, `calendary.py:71`).
5. Listar recursos con estadísticas por tipo (`cli.py:309`).
6. Gestionar restricciones (inclusión / exclusión) con submenú (`cli.py:330`).
7/8/9. Cargar / Guardar / Crear datos de ejemplo (persistencia JSON).
10. Salir (guarda antes de salir, `cli.py:83-86`).

El dominio está bien pensado: eventos con solapamiento temporal + conflicto por recurso compartido, y un pequeño motor de restricciones sobre los recursos de un evento.

## Dimensión 2 — Organización del código

**Fortaleza clara para 1er año.** El proyecto está bien modularizado por responsabilidad:

- `models.py` — jerarquía `Resource` con subclases `Ball`, `Cards`, `Kit`, `Supplements` (herencia + `to_dict`/`__repr__`/`__eq__` por ID).
- `event.py` — `Event` con `to_dict`/`from_dict`, `superposition`, `use_resource`.
- `calendary.py` — `Calendary`, gestión de eventos, IDs, conflictos, búsqueda de hueco.
- `contranins.py` — `Restriccion` (base), `RestriccionInclusion`, `RestriccionExclusion`, `GestorRestricciones`.
- `persistence.py` — `GestorPersistencia`, serialización JSON, reconstrucción de recursos por `subtype`.
- `cli.py` — capa de interfaz separada de la lógica.

La separación GUI/lógica es correcta: la lógica de negocio es invocable sin la interfaz (lo aproveché en las pruebas). Uso apropiado de herencia, `super()`, `@classmethod` (`event.py:30`) y polimorfismo en las restricciones. **Debilidad menor:** nomenclatura mezcla inglés/español (`Calendary`, `veri_conflict`, `find_posible_position` vs. `agregar_evento`, `recursos`) y hay typos en nombres de archivo/clase (`contranins.py`, `RestriccionInclusion`). No afecta a la ejecución pero conviene homogeneizar.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Qué se corrió y qué se observó:

1. **`py_compile` de los 7 módulos** → todos compilan sin error.
2. **Listar recursos (opción 5)** → correcto: 7 recursos, estadísticas por tipo (`Ball: 2, Kit: 1, Cards: 2, Supplements: 2`).
3. **Listar eventos (opción 1)** → correcto: "Partido vs Mayorca (ID: 1) - 05/04/2026 15:00 a 16:45" con sus recursos y duración.
4. **Agregar evento válido** (`2026-08-01 10:00`, 90 min, recursos 2,3) → "✅ Evento 'Entrenamiento Test' agregado" + resumen. Correcto.
5. **Entradas basura** (`fecha-basura`, `25:99`, `abc`) → "❌ Formato de fecha/hora inválido." Sin `Traceback`. Correcto.
6. **Fecha en el pasado** (`2020-01-01`) → "❌ No se pueden crear eventos en el pasado." Correcto.
7. **Opción de menú inválida** (`99`) → "❌ Opción no válida." Correcto.
8. **Buscar hueco (opción 4)** para 60 min → "✅ HUECO ENCONTRADO". Correcto en el caso sin conflicto.
9. **Restricción de inclusión**: agregué Amarillas→Rojas, luego intenté un evento con solo Amarillas → "❌ RESTRICCIONES VIOLADAS: … Al usar 'Amarillas' también se debe usar 'Rojas'". Correcto.
10. **Persistencia (roundtrip)**: agregué "Evento Persistente", guardé (opción 8), reinicié y listé → aparece; el JSON contiene 2 eventos. Correcto.

**BUG 1 (del estudiante, importante) — `AttributeError` al detectar un conflicto real.** En `calendary.py:57` el mensaje usa `current_event.nombre`, pero el atributo se llama `name` (`event.py:6`). En cuanto un evento nuevo **solapa en tiempo y comparte un recurso** con uno existente, `veri_conflict` lanza `AttributeError: 'Event' object has no attribute 'nombre'`. Reproducido de dos formas:
   - Lógica aislada: `cal.veri_conflict(evento_solapado)` → `AttributeError`.
   - End-to-end por el menú (moví el evento de ejemplo a `2026-08-01` y creé uno solapado con el recurso 1): la app imprime `❌ Error inesperado: 'Event' object has no attribute 'nombre'` (capturado por el `except Exception` de `main()`, `cli.py:455`) y **termina**.
   
   Impacto: la detección de conflictos de recursos —una de las funciones centrales del proyecto— nunca puede reportar el conflicto; siempre revienta primero. La corrección es de una palabra: `current_event.nombre` → `current_event.name`. También afecta a "Buscar hueco" (opción 4) si algún intervalo candidato choca con un evento existente, porque `find_posible_position` llama a `veri_conflict` internamente (`calendary.py:95`).

**BUG 2 (del estudiante, semántico) — `RestriccionExclusion` hace lo contrario de excluir.** `contranins.py:42-61` implementa un **bicondicional A⇔B** (o ambos o ninguno): falla cuando se usa solo uno de los dos recursos y **pasa cuando se usan los dos juntos**. Verificado:
   - Evento con Balón1 **y** Balón2 → `(True, 'Restricción de inclusión cumplida')`.
   - Evento con solo Balón1 → `(False, "…también se debe usar 'Balón de Fútbol 2'")`.
   
   El README declara exactamente lo opuesto ("No se pueden usar 2 Balones al mismo tiempo"). La exclusión real debería fallar cuando **ambos** están presentes. Además, incluso como corrección de inclusión, el mensaje de éxito dice "Restricción de inclusión cumplida" (`contranins.py:61`), texto copiado de la otra clase.

No se observaron otros `Traceback` en los flujos probados.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- Buen uso de type hints, `Optional`, `tuple[bool, str]` como patrón de retorno (valor + mensaje) — idiomático y legible.
- Manejo de errores razonable en la capa CLI (`try/except ValueError`) y en `from_dict`/`cargar_datos`.
- **Mejorable:** los `raise ValueError("Calendario no inicializado")` dispersos (`cli.py:95, 169, 209, 277`) delatan que `self.calendario` podría ser `None`; en la práctica siempre se inicializa, así que son defensas que nunca disparan y añaden ruido.
- **Mejorable:** `except Exception as e` en `main()` (`cli.py:455`) enmascara bugs como el del `nombre` bajo un "Error inesperado" genérico en vez de dejar ver el `Traceback` durante el desarrollo.
- Los emojis en los mensajes están bien para la experiencia de consola; no afectan a la lógica.

## Dimensión 5 — Datos y persistencia

Diseño de persistencia sólido para el nivel. Los eventos guardan **solo IDs de recursos** (`event.py:20-28`) y en la carga se reconstruyen mapeando `recursos_por_id` (`event.py:30-45`, `persistence.py:65-88`); esto evita duplicar el recurso y mantiene una sola fuente de verdad. La reconstrucción de subclases por el campo `subtype` (`persistence.py:15-52`) es un buen patrón (factoría simple). El roundtrip guardar→cargar funciona (probado). **Limitación real:** las restricciones (`GestorRestricciones`) **no se persisten** — no hay serialización de `self.restricciones`, así que se pierden al cerrar la app; en la práctica el menú de restricciones solo tiene efecto dentro de la misma sesión.

## Dimensión 6 — Informe (`README.md`)

No hay `report.md`; el `README.md` cumple ese papel y es honesto en tono, pero tiene dos discrepancias con el código:

1. **Semántica de exclusión invertida.** El README dice: "No se pueden usar 2 Balones al mismo tiempo" (`README.md:26`). El código de `RestriccionExclusion` hace justo lo contrario (exige que ambos estén juntos), como se verificó por ejecución. La descripción del informe **no coincide** con el comportamiento real.
2. **"interfaz visual … bastante cómoda" (`README.md:30`).** Es un menú de texto por consola; "visual" sobreestima un poco, aunque la ejecución confirma que el menú es claro y usable.

El resto del README describe correctamente el dominio, los recursos y las restricciones de inclusión (que sí funcionan). No hay afirmaciones de "demostrado/probado" que exageren la validación.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido con dos bugs concretos**. La arquitectura es la mejor parte: modularización limpia por responsabilidad, herencia y polimorfismo bien aplicados, persistencia por IDs con reconstrucción de subclases, y separación real entre lógica e interfaz — todo por encima de lo esperable en 1er año. La mayoría de los flujos corren correctamente y el manejo de entradas inválidas es robusto. Lo que baja la nota funcional son dos defectos que tocan el núcleo del proyecto: (1) un typo `nombre`/`name` que hace **crashear la detección de conflictos** —la función estrella— en cuanto ocurre un conflicto real, y (2) una `RestriccionExclusion` cuya semántica es la inversa de lo que promete el README. Ambos son arreglos pequeños (una palabra el primero; invertir la condición y corregir el mensaje el segundo), pero delatan que esas rutas no llegaron a probarse con datos que las activaran.

- **Principal fortaleza:** organización y diseño orientado a objetos maduros — modularidad, herencia, persistencia por referencia (IDs) y separación lógica/interfaz.
- **Principal área de mejora:** probar los caminos "que fallan a propósito" (conflicto real de recursos, exclusión con ambos recursos); habrían aflorado el crash de `nombre` y la semántica invertida de exclusión.
