# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #272
- **Repositorio:** https://github.com/brianRaul/gestor-eventos-espaciales
- **Estudiante:** Brian Raúl López Pérez
- **Grupo:** C122
- **Descripción declarada:** Orquestación y logística de misiones aeroespaciales: un "motor de simulación de recursos" para gestionar desde lanzamientos orbitales hasta pruebas de propulsión crítica en un entorno de inventario limitado.

> Nota administrativa: el issue #272 apunta al mismo repositorio, estudiante y descripción que el issue #269 (ver `269-gestor-eventos-espaciales.md`). Es una doble apertura de issue para la misma entrega. Este reporte reevalúa la entrega de forma independiente ejecutando el código.

---

## Nota metodológica importante

**No es una aplicación de consola.** Es una GUI de escritorio construida con **CustomTkinter** (`main.py:1`, clase `GestorEventos(ctk.CTk)` en `main.py:19`). No se puede alimentar con `printf` recorriendo un menú.

Adaptación de la ejecución:
- El entorno no tiene servidor X ni `xvfb-run`, por lo que no fue posible instanciar la ventana (`ctk.CTk()` requiere display). Esto es una limitación **del entorno de evaluación**, no un defecto del código.
- El estudiante separó muy bien la lógica de negocio de la GUI (todo en `modulos/`), lo que permitió ejecutar **directamente la lógica real** de forma headless, con los datos reales del repo (`recursos.json`, `eventos_predeterminados.json`), sin tocar `main.py`.
- Se corrieron 19 escenarios (T1–T19) contra `procesar_creacion_evento`, `sugerir_fecha_disponible_logica`, `procesar_eliminacion_eventos`, `rellenar_combustible_logica`, `validar_datos_serie` y `crear_serie_recurrente`, cubriendo flujos válidos e inválidos.
- `py_compile` de **los 13 módulos + `main.py`** pasó sin errores.

---

## Dimensión 1 — Qué hace el programa

Es un planificador de eventos aeroespaciales con gestión de inventario limitado. El flujo principal (`main.py:231 crear_evento`) es:

1. El usuario elige un tipo de evento en un ComboBox poblado desde `eventos_predeterminados.json` (`main.py:889`). Hay **15 tipos** de evento definidos (Despegue de cohete, Prueba estática de motor, etc.).
2. Marca recursos (checkboxes agrupados por categoría, `main.py:134 crear_checkboxes_recursos`) o pulsa "Marcar Recomendados" (`main.py:184`), que autoselecciona los requeridos vía `lr.obtener_recursos_recomendados` (`logica_recursos.py:579`).
3. Introduce fecha (día/mes/año) y duración.
4. La creación se delega íntegra a `procesar_creacion_evento` (`funciones_crear_evento.py:242`), que valida tipo, fecha/duración, existencia de recursos, reglas de exclusión, requisitos coexistentes, disponibilidad (solapamiento de equipos + stock de combustible) y luego **consume** los recursos.
5. El evento se persiste en `eventos_planificados.json` y la UI se sincroniza (`main.py:288`).

Funciones adicionales verificadas: sugerencia de próxima fecha libre (`logica_fechas.py:sugerir_fecha_disponible_logica`), series recurrentes (`funciones_series_recurrentes.py`), eliminación con devolución de recursos (`logica_eliminacion.py`), y gestión de combustible como consumible con relleno de tanques (`logica_combustible.py`).

La descripción declarada ("motor de simulación de recursos con inventario limitado") **coincide con lo que hace el programa**. No es marketing vacío: el consumo/devolución de combustible y el conteo de equipos ocupados por solapamiento están realmente implementados y funcionan.

## Dimensión 2 — Organización del código

Esta es la mayor fortaleza del proyecto. La separación GUI/lógica es **ejemplar para primer año**:

- `main.py` (1185 líneas) contiene solo la clase de la ventana: construcción de widgets y handlers que **delegan** a los módulos. No hay lógica de negocio incrustada en los callbacks (p. ej. `crear_evento` en `main.py:231` no valida nada por sí mismo; llama a `procesar_creacion_evento`).
- 13 módulos en `modulos/`, cada uno con una responsabilidad clara y bien nombrada: `logica_validaciones.py`, `logica_combustible.py`, `funciones_buscar_hueco.py`, `logica_eliminacion.py`, `logica_sincronizacion.py`, etc.
- La configuración vive en datos, no en código: los 15 tipos de evento con sus reglas (`recursos_requeridos`, `reglas_exclusion`, `requisitos_coexistentes`, `configuracion_evento`) están en `eventos_predeterminados.json`. Añadir un evento nuevo no requiere tocar Python. Esto es una decisión de diseño madura.
- Total ~3881 líneas de Python distribuidas con sensatez.
- Se introdujeron índices (`eventos_por_fecha`, `recursos_por_clave` en `main.py:52` y `main.py:859`) para acelerar las búsquedas de disponibilidad, con *fallback* a búsqueda lineal cuando el índice no está presente (`funciones_buscar_hueco.py:815`). Detalle de ingeniería poco común a este nivel.

