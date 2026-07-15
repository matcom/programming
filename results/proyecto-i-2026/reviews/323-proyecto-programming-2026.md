# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #323
- **Repositorio:** https://github.com/Alberto67067/Proyecto-Programming-2026
- **Estudiante:** Alberto Martinez Machin
- **Grupo:** c-122
- **Descripción declarada:** Sistema de gestión para una "barra móvil" de eventos: inventario de botellas e insumos (consumibles y no consumibles), planificación de eventos en calendarios, con dos conflictos de negocio modelados — falta de stock para un evento y solapamiento de fechas entre eventos.

---

## Nota metodológica importante

La verificación automática del bot marcó **1/6** ("No se encontraron archivos `.py`", "No se encontró `report.md`"). **Esto es un falso negativo:** el repositorio contiene un único archivo `Proyecto2.rar` (RAR v5, 122 KB) que empaqueta todo el proyecto. El bot no descomprime `.rar`, por eso no vio nada.

Al extraer el `.rar` (con `bsdtar`/libarchive) aparece un proyecto **completo y sustancial**: 6 módulos Python, `report.md`, JSONs de datos reales y un almacén/calendario de prueba ya poblados. Todo se ejecutó de verdad. La pérdida de puntos por "no ejecutable" sería injusta; el trabajo existe.

Dos observaciones de empaquetado importantes:
1. **Entregar el código dentro de un `.rar`** (en vez de subir los archivos al repo) rompe la verificación automática y dificulta el `git diff`. Para próximas entregas: subir los `.py` directamente.
2. El punto de entrada se llama literalmente **`python main.py`** (con espacio, dos "palabras") en vez de `main.py`. Se ejecuta con `python "python main.py"`. Es un nombre de archivo accidental, casi con seguridad producto de copiar un comando de terminal como nombre.

## Dimensión 1 — Qué hace el programa

Aplicación de **consola** con tres subsistemas bien delimitados:

- **Autenticación y usuarios** (`MenuInic.py:9` clase `Autenticacion`): login con contraseñas hasheadas SHA-256 (`MenuInic.py:59`), roles `administrador`/`coordinador` con permisos diferenciados (`MenuInic.py:642` `obtener_permisos_usuario`), registro de usuarios (solo admin, `MenuInic.py:530`), activar/desactivar usuarios (`MenuInic.py:835`), log de accesos a `accesos.log` (`MenuInic.py:176`).
- **Inventario / almacén** (`Inventario.py:8` `Almacen`, `Inventario_Obj.py:10` `Recurso`, gestor interactivo en `AlmacenCreate.py:8` `GestorAlmacen`): crear almacén con capacidad, añadir recursos (manuales o desde 21 predefinidos en `AlmacenCreate.py:18`), control de capacidad por espacio ocupado, alerta de stock crítico, persistencia JSON.
- **Calendario / eventos** (`Calendario.py:147` `Calendario`, con dataclasses `Evento` en `:46` e `ItemEvento` en `:14`): crear eventos con detección de solapamiento de fechas (`Calendario.py:394`), asignar recursos a eventos, **reservar** (restar del inventario, `Calendario.py:750`) y **liberar** (devolver, `Calendario.py:814`), estadísticas y vista de calendario mensual (`Calendario.py:1045`).

El flujo declarado en el informe se cumple: crear almacén → añadir productos → crear calendario → planificar evento → asignar recurso → reservar (se resta del inventario). Verifiqué que hay estado real guardado: `calendarios/uno_20260626_160657.json` contiene un evento `azul` con un ítem `Vodka` (`cantidad_requerida: 210, cantidad_asignada: 50`, estado `confirmado`) — evidencia de que el propio estudiante ejercitó el ciclo de reserva.

## Dimensión 2 — Organización del código

**Fortaleza clara para un primer año.** El proyecto está genuinamente modularizado por responsabilidad:

- `Inventario_Obj.py` — modelo de dominio (`Recurso`).
- `Inventario.py` — lógica de almacén (`Almacen`: add, usar, recargar, buscar, serializar).
- `AlmacenCreate.py` — capa interactiva de gestión de inventario (separada del modelo).
- `Calendario.py` — dominio de eventos + capa interactiva.
- `MenuInic.py` — autenticación y orquestación de menús.
- `python main.py` — punto de entrada mínimo (`import MenuInic`).

Uso correcto y idiomático de `@dataclass` con `field(default_factory=...)` para `Evento`/`ItemEvento` (`Calendario.py:46`), `to_dict`/`from_dict` como patrón de serialización consistente, type hints en casi todas las firmas. Esto está por encima de la media de primer año.

