# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #284
- **Repositorio:** https://github.com/lauxjmnz/Eventum_Center
- **Estudiante:** Lauren Jimenez Espinosa
- **Grupo:** C121
- **Descripción declarada:** Eventum Center es un centro de convenciones que automatiza la planificación de eventos y la asignación de recursos para mejorar la experiencia del usuario.

---

## Nota metodológica importante

Es una aplicación de **consola** con menú interactivo (`input()` en `main.py:19` y a lo largo de `planificador.py`). No es GUI. La ejecución se hizo alimentando el menú con `printf '...' | python main.py` para recorrer todos los flujos (crear, mostrar, editar, eliminar, ver agenda) con entradas válidas e inválidas. No hay dependencias externas: `requirements.txt` está vacío y todo el código usa solo la biblioteca estándar (`datetime`, `json`), por lo que el entorno se creó con `uv venv --python 3.12` sin instalar nada. `py_compile` de los cuatro módulos: **OK**.

## Dimensión 1 — Qué hace el programa

Gestor de eventos para un centro de convenciones. El punto de entrada `main.py` instancia `Planificador` (que carga `data.json` en el constructor, `planificador.py:7-9`) y presenta un bucle de menú con seis opciones (`main.py:16-36`):

1. **Crear evento** (`crear_evento`, `planificador.py:218-283`): pide nombre, fecha (formato `DD/MM/AAAA`, no pasada), hora inicio/fin, y una lista de recursos. Detecta eventos que cruzan medianoche y avanza `fecha_fin` un día (`planificador.py:236-239`). Valida dependencias (`requiere`), exclusiones (`excluye`) y colisiones de recursos con otros eventos.
2. **Mostrar evento** (`mostrar_evento`, `planificador.py:287-305`): lista todos los eventos con fechas, horas y recursos.
3. **Editar evento** (`editar_evento`, `planificador.py:308-507`): submenú para cambiar nombre, fecha, horas o recursos (agregar/eliminar), re-validando colisiones en cada cambio.
4. **Eliminar evento** (`eliminar_evento`, `planificador.py:510-529`).
5. **Ver agenda de un recurso** (`ver_agenda_recursos`, `planificador.py:151-189`): lista los eventos que usan un recurso dado.
6. **Salir**.

Cuando un evento choca por recursos, `buscar_huecos` (`planificador.py:192-215`) itera en pasos de 30 min durante 48 h y sugiere el primer hueco libre. Verificado en ejecución: al crear un segundo evento que solapa "Sala Central" el 20/08 10:00–12:00, imprimió correctamente *"Encontramos un hueco... 20/08/2026 de 11:00 al 13:00"*.

## Dimensión 2 — Organización del código

Muy buena separación en cuatro módulos con responsabilidades claras:

- `evento.py:3-50` — clase `Evento`: encapsula datos + métodos de tiempo (`inicio_datetime`, `fin_datetime`, `horas_solapadas`) y serialización (`to_dict`, `from_dict`, `copy`). Uso correcto de `@staticmethod` para `from_dict`.
- `recursos.py:1-47` — catálogo declarativo de recursos con reglas `requiere`/`excluye`. Separar los datos de la lógica es una decisión de diseño madura para primer año.
- `planificador.py` — motor: validación, persistencia, colisiones, búsqueda de huecos.
- `main.py` — solo orquesta el menú.

**Fortalezas:** la clase `Evento` está bien pensada; el uso de `copy()` para construir "eventos de prueba" antes de confirmar cambios (`planificador.py:203`, `346`, `381`, `450`) es un patrón limpio que evita mutar el estado real durante la validación.

**Debilidades menores:** `editar_evento` es una función de ~200 líneas con anidamiento profundo (`planificador.py:308-507`); la rama de edición de recursos duplica casi el mismo bloque de validación dos veces (con recursos y sin recursos, `planificador.py:427-499`). Extraer un método `agregar_recursos(evento, indice)` eliminaría esa duplicación. `i` se reutiliza como variable de bucle en contextos anidados (`planificador.py:314` y `404`), lo que no causa bug aquí pero es frágil.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Todo lo que se corrió funcionó sin `Traceback`. Numerado:

1. **Crear evento válido** (`Conferencia IA`, 20/08/2026 09:00–11:00, Sala Central + Proyector) → *"Su evento se ha creado correctamente"* y persistido en `data.json`. ✔
2. **Colisión de recursos** (segundo evento con Sala Central solapando) → rechazado + búsqueda de hueco correcta (11:00–13:00). ✔
3. **Validación `requiere`**: "Camara de Grabacion" sola → *"El recurso 'Camara de Grabacion' requiere 'Sala Central'"* + *"Error en los recursos seleccionados(requiere)"*. ✔
4. **Validación `excluye`**: Sala Central + Sala pequeña → *"no puede usarse junto con 'Sala pequeña'"*. ✔
5. **Entradas inválidas en menú**: `abc` → *"debe introducir un numero"*; `99` → *"esa opcion no aparece"*. ✔
6. **Fecha basura / fecha pasada / hora basura**: `xx/yy/zzzz` → formato incorrecto; `01/01/2020` → *"no se permiten fechas pasadas"*; `25:99` → formato de hora incorrecto. Todo re-pregunta sin reventar. ✔
7. **Evento de madrugada** (23:00→01:00) → *"Fue detectado un evento de madrugada. El evento termina el 24/08/2026"* y `fecha_fin` correcta en `data.json`. ✔
8. **Mostrar / Ver agenda / Editar (nombre) / Eliminar**: todos funcionaron y persistieron. ✔
9. **Recuperación de `data.json` corrupto**: con contenido no-JSON, arrancó vacío y reescribió el archivo válido (rama `JSONDecodeError`, `planificador.py:33-35`); con un `[]` (lista, no dict) imprimió el aviso de formato inválido (`planificador.py:29`). Manejo de errores de persistencia sólido. ✔

