# Recursividad (intro)

## Introducción

La recursividad es un concepto que se encuentra en diversos ámbitos, no solo en programación, sino también en la naturaleza y en la vida cotidiana. En términos generales, la recursividad se refiere a la repetición de un patrón a sí mismo. Un ejemplo en la naturaleza sería la estructura de las ramas de un árbol, donde cada rama se ramifica en más ramas, siguiendo un patrón similar. Otros ejemplos incluyen la repetición de patrones en los copos de nieve y la estructura fractal de las hojas de helecho.

## Recursividad en Computación

En el contexto de la programación, la recursividad implica que una función se llame a sí misma para resolver un problema más pequeño del mismo tipo. Este enfoque permite abordar problemas de manera más elegante y compacta, dividiéndolos en subproblemas manejables.

### Características clave de la recursividad:

1. **Caso Base:** Toda función recursiva debe tener un caso base que marque el final de las llamadas recursivas. Sin él, la función se llamaría infinitamente.

2. **Caso Recursivo:** La función debe llamarse a sí misma con un conjunto de parámetros que se acerquen al caso base.

### Ejemplo en C#

Consideremos un ejemplo clásico de recursividad: el cálculo del factorial de un número.

```csharp
using System;

class Program
{
    static void Main()
    {
        int numero = 5;
        int resultado = CalcularFactorial(numero);
        Console.WriteLine($"El factorial de {numero} es: {resultado}");
    }

    static int CalcularFactorial(int n)
    {
        // Caso base
        if (n == 0 || n == 1)
        {
            return 1;
        }
        // Caso recursivo
        else
        {
            return n * CalcularFactorial(n - 1);
        }
    }
}
```

En este ejemplo, la función `CalcularFactorial` se llama a sí misma hasta que alcanza el caso base (cuando `n` es 0 o 1), evitando así un bucle infinito. La recursividad proporciona una solución concisa y fácil de entender para este problema.

La recursividad es una herramienta poderosa en programación, pero debe usarse con precaución para evitar problemas de rendimiento y desbordamiento de la pila. Se debe garantizar que haya una condición de terminación y que cada llamada recursiva reduzca el problema original hacia el caso base.

## Más Ejemplos de Recursividad en C#

### Fibonacci

El cálculo de la secuencia de Fibonacci es otro ejemplo clásico de recursividad. La secuencia comienza con 0 y 1, y cada número siguiente es la suma de los dos anteriores.

```csharp
using System;

class Program
{
    static void Main()
    {
        int n = 8;
        Console.WriteLine($"El término {n} en la secuencia de Fibonacci es: {Fibonacci(n)}");
    }

    static int Fibonacci(int n)
    {
        // Caso base
        if (n <= 1)
        {
            return n;
        }
        // Caso recursivo
        else
        {
            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }
    }
}
```

### Torres de Hanoi

El problema de las Torres de Hanoi es un clásico ejemplo de recursividad que involucra mover discos de una torre a otra, respetando la regla de que ningún disco más grande puede colocarse sobre uno más pequeño.

```csharp
using System;

class Program
{
    static void Main()
    {
        int numeroDiscos = 3;
        MoverTorresHanoi(numeroDiscos, 'A', 'C', 'B');
    }

    static void MoverTorresHanoi(int n, char origen, char destino, char auxiliar)
    {
        if (n == 1)
        {
            Console.WriteLine($"Mover disco 1 desde {origen} hasta {destino}");
        }
        else
        {
            MoverTorresHanoi(n - 1, origen, auxiliar, destino);
            Console.WriteLine($"Mover disco {n} desde {origen} hasta {destino}");
            MoverTorresHanoi(n - 1, auxiliar, destino, origen);
        }
    }
}
```

## Ventajas y Desventajas de la Recursividad

### Ventajas:

1. **Claridad y Simplicidad del Código:** La recursividad puede conducir a un código más claro y fácil de entender, especialmente para problemas que se pueden descomponer en subproblemas similares.
  
2. **Manejo Elegante de Problemas Recurrentes:** Algunos problemas, como los mencionados anteriormente, se pueden abordar de manera más elegante y compacta mediante el uso de la recursividad.

### Desventajas:

1. **Uso de Recursos:** La recursividad puede consumir más recursos, ya que cada llamada recursiva agrega una nueva entrada a la pila de llamadas. Esto puede provocar desbordamiento de pila para problemas grandes.

2. **Rendimiento:** En comparación con las soluciones iterativas, la recursividad puede ser menos eficiente en términos de rendimiento debido al costo de las llamadas de función adicionales y la gestión de la pila.

3. **Dificultad de Depuración:** El seguimiento de llamadas recursivas puede ser más complicado, y el olvido del caso base puede conducir a bucles infinitos.

En resumen, la recursividad es una herramienta poderosa, pero su aplicación debe ser cuidadosa para evitar problemas de rendimiento y asegurar la correcta terminación del algoritmo. En algunos casos, el uso de enfoques iterativos puede ser preferible para mejorar la eficiencia y la legibilidad del código.