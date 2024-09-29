# Recursión (intro)

Recursión es uno de los temas fundamentales en Ciencia de la Computación, no solo porque es una herramienta extremadamente útil para resolver problemas complejos, sino porque además la teoría de computabilidad se fundamenta en conceptos recursivos (eso lo veremos en 2do año).

Pero para empezar, vamos a ver recursión como una estrategia para resolver problemas que pueden parecer más difíciles de resolver solo utilizando ciclos.

La premisa básica de una solución recursiva es resolver un problema a partir de reducirlo a uno o más subproblemas de naturaleza similar. A su vez, estos subproblemas se reducen a subproblemas más sencillos, hasta que solo quedan problemas tan básicos que se pueden resolver directamente.

Veamos un ejemplo. Calcular la suma de los $n$ primeros números enteros positivos:

```cs
static void RecursiveSum(int n)
{
    // ...
}
```

El primer paso va a ser identificar el input más sencillo posible, $n = 0$. Es un input donde la respuesta es trivial, la suma de cero números es $0$.

Ahora veamos problemas no triviales:

    🟩 = 1

    🟩   = 1 +
    🟩🟩 = 2 = 3

    🟩     = 1 +
    🟩🟩   = 2 +
    🟩🟩🟩 = 3 = 6

    🟩       = 1 +
    🟩🟩     = 2 +
    🟩🟩🟩   = 3 +
    🟩🟩🟩🟩 = 4 = 10

¿Se nota algún patrón?
La solución para $n=4$ es la solución para $n=3$ adicionando $4$.

    🟥       = 1 +
    🟥🟥     = 2 +
    🟥🟥🟥   = 3 = 6
    🟩🟩🟩🟩 = 4 + 6 = 10

Vamos a generalizar dicho patrón. La solución para cualquier valor de $n$ es la misma que para $n-1$ sumando $n$:

- $F(0) = 0$
- $F(n) = F(n-1) + n$

Una vez obtenida la solución recurrente general, es trivial de programar:


```cs
static void RecursiveSum(int n)
{
    return n == 0 ? 0 : n + RecursiveSum(n - 1);
}
```