**Bug real encontrado (menor):** al intentar eliminar un recurso del que otro depende (ej. quitar "Sala Central" cuando "Proyector" la requiere), la lógica lo deniega correctamente **pero el mensaje sale roto**: `planificador.py:432` imprime literalmente *"No puedes eliminar {r_eliminado} porque otro recurso depende de el"* — falta el prefijo `f` en el string, así que no interpola el nombre del recurso. La comparación con la línea 436 (que sí usa `f"..."`) confirma el descuido. La funcionalidad es correcta; solo el texto al usuario está mal.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- Manejo de errores adecuado: `try/except ValueError` en cada parseo de entrada, `FileNotFoundError`/`JSONDecodeError` en la carga. Muy por encima del promedio de primer año.
- Sin variables globales problemáticas; `planificador` es una única instancia en `main.py:2`.
- Nombres de métodos claros y en español consistente.
- Puntos mejorables: falta espaciado consistente alrededor de `=` y operadores (PEP 8) — casi ninguna línea lo respeta (`self.eventos=[]`, `eleccion==1`). Es cosmético y no afecta ejecución, pero un formateo automático (`ruff format` / `black`) lo dejaría prolijo.
- El bug del f-string (Dim. 3) es el ejemplo típico de por qué conviene, si se usa un editor con linter, atender los avisos de "f-string sin placeholders / placeholders sin f".
- `list(set(evento_elegido.recursos))` (`planificador.py:462`, `493`) elimina duplicados pero **reordena** los recursos de forma no determinista. Para este proyecto es inocuo; con `dict.fromkeys(...)` se preservaría el orden.

## Dimensión 5 — Datos y persistencia

Modelo simple y correcto: cada `Evento` se serializa vía `to_dict` (`evento.py:21-29`) y el conjunto se guarda como `{"eventos": [...]}` en `data.json` con `json.dump(..., indent=4, ensure_ascii=False)` (`planificador.py:37-40`), lo que conserva tildes. La carga reconstruye objetos con `from_dict`. El guardado ocurre tras cada operación de escritura (crear, editar, eliminar), así que el estado en disco siempre refleja la sesión. Las fechas/horas se almacenan como texto y se parsean a `datetime` bajo demanda — decisión razonable, aunque significa que la validación de formato depende de que el JSON no se edite a mano.

## Dimensión 6 — Informe (`report.md`)

El informe es claro, bien estructurado y **coincide con el código en lo esencial**: describe bien la arquitectura modular, la lógica `requiere`/`excluye`, la detección de eventos de madrugada y el buscador de huecos. Todo eso está implementado y verificado.

Discrepancias/matices:

- El informe afirma *"No se permiten eventos de varios días"* (`report.md:7`), pero el código **sí crea eventos multidía** en el caso de madrugada (`fecha` ≠ `fecha_fin`, verificado: 23/08→24/08). Es una contradicción menor entre la restricción declarada y el comportamiento real (que en realidad es más capaz de lo que el informe dice).
- El informe dice que el buscador analiza *"los próximos dos días"* (`report.md:43`); el código itera 96 pasos de 30 min = 48 h exactas (`planificador.py:199`), lo que es consistente.
- El tono del informe es algo grandilocuente ("solución informática avanzada", "inteligencia de negocio", "alta fiabilidad"). Para un proyecto de primer año está bien mostrar ambición, pero conviene calibrar: el sistema es correcto y bien organizado, no necesita superlativos para brillar.
- El informe no menciona ninguna limitación conocida ni el bug del mensaje. Un breve apartado de "limitaciones" sumaría honestidad técnica.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido**. Es de los trabajos de primer año que dan gusto ejecutar: arranca sin fricción, no revienta con ninguna entrada basura que le tiré, y cubre un dominio con reglas de negocio genuinamente no triviales (dependencias entre recursos, exclusión mutua, colisiones de horario, eventos que cruzan medianoche, y hasta un buscador automático de huecos). La separación en cuatro módulos y el patrón de "evento de prueba con `copy()` antes de confirmar" muestran una madurez de diseño superior a lo esperado. El único bug real es cosmético (un `f` faltante en un mensaje). El informe es bueno y en su mayoría fiel, con una contradicción menor sobre eventos multidía.

- **Principal fortaleza:** la lógica de negocio (validación de recursos + colisiones + búsqueda de huecos) es correcta y está verificada en ejecución; la arquitectura modular está bien pensada.
- **Principal área de mejora:** refactorizar `editar_evento` (200 líneas, duplicación) en submétodos más pequeños, y corregir el f-string roto de `planificador.py:432`. En segundo plano, un formateo automático dejaría el estilo a la altura de la lógica.
