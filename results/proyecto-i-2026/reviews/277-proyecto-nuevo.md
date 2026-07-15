# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #277
- **Repositorio:** https://github.com/Monica15268/Proyecto-Nuevo
- **Estudiante:** Mónica Alejandra Bobadilla Mir
- **Grupo:** 111
- **Descripción declarada:** Gestor de sala de eventos (LUDOPATH — sistema de reservas de salas de juego)

---

## Nota metodológica importante

Es una **aplicación de consola** con `input()` (punto de entrada `src/Consola.py`,
guardia `if __name__ == "__main__"` en `Consola.py:316`). El verificador automático
marcó "sin punto de entrada" porque busca `main.py`/`app.py`, pero el punto de entrada
real es `Consola.py`. La ejecución se realizó alimentando el menú con `printf '…' | python Consola.py`.

Detalles del entorno:
- No hay dependencias externas reales. `src/pyproject.toml` declara `requests` y `pytest`,
  pero **ningún módulo importa `requests`** (solo stdlib: `datetime`, `collections`, `json`, `os`).
  `src/requirements.txt` contiene únicamente la palabra `python`. El proyecto corre con la
  biblioteca estándar sola.
- El `guardador.json` inicial está en la **raíz** del repo, pero el código lo lee/escribe con
  ruta relativa `"guardador.json"` (`FuncionesEspecificas.py:4`), es decir, en el CWD. Al
  ejecutar desde `src/` se crea un `guardador.json` nuevo allí. Para probar la carga de datos
  hubo que copiar el JSON a `src/`.
- `py_compile` de los cuatro módulos: **OK** (Python 3.12.8, venv aislado con `uv`).

## Dimensión 1 — Qué hace el programa

Menú de consola con 8 opciones (`Consola.py:18-25`):

1. **Nueva Reserva** (`Consola.py:108` → `Interfaz.reservar_sala`, `bueno.py:214`): pide sala,
   fecha/hora, duración y descripción; valida y, si procede, permite seleccionar recursos y
   persiste la reserva.
2. **Ver Todas las Reservas** (`listar`, `FuncionesEspecificas.py:178`).
3. **Buscar Huecos Disponibles** (`buscar_huecos`, `Consola.py:159`) — sistema de sugerencias
   de horarios libres por sala/duración/período.
4. **Eliminar Reserva** por ID (`eliminar`, `FuncionesEspecificas.py:204`).
5. **Ver Estado de Salas** (`estado_salas`, `Consola.py:264`).
6. **Eliminar reservas fuera de fecha** (`eliminar_pasadas`, `Consola.py:76`).
7. **Salir**.
8. **Ver recursos** — stock disponible en una ventana horaria (`obtention_state`, `FuncionesEspecificas.py:395`).

El motor de validación (`bueno.py`) comprueba: sala existente, duración 1–7h, día laboral
(L-V), fecha no pasada / ≤30 días, horario 08:00–17:00 y solapamiento con reservas existentes.
Hay un modelo de recursos con inventario y reglas de compatibilidad (consolas exigen bocinas,
juegos de mesa exigen comida+bocinas, comida/licores incompatibles con consolas —
`Clasessala.py:98-131`). Es un proyecto **ambicioso** para primer año.

## Dimensión 2 — Organización del código

Buena separación en cuatro módulos:
- `Clasessala.py` — clase `Salas`, inventario global, métodos estáticos de estado/inventario/compatibilidad.
- `FuncionesEspecificas.py` — persistencia JSON, alta/baja/listado de reservas, lógica de recursos.
- `bueno.py` — clase `Interfaz`: validaciones y orquestación de la reserva.
- `Consola.py` — capa de presentación (menú + I/O).

**Fortalezas:** la capa de presentación (`Consola.py`) está razonablemente separada de la
lógica; la clase `Interfaz` concentra las validaciones en métodos pequeños y legibles
(`bueno.py:121-146`); las reglas de compatibilidad están bien encapsuladas.

