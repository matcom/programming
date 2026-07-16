# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #326
- **Repositorio:** https://github.com/Monica15268/Proyecto-Nuevo
- **Estudiante:** Mónica Alejandra
- **Grupo:** 111
- **Descripción declarada:** Gestor de eventos de juegos (Ludopath — sistema de reservas para una sala de juego, con JSON como base de datos)

---

## Nota metodológica importante

Es una aplicación de **consola** (`input()`/`print()`), no GUI. El punto de entrada es `src/Consola.py` (`if __name__ == "__main__"` en la línea 316), como indica el README. La verificación automática marcó "sin punto de entrada" porque buscaba `main.py`/`app.py`, pero `Consola.py:main()` existe y arranca correctamente.

No hay `report.md`; el informe está en `src/README.md` (unas ~2000 palabras), y lo trato como el informe a efectos de la Dimensión 6.

Una sutileza de ejecución: `archivo_de_json = "guardador.json"` (FuncionesEspecificas.py:4) es una ruta **relativa al directorio de trabajo**. El `guardador.json` con datos semilla vive en la raíz del repo, pero el README manda ejecutar desde `src/`. Al correr desde `src/` el programa no ve ese archivo y arranca con cero reservas (creará uno nuevo en `src/`). Para probar con datos reales copié el semilla a `src/`. Todos los ID/mensajes citados abajo son de ejecuciones reales.

`py_compile` de los cuatro módulos: **OK** (sin errores de sintaxis).

## Dimensión 1 — Qué hace el programa

Menú de consola (Consola.py:14-75) con 8 opciones:

1. **Nueva Reserva** (`nueva_reserva`, Consola.py:108) — pide sala, fecha/hora, duración, descripción; valida y, si pasa, entra a selección de recursos y persiste.
2. **Ver Todas las Reservas** (`listar`, FuncionesEspecificas.py:178).
3. **Buscar Huecos Disponibles** (`buscar_huecos`, Consola.py:159).
4. **Eliminar Reserva** por ID (`eliminar`, FuncionesEspecificas.py:204).
5. **Ver Estado de Salas** (`estado_salas`, Consola.py:264).
6. **Eliminar reservas fuera de fecha** (`eliminar_pasadas`, Consola.py:76).
7. **Salir**.
8. **Ver recursos** — consulta stock en una ventana horaria (`obtention_state`, FuncionesEspecificas.py:395).

El corazón del sistema es el flujo de reserva (`Interfaz.reservar_sala`, bueno.py:214), que encadena validaciones (`verificar_reserva_completa`, bueno.py:177): existencia de sala, duración 1-7h, día laboral (L-V), fecha no pasada y ≤30 días, horario 08:00-17:00, y solapamiento con reservas existentes. Si todo pasa, selecciona recursos, verifica compatibilidad e inventario, y guarda en `guardador.json`.

## Dimensión 2 — Organización del código

Reparto de responsabilidades **claro y con buena intención**:

- `Clasessala.py` — clase `Salas` (estado, inventario, verificación de disponibilidad y compatibilidad) + 5 instancias globales (`one_room`...`five_room`, líneas 137-141).
- `FuncionesEspecificas.py` — persistencia JSON (`load_archives`, `funtion_save`), CRUD de reservas, cálculo de recursos ocupados, selección de recursos con cantidades.
- `bueno.py` — clase `Interfaz`: reglas de negocio (validaciones) y orquestación de la reserva.
- `Consola.py` — capa de presentación / menú.

Esa separación presentación / lógica / datos es más de lo que se ve en muchos proyectos de primer año y merece reconocerse.

Debilidades de organización:

- **Importaciones diferidas por todas partes** para evitar imports circulares (p. ej. `from Clasessala import Salas` dentro de funciones: FuncionesEspecificas.py:141, 228, 311; y `from FuncionesEspecificas import ...` dentro de métodos de `Salas`: Clasessala.py:44, 76). Funciona, pero delata un acoplamiento circular entre módulos (`Clasessala` ↔ `FuncionesEspecificas`) que sería mejor romper.
- `Clasessala.py:3` importa `List` desde `FuncionesEspecificas` pero nunca lo usa en ese módulo.
- Nombres inconsistentes: mezcla inglés/español (`verify_disponibility`, `laboral_day`, `Recurses_inventary`) y typos en identificadores (`Recurses`, `bussiness_hour`, `funtion_save`, `room_exixtence`).
- Estado global mutable (`List`, `actually_id` en FuncionesEspecificas.py:9-10) compartido por todo el sistema; cada función llama `load_archives()` al entrar, lo que recarga el JSON constantemente.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí `Consola.py` desde `src/` alimentando el menú con `printf`. Resultados:

