# Reporte de Evaluación — Proyecto I (Programación, 1er año)

- **Issue:** #321
- **Repositorio:** https://github.com/leonardogq/proyecto
- **Estudiante:** Leonardo González Quintana
- **Grupo:** C122
- **Descripción declarada:** Planificador Inteligente de Eventos para un Estudio de Grabación. Aplicación en Python + Streamlit que gestiona salas, recursos, personal y restricciones del dominio, validando disponibilidad y aplicando reglas configurables vía JSON.

---

## Nota metodológica importante

No es una aplicación de consola: la interfaz es una app web **Streamlit** (`main.py`), por lo que no se puede alimentar con `printf` por stdin. La lógica de negocio está bien separada en la clase `PlanificadorEventos` (`planificador.py`), lo que permitió evaluarla de forma directa:

1. Instancié `PlanificadorEventos`, cargué `recursos.json` y `restricciones.json`, y llamé `agregar_evento` / `eliminar_evento` con datos reales del repo, cubriendo flujos válidos e inválidos (18 escenarios).
2. Arranqué además la GUI en modo headless (`streamlit run main.py --server.headless true`) para comprobar que la app real levanta: respondió **HTTP 200** y `_stcore/health` = **`ok`**.
3. `py_compile` de ambos módulos: OK.

Dato relevante: `agregar_evento` persiste en disco (llama a `guardar_eventos_json`), así que cada alta válida modifica `data/eventos.json`. En las pruebas neutralicé ese efecto o restauré el archivo con `git checkout`.

## Dimensión 1 — Qué hace el programa

El sistema modela un estudio de grabación con inventario limitado y programa eventos (podcast, doblaje, canción, instrumental) sobre dos salas (Pequeña, Grande) validando un conjunto amplio de reglas de negocio antes de aceptar cada reserva.

Flujo real (verificado ejecutando):

- **Alta de evento** (`planificador.py:17` `agregar_evento`): ejecuta las validaciones en etapas (fechas → disponibilidad de inventario → exclusiones → corequisitos/reglas/personal → sala ocupada → recursos por fecha) y solo si todo pasa hace `self.eventos.append` + orden por fecha + persistencia (`planificador.py:62-65`).
- **Baja** (`planificador.py:68` `eliminar_evento`): busca por tipo+sala+fecha y elimina; devuelve `(False, "Evento no encontrado")` si no existe.
- **GUI** (`main.py`): un `st.radio` con tres opciones (Agregar / Eliminar / Ver agenda). El alta arma dinámicamente inputs numéricos por cada recurso del inventario (`main.py:143-155`) y construye la fecha como `datetime.combine(fecha, datetime.min.time())` (`main.py:166`).

Ejemplos concretos que corrí y devolvieron exactamente lo esperado:

- Podcast válido en Pequeña con 2 micrófonos/soportes/audífonos + 4 cables + 1 técnico → `(True, 'Evento agregado correctamente')`.
- Fecha pasada → `No se pueden crear eventos en fechas anteriores a hoy`.
- Fecha a >1 año → `No se pueden crear eventos con más de un año de anticipación`.
- Doble reserva de sala mismo día → `Ya existe un evento en la sala Pequeña para el día ... Sugerencia: próxima fecha con recursos libres 2026-07-18`.

## Dimensión 2 — Organización del código

Fortalezas:

- **Separación GUI / lógica clara y correcta** (`main.py` vs `planificador.py`). Esto es lo que permitió testear el núcleo sin la interfaz — un acierto de diseño poco común a este nivel.
- **Reglas externalizadas en JSON** (`data/restricciones.json`, `data/recursos.json`). Cambiar mínimos de personal, exclusiones o corequisitos no toca el código. El propio informe justifica bien esta decisión.
- **Validaciones en métodos pequeños y de nombre autoexplicativo** (`_validar_fechas`, `validar_corequisitos_por_categoria`, `_validar_personal_obligatorio`, etc.), orquestadas por etapas en `agregar_evento`.

