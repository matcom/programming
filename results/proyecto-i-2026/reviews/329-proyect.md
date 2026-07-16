# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #329
- **Repositorio:** https://github.com/kapibaradeioi/proyect
- **Estudiante:** Liz Rachel Perez Almaguer
- **Grupo:** C111
- **Descripción declarada:** Un lugar para atender a tus mascotas y agendar servicios de cuidado y limpieza.

---

## Nota metodológica importante

**No es una aplicación de consola: es una GUI de Kivy.** La navegación se
construye con un `ScreenManager` (`main.py:40-48`) y seis pantallas definidas en
archivos `.kv`. No tiene `input()` ni bucle de menú por terminal, así que no se
puede alimentar con `printf`.

Adapté la ejecución en dos frentes:

1. **Lógica de negocio aislada.** El proyecto tiene una separación limpia entre
   `interfaz/` (pantallas Kivy) y `logica/` (funciones puras que manejan JSON,
   inventario, fechas y colisiones). Escribí un driver que ejercita directamente
   `logica/` con los datos reales del repo (`jsons/fijos de eventos/eventos.json`,
   `jsons/inventario/inventario.json`): crear cita, seleccionar eventos, decrementar
   inventario, programar acciones, detectar colisiones, buscar huecos, y flujos
   inválidos (JSON corrupto, evento inexistente).
2. **GUI headless real.** Arranqué `main.py` bajo `SDL_VIDEODRIVER=offscreen`
   con OpenGL de la GPU. La app **construyó el primer frame sin excepción**,
   registró las 6 pantallas y persistió una cita. Incluso conduje el flujo
   `Crear Cita → registrar()` a través de las pantallas reales (válido e inválido).
   Los únicos mensajes fueron `ERROR` de imágenes/fuentes que yo no descargué (el
   repo pesa ~16 MB en assets y clonarlo completo fallaba por la red); esos
   archivos **sí existen en el repositorio**, no son un fallo del código.

Conclusión metodológica: **el proyecto ejecuta correctamente**. Todo lo que
observé de "corrección" está basado en corridas reales, no en lectura.

## Dimensión 1 — Qué hace el programa

Es un gestor de citas de peluquería/cuidado de mascotas. El flujo, verificado
ejecutando:

1. **Crear Cita** (`interfaz/crear_cita.py:22`): pide nombre, mascota y teléfono;
   valida (campos no vacíos, teléfono de 8 dígitos) y guarda la cita en un JSON de
   sesión, luego navega a la pantalla de eventos.
2. **Eventos** (`interfaz/eventos.py`): lista servicios predefinidos (Bañar, Peinar,
   Corte de uñas, Cortar pelo, etc.) con su duración e insumos. Al activar un
   evento valida que haya inventario suficiente (`_validar_disponibilidad`,
   `eventos.py:281`) y, al confirmar, descuenta insumos del inventario
   (`_aplicar_eventos_pendientes`, `eventos.py:317`).
3. **Registro/tiempo** (`interfaz/registro.py`): para cada evento seleccionado abre
   un calendario navegable (`_mostrar_popup_fecha`, `registro.py:151`), pide hora
   (rango 8:00 AM–8:00 PM), valida colisiones y guarda la programación. También
   ofrece "buscar cupo" automático (`buscar_cupo_evento`, `registro.py:377`).
4. **Ver citas** (`interfaz/ver_citas.py`): construye tarjetas ordenadas por estado
   (próximo / en curso / finalizado) con tiempo restante calculado en vivo.
5. **Cargar citas** (`interfaz/cargar_citas.py`): lista los JSON de sesión previos y
   permite reactivar uno como sesión actual.

Corrida real del ciclo completo (driver de lógica):
- `guardar_cita("Ana","Firulais","55512345") -> True`, JSON escrito correctamente.
- Activar "Bañar" descontó inventario: `Shampoo 3→2, Jabon 31→30, Esponja 27→26,
  Guantes 8→7, Toalla 54→53, Secador 44→43`. Correcto.

## Dimensión 2 — Organización del código

**Ésta es la principal fortaleza del proyecto.** La arquitectura está por encima
de lo típico en primer año:

- **Separación interfaz/lógica real y consistente.** Cada pantalla Kivy
  (`interfaz/*.py` + su `.kv`) delega en un módulo de lógica pura (`logica/*.py`).
  Pude ejecutar toda la lógica sin instanciar un solo widget — señal de que la
  separación no es cosmética.
