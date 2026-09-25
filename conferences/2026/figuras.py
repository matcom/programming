"""Figuras de las conferencias de Programación 2026.

Cada función pública devuelve una cadena `<figure>` lista para imprimir desde un
bloque ```{python echo=false output=asis} de scriptorium.

La regla que siguen: una figura que ilustra un algoritmo recibe el estado que el
algoritmo calculó, nunca posiciones puestas a mano. La criba recibe qué primo tachó
a cada número, las barras reciben los segundos y los conteos que el propio documento
acaba de medir. Así una figura no puede contradecir al código de la conferencia.

El procedimiento, las trampas y la verificación están en
`repos/algos/know-how/ilustrando-una-conferencia.md`.
"""

from tesserax import Canvas, Circle, Line, Point, Rect, Text
from tesserax.color import hex

# Paleta copiada de `themes/note/theme.yml` de scriptorium, para que las figuras y
# el documento sean el mismo objeto visual. Está duplicada a propósito: WeasyPrint no
# resuelve `currentColor` ni `var(--x)` dentro de un SVG en línea, así que el color
# tiene que ser un hex literal en el Python. La misma paleta vive en
# `repos/algos/Lectures/2026/figuras.py`; si una cambia, cotejar la otra.
TINTA = hex("#1a1a1a")
APAGADO = hex("#6b7280")
REGLA = hex("#d8dce3")
ACENTO = hex("#0891b2")
OSCURO = hex("#0e7490")
CALIDO = hex("#b45309")
ALARMA = hex("#be123c")
VERDE = hex("#15803d")

CUERPO = 9.0
PIE = 7.5
MONO = "JetBrains Mono, monospace"
SANS = "Inter, sans-serif"


# --------------------------------------------------------------------------
# infraestructura
# --------------------------------------------------------------------------

def _figura(canvas, ident, pie, padding=14):
    canvas.fit(padding)
    return (f'<figure id="fig-{ident}">{canvas}'
            f'<figcaption>{pie}</figcaption></figure>')


def _txt(texto, x, y, color=TINTA, size=CUERPO, font=SANS, ancla="center"):
    """`ancla` dice qué parte del texto cae sobre (x, y): "center", "left" para
    alinear por la izquierda, "right" para alinear por la derecha.

    El `anchor` del propio `Text` no sirve para colocar. No es que esté roto:
    tesserax compensa el translate de cada `text-anchor`, así que las tres anclas
    producen exactamente la misma caja, y después `move_to` la recentra sobre el
    punto. El ancla que coloca es la de `move_to`, que es de caja."""
    return Text(texto, size=size, fill=color, anchor="middle",
                font=font).move_to(Point(x, y), anchor=ancla)


def _codigo(texto, x, y, color=TINTA, size=CUERPO, ancla="center"):
    return _txt(texto, x, y, color=color, size=size, font=MONO, ancla=ancla)


def _caja(x, y, w, h, color, relleno=0.07, grosor=1.2):
    """Rectángulo anclado por su esquina superior izquierda."""
    return Rect(w, h, fill=color.transparent(relleno), stroke=color,
                width=grosor).move_to(Point(x, y), anchor="topleft")


def _flecha(x0, y0, x1, y1, color=CALIDO, grosor=1.1):
    return Line(Point(x0, y0), Point(x1, y1), stroke=color, width=grosor,
                marker_end="arrow")


# --------------------------------------------------------------------------
# conferencia 3: funciones
# --------------------------------------------------------------------------

def parametro_y_argumento(ident="parametro-argumento", pie=""):
    """El hueco de la definición y el valor de la llamada."""
    with Canvas() as canvas:
        _caja(0, 46, 112, 30, OSCURO)
        _codigo("es_primo(91)", 56, 61, OSCURO)
        _txt("la llamada", 56, 32, APAGADO, PIE)

        _flecha(118, 61, 172, 61)
        _codigo("91", 145, 50, CALIDO, PIE)
        _txt("argumento", 145, 73, CALIDO, PIE)

        _caja(178, 34, 176, 56, ACENTO)
        _codigo("def es_primo(n):", 266, 52, ACENTO)
        _txt("↑", 296, 66, CALIDO, PIE)
        _txt("parámetro", 296, 78, CALIDO, PIE)
        _txt("la definición", 266, 22, APAGADO, PIE)

        _flecha(360, 61, 414, 61)
        _codigo("return", 387, 50, CALIDO, PIE)
        _codigo("False", 443, 61, TINTA)
    return _figura(canvas, ident, pie)


