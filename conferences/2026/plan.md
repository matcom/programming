# Programación: plan de conferencias del primer semestre, curso 2026-2027

Este plan cubre el primer semestre de Programación en el plan de estudio de Ciencia de
la Computación y de Ciencia de Datos. El programa de la asignatura reparte 192 horas
entre dos semestres y tres temas: 48 horas para recursos básicos y programación
estructurada, 48 para estrategias de solución y recursividad, y 96 para orientación a
objetos, programación funcional y estructuras de datos. Los dos primeros temas suman
96 horas, que es exactamente un semestre, así que **el primer semestre llega hasta
programación dinámica y el segundo es el tema tercero completo.** La corrida de 2024
se partió en ese mismo punto: once conferencias hasta programación dinámica y de la
duodécima en adelante, herencia.

| Tema | Conferencias |
|---|---|
| 1. Recursos básicos. Programación estructurada | 7 |
| 2. Estrategias de solución y recursividad | 5 |

Se imparte una conferencia semanal, los miércoles, más hasta dos clases prácticas en
aula o laboratorio. El semestre tiene dieciséis semanas nominales; descontando la
primera, la última y las que siempre se pierden, quedan doce de contenido. Este plan
tiene doce conferencias y ninguna de reserva: si se pierde una semana, la conferencia
que se funde con su vecina es la quinta dentro de la cuarta, y si se pierden dos,
también la undécima dentro de la décima.

El curso se imparte en **Python**. El cambio de lenguaje empezó el curso pasado por el
proyecto, cuyo verificador automático ya corre código Python, y estas conferencias son
la última parte en migrar. La bibliografía del programa es un libro de C#, pero sus
indicaciones metodológicas dejan la elección del lenguaje a cada centro, con la
condición de que sea un lenguaje de difusión real en la producción.

Cada conferencia se organiza alrededor de un proyecto que se construye en vivo, con
código que se ejecuta y se modifica delante de los estudiantes. No hay presentaciones
sobre temas abstractos. Se enuncia un problema, se escribe el programa que lo
resuelve, se rompe a propósito para ver qué error da, y se arregla. Al final se
orientan ejercicios sobre ese mismo proyecto, y la clase práctica siguiente trata
problemas que el estudiante no ha visto.

Las notas de cada conferencia se escriben en Markdown y se renderizan a PDF con
scriptorium, que ejecuta los bloques de código al construir el documento. Un ejemplo
que no corre no llega al PDF, y los mensajes de error que aparecen en las notas son
los que Python imprime de verdad en la versión que se usa en clase.

Las figuras se dibujan con tesserax desde `conferences/2026/figuras.py` y siguen la
misma regla: una figura que ilustra un algoritmo recibe el estado que el algoritmo
calculó, nunca posiciones puestas a mano. La criba coloreada recibe la lista de primos
que devolvió el código de la conferencia, y las barras de crecimiento reciben los
conteos que el documento acaba de medir, así que una figura no puede contradecir al
texto que tiene al lado. El procedimiento está en
`repos/algos/know-how/ilustrando-una-conferencia.md`.

El curso no asume ningún conocimiento previo de programación. Sí asume el
preuniversitario completo: aritmética, álgebra elemental, la ecuación de segundo
grado, funciones y el lenguaje de conjuntos.

Sobre el uso de asistentes de inteligencia artificial, la regla del colectivo se
mantiene. En clases prácticas y exámenes solo se cuenta con la computadora y el
conocimiento del estudiante. En los proyectos se permite cualquier recurso externo
siempre que se declare de forma honesta y que su uso no impida demostrar las
habilidades que el curso quiere formar.

Bibliografía básica: la documentación oficial de Python, y Katrib y colaboradores,
*Empiece a programar. Un enfoque multiparadigma con C#*, Editorial UH 2017, para los
capítulos independientes del lenguaje.

## Tema 1. Recursos básicos. Programación estructurada

### Conferencia 1. Lo básico de Python

Proyecto: una calculadora de las raíces de la ecuación de segundo grado, construida
pieza a pieza durante la hora. Qué es un programa y qué es la consola interactiva.
Expresiones y precedencia. Los tipos `int`, `float` y `str`, con la división entera, los
enteros sin límite de tamaño y los reales aproximados. Instrucciones y variables, con
la asignación explicada como un suceso en el tiempo y no como una igualdad
matemática, trazando la memoria instrucción por instrucción. `print`, sus parámetros
`sep` y `end`, y los f-strings. `input`, que siempre devuelve texto, y las conversiones.
El módulo `math`. La conferencia termina con la calculadora rota: falla cuando el
discriminante es negativo y falla cuando el coeficiente principal es cero.

### Conferencia 2. Condicionales y ciclos

