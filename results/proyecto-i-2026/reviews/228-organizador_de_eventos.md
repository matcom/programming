# Revisión detallada — Issue #228

- **Estudiante:** Manuel Mendoza Abreu
- **Grupo:** C-121
- **Repositorio:** https://github.com/ManuelMendozaIceWyvern/Organizador_de_eventos
- **Proyecto:** Organizador de Eventos del Complejo Deportivo "Antuan Jurantes"
- **Clonado en:** `.playground/proyecto1-eval/repos/228-organizador_de_eventos`

---

## 1. Qué hace el programa

A diferencia de la mayoría de las entregas (apps de consola), esta es una
**aplicación web construida con Streamlit**, no una app de menú por `input()`.
El punto de entrada es `main.py:76` (`main()`), que se ejecuta con
`streamlit run main.py`. La app modela la reserva de instalaciones deportivas
del complejo "Antuan Jurantes".

El flujo se organiza con un `selectbox` en la barra lateral (`main.py:39`) con
tres vistas:

- **Eventos** (`main.py:41`): lista los eventos reservados (`data.json`) y el
  almacén de recursos (`res.json`) como tablas de pandas.
- **Agregar Evento** (`add_event.py:8`): un asistente en cascada — el usuario
  elige deporte → terreno → pelota → árbitro → silbato, y cada paso se valida
  contra el catálogo de recursos; luego elige fecha/hora y agenda.
- **Eliminar Evento** (`del_event.py:7`): muestra los eventos y permite borrar
  uno con un slider.

La lógica de dominio es notablemente rica para un primer año: hay reglas de
compatibilidad (un árbitro solo arbitra su deporte, cada árbitro exige "su"
silbato — `add_event.py:43,53,63,73`), cálculo de hora de fin según la duración
por deporte (`add_event.py:79-82`), horario de apertura 8:00–18:00
(`add_event.py:84`) y detección de colisiones de reservas
(`verificar_fecha.py:7`).

## 2. Organización del código

**Muy buena para el nivel.** El proyecto está **dividido en módulos** con
responsabilidad clara, en lugar de un `main.py` gigante:

- `main.py` — enrutado/UI principal.
- `add_event.py` — alta de eventos.
- `del_event.py` — baja de eventos.
- `verificar_fecha.py` — verificación de colisiones.
- `models.py` — clases `Event` y `Resource`.

Los nombres de funciones y variables son descriptivos (`nombre_evento`,
`deporte_terreno`, `verificar_reserva`). Hay clases de dominio en `models.py:6`
y `models.py:27` con `to_dictionary()`, lo cual es un patrón correcto de
serialización. Esta separación es superior a la media esperada en 1er año.

Detalles menores:
- `Resource.from_dictionary` (`models.py:20`) construye `recurso` pero **no lo
  retorna** — método muerto/incompleto (nunca se usa, así que no rompe nada).
- Todos los módulos hacen `from datetime import *` (`main.py:2`, etc.), que
  contamina el namespace; mejor `from datetime import datetime, date, time, timedelta`.

## 3. Corrección funcional (basada en ejecución real)

Monté un entorno aislado con `uv` (Python 3.12) e instalé `streamlit` y
`pandas` (no había `requirements.txt`). Corrí la app con
`streamlit run main.py --server.headless true` (arranca, HTTP 200) y además la
ejercité con el framework `streamlit.testing.v1.AppTest`, que ejecuta el script
igual que el runtime real y captura excepciones.

**Hallazgo principal — bug bloqueante de ruta.** El código abre los JSON con
rutas *hardcodeadas* `proyect1/main/data.json` y `proyect1/main/res.json`
(`main.py:45,59`; `add_event.py:33,97,103`; `del_event.py:12,29,37`;
`verificar_fecha.py:17`), **pero esos archivos están en la raíz del repo**
(`data.json`, `res.json`). Esa carpeta `proyect1/main/` no existe. Resultado al
correr tal cual se clona:

- **Vista "Eventos":** no lanza excepción porque los `open()` están envueltos en
  `try/except (FileNotFoundError, json.JSONDecodeError)` (`main.py:53,63`), pero
  por eso **muestra tablas vacías** — nunca ves tus eventos reales guardados.
- **Vista "Agregar Evento":** `Traceback` real —
  `FileNotFoundError: [Errno 2] No such file or directory: 'proyect1/main/res.json'`
  en `add_event.py:33` (no hay `try/except` ahí).
- **Vista "Eliminar Evento":** `Traceback` real —
  `UnboundLocalError: cannot access local variable 'events'` en `del_event.py:24`.
  Cadena: el `open()` falla dentro del `try` (`del_event.py:12`), el `except`
  descarta la excepción pero `events` nunca se asignó, y la línea siguiente
  (`if len(events) > 1`) usa esa variable inexistente.

