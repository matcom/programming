# Reporte detallado — Proyecto I

- **Issue:** #232
- **Estudiante:** Carla Argote Hernández
- **Grupo:** C111
- **Repositorio:** https://github.com/Carla-inix/Arcane-Gestor-de-Eventos
- **Descripción del issue:** "gestor de eventos de un local de videojuegos en el que puedes reservar salas y juegos".

---

## Resumen de la ejecución dinámica

El proyecto no tiene dependencias externas (`report.md:126-128` declara solo Python 3.10+),
así que se ejecutó directamente con `python3` (Python 3.14.4), sin `uv venv`. Punto de
entrada: `main.py` (`main.py:14-15`), que llama a `cargar_estado()` y luego a `menu_principal()`.

Corridas realizadas (stdin piped + `timeout`):

1. **Arranque + inputs inválidos + guardar/salir** — `printf '9\n0\nfoo\n2\n6\n'`.
   Arranca limpio, imprime el encabezado del lounge y el menú. Las opciones fuera de rango
   responden `Opción inválida. Elige del 1-6` sin romperse (`menu.py:112-113`). Cierra por
   EOF cuando se agota el stdin (normal).

2. **Suscripción + reserva completa (sala PCs)** —
   `printf '4\nCarla\n12345678901\n\n1\n3\n2\n2\n2\n2\n1\n2\n2\nsi\n\n6\n'`.
   Registro OK (`suscripcion.py:14-66`), selección de sala PCs, cantidad de personas, sillas,
   audífonos, fecha (día siguiente), hora 09:00, duración 2h. Salida real:
   `Costo total: 2000$` y `Reserva realizada con éxito!❤`. Al elegir 6 se imprime
   `Estado guardado` y `Hasta pronto❤`.

3. **Round-trip de persistencia** — tras la corrida 2, se reinició el programa y en
   **Mis Reservas** apareció la reserva persistida:
   ```
   1. Arca: PCs
       Horario:  07-07-2026 | 09:00 - 11:00
       Duración: 2 hora(s)
       Personas: 2
       Monto: 2000$
   ```
   El JSON persistido conserva `suscrito: True`, `user_actual: 12345678901` y 1 reserva
   activa. La serialización datetime→str→datetime funciona (`persistencia.py:14-132`).

4. **Tienda** — `printf '4\n...\n3\n1\n1\n1\nsi\n...'`. Compra de FIFA 24 x1:
   `Costo total: 350$` → `Compra realizada con éxito!❤` (`tienda_arcane.py:24-108`).

5. **Cargar `estado_ejemplo.json`** — `cp datos/estado_ejemplo.json datos/estado_app.json`.
   Carga sin errores y en Mis Reservas muestra la reserva de Realidad Virtual
   `26-03-2026 | 16:00 - 18:00` (`persistencia.py:85-132`).

6. **Ofertas sin suscripción** — opción 5 responde `Debes suscribirte primero`
   (`ofertas.py:19-22`), sin traceback.

7. **Validación de carnet** — letras → `Tu ID solo puede tener números enteros`;
   `123` → `El ID debe tener exactamente 11 dígitos`; `12345678901` → registro OK
   (`suscripcion.py:33-57`).

**Tracebacks observados:** únicamente `EOFError: EOF when reading a line` en `input()`
al agotarse el stdin piped — es el comportamiento normal descrito en la rúbrica
(el programa está esperando input), **no un fallo del programa**. No se observó ningún
crash lógico en ninguna ruta ejercitada.

---

## 1. Qué hace el programa

Aplicación de consola que gestiona las reservas de **Arcane Gaming Lounge**, un local de
videojuegos con cuatro salas (dos de Consolas, una de PCs, una de Realidad Virtual;
`datos.py:1-57`). El flujo, según el código:

- El usuario se **suscribe** con nombre y carnet de 11 dígitos (`suscripcion.py:14-66`).
  Sin suscripción no puede reservar ni comprar (`menu.py:46-63`).
