# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #299
- **Repositorio:** https://github.com/tax-evasionmaster33/Proyecto-Pro.git
- **Estudiante:** Rafael Saumell Baranda
- **Grupo:** C121
- **Descripción declarada:** Software de gestión y optimización de tiempo y recursos para la Oficina de Trámites Proenza. Python + JSON para persistencia + Streamlit para la interfaz gráfica.

---

## Nota metodológica importante

**No es una app de consola.** Es una aplicación web multipágina construida con **Streamlit** (no hay `input()` en ningún módulo). Adapté la ejecución así:

1. **Lógica de negocio directa:** importé `Gestor.py` / `Restricciones.py` en un intérprete aislado y ejercité `hay_conflicto`, `buscar_hueco`, `validar_restricciones`, `archivar_citas_pasadas` y el inventario con los datos reales del repo (98 citas en `PerCitas.json`).
2. **Arranque headless real:** `streamlit run main.py --server.headless true`. El servidor levantó correctamente (Uvicorn en el puerto, `HTTP 200` en la home y en ambas páginas `1-Opciones_de_Citas` y `2-Itinerario_de_Citas`; `/_stcore/health` → `ok`), sin ninguna excepción en el log del servidor.
3. `py_compile` de los cinco módulos: todos OK.

Veredicto de ejecución: **el programa corre de verdad**. No hubo fallo de código; el único inconveniente relevante fue una discrepancia de nombre de fichero en la documentación (ver Dimensión 6).

---

## Dimensión 1 — Qué hace el programa

Es un gestor de citas para una oficina de trámites migratorios que cubre cinco destinos (España, EEUU, Canadá, Brasil, Turkia). Estructura de tres páginas Streamlit:

- **`main.py`** — página de inicio: datos de contacto de la oficina, imagen y un panel opcional con recursos humanos/materiales (`main.py:12-59`).
- **`pages/1-Opciones_de_Citas.py`** — el corazón operativo: agendar / eliminar / modificar citas mediante tres botones que conmutan `st.session_state.page` (`pages/1-Opciones_de_Citas.py:21-31`).
- **`pages/2-Itinerario_de_Citas.py`** — consulta: tabla de citas activas por día, tabla de archivadas, y vistas "todas activas / todas archivadas" (`pages/2-Itinerario_de_Citas.py:32-69`).

El flujo de **agendar** encadena: filtrado de recursos por tramitadora (`1-Opciones_de_Citas.py:50-55`), validación por regex de nombre/apellido/teléfono (`:106-111`), chequeo de inventario de planillas (`:120-122`), detección de conflicto (`:139`) y, si hay solape, propuesta automática de un hueco libre con confirmación del usuario (`:140-147`, `:83-99`). Al confirmar, descuenta planillas y persiste (`:149-153`).

## Dimensión 2 — Organización del código

**Buena separación de capas.** `Restricciones.py` es configuración pura (solo diccionarios tipados con `typing`, sin funciones ni imports de lógica), lo que permite añadir un país o cambiar una duración sin tocar el motor. `Gestor.py` concentra la lógica de dominio en dos clases (`GestorCitas`, `GestorInventario`) con métodos de responsabilidad única. Las páginas de UI solo orquestan. Esta modularidad es notable para primer año.

**Debilidad principal — código muerto que contradice el informe.** La clase `Cita` (`Gestor.py:12-37`) y el método `validar_restricciones` (`Gestor.py:96-113`) **nunca se usan**:

- `grep "Cita("` sobre todo el repo: **cero** instanciaciones. La UI construye el diccionario de cita a mano (`1-Opciones_de_Citas.py:124-135`), no vía `Cita.to_dict()`.
- `validar_restricciones` no se invoca en ninguna página. La validación real la hace la UI con regex y con los propios selectboxes (que ya limitan a valores autorizados).

Esto importa porque el informe presenta ambos como piezas centrales de la arquitectura (ver Dimensión 6).

**Detalle menor de claves.** `Cita.to_dict()` emite claves capitalizadas (`Pais`, `Fecha`, `Hora`…, `Gestor.py:26-36`), pero los datos reales y toda la lógica usan minúsculas (`c['fecha']`, `c['hora']`, `c['trabajadora']`). Como `Cita` está muerta, no rompe nada, pero es una inconsistencia latente que estallaría si alguien intentara usar la clase.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejercité la lógica con datos reales. Resultados concretos:

1. **Detección de conflicto por trabajadora** — dos citas de Yuli, 09:00-09:50 y 09:30-10:20 → `hay_conflicto` = `True`. Correcto.
2. **No-conflicto legítimo** — misma franja, distinta trabajadora (Yanara) y recursos disjuntos → `False`. Correcto: el diseño permite solapes cuando no hay recurso compartido.
3. **Conflicto por recurso compartido** — distinta trabajadora pero ambas usan `Planilla` en la misma franja → `True`. Correcto; la condición `set(...) & set(...)` (`Gestor.py:125-128`) captura bien el caso.
4. **`buscar_hueco`** — tras ocupar 09:00-09:50, pidió slot para una cita solapada → devolvió **10:00**. Correcto para la rejilla de 15 min: 09:50 no cae en múltiplo de 15, el siguiente libre es 10:00 (`Gestor.py:132-144`).
5. **`validar_restricciones`** (aunque muerta, la probé) — con `("España","Betsy",["Planilla"],…)` devolvió `['❌ Betsy no está autorizada…', '❌ Faltan recursos: Escaner, Laptop Yuli, Impresora, Laptop Zahili']`; con datos válidos → `[]`. La acumulación de errores funciona.
6. **Inventario y auto-recarga** — `stock_actual()` devolvió **995** partiendo de un `Inventario.json` con `stock: 495` y `ultimo_reset: "2026-06-01"`: `verificar_recarga` detectó el cambio de mes (hoy es 2026-07) y sumó los 500 de recarga, persistiéndolo. Comportamiento correcto y **con efecto secundario en disco** (ver más abajo).
7. **`archivar_citas_pasadas`** con las 98 citas reales → 30 quedaron activas, 177 pasaron a `CitasArchivadas.json` distribuidas en 54 días. Ejecutó sin error.
8. **Entrada basura en fecha** — `hay_conflicto` con `fecha="basura"` lanza `ValueError` de `strptime`. **No es un bug real**: la UI nunca pasa texto libre como fecha (usa `st.selectbox`/`date`), así que la ruta no es alcanzable en producción. Aun así, la función carece de defensa por sí sola.
9. **Arranque headless** — servidor Streamlit OK, home y ambas páginas `HTTP 200`, sin trazas de error en el log.

