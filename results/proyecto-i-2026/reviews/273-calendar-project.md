# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #273
- **Repositorio:** https://github.com/isabela533/Calendar-Project
- **Estudiante:** Isabela Alvarez Ramos
- **Grupo:** C122
- **Descripción declarada:** AgencePro — aplicación web para agencias de marketing que organiza eventos, recursos, equipo humano y presupuesto desde un solo lugar.

---

## Nota metodológica importante

**Esto NO es una app de consola.** Es una aplicación web construida con **Streamlit** (punto de entrada `main.py` → `streamlit run main.py`). No tiene menú de `input()`. Para evaluar la corrección funcional de verdad, adapté la ejecución así:

1. **Lógica de negocio ejecutada directamente.** El proyecto separa bien la lógica de la interfaz: `classes/`, `gestor/` y `tools/` no importan Streamlit y son ejecutables de forma aislada. Instancié `Session` con la cuenta de ejemplo `isabela.alvarez.ramos@gmail.com` y corrí `add_event`, `delete_event`, validación de fechas, ocupación/liberación de recursos, restricciones y gestión de operadores con datos reales del repo. (Trabajé sobre una copia de respaldo del JSON para no corromper el dato de ejemplo.)
2. **GUI en modo headless.** Arranqué `streamlit run main.py --server.headless true` y también la variante documentada en el README (`cd visual && streamlit run visual.py`). Ambas levantan el servidor sin `Traceback` y responden **HTTP 200**. No hubo forma de simular clics reales sin navegador, pero el arranque limpio y la ejecución directa de la lógica cubren la corrección funcional.
3. `py_compile` de **todos** los módulos: compilan sin error.

---

## Dimensión 1 — Qué hace el programa

AgencePro es un gestor integral para una agencia de marketing con autenticación por correo y dos roles (admin/operador):

- **Autenticación** (`visual/logic_buttons.py:7-57`): `handle_login` busca primero un archivo `data/<correo>.json` (dueño/admin); si no existe, recorre los `team_access` de todas las cuentas buscando al correo como operador. `handle_signup` crea una cuenta nueva o detecta una existente.
- **Eventos** (`classes/session.py:74-116`, `gestor/add_event.py`, `gestor/delete_event.py`): crear evento con nombre, fechas, costo, recursos y equipo; validación de presupuesto, fechas y restricciones; ocupación de inventario; eliminación con devolución de presupuesto y liberación de recursos.
- **Inventario** (`classes/resources.py`, `classes/working_team.py`): alta/baja/ocupar/liberar de recursos materiales y equipo humano, con control de disponibilidad y cantidad.
- **Restricciones** (`classes/restrictions.py`): co-requisitos (`{"Projector": ["Conference Room A"]}`) y exclusiones entre recursos, validadas al agendar.
- **Fechas clave** (`visual/fechas_clave.py`): 36 fechas comerciales del año calculadas (algunas con "n-ésimo día de la semana del mes").
- **Calendario** (`visual/calendario.py`): vista mensual en cuadrícula.
- **Reportes** (`visual/reportes.py:135-287`): presupuesto inicial/gastado/disponible, gasto por mes, eventos más costosos, recursos y roles más usados.
- **Persistencia** (`classes/gestor_json.py`): un JSON por cuenta en `data/`.

## Dimensión 2 — Organización del código

**Fortaleza destacada del proyecto.** La arquitectura en capas es notablemente madura para primer año:

- `Session` (`classes/session.py`) es un punto único de acceso que coordina managers especializados: `Resources_Manager`, `Team_Manager`, `Restrictions`, `Gestor_json`.
- La lógica de negocio (`classes/`, `gestor/`, `tools/`) está **limpiamente separada** de la GUI (`visual/`): ninguna clase de negocio importa Streamlit, y por eso pude ejecutarla aislada. Esto es exactamente lo que permite testear y es una decisión de diseño real.
- Responsabilidad única bien aplicada: `Gestor_json` es el único que toca el archivo (`classes/gestor_json.py:11-37`); `Restrictions.validate` (`classes/restrictions.py:51-68`) concentra la comprobación de reglas.
- Los `# region` en `session.py` agrupan operaciones por dominio (eventos, recursos, empleados, restricciones) — buen orden.