- **Reserva** una sala eligiendo personas, equipos (mandos, sillas/sofás, audífonos,
  visores/mandos/caminadoras RV), juegos (en salas de consolas), fecha y hora, con
  cálculo de costo (1000$/hora) y aplicación de cupón (`reservas.py:15-414`).
- Consulta/cancela sus reservas (`mis_reservas.py:7-118`), compra juegos en la
  **Tienda Arcane** (`tienda_arcane.py`), y ve **ofertas/cupón** por acumular reservas
  (`ofertas.py`).
- Al salir, todo el estado se serializa a `datos/estado_app.json` y se recarga al reiniciar
  (`persistencia.py`).

Punto de entrada: `python main.py` desde la raíz del repo. El menú principal está en
`menu.py:26-113`.

## 2. Organización del código

**Excelente para primer año.** El proyecto está dividido en un paquete `funciones/` con 13
módulos de responsabilidad única, no un `main.py` gigante:

- `estado.py` — estado global compartido (tiempo, reservas activas/historial, juegos
  reservados; `estado.py:1-13`).
- `inputs.py` — helper reutilizable `pedir_numero()` con validaciones integradas
  (`inputs.py:2-32`), usado en todos los módulos: buena eliminación de duplicación.
- `datos.py` — catálogo estático de salas y juegos.
- `horarios_r.py` / `recursos_r.py` / `juegos.py` — lógica de tiempo, inventario y juegos.
- `reservas.py` — flujo de reserva (el módulo más grande, 413 líneas).
- `mis_reservas.py`, `suscripcion.py`, `tienda_arcane.py`, `ofertas.py` — features.
- `persistencia.py` — serialización JSON.
- `menu.py` — enrutamiento.

Los nombres de funciones y variables son claros y en español coherente
(`validar_dispo_recursos`, `buscar_prox_horario`, `usuario_ocupado`). Uso adecuado de
imports relativos (`from . import estado`). El dominio se modela con diccionarios en vez de
clases — perfectamente razonable a este nivel; no penalizable. Buen uso de `set` para
`juegos_reservados` (`estado.py:13`) y de funciones puras como
`solapamiento_reserva()` (`horarios_r.py:81-82`).

## 3. Corrección funcional (basada en ejecución real)

El programa **funciona de punta a punta**. Todas las rutas ejercitadas se comportan
correctamente (ver "Resumen de la ejecución"): suscripción, reserva completa con cálculo de
costo, tienda, ofertas, mis reservas, carga del archivo de ejemplo y round-trip de
persistencia. La validación de entradas es sólida:

- `pedir_numero()` rechaza vacío, no-dígitos, ceros a la izquierda y valores fuera de rango
  (`inputs.py:10-31`).
- El carnet valida longitud exacta de 11 y unicidad (`suscripcion.py:43-57`).
- Opciones de menú inválidas se manejan sin crash (`menu.py:112-113`).
- Restricciones de negocio verificadas en ejecución: co-requisito mando RV ↔ caminadora
  (`reservas.py:240-242`), exclusión mutua sillas/sofás (`reservas.py:104-116`), tope de
  copias por juego y compras/día en la tienda (`tienda_arcane.py:29,76`).

**No se observó ningún traceback lógico.** El único `EOFError` visto proviene de agotar el
stdin en las pruebas automatizadas, no de un defecto del programa.

**Hallazgos menores (no rompen la ejecución):**

1. **`estado_app.json` versionado como `{}`** (`git ls-files` lo lista; contenido `{}`).
   En el primer arranque real esto dispara el mensaje
   `Archivo de estado corrupto o incompatible. Iniciando desde cero`
   (`persistencia.py:136-137`), impreso **dos veces** porque `cargar_estado()` se invoca en
   `main.py:7` y otra vez en `menu.py:27`. Es cosmético (la excepción se captura y el
   programa arranca limpio), pero da una impresión inicial de error. Además contradice el
   `report.md:186` que afirma que el archivo está en `.gitignore` (el `.gitignore:4` sí lo
   lista, pero el archivo ya estaba trackeado antes de añadir la regla, así que sigue en el
   repo). Sugerencia: `git rm --cached datos/estado_app.json` para que deje de versionarse, y
   tratar `{}` / archivo vacío como "no hay estado" en `cargar_estado()` para evitar el
   mensaje de "corrupto".

