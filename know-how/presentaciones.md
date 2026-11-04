---
when: hacer o cambiar la presentación HTML de una conferencia, con o sin animaciones de algoritmos
---

# Presentaciones de conferencia

Cada conferencia del curso 2026 lleva una presentación en
`conferences/2026/<NN>-<tema>/presentacion.html`, junto a las notas del capítulo. Es un
solo archivo HTML con el CSS y el JavaScript dentro, sin dependencias ni red, porque se
abre desde la laptop del aula. Acompaña a la conferencia en vivo, y el libro es la
referencia completa: la presentación trae el código, las preguntas al aula y las
animaciones, y deja la prosa larga al capítulo.

La plantilla es la presentación de la conferencia 8,
`08-busqueda-y-ordenacion/presentacion.html`, que tiene todas las piezas, incluido el
motor de animaciones.

## Procedimiento

1. Lee el capítulo (`notas.md`), su entrada en `conferences/2026/plan.md` y la
   presentación anterior, de la que sale la diapositiva "dónde estamos".
2. Copia la plantilla. Conserva el `<style>` y el `<script>` enteros. Borra las
   diapositivas, las funciones generadoras propias de la conferencia 8 y las líneas
   `new Viz(...)`, `new Carrera(...)`, `new Rango(...)` y `new Crecer(...)` del final del
   script. Las clases `Viz` y `Carrera` sirven para cualquier conferencia. `Rango` y
   `Crecer` son ejemplos de controladores hechos a medida.
3. Toma las salidas de la caché del libro, no de una corrida nueva, para que coincidan
   con el PDF, sobre todo los tiempos medidos:

   ```bash
   cd conferences/2026/libro
   python3 -c "
   import json, sys
   for v in json.load(open('.scriptorium/freeze.json')).values():
       if isinstance(v, dict) and sys.argv[1] in v.get('stdout', ''): print('---'); print(v['stdout'])
   " 'un texto que imprime el bloque'
   ```

   Un bloque que la presentación muestra pero el libro no corre se ejecuta aparte, y su
   salida se copia tal cual.