def return_y_break(ident="return-break", pie=""):
    """Dos paneles: `break` sale del ciclo, `return` sale del ciclo y de la
    función a la vez."""
    with Canvas() as canvas:
        for fila, clave in enumerate(("break", "return")):
            y = fila * 112
            _codigo(clave, -12, y + 40, CALIDO if fila == 0 else ALARMA,
                    ancla="right")

            _caja(0, y, 262, 82, ACENTO, relleno=0.04)
            _txt("la función", 8, y + 9, ACENTO, PIE, ancla="left")

            _caja(14, y + 18, 234, 36, OSCURO, relleno=0.07)
            _txt("el ciclo", 22, y + 26, OSCURO, PIE, ancla="left")
            Circle(3.2, fill=ALARMA, stroke=ALARMA).move_to(Point(62, y + 43))

            if fila == 0:
                Line(Point(62, y + 48), Point(62, y + 66), stroke=CALIDO,
                     width=1.1)
                _flecha(62, y + 66, 144, y + 66, CALIDO)
                _txt("sigue dentro de la función", 152, y + 66, APAGADO, PIE,
                     ancla="left")
            else:
                _flecha(68, y + 43, 330, y + 43, ALARMA, grosor=1.3)
                _txt("atraviesa los dos bordes", 150, y + 66, ALARMA, PIE)
                _txt("a quien llamó", 338, y + 43, ALARMA, PIE, ancla="left")
    return _figura(canvas, ident, pie)


def un_solo_lugar(ident="un-solo-lugar", pie=""):
    """Tres programas llaman a la misma función; la mejora se hace una vez."""
    llamadores = ["primos_hasta", "contar_primos", "medir"]
    with Canvas() as canvas:
        _txt("los que llaman", 70, 8, APAGADO, PIE)
        for i, nombre in enumerate(llamadores):
            y = 22 + i * 38
            _caja(0, y, 140, 26, OSCURO)
            _codigo(nombre, 70, y + 13, OSCURO)
            _flecha(146, y + 13, 226, 59 + i * 14)

        _caja(232, 46, 116, 54, ALARMA)
        _codigo("es_primo", 290, 73, ALARMA)
        _txt("una línea cambia aquí", 290, 116, ALARMA, PIE)
        _txt("y los tres corren más rápido", 290, 130, APAGADO, PIE)
    return _figura(canvas, ident, pie)


# --------------------------------------------------------------------------
# conferencia 4: listas
# --------------------------------------------------------------------------

def fila_de_casillas(valores, negativos=True, resaltar=None, etiqueta="",
                     nombre="", ident="fila", pie=""):
    """Una fila de casillas con sus índices. `resaltar` es un par (i, j) medio
    abierto, como una rebanada, y `etiqueta` es el texto de la cota de abajo."""
    lado = 30.0
    n = len(valores)
    with Canvas() as canvas:
        if nombre:
            _codigo(nombre, -12, lado / 2, TINTA, ancla="right")
        for i, v in enumerate(valores):
            x = i * lado
            dentro = resaltar is not None and resaltar[0] <= i < resaltar[1]
            color = ACENTO if dentro else REGLA.darker(0.25)
            Rect(lado, lado, fill=ACENTO.transparent(0.16) if dentro
                 else REGLA.transparent(0.35), stroke=color,
                 width=1.4 if dentro else 0.9).move_to(Point(x, 0),
                                                       anchor="topleft")
            _codigo(str(v), x + lado / 2, lado / 2, TINTA)
            _codigo(str(i), x + lado / 2, -11, APAGADO, PIE)
            if negativos:
                _codigo(str(i - n), x + lado / 2, lado + 11, APAGADO, PIE)

        base = lado + (22 if negativos else 10)
        if resaltar is not None and etiqueta:
            a, b = resaltar[0] * lado, resaltar[1] * lado
            Line(Point(a, base), Point(b, base), stroke=ACENTO, width=1.1,
                 marker_start="arrow", marker_end="arrow")
            _codigo(etiqueta, (a + b) / 2, base + 11, ACENTO, PIE)
        elif negativos:
            _txt("índices desde el final", n * lado / 2, base + 4, APAGADO, PIE)
    return _figura(canvas, ident, pie)


