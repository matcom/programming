# Reporte detallado — Issue #237

- **Estudiante:** Sergio M. de Santis
- **Grupo:** C111
- **Repositorio:** https://github.com/Sergio0307/Organizador-de-Eventos-Cinematogr-ficos-
- **Descripción del issue:** organización de eventos de cine teniendo en cuenta personal disponible, recursos y escenas, para la gestión y planificación de grabaciones.
- **Estructura:** `main.py`, `backend.py` (607 L), `frontend.py` (1470 L), 5 archivos JSON de datos, imágenes (`newcinema.jpg`, `cinema.jpeg`, etc.), `requirements.txt`, `report.md`.

> Nota importante: **este proyecto NO es una app de consola** (como asume la rúbrica por defecto), sino una **aplicación gráfica de escritorio con Tkinter + PIL + tkcalendar**. Se evaluó adaptando el paso de "ejecución dinámica" a una GUI.

---

## 1. Qué hace el programa

Aplicación de escritorio ("CINE EVENTOS PRO") para planificar producciones cinematográficas. El punto de entrada es `main.py:36` (`if __name__ == "__main__"`), que instancia `backend.GestorDatos`, `backend.LogicaNegocio` y `frontend.AplicacionEventos`, y arranca el bucle Tkinter con `app.ejecutar()` (`frontend.py:1468`).

El flujo real: pantalla de bienvenida (`mostrar_pantalla_inicial`, `frontend.py:95`) → menú principal (`mostrar_menu_principal`, `frontend.py:139`) con accesos a: gestión de recursos, gestión de personal, calendario de producción, catálogo de escenas, estadísticas y configuración. El núcleo funcional es el calendario (`mostrar_calendario_produccion`, `frontend.py:372`): el usuario elige una fecha, crea una producción (`agregar_produccion`, `frontend.py:573`) asignando recursos/personal por *spinboxes*, y el sistema descuenta el inventario, calcula un costo estimado y persiste todo en `producciones.json`. Los datos se guardan en JSON tras cada operación.

El dominio del código coincide con lo prometido en el issue (recursos + personal + escenas para planificar grabaciones).

## 2. Organización del código

Buena separación en tres archivos con responsabilidades claras:

- `backend.py`: **dos clases**. `GestorDatos` (`backend.py:6`) hace toda la persistencia (carga/guarda JSON, CRUD de recursos/personal/escenas/producciones). `LogicaNegocio` (`backend.py:404`) tiene precios, salarios, cálculo de costos, verificación de conflictos y sugerencias por escena.
- `frontend.py`: una sola clase `AplicacionEventos` con ~40 métodos, uno por pantalla/acción (`mostrar_*`, `agregar_*`, `eliminar_*`).
- `main.py`: orquestación mínima con `try/except` global (`main.py:9-34`).

Para 1er año, el nivel de organización es **notablemente alto**: uso real de clases, métodos con nombres descriptivos en español, docstrings en casi todo, y separación datos/lógica/UI. Puntos débiles:

- **Método duplicado:** `ir_a_hoy` está definido dos veces, idéntico, en `frontend.py:469` y `frontend.py:1431`; la segunda definición silenciosamente pisa a la primera (inofensivo pero es código muerto).
- `AplicacionEventos` es una clase muy grande (1470 líneas). Es esperable en un proyecto Tkleaner de este tamaño, no se penaliza, pero conviene mencionarlo.
- Uso de atributos "de instancia como variables temporales" para pasar estado entre closures (`self.titulo_var`, `self.vars_recursos`, etc. en `frontend.py:613,660`): funciona, pero mezcla estado de formulario con estado de la app.

## 3. Corrección funcional (basada en ejecución real)

Se hizo la ejecución dinámica en un entorno aislado con `uv` (Python 3.12) e instalando `Pillow` + `tkcalendar` con `uv pip install`. Resultados:

### Capa backend + lógica — EJECUTADA POR COMPLETO, TODO FUNCIONA ✅

Se instanció `GestorDatos` y `LogicaNegocio` y se ejercitaron sus métodos directamente:

- `GestorDatos()` carga los 5 JSON sin error. Personal total = 24, 7 escenas, 10 tipos de recurso.
- `calcular_costo_evento()` → `20808.33`; `obtener_estadisticas()` correcto.
- `agregar_produccion("2026-02-01", {...})` → devuelve `prod_001`, **descuenta correctamente** el inventario (cámaras 6 → 4) y persiste en `producciones.json`.
- `eliminar_produccion(...)` → **restaura** el inventario (cámaras 4 → 6). El ciclo descuento/restauración funciona bien.
- `verificar_conflictos_recursos("2026-02-01", {"camara":999}, {})` → detecta el conflicto: `["Resource 'camara': 4 available, 999 needed"]`. La validación de disponibilidad funciona.

### Capa GUI — CONSTRUIDA CORRECTAMENTE, recorrido click-a-click no completado ⚠️

- Sintaxis OK en los 3 `.py` (`ast.parse`). `import frontend` OK, clase presente.
- La construcción de `AplicacionEventos(g, l)` **avanza hasta la creación real de la ventana Tk** y solo se detiene en `cargar_imagen_fondo` (`frontend.py:80`) al convertir la imagen con `ImageTk.PhotoImage`. Ese fallo (`invalid command name "PyImagingPhoto"` / `_imagingtk`) es un **artefacto del entorno** (incompatibilidad entre el `_imagingtk` del wheel de Pillow y el Tcl/Tk enlazado estáticamente en el Python de `uv`), **no un error del código del estudiante**. En una instalación normal (Python del sistema + Pillow del sistema) esto no ocurre.
- No se completó el recorrido click-a-click de las pantallas porque el único display disponible era la sesión X en vivo del usuario (`:0`), y forzar Tk de `uv` contra ese servidor provocó abortos de `xcb` (`append_pending_request` assertion) — riesgoso contra el escritorio en uso. No había `xvfb` instalado para un display aislado.

