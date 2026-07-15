# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #305
- **Repositorio:** https://github.com/al3x24l-source/planificador_romano
- **Estudiante:** Alex Luis Parra Espinosa
- **Grupo:** C-111
- **Descripción declarada:** Simular la planificación de batallas o reuniones en la antigua Roma.

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una **aplicación de escritorio con interfaz gráfica Tkinter**. El `main.py` instancia `PlanificadorRomanoApp` (`app.py:16` crea `tk.Tk()`), que muestra tres ventanas sucesivas: una intro animada a pantalla completa, un menú principal y la pantalla de gestión de eventos. Alimentarla con `printf` por `stdin` no serviría: la app espera clics en botones y un servidor X.

Cómo adapté la ejecución:

1. **`py_compile` de los 11 módulos** → todos compilan sin error (rc=0). No hay dependencias de terceros (Tkinter es de la biblioteca estándar; `requirements.txt` solo pide `python>=3.6`).
2. **Ejecuté la lógica de negocio de verdad, sin GUI:** `nucleo/` y `modelos/` están bien desacoplados de la vista, así que instancié `Calendario`, agregué eventos reales, verifiqué el JSON escrito en disco, recargué desde disco, y probé todo el `Buscador`. Documento valores concretos abajo.
3. **Intenté arrancar la GUI headless bajo Xvfb.** Sin `DISPLAY` falla en `app.py:16` (`tk.Tk()`) con `TclError: couldn't connect to display` — fallo de entorno, no del código. Bajo Xvfb, un `Treeview` aislado se construye bien, pero el build completo de `PantallaGestionEventos` (que además crea un `Combobox` y llama a `_actualizar_recursos_disponibles`) aborta con un `[xcb] Assertion` — una fragilidad conocida de Tk+Xvfb, tampoco del código del estudiante. Verifiqué en cambio los métodos de la pantalla (`_agregar_evento`) por su lógica interna, que solo construye un `Evento` y lo pasa al `Calendario`, ambos ejercitados directamente.

---

## Dimensión 1 — Qué hace el programa

Sistema de gestión de eventos con temática romana. Flujo real:

1. **`main.py:14-22`** configura rutas, importa `PlanificadorRomanoApp` y llama `app.iniciar()`.
2. **`app.py:157-167`** — `iniciar()` muestra la intro épica (`pantallas/intro_epica.py`: canvas a pantalla completa con "SENATVS POPVLVSQVE ROMANVS", subtítulo, lema "DIVIDE ET IMPERA" y un contador de 3s; se salta con ESC) y luego arranca `mainloop()`.
3. **`pantallas/menu_principal.py:75-84`** — menú con cinco opciones: Gestionar Eventos, Convocar Senado, Ver Estadísticas, Configuración, Salir.
4. **`pantallas/gestion_eventos.py`** — pantalla principal: formulario (nombre, fecha inicio, fecha fin, recursos), tabla `Treeview` de eventos, y botones agregar/eliminar/actualizar/limpiar. Se pueden asignar **recursos** (Legionarios, Catapultas, etc.) a cada evento desde un combobox.
5. **Estadísticas** (`app.py:244-278`) — genera un informe: total de eventos, en curso, próximos 7 días, pasados, sin recursos, recursos más usados y conteo por mes.
6. **Configuración** (`app.py:280-447`) — exportar/importar datos, limpiar todo, info del sistema, "acerca de".
7. Al salir (`app.py:449-467`) guarda eventos y recursos en JSON.

La ambición va **mucho más allá** de la descripción declarada: hay persistencia real en disco, búsqueda multi-criterio, gestión de recursos con disponibilidad, y estadísticas. Es un proyecto notablemente completo para primer año.

## Dimensión 2 — Organización del código

**Fortaleza destacada.** La arquitectura por paquetes es lo mejor del proyecto:

- `modelos/` — `Evento` (`evento.py:2`), `Recurso` (`recurso.py:5`), `Restricciones` (`restricciones.py:1`).
- `nucleo/` — `Calendario` (`calendario.py:8`, orquestador), `Persistencia` (`persistencia.py:6`, I/O de JSON), `Buscador` (`buscador.py:4`, filtros y estadísticas, todo `@staticmethod`).
- `pantallas/` — una clase por vista.
- `app.py` — controlador que inyecta dependencias a las pantallas.

La separación **modelo / lógica / vista** es real y limpia. `Calendario` delega correctamente a `Persistencia` y `Buscador` (`calendario.py:56-118`). El `Buscador` no toca la GUI en absoluto — por eso pude testearlo entero sin display. Nombres en español claros y consistentes, docstrings en casi todos los métodos. Este nivel de modularidad es superior al típico de primer año.

**Debilidades:**

