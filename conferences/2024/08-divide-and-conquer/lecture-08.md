# Recursividad (Divide y Vencerás)

## Introducción

El enfoque de "divide y vencerás" es una técnica de resolución de problemas que se basa en dividir un problema en subproblemas más pequeños, resolver cada subproblema de manera recursiva y luego combinar las soluciones de los subproblemas para obtener la solución al problema original.

Por supuesto, todos los enfoques recursivos de una forma u otra siguen esa misma estructura;
la diferencia fundamental al aplicar "divide y vercerás" radica en que al dividir se crearán subproblemas con tamaño en ordenes de magnitud menores que la solución actual (o sea, en lugar de ser 1, 2, o N unidades más pequeño, cada subproblema será 2, 3, o M veces más pequeño).
Esta característica resultan en implementaciones que, generalmente, convergen a soluciones de forma más eficiente.

## Principios clave:

1. **Dividir**: Divide el problema en subproblemas más pequeños y manejables.
2. **Vencer**: Resuelve cada subproblema de manera recursiva.
3. **Combinar**: Combina las soluciones de los subproblemas para obtener la solución al problema original.

## Ejemplo de aplicación: 

### 1. Búsqueda binaria:

La búsqueda binaria es un ejemplo clásico de "divide y vencerás".

```csharp
public static int BinarySearch(int[] array, int target)
{
    return BinarySearch(array, target, 0, array.Length - 1);
}

private static int BinarySearch(int[] array, int target, int left, int right)
{
    if (left > right)
        return -1;

    int mid = left + (right - left) / 2;

    if (array[mid] == target)
        return mid;
    else if (array[mid] < target)
        return BinarySearch(array, target, mid + 1, right);
    else
        return BinarySearch(array, target, left, mid - 1);
}
```

### 2. Merge Sort:

Merge Sort es un algoritmo de ordenamiento que sigue el principio de "divide y vencerás".

```csharp
static void MergeSort(int[] array)
{
    MergeSort(array, 0, array.Length - 1, new int[array.Length]);
}

static void MergeSort(int[] array, int inicio, int fin, int[] aux)
{
    if(inicio == fin)
        return;

    int medio = inicio + (fin - inicio) / 2;
    MergeSort(array, inicio, medio, aux);
    MergeSort(array, medio + 1, fin, aux);
    Merge(array, aux, inicio, medio, fin);
}

static void Merge(int[] array, int[] aux, int inicioA, int finA, int finB)
{
    int i = inicioA;
    int j = finA + 1;
    int pos = inicioA;

    while(i <= finA && j <= finB) {
        if(array[i] <= array[j])
            aux[pos++] = array[i++];
        else
            aux[pos++] = array[j++];
    }
    while(i <= finA) {
        aux[pos++] = array[i++];
    }
    while(j <= finB) {
        aux[pos++] = array[j++];
    }
    Array.Copy(aux, inicioA, array, inicioA, finB - inicioA + 1);
}
```

Estos son solo dos ejemplos de cómo se puede aplicar el enfoque de "divide y vencerás" en la programación, pero este principio se puede utilizar para resolver una variedad de problemas de manera eficiente.