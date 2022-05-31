# Ejercicios de Condicionales y Ciclos

## Determinar sexo

Implemente un programa que le pida al usuario su número de identidad y determine su sexo. Note que el sexo puede determinarse por el penúltimo dígito del número de identidad, en caso de ser par es masculino, femenino en caso contrario.

## Mayor, menor y promedio de forma perezosa

Implemente un programa que lea una secuencia de números de la consola (uno por línea) hasta que se escriba una línea en blanco y de estos imprimir:

- El mayor
- El menor
- Su promedio

## Fecha válida

Implemente un programa que lea de la consola tres números y compruebe si forman una fecha. En caso de serlo, imprima el día siguiente a la misma con el formato **día/mes/año**.

## Tipo de triángulo

Implemente un método que reciba tres números enteros y diga qué tipo de triángulo forman. El método debe devolver `0` si los enteros son lo lados de ningún triángulo, `1` si es un triángulo escaleno, `2` si es isósceles y `3` si es equilátero.

> Cree y utilice un `enum` para mejorar la semántica de este método.

## Cantidad de dígitos

Implemente un método que reciba un número entero y halle su cantidad de dígitos (no usar la clase `string`)

## Representación binaria

Implemente un método que reciba un número entero y devuelva su representación binaria.

## Máximo común divisor

Implemente un método que reciba dos números enteros y halle el máximo común divisor entre ellos.

## Secuencia de Collatz

Escriba un programa que lea un número entero `n` de la consola e imprima la secuencia de _Collatz_ para `n`. La secuencia de _Collatz_ se define como:

$$
S_0 = n \\
S_{n+1} = \left\{ \begin{array}{lcc}
             3 S_n + 1 ~ si ~ S_n ~ impar \\
             \\ \frac{S_n}{2} ~ si ~ S_n ~ par
             \end{array}
   \right.
$$

Dicha secuencia termina cuando se alcanza el número `1` (lo cual siempre ocurre, aunque no se ha podido demostrar aún).

Ejemplo: Para `17` tenemos (`->` división entre `2`, `=>` multiplicación por `3` y adición de `1`)

```tex
17 => 52 -> 26 -> 13 => 40 -> 20 -> 10 -> 5 => 16 -> 8 -> 4 -> 2 -> 1
```

## Días entre dos fechas

Implemente un método que reciba como parámetro dos fechas (tres enteros por cada fecha) y calcule cuántos días hay entre ellas **(no usar ciclos)**.

## Factorial

Calcular el factorial de un número.

Ej: `5! = 5 x 4 x 3 x 2 x 1 = 120`

## Potencia de un número (sucesivas multiplicaciones)

Calcular la potencia de un número como sucesivas multiplicaciones.

## Número perfecto

Determinar si un número es perfecto. Un número es perfecto si la suma de sus divisores propios es igual a él.

Ej: `28 = 1 + 2 + 4 + 7 + 14`

## Sucesión de Fibonacci

Hallar el n-ésimo término de la sucesión de Fibonacci:

$$
F_0 = 1 \\
F_1 = 1 \\
F_n = F_{n-1} + F_{n-2} ~ para ~ n > 1
$$

$$
1, 1, 2, 3, 5, 8, 13, 21, 34, 55
$$

## Máximo común divisor por el algoritmo de Euclides

Hallar el Máximo Común Divisor entre dos enteros utilizando el algoritmo de Euclides basado en el resto. `MCD(a, b) = MCD(b, r)` donde `r = a % b`.

## Descomponer un número en factores primos

Dado un número `n`. Hallar su descomposición en factores primos. Dicha descomposición consiste en el producto de potencias de factores primos tal que se obtenga el número. Ej: $1960 = 2^3 ∗ 5^1 ∗ 7^2$.

> Nota: Esta descomposición es única para cada número.

Adicional: Determinar el número primo más cercano a `n`.
