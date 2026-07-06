# Reporte de evaluación — Proyecto I (1er año)

- **Issue:** #267
- **Repositorio:** https://github.com/Elcarli2712/Pelea-de-Boxeo
- **Estudiante:** Carlos Zorrilla Gonzalez
- **Grupo:** C121
- **Archivos:** `app.py` (568 líneas), `report.md`, `requirements.txt` (vacío), `planificador_eventos.json`

---

## 0. Ejecución dinámica (lo que realmente corrí)

Entorno: Python 3.14.4, sin dependencias (`requirements.txt` de 0 bytes), ejecutado como `python3 app.py`. No hizo falta `uv` ni venv. Punto de entrada: `app.py:567` (`if __name__ == "__main__": main()`), función `main()` en `app.py:285`.

Pruebas realizadas (stdin piped + timeout):

1. **Arranque + menú + salir** (`1`, `0`): arranca limpio, muestra el menú de 8 opciones, "No hay eventos planificados." (arranca con estado vacío en memoria, NO auto-carga el JSON). Salida ordenada con opción `0`. ✅
2. **Cargar estado + listar + ver detalles** (`7`, `1`, `4`→`1`): carga el JSON incluido, lista el evento 1 (2025-12-06, 20:00–22:00) y muestra sus detalles con todos los recursos. ✅
3. **Agregar evento válido** (fecha `2026-07-11` = sábado futuro, horario 1, recursos g16=2, vendas=2, peleadores=2, equipo=2, árbitros=1, protector=2): `✅ Evento 1 agregado exitosamente`, y luego aparece correctamente al listar. Flujo completo end-to-end funciona. ✅
4. **Entradas inválidas**: opción de menú `99` → "Opción inválida. Intente de nuevo." (`app.py:565`); fecha no-sábado `2026-07-08` → "❌ Error: La fecha debe ser un sábado." (`app.py:325`); fecha mal formada `abc` → "❌ Error: Formato de fecha invalido" (`app.py:333`); peleadores impares `3` → "❌ Error: La cantidad de peleadores debe ser par..." y re-pregunta (`app.py:432-434`). Todas manejadas sin romper. ✅
5. **Sugerir intervalo** (opción `5`, duración 2, recursos 0): `✅ Próximo intervalo disponible: 2026-07-11 de 18:00 a 20:00`. ✅
6. **Exclusión mutua de guantes en vivo** (g16=2 luego g14=2): detecta el conflicto y muestra el error, permitiendo corregir solo ese campo (`app.py:399-404`). ✅
7. **EOF a mitad de menú** (stdin vacío): `EOFError` no capturado en `app.py:310`. **No cuenta como fallo real** — es el artefacto normal de alimentar el programa por tubería cuando se acaba la entrada; con un usuario interactivo real no ocurre.
8. **Round-trip de persistencia** (cargar → guardar): guarda y recarga sin excepción; el JSON re-escrito es válido.

**Veredicto de ejecución:** el programa arranca y funciona en todas las opciones del menú que probé (1–7 y 0). No encontré ningún `Traceback` provocado por uso normal — solo el `EOFError` esperable al agotar el stdin piped. Es un proyecto que **corre de verdad y hace lo que dice**.

---

## 1. Qué hace el programa

Es un **planificador de eventos de boxeo** por consola (no un juego de peleas, pese al nombre del repo). Gestiona un inventario de 8 recursos (Guantes 16 oz, Guantes 14 oz, Vendas, Peleadores, Equipo de entrenamiento, Árbitros, Cascos, Protector Bucal), cada uno con cantidad total y asignada (`Recurso`, `app.py:5-24`). Permite crear eventos de boxeo que consumen recursos, sujetos a un rico conjunto de restricciones: los eventos solo pueden ser sábados en dos franjas (18–20h o 20–22h), no en el pasado, con reglas de negocio entre recursos (guantes mutuamente excluyentes, vendas obligatorias con guantes, peleadores en cantidad par, protector bucal ≥ peleadores, etc.) y una espera de 30 días entre eventos que usan recursos "no reutilizables".

El menú (`app.py:299-565`) ofrece: listar, agregar, eliminar, ver detalles, sugerir el próximo intervalo libre, y guardar/cargar estado en JSON. El flujo principal es el bucle `while True` con `input()` y despacho por `if/elif` sobre la opción elegida.

