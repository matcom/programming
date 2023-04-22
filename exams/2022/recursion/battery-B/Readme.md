# Examen Parcial de Programación - Curso 2022

En este ejericio, usted debe implementar un método para contar todas las secuencias estrictamente crecientes de números naturales $n_i > 0$ tales que:

$$
\sum n_i^{a_i} \leq N
$$

El método tiene la siguiente definición:

```cs
static int CountSequences(int N, int[] factors)
{
    // su implementación aquí
}
```

Por ejemplo, supongamos que tenemos los factores $1,2,3$ y el valor $N=100$, queremos encontrar todas las secuencias de tres números naturales $n_1, n_2, n_3$ tales que $n_1 + n_2^2 + n_3^3 \leq 100$.

Las únicas posibles secuencias de números estrictamente crecientes que suman menor o igual que 100 son:

```
1 + 2^2 + 3^3 = 32
1 + 2^2 + 4^3 = 69
1 + 3^2 + 4^3 = 74
2 + 3^2 + 4^3 = 75
```

Por lo tanto, la ejecución del método `CountSequences` con los parámetros correspondientes sería:

```cs
int result = CountSequences(100, new int[] {1, 2, 3});
Debug.Assert(result == 4);
```

Tanto los valores $a_i$ (parámetro `factors`) como el valor $N$, como los valores de las secuencias que usted debe contar son enteros positivos.