Debilidades:
- **Un `Calendario.py` de ~1500 líneas** mezcla modelo (`Evento`, `ItemEvento`), lógica de negocio (`verificar_conflictos`, `reservar_recursos_directo`) y toda la capa de menús interactivos (`_crear_evento_interactivo`, `mostrar_menu_calendario`). La separación modelo↔UI que sí logró entre `Inventario.py` y `AlmacenCreate.py` no la replicó aquí.
- **Métodos redundantes**: coexisten `reservar_recursos_para_evento` (`:685`), `reservar_recursos_directo` (`:750`) y `asignar_recursos_automaticamente` (`:631`) con solapamiento de propósito; también `verificar_stock_disponible`/`verificar_stock_para_evento`/`verificar_disponibilidad_recursos`. Se nota la iteración ("VERSIÓN CORREGIDA" en cada encabezado) sin poda posterior.
- Convención de nombres con guion bajo inicial (`_add_item`, `_save`, `_usar`) usada para métodos que en realidad son públicos y se llaman desde otros módulos — el `_` sugiere privado pero no lo son.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Entorno: `uv venv --python 3.12`, sin dependencias de terceros (solo stdlib). `py_compile` de los 6 módulos: **todos OK**. Lo que corrí y observé:

1. **`Almacen._add_item` con control de capacidad** — creé almacén cap 100, añadí Vodka×50 (espac 1) → OK, `capacidad_dis=50`. Intenté Manteles×30 (espac 2 = 60 > 50) → correctamente **rechazado** con mensaje de capacidad insuficiente. ✅
2. **Uso/reposición de stock** — `_usar(Vodka, 20)` → stock 50→30. ✅
3. **Detección de conflicto de fechas** — evento "Boda" 2026-08-01→02; luego "Fiesta" 2026-08-01 → **detectado como conflicto** (bloqueado, lista el evento solapado); "Cumple" 2026-09-10→11 → **sin conflicto**, creado. La lógica de solapamiento `not (fin1 < inicio2 or fin2 < inicio1)` (`Calendario.py:118`) es correcta. ✅
4. **Reserva que resta del inventario** — asigné Vodka×10 a un evento y reservé: stock 30→20. El descuento real del inventario funciona. ✅
5. **`verificar_stock_disponible` — 3 ramas**: suficiente ("Disponible: 20"), insuficiente ("Requerido: 999"), recurso inexistente ("no encontrado"). Las tres devuelven el dict y mensaje correctos. ✅
6. **Persistencia round-trip** — `_save`→`_load` del almacén recupera recursos y capacidad. ✅
7. **Login real (con PTY)** — `admin`/`admin123` vía `getpass` → hash SHA-256 coincide, imprime "¡Bienvenido!". La autenticación funciona de punta a punta. ✅
8. **Consola completa** — `python "python main.py"` arranca, menú principal navegable, "Información del sistema" lista los 2 usuarios reales. ✅

**Bugs/limitaciones reales encontradas (del estudiante, no del entorno):**

- **Sin validación de fechas** (`Calendario.py:288` `crear_evento`): creé un evento con `fecha_inicio="fecha-basura"` y **se creó sin error**. Peor: `obtener_periodo` (`:99`) lanza `ValueError: Invalid isoformat string` con esas fechas, pero `hay_conflicto_con` (`:110`) lo **traga con un `except: return False`** — es decir, un evento con fecha inválida **nunca entra en conflicto con nada** y se cuela silenciosamente. Falta validar el formato de fecha al crear.
- **`_usar` de `Almacen` libera espacio al consumir** (`Inventario.py:41-47`): al usar/reservar stock, `capacidad_dis` **aumenta**. Semánticamente cuestionable — reservar recursos para un evento no "vacía" físicamente el almacén, y se ve un desajuste de capacidad tras el round-trip (esperaba 90, obtuve 80). No rompe la ejecución, pero el modelo de "capacidad" queda inconsistente entre reservas y devoluciones.
- El `getpass` del login **requiere terminal real**: alimentar la contraseña por tubería (`printf`) no funciona porque `getpass` lee de `/dev/tty`. No es un bug del estudiante (es comportamiento de `getpass`), pero conviene saberlo para evaluarlo (hay que usarlo interactivamente o con PTY).

Ningún `Traceback` no controlado durante el uso normal; el manejo de excepciones en los menús (`try/except` en los bucles de menú) evita que la app reviente.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Buenas: type hints extensos, dataclasses, f-strings, `with open(...)` para archivos, SHA-256 para contraseñas (no texto plano — muy bien), `default_factory` correcto.

