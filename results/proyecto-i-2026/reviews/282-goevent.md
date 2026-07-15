# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #282
- **Repositorio:** https://github.com/Unit0code/GoEvent
- **Estudiante:** Marlon Alejandro Rodriguez Gigato
- **Grupo:** C122
- **Descripción declarada:** Gestión de recursos y tiempo de eventos con temática de empresa transportista en Cuba.

---

## Nota metodológica importante

Es una aplicación de **consola** (`input()`/`print()` con menús numerados). No hay GUI. La ejecución se hizo alimentando el menú con `printf '...' | python main.py` recorriendo todos los flujos, más pruebas dirigidas de la lógica de negocio importándola directamente (verificación de expiración/consumo de recursos). Todo se corrió en un venv Python 3.12 aislado; el proyecto usa **solo la biblioteca estándar** (`json`, `datetime`, `pathlib`, `os`, `platform`, `copy`), así que no hubo dependencias que instalar pese a la ausencia de `requirements.txt`/`pyproject.toml`.

**Un detalle de compatibilidad:** en `Agregar.py:125` hay un f-string con comillas simples anidadas (`f'{recursos.nombre + ' no deberia...'}'`), sintaxis válida **solo en Python 3.12+** (PEP 701). En Python 3.11 o anterior el módulo no compilaría. Bajo 3.12 todo compila y corre limpio.

## Dimensión 1 — Qué hace el programa

GoEvent gestiona la agenda de una base de transporte. El flujo real observado:

1. **Menú inicial** (`main.py:82-153`): crear cuenta, cargar cuenta (`.json`) o salir. Las cuentas se protegen con contraseña (`miscelaneo.verificador_passw`, `miscelaneo.py:29`).
2. **Menú principal** (`main.py:19-79`), 8 opciones: agregar/eliminar/ver eventos, ver recursos y disponibilidad, actualizar agenda, ver recursos rotos/agotados, buscar hueco, y salir guardando.
3. **Agregar evento** (`Agregar.py:36-46`): 12 tipos de evento (viajes a 9 provincias, boteo, mantenimiento, vacaciones), cada uno con restricciones propias. Se pide fecha, se muestran los recursos **disponibles en ese horario** (filtrando los ocupados por colisión temporal), el usuario los elige, y una batería de validadores decide si el evento se agenda.
4. **Modelo de recursos con desgaste**: vehículos tienen `usos` (5), personas tienen `energia` (100). Al **expirar** un evento (`miscelaneo.verificador_estado_eventos`, `miscelaneo.py:89`), los recursos consumidos pierden usos/energía; los eventos "curativos" (mantenimiento, vacaciones) los restauran.
5. **Buscar hueco** (`Buscar_hueco.py:92-111`): dado un evento y sus recursos, calcula los intervalos libres fusionando las colisiones existentes.
6. **Persistencia** en `<usuario>.json` (`Jsons.py`).

Verificado ejecutando: creé cuentas, cargué `marlon.json` (2 eventos de prueba a Pinar del Río), agregué un "Viaje a la Habana", provoqué expiración y confirmé el consumo. Todo funcionó.

## Dimensión 2 — Organización del código

**Fortalezas:**
- **Buena modularización por responsabilidad** (10 archivos): `Events.py` (jerarquía de eventos), `Recursos.py` (clase `Recurso` + inicializador), `Agregar.py` (creación + validadores), `Eliminar.py`, `Jsons.py` (serialización), `miscelaneo.py` (utilidades + actualización de estado), `recursos_dannados.py`, `Buscar_hueco.py`, `user.py`. Para 1er año es una separación notablemente limpia.
- **Uso correcto de herencia** (`Events.py:8-18`): clase base `Events` con `__init__`/`__dict__` comunes, y 12 subclases que solo cambian datos (nombre, duración, restricciones). Es el patrón adecuado.
- **Diccionarios como despacho** (`Agregar.py:14-18`, `Jsons.py:11-17`): mapear número→clase y nombre→clase evita cadenas gigantes de `if`. Muy buen instinto.

**Debilidades:**
- **Sobreuso de `__dict__` como método** (`user.py:13`, `Recursos.py:17`, `Events.py:12`). `__dict__` es un atributo especial de Python (el namespace de la instancia); redefinirlo como método funciona aquí solo porque siempre se llama explícitamente (`usuario.__dict__()`), pero es una colisión conceptual peligrosa. Lo idiomático sería `to_dict()` o `as_dict()`.
- **Mezcla de idiomas y de convenciones de nombres**: clases en inglés (`travel_Habana`, `Botear_Habana`), atributos capitalizados como si fueran clases (`self.Recursos`, `self.Needs`, `self.Restriction_hour`), funciones en español. Inconsistente pero legible.
- **Parámetro mutable por defecto** (`user.py:7`): `events: list = []`. Es una trampa clásica de Python (la lista se comparte entre instancias). Aquí no explota porque siempre se pasa una lista explícita, pero es un antipatrón real.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí lo siguiente (Python 3.12, venv aislado):

