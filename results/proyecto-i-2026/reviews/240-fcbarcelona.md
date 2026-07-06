# Reporte detallado — Issue #240 · `mr-sedal/fcbarcelona`

- **Estudiante:** Javier Márquez Zaldívar
- **Grupo:** C-121
- **Repositorio:** https://github.com/mr-sedal/fcbarcelona
- **Descripción del issue:** gestionar la disponibilidad de los jugadores del FC Barcelona para los partidos de la temporada 25/26, y la planificación de estos.
- **Archivos:** `main.py` (733 líneas), `doctor.py` (320 líneas), `datos.json`, `report.md`, `requirements.txt` (vacío).

---

## 1. Qué hace el programa

Es una aplicación de consola en dos ejecutables complementarios que comparten el estado en `datos.json`:

- **`main.py`** — gestor de partidos. Menú principal (`main.py:71`) con 5 opciones: listar partidos con detalle bajo demanda (`main.py:91`), añadir partido con rival/fecha y una convocatoria de hasta 18 jugadores (`main.py:133`), eliminar partido por ID (`main.py:371`), modificar rival/fecha/convocados de un partido (`main.py:396`), y guardar+salir (`main.py:723`).
- **`doctor.py`** — gestor de lesionados. Menú análogo (`doctor.py:51`): listar, añadir lesionado (dorsal + tipo + semanas de baja, con fecha de regreso calculada, `doctor.py:76`), quitar (`doctor.py:132`), modificar (`doctor.py:168`), guardar+salir (`doctor.py:293`).

El punto de entrada es código a nivel de módulo (no hay `if __name__ == "__main__"`): al ejecutar `python3 main.py` o `python3 doctor.py` desde la raíz del repo se carga `./datos.json`, se convierten las fechas string→`date` (`main.py:17-25`, `doctor.py:20-25`) y arranca el bucle de menú. No hay dependencias externas (`requirements.txt` vacío); solo `datetime` y `json` de la stdlib.

El dominio es un modelo recurso/evento coherente: jugadores como activos, partidos como eventos, con reglas reales (un partido por fecha, ventana de temporada, tope de 18 convocados, lesionados no convocables). Hay una integración cruzada real entre ambos programas mediante el JSON compartido.

## 2. Organización del código

- **Sin funciones ni clases más allá de `validar_fecha`** (`main.py:6`, `doctor.py:11`, duplicada literalmente en ambos archivos). Toda la lógica vive dentro del bucle `while` del menú, con bloques `if (bandera == N)` anidados. Es el patrón típico de 1er año; funciona pero es difícil de mantener.
- **Anidamiento profundo.** El bloque de "añadir partido" llega a 6-7 niveles de indentación (`main.py:260-285`), con `while` de convocatoria dentro de `while` de operación dentro de `while` de partido. Legible con esfuerzo, pero muy denso.
- **Nombres de variables buenos y expresivos.** `la_fecha_tiene_sentido`, `los_convocados_tienen_sentido`, `el_jugador_esta_en_el_plantel`, `cambios_en_disponibilidad` — se entiende la intención sin leer el detalle. Es una fortaleza clara del código.
- **Comentarios abundantes y útiles** que explican cada bloque (`main.py:40-56`, `doctor.py:35-46`). Muy por encima de lo esperado para el nivel.
- **Duplicación evidente:** el sub-menú de convocatoria (añadir/quitar/listar/guardar) está escrito dos veces casi idéntico — una en "añadir partido" (`main.py:224-365`) y otra en "modificar partido" (`main.py:559-709`). Una función `gestionar_convocatoria(...)` habría eliminado ~140 líneas.
- **Estado global.** `plantel`, `disponibles`, `lesionados`, `partidos` y `data` son globales manipulados directamente (`main.py:30-33`). Aceptable al nivel, pero acopla todo.

## 3. Corrección funcional (basada en ejecución real)

Ejecuté ambos programas con `printf '...' | timeout N python3 <archivo>.py` desde la raíz, restaurando `datos.json` desde copia entre pruebas. **Ambos compilan** (`python3 -m py_compile` OK). Resultados observados:

