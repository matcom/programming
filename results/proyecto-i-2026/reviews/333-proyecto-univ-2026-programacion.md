# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #333
- **Repositorio:** https://github.com/SamuelAlej18/proyecto-univ-2026-programacion
- **Estudiante:** Samuel Alejandro Vera Power
- **Grupo:** C111
- **Descripción declarada:** Planificador de conciertos (aplicación web cliente-servidor)

---

## Nota metodológica importante

Este **no** es un proyecto de consola. Es una aplicación **cliente-servidor**:

- Backend Flask (API REST) en `servidor/` — `main.py`, `funciones.py`, `crearUsuario.py`.
- Frontend web (HTML/CSS/JS puro) en `web/` — `index.html`, `style.css`, `script.js`, que consume la API vía `fetch`.

La verificación automática del bot marcó "sin punto de entrada" porque buscaba un `main.py` de consola en la raíz; el punto de entrada real es `servidor/main.py` (`app.run(port=5000)` en `main.py:158-159`), que se ejecuta desde dentro de `servidor/` por los imports relativos `from crearUsuario import ...` y `from funciones import ...` (`main.py:4-5`).

Adapté la ejecución así:

1. `uv venv --python 3.12` dentro de `servidor/` e instalé `Flask==3.1.2` y `flask-cors==6.0.2` (las dependencias de `requirements.txt`, que además está en codificación UTF-16 — ver Dimensión 5).
2. Arranqué el servidor Flask real y ejercité **todos** los endpoints con `curl` (flujos válidos e inválidos).
3. Probé la **lógica de negocio** de `funciones.py` de forma aislada importándola y llamándola con datos reales, para separar la calidad de los algoritmos de un bug de integración que rompe el arranque.
4. `py_compile` de los tres módulos.

## Dimensión 1 — Qué hace el programa

El sistema gestiona la planificación de conciertos en un complejo de cuatro locales (`Anfiteatro del Bosque Plateado`, `Sala 23`, `Teatro Recuerdos`, `Cuarto del Rock`, `funciones.py:38`) con un inventario central de recursos (`main.py:25-34`).

Endpoints (`main.py`):

- `POST /register` (`main.py:68`) — crea usuario si cumple formato y trae la clave secreta `'El concierto'` (`crearUsuario.py:35`).
- `POST /login` (`main.py:56`) — verifica credenciales contra `usuarios.json`.
- `GET /eventos` (`main.py:81`) — lista los eventos ordenados cronológicamente, purgando los expirados.
- `POST /ingresarEvento` (`main.py:85`) — valida el evento contra todas las reglas de negocio y lo inserta ordenado.
- `DELETE /eliminarEvento/<int:indice>` (`main.py:135`) — elimina por índice.
- `POST /eventoEnFuturo` (`main.py:146`) — busca una fecha alternativa en los próximos 30 días.

El frontend (`web/script.js`) implementa el flujo completo: bienvenida → registro/login → panel de eventos con tarjetas, formulario de creación, botón de eliminar y botón de "buscar fecha alternativa", con validación en vivo de usuario/clave (`script.js:88-184`) y efecto de partículas (`script.js:455`).

## Dimensión 2 — Organización del código

Buena separación de responsabilidades para primer año:

- `main.py` = capa HTTP/routing y persistencia de archivos.
- `funciones.py` = toda la lógica de negocio (validación, colisiones, demanda de recursos, ordenamiento).
- `crearUsuario.py` = autenticación y validación de credenciales.

Los nombres son descriptivos y en español consistente (`calcularMayorDemandaDeRecursos`, `verificarColisionConArtistaMundial`, `eliminarEventosExpirados`). Las funciones son en general pequeñas y de propósito único.

Debilidades menores:

- El `inventario` está *hardcodeado* como dict en `main.py:25-34`, mientras que los lugares válidos están *hardcodeados* en `funciones.py:38`. Conviene un único módulo de configuración/constantes.
- `import json` aparece en medio de `crearUsuario.py:32`, después de una función; los imports deberían ir arriba.
- El patrón de retorno `[True]` / `[False, mensaje]` (listas de longitud variable) funciona pero es frágil; una tupla `(bool, str)` o excepciones serían más idiomáticas (ver Dimensión 4).

## Dimensión 3 — Corrección funcional (basada en ejecución real)

**Lo que corrí y observé:**

