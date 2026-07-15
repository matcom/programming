# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #298
- **Repositorio:** https://github.com/hectorsebastian2025/Planificador-de-Gimnasio
- **Estudiante:** Héctor Sebastián Rodríguez Chacón
- **Grupo:** C121
- **Descripción declarada:** Planificador de eventos en un gimnasio donde puedes reservar los locales disponibles en la institución.

---

## Nota metodológica importante

Este proyecto **no es una aplicación de consola**: es una app web construida con **Streamlit** (multipágina, con sidebar personalizado). No usa `input()` ni un menú de texto. Para evaluarlo:

1. Ejecuté la **lógica de negocio** directamente importando los módulos `storage.py`, `storage_clientes.py` y `storage_eventos.py`, sobre una **copia** del `data.json` real del repo (para no corromper el archivo del estudiante), ejercitando flujos válidos e inválidos.
2. Arranqué además la **GUI en modo headless** (`streamlit run app_streamlit.py --server.headless true`): arrancó limpiamente (`Uvicorn server started`), sirvió la portada con `HTTP 200` (6602 bytes) y **sin ningún `Traceback`**.

La arquitectura separa bien la lógica del negocio (los `storage_*.py`) de la presentación (las `pages/*.py`), lo que permitió testear el núcleo de forma aislada. Esto es un acierto de diseño.

## Dimensión 1 — Qué hace el programa

Sistema de gestión y reserva de recursos de un gimnasio, persistido en `data/data.json`. La portada (`app_streamlit.py`) muestra un logo, un contador de clientes y barras de progreso de ocupación por recurso. El sidebar da acceso a cuatro páginas:

- **Registro de clientes** (`pages/clientes_registro.py:53-63`): alta con nombre, edad (14–120) y plan.
- **Gestión de clientes** (`pages/gestion_clientes.py`): baja lógica (marca `INACTIVO`) con confirmación.
- **Reservación** (`pages/eventos.py:87-115`): reserva un recurso para un cliente en fecha+turno, y si falla ofrece hasta 3 alternativas.
- **Gestión de eventos** (`pages/gestion_eventos.py`): cancela reservas activas de un cliente.

El núcleo real está en `storage_eventos.py:reservar_recurso` (líneas 4-160), que aplica todas las reglas de negocio: cliente activo, recurso existente, turno y fecha válidos, ventana de 7 días, acceso por plan, no-duplicado por turno, capacidad del recurso, disponibilidad de personal, y la regla especial de la cámara hiperbárica.

## Dimensión 2 — Organización del código

**Fortalezas.**
- Separación limpia lógica/presentación: los `storage_*.py` no importan `streamlit`; las `pages/*.py` solo llaman funciones y renderizan. Esto es lo que permitió testear todo el negocio sin GUI.
- Funciones con responsabilidad única y docstrings (`storage.py:15,21,56,65`; `storage_clientes.py:4,34`).
- Rutas robustas basadas en `os.path.dirname(__file__)` + `normpath` (`storage.py:7-13`, `app_streamlit.py:11-13`), no rutas relativas frágiles.

**Debilidades.**
- El módulo `models.py` define 6 clases (`Cliente`, `Recurso`, `Personal`, `Plan`, `Evento`, `Gimnasio`) pero en la práctica **solo se usan `Cliente`, `Recurso` y `Personal`**; `Plan`, `Evento` y `Gimnasio` no se instancian en ningún flujo real (`grep` confirma que solo `Cliente` se usa en pages, y `Recurso`/`Personal` solo en `cargar_objetos`, que a su vez apenas se consume). Es andamiaje que quedó sin conectar.
- `models.Plan` declara `precio_mensual` (`models.py:35`) pero el `data.json` no tiene precios: la clase no se puede construir con los datos reales. Código muerto.
- El bloque del sidebar (~30 líneas de botones `st.switch_page`) está **copiado literalmente en las 4 páginas** (`clientes_registro.py:18-41`, `gestion_clientes.py:17-39`, `eventos.py:21-44`, `gestion_eventos.py:24-47`). Debería extraerse a una función `render_sidebar()` en un módulo compartido.
- Queda un `src/tempCodeRunnerFile.py` (26 bytes, artefacto del editor) y un `src/__init__.py`/`pages/__init__.py` vacíos que no aportan.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté sobre copias del `data.json` real. Todo lo que sigue son mensajes/valores concretos observados.

**Lecturas (portada):**
1. `contar_clientes()` → `16` (cuenta solo `ACTIVO`; hay 29 clientes, 16 activos). Correcto según la implementación.
2. `mostrar_capacidad_rec_actual()` → todos `0/capacidad` (p.ej. `Sala de musculación: 0/40`). Correcto **con matiz**: cuenta reservas en estado `EN_CURSO`, y como todas las reservas del dataset son de ene–mar 2026 (fecha actual 2026-07-15), `actualizar_estado` ya las marcó `FINALIZADA`; ninguna está `EN_CURSO`, de ahí el 0.