def dos_nombres_una_lista(valores, ident="alias", pie=""):
    """`b = a` copia el nombre, no la lista."""
    lado = 30.0
    with Canvas() as canvas:
        for etiqueta, y, destino in (("a", 6, 27), ("b", 52, 41)):
            _caja(0, y, 34, 26, OSCURO)
            _codigo(etiqueta, 17, y + 13, OSCURO)
            _flecha(40, y + 13, 104, destino)
        _txt("dos nombres", 17, -8, APAGADO, PIE)

        for i, v in enumerate(valores):
            x = 110 + i * lado
            Rect(lado, lado, fill=REGLA.transparent(0.35),
                 stroke=REGLA.darker(0.25), width=0.9).move_to(
                     Point(x, 19), anchor="topleft")
            _codigo(str(v), x + lado / 2, 34, TINTA)
        ancho = len(valores) * lado
        _txt("una sola lista", 110 + ancho / 2, 8, APAGADO, PIE)
        _codigo("a.append(4)", 110 + ancho / 2, 64, ALARMA, PIE)
        _txt("modifica la que ven los dos", 110 + ancho / 2, 76, ALARMA, PIE)
    return _figura(canvas, ident, pie)


def rastro_de_la_criba(n, primos):
    """{número: primo que lo tachó}, derivado de la lista que devolvió el código de
    la conferencia. El primo que tacha a un compuesto es su menor factor primo, así
    que si esa lista estuviera mal la figura se equivocaría igual y no podría mentir."""
    sin_tachar = set(primos)
    rastro = {}
    for k in range(2, n + 1):
        if k in sin_tachar:
            continue
        for q in primos:
            if k % q == 0:
                rastro[k] = q
                break
    return rastro


def reparto_de_tachones(n, primos):
    """{primo: cuántas casillas tachó}. Sirve para decir en el pie de la figura lo
    desparejo que es el reparto, en vez de maquillarlo."""
    reparto = {}
    for q in rastro_de_la_criba(n, primos).values():
        reparto[q] = reparto.get(q, 0) + 1
    return reparto