## 2. Organización del código

**Muy buena para 1er año.** El código está estructurado en tres clases con responsabilidades claras:

- `Recurso` (`app.py:5-24`): encapsula un pool con `asignar`/`liberar`/`disponible`. Buen diseño.
- `Evento` (`app.py:27-43`): datos del evento + `intervalo()` para calcular solapamientos.
- `Planificador` (`app.py:46-282`): la lógica de negocio — agregar, eliminar, validar, detectar conflictos, sugerir, persistir.

Nombres de variables y métodos claros y en español consistente. Usa métodos "privados" por convención (`_hay_conflicto_horario`, `_validar_restricciones`, `_verificar_restriccion_espera`). Type hints presentes (`Dict`, `List`, `Optional`) — no exigidos a este nivel, es un plus. La lógica de dominio está bien separada de la interfaz.

**Punto débil de organización:** la función `main()` (`app.py:285-565`, ~280 líneas) es enorme, sobre todo el bloque de la opción `2` (agregar evento, `app.py:318-481`). El bucle de captura de recursos con validaciones en tiempo real está todo inline y podría extraerse a funciones auxiliares (p.ej. `pedir_recursos()`, `pedir_fecha()`). Es el clásico "menú gigante" — aceptable a este nivel, pero es lo primero que crecería mal.

## 3. Corrección funcional (según ejecución real)

Ver sección 0. Resumen: **todas las opciones probadas funcionan**. Validaciones robustas y re-preguntan en lugar de romperse. Cada rama del menú está envuelta en `try/except` (`app.py:319`, `484`, `493`, `505`, `547`, `554`), así que un error puntual no tumba el programa. Hallazgos concretos:

- **Doble validación** (defensa en profundidad): la interfaz valida en tiempo real (`app.py:386-472`) y `_validar_restricciones` (`app.py:85-123`) valida de nuevo antes de crear. Bien pensado.
- **Detección de conflictos de solapamiento** (`app.py:64-83`): implementa correctamente la condición de solapamiento `nuevo_inicio < fin and nuevo_fin > inicio` y suma recursos entre eventos solapados. Es un algoritmo no trivial y está bien resuelto.
- **Inconsistencia real detectada en el JSON incluido** (`planificador_eventos.json:23`): el recurso aparece como `"arbitros"` (minúscula, sin tilde) en la sección `recursos`, mientras que el evento y el resto del código usan `"Árbitros"` (`app.py:293`). Al cargar, esto crea un recurso fantasma `"arbitros"` que nunca se usa, y el `"Árbitros"` hardcodeado en `main()` queda con la cantidad por defecto. No provoca crash porque el evento sí trae la clave correcta, pero es un dato incoherente que se arrastra al re-guardar.
- **Dato muerto en el JSON**: el evento trae `"recurrente": true` (`planificador_eventos.json:52`) que el código nunca lee ni escribe (`cargar_estado`, `app.py:267-275`, lo ignora). Feature a medio hacer.
- **Discrepancia de totales**: `main()` recrea los recursos con totales fijos (Peleadores=10, Protector Bucal=20, `app.py:291-295`) pero el JSON tiene Peleadores con total 20. Como `cargar_estado` sobrescribe, gana el JSON — pero si el usuario no carga, opera con los valores hardcodeados. Comportamiento algo confuso pero no incorrecto.
- **Validación cruzada peleadores↔guantes** (`app.py:426-427`): "No es posible pedir mas peleadores que guantes" — regla propia añadida por el estudiante; funciona, aunque el orden de captura hace que si pides peleadores antes que suficientes guantes te bloquee (los recursos se piden en orden fijo de diccionario, y guantes van primero, así que en la práctica está bien).

No hallé ningún `Traceback` por uso normal.

## 4. Buenas prácticas de Python (nivel principiante)