- **Nombres claros y en español coherente**: `buscar_hueco_mas_cercano`,
  `validar_colision_accion`, `actualizar_inventario_por_evento`.
- **Docstrings en todos los módulos y casi todas las funciones.** Nivel de
  documentación notable.
- **Normalización defensiva de datos**: casi toda comparación de nombres pasa por
  `" ".join(str(x).strip().lower().split())` para tolerar mayúsculas/espacios
  (`eventos_logica.py:46`, `tiempo_eventos.py:132`). Muy maduro.
- **Datos externos, no hardcode**: eventos, inventario y colisiones viven en JSON.

Debilidades:
- **Función duplicada literal.** `validar_acciones_programadas` está definida dos
  veces, idéntica (`tiempo_eventos.py:339` y `:372`). La segunda sombrea a la
  primera; las líneas 339-369 son código muerto. Casi seguro un copy-paste
  accidental. No cambia el comportamiento, pero debe eliminarse.
- **Dos globales `RUTA_JSON` para el mismo concepto** (`crear_cita_logica.py:16` y
  `tiempo_eventos.py:46`). Ver Dimensión 3, hallazgo #2 — genera una inconsistencia
  real de estado.
- `_nombre_display` (`eventos.py:313`) devuelve la clave normalizada tal cual, así
  que los mensajes de error muestran nombres en minúscula ("faltan: shampoo").
  Menor.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí un driver sobre `logica/` con los JSON del repo y conduje además la GUI
headless. Observaciones numeradas:

1. **Ciclo completo correcto.** Crear cita → seleccionar evento → descontar
   inventario → programar → detectar colisión → buscar hueco: todo funciona sin
   `Traceback`. Los valores del inventario cuadran exactamente (ver Dim. 1).

2. **BUG de consistencia de estado (real).** `cargar_citas` invoca
   `seleccionar_json_actual` (`cargar_citas_logica.py:46`), que fija
   `crear_cita_logica.RUTA_JSON` pero **no** `tiempo_eventos.RUTA_JSON`. Comprobado
   ejecutando: tras cargar un JSON viejo,
   `crear_cita_logica.RUTA_JSON -> citas_2020-...json` pero
   `tiempo_eventos.RUTA_JSON -> citas_2026-...json` (la sesión anterior). Efecto: al
   cargar una cita guardada, "Ver citas" muestra la cargada, pero si el usuario va a
   "Registro" a reprogramar, `tiempo_eventos` sigue operando sobre la sesión
   equivocada. Es el defecto funcional más serio.

3. **Detección de colisiones correcta y sutil.** Verificado:
   - "Cortar pelo" a las 9:10 con "Bañar" 9:00-9:30 (excluyentes + solape)
     → `(False, "No se puede hacer esto en este horario.")` ✓
   - "Cortar pelo" a las 10:00 (excluyente pero sin solape) → `(True, "")` ✓
   - Decisión de diseño observada: la colisión sólo se evalúa entre eventos
     **mutuamente excluyentes** (definidos en `eventos.json` con `"colision"`). Dos
     eventos compatibles (p.ej. Bañar + Peinar) **sí** pueden programarse en el
     mismo horario solapado (verificado: `(True, "")`). Es defendible para
     peluquería (un mismo insumo no, pero tareas compatibles sí), aunque conviene
     que sea intencional y no un olvido.

4. **`buscar_hueco_mas_cercano` correcto.** Para "Cortar pelo" (60 min) con "Bañar"
   9:00-9:30 y referencia 8:00 devolvió `08:00 → 09:00` (cabe antes del baño). ✓
   Evento inexistente → `(None, None, None)`. ✓

5. **Robustez ante datos malos.** JSON corrupto → `obtener_citas_actuales()`
   devuelve `[]` sin reventar (`ver_citas_logica.py:26`, `tiempo_eventos.py:77`).
   `format_duracion("x")` y `format_duracion(0)` → `""`. Evento sin duración → `None`
   manejado. Muy bien cubierto.

6. **GUI arranca de verdad.** `main.py` bajo `SDL_VIDEODRIVER=offscreen` construyó
   el primer frame (EXIT=0), registró las 6 pantallas y, conducido programáticamente:
   - campos vacíos → popup de error, no navega ✓
   - teléfono "123" → popup de error, se queda en `crear_cita` ✓
   - datos válidos → navega a `eventos` y persiste el JSON ✓