Proyecto: cerrar la calculadora de la conferencia anterior, y después el juego de
adivinar un número entre 1 y 100. El tipo `bool` y los operadores de comparación.
`if`, `elif` y `else`, con la indentación explicada como sintaxis y no como estilo, que
es donde Python se separa de casi todos los demás lenguajes. Los operadores `and`, `or`
y `not`. Por qué dos números reales nunca se comparan con `==`, que quedó pendiente de
la primera conferencia, y por qué compararlos contra cero necesita además una
tolerancia absoluta. El ciclo `while` con sus tres partes, el ciclo infinito y Ctrl+C.
El patrón acumulador. `for` sobre `range`, y la regla para escoger entre los dos ciclos.
`break` y `continue`. La conferencia cierra invirtiendo el juego: la computadora
adivina partiendo el rango a la mitad, siete preguntas en lugar de cien, y se nombra
la búsqueda binaria como algo que vuelve en recursión.

### Conferencia 3. Funciones

Proyecto: decidir si un número es primo, y después imprimir y contar todos los primos
hasta un tope. `def`, parámetros y argumentos, `return` y las funciones que no devuelven
nada. El `return` dentro de un ciclo, que sale del ciclo y de la función a la vez y
sustituye a la bandera booleana. El alcance de las variables, con la distinción entre lo
local y lo global mostrada sobre un error real. Argumentos con valor por defecto y
argumentos por nombre. Descomponer un problema en funciones como método de trabajo, no
como adorno: la pregunta que lo guía es si el pedazo se explica en una frase. El argumento
a favor de las funciones no es que eviten copiar y pegar, sino que `es_primo` se mejora
tres veces, hasta `n // 2`, hasta la raíz y saltando los pares, cambiando una línea en un
solo sitio, y los tres programas que la llaman corren más rápido sin enterarse; las cuatro
versiones se cronometran. La conferencia cierra planteando la criba de Eratóstenes sin
poder escribirla: tachar exige acordarse de un sí o un no por cada número hasta `n`, y eso
son `n` variables que habría que teclear antes de saber cuánto vale `n`. Ese callejón sin
salida es la apertura de la conferencia siguiente. Se orientan como ejercicios `mcd`,
`factorial`, `combinaciones` y los primos gemelos, que las conferencias siguientes dan por
conocidos.

### Conferencia 4. Listas

Proyecto: un programa que lee las notas de un grupo y reporta la media, el máximo, el
mínimo y cuántos aprobaron. Abre con el callejón sin salida de la conferencia anterior,
que es el argumento entero a favor de las listas: no se puede escribir un programa que
declare tantas variables como estudiantes tenga el grupo, porque las variables se escriben
antes de saber cuántos son. La lista como la primera colección, el papel de los índices y
el conteo desde cero. Recorrer con `for`, con y sin índice. `len`, `append`, `in`, el
rebanado, y la mutabilidad, que es lo que separa una lista de todo lo visto hasta ahora y
la fuente de las sorpresas más caras del semestre. Listas de listas como matrices, con
ciclos anidados para recorrerlas y un ejemplo de tabla numérica. Cierra con la criba de
Eratóstenes, que es la deuda de la conferencia tres y no necesita más que
`[False] * (n + 1)`, y con las dos cuentas de operaciones una al lado de la otra: al
triplicar el tope, la criba triplica sus tachones y el método de uno en uno multiplica sus
divisiones por casi cinco. Eso deja abierto el orden de crecimiento para la conferencia
siete y el recordar en lugar de recalcular para la doce. En esta semana cae la orientación
del Proyecto I, que se trata aparte y no forma parte del contenido de la conferencia.

### Conferencia 5. Cadenas y archivos

Proyecto: contar las palabras de un archivo de texto. La cadena como secuencia, que se
indexa y se rebana igual que una lista pero no se puede modificar. Los métodos que más
se usan: `split`, `join`, `strip`, `lower`, `replace`. Abrir, leer y escribir archivos,
la instrucción `with`, y la codificación del texto, que en esta facultad se paga caro
cuando se ignora. `try` y `except` sobre el caso concreto del archivo que no existe y
del número mal escrito, que es la primera vez en el curso que un programa se hace
responsable de un error en lugar de morirse.

### Conferencia 6. Diccionarios y conjuntos

Proyecto: la frecuencia de cada palabra del texto de la conferencia anterior, y las
diez más comunes. El diccionario como correspondencia entre claves y valores, su
recorrido, `get` y el patrón de conteo. Los conjuntos y sus operaciones, con la
eliminación de repetidos como caso de uso inmediato. Por qué buscar una clave en un
diccionario cuesta lo mismo tenga diez elementos o diez millones, explicado como
intuición y no como implementación, que es materia del segundo semestre. El programa
de la asignatura sitúa los diccionarios en el tercer tema; este plan los adelanta
porque en Python son un tipo básico del lenguaje y porque la carrera de Ciencia de
Datos los necesita desde el principio.

### Conferencia 7. Búsqueda, ordenación y costo

Proyecto: ordenar las palabras por frecuencia a mano, sin usar la función de la
biblioteca, y después buscar en el resultado. Búsqueda lineal y búsqueda binaria sobre
una lista ordenada, ahora escritas y no solo intuidas. Ordenación por selección, por
inserción y por intercambio, las tres implementadas y comparadas. Contar operaciones
en lugar de medir segundos, que ya se hizo una vez con la criba, y la diferencia entre
$n$, $n \log n$ y $n^2$ vista sobre listas que crecen, midiendo también el reloj para que el orden de crecimiento deje de
ser una abstracción. Es una introducción informal a la complejidad, que se retoma con
rigor en Estructuras de Datos. Cierra el primer tema.

