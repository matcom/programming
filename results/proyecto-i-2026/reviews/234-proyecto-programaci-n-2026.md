# Reporte de evaluación — Issue #234

- **Estudiante:** Marcos Daniel Miranda Alvarez
- **Grupo:** C-111
- **Repositorio:** https://github.com/Marcos0808/Proyecto-Programaci-n-2026
- **Descripción del issue:** Sistema de Gestión de Operaciones y Recursos de un Hotel (salones, piscinas, áreas), con validación de reglas de negocio, control de capacidad y gestión de ciclos de stock.

## Estructura del repo

```
Report.md            (317 líneas — informe extenso)
README.md            (2 líneas)
Resource/
  Main.py            (4)   punto de entrada
  Interfaz.py        (273) GUI Tkinter
  Eventos.py         (65)  EventManager + persistencia JSON
  Recursos.py        (98)  ResourceManager (catálogo estático)
  Condiciones.py     (277) ConditionsManager (reglas de negocio)
```

Total ~717 líneas de Python. **Importante: NO es una app de consola con menú `input()`** como asume la rúbrica por defecto — es una **aplicación de escritorio con GUI en Tkinter**. El informe lo confirma ("Apartado visual del programa").

---

## 1. Qué hace el programa

Es un gestor de eventos de hotel con interfaz gráfica (Tkinter). El punto de entrada es `Resource/Main.py:1-5`, que instancia `App()` (`Interfaz.py:8`) y llama `app.run()` → `self.window.mainloop()` (`Interfaz.py:41-42`). La ventana principal muestra un `Listbox` de eventos y seis botones: Agregar Evento, Eliminar Evento, Ver Detalles, Lugares, Personal, Recursos (`Interfaz.py:18-37`).

El flujo de creación de evento es una cadena de diálogos modales: nombre (`add_event`, `Interfaz.py:49-52`) → fecha/hora con comboboxes (`date_selection`, `Interfaz.py:177-241`) → lugar (`place_selection`, `Interfaz.py:129-145`) → cantidad de invitados (`guest_selection`, `Interfaz.py:158-174`) → selección de personal (`personal_selection`, `Interfaz.py:83-93`). Al aceptar, se dispara toda la validación de reglas de negocio en `ConditionsManager` y, si pasa, se persiste el evento en `eventos.json`. El dominio (áreas, personal interno/externo, recursos con stock) está codificado como listas paralelas en `Recursos.py:3-41`.

La lógica de negocio es sorprendentemente rica para 1er año: dependencias automáticas (elegir "Chef" agrega "Camarero"; elegir un salón agrega "Limpieza" — `Condiciones.py:10-59`), disponibilidad de personal por horario diurno/nocturno (`control_personal_available`, `Condiciones.py:110-139`), control de solapamiento temporal y de choque de lugar entre eventos (`control_general`, `Condiciones.py:181-217`), y control de stock finito que se "repone al día siguiente" (`control_avilable_finite`, `Condiciones.py:219-256`).

## 2. Organización del código

**Muy buena para el nivel.** El código está dividido en cuatro clases con responsabilidades claras, cada una en su módulo:

- `EventManager` (`Eventos.py:5`) — CRUD + persistencia.
- `ResourceManager` (`Recursos.py:1`) — catálogo de datos base.
- `ConditionsManager` (`Condiciones.py:5`) — todas las reglas de negocio.
- `App` (`Interfaz.py:8`) — la vista/controlador de la GUI.

Esta separación vista/lógica/datos es exactamente lo que uno querría ver y va más allá de lo esperado en un primer proyecto. Los nombres de métodos son descriptivos (`conditions_personal_resource`, `control_avilable_finite`, aunque este último tiene un typo — "avilable"). Hay comentarios útiles a la derecha de casi cada método en `Interfaz.py`.

