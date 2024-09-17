# `IList<T>` e `ICollection<T>`

Los arrays tienen limitaciones significativas como estructuras de datos para modelar colecciones indexables. Aunque son eficientes en el acceso directo a elementos por índice, tienen un tamaño fijo que debe definirse al inicio y no puede modificarse dinámicamente sin crear una nueva instancia. Sin embargo, no son la única forma de modelar la idea de una colección indexable en programación. Las listas, como estructuras de datos en general, ofrecen una alternativa más flexible y dinámica para almacenar elementos ordenados y acceder a ellos mediante índices.

## `IList<T>`

La interfaz `IList<T>` en C# define un contrato para una colección indexable de elementos del tipo T. Permite acceder, insertar, eliminar y modificar elementos en una secuencia ordenada a través de índices enteros. Esta interfaz hereda de `ICollection<T>` y extiende `IEnumerable<T>` y `IEnumerable`, proporcionando así métodos para manipular colecciones de elementos de manera eficiente y flexible.

```csharp
public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    T this[int index] { get; set; }
    int IndexOf(T item);
    void Insert(int index, T item);
    void RemoveAt(int index);
}
```

- **this[int index]**: Permite acceder a un elemento específico mediante su índice y modificarlo si es necesario.
- **IndexOf(T item)**: Devuelve el índice de la primera aparición del elemento especificado dentro de la lista, o -1 si no se encuentra.
- **Insert(int index, T item)**: Inserta un elemento en la posición especificada del índice, desplazando los elementos existentes hacia la derecha.
- **RemoveAt(int index)**: Elimina el elemento en la posición especificada del índice y desplaza los elementos siguientes hacia la izquierda para llenar el espacio vacío.

## `ICollection<T>`

La interfaz `ICollection<T>` en C# define un contrato para una colección de elementos de tipo T. Esta interfaz proporciona métodos para trabajar con colecciones de elementos de manera eficiente y flexible, sin especificar el orden de los elementos.

```csharp
public interface ICollection<T> : IEnumerable<T>, IEnumerable
{
    int Count { get; }
    bool IsReadOnly { get; }
    void Add(T item);
    void Clear();
    bool Contains(T item);
    void CopyTo(T[] array, int arrayIndex);
    bool Remove(T item);
}
```

- **Count**: Proporciona el número de elementos presentes en la colección.
- **IsReadOnly**: Indica si la colección es de solo lectura o permite modificaciones.
- **Add(T item)**: Agrega un elemento al final de la colección.
- **Clear()**: Elimina todos los elementos de la colección.
- **Contains(T item)**: Determina si la colección contiene un elemento específico.
- **CopyTo(T[] array, int arrayIndex)**: Copia los elementos de la colección a un array, comenzando en el índice especificado del array de destino.
- **Remove(T item)**: Elimina la primera aparición de un elemento específico de la colección.

## `ArrayList<T>`

La implementación de una lista dinámica, como `ArrayList<T>`, se basa en el uso de un array subyacente que se redimensiona dinámicamente cuando es necesario añadir más elementos de los que puede contener en su tamaño actual. Esto se logra mediante los siguientes pasos:

1. **Inicialización**: Se crea un array con una capacidad inicial fija. En el caso de `ArrayList<T>`, esto se especifica al momento de la creación del objeto.

2. **Adición de elementos**: Cuando se añade un nuevo elemento a la lista utilizando el método `Add(T item)`, se verifica si la cantidad actual de elementos (`Count`) es menor que la capacidad del array. Si es así, el elemento se agrega al final del array en la posición correspondiente a `Count` y luego se incrementa el contador de elementos.

3. **Redimensionamiento**: Si la cantidad de elementos alcanza la capacidad máxima del array, se crea un nuevo array con una capacidad mayor (generalmente aumentando la capacidad actual en un tamaño predefinido o proporcional). Luego, se copian todos los elementos del array actual al nuevo array y se reemplaza el array subyacente por el nuevo. Esto asegura que haya suficiente espacio para más elementos sin necesidad de desplazar o reorganizar elementos existentes.

Este enfoque optimiza la eficiencia de operaciones de adición y garantiza un rendimiento adecuado en la mayoría de los casos, aunque puede tener un costo de tiempo ocasional durante la operación de redimensionamiento del array.


