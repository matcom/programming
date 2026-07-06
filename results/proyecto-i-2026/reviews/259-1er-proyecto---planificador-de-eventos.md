# Reporte detallado — Proyecto I (1er año)

- **Issue:** #259
- **Estudiante:** Leonardo David Alemán Bravo
- **Grupo:** C-111
- **Repositorio:** https://github.com/LLikeLemons/1er-Proyecto---Planificador-de-eventos
- **Descripción del issue:** "planificar eventos de una estación de policía"
- **Clonado en:** `/home/apiad/Workspace/.playground/proyecto1-eval/repos/259-1er-proyecto---planificador-de-eventos`

> **Nota de calibración:** Este NO es la app de consola típica del proyecto. Es una
> aplicación web con **Streamlit** (`streamlit run main.py`), ~2800 líneas de Python
> repartidas en 4 paquetes. El alcance y la ambición están muy por encima del promedio
> de un primer proyecto de 1er año. La evaluación se calibra al nivel, premiando esfuerzo
> y funcionamiento real, sin exigir arquitectura avanzada ni tests.

---

## 1. Qué hace el programa

Es un **planificador de eventos para una estación de policía**, implementado como
aplicación web con Streamlit. El punto de entrada es `main.py:9` (`def main()` con el
guard `if __name__ == "__main__"` en `main.py:94`), y se ejecuta con `streamlit run main.py`
tal como documenta el `report.md` y `requirements.txt` (`streamlit v1.51.0`).

Flujo principal (`main.py:29-93`):
- Al arrancar, carga el estado previo desde `data/data.json` (`main.py:11`, `load_json` en
  `methods/auxfunctions_2.py:34`), reconstruyendo un array de 4 posiciones: fechas indexadas,
  eventos, recursos globales y recursos personalizados.
- La página de inicio muestra 5 pestañas (`main.py:30`): INICIO, RECURSOS, EDICIÓN,
  VISUALIZACIÓN y RECURSOS PERSONALIZADOS.
- En la barra lateral (`main.py:56-70`) hay 8 tipos de evento agrupados en Cursos de
  Capacitación, Entrenamientos y Simulacros. Cada botón cambia `st.session_state.pagina_actual`
  vía `cambiar_pagina` (`methods/auxfunctions.py:8`) y despacha a la página del evento
  correspondiente (`main.py:74-89`).
- Cada página de evento (p. ej. `paginas_eventos/M_H.py`) presenta un formulario con tipo de
  frecuencia (único / semanal / mensual / rango de días), fecha, horario, y selección de
  recursos predeterminados + personalizados. Antes de permitir guardar, corre un **cálculo de
  colisiones** contra los eventos ya agendados y sugiere el **próximo intervalo disponible**.

El dominio está bien pensado: recursos con topes (`Helicóptero: 2`, `Oficiales de alto
rango: 7`, etc. en `data/data.json:4-26`), restricciones por tipo de capacitación
(teórica vs. práctica en `M_H.py:73-90`), y persistencia entre ejecuciones.

## 2. Organización del código

Muy por encima de lo esperado para 1er año. El código está dividido en **4 paquetes** con
`__init__.py` reales:

- `methods/` — lógica auxiliar: carga/guardado JSON (`auxfunctions_2.py`), clase `Event`
  (`recursos_eventos.py:5`), ordenación y búsqueda (`auxfunctions.py`), recalibración de
  índices (`recalibrate.py`).
- `paginas_eventos/` — un módulo por tipo de evento (`M_H.py`, `C_I.py`, `C_S.py`, `E_F.py`,
  `I_D.py`, `P_T.py`, `P_C.py`, `P_V.py`), cada uno ~225-250 líneas con estructura análoga.
- `paginas_admin/` — edición (`edicion.py`, `opciones_admin.py`), búsqueda de recursos
  (`resources_search.py`), visualización (`Eventos.py`), recursos personalizados
  (`custom_resources.py`).
- `data/` — `data.json` + `logo.png`.

Puntos fuertes:
- Uso real de una **clase** (`Event` en `recursos_eventos.py:5`) con `to_dict()` para
  serialización (`recursos_eventos.py:18`), y funciones de conversión dict↔objeto
  (`recursos_eventos.py:33-60`). Es exactamente el uso de clases que el dominio pide.
- Funciones auxiliares reutilizadas en todas las páginas (`collition_search`,
  `next_gap`, `smart_dates_sorter`, `binary_search`) — buen instinto de DRY.
- Comentarios de sección con banners (`#====|  ...  |====`) que ayudan a navegar archivos largos.

Debilidades:
- **Duplicación entre las 8 páginas de evento.** `M_H.py`, `C_I.py`, etc. comparten ~80% de
  estructura (formulario, colisiones, botones). Podría factorizarse una función/clase base
  parametrizada por los recursos y lugares de cada tipo. Es la deuda más grande del proyecto,
  aunque comprensible dado el tiempo.
