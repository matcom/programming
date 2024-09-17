# Programación Dinámica

La programación dinámica es una técnica de optimización utilizada en informática para resolver problemas que pueden ser divididos en subproblemas superpuestos y repetitivos. Su enfoque consiste en resolver cada subproblema solo una vez y luego almacenar su solución, evitando así recalcularla cada vez que se encuentre nuevamente.

### Estructura General

La estructura general de un algoritmo de programación dinámica implica los siguientes pasos:

1. **Definición del problema**: Identificar el problema y sus subproblemas.
2. **Formulación de la solución recursiva**: Plantear una solución recursiva para el problema que involucre soluciones a sus subproblemas.
3. **Memorización o tabla de memorización**: Almacenar las soluciones de los subproblemas en una tabla para evitar recálculos.
4. **Implementación de la solución**: Utilizar la tabla de memorización para resolver el problema de manera eficiente.

### Ejemplos de Código

#### Fibonacci

A continuación, se presenta un ejemplo simple de cómo se puede aplicar programación dinámica para resolver el problema de calcular el n-ésimo término de la secuencia de Fibonacci:

```csharp
public static int Fibonacci(int n)
{
    if (n <= 1)
        return 1;

    int[] fib = new int[n + 1];
    fib[0] = 1;
    fib[1] = 1;

    for (int i = 2; i <= n; i++)
    {
        fib[i] = fib[i - 1] + fib[i - 2];
    }

    return fib[n];
}
```

#### Problema de la Mochila

El problema de la mochila es un problema clásico de optimización en informática y combinatoria.
En su variante 0-1, se plantea de la siguiente manera:

> Supongamos que tienes una mochila con una capacidad máxima dada y una serie de objetos, cada uno con un peso y un valor asociado.
> El objetivo es determinar la combinación de objetos que puedes colocar en la mochila para maximizar el valor total,
> sin exceder la capacidad máxima de la mochila.
> Se pude asumir que no hay objetos con peso nulo.

A continuación, se presenta un ejemplo simple de cómo se puede aplicar cominatoria para resolver el problema.

```csharp
// ganancia maxima que se puede obtener de n objetos y una mochila con cierta capacidad sin repetir objetos
public static int Mochila(int[] ganancia, int[] peso, int capacidad, int n)
{
    return n == 0 ?       // Si no hay objetos entre los que elegir (si no pueden haber objetos de peso 0
                          // poner capacidad == 0 para podar)
        0 :               // Entonces la ganancia maxima es 0, sino
        peso[n - 1] > capacidad ? // Si el peso del objeto n-esimo sobrepasa la capacidad de la mochila
                                  // Entonces no se puede tomar, y por tanto es
                                  // la ganacia maxima con los 1ros n - 1 objetos
        Mochila(ganancia, peso, capacidad, n - 1) :
        Math.Max( // Sino, es el maximo entre
            Mochila(ganancia, peso, capacidad, n - 1), // no haberlo tomado y
            Mochila(ganancia, peso, capacidad - peso[n - 1], n - 1) + ganancia[n - 1] // Tomarlo más maximizar
                                                                                      // una mochila con capacidad
                                                                                      // y objetos restantes
        );
}
```

A continuación, se presenta un ejemplo simple de cómo se puede aplicar programación dinámica para resolver el problema.

```csharp
// ganancia maxima que se puede obtener de n objetos y una mochila con cierta capacidad sin repetir objetos
public static int MochilaDPMatriz(int[] ganancia, int[] peso, int capacidad, int n)
{
    int[,] m = new int[n + 1, capacidad + 1];   // m[i,j] = ganancia maxima para una mochila
                                                // con capacidad j pudiendo elegir entre i objetos

    for (int c = 0; c <= capacidad; c++)        // Si no hay objetos, no importa la capacidad de la mochila
        m[0, c] = 0;                            // La ganancia maxima va ha ser cero

    for (int i = 1; i <= n; i++)                // Para el resto de las posibles cantidades de objetos
        for (int c = 0; c <= capacidad; c++)    // Y para todas las capacidades
        {
            m[i, c] = c < peso[i - 1] ?         // Si el objeto i-esimo pesa más que la capacidad actual
                m[i - 1, c] :                   // Entonces no se puede tomar, sino
                Math.Max(                       // Es lo mejor entre
                    m[i - 1, c],                // No haberlo tomado (se mantiene la misma capacidad)
                    m[i - 1, c - peso[i - 1]] + ganancia[i - 1] // Y haberlo tomado (la capacidad se reduce según
                                                                // su peso y ganancia aumenta según la del objeto)
                );
        }
        
    return m[n, capacidad];                    // Devolver la ganancia maxima para una mochila con
                                               // la capacidad dada pudiendo elegir entre n objetos
}
```

A continuación, se presenta una optimización al aplicar programación dinámica para resolver el problema.
La memoria se reduce a dos filas.