- **`modelos/restricciones.py` está huérfano.** La clase `Restricciones` (co-requisitos y exclusiones entre recursos) está bien escrita y funciona (la probé: lanza el `ValueError` esperado), pero **no se importa ni se usa en ninguna parte** (`grep` confirma cero referencias fuera del propio archivo). Es una feature construida y luego olvidada de cablear.
- **Bloques `try/except ImportError` con clases-fallback en `app.py:27-109`.** El intento de "cargar módulos con respaldo" duplica definiciones de `Evento` y `Calendario` dentro de `_cargar_modulos`. En un proyecto con estructura fija esto es complejidad innecesaria: si un import falla, es un bug que conviene ver, no enmascarar.
- **`Evento` tiene `agregar_recurso`/`quitar_recurso` (`evento.py:25-37`) pero la GUI no los usa** — recolecta recursos en una `Listbox` y los pasa al constructor. Otra pequeña duplicación de responsabilidad.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Lo que **corrí** y observé (lógica de negocio, datos reales):

1. **Agregar eventos válidos + persistencia** → creé 3 eventos ("Batalla de Cannas" con recursos, "Sesion del Senado", "Triunfo de Cesar"). El `eventos.json` en disco quedó correcto:
   `[{"nombre":"Batalla de Cannas","inicio":"01/08/2024","fin":"03/08/2024","recursos":["Legionarios","Catapultas"]}, ...]`. ✅
2. **Recargar desde disco** → un `Calendario` nuevo cargó los 3 eventos con nombres intactos. ✅ La persistencia (ida y vuelta) funciona.
3. **Buscador** → `buscar_por_nombre("senado")` → `['Sesion del Senado']`; `buscar_por_recurso("legion")` → `['Batalla de Cannas','Triunfo de Cesar']`; `buscar_por_fecha("02/08/2024")` → `['Batalla de Cannas']` (correcto: cae dentro del rango inicio–fin); `estadisticas_recursos()` → `{'Legionarios':2,'Catapultas':1}`; `conteo_por_mes()` → `{'2024-08':1,'2024-03':1,'2026-07':1}`. Todo ✅.
4. **`generar_informe()`** → devolvió el dict completo con `total_eventos:3`, `eventos_pasados:2`, `eventos_proximos_7_dias:1`, `eventos_sin_recursos:1`. ✅
5. **Eliminar** → `eliminar_evento("Sesion del Senado")` dejó `['Batalla de Cannas','Triunfo de Cesar']` y reescribió el JSON. ✅
6. **`agregar_evento` con un no-`Evento`** → lanzó el `TypeError("Solo se pueden agregar objetos Evento")` esperado (`calendario.py:26`). ✅ Buena defensa de tipos.
7. **Fechas basura por rango** → `buscar_por_rango_fechas(..., "xx","yy")` imprimió el aviso y devolvió `[]` sin `Traceback`. ✅ El `Buscador` es defensivo: los métodos con fechas atrapan `ValueError` por evento y lo saltan.

Bugs y huecos encontrados al ejecutar:

- **[BUG confirmado] `_nuevo_recurso_dialogo` va a reventar** (`gestion_eventos.py:393`): llama `messagebox.askstring(...)`, pero **ese método no existe** en `tkinter.messagebox` (verificado: `AttributeError: module 'tkinter.messagebox' has no attribute 'askstring'`). El correcto es `simpledialog.askstring`. El botón **"+ Nuevo Recurso" lanzará `AttributeError`** al pulsarse. Como el `except Exception` de `app.py`/`_nuevo_recurso_dialogo` está en el *caller* de nivel superior, en la práctica el `except` genérico del propio método (`gestion_eventos.py:415`) lo captura y muestra un `messagebox.showerror` — o sea, el botón "no hace nada útil" pero no tumba la app. Aun así, la feature está rota.
- **[Hueco] No hay validación de fechas en ningún nivel.** El README promete validación de formato `DD/MM/AAAA` y rango `fin ≥ inicio`, pero `_agregar_evento` (`gestion_eventos.py:444-499`) solo comprueba que los tres campos no estén vacíos. Lo verifiqué directamente: un `Evento("...", "2024-03-15", "no-es-fecha")` se acepta y se guarda tal cual; y un evento con `inicio="10/05/2024"`, `fin="01/05/2024"` (fin antes de inicio) también se acepta sin queja. La app **no revienta** (el `Buscador` ignora fechas rotas), pero acepta datos inválidos que luego quedan invisibles en las búsquedas y estadísticas. Es la brecha funcional más importante frente a lo que el informe afirma.
- **[Menor] Ruta de datos inconsistente en `Recurso`.** `Recurso.agregar_recurso` (`recurso.py:78`) llama `guardar_estado()` sin argumento → usa el default relativo `"datos"`, no el `directorio_datos` absoluto que la app calcula en `app.py:13`. En ejecución normal la app inicializa recursos vía `Recurso.inicializar(self.directorio_datos)`, pero los guardados automáticos de `agregar_recurso`/`marcar_como_usado`/`liberar_recurso` escriben en `./datos` relativo al *cwd*. Según desde dónde se lance `main.py`, recursos y eventos pueden acabar en carpetas distintas.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Bien:** docstrings consistentes, nombres descriptivos en español, uso correcto de `@staticmethod`/`@classmethod`, `with open(...)` para archivos, `encoding='utf-8'` y `ensure_ascii=False` (los emojis y tildes se guardan bien), `datetime.strptime` para parseo.
- **Mejorable:**
  - **`except` genéricos silenciosos** — hay varios `except:` desnudos (`gestion_eventos.py:582`, `intro_epica.py:117`, `menu_principal.py:183`) y `except Exception` que atrapan todo. Ocultan bugs; conviene atrapar el tipo específico.
  - **Estado global mutable en `Recurso`** — `recursos_disponibles`/`recursos_usados` son atributos de clase compartidos (`recurso.py:8-9`). Funciona porque hay una sola instancia lógica, pero es frágil.
  - **`Recurso.__str__` (`recurso.py:105-106`) referencia `self.nombre`**, que no existe (la clase nunca se instancia con `nombre`). Método muerto.
  - **Emojis en `print` de diagnóstico** — inofensivo, pero mezcla logging y presentación.