1. **`py_compile` de los 10 módulos** → todos OK.
2. **Crear cuenta + navegar menú** → OK. Muestra los 14 recursos con estado; "ver eventos" con lista vacía responde "No hay eventos".
3. **Cargar `marlon.json` + contraseña `k23`** → carga los 2 eventos a Pinar del Río correctamente, con fechas y recursos deserializados.
4. **Agregar "Viaje a la Habana"** con Juan (Conductor) + Transtur1 (Vehículo) + Federico (Guía), fecha `15/08/2026 --- 10:00` → **"Se ha agregado el evento a la agenda ✅"**. Cumple `Needs = [Conductor, Vehiculo, Guia]`.
5. **Evento inválido — faltan categorías necesarias** (solo un vehículo) → **"Te faltan recursos necesarios ❌"** + no se agenda. Correcto.
6. **Fecha basura** (`basura-fecha`) → "Ha habido un error. Introduce una fecha en el formato solicitado" y re-pide. **Fecha pasada** (`01/01/2020`) → "Esta fecha es anterior al dia de hoy". **Hora muy temprana** (`05:00`, restricción 08:00) → "Estas intentando hacerlo muy temprano… a partir de las 08:00:00". Los tres, correctos.
7. **Restricción de par mutuamente excluyente** (Jose + Marlon en Habana) → **"Jose, Marlon no pueden estar juntos… ❌"** y no se agenda. Correcto.
8. **Entradas basura al menú** (`abc`, `99`, `-5`) → `try_option` (`miscelaneo.py:32`) captura el error y el fuera-de-rango sin reventar. Robusto.
9. **Buscar hueco** en `marlon.json` para un Pinar con Camión1: con eventos existentes 11:40–16:40 y 18:40–23:40 sobre Camión1, sugirió correctamente **"Antes del 2026-12-12 06:40 o para después del 2026-12-12 23:40"** (fusionó los intervalos y restó la duración de 5 h). El algoritmo de fusión recursiva de intervalos (`Buscar_hueco.py:24-49`) funciona.
10. **Ciclo de desgaste** (probado importando la lógica): un "Viaje a la Habana" ya expirado consumió correctamente **Transtur1: usos 5→4** y **Juan: energia 100→80**, y removió el evento del usuario.
11. **Cargar cuenta inexistente** → "El archivo… no existe ❌. Puede volverlo a intentar". **Contraseña incorrecta** → mensaje claro con opción de escribir "salir". Ambos, correctos.
12. **Persistencia round-trip**: agregué un evento, salí (guardó `.json`), recargué → el evento persistió con nombre y fecha intactos.

**No observé ningún `Traceback` no controlado** en ninguna prueba. Los `try/except Exception: pass` que envuelven las operaciones en `main.py` (líneas 36-39, 43-46, 59-63) blindan el flujo, aunque a costa de tragarse errores silenciosamente (ver Dimensión 4).

**Bug menor confirmado (no rompe la ejecución):** en `Events.py:29` la restricción de par de "Viaje a la Habana" es `('Suarez', 'Menedez')`, pero el recurso real se llama `'Menendez'` (`Recursos.py:52`). El nombre está mal escrito, así que **esa restricción específica nunca se dispara** — un par que debería estar prohibido puede coexistir. Las demás restricciones sí funcionan (verificado con Jose+Marlon).

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- **`try/except Exception: pass` en el menú** (`main.py:36-63`): captura *cualquier* error y lo descarta en silencio. En 1er año es comprensible como red de seguridad, pero esconde bugs: si algo falla dentro de "agregar evento", el usuario ve el menú de nuevo sin explicación. Mejor: capturar excepciones específicas o al menos imprimir el error.
- **`try_option` bien resuelto** (`miscelaneo.py:32-41`): el patrón de validar `int(input())` en un bucle con `min/max` es exactamente lo correcto; se reutiliza en todo el proyecto. Buen trabajo.
- **`comparador_nombres` puede devolver `None`** (`Agregar.py:248-251`): si el nombre no está en la lista, no hay `return` explícito y devuelve `None`, que luego se usa como índice (`recursoscopia[idx]`). En la práctica los nombres siempre coinciden, pero es frágil.
- **`dividir_lista_str` reimplementa `', '.join(...)`** (`Agregar.py:175`): el propio comentario del estudiante lo admite ("se que existe pero no se cual es"). Honesto y simpático; la versión idiomática es `', '.join(map(str, lista))`.
- **`miscelaneo.clean` sin paréntesis** (`main.py:53`): en la opción 4 se escribe `miscelaneo.clean` (referencia a la función, no llamada), así que no limpia la pantalla. Inofensivo pero es un descuido.
- **Comentarios abundantes y útiles**: casi cada bloque está explicado en español. Excelente hábito para aprender y para que otro lea el código.