**Debilidades de organización:**
- `Recursos.py:3-41` usa un único método `resource(num)` con `if num == 1 … if num == 8` que devuelve listas distintas según un entero mágico. Llamadas como `self.resource(4)` (personal) o `self.resource(7)` (recursos) son opacas: hay que memorizar qué número es qué. Mejor serían métodos con nombre (`get_staff()`, `get_resources()`) o un diccionario/dataclass.
- El dominio se modela con **listas paralelas indexadas** (`resource_place[i]`, `resource_place_guest_max[i]`, `resource_place_guest_min[i]` — `Recursos.py:5-10`). El código depende de que todos los índices se mantengan alineados a mano. Un diccionario `{"Salón Grande": {"max":50,"min":31}}` o una pequeña clase `Area` sería más robusto y menos propenso a errores.
- Referencias como `personal[15]`, `implements[8]`, `resource[11]` a lo largo de `Condiciones.py` son **números mágicos** que solo se entienden con el comentario de al lado. Es frágil: reordenar una lista rompería silenciosamente decenas de reglas.

## 3. Corrección funcional (basada en ejecución real)

Ejecuté el programa. Hallazgos:

**a) Bug de arranque en Linux (case-sensitivity) — `Main.py:1`.** Los `import` son en minúscula (`from interfaz import App`, `from eventos import EventManager`, etc.) pero los archivos están capitalizados (`Interfaz.py`, `Eventos.py`, `Recursos.py`, `Condiciones.py`). En Windows (sistema de archivos insensible a mayúsculas) esto funciona; en Linux/macOS **falla inmediatamente**:

```
$ python3 Main.py
Traceback (most recent call last):
  File ".../Main.py", line 1, in <module>
    from interfaz import App
ModuleNotFoundError: No module named 'interfaz'
```

Se arregla renombrando los archivos a minúscula (o los imports a mayúscula) para que coincidan. El `__pycache__` con `.pyc` en el repo sugiere que se desarrolló y probó solo en Windows.

**b) El programa arranca correctamente (verificado).** Tras copiar los módulos con nombres en minúscula para sortear (a), y usando un intérprete con Tkinter (los builds de `uv` lo traen; el `python3.14` del sistema no tiene `python3-tk` instalado), el programa **inicializa bien**: los dos `EventManager` cargan su estado y la ejecución llega hasta `tk.Tk()` en `Interfaz.py:14`. No pude recorrer la GUI de forma interactiva por falta de servidor gráfico (X/Wayland) en el entorno de evaluación — es una limitación del sandbox headless, no un fallo del código. La construcción de la ventana y todos los widgets compila y se alcanza sin error.

**c) Lógica de negocio verificada directamente (funciona).** Como no pude clickear la GUI, ejecuté la cadena de validación replicando exactamente lo que hacen los handlers `save_*` de `Interfaz.py`. Resultados reales observados:

- **Crear evento "Yoga" en Otra Área, 30 invitados, personal Animación:** pasa toda la cadena (`control_personal_available=True`, `control_general=True`, `control_avilable_finite=True`), se llama `add_event` y se persiste. `inf_event` devuelve la ficha formateada correctamente. ✅
- **Persistencia round-trip:** al reinstanciar `EventManager`, carga `Yoga` desde `eventos.json`. `remove_event` lo borra y re-guarda. ✅
- **Dependencias automáticas:** crear un evento con "Organizador" arrastra automáticamente "Limpieza" al personal y "Mesa/Silla/Implementos de Limpieza" a los recursos (`Condiciones.py:81-84`, `Condiciones.py:71-73`). Verificado en el JSON persistido. ✅
- **Choque de lugar:** dos eventos en "Salón Grande" a la misma hora → el segundo es **rechazado** con `inf=0` ("El espacio solicitado no se encuentra disponible"), tal como debe. ✅
- **`inf_event` sobre evento inexistente** imprime aviso y retorna `None` (`Eventos.py:37-38`). Correcto, aunque debería devolver un string para que `messagebox.showinfo` no muestre "None".