- **Labels de depuración olvidados en producción:** `st.radio("IS THIS GONNA WORK???...")`
  (`paginas_admin/edicion.py:36`) y `st.multiselect("I THINK IT IS OK", ...)`
  (`paginas_eventos/M_H.py:92`). Funcionan, pero se ven en la UI final.
- **Sombreado de nombres:** la variable local `custom_resources` en `M_H.py:92` tiene el
  mismo nombre que la función importada `custom_resources` del paquete admin. No rompe (están
  en scopes distintos), pero confunde.
- Uso de `dict`, `type`, `list` como **nombres de variable** (p. ej. `recursos_eventos.py:19`,
  `M_H.py:189`), sombreando builtins. Nivel principiante, sin consecuencias aquí.

## 3. Corrección funcional (basada en ejecución real)

**Cómo lo ejecuté.** Creé un venv aislado con `uv` (Python 3.12), instalé `streamlit` +
`pillow`. Verifiqué sintaxis de todos los `.py` con `ast.parse` (OK) e importé `main.py`
(resuelve toda la cadena de imports, incluido `edition_selector`). Luego:

1. **Servidor real:** `streamlit run main.py --server.headless true --server.port 8791`.
   Arrancó sin errores; `http://localhost:8791/` respondió 200 y sirvió el shell de la SPA.
2. **Recorrido dinámico con `streamlit.testing.v1.AppTest`** (ejecuta el script server-side y
   simula interacciones de widgets), que es la vía correcta para ejercitar los callbacks Python
   de una app Streamlit sin navegador.

**Lo que observé al correr:**

- **Página de inicio:** renderiza sin excepción. 5 tabs, 8 botones de evento en la barra
  lateral, más los 3 botones (Agregar/Editar/Eliminar) de recursos personalizados de la tab 5.
- **Las 8 páginas de evento arrancan sin `Traceback`.** Recorrí cada botón de la barra lateral
  (Manejo de Helicóptero, Capacitación de Instructores, Capacitación SWAT, Prácticas de Tiro,
  Práctica de Conducción, Entrenamiento Físico, Persecución y aprehensión vehicular,
  Intervención a Domicilio) y todas renderizan el formulario completo, ejecutando en cada
  render `collition_search` (`auxfunctions.py:129`) y `next_gap` (`auxfunctions.py:221`) sin
  fallar sobre el dataset vacío.
- **Los 4 tipos de frecuencia funcionan** en la página de Manejo de Helicóptero: Evento único,
  Frecuencia semanal, Frecuencia mensual y Rango de días — todos sin excepción, incluida la
  rama de `next_gap` específica de cada frecuencia (`auxfunctions.py:226-291`).
- **Creación de evento end-to-end:** fijé horario 09:00→11:00 y fecha a 2 días futuros, el
  botón "Confirmar" se habilitó y al hacer click el evento se **persistió correctamente** en
  `data/data.json` (verifiqué: `n events persisted: 1`, con `date`, `time`, `resources`,
  `place`, `frecuency type` serializados bien vía `Event.to_dict()`).
- **Round-trip de persistencia:** al recargar la app, el evento se **deserializa** de disco sin
  problema (`dict_event`/`dict_date`/`dict_time` en `recursos_eventos.py:33-54`) y aparece en
  las tabs de edición y visualización.
- **Validación de entradas — funciona:** con horario por defecto (`now`→`now`), "Confirmar"
  aparece **deshabilitado** porque `time_2 <= time_1` (`M_H.py:205`). También bloquea fecha en
  domingo (`M_H.py:209`) y eventos del mismo día con hora ya pasada. Buen manejo de casos borde.
- **Recursos personalizados:** creé "Perro K9" con cantidad 5 vía el botón "Agregar"; se guardó
  en `st.session_state.custom_resources` correctamente (`custom_resources.py:35` → `st_resources_edit`
  en `auxfunctions_2.py:68`).
- **Tab RECURSOS (búsqueda de disponibilidad):** renderiza y ejecuta `collition_search2`
  (`opciones_admin.py:74`) sin excepción; muestra recursos y lugares disponibles.
- **Tab EDICIÓN / VISUALIZACIÓN:** con un evento en el store, ambas listan el evento y muestran
  botones Editar/Eliminar; renderizan sin excepción. La rama "evento en transcurso" y
  "evento finalizado" está codificada (`edicion.py:52-63`).

**Hace lo que dice el issue.** Sí: es un planificador de eventos para una estación de policía,
con creación, edición, visualización, gestión de recursos y control de conflictos de
horario/recurso/lugar. No encontré ningún `Traceback` en ninguno de los caminos que recorrí.

**Bugs latentes detectados (estáticos, en rutas muertas — no afectan la ejecución):**
- `weekdays_search` (`auxfunctions.py:189-197`) usa `validation` antes de asignarla (líneas
  191-194 la referencian; la asignación es en 194) y `x.weekday` sin llamar. **Función muerta**
  — un `grep` confirma que nunca se invoca. Sin impacto.
- `frecuency_type(event)` (`auxfunctions.py:199-208`) puede retornar `None` (rama `else: pass`).
  También función muerta (no confundir con `next_gap`, que usa el atributo
  `event.frecuency_type`, no esta función). Sin impacto.
