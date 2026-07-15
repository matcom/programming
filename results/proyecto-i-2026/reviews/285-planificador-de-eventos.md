# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #285
- **Repositorio:** https://github.com/marlamora234564/planificador-de-eventos
- **Estudiante:** Marla Cejas Mora
- **Grupo:** C-122
- **Descripción declarada:** Planificador de eventos para la gestión de recursos y restricciones en producciones audiovisuales.

---

## Nota metodológica importante

Es una aplicación de **consola** (`input()`/`print()`), no GUI. Se ejecutó de verdad
alimentando el menú con `printf`, recorriendo flujos válidos e inválidos, y además se
invocó directamente la **lógica de negocio** de `restricciones.py` (conflictos de
calendario y búsqueda de horario) con datos construidos a mano para verificar la
corrección de los algoritmos sin depender del flujo interactivo. Entorno: `uv venv`
con Python 3.12.8. `requirements.txt` está vacío — el proyecto usa solo biblioteca
estándar (`json`, `os`, `datetime`), lo cual es correcto y no requiere instalación.

Todos los módulos pasan `py_compile` sin errores.

## Dimensión 1 — Qué hace el programa

Es un planificador de eventos para producción audiovisual con seis opciones de menú
(`main.py:14-39`):

1. **Agregar evento** (`eventos.py:13-68`): pide nombre, fecha de inicio y fin, valida
   el formato de fecha y que fin ≥ inicio, luego abre el selector de recursos y valida
   co-requisitos, exclusiones mutuas y conflictos de calendario antes de guardar.
2. **Ver eventos** (`eventos.py:72-103`): lista los eventos con sus recursos y re-valida
   exclusiones/co-requisitos mostrando advertencias.
3. **Eliminar evento** (`eventos.py:107-123`): por índice, con validación de rango.
4. **Gestionar recursos** (`main.py:42-64`): submenú para ver el catálogo, restricciones
   y exclusiones mutuas.
5. **Buscar hueco** (`main.py:84-181` + `restricciones.py:164-239`): dado un conjunto de
   recursos y una duración, busca día a día (hasta 365) el primer intervalo libre de
   conflictos de calendario y ofrece crear el evento automáticamente.
6. **Salir.**

El modelo de dominio es rico: 20 recursos en tres categorías (humanos, técnicos,
físicos, `recursos.py:10-37`), un grafo de co-requisitos de 9 reglas (`restricciones.py:3-41`)
y 4 reglas de exclusión mutua (`restricciones.py:46-64`).

## Dimensión 2 — Organización del código

**Fortaleza destacada.** El proyecto está bien modularizado en cinco archivos con
responsabilidades claras:

- `main.py` — interfaz de menú (`InterfazMenu`).
- `eventos.py` — lógica de eventos (`PlanificadorEventos`).
- `recursos.py` — catálogo y selección (`Recurso`).
- `restricciones.py` — reglas de negocio y algoritmos (funciones puras).
- `persistencia.py` — E/S de JSON (`PersistenciaEventos`).

Esta separación es notablemente buena para primer año. Las validaciones de
`restricciones.py` son **funciones puras** que reciben datos y devuelven
`(es_valido, dict_de_conflictos)` — un patrón limpio y testeable (verificado
invocándolas directamente). La persistencia está aislada y devuelve tuplas
`(exito, mensaje)`.

**Debilidades menores:**

- La clase `Recurso` mantiene un atributo de instancia `self.recursos = []`
  (`recursos.py:41`) que **nunca se llena** durante el uso normal: los recursos viven
  dentro de cada evento, no en la instancia `Recurso`. Métodos como `agregar_recurso`
  (`recursos.py:43-50`), `eliminar_recurso` (`recursos.py:60-67`) y `ver_recursos`
  (`recursos.py:52-58`) son código muerto en la práctica. Ver Dimensión 3, hallazgo 6.
- `persistencia.py:66-98` define `guardar_evento_unico` y `eliminar_evento` que nunca
  se invocan desde el resto del programa (código muerto).
- Los `import` locales dentro de funciones (`from datetime import ...` en
  `restricciones.py:123,166` y `main.py:118`) funcionan pero es más idiomático
  importarlos una vez arriba del módulo.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

1. **Alta de evento válido** — `printf` con Director(1)+Camarógrafo(3)+Actor(6),
   fechas `2025-02-01`/`2025-02-03`, `listo`. Resultado: "✅ Todos los co-requisitos
   están cumplidos", evento guardado, JSON escrito correctamente. **Correcto.**
2. **Violación de co-requisitos** — seleccionar solo Cámara(9) y escribir `listo`:
   el sistema bloquea con "'Cámara' requiere: Camarógrafo" y no deja terminar; tras
   agregar Camarógrafo(3), acepta. **Correcto.**
3. **Exclusión mutua** — Micrófonos(11)+Grabadora(12)+Sonidista(4), `listo`: bloquea con
   "VIOLACIONES DE EXCLUSIÓN MUTUA: Micrófonos ↔ Grabadora de audio". **Correcto.**
4. **Fechas inválidas** — formato `01-05-2025`: "Error: Formato de fecha inválido"
   (`eventos.py:27-29`). Fin < inicio (`2025-06-10`→`2025-06-01`): "Error: La fecha de
   conclusión no puede ser anterior a la de inicio" (`eventos.py:23-25`). **Correcto,
   sin `Traceback`.**
5. **Conflictos de calendario** (invocando `validar_conflictos_recursos` directamente):
   evento A con Camarógrafo del 2025-01-10 al 15; solicitar el mismo recurso 01-12→01-18
   ⇒ `valido=False` (solape detectado); 02-01→02-03 ⇒ `valido=True`; y el toque de frontera
   01-15→01-16 ⇒ `valido=False` (solape inclusivo, `restricciones.py:146`). La lógica de
   solapamiento `inicio1 ≤ fin2 AND fin1 ≥ inicio2` es **correcta**.