```csharp
// ganancia maxima que se puede obtener de n objetos y una mochila con cierta capacidad sin repetir objetos
public static int MochilaDP2Filas(int[] ganancia, int[] peso, int capacidad, int n)
{
    int[] completado = new int[capacidad + 1]; // Tras haber analizado i objetos completado[c] contiene
                                               // la ganancia maxima que se puede obtener con i objetos
                                               // y una mochila de capacidad c sin repetir objetos

    int[] actual = new int[capacidad + 1]; // Tras haber analizado i objetos actual[c] contiene
                                           // la ganancia maxima que se puede obtener con i - 1 objetos
                                           // y una mochila de capacidad c sin repetir objetos

    int[] aux; // Para no perder las referencias durante el swap y tener que reservar más memoria

    for (int c = 0; c <= capacidad; c++) // Si no hay objetos, no importa la capacidad de la mochila 
        completado[c] = 0; // La ganancia maxima va ha ser cero

    for (int i = 1; i <= n; i++) // Para el resto de las posibles cantidades de objetos
    {
        for (int c = capacidad; c >= 0; c--) // Y para todas las capacidades
        {
            if (c < peso[i - 1]) // Si el objeto i-esimo pesa más que la capacidad actual
                actual[c] = completado[c]; // Entonces optimizar sin haberlo tomado
                                           // (completado[c] contiene el de i - 1 objetos)
            else
                actual[c] = Math.Max( // Sino, es lo mejor entre 
                    completado[c],  // No haberlo tomado (completado[c] contiene el de i - 1 objetos)
                    completado[c - peso[i - 1]] + ganancia[i - 1] // Y haberlo tomado (la capacidad se reduce
                                                                  // segun su peso y ganancia aumenta según la
                                                                  // del objeto)
                                                                  // (completado[c - peso[i - 1]] contiene el
                                                                  // optimo de i - 1 objetos)
                );
        }
        aux = completado; // Guardar puntero (referencia) al resultado de la iteración anterior
        completado = actual; // Poner referencia del resultado de esta iteración en completado[]
        actual = aux; // Recuperar referencia para reescribir (reutilizar) en la proxima iteracion
    } // Cada iteración finalizada pone en completado la ganancia maxima hasta con i objetos para cada capacidad

    return completado[capacidad];  // Devolver la ganancia maxima para una mochila con la capacidad dada
                                   // pudiendo elegir entre n objetos
}
```

A continuación, se presenta una optimización al aplicar programación dinámica para resolver el problema.
La memoria se reduce a una fila.

```csharp
// ganancia maxima que se puede obtener de n objetos y una mochila con cierta capacidad sin repetir objetos
public static int MochilaDP1Fila(int[] ganancia, int[] peso, int capacidad, int n)
{
    int[] best = new int[capacidad + 1]; // Tras haber analizado i objetos best[c] contiene
                                         // la ganancia maxima que se puede obtener con i objetos y
                                         // una mochila de capacidad c sin repetir objetos

    for (int c = 0; c <= capacidad; c++) // Si no hay objetos, no importa la capacidad de la mochila
        best[c] = 0; // La ganancia maxima va ha ser cero

    for (int i = 1; i <= n; i++) // Para el resto de las posibles cantidades de objetos
        for (int c = capacidad; c >= 0; c--) // Y para todas las capacidades
        {
            if (c < peso[i - 1]) // Si el objeto i-esimo pesa más que la capacidad actual
                continue; // Entonces best[c] = best[c] (no tomarlo)

            best[c] = Math.Max( // Sino, es lo mejor entre
                best[c], // No haberlo tomado
                         // (best[c] contiene aun valor de la iteracion anterior)
                best[c - peso[i - 1]] + ganancia[i - 1]   // Y haberlo tomado (la capacidad se reduce
                                                          // segun su peso y ganancia aumenta según la
                                                          // del objeto)
                                                          // (best[c - peso[i - 1]] sigue conteniendo
                                                          // el optimo para i - 1 objetos porque
                                                          // c - peso[i - 1] <= c)
            );
        }
    return best[capacidad]; // Devolver la ganancia maxima para una mochila con la capacidad dada
                            // pudiendo elegir entre n objetos
}
```

### Consejos

La programación dinámica es una herramienta poderosa para resolver problemas complejos de manera eficiente, pero su aplicación requiere comprensión y cuidado para evitar posibles trampas de rendimiento y memoria.

- **Comprender el problema**: Antes de aplicar programación dinámica, es esencial entender completamente el problema y sus subproblemas.
- **Empezar con casos simples**: Comenzar resolviendo casos simples del problema y luego generalizar a casos más complejos puede ayudar a entender mejor la estructura de la solución.
- **Optimización de memoria**: Si el espacio de memoria es una preocupación, considerar técnicas de optimización de memoria como utilizar matrices de menor tamaño o implementar una tabla de memorización de manera más eficiente.

