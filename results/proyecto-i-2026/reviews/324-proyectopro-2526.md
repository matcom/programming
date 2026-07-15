# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #324
- **Repositorio:** https://github.com/RubixxxPdA/proyectopro-2526
- **Estudiante:** Rubi Perez de Alejo Lorenzo
- **Grupo:** C-121
- **Descripción declarada:** (El cuerpo del issue no incluye descripción.) Según `report.md`: sistema de agendamiento de citas por consola para un salón de uñas ("RR Nails") con gestión de servicios, recursos (gastables / no gastables), manicuristas y persistencia en JSON.

---

## Nota metodológica importante

Es una aplicación **de consola** con `input()`, así que se ejecutó de verdad alimentando el menú con `printf`. No hay GUI ni dependencias externas: solo la biblioteca estándar (`random`, `copy`, `json`), por lo que `requirements.md` diciendo únicamente "Python 3" es correcto. Entorno: `uv venv --python 3.12` (CPython 3.12.8). Antes de cada corrida que muta datos se respaldaron `agenda.json` y `recursos.json` y se restauraron al final.

---

## Dimensión 1 — Qué hace el programa

El punto de entrada real es `pythonmain.py` (no `mainrr_corregido.py`, como dice el informe en `report.md:150`). `MenuPrincipal` (`pythonmain.py:3`) muestra un menú con dos roles:

- **Administrador** (contraseña `rrnails`, `pythonmain.py:18`): mostrar toda la agenda (`mostrar_toda_agenda`, `core.py:687`), agregar recurso (`agregar_recurso`, `core.py:838`), mostrar inventario (`mostrar_inventario`, `core.py:900`), agregar servicio (`agregar_servicio`, `core.py:749`), y guardar y salir.
- **Cliente** (`pythonmain.py:42`): agendar cita (`agendar_cita`, `core.py:337`), eliminar cita (`eliminar_cita`, `core.py:499`), guardar y salir.

El corazón del sistema es el motor de disponibilidad: `agendar_cita` valida horario laboral (9:00–17:00), verifica manicuristas libres y recursos, y si el hueco pedido no sirve, `buscar_siguiente_hueco` (`core.py:263`) propone el próximo espacio recorriendo huecos entre citas y saltando de día recursivamente. Los datos viven en `recursos.json` (servicios, recursos, manicuristas) y `agenda.json` (citas + contador de clientes), cargados al importar `core.py` (`core.py:6-10`).

**Verificado ejecutando:**
- Admin → mostrar agenda: listó correctamente las dos citas de ejemplo (rubi, 10 marzo, Manicura regular 14:00–14:30, Ana; jessica, 2 abril, Manicura softgel 15:00–16:40, Beatriz).
- Cliente → agendar (marzo 15, 10:00, "laura"): cita creada y persistida; la agenda quedó ordenada por fecha (rubi 10 mar, laura 15 mar, jessica 2 abr).
- Cliente → agendar fuera de horario (marzo 20, 18:00): el sistema rechazó y ofreció "21 de marzo a las 09:00"; al responder "si" agendó a pedro en ese hueco.
- Cliente → eliminar (rubi, marzo 10): cita eliminada, la entrada del día se limpió (2→1 días) y el cliente rubi desapareció de `Clientes` al llegar su contador a 0.
- Admin → agregar servicio ("Manicura Test", 50 min, recursos 1,3): persistido como `["Manicura Test", 50, ["esmalte regular", "gel builder"]]`.
- Admin → agregar recurso ("glitter", gastable, 50): persistido en `RecursosGastables`.

## Dimensión 2 — Organización del código

Buena separación de dos archivos: `core.py` con toda la lógica de negocio y `pythonmain.py` con la interfaz/menú (`pythonmain.py:1` importa `from core import *`). Para primer año, esta división presentación/lógica es un acierto notable.

Dentro de `core.py` las funciones son pequeñas y con responsabilidad única, con buenos nombres en español (`solapan_horarios`, `horas_a_minutos`, `comprobar_recursos_disponibles`, `buscar_siguiente_hueco`) y docstrings en casi todas. Las utilidades de tiempo (`formatear_hora`, `minutos_a_horas`, `horas_a_minutos`, `solapan_horarios`, `core.py:28-42`) están bien factorizadas y se reutilizan en todo el módulo.