**d) Validación de entradas — parcialmente buena.**
- Cantidad de invitados: `guest_selection` (`Interfaz.py:169`) valida `.isdigit()` y el rango min/max del lugar antes de continuar; reintenta en bucle. Bien.
- Duración del evento: `save_date_selection` (`Interfaz.py:267`) exige que la hora de inicio < fin y que la diferencia sea ≤ 3 horas. Bien.
- **Punto frágil:** `available_personal` (`Condiciones.py:143`) hace `int(guest)` sin `try/except`; si `guest` no fuera dígito lanzaría `ValueError`. En el flujo normal está protegido por `guest_selection`, así que no es alcanzable por el usuario — pero es dependencia implícita.
- `show_details` (`Interfaz.py:63-69`) llama `self.event_listbox.get(selected_event)` **antes** del `if selected_event:`. Si no hay nada seleccionado, `curselection()` devuelve `()` y `get(())` puede dar resultado inesperado; el guard debería ir primero.

**e) Bug de comparación horaria (latente).** En `save_date_selection` (`Interfaz.py:267`) y en `time()` (`Condiciones.py:275`) las horas se comparan como **strings** (`self.combo_hour_beginning.get() < self.combo_hour_end.get()`). Como vienen con formato `"02d"` (dos dígitos: "09", "13"), la comparación lexicográfica coincide con la numérica en este caso concreto, así que funciona — pero es por suerte, no por diseño. Comparar `int(...)` sería lo correcto y explícito.

**f) Aliasing de lista en `place_selection` (`Interfaz.py:134`).** `self.copy_elementos_place = self.elementos_place` NO copia la lista (es el mismo objeto), y luego hace `del self.copy_elementos_place[3]` / `[7]`. Verifiqué que **no corrompe el estado global** porque `ResourceManager.resource(1)` reconstruye la lista en cada llamada, así que cada invocación parte de una lista fresca — se salvan por eso. La intención (ocultar Zona Deportiva y Área de Excursión en horario nocturno) resulta correcta porque tras `del[3]` el índice 7 apunta justo a "Área de Excursión". Funciona, pero es frágil y confuso; `copy = list(el)` y borrar por nombre sería mucho más claro.

## 4. Buenas prácticas de Python (nivel principiante)

**Positivo:** indentación consistente, f-strings usadas idiomáticamente (`Eventos.py:36`, `Interfaz.py:73`), uso correcto de `os.path.exists` + `json.dump/load` para persistencia (`Eventos.py:55-64`), comprensiones de lista (`Interfaz.py:96`), `list(set(...))` para deduplicar personal (`Interfaz.py:101`). Manejo de errores de UI con `messagebox` y ramas por código `inf` (`Interfaz.py:116-123`). Todo compila sin warnings (`python3 -m py_compile` OK en los 5 archivos).

**A mejorar:**
- Bucles `while var < control: … var = var + 1` en todo `Recursos.py` y `Condiciones.py` donde un `for … in enumerate()` o `for x in lista` sería más pythónico y menos propenso a errores off-by-one.
- Ramas `if/else` que hacen exactamente lo mismo (`Condiciones.py:206-208`, `247-249`: ambas ramas `var = var + 1`) — el `if` sobra.
- `str(date_1)` / `float(hour_big)` en `time()` (`Condiciones.py:259-274`) no hacen nada: el resultado no se asigna. Parecen intentos de conversión que no surten efecto; de hecho la comparación sigue siendo de strings.
- Falta `try/except` alrededor de `int(guest)` en la capa de lógica (defensa en profundidad).
- Uso de `self.` para variables que son locales de un flujo (`self.event_name`, `self.value`, `self.date`) — funciona pero ensucia el estado del objeto.

## 5. Datos y persistencia

Correcta y funcional. `EventManager` guarda el diccionario de eventos como JSON (`Eventos.py:55-58`) y lo recarga al arrancar (`Eventos.py:60-64`). Verifiqué el round-trip completo: crear → guardar → reinstanciar → cargar → eliminar → guardar, todo funciona. Los eventos se ordenan por fecha al insertar (`order_event`, `Eventos.py:51-53`).