def criba_visual(n, primos, por_fila=12, ident="criba", pie=""):
    """Dibuja la criba hasta `n`, con cada casilla tachada del color del primo que
    la tachó."""
    tachado_por = rastro_de_la_criba(n, primos)
    lado = 28.0
    colores = {2: CALIDO, 3: VERDE, 5: ALARMA, 7: APAGADO}
    with Canvas() as canvas:
        for k in range(2, n + 1):
            i = k - 2
            x, y = (i % por_fila) * lado, (i // por_fila) * lado
            p = tachado_por.get(k)
            if p is None:
                Rect(lado, lado, fill=ACENTO.transparent(0.18), stroke=ACENTO,
                     width=1.5).move_to(Point(x, y), anchor="topleft")
                _codigo(str(k), x + lado / 2, y + lado / 2, TINTA)
            else:
                c = colores.get(p, APAGADO)
                Rect(lado, lado, fill=c.transparent(0.10),
                     stroke=REGLA.darker(0.2), width=0.8).move_to(
                         Point(x, y), anchor="topleft")
                # la raya va antes que el número, para que el número quede encima
                Line(Point(x + 5, y + lado - 5), Point(x + lado - 5, y + 5),
                     stroke=c.transparent(0.5), width=1.0)
                _codigo(str(k), x + lado / 2, y + lado / 2, APAGADO, PIE)

        filas = (n - 1 + por_fila - 1) // por_fila
        base = filas * lado + 18
        entradas = [(ACENTO, "sin tachar: primo")] + [
            (colores.get(q, APAGADO), f"tachado por {q}")
            for q in sorted(set(tachado_por.values()))]
        paso = por_fila * lado / len(entradas)
        for j, (c, texto) in enumerate(entradas):
            x = j * paso
            Rect(9, 9, fill=c.transparent(0.35), stroke=c, width=1.0).move_to(
                Point(x, base - 4), anchor="topleft")
            _txt(texto, x + 14, base, c, PIE, ancla="left")
    return _figura(canvas, ident, pie)


def crecimiento(pasos, ident="crecimiento", pie=""):
    """`pasos` es [(etiqueta, factor_divisiones, factor_tachones)]. La línea
    horizontal marca el 3, que es lo que se multiplicó el tope."""
    ancho, alto = 250.0, 124.0
    tope = max(max(a, b) for _, a, b in pasos) * 1.22
    esc = lambda v: v / tope * alto
    barra, hueco = 26.0, 14.0
    grupo = 2 * barra + hueco
    paso_g = ancho / len(pasos)
    with Canvas() as canvas:
        for e in range(1, int(tope) + 1):
            y = alto - esc(e)
            Line(Point(-4, y), Point(ancho, y), stroke=REGLA, width=0.7)
            _txt(f"×{e}", -10, y, APAGADO, PIE, ancla="right")

        y3 = alto - esc(3)
        Line(Point(-4, y3), Point(ancho + 4, y3), stroke=CALIDO, width=1.2)
        _txt("el tope se multiplica por 3", ancho + 12, y3, CALIDO, PIE,
             ancla="left")

        for g, (etiqueta, div, tac) in enumerate(pasos):
            base = g * paso_g + (paso_g - grupo) / 2
            for k, v, color in ((0, tac, ACENTO), (1, div, ALARMA)):
                h = esc(v)
                x = base + k * (barra + hueco)
                Rect(barra, h, fill=color.transparent(0.25), stroke=color,
                     width=1.1).move_to(Point(x, alto - h), anchor="topleft")
                _txt(f"×{v:.1f}", x + barra / 2, alto - h - 7, color, PIE)
            _txt(etiqueta, g * paso_g + paso_g / 2, alto + 12, APAGADO, PIE)

        Line(Point(0, alto), Point(ancho, alto), stroke=APAGADO, width=1.0)
        _txt("criba", ancho + 12, -6, ACENTO, PIE, ancla="left")
        _txt("uno por uno", ancho + 12, 8, ALARMA, PIE, ancla="left")
    return _figura(canvas, ident, pie)


# --------------------------------------------------------------------------
# conferencia 1: lo básico
# --------------------------------------------------------------------------

def traza_de_memoria(pasos, nace=None, sin_vinculo=None, ident="traza", pie=""):
    """`pasos` es [(instrucción, {nombre: valor})], tal como quedó la memoria
    después de ejecutar esa instrucción. `nace` es (columna, origen, destino,
    etiqueta) y dibuja de dónde salió un valor; `sin_vinculo` es la columna en la
    que ese vínculo ya no existe."""
    ancho, alto_caja, paso_fila = 116.0, 24.0, 34.0
    nombres = []
    for _, estado in pasos:
        for n in estado:
            if n not in nombres:
                nombres.append(n)

    def caja(col, nombre):
        x = col * ancho + 30
        y = 24 + nombres.index(nombre) * paso_fila
        return x, y

    with Canvas() as canvas:
        for col, (instruccion, estado) in enumerate(pasos):
            x0 = col * ancho
            _codigo(instruccion, x0 + ancho / 2, 6, TINTA, PIE)
            for nombre in nombres:
                x, y = caja(col, nombre)
                _codigo(nombre, x - 6, y + alto_caja / 2, APAGADO, PIE,
                        ancla="right")
                if nombre not in estado:
                    _txt("no existe", x + 26, y + alto_caja / 2, REGLA.darker(0.4),
                         PIE)
                    continue
                cambio = col == 0 or estado.get(nombre) != pasos[col - 1][1].get(nombre)
                color = ACENTO if cambio else REGLA.darker(0.25)
                Rect(52, alto_caja, fill=ACENTO.transparent(0.16) if cambio
                     else REGLA.transparent(0.35), stroke=color,
                     width=1.4 if cambio else 0.9).move_to(Point(x, y),
                                                           anchor="topleft")
                _codigo(str(estado[nombre]), x + 26, y + alto_caja / 2, TINTA)

        if nace:
            col, origen, destino, etiqueta = nace
            xo, yo = caja(col, origen)
            xd, yd = caja(col, destino)
            _flecha(xo + 26, yo + alto_caja + 1, xd + 26, yd - 1, CALIDO)
            _codigo(etiqueta, xo + 58, (yo + alto_caja + yd) / 2, CALIDO, PIE,
                    ancla="left")

        if sin_vinculo is not None and nace:
            col, origen, destino, _ = sin_vinculo, nace[1], nace[2], None
            xo, yo = caja(col, origen)
            xd, yd = caja(col, destino)
            medio = (yo + alto_caja + yd) / 2
            Line(Point(xo + 26, yo + alto_caja + 2), Point(xd + 26, yd - 2),
                 stroke=REGLA.darker(0.3), width=0.9)
            _txt("✗", xo + 26, medio, ALARMA, CUERPO)
            _txt("sin vínculo", xd + 26, yd + alto_caja + 11, APAGADO, PIE)
    return _figura(canvas, ident, pie)


# --------------------------------------------------------------------------
# conferencia 2: condicionales y ciclos
# --------------------------------------------------------------------------

def partes_del_while(ident="partes-while", pie=""):
    """Las tres partes de un ciclo, sobre la cuenta atrás de la conferencia."""
    x, ancho = 70.0, 150.0
    filas = [("inicializar", "n = 5", OSCURO, 0),
             ("condición", "n > 0", CALIDO, 46),
             ("cuerpo", "print(n)", TINTA, 100),
             ("avanzar", "n = n - 1", OSCURO, 146)]
    with Canvas() as canvas:
        for etiqueta, codigo, color, y in filas:
            _caja(x, y, ancho, 26, color if color is not TINTA else ACENTO)
            _codigo(codigo, x + ancho / 2, y + 13, TINTA)
            _txt(etiqueta, x + 2, y - 6, color if color is not TINTA else APAGADO,
                 PIE, ancla="left")
        for y in (26, 126):
            _flecha(x + ancho / 2, y, x + ancho / 2, y + 20, APAGADO)
        _flecha(x + ancho / 2, 72, x + ancho / 2, 100, APAGADO)
        _codigo("True", x + ancho / 2 + 8, 86, CALIDO, PIE, ancla="left")

        # la salida por la condición falsa
        _flecha(x + ancho, 59, x + ancho + 70, 59, CALIDO)
        _codigo("False", x + ancho + 34, 49, CALIDO, PIE)
        _txt("sigue el programa", x + ancho + 76, 59, APAGADO, PIE, ancla="left")

        # la vuelta al principio
        Line(Point(x, 159), Point(x - 40, 159), stroke=APAGADO, width=1.0)
        Line(Point(x - 40, 159), Point(x - 40, 59), stroke=APAGADO, width=1.0)
        _flecha(x - 40, 59, x, 59, APAGADO)
        _txt("otra vuelta", x - 20, 176, APAGADO, PIE)
    return _figura(canvas, ident, pie)


def rango_que_se_parte(tamanos, ident="rango-binario", pie=""):
    """`tamanos` son los números que quedan vivos después de cada pregunta, tal
    como los produjo el ciclo de la búsqueda binaria."""
    ancho, alto, paso = 300.0, 15.0, 21.0
    tope = tamanos[0]
    with Canvas() as canvas:
        for i, n in enumerate(tamanos):
            y = i * paso
            largo = max(n / tope * ancho, 1.2)
            color = ACENTO if i == len(tamanos) - 1 else OSCURO
            Rect(largo, alto, fill=color.transparent(0.22), stroke=color,
                 width=1.0).move_to(Point(0, y), anchor="topleft")
            _codigo(f"{n}", largo + 16, y + alto / 2, color, PIE)
            etiqueta = "al empezar" if i == 0 else f"pregunta {i}"
            _txt(etiqueta, -10, y + alto / 2, APAGADO, PIE, ancla="right")
        _txt("números que todavía pueden ser", ancho / 2,
             len(tamanos) * paso + 6, APAGADO, PIE)
    return _figura(canvas, ident, pie)


# --------------------------------------------------------------------------
# conferencia 5: cadenas
# --------------------------------------------------------------------------

def split_visual(texto, piezas, ident="split", pie=""):
    """`piezas` es lo que devolvió `split`, no una lista escrita a mano."""
    lado, hueco = 13.0, 10.0
    with Canvas() as canvas:
        for i, ch in enumerate(texto):
            x = i * lado
            hueca = ch == " "
            Rect(lado, 22, fill=REGLA.transparent(0.15 if hueca else 0.35),
                 stroke=REGLA.darker(0.1 if hueca else 0.25),
                 width=0.8).move_to(Point(x, 0), anchor="topleft")
            _codigo("·" if hueca else ch, x + lado / 2, 11,
                    APAGADO if hueca else TINTA, PIE)
        ancho = len(texto) * lado
        _codigo("texto", -10, 11, TINTA, PIE, ancla="right")

        _flecha(ancho / 2, 26, ancho / 2, 48, CALIDO)
        _codigo(".split()", ancho / 2 + 8, 37, CALIDO, PIE, ancla="left")

        x = 0.0
        for pieza in piezas:
            w = len(pieza) * lado
            _caja(x, 54, w, 22, ACENTO, relleno=0.16)
            _codigo(pieza, x + w / 2, 65, TINTA, PIE)
            x += w + hueco
        _txt("una lista, y los espacios ya no están", (x - hueco) / 2, 88,
             APAGADO, PIE)
    return _figura(canvas, ident, pie)