6. **Buscar hueco** — con A ocupando 01-10→01-15, buscar 3 días para Camarógrafo desde
   01-10 devolvió `2025-01-16 → 2025-01-18` (saltó la ventana ocupada). Flujo interactivo
   completo (opción 5, ENTER=hoy, `s`, nombre) creó el evento con fecha `2026-07-15` y lo
   persistió. **Correcto y funciona de punta a punta** — es la parte más ambiciosa del
   proyecto y funciona bien.
7. **Persistencia y backup** — tras dos altas en una sesión, `eventos_backup.json`
   contiene el estado previo (`['Ev1']`) y `eventos.json` el actual (`['Ev1','Ev2']`).
   La rotación de backup (`persistencia.py:18-26`) **funciona**.
8. **Eliminar índice inválido** (99): "Número de evento inválido", sin crash
   (`eventos.py:119-120`). **Correcto.**
9. **JSON corrupto al arrancar** — con `eventos.json` malformado, el programa **no
   revienta**: arranca con lista vacía. Sin embargo, el mensaje de error de
   `cargar_eventos` (`persistencia.py:61-62`) se **pierde silenciosamente** porque
   `eventos.py:135` solo imprime si `eventos` es no-vacío. El usuario no se entera de que
   su archivo estaba corrupto. Degradación graciosa pero sin aviso — mejora menor.

No se observó ningún `Traceback` en toda la batería de pruebas.

**"Ver recursos asignados"** (menú 4 → 2, `recursos.py:52-58`) siempre muestra "No hay
recursos registrados" porque `Recurso.recursos` nunca se puebla (ver Dimensión 2). Es
una opción de menú sin función real.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Legibilidad:** buena. Nombres en español coherentes, comentarios abundantes (a veces
  informales — "Esto es pa..." —, aceptable para 1er año).
- **Manejo de errores:** correcto y consistente con `try/except ValueError` en las
  conversiones de fecha y de índice. Bien.
- El `except:` desnudo en `persistencia.py:25` (backup) es un antipatrón menor: captura
  cualquier excepción, incluyendo interrupciones. Mejor sería `except Exception:`.
- **Globales:** los diccionarios `COREQUISITOS`/`EXCLUSIONES_MUTUAS` son constantes de
  módulo — uso legítimo y correcto.
- **Idiomático:** `obtener_recurso_por_numero` (`recursos.py:207-220`) concatena tres
  listas con tres bucles `for ... append`; sería más limpio
  `RECURSOS_HUMANOS + RECURSOS_TECNICOS + RECURSOS_FISICOS`. Detalle menor.
- Código muerto (métodos nunca llamados en `recursos.py` y `persistencia.py`) que
  convendría eliminar para claridad.

## Dimensión 5 — Datos y persistencia

Modelo simple y adecuado: cada evento es un `dict` con `nombre`, `fecha`, `fecha_fin`,
`recursos` (lista de strings). La serialización a JSON usa `ensure_ascii=False` e
`indent=2` (`persistencia.py:36`) — legible y con acentos correctos (verificado en el
archivo generado). El envoltorio con metadatos (`fecha_guardado`, `cantidad_eventos`) es
un buen detalle. El mecanismo de backup antes de sobrescribir es una decisión de diseño
madura y **verificada funcional**. Las fechas se guardan como strings ISO y se parsean con
`datetime.strptime`, consistente en todo el código.

## Dimensión 6 — Informe (`report.md`)

El informe es extenso, bien estructurado y **coincide fielmente con el código**: las 9
reglas de co-requisitos y las 3 exclusiones descritas (secciones 2.3.1/2.3.2) corresponden
exactamente a `restricciones.py:3-64` (la exclusión Micrófonos↔Grabadora está listada dos
veces en el código como bidireccional, lo cual es correcto). El criterio de solapamiento
documentado (sección 2.4) coincide con `restricciones.py:146`. El ejemplo de JSON coincide
con el formato real generado.

Discrepancias menores:

- La sección 5 lista `README.md` en la estructura, pero el archivo fue renombrado a
  `report.md` (commit `ea5704f`); no queda ningún README.
- El informe no menciona el código muerto ni que "ver recursos asignados" no muestra nada
  útil — no es exageración deliberada, sino omisión.

El informe **no sobreestima** la validación: describe features que efectivamente funcionan.
No abusa de "demuestra"/"prueba". Honesto.

---

## Valoración global (orientativa, sin nota numérica)

Un proyecto **sólido y bien por encima del promedio para primer año**. La modularización en
cinco archivos con responsabilidades separadas, las validaciones implementadas como
funciones puras testeables, la persistencia con backup y —sobre todo— la funcionalidad de
"buscar hueco" (un algoritmo de búsqueda día a día que respeta co-requisitos, exclusiones y
conflictos de calendario) revelan verdadera ambición técnica. Todo lo probado funciona sin
un solo `Traceback`, en flujos válidos e inválidos. Las debilidades son menores y de
madurez: código muerto (la clase `Recurso` con su almacenamiento no usado, dos métodos de
persistencia huérfanos), una opción de menú vacía, y un mensaje de error de carga que se
pierde en silencio.

- **Principal fortaleza:** arquitectura modular limpia con lógica de negocio separada y
  testeable, coronada por la funcionalidad "buscar hueco" que integra correctamente todas
  las restricciones (verificada de punta a punta).
- **Principal área de mejora:** eliminar el código muerto y cerrar la brecha de la clase
  `Recurso` — o poblarla de verdad para que "ver recursos asignados" tenga sentido, o
  quitar esa opción del menú.
