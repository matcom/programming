# El libro de Programación

Las conferencias del curso 2026-2027 se publican como un solo libro, con cada
conferencia como un capítulo. Este directorio es el proyecto de
[scriptorium](https://github.com/apiad/scriptorium) que lo construye. Hay dos salidas:

- `programacion.pdf`, el libro completo. Se construye en local y se sube al repositorio.
- `sitio/`, la documentación en línea. La construye el workflow `libro` en cada push a
  `main` y la publica en <https://matcom.github.io/programming/>, con el PDF al lado.

No hay PDF por conferencia.

## Agregar un capítulo

1. Escribe el capítulo en `conferences/2026/<NN>-<tema>/notas.md`. Empieza con
   `# Título {#chap-<tema>}`, sin frontmatter ni línea de fecha. El número del capítulo
   lo pone el libro. En el texto, las referencias a otros capítulos van como
   "el capítulo N", con el número del libro, que no coincide con el de la conferencia
   desde la 8, porque la semana 7 fue examen.
2. Agrégalo a `files:` en `scriptorium.yaml`, en su lugar.
3. Los bloques de código corren con este directorio como directorio de trabajo: `..` es
   `conferences/2026`, donde está `figuras.py`, y los archivos de datos (`quijote.txt`,
   `quijote-cap1.txt`) viven aquí.
4. Construye y verifica, como dice la sección siguiente.
5. Sube el capítulo, `programacion.pdf` y `.scriptorium/freeze.json`.

## Construir

Hace falta scriptorium 0.12.0 con el extra `pdf`:

```bash
uv tool install --force 'scriptorium[pdf] @ git+https://github.com/apiad/scriptorium@v0.12.0'
cd conferences/2026/libro
scriptorium render scriptorium.yaml          # programacion.pdf y sitio/
python3 arreglar-sitio.py sitio              # dos arreglos al sitio, ver el script
```

Después:

```bash
pdftotext programacion.pdf - | grep -ci traceback   # los que el texto muestra a propósito: 21
pdftotext programacion.pdf - | grep -c '^Figura '    # una por figura
```

Un bloque que falla no rompe el render: scriptorium pinta el traceback dentro del libro.
Por eso hay que contar los tracebacks.

## La caché

`.scriptorium/freeze.json` guarda la salida de cada bloque, con su código como clave.
Se sube al repositorio por dos razones: los capítulos ya publicados conservan sus
números al reconstruir, y el sitio que construye el workflow muestra los mismos tiempos
que el PDF. Editar la prosa no vuelve a correr nada. Editar un bloque lo vuelve a
correr, a él y a los bloques `continue` que vienen después. La caché no ve `figuras.py`:
después de cambiar una figura, borra de `freeze.json` las entradas de los bloques que la
dibujan.

Los tiempos que miden los capítulos dependen de la carga de la máquina. Si un capítulo
se vuelve a medir, hay que comprobar que la prosa siga diciendo lo que imprimen sus
bloques.

## Lo que se arregla aquí y no en scriptorium

- `libro.css` pone en español las etiquetas del tema `book`, y oculta los números de
  página del índice, que scriptorium 0.12 escribe siempre como 0.
- `sitio.js` traduce las etiquetas del sitio, que scriptorium escribe en inglés.
- `arreglar-sitio.py` devuelve a su página las referencias internas de las fórmulas y
  escribe "figura N" en las referencias a figuras del sitio.

Cuando scriptorium arregle cada una, se borra el arreglo correspondiente.