Debilidades:
- **Estado global mutable**: `Agenda`, `Clientes`, `Servicios`, `RecursosGastables`, etc. son globales que las funciones mutan directamente (`core.py:18-23`). Funciona en un proyecto de este tamaño, pero acopla todo y dificulta las pruebas.
- **Duplicación grande** en `agendar_cita`: el bloque "confirmar → registrar cliente → asignar manicurista → consumir recursos → crear cita → guardar" aparece casi idéntico tres veces (`core.py:370-404`, `core.py:417-446`, `core.py:462-492`). Se podría extraer a una sola función `confirmar_y_crear_cita(...)`.
- **`ingresar_fecha` tiene una rama muerta**: el `if para_agendar / else` (`core.py:237-240`) devuelve exactamente lo mismo en ambos casos, así que el parámetro no hace nada.
- **Los datos son listas posicionales** (`cita[0][0]`, `cita[4]`, `Servicios[servicio_idx][2]`). Es idiomático de primer año, pero al lector le cuesta recordar qué índice es qué. Un diccionario o `namedtuple` haría el código autoexplicativo.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

1. **Agendar válido — CORRECTO.** `printf '2\n1\n1\nmarzo\n15\n10:00\nlaura\n'`: cita creada y persistida en `agenda.json`; cliente "laura" añadido a `Clientes`.
2. **Reprogramación fuera de horario — CORRECTO.** Pedido a las 18:00 rechazado con mensaje claro y propuesta "21 de marzo a las 09:00"; aceptando "si" se agendó. El motor de huecos funciona.
3. **Eliminar cita — CORRECTO.** Se eliminó la cita, se limpió el día vacío y se decrementó/eliminó el cliente. Buena lógica en cascada (`core.py:550-560`).
4. **Agregar servicio / agregar recurso — CORRECTOS.** Ambos persisten llamando a `guardar_recursos` (`core.py:827`, `core.py:870/897`).
5. **Entradas inválidas — BIEN MANEJADAS.** Mes inexistente ("february"), día imposible (febrero 30), hora mala ("25:99"), servicio fuera de rango ("99") y servicio no numérico ("abc") produjeron todos mensajes de error controlados, **sin `Traceback`**. Muy buen manejo defensivo para primer año.
6. **BUG — "Salir" (opción 3) no funciona.** En `pythonmain.py:69`, `elif opcion == 3:` compara `opcion` (que es un **string**, viene de `input().strip()` en `pythonmain.py:14`) contra el **entero** `3`. La condición nunca es verdadera, así que elegir "3" en el menú principal no guarda ni sale: simplemente vuelve a mostrar el menú. (En mi corrida esto se manifestó como `EOFError` al agotarse la entrada, pero de forma interactiva el efecto es un menú que "no responde" a Salir.) Corrección: `elif opcion == "3":`.
7. **BUG — el consumo de recursos gastables NO se persiste.** `agendar_cita` llama a `consumir_recursos_gastables` (`core.py:379/425/471`), que decrementa `RecursosGastables` en memoria, y luego a `guardar_datos()`. Pero `guardar_datos` (`core.py:718`) solo escribe `agenda.json` (Agenda + Clientes); **nunca** llama a `guardar_recursos()`, que es quien escribe `recursos.json`. Verificado: tras agendar una Manicura regular, `esmalte regular` seguía en 20 (sin cambio) y `recursos.json` quedó **idéntico** al respaldo. Es decir, la "gestión de inventario" que el informe describe (consumo automático, alertas de stock) no tiene efecto entre ejecuciones. Corrección: llamar también a `guardar_recursos()` tras agendar (o unificar el guardado).
8. **Función muerta.** `ver_detalles_cita` (`core.py:605`) y su ayudante `mostrar_detalles_cita` (`core.py:669`) están definidas y completas, pero **no están conectadas a ningún menú** en `pythonmain.py`. El informe las anuncia como funcionalidad de cliente ("Ver detalles de sus citas", `report.md:128`, `report.md:301-305`), pero el usuario nunca puede invocarlas.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Puntos positivos: uso correcto de `enumerate`, comprensiones de lista (`core.py:58`), `try/except ValueError` alrededor de conversiones (`core.py:225-230`, `pythonmain.py:53-60`), docstrings, y feedback visual con códigos ANSI de color.