1. `py_compile` de los tres módulos → **OK** (compilan sin error de sintaxis).
2. **`GET /eventos` con la BD vacía → HTTP 500, `TypeError: object of type 'bool' has no len()`.** Bug **crítico** en `main.py:43`: `if len(eventosPosteriores!= len(eventos)):`. Los paréntesis están mal colocados: Python evalúa primero `eventosPosteriores != len(eventos)` (que da un `bool`) y luego intenta `len(bool)`. Debe ser `if len(eventosPosteriores) != len(eventos):`.
3. **Alcance del bug:** `cargarEventos()` (`main.py:39-46`) es llamada por **todos** los endpoints de eventos — `/eventos` (`main.py:83`), `/ingresarEvento` (`main.py:106`), `/eliminarEvento` (`main.py:138`) y `/eventoEnFuturo` (`main.py:149`). Verifiqué que `POST /ingresarEvento` con el evento "válido" del propio informe también revienta con el mismo `TypeError`. **En el estado subido, el subsistema de eventos no funciona en absoluto.** El único fallo es esa línea; el sistema arranca y registro/login sí funcionan.
4. `POST /register` clave secreta incorrecta → HTTP 400 `"...no es válido o alguna de las claves"`. **Correcto.**
5. `POST /register` válido (`usuario=samuelvera`, `clave=Password12`, secreta correcta) → HTTP 200 `"Usuario registrado"`, se persiste en `usuarios.json`. **Correcto.**
6. `POST /login` correcto → HTTP 200 `"Sesión iniciada"`; clave errónea → HTTP 400. **Correcto.**

**Para separar el bug de integración de la calidad de la lógica**, apliqué la corrección de una línea del punto 2 en mi copia (no en el repo) y volví a ejercitar todo:

7. `GET /eventos` → HTTP 200 `[]`. **Arreglado con un solo carácter.**
8. `POST /ingresarEvento` (Concierto A, Anfiteatro, recursos válidos) → HTTP 200 `"Evento guardado correctamente"`. **Correcto.**
9. `POST /ingresarEvento` (Concierto B, mismo lugar, horario solapado) → HTTP 400 `"Recursos insuficientes o colisión de lugar"`. **Detección de colisión de lugar correcta.**
10. `POST /ingresarEvento` (nombre `"Grandes éxitos"`, con tilde) → HTTP 400 rechazo de nombre. **Correcto.**
11. `DELETE /eliminarEvento/0` → HTTP 200; `/eliminarEvento/99` (fuera de rango) → HTTP 400. **Ambos correctos.**

También probé la lógica de `funciones.py` de forma aislada (11 casos): evento válido, nombre con tilde, amplificadores < guitarras+1, brea sin violín, duración < 1h, antelación < 24h, cámaras insuficientes, demanda sobre inventario, inserción binaria ordenada y filtro de expirados. **Todos dieron el resultado esperado** — la inserción binaria mantuvo `[E5, E10, E15, E20]` en orden correcto y el barrido de demanda máxima detectó bien la escasez de recursos.

12. **Segundo bug (lógico), confirmado por ejecución.** En `funciones.py:99-105`, la regla de `Sala 23` / `Cuarto del Rock`:
    ```python
    if lugar == 'Sala 23' or lugar == 'Cuarto del Rock':
        if piano > 0 and bateria > 0:
            return [False, '...no se admiten piano y batería...']
        elif piano != 1:
            return [False, 'En este lugar solo se admite un piano']
        elif bateria != 1:
            return [False, 'En este lugar solo se admite una batería']
    ```
    La intención (según el informe) es "como máximo un piano y como máximo una batería". Pero `piano != 1` rechaza también el caso `piano == 0`. Verifiqué: un evento en `Sala 23` **sin piano ni batería** (solo cámara + micrófono) → `[False, 'En este lugar solo se admite un piano']`, cuando debería ser válido. Y con exactamente 1 piano pero sin batería → `[False, 'En este lugar solo se admite una batería']`. **Resultado: dos de los cuatro locales quedan prácticamente inutilizables** (rechazan casi cualquier evento). La corrección es `elif piano > 1:` y `elif bateria > 1:`. Esto salió al probar `/eventoEnFuturo` end-to-end, que devolvió el error del piano para un evento sin piano.

**Distinción entorno vs. estudiante:** ninguno de los dos bugs es del entorno. El X11/display no aplica (es una web). Ambos son errores del código del estudiante; el primero es de integración (un carácter), el segundo es lógico (condición mal formulada).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Fortalezas:

- `convertirStrDatetime` (`funciones.py:6-15`) maneja `ValueError`/`TypeError` y devuelve `None` — buen patrón defensivo.
- Uso de `.get(clave, 0)` para recursos ausentes (`funciones.py:80-107`) — evita `KeyError`.
- Comentarios que explican decisiones no obvias (el `+2` para redondeo por exceso en `funciones.py:110`, la razón del `strip()` en `main.py:88`).

Mejorables:

- El retorno `[True]` / `[False, msg]` (listas heterogéneas) obliga a indexar `resultado[0]`/`resultado[1]`; una tupla o excepciones sería más claro y menos propenso a errores.
- `except:` desnudo en `funciones.py:173` y `funciones.py:275` captura cualquier excepción (incluido `KeyboardInterrupt`); mejor `except (ValueError, TypeError):`.
- Variables globales de estado: `usuarios` se carga una vez al arrancar (`main.py:36-37`) y se muta en memoria; si dos procesos escriben, se pierde consistencia. Aceptable para el alcance, pero conviene releer el archivo antes de escribir.
- `debug=True` en producción (`main.py:158`) expone la consola del depurador Werkzeug — por eso el `TypeError` mostró un traceback HTML completo con PIN. Fuera de desarrollo debería ir en `False`.