**Verificación de que la lógica sí funciona.** Para aislar el bug, creé la
carpeta `proyect1/main/` y copié ahí los JSON. Re-ejecutando con `AppTest`:

- "Agregar Evento" renderiza sin excepción los `selectbox` de deporte, terreno,
  pelota y árbitro. La cascada de validación **funciona correctamente**: al
  elegir "Partido de futbol" el terreno se filtra a terrenos de futbol; si eliges
  un terreno incompatible aparece `st.error("No puede realizar este evento en
  este terreno!")`; recorriendo terreno→pelota→árbitro de futbol la app avanza
  paso a paso como se espera.
- "Eliminar Evento" renderiza el slider sin excepción.

Es decir: **el programa está bien construido y su lógica es correcta; el único
motivo por el que no funciona al clonarlo es el prefijo de ruta equivocado.**

**Otros puntos de corrección:**
- `verificar_fecha.py:12-15` extrae la hora parseando caracteres por posición
  del string ISO (`evento["inicio"][11]+[12]+[14]+[15]`). Funciona porque
  `isoformat()` es de longitud fija, pero es frágil; `datetime.fromisoformat()`
  sería robusto. Además solo compara **horas enteras** (ignora minutos), así que
  la detección de colisión es aproximada.
- La verificación de colisión solo dispara si coincide el **mismo nombre de
  evento** (`verificar_fecha.py:22`), no si dos eventos distintos comparten
  terreno/árbitro/pelota. La regla real ("un evento que utiliza estos recursos")
  no se comprueba a nivel de recurso.
- No se controla el caso de `date_input`/`time_input` fuera de rango más allá del
  chequeo de horario (razonable para el nivel).

## 4. Buenas prácticas de Python (nivel principiante)

- **Legibilidad e indentación:** consistentes y limpias. Buen uso de f-strings
  implícitas vía `st.write`, comprensiones de lista para filtrar recursos
  (`add_event.py:38,48,58,68`) — idiomático y correcto.
- **Manejo de errores:** hay `try/except` en las vistas de lectura
  (`main.py:53`, `del_event.py:20`) — bien —, pero **falta** en los `open()` de
  `add_event.py:33` y en la ruta que produce el `UnboundLocalError` de
  `del_event.py`. La lección: un `except` que silencia el error deja variables
  sin inicializar y explota más adelante.
- **Duplicación:** el bloque de leer `data.json` a un DataFrame aparece casi
  idéntico en `main.py:44-52` y `del_event.py:11-19`; podría extraerse a una
  función `cargar_eventos()`. Menor.
- El if-else de duración por deporte (`add_event.py:17-31`) usa `if`
  independientes en vez de `elif`; funciona pero un `dict` o `elif` sería más
  limpio.

## 5. Datos y persistencia

Persiste en dos JSON: `res.json` (catálogo estático de recursos) y `data.json`
(eventos agendados). El alta hace lectura-append-escritura con `indent=4`
(`add_event.py:97-104`) y la baja reescribe la lista (`del_event.py:29,37`). El
modelo de datos es razonable (lista de diccionarios). La clase `Event`
serializa fechas con `isoformat()` (`models.py:37`), correcto. El problema no es
el diseño de datos sino la ruta con la que se accede a los archivos (ver §3).

## 6. Informe (`report.md`)

El `report.md` describe fielmente el **dominio** (deportes soportados, horario
8–18, árbitros y sus silbatos, duraciones por deporte) y coincide con lo que el
código implementa — **no sobreestima features**; de hecho todas las reglas que
narra están en el código. Lo que **omite** es cualquier instrucción de **uso**:
no dice que es una app de Streamlit ni cómo ejecutarla (`streamlit run main.py`),
no menciona dependencias, y no advierte del prefijo `proyect1/main/` en las
rutas (que es justo lo que impide correrla). El `README.md` es casi idéntico al
informe. Sería muy valioso añadir una sección "Cómo ejecutar" con las
dependencias y el comando.

---

## Síntesis

Trabajo **por encima del promedio para un primer año**: elige Streamlit por
iniciativa propia, separa el código en módulos con responsabilidad única, usa
clases de dominio, y modela una lógica de negocio genuinamente rica (cascada de
validación deporte→terreno→pelota→árbitro→silbato, horarios, colisiones). La
lógica es correcta al ejecutarla. El defecto que lo tumba es **un solo bug de
ruta**: los archivos JSON se buscan en `proyect1/main/` pero viven en la raíz,
lo que hace fallar dos de las tres vistas nada más clonar. Corrigiendo ese
prefijo (o usándolo relativo al script), la app funciona de punta a punta.
Prioridades de mejora: (1) arreglar las rutas de los JSON, (2) inicializar
`events` antes del `try` en `del_event.py` para evitar el `UnboundLocalError`,
(3) documentar cómo ejecutar en el informe.
