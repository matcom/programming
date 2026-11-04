// scriptorium 0.12 escribe en inglés las etiquetas del sitio de documentación.
// Este libro va en español, así que las traducimos al cargar la página.
(function () {
  const textos = {"On this page": "En esta página", "Source": "Código fuente",
                  "No results": "Sin resultados"};
  function traducir(raiz) {
    raiz.querySelectorAll("p, a, div, span").forEach(function (el) {
      if (el.children.length === 0 && textos[el.textContent.trim()]) {
        el.textContent = textos[el.textContent.trim()];
      }
    });
  }
  const buscar = document.querySelector('.search input');
  if (buscar) { buscar.placeholder = "Buscar"; buscar.setAttribute("aria-label", "Buscar"); }
  traducir(document);
  const resultados = document.querySelector('.search-results');
  if (resultados) {
    new MutationObserver(function () { traducir(resultados); })
      .observe(resultados, {childList: true, subtree: true});
  }
})();