## Dimensión 5 — Datos y persistencia

- Persistencia en JSON: `usuarios.json` (dict `usuario → clave`) y `recursos.json` (lista de eventos). Verifiqué que ambos se crean automáticamente si faltan (`main.py:17-23`) — buen detalle, y necesario porque el estudiante notó en el último commit que la BD no se le había subido.
- **La lista de eventos se mantiene ordenada por `(momentoInicial, momentoFinal)`** mediante inserción con búsqueda binaria (`funciones.py:118-141`), en lugar de reordenar cada vez. Es una decisión de diseño acertada y bien ejecutada; la probé y mantiene el orden.
- Las contraseñas se guardan **en texto plano** (`crearUsuario.py:38`, `usuarios.json`). Para primer año es esperable, pero conviene mencionar que en un sistema real se hashean.
- **`requirements.txt` está codificado en UTF-16 LE con CRLF** (probablemente generado con `pip freeze > requirements.txt` desde PowerShell). Esto hace que se vea con caracteres nulos intercalados y que `pip install -r requirements.txt` pueda fallar. Debería regenerarse en UTF-8 (`pip freeze | Out-File -Encoding utf8`, o simplemente reescribirlo a mano). Fue la causa de que el bot marcara "sin `requirements.txt`".

## Dimensión 6 — Informe (`report.md`)

El informe (1536 palabras, por debajo del mínimo de 2000) es **honesto y coincide bien con el código**. Describe correctamente la arquitectura, las tres decisiones algorítmicas (inserción binaria, obtención de colisiones, barrido de demanda máxima) y las reglas de negocio. No infla funcionalidades: todo lo que enumera existe en el código.

Discrepancias:

- El informe da como "evento válido" de ejemplo (`report.md:111-118`) exactamente el que en el estado subido produce un `TypeError` (`main.py:43`). Es decir, **el ejemplo canónico del informe no corre** en el código entregado. Esto sugiere que el estudiante no reejecutó el flujo completo después de su último cambio en `cargarEventos`.
- `report.md:64` afirma la regla "solo se permite un piano o una batería, no más de uno"; el código la implementa mal para el caso de cero (Dimensión 3, bug 2). El informe describe la *intención* correcta; el código no la cumple.
- `report.md:100` pide "Python 3.8+", pero `datetime.fromisoformat` con el formato usado funciona; sin embargo el `requirements.txt` en UTF-16 dificulta la instalación tal como se documenta.

El informe **no** exagera con "demuestra"/"prueba": describe las validaciones sin afirmar haberlas testeado exhaustivamente, lo cual es coherente con que dos bugs sobrevivieron.

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **ambicioso y técnicamente sólido en su núcleo**, empañado por un bug de integración de un solo carácter que, irónicamente, deja el subsistema principal completamente inoperante en el estado subido. El estudiante fue más allá de lo pedido: montó una arquitectura cliente-servidor real con Flask, implementó autenticación, y — lo más valioso — resolvió el problema de planificación con algoritmos genuinamente buenos: inserción ordenada por búsqueda binaria, obtención de colisiones por búsqueda binaria, y un **barrido de línea (sweep-line) para calcular la demanda máxima de cada recurso** considerando solapamientos. Verifiqué por ejecución que toda esa lógica es correcta. El historial de commits muestra desarrollo incremental real y comprensión de lo que estaba construyendo.

El contraste es agudo: la lógica de negocio pasa 11/11 de mis pruebas aisladas, pero un `len(a != b)` en vez de `len(a) != len(b)` en `main.py:43` hace que `GET /eventos` y todo lo que dependa de `cargarEventos()` devuelva HTTP 500. Con esa única línea corregida, ejercité el sistema completo end-to-end y funcionó: crear, detectar colisiones, validar, eliminar, rango. El segundo bug (regla de piano/batería en Sala 23 y Cuarto del Rock que rechaza el caso "cero") es menor en tamaño pero deja dos de cuatro locales inutilizables.

**Principal fortaleza:** el diseño algorítmico del planificador — búsqueda binaria para inserción/colisiones y barrido de demanda máxima — está correctamente pensado y correctamente implementado; es un nivel de sofisticación notable para primer año.

**Principal área de mejora:** **probar el flujo completo después de cada cambio antes de entregar.** Los dos bugs son exactamente lo que un solo `GET /eventos` y un solo evento en Sala 23 habrían revelado en segundos. La lección no es de algoritmos (esos están bien) sino de disciplina de verificación: reejecutar el ejemplo del propio informe antes de subir.