1. **Reserva válida sin recursos** (sala 1, 2026-07-20 10:00, 2h): ✅ "RESERVA CREADA EXITOSAMENTE, ID: 6", persiste en el JSON. Correcto.
2. **Validaciones de entrada** — todas correctas y sin crash:
   - Fecha pasada (2020-01-01) → `❌ No se puede reservar en el pasado`.
   - Domingo (2026-07-19) → `❌ Solo se pueden hacer reservas de lunes a viernes`.
   - Hora 20:00 → `❌ Horario no valido`.
   - Duración 10h → `❌ La duracion debe ser entre 1 y 7 horas`.
   - Fecha basura (`notadate`) → capturada por try/except: `❌ Ocurrió un error inesperado: time data ...` (no revienta, aunque el mensaje es genérico).
   - Opción de menú basura (`zzz`) → `❌ Opción inválida`.
3. **Detección de solapamiento** (dos reservas sala 3, 10:00-12:00 y 11:00-13:00): la segunda se rechaza con `❌ Horario ocupado` y sugiere alternativas reales (08:00, 12:00, 12:30). Correcto — este flujo funciona bien vía `Interfaz.verification` (bueno.py:151) → `search_disponibles_hours`.
4. **Compatibilidad de recursos**: PS5 solo → `❌ Las consolas requieren bocinas` (correcto). PS5+Bocinas → aceptado (correcto).
5. **Eliminar por ID** (opción 4, ID existente): ✅ "Borrado con éxito", el listado posterior lo confirma.
6. **Eliminar pasadas** (opción 6, semilla con 2 reservas de feb-2026): purga ambas con mensaje por cada una. Correcto.

**Bugs reproducidos (fallos del estudiante, no del entorno):**

- **B1 — Opción 5 (Ver Estado de Salas) rompe con Traceback.** `AttributeError: 'Salas' object has no attribute 'capacidad'` en Consola.py:275. El atributo real es `self.capacity` (Clasessala.py:18); el código lo lee como `sala.capacidad` en Consola.py:171 y 275. La opción 5 es **inusable**.
- **B2 — Opción 3 (Buscar Huecos) también cae por el mismo `sala.capacidad`** (Consola.py:171), capturado por el `try/except` general (Consola.py:261), así que muestra `❌ Error: 'Salas' object has no attribute 'capacidad'` y vuelve al menú. La funcionalidad no llega a ejecutarse.
- **B3 — Método mal nombrado.** Consola.py:205 y FuncionesEspecificas.py:79 llaman `interfaz.buscar_huecos_disponibles(...)`, pero el método definido es `search_disponibles_hours` (bueno.py:279). Si se corrigiera B2, la opción 3 caería aquí a continuación. La ruta de FuncionesEspecificas.py:79 es código muerto (no se alcanza porque el solapamiento se detecta antes, en `verification`).
- **B4 — `KeyError` latente en el render de huecos.** Consola.py:227 lee `hueco['hora_fin']`, pero `search_disponibles_hours` solo llena `fecha`, `hora_inicio`, `duracion` (bueno.py:311-314). Aunque se arreglen B2 y B3, la opción 3 seguiría rompiendo aquí.
- **B5 — Opción 8 (Ver recursos) reporta stock incorrecto.** Hice una reserva con 1x Bocinas en la ventana 10:00-12:00 y luego consulté el stock para esa misma ventana: reportó `Bocinas | Cantidad: 2` (inventario completo). El descuento no ocurre porque `obtention_state` (FuncionesEspecificas.py:413) lee `valor.get('Recurses_inventary', [])`, pero las reservas guardan los recursos bajo la clave `'Recursos'` (FuncionesEspecificas.py:154). El stock **nunca** se decrementa; la opción siempre muestra inventario lleno.