**Debilidades:**
- **Nombres inconsistentes** y mezcla español/inglés en la misma base
  (`window_inventary`, `funtion_save`, `Recurses_inventary`, `Obtencion_estado`,
  `bussiness_hour`). Errores ortográficos en identificadores (`inventary`, `funtion`,
  `Recurses`, `disponibility`, `bussiness`, `exixtence` en `bueno.py:133`).
- **Estado global mutable** (`List`, `actually_id` en `FuncionesEspecificas.py:9-10`) usado
  con `global` en varias funciones — funciona, pero acopla todo al mismo diccionario y hace
  el flujo difícil de razonar.
- `load_archives()` se llama repetidamente dentro de casi cada función
  (`add_reservations`, `listar`, `eliminar`, `verificar_disponibilidad_recursos`,
  `obtener_recursos_ocupados_en_horario`), recargando el JSON del disco una y otra vez.
- La clase `Salas` guarda `self.capacity` (`Clasessala.py:18`) pero la presentación lee
  `sala.capacidad` — inconsistencia que produce un crash (ver Dimensión 3).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Se ejecutó `Consola.py` recorriendo el menú con entradas concretas. Resultados:

1. **Arranque y salida (opción 7): OK.** El menú se dibuja y sale limpio (`EXIT=0`).
2. **Ver reservas (opción 2): OK.** Con el `guardador.json` de ejemplo lista las 2 reservas
   (IDs 4 y 5) con todos sus campos.
3. **Nueva reserva válida (opción 1): OK.** Reservé sala 1, `2026-08-03 09:00`, 2h, con
   recurso Bocinas → *"RESERVA CREADA EXITOSAMENTE, ID: 6"* y persistió en el JSON.
   El flujo completo (validación → selección de recursos → compatibilidad → alta) funciona.
4. **Validaciones inválidas (opción 1): TODAS OK.**
   - Fecha en el pasado (`2020-01-06`) → *"❌ No se puede reservar en el pasado"*.
   - Sábado (`2026-08-08`) → *"❌ Solo se pueden hacer reservas de lunes a viernes"*.
   - Fuera de horario (`20:00`) → *"❌ Horario no valido"*.
   - Sala inexistente (`9`) → *"❌ La sala no existe"*.
   - Fecha basura (`nononono`) → capturada: *"❌ Ocurrió un error inesperado: time data
     'nononono' does not match format…"* (no revienta con Traceback; el `try/except` de
     `Consola.py:137` lo atrapa).
5. **Solapamiento de horarios: OK.** Reservé sala 3 `2026-08-04 09:00` 3h, luego intenté
   `10:00` 2h en la misma sala → *"❌ Horario ocupado"* + sugerencias de huecos
   (`12:00`, `12:30`, `13:00`). Detección de colisión correcta y con sugerencias útiles.
6. **BUG — Ver Estado de Salas (opción 5): CRASH.** Traceback real:
   `AttributeError: 'Salas' object has no attribute 'capacidad'. Did you mean: 'capacity'?`
   en `Consola.py:275` (`sala.capacidad`). El atributo de la clase es `self.capacity`
   (`Clasessala.py:18`). **La opción 5 es inutilizable.**
7. **BUG — Buscar Huecos (opción 3): FALLA por el mismo motivo.** En `Consola.py:171` se lee
   `sala.capacidad` al listar salas; el `try/except` de `buscar_huecos` lo atrapa e imprime
   *"❌ Error: 'Salas' object has no attribute 'capacidad'"*. La opción 3 **no llega nunca**
   a buscar huecos.
8. **BUG latente — método mal nombrado.** `Consola.py:205` y `FuncionesEspecificas.py:79`
   llaman `interfaz.buscar_huecos_disponibles(...)`, pero el método real se llama
   `search_disponibles_hours` (`bueno.py:279`). No existe `buscar_huecos_disponibles` en
   ningún módulo. Estas llamadas **crashearían con AttributeError** si el flujo llegara a
   ellas; hoy quedan tapadas porque (a) la opción 3 muere antes por el bug de `capacidad`,
   y (b) en la ruta de alta, el solapamiento lo detecta antes `Interfaz.verification`
   (`bueno.py:151`, que sí usa el nombre correcto `search_disponibles_hours`).
