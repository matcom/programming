# Diccionarios

Los arrays son una estructura de datos fundamental en la programación, pero que sabemos que cuentan con algunas limitaciones. Una de las más notables (pero que ya hemos estudiado cómo resolver en el curso) es su tamaño fijo, que debe ser especificado en el momento de creación. Más importante aún, los arrays utilizan índices enteros para acceder a sus elementos, lo cual puede ser restrictivo en ciertas aplicaciones.

En esta clase, exploraremos cómo definir una estructura de datos que, al igual que los arrays, nos permita un acceso constante (o casi constante) a los elementos almacenados, pero que utilice cualquier tipo de objeto como índice. Esta estructura es conocida como `Dictionary<T>`, y su implementación eficiente requiere el uso de una función de hash.

## Implementación de una Estructura de Datos Dictionary<T>

Para implementar una estructura de datos que permita la recuperación de valores sin realizar una búsqueda lineal por las llaves, seguimos estos pasos:

1. Utilizar una función de hash para mapear cada **llave** a un número entero.
    - Llaves iguales deben tener `HashCode` iguales. Pero el recíproco no es necesariamente cierto.
    - `A.Equals(B)` => `A.GetHashCode() == B.GetHashCode()`.
    - `A.GetHashCode() == B.GetHashCode()` !=> `A.Equals(B)`.
2. Ajustar ese entero para que funcione como índice de un array de valores.
    - Operador `%`.
    - Construir una estructura que almacene tanto la llave como el valor.
3. Almacenar o recuperar el valor en ese índice del array.
    - Tabla de hash.
    - Habrá que lidiar con que puedan ocurrir coliciones en el array.

## I. El Método GetHashCode en C#

El método `GetHashCode` es un método incorporado en todos los objetos de C# que retorna un valor hash, un entero que se utiliza para identificar de manera única un objeto en el contexto de una tabla hash. El valor retornado por `GetHashCode` se usa para distribuir los objetos en una estructura de datos hash, como un `Dictionary` o un `HashSet`.

#### Sobrescribiendo GetHashCode

Para garantizar que `Dictionary` funcione correctamente, a veces es necesario sobrescribir el método `GetHashCode` en nuestras clases personalizadas. Aquí hay un ejemplo:

```csharp
class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName, DateOfBirth);
    }

    public override bool Equals(object obj)
    {
        if (obj is Person other)
        {
            return FirstName == other.FirstName &&
                   LastName == other.LastName &&
                   DateOfBirth == other.DateOfBirth;
        }
        return false;
    }
}
```

En este ejemplo, la combinación de `FirstName`, `LastName` y `DateOfBirth` se usa para generar un valor hash único para cada instancia de `Person`.

## II. La Clase KeyValuePair

La clase `KeyValuePair` es una estructura que contiene una llave y un valor. Aquí hay un ejemplo de cómo se utiliza:

```csharp
KeyValuePair<string, int> entry = new KeyValuePair<string, int>("apple", 3);

Console.WriteLine("Key: " + entry.Key);    // Output: Key: apple
Console.WriteLine("Value: " + entry.Value); // Output: Value: 3
```

En este ejemplo, `entry` es una instancia de `KeyValuePair` donde la llave es una cadena ("apple") y el valor es un entero (3).

## III. Manejo de Colisiones

Dado que el tamaño del array es finito y el número de llaves no necesariamente, las colisiones son inevitables. Existen dos alternativas principales para manejarlas: **Hash Cerrado** y **Hash Abierto**.

### A. Hash Cerrado

En el hash cerrado:

- Se almacena en el array el `KeyValuePair` y además un número entero.
- Si este número es negativo (-1), indica que el diccionario no contiene otra llave que comparta el mismo hash.
- En otro caso, el número representa el índice en el array donde se guardó otro `KeyValuePair` debido a una colisión.
- Para insertar un nuevo `KeyValuePair`, se recorren esos índices hasta llegar a uno con -1, lo cual indica que la nueva llave no está almacenada todavía en el diccionario. A partir de ahí, se recorre el array (de forma circular) buscando un espacio vacío donde almacenar el `KeyValuePair`.

#### Ventajas del Hash Cerrado

- Se trabaja directamente sobre un array, y no sobre una estructura enlazable, lo cual proporciona:
    - Mejor rendimiento en caché.
    - Menor consumo de memoria.

#### Desventajas del Hash Cerrado

- La tabla de hash puede llenarse, y en tal caso hay que "hacerla crecer".
- A medida que el número de `KeyValuePair` almacenados se va acercando a la capacidad máxima, la eficiencia disminuye drásticamente debido al aumento de colisiones.

### B. Hash Abierto

En el hash abierto:

- Cada entrada del array almacena una secuencia enlazable de `KeyValuePair`.
- En caso de colisión, el nuevo `KeyValuePair` se añade al final de la secuencia enlazable correspondiente.

#### Ventajas y Desventajas del Hash Abierto

- Las ventajas y desventajas del hash abierto son opuestas a las del hash cerrado:
    - Menor rendimiento en caché y mayor consumo de memoria.
    - No hay necesidad de crecer la tabla, ya que las listas enlazadas pueden crecer dinámicamente.

## Base para un Dictionary en C#

A continuación se presenta una implementación básica de un diccionario utilizando hash abierto:

```csharp
class HashDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    class LinkedNode
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public LinkedNode Next { get; set; }
        public LinkedNode Previous { get; set; }

        public LinkedNode(TKey key, LinkedNode next = null)
        {
            Key = key;
            Next = next;
            Previous = next.Previous; // Just in case. But it should be `null`.
            next.Previous = this;
        }
    }

    int[] lengths = { 37, 79, 163, 331, 673, 1361, 2729,
                      5471, 10949, 21911, 43853, 87719,
                      175447, 350899, 701819, 1403641,
                      2807303, 5614657, 11229331, 22458671,
                      44917381, 89834777, 179669557, 359339171,
                      718678369, 1437356741 };
    int lengthPos = 0;
    LinkedNode[] hashTable;

    //...

    private LinkedNode Get(TKey key, bool add, out bool added)
    {
        int index = Math.Abs(key.GetHashCode()) % hashTable.Length;
        added = false;
        for (LinkedNode node = hashTable[index]; node != null; node = node.Next)
            if (node.Key.Equals(key))
                return node;
        if (add) {
            /* // Puede que sea necesario contar el número de colisiones en el `for` anterior
            if (NeedAndCanGrow()) {
                Grow();
                return Get(key, true, out added);
            }
            */
            added = true;
            return hashTable[index] = new LinkedNode(key, hashTable[index]);
        }
        return null;
    }

    //...
}
```

## Notas Generales y Recomendaciones

1. **Funciones de Hash**: Es crucial elegir una buena función de hash que distribuya las llaves uniformemente para minimizar las colisiones.
2. **Capacidad Inicial**: Elegir una capacidad inicial apropiada puede mejorar el rendimiento.
3. **Crecimiento de la Tabla**: Implementar un mecanismo para crecer la tabla cuando esté llena, redistribuyendo los `KeyValuePair` existentes.
4. **Pruebas y Validación**: Probar el diccionario con diferentes tipos de datos y patrones de acceso para asegurar su correcto funcionamiento y rendimiento.

El `Dictionary<T>` es una estructura poderosa en C# que, cuando se implementa correctamente, proporciona acceso eficiente y flexible a los datos mediante el uso de cualquier objeto como índice.