Debilidades menores:

- Convención de nombres **inconsistente**: unos métodos llevan guion bajo (`_validar_fechas`) y otros no (`validar_exclusiones_por_sala`) sin criterio claro de "privado vs público" (`planificador.py:98` vs `:246`).
- Bloques de líneas en blanco excesivos (`planificador.py:386-392`, `:463-469`) — cosmético.
- `main.py` repite el bloque de serialización a JSON en tres sitios (`:76`, `:110-119`, `:177-186`) en vez de delegar en `planificador.guardar_eventos_json`, que ya existe y hace lo mismo.

## Dimensión 3 — Corrección funcional (basada en ejecución real)

Corrí 18 escenarios contra la lógica real. Todos se comportaron correctamente:

1. Podcast válido → alta OK, `eventos` pasa a 1.
2. Fecha pasada → rechazo con mensaje correcto.
3. Fecha >365 días → rechazo correcto.
4. Corequisito 3 micrófonos / 1 soporte → `Si hay 3 'Micrófonos' debe haber 3 'Soportes de micrófono' (se indicaron 1).` + además detecta cables insuficientes (`Se requieren al menos 6 'Cables' ...`).
5. Instrumental en Pequeña → doble rechazo (instrumento prohibido en sala + evento prohibido en sala).
6. Sala Grande sin personal → exige técnico y productor (mensajes del ejemplo 5 del informe).
7. Micrófonos=100 (>inventario 8) → `Se disponen de 8 'Micrófonos', pero se solicitaron 100.`
8. Evento sin recursos → `El evento debe tener recursos especificados`.
9. Dos eventos misma sala/día → rechazo + sugerencia de próxima fecha.
10. Inventario agotado el mismo día por otra sala → `No hay suficientes recursos disponibles ese día: Micrófonos:0, ... Sugerencia: próxima fecha libre 2026-07-19.` (comprobado que el conteo cruza salas: bien).
11. `eliminar_evento` de un evento existente → OK; de uno inexistente → `Evento no encontrado`.
12. Tipo inexistente ("Concierto en vivo") → `No existe el evento 'Concierto en vivo'`.
13. Fecha pasada como `date` en vez de `datetime` → `La fecha debe ser un objeto datetime` (defensa correcta; en la GUI nunca ocurre porque siempre se combina a datetime).
14. Doblaje en Grande (evento prohibido) → rechazo correcto.
15. Instrumental válido en Grande (guitarra + cable + personal) → alta OK.
16. Instrumental sin instrumentos → `debe incluir al menos un instrumento.`
17. Guitarra en Pequeña → `El instrumento 'Guitarra eléctrica' no puede usarse en la sala Pequeña`.
18. Batería (excepción de la regla de cables) → alta OK sin cables. La lista `excepto: ["Batería"]` funciona.

**No encontré ningún `Traceback` provocado por el estudiante.** La única forma de romper la lógica fue pasar `fecha` que no sea `datetime`, y ese caso está manejado con un mensaje claro. La GUI headless levantó sin errores.

Observación (no es bug, es alcance): el modelo trabaja **a nivel de día** — una sala solo admite un evento por fecha, sin franjas horarias. El propio informe lo reconoce como mejora futura.

## Dimensión 4 — Buenas prácticas de Python (nivel principiante)

- Buen uso de `dict.get(...)` con valores por defecto para no reventar ante claves ausentes (omnipresente en las validaciones).
- Manejo de `FileNotFoundError` al cargar eventos (`planificador.py:551`) — pero `cargar_recursos_json` / `cargar_restricciones_json` **no** lo hacen (`planificador.py:555-567`): si faltaran esos JSON, la app lanzaría excepción al arrancar. Aceptable porque son archivos del repo, pero conviene saberlo.
- `_validar_reglas_evento` distingue bien `int` de `bool` para no confundir la clave `requiere_instrumentos: true` con un mínimo numérico (`planificador.py:145`) — detalle fino y correcto.
- Los comentarios "docstring" van como comentarios `#` dentro del cuerpo en vez de cadenas `"""..."""` (p.ej. `planificador.py:5-6`, `:70-73`). No afecta a la ejecución; solo no aparecen en `help()`.

