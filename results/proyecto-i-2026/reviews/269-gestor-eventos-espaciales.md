# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #269
- **Repositorio:** https://github.com/brianRaul/gestor-eventos-espaciales
- **Estudiante:** Brian Raúl López Pérez
- **Grupo:** C122
- **Descripción declarada:** Orquestación y logística de misiones aeroespaciales: motor de simulación de recursos, desde lanzamientos orbitales hasta pruebas de propulsión, con inventario limitado.

---

## Nota metodológica importante

El proyecto **no es una aplicación de consola** con menú `input()`, como asume la
plantilla base de la rúbrica. Es una **aplicación de escritorio con interfaz gráfica**
construida sobre `customtkinter` (`main.py:1`, `main.py:19` — la clase
`GestorEventos` hereda de `ctk.CTk`). Por tanto no se puede recorrer con
`printf '1\n2\n...' | python main.py`. La estrategia de ejecución dinámica se
adaptó en consecuencia (ver dimensión 3): se ejecutó directamente la **lógica de
negocio** (que está limpiamente separada de la GUI) alimentándola con datos
reales, y se intentó arrancar la GUI en modo headless.

---

## Dimensión 1 — Qué hace el programa

Es un planificador de misiones aeroespaciales con simulación de inventario
limitado. El punto de entrada es `main.py:1182` (`if __name__ == "__main__"`),
que instancia `GestorEventos` y llama a `app.mainloop()`. La ventana principal
(`main.py:868` `crear_interfaz`) ofrece: seleccionar un tipo de evento de un
`ComboBox`, marcar recursos (con botón de "recomendados" automáticos), fijar
fecha y duración, y crear el evento. Funcionalidades adicionales: sugerir la
próxima fecha libre (`main.py:317`), crear **series recurrentes** de eventos
(`main.py:651`), ver/rellenar combustible (`main.py:364`, `main.py:439`),
listar eventos planificados (`main.py:311`) y eliminarlos devolviendo los
recursos al inventario (`main.py:463`).

El dominio está modelado con seriedad: 15 tipos de evento y 30 recursos cargados
desde JSON (verificado al ejecutar). Cada tipo de evento define recursos
requeridos, **reglas de exclusión** (p. ej. "Despegue de cohete" prohíbe cohete
tipo Ligero — `eventos_predeterminados.json`), **requisitos coexistentes** y
límites de duración. El combustible es un recurso consumible con cantidad
disponible que baja al crear eventos y se recupera (con desperdicio) al
eliminarlos.

El flujo principal: usuario elige evento → marca recursos → fecha/duración →
"Crear Nuevo Evento" → `crear_evento` (`main.py:231`) delega en
`procesar_creacion_evento` (`funciones_crear_evento.py:241`), que valida por
capas y, si todo es correcto, consume recursos, persiste en JSON y actualiza la
interfaz.

## Dimensión 2 — Organización del código

Este es el punto **más fuerte** del proyecto y está muy por encima del nivel
esperado en 1er año. El código está dividido en `main.py` (interfaz, 1184
líneas) y **14 módulos** en `modulos/` con responsabilidades bien delimitadas:

- `funciones_datos.py` — carga/guardado JSON.
- `funciones_crear_evento.py:241` — orquestación de creación con validación por capas.
- `funciones_buscar_hueco.py:4` — algoritmo de disponibilidad y solapamiento.
- `funciones_series_recurrentes.py` — series recurrentes.
- `logica_validaciones.py` — reglas de exclusión/coexistencia/opcionales.
- `logica_fechas.py`, `logica_combustible.py`, `logica_eliminacion.py`,
  `logica_recursos.py`, `logica_serie.py`, `logica_visualizaciones.py`,
  `logica_info_eventos.py`, `logica_sincronizacion.py`.

