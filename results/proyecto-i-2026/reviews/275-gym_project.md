# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #275
- **Repositorio:** https://github.com/LianBob/gym_project
- **Estudiante:** Lian Ernesto Gomez Piñero
- **Grupo:** c11
- **Descripción declarada:** Aplicación de gestión de sesiones de gimnasio con GUI, consola y entrenador IA. Desarrollada en Python con Tkinter, Rich y SQLite.

---

## Nota metodológica importante

El proyecto **no es una app de consola simple**: tiene **dos front-ends** que comparten la misma capa de lógica de negocio:

- `main.py` — GUI de escritorio con Tkinter (login, pestañas, diálogos, chat IA en hilos).
- `consola.py` — interfaz de consola interactiva con Rich (paneles, tablas, prompts).

Ambos consumen los mismos módulos (`database.py`, `sesiones.py`, `auth.py`, `entrenador_ia.py`, `ui.py`). Esta separación presentación/negocio es real y verificable, y es lo que permitió evaluar el núcleo del sistema sin depender de un display gráfico.

Adapté la ejecución así:

1. `py_compile` de los 8 módulos → todos compilan sin error.
2. Ejecuté la **lógica de negocio directamente** (import de `sesiones`, `database`, `entrenador_ia`) con datos reales, cubriendo flujos válidos e inválidos.
3. Ejecuté la **consola de verdad** con `printf` alimentando el menú (registro, login, ver sesiones, crear sesión con recursos).
4. Intenté arrancar la **GUI en headless** con `xvfb-run`: aborta con `xcb ... Aborting, sorry about that` / `Assertion !xcb_xlib_unknown_seq_number failed`. Verifiqué que **incluso un `tkinter` mínimo con `update_idletasks()` aborta igual bajo este xvfb** → es un fallo del entorno de renderizado headless (xcb), **no del código del estudiante**. El `import main` sin instanciar `GymApp` funciona limpio.

## Dimensión 1 — Qué hace el programa

Sistema de reserva de sesiones de entrenamiento con gestión de recursos limitados del gimnasio.

- **Autenticación** (`auth.py:22-115`): registro con SHA-256 + salt aleatoria por usuario (`auth.py:11-19`); login que recalcula el hash y compara (`auth.py:109-110`). Perfil físico: peso, edad, sexo, nivel de actividad.
- **Creación de sesión** (`sesiones.py:581-601`): valida horario, valida que los recursos no superen el total físico, verifica disponibilidad en la franja e inserta la sesión + las reservas de recursos.
- **Disponibilidad** (`sesiones.py:102-133`): detecta solapamientos temporales por recurso vía SQL, sumando cantidades reservadas y comparando contra la capacidad total.
- **Sugerencias inteligentes** (`sesiones.py:139-321`): cuando no cabe, busca hacia adelante y hacia atrás (hasta 7 días) saltando al final/inicio de la sesión bloqueante en vez de barrer minuto a minuto. Devuelve alternativas ordenadas por cercanía.
- **Recurrencia** (`sesiones.py:642-655`, `consola.py:147-175`): frecuencias diaria/semanal/quincenal/mensual; agrupa las ocurrencias bajo un `grupo_id`.
- **Entrenador IA** (`entrenador_ia.py:52-154`): chat con Groq (LLaMA 3.3 70B) que, al cerrar el plan, emite un JSON que la app parsea (`entrenador_ia.py:26-49`) y convierte en sesiones reales.

Flujo verificado end-to-end en consola: registro de "tester" → login → crear sesión con banca → `Sesión creada para el 2026-07-16 09:00.`

## Dimensión 2 — Organización del código

**Fortalezas.** La modularidad es de las mejores que se ven en 1er año:

- Separación real presentación/negocio: los dos front-ends (`main.py`, `consola.py`) reutilizan la misma lógica sin duplicarla.
- Cada módulo tiene una responsabilidad clara y única (`database.py` = persistencia + esquema, `auth.py` = credenciales, `sesiones.py` = negocio, `ui.py` = selector de recursos de consola, `entrenador_ia.py` = cliente LLM).
- Funciones auxiliares privadas bien nombradas (`_str_a_datetime`, `_fin_sesion_str`, `_obtener_reservas_solapadas`, `_buscar_siguiente_adelante`).
- Docstrings presentes y descriptivos en casi todas las funciones.