Resumen de corrección: el flujo **principal** (crear/validar/listar/eliminar reservas, detección de solapamiento, compatibilidad de recursos) funciona sólidamente. Fallan 2 de las 8 opciones del menú (5 con traceback, 3 con error capturado) y la opción 8 da datos erróneos.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **Manejo de errores demasiado amplio:** varios `try/except:` desnudos (Clasessala.py:37, FuncionesEspecificas.py:248, 281) que se tragan cualquier excepción con `continue`. Ocultan bugs; mejor capturar excepciones concretas (`ValueError`, `KeyError`).
- **f-string con comillas anidadas** en Clasessala.py:64 (`f'Sala {u["sala"]}'` dentro de comillas dobles) — funciona en 3.12 pero es frágil; conviene alternar comillas.
- `format` como nombre de variable (FuncionesEspecificas.py:47) sombrea el builtin `format`.
- Uso correcto de `Counter`, `timedelta`, comprensiones — buen manejo idiomático en varios puntos (p. ej. FuncionesEspecificas.py:342-343, Clasessala.py:106).
- Emojis en toda la interfaz: hacen la consola amena, aunque dependen de que la terminal soporte UTF-8.
- Código muerto / duplicado: la impresión "Buscando horarios alternativos..." aparece dos veces seguidas (FuncionesEspecificas.py:69-70).

## Dimensión 5 — Datos y persistencia

Modelo simple y adecuado: un dict `{id: reserva}` serializado a `guardador.json` bajo la clave `guardador`, más un contador `identidad` para IDs autoincrementales (FuncionesEspecificas.py:26-27). Cada reserva es un dict plano con `Sala`, `Descripcion`, `Inicio`, `Fin`, `Duracion`, `Recursos`, `Estado`. `load_archives` maneja el caso de archivo inexistente/corrupto (FuncionesEspecificas.py:21-23). Es una elección razonable y honesta para el problema.

El punto débil ya señalado (B5): la **inconsistencia de clave** entre cómo se escribe (`'Recursos'`) y cómo se lee en `obtention_state` (`'Recurses_inventary'`) rompe silenciosamente el cálculo de stock. También la ruta relativa del JSON (FuncionesEspecificas.py:4) hace que "dónde ejecutas" cambie qué base de datos ves.

## Dimensión 6 — Informe (`src/README.md`)

El informe está bien escrito, entusiasta y cubre motivación, características, tecnologías, dificultades y aprendizajes. Pero **sobredimensiona** varias funcionalidades frente al código real:

- "búsqueda inteligente... sugiere automáticamente los próximos 15 horarios" — `search_disponibles_hours` corta en 5 huecos (`if len(huecos) >= 5: return huecos`, bueno.py:317). La funcionalidad de menú que la expone (opción 3) además **no ejecuta** por B2/B3/B4.
- "Comprobación de Recursos... verifica si los recursos necesarios están disponibles" — la verificación en el momento de reservar sí existe (`verify_disponibility`), pero la consulta de stock de la opción 8 está rota (B5) y siempre reporta inventario lleno.
- "sistema de evaluación / calificación de experiencia", "recordatorios de medidas sanitarias", "tutoriales interactivos", "métodos de pago" — no existen en el código (algunos el propio informe los lista como "Futuras Mejoras", pero otros aparecen redactados como si ya estuvieran).
- Menciona `Collections`, `DateTime` y "otras librerías"; `requests` figura en `pyproject.toml` pero no se usa en ningún módulo.

El informe describe bien el **diseño pretendido**; conviene ajustarlo a lo que hoy corre de verdad.

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **ambicioso y bien estructurado** para primer año: separa presentación, lógica de negocio y persistencia en módulos coherentes, y el flujo central de reservas es genuinamente sólido —lo ejecuté de punta a punta y validaciones, detección de solapamientos y reglas de compatibilidad de recursos funcionan como se espera, sin reventar ante entradas inválidas. El problema es que **2 de las 8 opciones del menú no funcionan** (opción 5 con traceback, opción 3 con error capturado) por un mismo error de nombre de atributo fácil de arreglar (`capacidad` vs `capacity`), y la opción 8 muestra datos incorrectos por una clave de diccionario mal escrita (`Recurses_inventary` vs `Recursos`). Son bugs de "última milla" —el diseño está bien, la conexión final falló— pero afectan la experiencia real y desmienten parte de lo que promete el informe.

- **Principal fortaleza:** arquitectura modular clara (presentación / lógica / datos) con un motor de validación de reservas que funciona de verdad, robusto ante entradas inválidas.
- **Principal área de mejora:** corregir los bugs de conexión que dejan muertas las opciones 3, 5 y 8 (`sala.capacidad`→`sala.capacity`, `buscar_huecos_disponibles`→`search_disponibles_hours`, añadir `hora_fin` a los huecos, y `'Recurses_inventary'`→`'Recursos'` en `obtention_state`), y alinear el informe con lo que el programa realmente hace hoy.