Nada de esto es grave para primer año; son detalles de estilo.

## Dimensión 5 — Datos y persistencia

**Sólido.** Modelo de datos claro: `Evento` = `{nombre, inicio, fin, recursos[]}`, serializado a `eventos.json`; recursos a `recursos.json` con `{disponibles, usados}`. `Persistencia` (`persistencia.py`) encapsula bien: guardar/cargar con manejo de `JSONDecodeError` (archivo corrupto → lista vacía, sin crash — lo confirmé), export/import con `shutil.copy2`, limpiar, y estadísticas de archivos. El guardado es automático tras cada mutación (`calendario.py:32,41`). El *round-trip* disco→memoria→disco lo probé y es correcto. Único pero: la inconsistencia de ruta de `Recurso` (Dim. 3) puede desincronizar dónde viven los dos JSON.

## Dimensión 6 — Informe (`README.md`)

El README es extenso y bien presentado, pero **sobreestima la validación** y describe una arquitectura ligeramente distinta de la real:

- **Afirma** "Validaciones exhaustivas", "Validar formato de fechas (DD/MM/AAAA)", "Rango temporal válido (fin ≥ inicio)" y hasta una tabla de "Evento Inválido" con ejemplos que "dan Error". **En el código no existe validación de fechas alguna** — lo verifiqué ejecutando: entradas como `2024-03-15` o `fin` anterior a `inicio` se aceptan sin error. Esta es la discrepancia principal.
- **Afirma** "Manejo de errores robusto" y "Recuperación ante excepciones" — parcialmente cierto (el `Buscador` y `Persistencia` sí son defensivos), pero el botón "Nuevo Recurso" está roto por el bug de `askstring`.
- **Lista** en la estructura solo `evento.py` y `calendario.py`, omitiendo `recurso.py`, `restricciones.py`, `persistencia.py` y `buscador.py` — el proyecto real es **más grande** que lo que el README documenta. Curiosamente, aquí el informe se queda *corto*, no exagera.
- **Menciona** "JSON: (Planeado) Persistencia de datos" como futuro — pero la persistencia JSON **ya está implementada y funciona**. El README está desactualizado respecto al propio código.

---

## Valoración global (orientativa, sin nota numérica)

Un proyecto **ambicioso y bien estructurado** para primer año. La verdadera fortaleza está bajo la superficie: una separación modelo/lógica/vista real, con un `nucleo` (`Calendario` + `Persistencia` + `Buscador`) que funciona de punta a punta y que pude ejercitar entero sin tocar la GUI — persistencia JSON correcta, búsqueda multi-criterio, estadísticas y gestión de recursos, todo verificado con datos reales. Eso demuestra un entendimiento de POO y de diseño modular por encima de lo esperado. Los defectos son concretos y arreglables: falta la validación de fechas que el propio informe promete, hay un bug en el botón "Nuevo Recurso" (`messagebox.askstring` → debe ser `simpledialog.askstring`), un módulo (`Restricciones`) construido pero nunca cableado, y una inconsistencia de ruta en el guardado de recursos. Ninguno tumba la aplicación; sí dejan huecos entre lo que el README afirma y lo que el código hace.

- **Principal fortaleza:** arquitectura modular real (modelos / núcleo / pantallas) con una capa de lógica de negocio limpia, testeable y funcional, muy por encima del nivel típico de primer año.
- **Principal área de mejora:** implementar la validación de fechas (formato `DD/MM/AAAA` y `fin ≥ inicio`) que el informe promete pero el código no tiene, y arreglar el bug `askstring` del botón "Nuevo Recurso".