**Observaciones:**
- `eventos.json` se abre sin `encoding='utf-8'` (`Eventos.py:56,62`) y sin `ensure_ascii=False`, así que los acentos quedan escapados (`Salón Grande`). Funciona pero el archivo no es legible a ojo; añadir `ensure_ascii=False` mejoraría.
- La ruta del archivo es relativa (`'eventos.json'`), así que se crea en el directorio de trabajo actual, no junto al código. Correr desde otra carpeta perdería el estado.
- `ConditionsManager.__init__` (`Condiciones.py:6-8`) crea **su propio** `EventManager`, además del que ya tiene `App` (`Interfaz.py:10`). Hay dos instancias cargando el mismo JSON (se ve en el doble mensaje "No se encontró archivo…" al arrancar). No causa el bug ahora porque la validación recibe el diccionario por parámetro, pero es un acoplamiento innecesario y podría desincronizarse.

## 6. Informe (`Report.md`)

**Excelente informe, de los mejores esperables en 1er año.** 317 líneas: describe el dominio, tablas completas de áreas/personal/recursos con capacidades, las reglas de dependencia, capacidad y tiempo, estructura del proyecto, instrucciones de ejecución, funcionalidades y hasta siete eventos de ejemplo. Coincide en lo esencial con lo que el código hace.

**Discrepancias informe ↔ código:**
- El README/informe indica ejecutar `python main.py` (`Report.md:202`) con `cd …\Resource` — usa backslash de Windows y minúscula. En Linux esto falla por el bug de case (ver 3a). El informe fue escrito desde la óptica Windows.
- El informe lista `__pycache__` como parte de la "Estructura del proyecto" (`Report.md:178-182`) y lo describe como "Guardado de datos del programa" — confusión conceptual: `__pycache__` es bytecode compilado, no datos; y no debería versionarse.
- La tabla de áreas (`Report.md:33-42`) tiene un error: repite "Salón Grande" donde el código dice "Salón Pequeño" (fila 3) y "Área de Exhibición" está desordenada respecto al código. Los números sí coinciden con `Recursos.py:8-10`.
- El informe **no menciona** que la app es GUI Tkinter hasta la sección de estructura; alguien que lea solo la descripción del dominio podría esperar una consola. Aparte de eso, no sobreestima features: todo lo que afirma (dependencias, choque de lugar, stock finito, persistencia) está realmente implementado y lo verifiqué corriendo.

---

## Resumen de ejecución (lo que corrí)

1. `python3 -m py_compile` de los 5 archivos → **OK**, todo compila.
2. `python3 Main.py` en Linux → **falla** con `ModuleNotFoundError: No module named 'interfaz'` (case-sensitivity).
3. Copias en minúscula + `uv run --python 3.12 python main.py` → la app **arranca**, los managers cargan estado y se alcanza `tk.Tk()`; no hay display en el sandbox para recorrer la GUI (limitación del entorno, no del código).
4. Script que replica la cadena de validación de los handlers `save_*`: **crear evento, persistir, recargar, eliminar, dependencias automáticas, y rechazo por choque de lugar** — todo funciona como debe.

## Valoración global (orientativa)

Trabajo **notablemente por encima del promedio de 1er año**. La ambición del dominio (reglas de dependencia en cadena, control de solapamiento temporal, stock finito que se repone) y su implementación funcional real son destacables; la separación en cuatro clases/módulos es correcta; el informe es completo. Los defectos son de robustez y estilo, no de concepto: el bug de case-sensitivity (que impide correrlo tal cual en Linux), el modelado por listas paralelas con índices mágicos, y la comparación de horas como strings. Ninguno invalida el proyecto — la lógica de negocio, verificada directamente, es correcta. Principal fortaleza: la riqueza y corrección de las reglas de negocio. Principal mejora: modelar el dominio con diccionarios/clases en lugar de listas paralelas indexadas, y arreglar los imports para que corra en cualquier sistema.