- `M_H.py:89` asigna `places_options1 = places_options[0]` (un **string**, no lista) en la rama
  de Capacitación Práctica. Curiosamente `st.selectbox` lo maneja bien (mostró la opción
  correcta `['Centro de entrenamiento']` al probarlo), así que no crashea, pero es frágil:
  debería ser `places_options[:1]`.

## 4. Buenas prácticas de Python (nivel principiante)

- **Legibilidad e indentación:** consistentes; f-strings usadas idiomáticamente
  (`edicion.py:16`, `Eventos.py:31`). Bien.
- **Manejo de archivos:** `load_json`/`save_json` (`auxfunctions_2.py:28-64`) usan `with open`,
  `encoding="utf-8"`, `ensure_ascii=False`, y una estructura por defecto si el archivo no existe
  (`auxfunctions_2.py:37-61`). Robusto y correcto — impresionante para 1er año.
- **`deepcopy`:** usado deliberadamente (`auxfunctions.py:131`, `auxfunctions_2.py:75`) para
  evitar aliasing de diccionarios — el estudiante identificó y resolvió el problema de
  referencias compartidas (lo narra bien en el `report.md`).
- **`datetime`:** uso sólido de `date`, `time`, `timedelta` para la lógica de fechas.
- **Algoritmos:** implementó **merge sort** (`smart_dates_sorter`/`merge`/`merge2` en
  `auxfunctions.py:21-59`) y **búsqueda binaria** de primer/último índice
  (`binary_search*` en `auxfunctions.py:68-105`) a mano. Muy por encima del nivel esperado.

Debilidades menores (no penalizables al nivel):
- Casi no hay `try/except`; la validación se hace con condicionales previos (que funciona, pero
  un input muy raro en el JSON podría romper la carga). Aceptable para 1er año.
- Variables globales de estado vía `st.session_state` — es el modelo idiomático de Streamlit,
  así que aquí está bien.
- Labels de widget con texto de depuración (ver §2).

## 5. Datos y persistencia

- Estructura de `data.json`: `[fechas_indexadas, eventos, recursos_globales, recursos_custom]`
  (`data/data.json`, esquema por defecto en `auxfunctions_2.py:37-61`). Razonable y explícita.
- Las fechas se guardan como tuplas `(año, mes, día, índice_evento)` y se ordenan tras cada
  inserción (`agregar_fecha` + `smart_dates_sorter` en `auxfunctions.py:12-16`) para que la
  búsqueda binaria por fecha funcione. Diseño coherente.
- **Verificado dinámicamente:** guardar un evento escribe el JSON correcto y recargar lo
  reconstruye sin pérdida. Persistencia correcta.
- `recalibrate_dates_index` (`recalibrate.py:3`) reindexa las fechas cuando se elimina un
  evento, para que los índices no queden colgados. Buen detalle.

## 6. Informe (`report.md`)

Extenso, bien escrito y **fiel al código** — no sobreestima. Contrastes:

- Afirma "hasta 8 tipos de eventos" (`report.md:19`): **cierto**, verifiqué los 8 botones.
- Describe restricciones de co-requisito, exclusión mutua y mínimos de recurso
  (`report.md:32-38`): están codificadas en las páginas de evento (p. ej. la lógica de
  teórica/práctica y lugares en `M_H.py:73-90`).
- Describe frecuencias semanal/mensual/rango/único (`report.md:47-48`): **verificado**, las 4
  funcionan.
- Explica el algoritmo de "próximo intervalo disponible" y su dificultad
  (`report.md:82`): coincide con `next_gap` (`auxfunctions.py:221`).
- Menciona búsqueda binaria y `deepcopy` para el problema de referencias
  (`report.md:82-83`): ambos presentes en el código.

Discrepancia menor: el informe menciona "búsqueda binaria" para reducir trabajo en `next_gap`,
pero `next_gap` recorre día a día linealmente; la binaria se usa en `collition_search` para
localizar las fechas coincidentes. Es una imprecisión pequeña, no una sobreestimación.

La sección "Guía de uso" (`report.md:68-74`) es un poco confusa de leer, pero describe
operaciones que existen. La sección de aprendizaje (`report.md:77-84`) es reflexiva y honesta.

## Síntesis

Trabajo **sobresaliente para un primer proyecto de 1er año**. Ambición muy alta (app web,
persistencia, algoritmos a mano, 8 tipos de evento, edición y control de conflictos), y —lo más
importante— **funciona de verdad**: arrancó, recorrí las 8 páginas, las 4 frecuencias, creé y
persistí eventos, y no hallé ningún `Traceback` en los caminos ejercitados. La deuda técnica
real es la duplicación entre las 8 páginas de evento y algunos labels de depuración olvidados;
ambos son cosméticos/de mantenimiento, no de corrección. El informe es fiel al código.

**Principal fortaleza:** funciona end-to-end con un dominio rico y persistencia real.
**Principal mejora:** factorizar la duplicación entre páginas de evento y limpiar los labels.