Debilidades menores:
- **Imports con wildcard**: `from modulos.funciones_datos import *` (`main.py:5`, `main.py:6`, `main.py:7`; también `funciones_series_recurrentes.py:3-4`). Funcionan, pero ocultan de dónde viene cada nombre y pueden colisionar. Preferible importar explícito.
- Algunas funciones son largas (`eliminar_eventos_planificados`, `main.py:463`, mezcla construcción de UI con lógica de armado de la ventana); es aceptable para una GUI, pero podría extraerse la parte de datos.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Se ejecutó la lógica de negocio con datos reales del repo. Todo lo listado abajo es **observado**, no leído:

1. **`py_compile`** de `main.py` + los 13 módulos: **sin errores**.
2. **T1 — Crear "Despegue de cohete" válido**: `procesar_creacion_evento` devolvió `True`, evento `14/08/2026 → 15/08/2026`, `resumen_recursos` correcto con 11 recursos incluyendo `SISTEMA DE COMBUSTIBLE|Liquido: 45000`. El tanque de combustible líquido bajó de 600000 a **555000L** (consumió exactamente 45000). Correcto.
3. **T2 — Faltando un recurso requerido**: rechazado con mensaje del validador de coexistencia (`'COHETE Pesado' requiere también 'SISTEMA DE SEGURIDAD Activa'`). Correcto.
4. **T3 — Fecha inválida `32/13/2026`**: rechazado, `❌ Fecha o duración inválida`. No revienta (el `ValueError` se captura en `logica_fechas.py:validar_fecha_basica`).
5. **T4 — Fecha en el pasado**: rechazado, `❌ No puedes planificar en el pasado`.
6. **T5 — Duración basura `"abc"`**: rechazado, `❌ Fecha o duración inválida`.
7. **T6 — Duración > máxima (99, máx 3)**: rechazado, `❌ Duración permitida: 1-3 días`. La duración min/max se lee del propio JSON del evento (`logica_fechas.py`).
8. **T7 — Campos de fecha vacíos**: rechazado, `❌ Fecha o duración inválida`. Sin Traceback.
9. **T8 — Tipo por defecto ("Elige un tipo de evento")**: rechazado, `❌ Selecciona un tipo de evento`.
10. **T9 — Regla de exclusión (cohete Ligero en Despegue)**: rechazado, `⛔ RECURSO PROHIBIDO: 'Explorador-1 (Ligero)' no está permitido`.
11. **T10 — Combustible insuficiente (tanque a 0)**: rechazado, `❌ Combustible insuficiente: SISTEMA DE COMBUSTIBLE Liquido (faltan 45000L)`.
12. **T11 — Sugerir fecha disponible**: devolvió `14/08/2026` con mensaje de éxito.
13. **T12 — Solapamiento de equipo escaso**: se crearon 2 eventos en las mismas fechas que compiten por `SALA DE CONTROL PRINCIPAL Primaria` (total=1). El primero pasó; el segundo fue **correctamente rechazado**: `❌ Ocupado en esas fechas: SALA DE CONTROL PRINCIPAL Primaria. (Total: 1, Ocupados: 1, Necesarios: 1)`. La detección de solapamiento de inventario **funciona de verdad**.
14. **T13 — Eliminar evento**: devolvió 10 equipos al inventario y `45000L` de combustible al tanque (600000L de nuevo), con conteo de litros desperdiciados = 0. La lógica de "el exceso se pierde" (`logica_eliminacion.py:40`) está bien implementada con `min(cantidad, espacio_disponible)`.
15. **T14 — Rellenar combustible**: desde 100L, rellenó ambos tanques al máximo (`1099800L` agregados). Correcto.
16. **T15/T16/T17 — Validación de series**: serie válida (3×cada 5 días) aceptada; serie de 999 repeticiones rechazada con `❌ Serie demasiado larga (máximo 73 eventos)`; intervalo/repeticiones basura rechazados con `❌ Usa solo números válidos en todos los campos`.
17. **T18 — Serie recurrente real (`crear_serie_recurrente`)**: creó los 3 eventos con intervalos correctos (`24/08`, `29/08`, `03/09`) y consumió combustible de forma acumulada (600000 → 576000 = 3×8000L). End-to-end correcto.