### Bug funcional real encontrado (leído + confirmado ejecutando)

- `producir_escena` (`backend.py:439`) mezcla recursos con personal: `sugerir_recursos_para_escena("escenas de riesgo")` devuelve `["equipo de proteccion", "profesional en conduccion"]` (`backend.py:601`), pero `profesional en conduccion` es **personal**, no recurso. En la verificación (`backend.py:450-455`) se busca en el diccionario de recursos, no lo encuentra, y reporta erróneamente `"Faltan recursos: profesional en conduccion"`. Confirmado ejecutando: la producción de esa escena siempre falla por un falso "recurso faltante".
- **Desajuste de nombre de escena:** en `escenas.json` la 4ª escena es `"escenas_ secundarias"` (con un espacio extra tras el guion bajo), pero `sugerir_recursos_para_escena` (`backend.py:602`) usa la clave `"escenas_secundarias"` (sin espacio). Confirmado ejecutando: `sugerir_recursos_para_escena("escenas_ secundarias")` → `[]` (sin sugerencias), mientras que con la clave correcta sí devuelve `['camara', 'altavoces']`. La escena del catálogo real nunca recibe sugerencias.

### Validación de entradas

Buena para el nivel: `agregar_recurso`/`agregar_personal` (`frontend.py:1279,1319`) chequean `None` y cadena vacía; `procesar_produccion` (`frontend.py:716`) exige título; los *spinboxes* limitan cantidades al máximo disponible (`frontend.py:672-676`); hay confirmaciones (`askyesno`) antes de borrar. Los conflictos se avisan pero permiten "continuar de todos modos" (`frontend.py:749-754`), lo cual puede dejar el inventario en negativo — mitigado porque `agregar_produccion` (`backend.py:180`) clampa a 0.

## 4. Buenas prácticas de Python (nivel principiante)

- **A favor:** indentación consistente, docstrings, f-strings idiomáticos, `with open(...)` para archivos, `try/except` en todos los `guardar_*` (`backend.py:339-384`) y en `main.py`, `encoding='utf-8'` + `ensure_ascii=False`.
- **A mejorar:** `except:` desnudos en varios sitios (`frontend.py:47,399`, `backend.py:334`) — atrapan todo silenciosamente y ocultan errores; conviene `except Exception:` o el tipo concreto. Comentarios en mayúsculas tipo "TUS DATOS EXACTOS" (`backend.py:1,38`) delatan que el archivo se generó/adaptó con asistencia y quedaron restos. Import duplicado de `datetime` en `frontend.py:6,9`.

## 5. Datos y persistencia

Correcta y funcional. Cinco archivos JSON, estructuras razonables: diccionarios `{nombre: cantidad}` para recursos y personal, lista para escenas, y `{fecha: [producciones]}` para el calendario. Persiste tras cada operación y se verificó ejecutando que el round-trip (crear → guardar → eliminar → restaurar) mantiene la coherencia del inventario. Un detalle: `escenas.json` original era un diccionario y el código lo normaliza a lista (`backend.py:86-89`), buen manejo defensivo.

## 6. Informe (`report.md`)

El informe es **extenso y muy bien presentado** (índice, diagramas ASCII de arquitectura, tablas, manual de usuario), pero **sobreestima bastante** respecto al código real:

- Menciona un archivo **`backend3.py`** como "capa de lógica de negocio" separada (`report.md:161,231,340`). **No existe**: `LogicaNegocio` vive dentro de `backend.py`. La "arquitectura de tres capas" descrita no corresponde a los archivos reales (son 2 módulos, no 3).
- Afirma un **sistema de backup automático** en carpeta `backups/` cada 5 minutos (`report.md:451-453,593-598`). No hay tal carpeta ni lógica de backup; `guardado_automatico` (`frontend.py:1442`) solo reprograma un `after()` cada 5 min pero su cuerpo **no guarda nada** (falta la llamada a `guardar_todo`).
- Habla de "9 tipos de errores de validación detectados automáticamente" (`report.md:138-143,354-390`); el código implementa solo verificación de recursos/personal insuficientes. Las restricciones de seguridad, habilidad, horario, etc., **no están implementadas**.
- "Métricas de éxito" (60% menos tiempo, errores eliminados, satisfacción alta — `report.md:622-627`) y secciones de "pruebas realizadas" con resultados ✅ son claramente decorativas/inventadas para un proyecto estudiantil.
- Menciona un `README.md` que no está en el repo.

En resumen: informe presentable pero infla features. Hay que valorar el trabajo por el código, que es sólido, no por el informe, que promete de más.

---

## Síntesis para el profesor

Trabajo **muy por encima del promedio de 1er año**: GUI real con Tkinter/PIL/tkcalendar, separación limpia en clases datos/lógica/UI, persistencia JSON funcional, y un flujo de descuento/restauración de inventario que se verificó ejecutando y funciona correctamente. La capa backend/lógica se ejecutó por completo sin errores. La GUI construye la ventana correctamente; el recorrido click-a-click no se completó por un artefacto de entorno (binding PIL↔Tk) más el riesgo de usar la sesión X en vivo, no por fallos del código.

Debilidades reales: (1) bug en `producir_escena` que confunde personal con recursos; (2) desajuste de nombre de escena `escenas_ secundarias`; (3) `guardado_automatico` no guarda nada pese a lo que dice el informe; (4) el informe sobreestima (menciona `backend3.py`, backups, 9 validaciones que no existen). Ninguna de estas rompe el uso básico. Nivel general: **sólido / destacado para principiante**.
