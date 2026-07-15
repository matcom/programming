# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #297
- **Repositorio:** https://github.com/Gabriel84848/Remake-del-1er-Proyecto-Pyton-Matcom
- **Estudiante:** Gabriel Alvarez Amaro
- **Grupo:** c-122
- **Descripción declarada:** Sistema de reservas para el hotel "Blue Gate" con interfaz de consola que gestiona habitaciones y servicios, validando disponibilidad y aplicando restricciones (desayuno obligatorio para suites, incompatibilidad masaje/yoga).

---

## Nota metodológica importante

Es una **aplicación de consola** con `input()`/`print()`, así que se ejecutó de dos formas complementarias:

1. **Por consola, recorriendo el menú real** con `printf '...' | python Main.py` (creación de reserva, cancelación, validaciones de entrada, opción de menú inválida).
2. **Invocando directamente la lógica de negocio** de `logica_reservas.py` (que está limpiamente separada de la interfaz) con datos reales del repo, para verificar cada restricción de forma aislada y determinista sin pelear con el orden de los `input()`.

Un detalle: la verificación automática del bot reportó el punto de entrada como `Clases.py` y "sin salida". El punto de entrada real es **`Main.py`** (contiene `if __name__ == "__main__": main()`); `Clases.py` es solo definiciones. El bot no produjo salida porque la app espera `input()` de inmediato y no se le alimentó stdin.

## Dimensión 1 — Qué hace el programa

Sistema de reservas de hotel con menú de 7 opciones (`Main.py:13-52`):

1. **Ver catálogo de habitaciones** (`interfaz_usuario.py:20`) — 8 habitaciones en 2 pisos; permite consultar las reservas por ID de habitación.
2. **Ver catálogo de servicios** (`interfaz_usuario.py:84`) — desayuno (cap. 5), masaje (cap. 3), yoga (cap. 3).
3. **Ver reservas existentes** (`interfaz_usuario.py:446`).
4. **Crear reserva** (`interfaz_usuario.py:470`) — flujo guiado: cliente → fechas → habitaciones → servicios → resumen → confirmación.
5. **Buscar hueco automático** (`interfaz_usuario.py:624`) — recorre desde hoy hasta 2 años buscando la primera fecha que satisfaga habitaciones + servicios + nº de noches (`logica_reservas.py:164`).
6. **Cancelar reserva** (`interfaz_usuario.py:530`).
7. **Salir**.

Las reservas persisten en `Yeison.json`. Verifiqué el ciclo completo: crear una reserva de la suite H204 la escribe en el JSON, y cancelarla lo deja vacío.

## Dimensión 2 — Organización del código

**Buena separación por responsabilidades**, notable para primer año:

- `Clases.py` — 3 clases de datos (`Habitacion`, `Servicio`, `Reserva`) con `Reserva.to_dict()` para serializar (`Clases.py:23`).
- `logica_reservas.py` — validaciones y disponibilidad puras: reciben datos, devuelven `(bool, mensaje)` o valores. **Sin `print` ni `input`**, así que son testeables de forma aislada (lo aproveché en la ejecución directa).
- `interfaz_usuario.py` — toda la interacción con el usuario (33 KB, la más larga).
- `guardar_y_cargar.py` — persistencia JSON con fallback a datos por defecto.
- `Main.py` — bucle principal y menú.

El patrón `(booleano, mensaje)` que devuelven las validaciones (`logica_reservas.py:4-45`) es consistente y limpio, y la interfaz solo tiene que mostrar el mensaje. Buen instinto de diseño.

Debilidades menores:
- `interfaz_usuario.py` es muy larga y con mucha repetición de bloques de UI (`limpiar_pantalla()` + banner) que podrían factorizarse en un helper.
- `ids_validos` está hardcodeado en `interfaz_usuario.py:22` en vez de derivarse de la lista de habitaciones cargada; si se cambian las habitaciones del JSON, ese literal queda desincronizado.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Ejecuté por consola y por lógica directa. Todo lo probado se comporta correctamente:

1. **Crear reserva suite H204 (consola)** → "¡Reserva creada exitosamente! ID de reserva: 1"; el JSON queda `[{'cliente': 'Juan Perez', 'habitaciones': ['H204'], 'servicios': ['desayuno:1'], 'check_in': '2026-08-01', 'check_out': '2026-08-05'}]`. El desayuno obligatorio se añade automáticamente.
2. **Cancelar reserva (consola)** → "RESERVA CANCELADA", JSON vuelve a `reservas: []`.
3. **Nombre inválido** ("Al" → "No aceptamos personas con menos de 3 letras"; con números → "Imposible que tengas eso en tu nombre"). Nombres con tilde y apóstrofe ("José O'Brien") aceptados (`logica_reservas.py:24`).
4. **Fecha en pasado** (`01-01-2020`) → "El check-in no puede ser en el pasado (hoy es 15-07-2026)".
5. **Fecha con formato basura** (`32-13-2026`, `31-02-2026`) → capturada con `ValueError`, "Formato incorrecto. Usa DD-MM-AAAA". No revienta.
6. **Opción de menú inválida** (`99`) → "Opción no válida. Inténtalo de nuevo".
7. **Mismo piso obligatorio** → `["H101","H104"]` válido; `["H101","H201"]` → "Las habitaciones deben estar en el mismo piso".
8. **Máximo 2 habitaciones** → `["H101","H102","H103"]` rechazado; **duplicados** → rechazado; **inexistente** (`H999`) → rechazado.
9. **Exclusión masaje/yoga** → `crear_reserva_sistema` con `["masaje:1","yoga:1"]` → "No se pueden reservar 'masaje' y 'yoga'".
10. **Solapamiento de fechas** → tras reservar H101 del 01 al 05 ago, disponible 03–07 ago = `False` (colisiona), disponible 05–09 ago = `True` (adyacente, correcto: check-out = check-in no colisiona). La lógica `check_in < res.check_out and check_out > res.check_in` (`logica_reservas.py:53`) maneja bien la adyacencia.
11. **Capacidad de servicio** → tras 5 desayunos, `verificar_disponibilidad_servicio("desayuno")` = 0.
12. **Suite sin desayuno** → con los 5 desayunos ocupados, H204 desaparece de `obtener_habitaciones_disponibles` (`logica_reservas.py:105`).
13. **Búsqueda automática de hueco** → devuelve fechas coherentes respetando habitaciones + servicios + noches.
14. **Fallback ante JSON corrupto** → `cargar_datos` con `{ corrupto ]` imprime aviso y carga los 8 defaults.