**No se produjo ningún `Traceback` del código del estudiante en ninguno de los 19 escenarios.** La validación por capas hace su trabajo: las entradas basura se rechazan con mensajes específicos antes de tocar el estado.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Sólido para primer año. Puntos mejorables **menores**:

- **`except:` desnudo** en 6 sitios (`logica_fechas.py:24`, `logica_fechas.py:41`, `funciones_crear_evento.py:91`, `funciones_buscar_hueco.py:84`, `funciones_series_recurrentes.py:33`, `logica_sincronizacion.py:67`). Capturar `Exception` o el tipo concreto (`ValueError`) es más seguro: un `except:` desnudo se traga también `KeyboardInterrupt`. En la práctica no causó fallos, pero es un hábito a corregir.
- **`print()` de depuración** que llegan a stdout en producción (`funciones_datos.py`: "✅ Se cargaron 30 recursos", `main.py:87`, `main.py:117`). Para una GUI, estos mensajes no se ven; conviene un logger o eliminarlos.
- Nombres en general buenos y en español consistente. Los módulos con prefijo `logica_`/`funciones_` son descriptivos.
- No hay variables globales (verificado con `grep global`): el estado vive en la instancia `GestorEventos` y se pasa por parámetro a los módulos. Muy bien.

## Dimensión 5 — Datos y persistencia

Modelo de datos claro y coherente, con tres archivos JSON:

- `eventos_predeterminados.json` — "base de conocimiento": 15 tipos con reglas declarativas. Bien pensado.
- `recursos.json` — inventario por categoría; los combustibles llevan `cantidad_disponible` además de `cantidad_total`. La carga (`funciones_datos.py:cargar_recursos_desde_json`) normaliza cada recurso a un dict plano con `id`, `nombre_mostrar`, `es_combustible`, etc. — buena capa de adaptación.
- `eventos_planificados.json` — se ordena por fecha y se serializa quitando los objetos `date` no serializables (`main.py:66 guardar_eventos_en_json`, líneas 74-79). Detalle correcto: muchos principiantes olvidan que `datetime` no es JSON-serializable y aquí se maneja explícitamente.

Observación menor: `logica_serie.py` genera `fecha_str` como `"24/8/2026"` (mes sin cero a la izquierda), mientras el resto del código usa `%d/%m/%Y`. Verifiqué (T19) que `datetime.strptime("24/8/2026", "%d/%m/%Y")` **parsea sin problema** en CPython, así que no rompe nada; aun así, homogeneizar el formato con `strftime("%d/%m/%Y")` evitaría sorpresas futuras.

## Dimensión 6 — Informe (`report.md`)

Informe extenso (~2100 palabras declaradas), bien estructurado (introducción, objetivos, módulos, modelo de datos, casos de uso, decisiones de diseño, dificultades, mejoras futuras, conclusiones). Describe con precisión lo que el código hace y **no exagera funciones**: cada módulo mencionado existe y hace lo que dice.

Un punto de honestidad a señalar: en la sección 7 ("Pruebas y Validación") afirma que *"Todos los casos de prueba han sido superados satisfactoriamente, lo que demuestra la robustez del sistema"*. La palabra **"demuestra"** sobreestima: las pruebas fueron manuales y no hay suite automatizada (el propio informe lo reconoce en 7 y 9.1). Mis 19 escenarios ejecutados **respaldan** que la lógica es robusta en los caminos probados, pero "demostrar" en sentido fuerte requeriría pruebas automatizadas. El estudiante ya identifica esto correctamente como mejora futura (sección 9), lo que matiza la exageración.

Salvo ese matiz, el informe es fiel al código. No detecté features inventadas.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y ambicioso**, por encima de lo esperable en un primer año. La lógica de negocio no solo existe: **se ejecutó de verdad** y funciona correctamente en 19 escenarios que cubren creación válida, seis clases de entrada inválida, solapamiento de inventario escaso, consumo/devolución de combustible con desperdicio, y series recurrentes end-to-end. Nada reventó. La separación GUI/lógica, la configuración dirigida por datos, y los índices con fallback denotan una madurez de diseño notable a este nivel.

- **Principal fortaleza:** Arquitectura. La lógica de negocio está completamente desacoplada de la GUI, distribuida en módulos con responsabilidad única, y dirigida por datos (`eventos_predeterminados.json`). Esto es lo que permitió ejecutarla headless y comprobar que las reglas de exclusión, coexistencia, solapamiento y combustible funcionan realmente.
- **Principal área de mejora:** Higiene de Python de detalle — reemplazar los 6 `except:` desnudos por captura de excepción específica, eliminar los `print()` de depuración, y sustituir los `import *` por imports explícitos. Y, como el propio informe reconoce, añadir una suite `pytest` que convierta las pruebas manuales en verificación reproducible (los 19 escenarios de este review son un buen punto de partida).