9. **BUG — Ver recursos (opción 8): resultado incorrecto.** Reservé PS5 + Bocinas en
   `2026-08-05 09:00–11:00` y luego consulté el stock en esa misma ventana → siguió mostrando
   *PS5 = 3, Bocinas = 2* (stock lleno). Causa: `obtention_state` lee
   `valor.get('Recurses_inventary', [])` (`FuncionesEspecificas.py:413`), pero las reservas
   guardan los recursos bajo la clave `'Recursos'` (`FuncionesEspecificas.py:154`). El
   descuento de stock nunca ocurre. **La opción 8 siempre reporta inventario completo.**
10. **Eliminar pasadas (opción 6): OK.** Con las 2 reservas de febrero de 2026 (ya vencidas)
    → *"🗑️ Eliminando 4 / 5"* y guardó. (Cosmético: el mensaje dice
    `Eliminando if {rid}` — sobra la palabra "if", `Consola.py:99`.)
11. **Opción inválida (99): OK** → *"❌ Opción inválida. Intenta de nuevo."*

**Nota importante:** el bloqueo de recursos *entre reservas* (que dos reservas simultáneas
no compartan el mismo PS5) sí funciona por otra ruta —
`verificar_disponibilidad_recursos` (`FuncionesEspecificas.py:217`) usa la clave correcta
`'Recursos'`. El bug de la clave errónea afecta **solo** al display de la opción 8.

Resumen: el **núcleo (alta de reservas + todas las validaciones + solapamiento + recursos)
funciona de verdad**. Fallan tres cosas por errores de nombre: opción 5 (crash), opción 3
(crash atrapado), opción 8 (cálculo silenciosamente incorrecto).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores:** decente para 1er año — hay `try/except` en los puntos de entrada de
  I/O (`Consola.py:110`, `buscar_huecos`), lo que evita que el programa reviente ante entrada
  basura. Pero varios `except:` desnudos (`Clasessala.py:37`, `FuncionesEspecificas.py:248,281`)
  silencian cualquier error; conviene `except (ValueError, KeyError):`.
- **Imports dentro de funciones** por todas partes (`from Clasessala import Salas` repetido en
  ~8 sitios) para sortear ciclos de importación. Señal de que las dependencias entre módulos
  están enredadas; con reorganizar `Salas`/`List` en un solo módulo base se evitaría.
- **f-string con comillas anidadas del mismo tipo** en `Clasessala.py:64`
  (`f'Sala {u["sala"]}'`): funciona solo desde Python 3.12; en 3.8 (que el `pyproject` declara
  como mínimo) sería `SyntaxError`.
- **Código muerto / import sin usar:** `selection_recurses` importa `obtention_state`
  (`bueno.py:55`) y nunca lo usa; `guardador.json` versionado con datos de prueba.
- **Retorno inconsistente:** `add_reservations` a veces retorna `False`, a veces `None`, a
  veces un id entero (`FuncionesEspecificas.py:119, 145, 177`). El llamador `if id_reserva:`
  (`bueno.py:239`) trata `None`, `False` y `0` igual; para 1er año está bien, pero un id 0
  válido daría falso negativo (no ocurre porque los ids empiezan en 1).
- Nombres en español serían más consistentes que la mezcla actual, pero es un defecto menor.

## Dimensión 5 — Datos y persistencia

- Persistencia en `guardador.json` con `json.dump(..., indent=4)` (`FuncionesEspecificas.py:27`).
  Formato: `{"guardador": {id: {...}}, "identidad": N}`. Legible y correcto.
- Carga tolerante a fallos (`try/except (json.JSONDecodeError, FileNotFoundError)`,
  `FuncionesEspecificas.py:21`).