**Reserva — flujos inválidos (todos rechazados con el mensaje correcto):**
3. Cliente inexistente → `Cliente no encontrado.`
4. Recurso inexistente → `Recurso no encontrado.`
5. Turno basura (`99:00-99:00`) → `Turno inválido. Debe ser uno de: [...]`
6. Fecha mal formateada (`2026/07/17`) → `Formato de fecha inválido. Usa 'YYYY-MM-DD'.`
7. Fecha en el pasado → `No se puede reservar un turno que ya comenzó o terminó`
8. Fecha a +30 días → `Solo se permiten reservas hasta 7 días en el futuro.`
9. Plan Básico intentando Jacuzzi → `El cliente no puede acceder a 'Jacuzzi' con su plan 'Básico'.`
10. Cliente `INACTIVO` → `Cliente no encontrado.` (correcto: el filtro exige `estado == "ACTIVO"`, `storage_eventos.py:15`).
11. Cámara hiperbárica sin cita previa → `Para reservar la Cámara hiperbárica debes haber tenido una cita con el médico del deporte en los últimos 7 días.`

**Reserva — flujos válidos:**
12. Reserva válida cliente 2 en Sala de musculación → `Reserva exitosa para Lili Chacon...`. Correcto.
13. Capacidad: consultorio (cap 1) reservado por cliente 2; segundo cliente mismo turno → `Recurso no disponible: capacidad máxima alcanzada en ese turno.` Correcto.
14. Duplicado: mismo cliente, mismo recurso/turno → `El cliente ya tiene una reserva en ese turno.` Correcto.
15. **Cámara hiperbárica — camino feliz:** inyecté una cita de consultorio `FINALIZADA` a 2 días atrás para el cliente 7; la reserva de la cámara **se autorizó correctamente**. La regla especial funciona en ambas direcciones.
16. `eliminar_reserva` correcta → `Reserva cancelada correctamente.`; repetida → `La reserva no está activa.`; inexistente → `No se encontró la reserva.` Los tres correctos.
17. `alternativa_reservar_recurso` → devolvió 3 tuplas `(fecha, turno)` reales dentro de la ventana de 7 días, saltando el turno solicitado.

**Clientes:**
18. `agregar_cliente` → OK; ID repetido → `Ya existe un cliente con ese ID.`; `eliminar_cliente` existente → `Cliente con ID 30 eliminado y sus reservas activas finalizadas.`; inexistente → `No se encontró un cliente con ese ID.` Todos correctos.

**Bug real detectado (lógico, no crash):**
19. **La validación de disponibilidad de personal es código muerto.** En `reservar_recurso` la comprobación de capacidad (`storage_eventos.py:87-97`) se ejecuta *antes* que la de personal (`:111-123`). Los tres recursos que requieren personal — Sala de fisioterapia, Consultorio médico, Nutricionista — tienen **capacidad 1** en `data.json`. Por tanto, en cuanto hay una reserva activa, la capacidad ya está llena y el flujo aborta con "capacidad máxima alcanzada" **sin llegar nunca** a la validación de personal. Lo verifiqué: dos clientes a fisioterapia en el mismo turno → el segundo es rechazado por capacidad, no por personal. La regla de personal solo podría dispararse si algún recurso con personal tuviera capacidad > 1, cosa que no ocurre.

**Segundo detalle (menor, inconsistencia de campo):**
20. El campo `personal_necesario` de la reserva solo se escribe si `personal_disponible` es `True` (`storage_eventos.py:155-156`). Como en la práctica siempre es `True` (por el punto 19), el campo termina guardándose siempre — pero para recursos sin personal se guarda como `None` (verificado: reserva de Sala de musculación → `"personal_necesario": None`). La intención parece haber sido no escribir el campo cuando no hay personal; el efecto real es que siempre se escribe. Sin consecuencias funcionales, pero la condición del `if` no aporta lo que sugiere.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores idiomático y consistente:** todas las reglas se expresan como `raise Exception(mensaje)` y las pages las capturan con `try/except` mostrando `st.error`. Para 1er año está muy bien; el siguiente paso sería usar excepciones específicas (`ValueError`) en vez de `Exception` genérica.
- Uso correcto de `.get(...)` con defaults y `setdefault` (`storage_eventos.py:74`) — buen reflejo defensivo.
- Nombres de variables con tildes y `ñ` (`ocupación_actual`, `máx_clientes`, `capacidad_máxima`) en `models.py` y las claves del JSON. Es válido en Python 3, pero mezcla español acentuado con identificadores y complica el tecleo; convención habitual es ASCII para identificadores.
- `from datetime import ... timedelta` importado en `storage_eventos.py:1` pero `timedelta` se usa; en `storage.py:4` se importa `timedelta` y **no se usa** (import huérfano menor).
- Comentario "Validar si el cliente puede acceder a la sala de musculacion" (`storage_eventos.py:61`) revela una solución específica y correcta al caso "con/sin entrenador" (`:62-71`) — buen manejo de un caso de dominio no trivial.

## Dimensión 5 — Datos y persistencia