**Debilidades menores:**
- `add_event` de `Session` (`classes/session.py:74-88`) tiene un **valor de retorno polimórfico**: devuelve `True`, o una tupla `(False, suggestions)`, o lanza excepción. El dashboard lo consume correctamente (`visual/dashboard.py:350-359`), pero un contrato uniforme (siempre un dict/objeto resultado) sería más robusto y menos frágil.
- Los mensajes mezclan español e inglés (`session.py:76` en inglés, `session.py:54` en español). Inconsistente de cara al usuario.
- Los módulos `visual/*.py` son grandes por el CSS embebido inline (reportes.py = 287 líneas, la mayoría estilos). Funciona, pero mezcla presentación y lógica en el mismo archivo.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí la lógica de negocio con la cuenta de ejemplo (`money=49900.0`, 6 recursos, 5 roles, 4 eventos). Observado:

1. **Alta de evento válido (2027-01-10 a 2027-01-12, costo 100, 1 Laptop + 1 Technician):** `add_event` devolvió `True`; presupuesto `49900 → 49800`; Laptop `8 → 7`. ✅
2. **Baja de ese evento:** `delete_event` devolvió `True`; presupuesto restaurado a `49900`; Laptop restaurado a `8`. El round-trip de presupuesto e inventario es correcto. ✅
3. **Formato de fecha inválido** (`is_time_valid("bad","worse")`): devuelve `"❌ Invalid format. Use YYYY-MM-DD"` sin reventar. ✅
4. **Fecha pasada** (`2020-01-01`): lanza `"❌ Invalid dates. Please use a valid date"`. ✅
5. **`init > end`** (`2027-05-10` → `2027-05-01`): lanza la misma excepción de fecha inválida. ✅
6. **Costo mayor al presupuesto** (999999): `add_event` lanza `"The agency's budget is not enough"`. ✅
7. **Solapamiento de fechas:** al agendar un evento que solapa a uno existente futuro, `add_event` devolvió `(False, [('2027-08-06','2027-08-07')])` — **sugiere huecos libres reales**, como promete el README. ✅
8. **Cantidad insuficiente de recurso** (pedir 5 Camera con cant=1): lanza `"Not enough Camera available to occupy 5..."`. ✅
9. **Violación de restricción** (Projector sin Conference Room A): `Restrictions.validate` lanza `"Projector requires Conference Room A"` **antes** de tocar el inventario. ✅
10. **Rollback atómico verificado:** en las fallas de ocupación, el inventario queda intacto — comprobé que Laptop/Camera no se descontaron cuando la operación falló. La lógica de `gestor/add_event.py:33-51` (validar todo primero, ocupar después, revertir lo ya ocupado si algo falla) **funciona de verdad**. Este es el punto fuerte de corrección del proyecto.
11. **Evento duplicado** (mismo dict): lanza `"Ummm 🤔 This event is already registered in the system."` (`session.py:76`). ✅
12. **Gestión de operadores:** `add_operator`/`remove_operator` funcionan; duplicado lanza `"Este correo ya tiene acceso como operador."`; un operador (rol no-admin) que intenta agregar operadores recibe `"Solo el administrador puede agregar operadores."` (`session.py:53-54`). ✅
13. **Login de operador puro** (correo sin cuenta propia, presente en `team_access` de Isabela): inicia sesión con `role="operador"` y `owner_correo` apuntando al dueño, viendo los datos del dueño. El rol se respeta y la UI oculta Inventario/Reportes a no-admins (`reportes.py:107-114`). ✅

**Bug real encontrado (menor, no rompe la app):**

14. **Desajuste de mensaje en el registro.** En `visual/signup.py:137` el guard comprueba la cadena inglesa `"already exists"`, pero `handle_signup` (`visual/logic_buttons.py:54`) devuelve el mensaje en español `"ℹ️ Cuenta ya existente. Inicia sesion"`. Consecuencia verificada ejecutando los handlers: si te registras con un correo **ya existente**, la condición de "cuenta existente" nunca se cumple, así que el flujo cae en `elif session:` (`signup.py:141`) y **te loguea directamente en esa cuenta existente** (con su presupuesto original, no el que acabas de teclear). No hay pérdida de datos (la cuenta no se sobrescribe: confirmé que el dinero seguía siendo 49900, no el 5000 del formulario), pero el aviso de "esta cuenta ya existe, inicia sesión" que promete el `report.md:87-88` nunca aparece. Arreglo: comparar contra la cadena que realmente devuelve `handle_signup` (o usar un flag/código en vez de comparar texto).