**`main.py`:**
- **Arranca correctamente.** Mensaje de bienvenida + menú (`main.py:36,73`).
- **Opción 1 (listar):** avisa "No hay partidos establecidos" cuando está vacío; con partidos, imprime ID/rival/fecha/estado y permite pedir detalles por ID. ✔
- **Opción 2 (añadir):** flujo completo verificado. Rechaza "FC Barcelona" como rival con el easter egg (`main.py:143`). Valida fechas: rechazó `32/13` y `30/2` como inexistentes (`main.py:168`), rechazó una fecha pasada (`main.py:182`) y rechazó fechas ≥ 2026-08-01 (`main.py:185`). Convoqué jugadores, pedí la lista (op 3), guardé incompleto (< 18) → estado `"!"` (`main.py:348`). ✔
- **Conflicto de fecha duplicada:** al intentar un segundo partido el 15-07 con uno ya existente, respondió "Ya existe un partido contra (Sevilla) en esta misma fecha" (`main.py:174`). ✔
- **Opción 3 (eliminar):** eliminó el partido `20260715` ("Partido removido exitosamente") y quedó lista vacía; ID inexistente → error controlado. ✔
- **Opción 4 (modificar):** cambié el rival de "Espanyol" a "Betis", persistió correctamente y reasignó ID (`main.py:718`). ✔
- **Opción 5 (guardar y salir):** persiste a `datos.json` convirtiendo `date`→string (`main.py:723-732`). Verifiqué el JSON resultante: partido con id `20260715`, rival, fecha `15-07-2026`, estado `!`, convocados correctos. ✔
- **Validación de entradas:** el menú principal maneja `ValueError` y números fuera de rango sin romperse (`main.py:79-88`). ✔

**`doctor.py`:**
- **Arranca correctamente.** Al iniciar depura lesionados ya recuperados (`doctor.py:37-46`). ✔
- **Opción 2 (añadir lesionado):** añadí a Lamine Yamal (dorsal 10), tipo "Molestia muscular", 2 semanas → calculó regreso `20-07-2026` (hoy + 14 días, `doctor.py:127`). Guardó y quitó a Yamal de `disponibles` (28→27). ✔
- **Opción 4 → op1 (cambiar tipo lesión):** cambié "Leve"→"Grave", persistió. ✔
- **Opción 4 → op2 (semanas a 0):** al poner 0 entra la rama de eliminación (`doctor.py:238-258`) y con "si" eliminó al jugador correctamente. Funciona, aunque la lógica es frágil (ver §4). ✔

**Integración cruzada (la feature central del informe):** verificada de punta a punta. (1) En `main.py` creé un partido el 25-07 con Yamal convocado. (2) En `doctor.py` lesioné a Yamal 4 semanas (regreso 03-08, posterior al partido). (3) Al reabrir `main.py`, el programa imprimió "ADVERTENCIA: Hubo cambios en la disponibilidad..." (`main.py:66`) y **removió automáticamente a Yamal de los convocados** del partido (convocados `[]`, `cantidad_convocados` 0). Exactamente lo que describe el informe (`main.py:43-56`). Muy destacable para el nivel.

**`Traceback` observados:** los únicos que aparecieron fueron `EOFError: EOF when reading a line` cuando el stdin canalizado se agotaba en medio de un prompt (p.ej. `main.py:106`, `doctor.py:270`). **No son fallos del programa** — es el comportamiento normal ante fin de entrada, contemplado por la rúbrica. No encontré ningún `Traceback` provocado por datos o navegación normal del menú.

**Fragilidades observadas (no crashes):**
- En `doctor.py:240` el `while (respuesta != "si") or (respuesta != "no")` es siempre verdadero (tautología); funciona solo porque se sale por `break` internos. En `doctor.py:242` hay `respuesta.strip == "sí"` (falta `()` — compara el método, no el string), por lo que esa rama nunca se cumple; el flujo se salva por la comparación previa `respuesta.strip() == "si"`.
- En `doctor.py:208` y `:269` hay condiciones `while (op != 1) or (op != 2)` igualmente tautológicas, funcionales solo por los `break`.
- El propio autor deja el comentario "Esto no está completo. Estoy intentando perfeccionarlo" (`doctor.py:240`) — honesto y consciente.