**Debilidades menores.**

- `sesiones.py:327-426` contiene ~100 líneas de código comentado (dos versiones antiguas de `crear_sesion`/`confirmar_sugerencia`). Debieron borrarse — Git ya guarda la historia.
- Duplicación del cálculo de ocurrencias de recurrencia: existe `generar_ocurrencias_futuras` (`consola.py:147-175`) pero `main.py:571-588` reimplementa la misma lógica inline en vez de reusar esa función.
- Constantes de horario duplicadas: `HORA_APERTURA/HORA_CIERRE` viven en `sesiones.py:11-12` pero se redefinen locales en `consola.py:89-90` y con literales en `main.py:422-426`.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo siguiente se **ejecutó**, no se leyó:

1. **Sesión válida** (banca x1, mañana 09:00) → `True | Sesión creada para el 2026-07-16 09:00.` ✔
2. **Conflicto de capacidad** (pedir 6 bancas cuando ya hay 1 reservada y el total es 6) → `False | No hay suficientes unidades del recurso #8 en ese horario.` ✔ La lógica de solapamiento + suma funciona.
3. **Sugerencias** para esa franja imposible → `['2026-07-16 08:00', '2026-07-16 10:01']` ✔ Devuelve una antes y una después, ordenadas por cercanía. El `10:01` refleja el salto al minuto siguiente tras liberarse el recurso.
4. **Fecha en el pasado** (`2020-01-01`) → `False | No puedes reservar en el pasado.` ✔
5. **Antes de apertura** (04:00) → `False | El gimnasio abre a las 5:00.` ✔
6. **Termina tras el cierre** (21:30 + 60 min) → `False | La sesión debe terminar antes de las 22:00.` ✔
7. **Recurso inexistente** (id 999) → `False | El recurso #999 no existe.` ✔
8. **Sobre-solicitud física** (100 bancas) → `False | No hay suficientes unidades del recurso #8.` ✔
9. **Recurrencia semanal** (grupo + 3 ocurrencias) → `extra creadas: 3` ✔
10. **Extractor de JSON del IA** (`_extraer_json_plan`): probado con bloque ```json, JSON balanceado sin backticks, texto sin JSON y JSON roto → devuelve el dict correcto en los dos primeros y `None` en los dos últimos, sin crashear. ✔ Robusto de verdad.
11. **Chat IA sin `GROQ_API_KEY`** → captura `Connection error`, muestra el mensaje y devuelve `None`; no arrastra el fallo al resto de la app. ✔
12. **Consola end-to-end** con `printf`: registro → login → ver sesiones → crear sesión → recurrencia, sin `Traceback`. La validación de opciones de Rich rechaza correctamente entradas fuera de menú (`Please select one of the available options`). ✔

**Bug real encontrado (menor):** `datos_prueba.py` **no crea las tablas** antes de insertar. Ejecutado sobre una BD limpia → `sqlite3.OperationalError: no such table: usuarios` (`datos_prueba.py:36`). Solo funciona si antes se corrió `main.py`/`consola.py` (que sí llaman `crear_tablas()`). Debería llamar `database.crear_tablas(); database.precargar_recursos()` al inicio.

**Fallo de entorno (no del estudiante):** arranque de la GUI Tkinter bajo `xvfb` aborta por xcb; reproducido también con un Tk mínimo → es limitación del headless, no del código.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Nivel claramente por encima de la media de 1er año.

- **Muy bien:** hashing con salt (no contraseñas en claro), consultas SQL **parametrizadas** en todo el código (sin `f-string` en SQL → sin inyección), `row_factory = Row` para acceso por nombre, `PRAGMA foreign_keys = ON`, uso de `try/except ValueError` en las conversiones de entrada (`main.py:153-158`, `auth.py:49-55`), transacción con `rollback` en `_insertar_sesion` (`sesiones.py:559-578`).
- **Idiomático:** `defaultdict` para acumular, type hints en firmas, `strptime/strftime` centralizados en helpers.
- **Mejorable (menor):**
  - Imports locales repetidos dentro de funciones (`from collections import defaultdict` aparece 3 veces en `sesiones.py`; `from datetime import datetime` reimportado en `sesiones.py:489`). Deberían subir al tope del módulo.
  - El estado (`estado='confirmada'`) nunca cambia: no hay cancelar/completar sesión, aunque el esquema lo contempla.
  - `ui.py:100-106` reabre la conexión dentro de un bucle solo para traer un nombre; podría cachearse. Detalle de eficiencia, no de corrección.

## Dimensión 5 — Datos y persistencia

Modelo relacional **sólido y normalizado**, otra vez por encima del nivel esperado:

- 5 tablas con `CHECK` constraints (`sexo`, `nivel_actividad`, `frecuencia` — `database.py:28-29,69`), `UNIQUE` en `nombre_usuario` y `recursos.nombre`, claves foráneas explícitas.
- Tabla asociativa `recursos_sesion` con **clave primaria compuesta** `(sesion_id, recurso_id)` y `ON DELETE CASCADE` (`database.py:55-64`) — modelado correcto de la relación N:M sesión↔recurso.
- **Índices** sobre las columnas calientes de las consultas de solapamiento (`database.py:81-95`).
- Migración defensiva con `ALTER TABLE ... ADD COLUMN grupo_id` envuelta en `try/except OperationalError` (`database.py:74-79`) — muestra conciencia de evolución de esquema.

**Observación:** cada operación abre y cierra su propia conexión. Es correcto y simple, pero implica que la verificación de disponibilidad y la inserción ocurren en **conexiones distintas** (ver Dimensión 6).

## Dimensión 6 — Informe (`report.md`)

El informe es extenso, bien escrito y en general **honesto y fiel al código**. Describe correctamente la arquitectura modular, el esquema de BD, el algoritmo de sugerencias y el entrenador IA. `README.md` es una copia del `report.md`.

Dos matices a marcar:

1. **Sobreafirmación sobre transacciones/concurrencia.** El informe dice (sección *Manejo de concurrencia* y *Base de datos*): *"para evitar condiciones de carrera, las comprobaciones de disponibilidad y la inserción de la sesión se realizan dentro de una misma transacción"*. En el código, `crear_sesion_intento` (`sesiones.py:581-601`) llama a `verificar_disponibilidad` (`sesiones.py:102`, abre su conexión) y **luego** a `_insertar_sesion` (`sesiones.py:559`, abre **otra** conexión). Son transacciones separadas: entre la comprobación y la inserción existe una ventana teórica de carrera. Para un proyecto monousuario de 1er año no tiene impacto práctico, pero el informe afirma una garantía que el código no da.

2. **"commits atómicos y mensajes claros"** (sección *Aprendizajes*). La historia real tiene **4 commits** (`Primer commit`, `Corrige nombres`, `Añade informe`, `Create README.md`). Es un flujo de versionado razonable, pero no ilustra "commits atómicos" en el sentido de una unidad lógica por commit.

Ambas son sobreestimaciones menores, no invenciones de features: todo lo descrito **existe y funciona**.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **excepcional para 1er año**. Se ejecutó en profundidad y respondió correctamente en todos los flujos válidos e inválidos probados: capacidad de recursos, solapamientos temporales, validación de horario, sugerencias inteligentes hacia adelante/atrás, recurrencia y extracción robusta de JSON del LLM. La ingeniería está por encima del nivel esperado: dos front-ends sobre una única capa de negocio, esquema relacional normalizado con claves compuestas, índices y `CHECK`, SQL parametrizado en todo el código, y credenciales con SHA-256 + salt. El único bug propio detectado es menor y aislado (`datos_prueba.py` no crea las tablas). El resto de observaciones son de estilo (código muerto comentado, imports locales duplicados) o de precisión del informe (afirma una transacción única que el código no implementa).

- **Principal fortaleza:** arquitectura y modelo de datos — separación limpia presentación/negocio reutilizada por GUI y consola, sobre un esquema SQLite normalizado con integridad referencial, verificado corriendo el núcleo completo.
- **Principal área de mejora:** limpieza y fidelidad — borrar los ~100 renglones comentados de `sesiones.py`, arreglar el arranque de `datos_prueba.py`, y ajustar el informe donde afirma una garantía transaccional que el código no cumple.
