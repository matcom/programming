# IEnumerable e IEnumerator

En C#, `IEnumerable<T>` e `IEnumerator<T>` son interfaces que permiten recorrer colecciones de manera eficiente y estandarizada. A diferencia de otras estructuras que hemos estudiado en la asignatura, los **IEnumerables** permiten representar incluso colecciones potencialmente infinitas.

Ambas interfaces son fundamentales para el funcionamiento de la estructura `foreach`, facilitando así la iteración sobre cualquier colección que las implemente. Dado que inicialmente no había genericidad en C#, se utilizaba la versión no genérica `IEnumerable`, sin embargo, las versiones más recientes del lenguaje introdujeron `IEnumerable<T>` y `IEnumerator<T>`, ofreciendo mayor seguridad y flexibilidad.

## IEnumerable<T>

La interface `IEnumerable<T>` define una colección que puede ser recorrida. En términos simples, `IEnumerable<T>` es todo tipo capaz de proporcionar un enumerador (`IEnumerator<T>`), el cual no es más que una "maquinita" para recorrer la colección. Para ello, la interface obliga a implementar el método `GetEnumerator`, que devuelve un objeto `IEnumerator<T>`.

### Sintaxis

En C#, `IEnumerable<T>` hereda de `IEnumerable` para mantener la compatibilidad con versiones anteriores:

```csharp
public interface IEnumerable<out T> : IEnumerable
{
    IEnumerator<T> GetEnumerator();
}
```

La implementación explícita es común para evitar confusiones y posibles conflictos:

```csharp
public class MyCollection<T> : IEnumerable<T>
{
    T[] items;

    public MyCollection(int length)
    {
        items = new T[length];
    }

    public void UpdateAt(T value, int pos)
    {
        if (pos < 0 || pos >= items.Length)
            throw new InvalidOperationException();
        items[pos] = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new MyEnumerator<T>(this.items);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
```

## IEnumerator<T>

La interface `IEnumerator<T>` define la "maquinita" que recorre una colección. Su funcionalidad principal es proporcionar acceso secuencial a los elementos de la colección. Para ello, `IEnumerator<T>` proporciona los métodos necesarios para iterar sobre una colección. Estos métodos incluyen `MoveNext`, `Reset` y la propiedad `Current`.

### Sintaxis

```csharp
public interface IEnumerator<out T> : IDisposable, IEnumerator
{
    T Current { get; }
    bool MoveNext();
    void Reset();
}
```

### Métodos y Propiedades

- **MoveNext()**: Avanza el enumerador al siguiente elemento de la colección. Devuelve `true` si hay un siguiente elemento y `false` si no lo hay.
- **Reset()**: Restablece el enumerador a su posición inicial, antes del primer elemento de la colección.
- **Current**: Obtiene el elemento en la posición actual del enumerador.

#### Convenciones:

- Si `MoveNext()` no se ha llamado, `Current` lanza una excepción.
- Si `MoveNext()` devuelve `false`, `Current` también lanza una excepción.
- Una vez que `MoveNext()` devuelve `false`, cualquier llamado futuro a `MoveNext()` debe devolver `false`.

### Ejemplo de Implementación

```csharp
public class MyEnumerator<T> : IEnumerator<T>
{
    private T[] items;
    private int index = -1;

    public MyEnumerator(T[] items)
    {
        this.items = items;
    }

    public bool MoveNext()
    {
        if (index < items.Length - 1)
        {
            index++;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        index = -1;
    }

    public T Current
    {
        get { return items[index]; }
    }

    object IEnumerator.Current
    {
        get { return Current; }
    }

    public void Dispose()
    {
        // Liberar recursos si es necesario.
    }
}
```

En este ejemplo, `MyEnumerator<T>` implementa `IEnumerator<T>` para recorrer una colección de elementos de tipo `T`.

## Uso del foreach

La instrucción `foreach` facilita la iteración sobre cualquier colección que implemente la interface `IEnumerable<T>`. El compilador de C# convierte la estructura `foreach` en llamadas a `GetEnumerator`, `MoveNext` y `Current`. Por ejemplo:

```csharp
IEnumerable<int> numbers = new List<int> { 1, 2, 3 };
foreach (int number in numbers)
{
    Console.WriteLine(number);
}
```

Se convierte en:

```csharp
IEnumerable<int> numbers = new List<int> { 1, 2, 3 };
IEnumerator<int> enumerator = numbers.GetEnumerator();
while (enumerator.MoveNext())
{
    int number = enumerator.Current;
    Console.WriteLine(number);
}
```

## Consideraciones y Consejos

1. **Implementación explícita**: Cuando una clase implementa múltiples interfaces con métodos de nombres similares, es común usar la implementación explícita para evitar conflictos.
2. **Recursos**: Asegúrate de liberar recursos si el enumerador realiza operaciones que lo requieren, implementando la interface `IDisposable`.

## Clase Enumerable y Métodos Extensores

La clase estática `Enumerable` en el espacio de nombres `System.Linq` proporciona un conjunto de métodos extensores que se pueden usar con cualquier objeto que implemente `IEnumerable<T>`. Estos métodos, conocidos como métodos LINQ (Language Integrated Query), permiten realizar operaciones como filtrado, proyección, agregación y ordenación sobre colecciones.

### Métodos Comunes

- **Where**: Filtra una secuencia en función de un predicado.
- **Select**: Proyecta cada elemento de una secuencia en una nueva forma.
- **OrderBy**: Ordena los elementos de una secuencia en orden ascendente.
- **GroupBy**: Agrupa los elementos de una secuencia según una clave especificada.

> El método extensor **Count()** para los **IEnumerables** es muy ineficiente, así que no confundirlo con la propiedad `Count` de las listas o `Length` de los arrays.

### Creación de Métodos Extensores Personalizados

Los métodos extensores personalizados se pueden crear definiendo métodos estáticos en una clase estática. Estos métodos deben tener como primer parámetro un `this` seguido del tipo al que extienden.

```csharp
public static class EnumerableExtensions
{
    public static IEnumerable<T> GetFirstElement<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
            return item;
        throw new InvalidOperationException();
    }
}

// Uso del método extensor personalizado
int[] numbers = new int[] { 1, 2, 3 };
int firstNumber = numbers.GetFirstElement();
```

En este ejemplo, `GetFirstElement` es un método extensor personalizado que devuelve el primer elemento de la secuencia.