## Dimensión 5 — Datos y persistencia

- **Modelo de datos coherente**: `User` (nombre, passw, path, events) → `Events` (fecha, nombre, recursos, restricciones) → `Recurso` (nombre, categoría, estado, usos, energía). La jerarquía refleja bien el dominio.
- **Serialización manual a JSON** (`Jsons.py:19-39`): convierte recursivamente usuario→eventos→recursos a `dict` antes de `json.dumps`. Al cargar (`Jsons.py:41-109`), reconstruye las instancias en cascada (recursos → eventos vía la clase correcta por nombre → usuario). Es un round-trip completo y **verificado que funciona**.
- **Detalle correcto y no trivial**: en `guardar_json` se hace una **copia** de los recursos globales antes de convertirlos a `dict` (`Jsons.py:28`, `miscelaneo.copia_recursos`), evitando mutar los objetos vivos. El historial de git muestra que esto fue un bugfix consciente ("recursos disponibles se pasaban por referencia"). Buen razonamiento.
- **Limitación de seguridad esperable en 1er año**: la contraseña se guarda en texto plano en el `.json` y cualquiera puede leerla o listar cuentas con la opción "cargar". No es un reproche a este nivel, solo constancia.

## Dimensión 6 — Informe (`Readme.md`)

El repositorio trae **`Readme.md`** (no `report.md`, por eso la verificación automática lo marcó como ausente). Tiene ~525 palabras — por debajo del umbral de 2000 que pedía el chequeo automático, pero es un README real, bien escrito y **fiel al código**:

- Describe correctamente detección de colisiones, desgaste de recursos (energía/usos), restricciones de exclusión mutua, restricciones particulares y horarios de funcionamiento. **Todo esto existe y funciona** — verificado.
- Los tipos de restricción que enumera (`Events.py:107` "horarios", "restricciones particulares", "exclusión mutua", "requisitos mínimos") coinciden uno a uno con los validadores reales (`verificador_horarios_adecuados`, `verificador_restricciones`, `verificador_restricciones_tuplas`, `verificador_necesarias` en `Agregar.py`).
- Los eventos "curativos" (mantenimiento restaura vehículos, vacaciones restaura personas) están correctamente descritos y correctamente implementados (`miscelaneo.py:140-147`).

**Discrepancias menores:**
- Dice "**Python version 3.17**" — no existe tal versión (probablemente quiso decir 3.12/3.11). La sintaxis real exige 3.12+.
- Las rutas de imagen usan backslash de Windows (`capturas\Console.png`), que no renderiza en GitHub/Linux. Las imágenes existen en `capturas/`.
- No exagera ni inventa features: el informe **subestima** más bien lo logrado (no menciona "Buscar hueco", que sí está implementado y funciona).

---

## Valoración global (orientativa, sin nota numérica)

Este es un proyecto **sólido y ambicioso** para un primer año. El estudiante modeló un dominio real (una base de transporte) con un sistema de reglas genuinamente rico —12 tipos de evento, cinco clases de restricción, un modelo de desgaste de recursos con eventos curativos, detección de colisiones temporales y hasta un buscador de huecos con fusión de intervalos— y **todo eso funciona de verdad al ejecutarlo**. La arquitectura está bien separada en módulos, usa herencia y despacho por diccionario con buen criterio, y la serialización JSON hace un round-trip completo. La validación de entradas es robusta: alimenté fechas basura, horas fuera de rango, opciones inválidas y pares prohibidos, y en ningún caso reventó con un `Traceback`. El historial de git delata un desarrollo iterativo maduro, con bugfixes conscientes (paso por referencia, redundancias eliminadas).

Los defectos son menores y típicos del nivel: `try/except Exception: pass` que oculta errores, la colisión conceptual de redefinir `__dict__` como método, el parámetro mutable por defecto, un `miscelaneo.clean` sin paréntesis, y un typo (`'Menedez'`) que deja una restricción muerta. Nada de esto compromete el funcionamiento.

- **Principal fortaleza:** un sistema de reglas de negocio complejo y coherente que **funciona de extremo a extremo** —desde la creación de eventos con validación múltiple hasta el desgaste/renovación de recursos y la persistencia— respaldado por una modularización limpia poco común en primer año.
- **Principal área de mejora:** dejar de tragar errores con `try/except Exception: pass` (capturar excepciones específicas o al menos reportarlas) y renombrar los `__dict__()` a `to_dict()`; ambos harían el código más depurable y más idiomático sin cambiar su comportamiento.