Mejorables (todas menores para 1er año):
- **`except:` desnudos** en varios puntos (`MenuInic.py:41`, `Inventario_Obj.py:77/84/91`, `Calendario.py:105/119`). Capturan todo, incluido lo que no deberían, y ocultan bugs (justo lo que pasó con las fechas). Preferir `except ValueError:` o al menos `except Exception as e:` y loguear.
- **Sets para `CATEGORIAS`/`GRUPO`** (`Inventario_Obj.py:7-8`) usados luego con `list(CATEGORIAS)[opcion-1]` (`AlmacenCreate.py:62`): el orden de un `set` no es estable, así que el número de menú no siempre mapea a la categoría que el usuario ve. Debería ser una `list` o `tuple`.
- Emojis en todos los mensajes: simpático, pero mézclalo con menos ruido para que los mensajes de error resalten.
- Errata en constante: `"Insumos Escenciales"` (→ *Esenciales*), presente en datos y menús.

## Dimensión 5 — Datos y persistencia

Modelo sólido y coherente:
- `Recurso` con código autogenerado (`_gen_code`, primeras 3 letras del nombre + 3 del grupo → `VODCON`), `to_dict` para serializar (`Inventario_Obj.py:98`).
- `Almacen._save`/`_load` (JSON, `ensure_ascii=False`, reconstrucción de objetos `Recurso` desde dict) — round-trip verificado.
- `Evento`/`ItemEvento` con `to_dict`/`from_dict` simétricos; `Calendario` guarda/carga la lista de eventos con timestamp de última actualización.
- Los datos reales del repo (`inventario.json` con 20 recursos, `usuarios.json`, el calendario `uno` con un evento reservado) confirman que la persistencia se usó de verdad.

Detalle: se guarda `capacidad_dis` en el JSON del almacén (`Inventario.py:66`); combinado con el `_usar` que libera espacio, la capacidad persistida puede quedar desalineada de la realidad física — pero como concepto de persistencia, está bien resuelto.

## Dimensión 6 — Informe (`report.md`)

El informe es **honesto y humilde**, no infla nada — al contrario, se subestima ("proyecto sencillo", "realmente no aprendí mucho"). Describe con precisión el dominio, los dos conflictos de negocio (stock y solapamiento de fechas), da credenciales de prueba (`admin`/`admin123`) e instrucciones de uso correctas. No hay discrepancias entre lo que declara y lo que hace el código; si acaso, el informe **vende de menos** un proyecto que es más completo de lo que su autor reconoce.

Faltas menores del informe: 307 palabras (por debajo del umbral de 2000 del bot; el bot igual no lo halló por estar en el `.rar`), sin secciones estructuradas ni ejemplos de ejecución. Las carpetas `clientes/`, `reportes/`, `inventario/`, `eventos/` se crean pero quedan **vacías** — no hay un subsistema de clientes ni de reportes generados a disco, así que el andamiaje de directorios promete algo que no se llegó a implementar (el informe, correctamente, no lo reclama).

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y por encima de lo esperable en primer año**, ejecutable de punta a punta, con un dominio de negocio no trivial correctamente modelado: inventario con control de capacidad, eventos con detección real de solapamiento de fechas, y un ciclo reservar/liberar que efectivamente mueve el stock. La modularización (5 módulos + entrada), el uso de dataclasses, type hints y hashing de contraseñas son señales de madurez. Verifiqué ejecutando —no leyendo— que la autenticación, la gestión de capacidad, la detección de conflictos y la persistencia funcionan con datos reales.

Las debilidades son de refinamiento, no de fondo: falta validar el formato de fechas (un evento con fecha basura se cuela y nunca "conflictúa"), hay métodos redundantes acumulados por iteración, `Calendario.py` mezcla modelo con UI, y el modelo de "capacidad" queda inconsistente porque `_usar` libera espacio. Y dos problemas de **entrega** que le costaron la verificación automática: el código va dentro de un `.rar` (súbelo descomprimido) y el entry point se llama `python main.py` (renómbralo a `main.py`).

- **Principal fortaleza:** modelado de dominio real y funcional (capacidad + conflicto de fechas + reserva de stock), con arquitectura modular y persistencia verificadas en ejecución — muy meritorio para las condiciones descritas.
- **Principal área de mejora:** validación de entradas (sobre todo fechas) y poda de la lógica duplicada; y a nivel de entrega, subir los `.py` al repo en vez de un `.rar` y arreglar el nombre del punto de entrada.