La **separación GUI ↔ lógica** es real y consistente: `crear_evento`
(`main.py:231`) solo lee widgets y muestra resultados; toda la decisión vive en
funciones puras que reciben datos y devuelven `(bool, mensaje, ...)`. Esto es lo
que permitió testear la lógica sin display. Los nombres son claros y en español
coherente (`validar_reglas_exclusion`, `obtener_stock_combustible`). Hay uso
correcto de clases donde el dominio lo pide: `GestorEventos` y
`GestorSincronizacion` (`logica_sincronizacion.py:8`, un patrón Observer para
mantener ventanas secundarias en sincronía). Casi no hay duplicación
estructural; sí hay algo de repetición interna (ver dimensión 4).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Qué se corrió:** entorno aislado con `uv` (`uv venv --python 3.12`,
`uv pip install customtkinter` → customtkinter 6.0.0). Como es una GUI, se
ejecutó la lógica de negocio con datos reales del repo:

1. **Carga de datos:** OK. `cargar_eventos_desde_json` → 15 tipos;
   `cargar_recursos_desde_json` → 30 recursos; `cargar_eventos_planificados` → 0
   (lista vacía). Sin errores.
2. **Recursos recomendados** (`logica_recursos.py:43`) para "Despegue de cohete":
   devuelve 11 recursos. OK.
3. **Crear evento válido** (`funciones_crear_evento.py:241`) con fecha futura y
   recursos recomendados: `resultado=True`, mensaje "🚀 Evento creado
   exitosamente", evento con 11 recursos_detalle y rango de fechas correcto.
   Además el **combustible se consumió**: `Deposito-Liquido` bajó de 600000 a
   555000 L (–45000, exactamente lo requerido). Consumo correcto.
4. **Solapamiento de recursos** (`funciones_crear_evento.py:83-105`): crear un
   segundo evento el mismo día con los mismos recursos escasos se rechaza:
   `❌ Ocupado en esas fechas: SALA DE CONTROL PRINCIPAL Primaria. (Total: 1,
   Ocupados: 1, Necesarios: 1)`. La detección de conflictos **funciona**.
5. **Entrada inválida — fecha basura** (`day="abc", month="99", year="xx"`):
   no revienta; devuelve `❌ Fecha o duración inválida`
   (`logica_fechas.py:161` captura `ValueError`). Validación robusta.
6. **Entrada inválida — sin tipo de evento**: devuelve
   `❌ Selecciona un tipo de evento` (`logica_validaciones.py:5`). OK.
7. **Compilación** (`py_compile` de `main.py` + los 14 módulos): **todos
   compilan**, sin errores de sintaxis.
8. **Arranque de la GUI** (`python main.py` headless): aborta con error de X11
   (`[xcb] Aborting`, assertion `xcb_xlib_unknown_seq_number`). Esto **no es un
   bug del estudiante** — es la ausencia de un display X funcional en el entorno
   de evaluación. El código GUI en sí compila e importa correctamente.

**Conclusión de corrección:** la lógica hace exactamente lo que dice el
issue/informe. No se observó ningún `Traceback` atribuible a un bug del código
del estudiante en ninguno de los flujos ejercitados. La única excepción
observada (xcb) es del entorno headless, no del programa.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Muy sólido para 1er año:

- **Legibilidad e indentación:** consistentes en todo el repo. f-strings usados
  idiomáticamente. Comentarios abundantes y con secciones numeradas.
- **Manejo de errores:** `try/except` donde corresponde — carga de JSON con
  `FileNotFoundError`/`json.JSONDecodeError` (`funciones_datos.py:82`),
  conversión de fechas (`logica_fechas.py:161`), guardado
  (`main.py:89`).
- **Evita variables globales:** el estado vive en `self` dentro de
  `GestorEventos`; las funciones de lógica son puras y reciben todo por
  parámetro. Excelente disciplina.
- **Uso de índices** (`eventos_por_fecha`, `recursos_por_clave` —
  `main.py:832`, `main.py:859`) para acelerar búsquedas: iniciativa notable.

Puntos mejorables (menores para el nivel):

- **`except:` desnudos** en varios sitios (`funciones_crear_evento.py:91`,
  `funciones_buscar_hueco.py:84`, `logica_fechas.py:36-41`,
  `logica_sincronizacion.py:67`). Funcionan, pero atrapan cualquier cosa
  (incluido `KeyboardInterrupt`). Mejor `except (ValueError, KeyError):`.