**Fallos del entorno (no del código):** ninguno. Ambos puntos de entrada de Streamlit arrancan limpios en headless.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Bien:** uso idiomático de comprehensions (`session.py:195-196`, `add_event.py:21-26`), `dict` como índice de inventario (`resources.py:14`), `@property` para exponer `data` (`session.py:44-46`), `TYPE_CHECKING` para evitar imports circulares (`add_event.py:2-3`) — un detalle avanzado bien resuelto.
- **Mejorable:** las excepciones son todas `Exception` genéricas; excepciones propias (`PresupuestoInsuficienteError`, etc.) comunicarían mejor la intención y permitirían al llamador distinguir casos sin comparar texto (que es justo lo que causó el bug #14).
- **Mejorable:** `Gestor_json.save_data` imprime `"Your data is saved"` con `print` en cada guardado (`gestor_json.py:36`) — ruido de depuración que debería eliminarse o pasar a logging.
- **Menor:** import duplicado de `os` en `logic_buttons.py:1` y `:4`.
- **Menor:** `type` se usa como nombre de parámetro (`resources.py:2`, `:73`), sombreando el builtin `type`. Sin efecto aquí, pero mejor evitarlo.

## Dimensión 5 — Datos y persistencia

- Modelo por cuenta: un JSON por correo en `data/`, con claves `money`, `resources`, `employees`, `co_requisites`, `exclusions`, `events`, `team_access` (`gestor_json.py:19-30`).
- Recursos/empleados se serializan como listas posicionales `[name, type, cant, dispo]` y `[rol, cant, dispo]` (`session.py:195-201`). Funciona, pero un dict con claves nombradas sería más legible y menos frágil ante cambios de orden.
- **Sincronización explícita memoria↔disco** bien pensada: `sync_resources_to_json`/`sync_employees_to_json`/`sync_restrictions_to_json` (`session.py:194-207`) reescriben el JSON tras cada mutación. La estudiante identificó correctamente en el informe que esto no se sincroniza solo.
- **Detalle a vigilar:** algunos eventos de ejemplo guardan recursos con `type=null` (ej. "Whiteboard" en el JSON de Isabela) y un recurso "Whiteboard" que no está en el inventario. La lógica de `add_event.py:21-23` lo tolera con el filtro `if name in session.rc_mg.recursos`, así que no revienta, pero indica que datos históricos pueden quedar desalineados con el inventario actual.

## Dimensión 6 — Informe (`report.md`)

El informe es **honesto y coincide muy bien con el código**, sin exageraciones importantes:

- La arquitectura en capas descrita (`report.md:64-74`) corresponde exactamente a `classes/`. ✅
- El punto clave que el informe destaca — **"primero valida todo sin tocar el inventario, y solo si todo pasa ocupa recursos"** (`report.md:76`, `:176`) — es **cierto y verificado en ejecución** (test 10 arriba, `gestor/add_event.py:33-51`). No exagera: el rollback atómico funciona de verdad.
- La sugerencia de huecos ante fechas ocupadas (`report.md:12`, `:111`) también es real (test 7).

**Única discrepancia:** `report.md:87-88` afirma que al registrarse "El sistema valida que el correo no esté registrado previamente. Si ya existe, invita a iniciar sesión." En la práctica, por el bug de `signup.py:137` (mensaje en inglés vs. español), ese aviso **no se dispara**: el usuario es logueado directamente. La intención está codificada, pero el desajuste de cadena la desactiva. Vale corregir el informe o el código para que coincidan.

El README repite el comando de arranque como `cd visual && streamlit run visual.py`, mientras que `main.py` sugiere `streamlit run main.py`. Verifiqué que **ambos funcionan**, pero conviene unificar para no confundir.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y ambicioso**, claramente por encima de lo típico en primer año. La estudiante no solo construyó una app web completa con Streamlit (autenticación, roles, calendario, reportes, restricciones), sino que la sostuvo sobre una arquitectura en capas real: la lógica de negocio está tan bien separada de la interfaz que pude ejecutarla aislada y comprobar su corrección punto por punto. Y lo más importante: la promesa central del informe — validación atómica que nunca deja el inventario inconsistente — **es verdad y la verifiqué ejecutándola**, incluyendo el rollback cuando una operación falla a mitad de camino. Encontré un solo bug real (menor, sin pérdida de datos: un desajuste de cadena en el registro que desactiva un aviso), y varios detalles de estilo menores. El informe es honesto y casi enteramente fiel al código.

- **Principal fortaleza:** la separación limpia lógica/GUI y la validación atómica con rollback en `add_event` — diseño de software real, no solo un programa que "funciona", verificado en ejecución.
- **Principal área de mejora:** dar a `add_event` un contrato de retorno uniforme y sustituir las comparaciones de mensajes de texto (que causaron el bug del registro) por excepciones propias o códigos de resultado; de paso, unificar el idioma de los mensajes y quitar los `print` de depuración.