```csharp
class ArrayList<T> : IList<T>
{
    T[] array;
    int increaseLength;

    public int Count { get; private set; }

    public ArrayList(int increaseLength = 1000)
    {
        array = new T[increaseLength];
        this.increaseLength = increaseLength;
        Count = 0;
    }

    public void Add(T x)
    {
        if (Count < array.Length)
            array[Count++] = x;
        else
        {
            //Crear un array de mayor longitud para poder poner el nuevo elemento
            T[] values = new T[array.Length + increaseLength];
            System.Array.Copy(array, 0, values, 0, array.Length);
            values[Count++] = x;
            array = values;
        }
    }
    public bool Contains(T x)
    {
        for (int k = 0; k < Count; k++)
            if (array[k].Equals(x)) return true;
        return false;
    }

    public T this[int i]
    {
        get
        {
            if (i < Count) return array[i];
            else throw new Exception("Index out of range");
        }
        set
        {
            if (i < Count) array[i] = value;
            else throw new Exception("Index out of range");
        }
    }

    // ...
}
```

Esta implementación demuestra cómo se puede crear una estructura de datos dinámica utilizando un array subyacente en C#.

## `LinkedList<T>`

La `LinkedList<T>` es una estructura de datos enlazada que permite almacenar una colección de elementos de tipo `T` de manera dinámica. A diferencia de las listas basadas en arrays, donde los elementos están almacenados en ubicaciones contiguas de memoria, una lista enlazada organiza los elementos como nodos individuales que están conectados entre sí mediante referencias.

### Objetivos

La `LinkedList<T>` tiene como objetivo principal optimizar las operaciones de inserción y eliminación en comparación con las listas basadas en arrays, donde agregar o eliminar elementos puede requerir mover o redimensionar grandes bloques de memoria. Sus principales objetivos incluyen:

- **No tener espacio reservado sin usar**: A diferencia de un array donde se debe definir un tamaño fijo, una lista enlazada utiliza solo el espacio necesario para los elementos que contiene.
  
- **No necesitar expansión cuando se llena**: No requiere redimensionamiento de estructuras de datos subyacentes al agregar elementos nuevos, ya que cada elemento se aloja en un nuevo nodo enlazado.

- **No necesitar desplazar elementos al insertar o eliminar al comienzo o final**: Las operaciones de inserción y eliminación son más eficientes en términos de tiempo, ya que solo implican ajustar las referencias entre nodos, en lugar de mover elementos en una estructura de datos contigua.

### ¿Cómo conseguirlo?

Seguir una idea similar a la de **boxing**: para formar una colección de elementos, voy a envolver cada elemento en un objeto `Linkable<T>`. El objeto que envuelve a parte de conocer el valor, mantiene una referencia al siguiente enlazable.

```csharp
public class Linkable<T>
{
    T Value {get; private set;}
    Linkable<T> Next {get; private set;}
}
```

- **Value**: Almacena el valor del elemento actual.
- **Next**: Mantiene una referencia al siguiente nodo en la secuencia, permitiendo la navegación secuencial a través de la lista.

### Implementación de `LinkedList<T>`

La clase `LinkedList<T>` maneja la estructura de la lista enlazada, manteniendo referencias al primer y último nodo de la lista:

```csharp
class LinkedList<T> : IList<T>
{
    Linkable<T> first;
    Linkable<T> last;

    public int Count { get; private set; }

    public LinkedList()
    {
        Count = 0;
    }

    public void Add(T x)
    {
        Linkable<T> node = new Linkable<T>(x, null);
        if (Count == 0)
        {
            first = node;
            last = node;
            Count = 1;
        }
        else
        {
            last.Next = node;
            last = node;
            Count++;
        }
    }

    public bool Contains(T x)
    {
        Linkable<T> cursor = first;
        for (int i = 0; i < Count; i++)
            if (cursor.Value.Equals(x)) return true;
            else
                cursor = cursor.Next;
        return false;
    }

    public T this[int i]
    {
        get
        {
            Linkable<T> cursor;
            if (i >= 0 && i < Count)
            {
                cursor = first;
                for (int k = 0; k < i; k++)
                    cursor = cursor.Next;
                return cursor.Value;
            }
            else throw new Exception("Index out of range");
        }
        set
        {
            Linkable<T> cursor;
            if (i >= 0 && i < Count)
            {
                cursor = first;
                for (int k = 0; k < i; k++)
                    cursor = cursor.Next;
                cursor.Value = value;
            }
            else throw new Exception("Indice fuera de rango");
        }
    }

    // ...
}
```

Esta implementación de `LinkedList<T>` permite agregar elementos al final de la lista de manera eficiente, recorrer la lista para verificar la presencia de un elemento y acceder a elementos específicos por índice, aprovechando la flexibilidad y eficiencia de las listas enlazadas para operaciones dinámicas en C#.

## Notas generales

- `List<T>` es una opción común para almacenar y manipular colecciones de elementos en C# debido a su eficiencia y versatilidad.
- `LinkedList<T>` puede ser más adecuado en situaciones donde se necesitan frecuentes inserciones y eliminaciones en el medio de la lista.