`py_compile` de los 5 módulos: **OK** en todos.

**No encontré ningún bug funcional.** El único `Traceback` que apareció fue `EOFError` al agotarse el stdin del pipe — artefacto de mi método de prueba, no del programa.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

Puntos mejorables menores:

- **`except:` desnudo** en `logica_reservas.py:74-77` (dentro de `verificar_disponibilidad_servicio`) — atrapa cualquier excepción y hace `pass`. Aquí es defensivo ante un item mal formado, pero un `except (ValueError, IndexError)` sería más honesto.
- **`except Exception` genérico** en `guardar_y_cargar.py:45` — enmascara *cualquier* fallo de carga como "usar datos por defecto", incluyendo bugs. Funciona, pero oculta la causa real.
- Los servicios se codifican como strings `"desayuno:1"` que luego se parsean con `.split(":")` en varios sitios. Funciona, pero una tupla `("desayuno", 1)` o un pequeño dict evitaría el parseo repetido y frágil.
- Algunos mensajes usan tono coloquial ("Imposible que tengas eso en tu nombre", "menos de 3 letras en el nombre") — simpático, aunque un registro más neutro encajaría mejor con el resto de la UI.

Nada de esto afecta la corrección. Los nombres de funciones y variables son claros y en español consistente.

## Dimensión 5 — Datos y persistencia

- Modelo simple y adecuado: 3 clases de datos + JSON (`Yeison.json`).
- Serialización correcta de `date` → ISO string en `Reserva.to_dict()` (`Clases.py:28`) y `date.fromisoformat` al cargar (`guardar_y_cargar.py:38`). El estudiante menciona en el report que esto le costó, y lo resolvió bien.
- `guardar_datos` reescribe el JSON completo con `indent=2` tras cada creación/cancelación — verificado que persiste.
- El archivo de datos se llama `Yeison.json` (nombre curioso, probablemente interno), y hay un `requirements.txt.txt` con **doble extensión** (creado desde un editor de Windows), por eso el bot no lo detectó. Contenido: comentario indicando que no usa librerías externas — correcto, solo stdlib.

## Dimensión 6 — Informe (`Report.md`)

El informe (2108 palabras contadas con `wc -w`, por encima del mínimo de 2000 — el estudiante lo actualizó tras el aviso automático) es **honesto y coincide bien con el código**. Explica el dominio, las 4 restricciones, la estructura de 5 archivos y las dificultades reales (organización, serialización de fechas, caso de la suite, bucles de entrada). Los ejemplos de flujo (secciones 4.1–4.3) reproducen fielmente la salida real que observé al ejecutar.

Discrepancias menores:
- **`Report.md:48`** dice: *"el sistema se encargara de verificar que no esten en el mismo piso"* — redacción invertida: el código exige que **sí** estén en el mismo piso (`logica_reservas.py:149`). Es un lapsus de redacción, no del código.
- Encabezado duplicado al inicio (`Report.md:1` y `:3` repiten "1. Qué hace el programa").
- El report presenta las restricciones sin exagerar validación: dice claramente que la exclusión masaje/yoga "en la práctica el usuario nunca podrá seleccionar ambos" y que añadió la verificación "por si acaso" — lo cual es exacto (la interfaz solo deja elegir uno; la verificación en `validar_exclusion_mutua` es un cinturón extra). No hay sobreestimación de "demuestra"/"prueba".

---

## Valoración global (orientativa, sin nota numérica)

Proyecto **sólido**. La arquitectura en 5 módulos con separación real entre lógica pura y presentación es más madura de lo habitual en primer año, y esa separación se sostiene bajo prueba: pude ejercitar toda la lógica de negocio de forma aislada y **las 14 verificaciones pasaron**, incluyendo casos borde no triviales (adyacencia de fechas, suite oculta sin desayuno, capacidad de servicios, JSON corrupto). La app de consola corre de punta a punta, valida entradas basura sin reventar y persiste correctamente. El informe es honesto y refleja el código real, incluyendo las dificultades que el estudiante sorteó. El hecho de que sea un "remake" consciente (reescritura para eliminar código espagueti, documentado en el propio report) habla bien de su criterio.

- **Principal fortaleza:** separación limpia entre `logica_reservas.py` (funciones puras testeables) e `interfaz_usuario.py`, que hace el sistema robusto y verificable. Toda la lógica de restricciones probada resultó correcta.
- **Principal área de mejora:** los `except` genéricos/desnudos (`logica_reservas.py:74`, `guardar_y_cargar.py:45`) que enmascaran errores, y el modelado de servicios como strings `"nombre:cantidad"` que obliga a parsear con `.split(":")` en múltiples lugares. Sustituir esos strings por tuplas o dicts eliminaría fragilidad.