4. Escribe las diapositivas siguiendo la anatomía de abajo.
5. Comprueba con Python cada afirmación que la presentación agrega y el libro no tiene:
   un error plantado que de verdad falla, un caso que de verdad da `IndexError`. Si al
   hacerlo el libro resulta decir algo falso, se corrige el libro en un PR aparte (así
   pasó con el cortocircuito de la inserción, en el PR #348).
6. Prueba la presentación:

   ```bash
   uv run conferences/2026/probar_presentacion.py conferences/2026/<NN>-<tema>/presentacion.html /tmp/capturas
   ```

   Sale con 1 si hay errores en la consola, diapositivas que se desbordan o animaciones
   que no terminan. Además imprime el mensaje final de cada animación en cada escenario,
   y hay que leerlos contra el algoritmo: el script sabe que la animación termina, no que
   termine bien. Mira también las capturas, que en las diapositivas animadas se toman a
   mitad de animación. Usa la versión de Playwright fijada en la cabecera del script,
   cuyo Chromium ya está instalado. La primera vez en otra máquina hay que instalarlo:
   `uv run --with playwright==<versión> playwright install chromium`.
7. Entrega por PR, y manda el HTML para revisarlo antes de la clase.

## Anatomía

La diapositiva es una `<section class="slide aN">`. La clase `aN` le da el color de
acento a su sección: `a1` azul para la portada, el plan, el proyecto y el cierre; `a2` a
`a5` (naranja, verde, rosa, violeta) para las partes, en orden; `a0` para el guion del
profesor. La barra de progreso toma el color de la diapositiva.

| Diapositiva | Cómo se arma |
|---|---|
| Portada | `ctr`, `.lbl` con curso, número de conferencia y fecha, `h1.hero`, `.sub` con el proyecto del día. Una figura o una animación sin controles. |
| Dónde estamos | `.road` con seis `.stop`: las anteriores, `now` la de hoy, `next` la siguiente, `off` una semana sin contenido. |
| El plan | Un SVG con una caja por parte, del color de la parte, que desembocan en el proyecto. |
| Separador de parte | `ctr`, `.lbl` "Parte N", `h1.hero`, `.sub` con la idea en una frase. |
| Explicación | `.c2` (o `.c2.wl` / `.c2.wr` para cargar un lado) con código y figura, tarjetas o una animación. |
| Demo en vivo | `.live` rojo con el archivo donde se escribe, código, `pre.out.hidden` y `.steps` numerados para el profesor. |
| Resumen y ejercicios | `ul.pts.sm`. Los ejercicios en una línea cada uno: el enunciado completo está en el libro. |
| Guion | La última diapositiva: `table#guion`, que el script llena solo con las diapositivas que llevan `.live`. |

El `h1` dice la idea de la diapositiva, no su tema, con la segunda mitad en `<em>`. Un
`pre.out.hidden` oculta la salida hasta que se pulsa R o se hace clic, para que el aula
prediga primero. Un `pre` con `data-hl` se colorea como Python, y cada línea queda en un
`span.ln` que la animación ilumina; para eso el `pre` lleva un `id`.

Las teclas: flechas, espacio o clic para avanzar; R revela una salida; V salta a la
próxima demo en vivo; un número y Enter van a esa diapositiva; F pantalla completa; ? la
ayuda. En las diapositivas animadas, A anima o pausa, S da un paso y X reinicia.

Los números son los de la conferencia, que es como los conocen los estudiantes. El libro
numera distinto desde la conferencia 8 (la semana 7 fue examen), así que el "capítulo 9"
de las notas es la conferencia 10.

## Animaciones

Una animación es un generador de JavaScript que recorre el algoritmo igual que el código
de la diapositiva y entrega una foto por paso. `Viz` la dibuja y pone los controles.

```js
function* gSeleccion(a, c, s) {        // a: la lista de {v, id}; c: contadores; s: el escenario
  c.comparaciones = 0;
  const F = o => ({ a: a.slice(), cnt: { ...c }, ...o });
  yield F({ msg: 'la lista sin ordenar' });
  // ... cada paso:
  yield F({ line: 6, ptr: { i, menor, j }, cls: { done, cmp: [j, menor] }, msg: `¿${a[j].v} < ${a[menor].v}?` });
  // ... al final:
  yield F({ cls: { done: range(0, n) }, msg: '...', end: true });
}
```

Cada foto puede llevar:

- `a`: la lista en ese momento. Los elementos se mueven por su `id`, así que un
  intercambio se anima solo. Desplazar un elemento, como hace la inserción, se dibuja
  como intercambios con el elemento que se mueve.
- `cls`: clases por posición. `cmp` (naranja, lo que se compara), `swap` (rosa, lo que se
  movió), `done` (azul, lo ya ordenado), `found` (verde), `key` (violeta y levantado,
  el elemento que busca su lugar), `dim` (apagado, lo descartado).
- `ptr`: los índices con su posición. Uno fuera de la lista, como `j = -1`, se oculta.
- `line`: la línea del código que se ejecuta, contando desde 0.
- `msg`: una línea corta con lo que pasa en ese paso.
- `end: true` en la última.

`new Viz(elemento, cfg)` recibe en `cfg`: `title`; `kind`, que es `'bars'` (barras, para
ordenar) o `'cells'` (casillas con el número, para buscar); `gen`, el generador; `code`,
el `id` del `pre` cuyas líneas ilumina; `ptrs`, un objeto
`{ nombre: [fila, color] }`; `scen`, la lista de escenarios
`{ label, data, ... }`, donde `data` es una lista o una función que la construye (para
"al azar") y lo demás llega al generador como `s`; y `speed`, los milisegundos por paso.
`Carrera` pone varios generadores lado a lado sobre la misma lista y avanza todos de una
comparación en una comparación, contando por `c.comparaciones`.

Un controlador hecho a medida, como `Rango` o `Crecer`, cumple el mismo contrato para
que las teclas y `probar_presentacion.py` lo manejen: `reset()`, `next()` que devuelve
`false` al terminar, `step()`, `toggle()`, `stop()`, un elemento `msg` con su estado y,
si tiene escenarios, `cfg.scen` y `si`. Al construirse se registra en
`root.closest('.slide')._ctl`. Al cambiar de diapositiva se paran las animaciones de la
que se deja.

## Trampas

- **Una columna de grilla crece con su texto más largo.** Con `1fr`, una línea con
  `white-space: nowrap` o un `pre` ancho empuja la columna y desbalancea la diapositiva
  cuando la animación llega a su mensaje final. La plantilla ya trae
  `.c2>*{min-width:0}` y `minmax(0,1fr)` en la carrera. Al agregar otra grilla, hazle lo
  mismo, y prueba la animación hasta el final, no solo el primer paso.
- **Con `min-width:0`, lo largo se corta en vez de empujar.** Los comentarios largos de un
  bloque de código quedan cortados. Acórtalos.
- **Un hijo de la diapositiva se encoge en vez de desbordarse.** La diapositiva es una
  columna flex de alto fijo, así que un `div` con `height` se aplasta sin aviso. El
  desborde que el script detecta es el de texto que no cabe. Por eso hay que mirar las
  capturas.
- **Lo que la animación cuenta puede regalar un ejercicio.** La conferencia 8 muestra
  solo comparaciones, porque el ejercicio 6 pide contar las escrituras.
- **Una afirmación fácil de creer puede ser falsa.** Plantar un error para la demo en
  vivo exige correrlo primero: el que parecía obvio, invertir las condiciones del `while`
  de la inserción, no rompe nada en Python.