- **Duplicación:** la búsqueda de "cantidad requerida" recorriendo
  `recursos_requeridos` aparece repetida (`funciones_crear_evento.py:184-190` y
  `:209-215`; también en `funciones_series_recurrentes.py:183-189`). Se podría
  extraer una pequeña función auxiliar.
- **Números mágicos / límite duplicado:** `365` aparece hardcodeado en muchos
  módulos como límite de 1 año; una constante compartida evitaría inconsistencias.
- **`print()` de depuración** (`✅ Eventos guardados`, `✅ Ventana registrada`,
  etc.) mezclados con la lógica; para una GUI conviene retirarlos o usar
  `logging`.

Ninguno de estos afecta el funcionamiento; son observaciones de estilo.

## Dimensión 5 — Datos y persistencia

Modelo de datos bien pensado y con estructuras de datos razonables:

- Tres JSON: `eventos_predeterminados.json` (base de conocimiento de tipos, con
  reglas), `recursos.json` (inventario por categorías) y
  `eventos_planificados.json` (estado creado por el usuario).
- La persistencia funciona: `guardar_eventos_en_json` (`main.py:66`) ordena por
  fecha, hace una **copia limpia** quitando los objetos `date` no
  serializables (`main.py:76-79`) antes de `json.dump` — detalle maduro que
  evita el típico error de serialización.
- Índices en memoria (`eventos_por_fecha`, `recursos_por_clave`) reconstruidos
  con `_reindexar_eventos`/`_indexar_recursos`. La reindexación por rango de
  fechas del evento (`main.py:852`) es correcta.
- Único riesgo menor: `guardar_recursos` (`main.py:93`) reconstruye la
  estructura desde cero cada vez; correcto pero acoplado al formato exacto del
  JSON original.

## Dimensión 6 — Informe (`report.md`)

El informe (~2100 palabras) es de **alta calidad y en general honesto**.
Describe con precisión la arquitectura real: la clase `GestorEventos`, los
módulos y sus responsabilidades, el modelo de datos de tres JSON, los flujos de
uso, y decisiones de diseño (customtkinter, JSON, patrón Observer, validación
por capas). Todo lo que afirma sobre estructura **coincide con el código**.

Discrepancias detectadas (leves):

- En **§7 "Pruebas y Validación"** afirma que "todos los casos de prueba han
  sido superados satisfactoriamente, lo que demuestra la robustez del sistema",
  pero reconoce que **no hay pruebas automatizadas**. Es validación manual, no
  un banco de pruebas — la palabra "demuestra" **sobreestima** ligeramente. Para
  1er año es aceptable, pero conviene distinguir "probé a mano" de "tengo tests".
- El informe menciona un límite de "3 años" en algún residuo histórico
  (`logica_validaciones.py:171` lista "3 años"/"1095" entre palabras clave),
  mientras el código actual usa **1 año / 365 días** de forma consistente. Es un
  vestigio inofensivo, no una contradicción de features.

No se detectó ninguna feature afirmada que el código no tenga: series
recurrentes, sugerencia de fecha, sincronización de ventanas, desperdicio de
combustible — todo existe en el código.

---

## Valoración global (orientativa, sin nota numérica)

Trabajo **excepcional para primer año**. La modularización real, la separación
GUI/lógica, el modelado de un dominio con reglas de exclusión y coexistencia, la
gestión de un recurso consumible con desperdicio, y las series recurrentes
componen un proyecto de ambición y ejecución muy por encima del nivel esperado.
La ejecución dinámica confirmó que la lógica **funciona de verdad**: crea
eventos, consume combustible, detecta solapamientos y valida entradas inválidas
sin romperse.

- **Principal fortaleza:** arquitectura modular y separación de
  responsabilidades — permite razonar, testear y extender el código.
- **Principal área de mejora:** pulir detalles de estilo (`except:` desnudos,
  duplicación puntual, `print` de depuración) y, para el futuro, escribir
  pruebas automatizadas reales en lugar de solo validación manual.
