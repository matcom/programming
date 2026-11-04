"""Arregla el sitio que escribe `scriptorium render --only sitio`.

Dos cosas de scriptorium 0.12 que este libro no puede ajustar desde su proyecto:

- Reescribe como enlaces entre capítulos también los `xlink:href="#MJX-…"` con que
  cada fórmula de MathJax apunta a sus propios glifos, porque el mismo id aparece en
  otra página. La fórmula queda apuntando a otro archivo y no se dibuja. Aquí cada
  referencia vuelve a su propia página.
- Una referencia `@fig-…` del tema `book` cita el pie entero de la figura entre
  comillas. Aquí dice "figura N", con el número que la figura lleva en el libro.

Uso: python3 arreglar-sitio.py sitio
"""

import pathlib
import re
import sys

sitio = pathlib.Path(sys.argv[1])
FORMULA = re.compile(r'(xlink:href|href)="[^"#]*\.html(#MJX-[^"]*)"')
FIGURA = re.compile(r'<figure id="(fig-[^"]+)"')
REF = re.compile(r'(<a class="ref-fig" href="[^"#]*#(fig-[^"]+)">).*?(</a>)', re.S)

# El orden de las páginas es el de la barra lateral de la portada.
indice = (sitio / "index.html").read_text(encoding="utf-8")
paginas = []
for nombre in re.findall(r'href="([a-z0-9-]+\.html)', indice):
    if nombre not in paginas and (sitio / nombre).exists():
        paginas.append(nombre)

numero = {}
for nombre in paginas:
    for ident in FIGURA.findall((sitio / nombre).read_text(encoding="utf-8")):
        numero[ident] = len(numero) + 1

formulas = referencias = 0
for pagina in sitio.glob("*.html"):
    texto = pagina.read_text(encoding="utf-8")
    texto, n = FORMULA.subn(r'\1="\2"', texto)
    formulas += n
    texto, n = REF.subn(lambda m: f"{m.group(1)}figura {numero[m.group(2)]}{m.group(3)}", texto)
    referencias += n
    pagina.write_text(texto, encoding="utf-8")

print(f"{formulas} fórmulas devueltas a su página, {referencias} referencias a figuras, "
      f"{len(numero)} figuras en {len(paginas)} páginas")
