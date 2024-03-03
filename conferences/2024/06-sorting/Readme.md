# Ordenación y Búsqueda

Ordenación y búsqueda son dos de los problemas fundamentales en Ciencia de la Computación.

## Búsqueda binaria

Es el algoritmo de búsqueda más eficiente que se puede lograr si solo podemos comparar elementos.
La idea central es que si los elementos están ordenados, es posible descartar fácilmente hasta la mitad de los elementos haciendo una sola comparación.

```csharp
public static int BinarySearch(int[] items, int x)
{
    int l = 0;
    int r = items.Length - 1;

    while (l <= r)
    {
        int m = (l + r) / 2;

        if (items[m] < x)
            l = m + 1;
        else if (items[m] > x)
            r = m - 1;
        else
            return m;
    }

    return -1;
}
```

<video src="https://user-images.githubusercontent.com/1778204/227782224-d6c62a8a-36e7-41af-9604-ad1b537b38f8.mp4" controls width="100%" autoplay loop></video>

## Ordenación

En última instancia, ordenar consiste en eliminar las inversiones. Una inversión es cualquier par $(i,j)$ tal que $i < j$ y $x_i > x_i$.

## Bubble sort

Bubble sort es un algoritmo de ordenación que funciona arreglando las inversiones una a una.

```csharp
public static void BubbleSort(int[] array)
{
    for (int i = 0; i < array.Length; i++)
        for (int j = 0; j < array.Length - 1; j++)
            if (array[j] > array[j + 1])
                Swap(array, j, j + 1);
}
```

<video src="https://user-images.githubusercontent.com/1778204/227782811-72096ba6-41d8-4c99-80d1-dfa85368add4.mp4" controls width="100%" autoplay loop></video>

## Selection sort

Selection sort es un algoritmo de ordenación que funciona escogiendo en todo momento el menor elemento de los que quedan por ordenar.

```csharp
public static void SelectionSort(int[] array)
{
    for (int i = 0; i < array.Length; i++)
    {
        int min = i;

        for (int j = i + 1; j < array.Length; j++)
            if (array[j] < array[min])
                min = j;

        Swap(array, min, i);
    }
}
```

<video src="https://user-images.githubusercontent.com/1778204/227782837-b139c430-5b77-45b0-af2b-331ec790c5ce.mp4" controls width="100%" autoplay loop></video>

## Insertion sort

Insertion sort es un algoritmo de ordenación que funciona en cada iteración ubicando el elemento i-ésimo en la posición que le corresponde.

```csharp
public static void InsertionSort(int[] array)
{
    for (int i = 1; i < array.Length; i++)
    {
        int j = i - 1;

        while (j >= 0 && array[j] > array[j + 1])
        {
            Swap(array, j, j + 1);
            j = j - 1;
        }
    }
}
```

<video src="https://user-images.githubusercontent.com/1778204/227782855-125f5647-3fd9-4d33-ba38-9c674d7a633b.mp4" controls width="100%" autoplay loop></video>

## Ejercicios

1) ¿Qué sucede con `BinarySeach` cuando existen valores repetidos? Modifique el algoritmo para que en esos casos:
   - a) Devuelva el índice del valor más a la izquierda.
   - b) Devuelva el índice del valor más a la derecha.

2) En `BubbleSort`, si una iteración del ciclo más interno no hace ningún intercambio,
   se puede garantizar que el array está ordenado (¿Por qué?).
   Modifique el algoritmo para que termine en ese caso.

   - a) En el mismo algoritmo, note que no siempre es necesario siempre llevar el ciclo más interno
        hasta el final (¿Por qué?). Modifique el algoritmo en consecuencia.

3) Modifique el método `InsertionSort` para que haga la menor cantidad de asignaciones posibles.
   Hint: En el ciclo más interno, note que `Swap(j,j+1)`, siempre se intercambia con el mismo elemento.

4) Bonus track: Modifique `BinarySearch` de forma que no necesite usar ciclos :)