## 4. Buenas prácticas de Python (nivel principiante)

- **Manejo de errores:** buen uso de `try/except ValueError` en cada `input()` numérico (`main.py:79`, `doctor.py:87`, etc.). Consistente en todo el código.
- **f-strings** usadas idiomáticamente en todas las salidas.
- **Indentación consistente** (compila sin problemas; no hay mezcla tab/espacio detectada).
- **Legibilidad:** ayudada por los nombres descriptivos y comentarios.
- **Puntos a mejorar:** (a) duplicación masiva del sub-menú de convocatoria (§2); (b) condiciones tautológicas en `doctor.py` (§3); (c) `validar_fecha` (`main.py:9`) parsea la fecha por posición de caracteres (`string[0]+string[1]`) en vez de `datetime.strptime`, frágil si el formato cambia; (d) variable `cantidad_lesionados` declarada y nunca usada (`doctor.py:31`); (e) uso de identificadores con acentos/ñ (`año`, `están`) — funciona en Python 3 pero no es lo convencional.

## 5. Datos y persistencia

- **Estructura razonable:** `datos.json` con cuatro listas de diccionarios (`plantel`, `lesionados`, `disponibles`, `partidos`), cada partido con sus propias sublistas `convocados`/`disponibles`. Modelo coherente con el dominio.
- **Persistencia correcta y verificada:** los cambios sobreviven entre ejecuciones. El truco de serializar `date`→`"%d-%m-%Y"` al guardar (`main.py:727-730`, `doctor.py:309-311`) y deserializar al cargar es correcto y es justo la dificultad que el informe relata haber resuelto.
- **Ruta relativa `./datos.json`** (`main.py:17`): depende de ejecutar desde la raíz del repo. El informe lo reconoce como dificultad ya resuelta; en la práctica funciona correctamente desde el directorio del proyecto.
- **Riesgo menor:** si `doctor.py` guarda y a mitad ocurriera un fallo, el JSON podría quedar parcial (el propio informe menciona haber sufrido esto); no lo reproduje en uso normal.

## 6. Informe (`report.md`)

- **Muy completo y honesto.** Describe fielmente lo que el código hace: los dos programas, el modelo recurso/evento, las reglas (un partido por fecha, ventana de temporada, tope 18, estados `OK`/`!`), y la integración cruzada. Todo lo que afirma lo confirmé ejecutando.
- **No sobreestima.** Al contrario, documenta con detalle las dificultades reales (ruta relativa → `FileNotFoundError`, serialización de `date` en JSON, el bug de jugadores lesionados que seguían convocados, dobles/faltantes en disponibles) y cómo las resolvió. Coincide con lo que observé en el código.
- **Sección de aprendizaje personal** reflexiva y bien escrita.
- **Discrepancia menor:** el informe menciona estados y easter eggs ("Barcelona SC" como rival válido) que sí existen; no detecté afirmaciones de features inexistentes. La única imprecisión es que describe el proyecto como "dos archivos" en un punto y "tres archivos" en otro (cuenta `datos.json`), pero es cosmético.

---

## Síntesis para el profesor

Trabajo **sólido y por encima del promedio de 1er año**. Ambos programas arrancan, todas las opciones de menú funcionan en ejecución real, la validación de entradas es consistente, la persistencia es correcta y — lo más notable — la **integración cruzada entre `main.py` y `doctor.py` vía el JSON compartido funciona de verdad**, incluyendo la remoción automática de lesionados de partidos ya convocados. El informe es honesto y no infla nada.

Áreas de mejora, todas de nivel esperado: extraer funciones para eliminar la duplicación grande del sub-menú de convocatoria, reducir el anidamiento, y limpiar un par de condiciones tautológicas en `doctor.py` (que el propio autor ya identificó como incompletas). Ningún `Traceback` provocado por uso normal; los únicos fueron `EOFError` por fin de stdin en las pruebas automatizadas, que no cuentan como fallo.

**Nivel general:** notable. Fortaleza principal: alcance funcional real y correcto, con integración cruzada. Mejora principal: modularizar con funciones para reducir duplicación y anidamiento.