- Persistencia en un único `data/data.json` con `json.dump(..., ensure_ascii=False, indent=4)` (`storage.py:23-24`). Legible y correcto para el alcance.
- Modelo de datos sensato: `gimnasio` con `recursos`, `personal`, `planes`, `clientes`, `reservas`, más `horario`, `MAPA_DE_ROLES` y `capacidad_máxima`. El `MAPA_DE_ROLES` es una idea limpia para asociar recurso→rol requerido.
- **Cada página hace `cargar_datos()` → `actualizar_estado()` → `guardar_datos()` al importarse** (p.ej. `eventos.py:6-8`). Funciona, pero implica reescribir el JSON completo en cada navegación; en un sistema concurrente sería frágil (dos usuarios pisándose). Para el alcance de un proyecto de 1er año es aceptable.
- **Generación de IDs frágil:** clientes usan `id = len(clientes) + 1` (`clientes_registro.py:56`) y reservas `id = reservas[-1]["id"] + 1` (`storage_eventos.py:99-101`). Como los clientes nunca se borran físicamente (solo `INACTIVO`), el `len+1` de clientes hoy no colisiona; pero es un patrón que se rompería si alguna vez se eliminara físicamente un registro. Lo idiomático sería `max(ids)+1`.
- Hay clientes con nombres duplicados (`Jian`, `Almendra`, `Caliente`, `Guanajo`) — el sistema los distingue por ID, lo cual es correcto, pero conviene tenerlo presente.

## Dimensión 6 — Informe (`report.md`)

El informe es claro, bien estructurado y en general fiel al código. Discrepancias detectadas:

1. **Capacidades incorrectas.** La tabla del informe (`report.md:23,27`) declara **Sala de fisioterapia = 2** y **Nutricionista = 2**, pero en `data.json` ambas tienen **capacidad 1**. Esta discrepancia no es cosmética: es exactamente lo que hace que la validación de personal (punto 19) sea código muerto. Si las capacidades fueran realmente 2 como dice el informe, la regla de personal sí se ejercitaría.
2. **Nomenclatura de estados.** El informe (`report.md:105-109`) lista los estados como `ACTIVA / EN CURSO / INACTIVA / CANCELADA`, pero el código usa `EN_CURSO` y `FINALIZADA` (no `INACTIVA`) — ver `storage.py:100-102`. Es una inconsistencia de etiquetas entre doc y código.
3. **"Entrenadores" en el personal.** El informe menciona un equipo con "entrenadores" (`report.md:6`) y los planes distinguen "con/sin entrenador", pero el `data.json` de `personal` **no incluye ningún rol Entrenador** (solo Fisioterapeuta, médico del deporte, Nutricionista) y el `MAPA_DE_ROLES` no mapea la sala de musculación a ningún rol. El "con entrenador" se resuelve solo como una variante de acceso del plan (`storage_eventos.py:62-71`), no como asignación de personal real.
4. **"Si el recurso requiere personal y este no está disponible, la reserva se cancela"** (`report.md:100`): la intención está en el código pero, como se mostró, es inalcanzable con las capacidades actuales.
5. La conclusión dice que el proyecto "**demuestra** un diseño consciente del dominio" (`report.md:139`). El diseño *sí* es consciente del dominio, pero conviene saber que **no hay validación automatizada** (tests) que lo demuestre; la evidencia es la ejecución manual.

El informe **no exagera groseramente** las features — casi todo lo que describe existe y funciona. Los desajustes son de precisión (capacidades, nombres de estados) más que invención.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido**. Es una app web funcional, con una separación limpia entre lógica y presentación que resistió sin problemas ser ejecutada de forma aislada y en modo headless (cero `Traceback`). El conjunto de reglas de negocio es genuinamente ambicioso para 1er año: ventana temporal de 7 días, acceso por plan con el caso especial "con/sin entrenador", capacidad por turno, no-duplicados y la regla encadenada de la cámara hiperbárica (que exige una cita previa de consultorio) — y **todas se comportaron correctamente** en los flujos válidos e inválidos que probé. El manejo de errores es consistente y los mensajes al usuario son claros.

Las debilidades son de rigor y limpieza, no de funcionamiento: un bloque de validación (disponibilidad de personal) que quedó inalcanzable porque las capacidades reales son 1, varias clases de `models.py` sin conectar, ~30 líneas de sidebar duplicadas en cada página, generación de IDs frágil, y un informe con capacidades y nombres de estados que no cuadran con el `data.json`.

- **Principal fortaleza:** una lógica de negocio rica y correcta, limpiamente separada de la GUI, verificada al ejecutarla de verdad — incluyendo reglas de dominio no triviales como la autorización encadenada de la cámara hiperbárica.
- **Principal área de mejora:** cerrar la brecha entre lo declarado y lo real — corregir las capacidades del `data.json` (o del informe) para que la validación de personal deje de ser código muerto, y eliminar el andamiaje sin usar (clases de `models.py`, sidebar duplicado, archivos vacíos).