**Observación de efectos secundarios en importación.** `1-Opciones_de_Citas.py:12-13` llama a `archivar_citas_pasadas()` y `verificar_recarga()` en el nivel de módulo, es decir, cada vez que se carga la página se muta el estado en disco. Es funcional pero acopla la lógica de mantenimiento al ciclo de render de Streamlit.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Aciertos: manejo defensivo de I/O (`try/except (FileNotFoundError, json.JSONDecodeError)` recreando el archivo, `Gestor.py:42-51`); `encoding="utf-8"` + `ensure_ascii=False` para preservar tildes; `max(0, ...)` para evitar stock negativo (`Gestor.py:197`); rutas robustas con `os.path.dirname(__file__)` en vez de rutas relativas frágiles (`Gestor.py:7-9`, `2-Itinerario_de_Citas.py:6-8`); regex razonables para validar nombre/teléfono (`1-Opciones_de_Citas.py:106-111`).

Mejorables (menores): mezcla de convenciones de nombres de variable (`Pais`, `Fecha`, `Hora` con mayúscula inicial junto a `nombre`, `apellido` en minúscula, `1-Opciones_de_Citas.py:45,64,75`); código muerto (`Cita`, `validar_restricciones`) que conviene borrar o cablear; efectos secundarios a nivel de módulo (punto 6 de arriba); el fichero de dependencias se llama `requerimientos.txt` pero el README manda `pip install -r requirements.txt` (nombre inexistente).

## Dimensión 5 — Datos y persistencia

Modelo sencillo y coherente con tres ficheros JSON:

- `PerCitas.json` — **lista** de citas activas (verificado: 98 objetos con claves `Tipo_Tramite, Nombre, Apellido, pais, fecha, hora, duracion, trabajadora, recursos_usados, Telefono`).
- `CitasArchivadas.json` — **diccionario** `{fecha: [citas]}`, buena decisión para consultar un día en O(1).
- `Inventario.json` — objeto único `{planillas: {stock, recarga_mensual, ultimo_reset}}`, extensible a otros consumibles.

La separación activas/archivadas mantiene pequeño el fichero de escritura frecuente. La serialización es directa (dicts planos de primitivos), apropiada al alcance. El único punto flojo: la persistencia va por manipulación directa de dicts, no por la clase `Cita` que el informe presume como capa de serialización.

## Dimensión 6 — Informe (`report.md`)

El informe está **muy bien escrito y es técnicamente detallado** — con diagramas de estructura, fragmentos de código y explicación del algoritmo de solapamiento de intervalos. Pero sobreestima el código en dos puntos concretos:

1. **Clase `Cita` como DTO de persistencia** (sección 4.1): describe `to_dict()` como *"el único mecanismo de persistencia del sistema"*. **Falso** — la clase nunca se instancia; la persistencia es por dicts construidos en la UI.
2. **`validar_restricciones` como validación central** (secciones 4.2 y 7): dice que el flujo de agendar *"llama a `GestorCitas.validar_restricciones` → si hay errores, se muestran todos y se detiene"*. **No ocurre**: la página nunca invoca ese método; la validación efectiva es la de regex de la UI.

Además, discrepancias de nombre que rompen las instrucciones al pie de la letra:

3. El informe (sección 5) y el README mandan `streamlit run Main.py`, pero el fichero es **`main.py`** (minúscula). En Linux/macOS (case-sensitive) ese comando **falla**; solo funcionaría en Windows. Verifiqué: `test -f Main.py` → no existe.
4. La estructura del informe lista `Main.py` y `requerimientos.txt` presentado como `requirements.txt` en el README.

El informe es honesto sobre el diseño *pretendido*, pero describe una arquitectura ligeramente más elaborada que la que el código realmente ejecuta.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido**. La lógica de negocio funciona de verdad: la detección de conflictos por trabajadora **y** por recurso compartido está bien pensada y verificada, el `buscar_hueco` resuelve solapes automáticamente, y el inventario con auto-recarga mensual es un detalle maduro poco común en primer año. La separación configuración/lógica/UI es limpia y la aplicación Streamlit levanta y sirve sus tres páginas sin errores. Es un trabajo ambicioso con una lógica de dominio real, no un ejercicio de juguete.

El punto que le resta es el desajuste entre el informe y el código: se documentan como piezas centrales una clase `Cita` y un `validar_restricciones` que en realidad son código muerto, y las instrucciones de ejecución apuntan a un `Main.py` que no existe con ese nombre en un sistema case-sensitive. Nada de esto es un bug funcional, pero afecta la fidelidad del informe y la reproducibilidad.

- **Principal fortaleza:** motor de detección de conflictos + resolución automática de huecos + inventario con auto-recarga, todo verificado ejecutando con los datos reales del repo.
- **Principal área de mejora:** alinear informe y código — borrar (o cablear) `Cita` y `validar_restricciones`, y corregir el nombre de entrada a `main.py` y del fichero de dependencias.