## Tema 2. Estrategias de solución y recursividad

### Conferencia 8. Recursión

Proyecto: las Torres de Hanói, que se resuelven en cinco líneas recursivas y no se
resuelven fácil de ninguna otra manera. Caso base y caso recursivo. La pila de
llamadas, trazada a mano para una entrada pequeña, y el `RecursionError` provocado a
propósito. Factorial y Fibonacci escritos recursivamente, con el reloj puesto: el
término treinta sale en una décima de segundo, el cuarenta tarda dieciséis segundos y
el cincuenta pasaría de la media hora. La observación queda abierta y se cobra en la
conferencia doce. Recursión sobre listas
y sobre cadenas, para que la recursión no quede asociada solo a los números.

### Conferencia 9. Divide y vencerás

Proyecto: ordenación por mezcla, cronometrada contra los tres métodos de la conferencia
siete sobre las mismas listas. El esquema de partir, resolver las partes y combinar.
La búsqueda binaria reescrita recursivamente, que cierra el arco abierto al final de
la conferencia dos. Ordenación rápida y su elección del pivote. Por qué estos
algoritmos cuestan $n \log n$, argumentado contando los niveles del árbol de llamadas y
el trabajo de cada nivel, sin recurrencias formales.

### Conferencia 10. Backtrack

Proyecto: las ocho reinas. El salto conceptual del semestre, y conviene decirlo en
clase: hasta aquí los estudiantes han buscado en espacios que alguien les entregó,
una lista, un rango de números. Ahora buscan en un espacio combinatorio que solo está
definido de forma implícita por las reglas del problema. Construir una solución por
decisiones parciales, verificar la validez de cada decisión, y deshacerla cuando el
camino no lleva a ninguna parte. La poda como la diferencia entre un programa que
termina y uno que no. Se orientan el sudoku y el recorrido del caballo.

### Conferencia 11. Combinatoria

Proyecto: generar todas las permutaciones, combinaciones y subconjuntos de un
conjunto, y usarlos para resolver el problema de la mochila por fuerza bruta. Contar
cuántos objetos hay antes de generarlos, que es lo que dice de antemano si el programa
va a terminar. El árbol de decisiones como la forma común de esta conferencia y la
anterior. Se muestra `itertools` al final, después de que los estudiantes hayan
escrito sus propios generadores y no antes.

### Conferencia 12. Programación dinámica

Proyecto: arreglar el Fibonacci de la conferencia ocho. Se mide, se memoriza en un
diccionario y se vuelve a medir, y la mejora es de un factor imposible de ignorar. De
ahí salen los dos conceptos: subproblemas que se repiten y la tabla que guarda las
respuestas. El problema de la mochila resuelto ahora en tiempo razonable, y la
subsecuencia común más larga. Memoización de arriba hacia abajo y tabla de abajo hacia
arriba, con la conversión de una en otra. Cierra el segundo tema.

## Evaluación

El esquema del colectivo está en `methodology/2024.md` y este plan no lo cambia; lo que
sigue es dónde caen sus hitos en este calendario.

El **Proyecto I** se orienta en la semana 4, que aquí es la conferencia de listas, y es
el primer momento en que un estudiante tiene con qué escribir un programa que merezca
el nombre. Se revisa a partir de la segunda semana del segundo semestre. Aprobarlo es
requisito para tener derecho a los exámenes finales.

El **primer examen parcial**, sobre algoritmia básica, va después de la semana 10, que
en este plan cae entre backtrack y combinatoria. Los otros dos parciales son del
segundo semestre. Conviene notar que el esquema de 2024 pone un solo parcial en todo el
primer semestre y lo sitúa cuando el tema de recursión ya empezó, así que el primer
tema completo no tiene evaluación escrita propia; si el colectivo quiere una, el lugar
natural es al cerrar la conferencia 7.

Las clases prácticas llevan evaluación presencial que no penaliza la nota final pero sí
puede bonificarla, y los ejercicios semanales resueltos se suben a GitHub y cuentan
para toda valoración subjetiva del desempeño.

## Lo que queda para el segundo semestre

El tercer tema, con sus 96 horas, cubre clases y objetos, herencia y polimorfismo,
tipos abstractos e interfaces, iteradores, programación funcional, estructuras
enlazadas, pilas, colas, árboles y diccionarios implementados, y elementos de
programación concurrente.

Vale la pena registrar por qué este plan no da ninguna conferencia a clases y objetos,
aunque el programa de la asignatura los sitúa en el primer tema. En C# no se puede
escribir un programa sin declarar una clase, y por eso la corrida de 2024 tuvo que
introducirlas en su cuarta conferencia. En Python se llega hasta programación dinámica
sin declarar ninguna, y el tercer tema son 96 horas dedicadas enteras a eso. Gastar una
semana del primer semestre en una versión apurada del asunto costaría una conferencia
de estrategias de solución y no ahorraría nada en el segundo.