7. **`py_compile` de los 13 módulos: OK.** Todos compilan.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **`requirements.txt` roto (`requirements.txt:5`): `cleidna==3.11`.** El paquete no
  existe (es un typo de `idna`); con ese archivo, `pip install -r requirements.txt`
  falla en bloque. Comprobado: *"Because cleidna was not found in the package
  registry ... your requirements are unsatisfiable."* Además `Kivy-Garden` y varias
  deps (`certifi`, `docutils`, `filetype`) son transitivas/no usadas — la lista
  parece un `pip freeze` sin limpiar. La dependencia real mínima es `kivy` +
  `pillow`.
- Uso idiomático correcto de `with open(...)`, `try/except` acotados,
  comprensiones. Buen nivel.
- `except Exception:` genérico en varios sitios (`eventos_logica.py:105`,
  `tiempo_eventos.py:138`); para primer año está bien, pero conviene atrapar
  `ValueError`/`TypeError` específicos.
- `CargarCitasScreen.lista_jsons = []` (`cargar_citas.py:24`) es atributo de clase
  mutable; funciona porque se reasigna en `on_pre_enter`, pero es un patrón a evitar.
- Docstring mal ubicado: en `registrar` (`crear_cita.py:27`) el docstring está
  **después** de tres asignaciones, así que no es docstring de la función. Menor.

## Dimensión 5 — Datos y persistencia

- Modelo de datos claro y sensato: una cita es un dict
  `{nombre, mascota, telefono, eventos[], acciones_programadas[]}`; el inventario es
  `[{nombre, cantidad}]`; los eventos incluyen `duracion_min` y `colision[]`.
- **Sesión por archivo con timestamp** (`citas_YYYY-MM-DD_HH-MM-SS.json`), lo que da
  historial de sesiones cargables. Idea buena y bien implementada.
- Serialización con `ensure_ascii=False` e `indent=4` → JSON legible con acentos.
- Exclusiones bidireccionales calculadas en carga (`eventos_logica.py:68-71`):
  detalle cuidadoso.
- Única fricción: el estado de "sesión activa" vive en dos globales de módulo
  distintos (ver Dim. 3 #2), lo que rompe la consistencia al cargar.

## Dimensión 6 — Informe (`report.md`)

El informe es **honesto y bastante fiel al código**. Describe la arquitectura
real, nombra funciones que existen con la firma correcta
(`buscar_hueco_mas_cercano(evento, acciones_programadas, referencia)` coincide con
`tiempo_eventos.py:262`) y explica el flujo tal como se ejecuta.

Discrepancias menores:
- Menciona `jsons/temporales/` (`report.md:13`) como si viniera en el repo; en
  realidad esa carpeta se **crea en tiempo de ejecución** (no está versionada, sólo
  aparece al correr). No es un error, pero puede confundir a quien clone.
- El informe **no menciona** ni el `requirements.txt` roto ni el bug de
  `RUTA_JSON`. No exagera features (no dice "prueba" ni "demuestra" validación
  manual), así que no infla; simplemente omite los dos defectos reales.
- Describe la colisión sin aclarar que sólo aplica a eventos excluyentes; un lector
  podría suponer que impide todo solapamiento.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido y ambicioso**. Es una GUI de Kivy real, con varias pantallas,
calendario navegable, gestión de inventario con validación de stock, detección de
colisiones sensible a exclusiones, y búsqueda automática de huecos — todo
verificado ejecutándose de verdad, sin un solo `Traceback` en la lógica y con la
GUI arrancando y persistiendo datos. La separación interfaz/lógica, la
documentación y la normalización defensiva están claramente por encima del
promedio de primer año. Los dos defectos reales (el `requirements.txt` que no
instala y la desincronización de `RUTA_JSON` al cargar una sesión) son concretos y
arreglables en pocas líneas, y no empañan lo esencial: el sistema funciona.

- **Principal fortaleza:** arquitectura limpia con separación interfaz/lógica que
  permitió ejecutar toda la lógica de negocio de forma aislada — combinada con una
  robustez notable ante datos inválidos (JSON corrupto, campos vacíos, eventos
  inexistentes).
- **Principal área de mejora:** unificar el estado de "sesión activa" (hoy repartido
  entre `crear_cita_logica.RUTA_JSON` y `tiempo_eventos.RUTA_JSON`) para que cargar
  una cita guardada la deje coherente en todas las pantallas; y arreglar el
  `requirements.txt` (`cleidna` → `idna`, y podar deps no usadas) para que el
  proyecto instale de una sola pasada.
