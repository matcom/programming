# matcom/programming

Materiales del curso de Programación de MatCom (Universidad de La Habana): conferencias,
ejercicios, exámenes y proyectos. Los leen los estudiantes y los usa el profesor en el
aula, así que todo lo que se publica está en español.

## Dónde está cada cosa

- `conferences/2026/`: el curso actual, en Python. Cada conferencia es una carpeta
  `<NN>-<tema>/` con su capítulo (`notas.md`) y su presentación (`presentacion.html`).
  `plan.md` es el plan del semestre, y `libro/` el proyecto de scriptorium que junta los
  capítulos en el libro publicado; su README dice cómo se construye.
- El resto de `conferences/`, `exercises/`, `exams/` y `projects/` son del curso
  anterior, en C#, y se conservan como referencia.

## Cómo se trabaja

Los cambios llegan por PR desde una rama, y se mezclan cuando el build del libro está en
verde, porque ese build es el que publica el sitio de los estudiantes.

Los procedimientos están en `know-how/`. El menú:

```bash
grep -m1 -H '^when:' know-how/*.md
```