- Modelo de reserva razonable: `Sala`, `Descripcion`, `Inicio`, `Fin`, `Duracion`, `Recursos`,
  `Estado`. Fechas como strings ISO-like — se parsean con `strptime` en múltiples sitios (algo
  repetitivo pero correcto).
- **Fragilidad de ruta:** al usar ruta relativa `"guardador.json"`, la base de datos "vive"
  en el CWD desde donde se lanza el programa, no junto al código. El README dice ejecutar
  `consola.py` desde `src/`, con lo que el `guardador.json` inicial (que está en la raíz) no
  se carga. Sería más robusto anclar la ruta al directorio del script.
- Inventario de recursos como dict global (`Recurses_inventary`, `Clasessala.py:4`) — simple y
  suficiente para el alcance.

## Dimensión 6 — Informe (`README.md`, ~1849 palabras)

El repositorio **no tiene `report.md`**; el informe es `src/README.md`. Está bien escrito y es
extenso, pero **exagera y menciona features que el código no tiene**:

- Afirma *"sistema de evaluación donde los usuarios pueden calificar su experiencia"* y
  *"sistema de puntuación o comentarios"* → **no existe** en el código.
- *"tutoriales interactivos para ayudar a los nuevos usuarios"* → **no existe**.
- *"recordatorios sobre las medidas sanitarias vigentes"* y *"gestión de aforo"* → **no
  existe** (no hay control de aforo ni recordatorios; la capacidad de sala ni siquiera se
  muestra porque la opción que la usaría, la 5, crashea).
- Lista recursos *Licores* y *Bocinas* (correcto, están en el inventario) pero también dice
  que sugiere *"los próximos 15 horarios"* mientras el código limita a 5 (`bueno.py:317`).
- La sección "Cómo Correr" es honesta (clonar + ejecutar `consola.py` desde `src`).
- Las secciones "Dificultades" y "Cosas aprendidas" son genuinas y apropiadas.

Es decir, hay **desajuste entre lo que el README promete y lo que el programa hace**. Buena
parte de la prosa es aspiracional/comercial (medidas sanitarias, protección de bienes,
métodos de pago futuros) más que descriptiva del sistema entregado.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **ambicioso y con un núcleo funcional sólido**. La lógica de reservas —validación de
día laboral, horario, fecha, duración, existencia de sala, detección de solapamientos con
sugerencia de huecos alternativos, e incluso un modelo de recursos con reglas de
compatibilidad— **funciona de verdad al ejecutarla**, y las entradas inválidas se manejan sin
que el programa reviente. Para primer año, es un alcance notable y bien pensado.

Lo empañan tres bugs muy concretos, todos por inconsistencias de nombre entre módulos, que
delatan que el estudiante probó unas rutas del menú pero no otras: la opción 5 (Estado de
Salas) crashea por `capacidad` vs `capacity`, la opción 3 (Buscar Huecos) muere por el mismo
motivo antes de buscar nada, y la opción 8 (Ver recursos) calcula mal el stock por una clave
JSON equivocada (`Recurses_inventary` vs `Recursos`). A esto se suma un método fantasma
(`buscar_huecos_disponibles`) que hoy queda tapado pero es una bomba de tiempo. Ninguno es
difícil de arreglar (son ediciones de una línea), pero muestran que faltó una pasada de prueba
sobre cada opción del menú.

- **Principal fortaleza:** el motor de validación y el flujo de alta de reservas están bien
  diseñados y funcionan de extremo a extremo, incluida la detección de solapamientos con
  sugerencia de alternativas.
- **Principal área de mejora:** consistencia de nombres entre módulos (atributos, claves de
  diccionario, nombres de métodos) y una prueba manual de **cada** opción del menú antes de
  entregar — dos de las tres opciones rotas se habrían detectado con solo pulsar el número.
  En segundo lugar, alinear el README con lo que el programa realmente hace.
