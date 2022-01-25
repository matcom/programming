# Ejercicios de Recursión

## Menor elemento

Dado un array que no está necesariamente ordenado, implemente el método `Min` que busca recursivamente el menor de los elementos.

```csharp
int Min(int[] elements) {
    // Devuelve le menor de los elementos en el array
}
```

## Búsqueda binaria

Dado un *array* ordenado de enteros, implemente el método `BinarySearch` que realiza una búsqueda binaria recursiva en el array.

```csharp
bool BinarySearch(int[] array, int x) {
    // Devuelve true si el elemento `x` está en el array.
}
```

## Factorial

El factorial de un número entero, $n!$, se define como la multiplicación de todos los números entre $1$ y $n$:

$$
n! = \Pi_{k=1}^{n} k
$$

Una posible definición recursiva de $n!$ es la siguiente:

$$
n! = n \cdot (n-1)!
$$

(Tenga en cuenta que por definición, $0!=1$)

Implemente el método `Factorial` que computa el factorial de un número de forma recursiva.

```csharp
int Factorial(int n) {
    // Devuelve n!
}
```