2. **Doble llamada a `cargar_estado()`** (`main.py:7` y `menu.py:27`): redundante. Basta con
   una. No causa fallo, pero es la razón del mensaje duplicado.

## 4. Buenas prácticas de Python (nivel principiante)

Muy por encima de lo esperado:

- Indentación consistente, f-strings idiomáticas, bucles claros.
- `try/except` acotado en la carga de estado (`persistencia.py:136`) y en el parseo de
  horas del modo prueba (`menu.py:89-96`) — captura excepciones específicas
  (`KeyError, ValueError, json.JSONDecodeError`), no un `except:` desnudo.
- Comentarios útiles y en español que explican el *porqué*, no lo obvio.
- Reutilización de código real vía `inputs.pedir_numero()` en lugar de repetir validaciones.
- Uso de estructuras adecuadas: `set` para reservados, `dict` para recursos/stock.

Punto de mejora idiomático: hay bastante estado global mutable en `estado.py` y
`suscripcion.py` (`global suscrito, user_actual`). A este nivel es aceptable y coherente,
pero conviene mencionar que agrupar el estado en una clase o pasarlo como parámetro
reduciría el acoplamiento a futuro.

## 5. Datos y persistencia

Sólida. La serialización maneja el caso no trivial de `datetime` (que JSON no soporta):
convierte a texto con `strftime` al guardar y reconstruye con `strptime` al cargar
(`persistencia.py:20-24, 104-105`); los objetos sala se guardan por nombre y se re-resuelven
contra el catálogo (`persistencia.py:99-106`). El round-trip se verificó en ejecución
(reserva creada → guardada → recargada intacta). El manejo de estado corrupto degrada con
gracia en vez de crashear (`persistencia.py:136-137`). Estructuras de datos razonables en
todo el sistema.

## 6. Informe (`report.md`)

Muy completo y bien escrito: índice, tablas de salas/recursos, restricciones con fragmentos
de código, estructura del proyecto, instrucciones de ejecución y desafíos opcionales. En
general **describe fielmente** lo que el código hace. Dos discrepancias:

1. **Modo de prueba (opción 7) — sobreestimado.** `report.md:210-212` lo lista como feature
   implementada y accesible desde el menú principal. Pero en el código shippeado
   `MODO_DEBUG = False` (`menu.py:10`), así que la opción 7 **no se muestra ni es accesible**
   (`menu.py:41-42, 79`). El código del modo prueba existe pero está desactivado. Sugerencia:
   o bien aclarar en el informe que es una herramienta de desarrollo desactivada por defecto,
   o exponerla (p.ej. una opción de menú oculta).

2. **`estado_app.json` y `.gitignore`.** `report.md:186` afirma que el archivo está ignorado;
   en la práctica sigue versionado con contenido `{}` (ver Hallazgo 1). Menor.

Fuera de eso, el informe no infla features: las restricciones de co-requisito y exclusión
mutua, los pools de recursos y la persistencia que describe **sí existen y funcionan**.

---

## Valoración global (interna)

Trabajo **sobresaliente para un primer proyecto de primer año**. Arquitectura modular
limpia, lógica de negocio genuinamente compleja (pools de recursos compartidos, conflictos
de horario, restricciones de co-requisito/exclusión, cupones), validación de entradas
robusta y persistencia JSON correcta con serialización de fechas. Corre de punta a punta sin
un solo crash lógico en las rutas probadas. Los únicos hallazgos son cosméticos: el
`estado_app.json` versionado que dispara un mensaje de "corrupto" duplicado, y el modo de
prueba documentado pero desactivado. Principal fortaleza: organización + profundidad de la
lógica. Principal mejora: pulir el arranque (persistencia versionada) y alinear el informe
con lo realmente accesible.