## Dimensión 5 — Datos y persistencia

- Modelo simple y adecuado: eventos como lista de dicts `{tipo, sala, fecha, recursos}`; recursos y restricciones como dicts anidados cargados de JSON.
- Serialización de fechas correcta: `datetime` → ISO string al guardar (`planificador.py:531-532`) y string → `datetime` al cargar (`planificador.py:549-550`). Round-trip verificado: tras un alta, `data/eventos.json` quedó con `"fecha": "2026-07-16T00:00:00"` y se recarga bien.
- Inconsistencia menor: `main.py` guarda la fecha como `ev["fecha"].date().isoformat()` (solo día, `main.py:114`/`:181`) mientras `guardar_eventos_json` la guarda con hora (`...T00:00:00`). Ambos recargan sin problema, pero son dos formatos distintos en el mismo archivo según qué ruta lo escriba.

## Dimensión 6 — Informe (`report.md`)

El informe es **extenso, honesto y bien alineado con el código**. Puntos a favor:

- Los 5 ejemplos de uso (`report.md:184-238`) coinciden literalmente con los mensajes que produce el código — los reproduje y salen iguales.
- La sección "Problemas encontrados" (`report.md:243-263`) es especialmente valiosa: explica con honestidad que `validar_exclusiones_por_evento` existe pero hoy no tiene ninguna exclusión configurada (cierto: `por_evento` solo tiene `Grabación de instrumental: {prohibido: []}`, `restricciones.json:33-37`), y describe con precisión el diseño en etapas de la validación para evitar avalanchas de errores.

Discrepancias menores:

- El informe llama al proyecto "Planificador **Inteligente**" y a la sugerencia de fechas un "componente más inteligente" (`report.md:161`); en realidad `sugerir_proxima_fecha_libre` (`planificador.py:472`) es una búsqueda lineal día a día hasta 365 días — honesto que el propio texto la califique de "relativamente sencilla".
- Ejemplo 2 del informe muestra una sugerencia con fecha **2026-06-26** (`report.md:211`), anterior a "hoy"; es solo un valor ilustrativo pegado del desarrollo, no refleja el comportamiento real (que sugiere fechas futuras). Detalle cosmético.
- El informe menciona "autenticación", "base de datos relacional" y "calendario visual" únicamente como mejoras futuras, sin afirmar que estén implementadas — correcto, no sobrevende.

---

## Valoración global (orientativa, sin nota numérica)

Trabajo **sólido y por encima de lo esperable en primer año**. El estudiante modeló un dominio genuinamente rico (inventario compartido, corequisitos entre recursos, exclusiones por sala y por evento, personal obligatorio, sugerencia de fechas) y lo implementó con una arquitectura limpia: lógica de negocio totalmente separada de la GUI y reglas externalizadas en JSON. Ejecuté 18 escenarios válidos e inválidos y **todos se comportaron correctamente, sin un solo crash atribuible al código**; la GUI Streamlit levanta sin errores. El informe es largo, coherente con el código y —lo más meritorio— honesto sobre las limitaciones (sugeridor lineal, validación por día, exclusión-por-evento vacía).

- **Principal fortaleza:** la separación GUI/lógica y la profundidad del sistema de validación en etapas — es lo que hace el núcleo testeable y el dominio creíble.
- **Principal área de mejora:** consolidar la persistencia. `main.py` reimplementa a mano la serialización a JSON (en tres sitios y con un formato de fecha distinto del de `guardar_eventos_json`) en vez de reutilizar el método que ya existe en la clase; unificarlo elimina duplicación y la inconsistencia de formato de fecha.