- **Legibilidad:** buena. f-strings usados consistentemente, comentarios que explican cada bloque de reglas, indentación mayormente consistente.
- **Manejo de errores:** `try/except ValueError` en las conversiones `int()` (`app.py:376-477`), y `try/except Exception` por opción de menú. Apropiado.
- **Duplicación:** hay repetición notable — el bucle de captura de recursos con `while True`/`int()`/validación se repite casi idéntico entre la opción 2 (`app.py:374-477`) y la opción 5 (`app.py:520-535`). Extraer a una función lo eliminaría.
- **Variables locales redundantes** (`app.py:366-373`): declara `guantes_16`, `vendas`, etc. y luego las reasigna dentro del bucle; algunas (`arbitros`, `cascos`, `protector_bucal`) se calculan pero no se usan fuera del bucle. Menor.
- **Bug lógico menor en validación de vendas** (`app.py:409-422`): el `else` del `if guantes_14 > 0 and vendas < guantes_14` (`app.py:418-422`) reasigna `vendas = cantidad` de forma un poco enredada; la lógica funciona en los casos probados pero el flujo if/else está mal anidado y podría dejar pasar un caso borde (vendas insuficientes con guantes_16 pero el segundo `if` de guantes_14 no aplica). No lo reproduje como fallo, pero es frágil.
- **Indentación irregular** (`app.py:78-79`): sangría extra sin efecto (cosmético).
- No usa variables globales innecesarias. No abusa de nada. Correcto para el nivel.

## 5. Datos y persistencia

Persistencia en JSON bien implementada (`guardar_estado`, `app.py:237-254`; `cargar_estado`, `app.py:256-282`). Serializa fechas/horas a strings ISO y las reconstruye al cargar (`app.py:268-270`). Reconstruye el `ultimo_id_evento` tomando el máximo entre el guardado y el mayor id real (`app.py:276-278`) — detalle cuidadoso. Estructuras de datos razonables: `Dict[str, Recurso]` para el inventario, `List[Evento]` para los eventos. El round-trip cargar→guardar funciona sin pérdida (probado, TEST 8). La única mancha es el dato inconsistente `"arbitros"`/`"recurrente"` del JSON incluido (ver sección 3), pero eso es del archivo de ejemplo, no del mecanismo.

**Observación:** el programa **no auto-carga** el estado al arrancar ni auto-guarda al salir; el usuario debe usar explícitamente las opciones 6/7. Es una decisión de diseño válida pero sorprendente (fácil perder trabajo). Sugerencia: cargar automáticamente al inicio si el archivo existe.

## 6. Informe (`report.md`)

**Informe excelente y muy completo** — 11 secciones, ~240 líneas, describe con detalle el dominio, cada restricción, el algoritmo de conflictos, la persistencia y la interfaz. En su mayoría **coincide fielmente con el código**: las restricciones descritas (guantes excluyentes, vendas obligatorias, peleadores pares, espera de 30 días, sábados nocturnos) están todas implementadas y las verifiqué al ejecutar.

**Sobreestimaciones menores a señalar:**

- El informe (§10.1) dice "menú interactivo con 8 opciones" listando la 8ª como "Salir", pero en el código Salir es la opción `0` (`app.py:308`), no la 8. Discrepancia cosmética.
- §11.3 afirma "El código es limpio, bien comentado y sigue las convenciones de Python" — es en general cierto, aunque `main()` es demasiado larga y hay duplicación, matices que el informe no reconoce.
- El informe no menciona la inconsistencia `arbitros`/`Árbitros` ni el campo `recurrente` sin usar; presenta la persistencia como completamente pulida cuando el archivo de ejemplo tiene ruido.
- §8.1 menciona "Confirmación para guardar el evento" como parte del flujo de creación, pero el código no pide confirmación explícita (`app.py:478`) — el evento se agrega directo.

Ninguna sobreestimación es grave; el informe es honesto en lo esencial y refleja comprensión real del proyecto.

---

## Valoración global (interna)

Trabajo **notablemente por encima del promedio de un primer proyecto de 1er año**. Diseño orientado a objetos correcto, un dominio con restricciones ricas bien modeladas, validación en dos niveles, un algoritmo de detección de conflictos no trivial resuelto bien, persistencia funcional, e informe extenso y en su mayoría fiel. Corre de verdad en todas las opciones del menú sin crashes por uso normal.

Áreas de mejora principales: (1) `main()` demasiado larga con duplicación — extraer funciones auxiliares; (2) pequeñas inconsistencias en el JSON de ejemplo y datos muertos (`arbitros`, `recurrente`); (3) auto-carga/guardado ausente. Nada de esto es descalificante; son los siguientes pasos naturales de refinamiento.

**Fortaleza principal:** modelado del dominio + POO limpia.
**Mejora principal:** modularizar `main()` y limpiar la persistencia.
