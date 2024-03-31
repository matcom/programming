# Recursividad (Combinatoria)

La combinatoria, en programación, describe el proceso de resolver problemas a partir de explorar todas las posibles soluciones candidatas y seleccionar la óptima.

A diferencia del caso más general de _backtrack_, el espacio de soluciones sigue una estructura combinatoria, esto es, son permutaciones, variaciones, combinaciones o subconjuntos de una colección de elementos. Estos principios se aplican mediante algoritmos recursivos que generan todas las posibles combinaciones del conjunto de elementos y de ahí el nombre.

## Variantes generales de combinatoria

- Permutaciones
    - $n!$
- Variaciones sin repeticiones
    - $n! / (n-m)!$
- Variaciones con repeticiones
    - $n^m$
- Combinaciones
    - $n / (m! * (n-m)! )$
- Subconjuntos
    - $2^n$

## Ejemplos de Código

### Variaciones sin repeticiones

```csharp
void VariationsWithoutRepetitionsB(int[] items, int k)
{
    int[] variation = new int[k];
    bool[] taken = new bool[items.Length];
    InternalVariationsWithoutRepetitionsB(items, k, variation, 0, taken);
}

void InternalVariationsWithoutRepetitionsB(int[] items, int k, int[] variation, int count, bool[] taken)
{
    if (count == k)
    {
        // variation is ready!!!
        Console.WriteLine(string.Join(", ", variation.Take(count)));
        return;
    }

    for (int i = 0; i < items.Length; i++)
    {
        if (!taken[i])
        {
            taken[i] = true;
            variation[count] = items[i];
            InternalVariationsWithoutRepetitionsB(items, k, variation, count + 1, taken);
            taken[i] = false;
        }
    }
}
```

### Permutaciones

```csharp
void EasyPermutations(int[] items)
{
    VariationsWithoutRepetitionsA(items, items.Length);
}
```

```csharp
void InPlacePermutations(int[] items, int pos = 0)
{
    if(pos == items.Length) {
        // item is a permutation!!!
        Console.WriteLine(string.Join(", ", items));
        return;
    }

    for(int i = pos; i < items.Length; i++) {
        int temp = items[pos];
        items[pos] = items[i];
        items[i] = temp;

        Permutations(items, pos + 1);

        temp = items[pos];
        items[pos] = items[i];
        items[i] = temp;
    }
}
```

### Problema del Viajante (Traveling Salesman Problem)
El problema del viajante es un problema clásico de optimización que busca encontrar la ruta más corta que visite todas las ciudades exactamente una vez. A continuación, se presenta un ejemplo básico de cómo resolver este problema utilizando fuerza bruta y recursividad. El objetivo es ilustrar cómo se puede modificar la plantilla base de combinatoria para ajustarla al problema.

> **OJO:** En la práctica no se suele hacer la traducción directa pues estos algoritmos son muy ineficientes, así que se vuelve necesario computar las soluciones incrementalmente para poder hacer poda.
>
>> Con $5$ ciudades tenemos que explorar $120$ posibilidades.
>>
>> Pero con tan solo $20$ ciudades ya tenemos que explorar $2\,432902\,008176\,640000$ posibilidades!

```csharp
class TravelingSalesman
{
    public static int Solve(int[,] distances)
    {
        int nCities = distances.GetLength(0);
        int[] cities = Enumerable.Range(0, nCities).ToArray();
        int[] variation = new int[nCities];
        bool[] taken = new bool[nCities];
        return ModifiedVariationsWithoutRepetitionsB(cities, nCities, variation, 0, taken, distances, int.MaxValue);
    }

    static int ModifiedVariationsWithoutRepetitionsB(
        int[] items,
        int k,
        int[] variation,
        int count,
        bool[] taken,
        int[,] distances,
        int min
    )
    {
        if (count == k)
            return Math.Min(min, EvaluateVariation(variation, distances));

        for (int i = 0; i < items.Length; i++)
        {
            if (!taken[i])
            {
                taken[i] = true;
                variation[count] = items[i];
                min = ModifiedVariationsWithoutRepetitionsB(items, k, variation, count + 1, taken, distances, min);
                taken[i] = false;
            }
        }
        return min;
    }

    static int EvaluateVariation(int[] variation, int[,] distances)
    {
        int result = 0;
        for (int i = 0; i < variation.Length - 1; i++)
            result += distances[variation[i], variation[i + 1]];
        return result;
    }
}
```

## Consideraciones Generales
- **Eficiencia**: La combinatoria puede generar un gran número de combinaciones o permutaciones, por lo que es importante considerar la eficiencia de los algoritmos, especialmente en problemas con conjuntos de datos grandes.
- **Optimización**: Algunos problemas combinatorios pueden ser resueltos con algoritmos más eficientes, como el uso de programación dinámica en lugar de fuerza bruta.
- **Validación de Datos**: Es fundamental validar los datos de entrada para evitar problemas como desbordamientos de memoria o índices fuera de rango.