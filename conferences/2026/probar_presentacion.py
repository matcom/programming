# /// script
# requires-python = ">=3.11"
# dependencies = ["playwright==1.61.0"]
# ///
"""Prueba una presentación HTML de conferencia en Chromium sin ventana.

Uso:
    uv run conferences/2026/probar_presentacion.py <presentacion.html> [carpeta-de-capturas]

La primera vez hace falta el navegador: uv run --with playwright playwright install chromium

Comprueba tres cosas y sale con código 1 si falla alguna:
- que la página no da errores en la consola;
- que ningún elemento se sale de su diapositiva (abajo o a la derecha);
- que cada animación llega a su final en cada uno de sus escenarios.

Imprime el mensaje final de cada animación en cada escenario. Esos mensajes hay que
leerlos y compararlos con lo que hace el algoritmo: el script ve que la animación
termina, no que termine bien.

Si se le da una carpeta, guarda una captura de cada diapositiva a 1600x900. En las
diapositivas con animación, la captura se toma después de seis pasos, para que se vean
los índices y los colores.
"""

import sys
from pathlib import Path

from playwright.sync_api import sync_playwright

PASOS_CAPTURA = 6
MAX_PASOS = 5000

DESBORDES = """() => {
  const s = slides[cur], r = s.getBoundingClientRect(), malos = [];
  s.querySelectorAll('*').forEach(e => {
    const q = e.getBoundingClientRect();
    if (q.width && (q.bottom > r.bottom - 4 || q.right > r.right + 1))
      malos.push(`${e.tagName.toLowerCase()}.${e.className} (${Math.round(q.bottom - r.bottom)} px)`);
  });
  return malos.slice(0, 3);
}"""

# Cada controlador de animación (Viz, Carrera y los propios de cada deck) cumple el
# contrato del know-how: reset(), next() que da false al terminar, msg con su estado y
# cfg.scen si tiene escenarios.
FINALES = """([k, max]) => slides[k]._ctl.map(c => {
  const n = c.cfg && c.cfg.scen ? c.cfg.scen.length : 1, out = [];
  for (let si = 0; si < n; si++) {
    if (c.cfg && c.cfg.scen) c.si = si;
    c.reset();
    let pasos = 0;
    while (c.next() && pasos < max) pasos++;
    const vistas = c.lanes || [c];
    const msg = vistas.map(v => v.msg ? v.msg.textContent : '(sin msg)').join(' | ');
    out.push({ si, pasos, termino: pasos < max, msg });
  }
  c.reset();
  return out;
})"""


def main() -> int:
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    html = Path(sys.argv[1]).resolve()
    capturas = Path(sys.argv[2]) if len(sys.argv) > 2 else None
    if capturas:
        capturas.mkdir(parents=True, exist_ok=True)

    errores, desbordes, sin_final, corridas = [], [], [], 0
    with sync_playwright() as p:
        navegador = p.chromium.launch()
        pagina = navegador.new_page(viewport={"width": 1600, "height": 900})
        pagina.on("pageerror", lambda e: errores.append(str(e)))
        pagina.on("console", lambda m: m.type == "error" and errores.append(m.text))
        pagina.goto(html.as_uri())
        pagina.wait_for_timeout(500)
        total = pagina.evaluate("slides.length")

        for k in range(total):
            pagina.evaluate(f"go({k})")
            pagina.wait_for_timeout(450)
            malos = pagina.evaluate(DESBORDES)
            if malos:
                desbordes.append((k + 1, malos))
            animaciones = pagina.evaluate(f"slides[{k}]._ctl.length")
            if capturas:
                for _ in range(PASOS_CAPTURA if animaciones else 0):
                    pagina.evaluate("slides[cur]._ctl.forEach(c => c.step())")
                pagina.wait_for_timeout(500 if animaciones else 0)
                pagina.screenshot(path=str(capturas / f"diapositiva-{k + 1:02d}.png"))
            if not animaciones:
                continue
            for a, escenarios in enumerate(pagina.evaluate(FINALES, [k, MAX_PASOS])):
                for e in escenarios:
                    corridas += 1
                    marca = "ok " if e["termino"] else "NO TERMINA"
                    print(f"diapositiva {k + 1}, animación {a}, escenario {e['si']}: {marca} "
                          f"{e['pasos']} pasos · {e['msg']}")
                    if not e["termino"]:
                        sin_final.append((k + 1, a, e["si"]))
        navegador.close()

    for k, malos in desbordes:
        print(f"diapositiva {k} se desborda: {', '.join(malos)}")
    for e in errores:
        print(f"error en la consola: {e}")
    print(f"{total} diapositivas, {corridas} corridas de animación, {len(desbordes)} desbordes, "
          f"{len(sin_final)} sin final, {len(errores)} errores")
    return 1 if errores or desbordes or sin_final else 0


if __name__ == "__main__":
    sys.exit(main())