Mejorables (menores):
- **`except:` desnudo** en `ingresar_hora` (`core.py:258`). Captura *cualquier* excepción, incluida `KeyboardInterrupt`. Debería ser `except ValueError:`.
- **`from core import *`** (`pythonmain.py:1`) trae todo al espacio de nombres, incluyendo `random`, `copy`, `json` reexportados. Import explícito sería más limpio, pero para este tamaño es aceptable.
- **Carga de JSON a nivel de módulo** (`core.py:6-10`): si falta el archivo, el `import` revienta antes de que el `main` pueda dar un mensaje amable. Envolver en `try/except FileNotFoundError` o cargar dentro de una función sería más robusto.
- Emojis y colores ANSI en los `print` — bonito, pero se ve raro en terminales sin soporte ANSI (en mi captura aparecen como `[32m...`).

## Dimensión 5 — Datos y persistencia

Modelo claro y consistente entre código y datos reales:
- **Servicios**: `[nombre, duración_min, [recursos...]]` (`recursos.json:2`).
- **Recursos**: `[nombre, cantidad]`, separados en gastables y no gastables (`recursos.json:190`, `recursos.json:207`) — la distinción es conceptualmente buena y bien usada (los no gastables se verifican por solapamiento horario, los gastables por stock).
- **Agenda**: `[[año,mes,día], [ [[ini],[fin]], cliente, idx_servicio, duración, manicurista ] ]` (`agenda.json:2`).
- **Clientes**: `[nombre, contador_citas]`.

Serialización con `json.dump(..., indent=4, ensure_ascii=False)` — se conservan los acentos, legible. El defecto de persistencia no es del modelo sino del guardado incompleto (ver Dimensión 3, punto 7): `guardar_datos` y `guardar_recursos` están separadas y solo la primera se dispara al agendar, dejando el inventario desincronizado en disco.

## Dimensión 6 — Informe (`report.md`)

El informe es extenso, ordenado y describe con precisión el modelo de datos y los algoritmos. Discrepancias con el código real:

- **Punto de entrada equivocado**: dice "Módulo mainrr_corregido.py: Interfaz por consola" (`report.md:150`); el archivo real es `pythonmain.py` y no existe ningún `mainrr_corregido.py`.
- **"Ver detalles de cita" listado como funcionalidad activa** (`report.md:128`, `report.md:301-305`, `report.md:343`) — el código existe (`core.py:605`) pero no está conectado al menú, así que el usuario no puede usarlo.
- **"Ver recursos por servicio"** aparece en el menú de administración del informe (`report.md:339`) pero no está implementado en `pythonmain.py`.
- **Gestión de inventario con consumo automático** (`report.md:236-240`): el informe afirma "se consumen automáticamente los recursos gastables" y "alertas de sin stock". El consumo ocurre en memoria pero **no se guarda** (Dimensión 3, punto 7), así que entre ejecuciones el inventario nunca baja. El informe describe la intención, no el comportamiento persistido observado.
- Detalles menores: menciona "Alcohol" como recurso (existe en `recursos.json:205`) pero ningún servicio lo usa; "de lunes a sábado" no se valida en código (no hay lógica de día de la semana).

El informe no abusa de "demuestra"/"prueba"; describe funcionalidades como implementadas, y la mayoría lo están, pero sobreestima las tres señaladas.

---

## Valoración global (orientativa, sin nota numérica)

Un proyecto **sólido y ambicioso** para primer año. La estudiante separó lógica de presentación, factorizó utilidades de tiempo limpias, y construyó un motor de disponibilidad real (solapamiento de horarios, distinción gastable/no gastable, búsqueda recursiva del siguiente hueco entre citas y entre días) que **funciona de verdad** al ejecutarlo. El manejo de entradas inválidas es notablemente robusto: nada de lo que le tiré la hizo reventar. La persistencia en JSON está bien modelada.

Las fallas son puntuales y arreglables: (1) "Salir" nunca funciona por comparar string con int (`pythonmain.py:69`); (2) el consumo de inventario no se persiste porque `guardar_datos` no llama a `guardar_recursos`; (3) hay una función completa (`ver_detalles_cita`) que quedó desconectada del menú aunque el informe la anuncia. Ninguna es un problema conceptual grave — son cabos sueltos de integración.

- **Principal fortaleza:** el motor de agendamiento y disponibilidad funciona de verdad, con validación defensiva sobresaliente y una arquitectura de datos coherente entre código, JSON e informe.
- **Principal área de mejora:** cerrar los cabos sueltos de integración — arreglar la comparación de "Salir" (`opcion == "3"`), persistir el inventario tras agendar (llamar a `guardar_recursos`), y conectar `ver_detalles_cita` al menú de cliente o retirarla del informe.
