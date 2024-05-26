# Interfaces y Genericidad

En esta conferencia trabajaremos con dos nuevas herramientas que ofrece C# para diseñar software flexible y reutilizable: las _interfaces_ y los tipos genéricos. Estos conceptos permiten crear aplicaciones modulares y adaptables, mejorando tanto la robustez como la mantenibilidad del código.

## Interfaces

Las _interfaces_ son un mecanismo en C# que:

+ Define tipos, de manera similar a las clases y enumeraciones.
+ Establece un protocolo que deben seguir los objetos que implementen la interface.
    > Esto es útil para definir métodos que funcionen con cualquier objeto cuyo tipo **implemente** la interface.

### Sintaxis

```csharp
interface InterfaceName {
    void FuncA(int arg1);
    // ...
    int FuncB(int arg1, ..., int argN);
}
```
> **Nota:** Los métodos en una interfaz no tienen modificadores de visibilidad porque deben ser `public`.

En este punto, pueden estar preguntándose por qué usar interface si ya tenemos clases `abstract` (las funcionalidades de la interface podrían ser sustituidas por métodos abstractos en una clase abstracta).

+ Una clase puede heredar solo de otra clase, pero puede implementar más de 1 interface!!!
    * Por tanto, si se tiene una grupo de métodos que funcionan sobre una interface, y un nuevo tipo que se está definiendo quiere aprovecharse de esas funcionalidades ya implementadas, simplemente implementa la interface (o interfaces) correspondientes, lo cual se traduce en proveer una implementación de las funcionalidades exigidas en la interface.

+ ¿Pero por qué es posible? ¿Qué teníamos en las clases que no nos permitian heredar de más de 1?
    * Con la herencia se transmiten campos de instancia: pero cómo organizar la memoria cuando se está heredando de multiples clases?
    * Como eso hay muchas otras complicaciones, como qué hacer en caso de que se hereden campos con el mismo nombre desde los padres.
        - En lenguajes como python se resuelve de forma natural estableciendo un orden de resolución de nombres (en python todo nombre se resuelve en ejecución y por eso lo "natural").
    * En las interfaces solo se incluyen las carcasas de las definiciones.
        - No se pueden declarar variables de instancia, etc.
            + Pero propiedades sí.

+ Cuando se implementan múltiples interfaces que comparten un método con el mismo nombre y firma (parámetros y tipos), es necesario usar la implementación explícita de cada método de las interfaces (para desambiguar en caso de que no se quiera la misma implementación (o no sea posible porque tienen valores de retorno distinto)).
    ```csharp
    class ClassName : IInterfaceA, IInterfaceB {

        public void CommonMethod() {
            // Implementación común
        }

        void IInterfaceB.CommonMethod() {
            // Implementación específica para IInterfaceB
        }
    }
    ```

    > Cuando se usa la implementación explícita no se pone modificador de visibilidad (porque es una implementación que solo es visible si el objeto está siendo referido desde la interface)
    > > Solo la implementación implicita puede ser llamada desde la clase.

+ Las interfaces pueden "implementar" (heredar de) otras interfaces.

### Ejemplo

```csharp
public interface IA
{
    void Pepe();
}

public interface IB
{
    void Pepe();
}

public interface IC
{
    int Pepe();
}

// MyClass m = new MyClass();
// m.Pepe(); da error de compilación
class MyClass : IA, IB, IC
{
    void IB.Pepe()
    {
        throw new NotImplementedException();
    }

    void IA.Pepe()
    {
        throw new NotImplementedException();
    }

    int IC.Pepe()
    {
        throw new NotImplementedException();
    }
}

class MyClass2 : MyClass
{
    public void Pepe2()
    {
        (this as IA).Pepe();
    }
}
```

> Implementando solo IA e IB no causa conflicto.
>
> > Aun así es posible dar definiciones independientes.
>
> Implementando IA o IB y IC causa conflicto por lo que hay que usar notación explícita.
>
> Las implementaciones explicitas no pueden ser accedidas salvo si se referencian desde la interface correspondiente.

## Genericidad

La _genericidad_ es un mecanismo de C# que:

+ Permite definir tipos (y funciones) cuya definición depende de uno o más tipos que pueden variar en la práctica.
+ `Array` es la estructura de datos genérica más básica que conocen.
    * La analogía con el caso en que no fuera genérica sería que los arrays no almacenaran elementos de un tipo en concreto, sino de cualquier tipo, por lo que para recuperar el objeto original habría que castear.
+ Evita tener que crear clases con las mismas funcionalidades donde solo varía el tipo de dato con el que trabaja: aumenta la flexibilidad manteniendo la robustez.
+ Seguridad de tipos.
    * En tiempo de compilación de pueden detectar errores causados por el mal uso de las funcionalidades de la clase según el tipo del que dependen.

### Sintaxis

```csharp
class ClaseName<T> {
    T item;

    public T GetItem() {
        // ...
    }

    public void SetItem(T item) {
        // ...
    }
}
```

```csharp
public T MethodName<T>(T arg) {
    // ...
}
```
> `T` representa un comodín que debe ser reemplazado por un tipo específico al usar la clase o método genérico.

### Genericidad restringida

Se puede restringir el tipo genérico `T` al subconjunto que cumpla ciertas condiciones.

```csharp
class Generic<T> where T : ClassName { }
class Generic<T> where T : InterfaceName { }
class Generic<T> where T : class { }
class Generic<T> where T : struct { }
class Generic<T> where T : new() { }
```

> Tenemos pendiente un tema muy interesante sobre la genericidad y el principio de sustitución: la varianza (covarianza y contravarianza).

## Abstrayendo funcionalidades

### ¿CompareTo?

La semana pasada mencionamos que `Object` no proporciona una implementación base de un método `CompareTo` para comparar objetos de manera polimórfica. Esto sería útil para ordenar colecciones de objetos.

Repetimos la misma pregunta entonces:
> ¿Cómo podríamos definir un tipo que encapsule la idea de que todos los objetos que son comparables?


```csharp
public interface IComparable
{
    int CompareTo(object other);
}

public class Person : IComparable
{
    public string Name { get; set; }
    public int Age { get; set; }

    public int CompareTo(object other)
    {
        if (other == null)
            return 1;

        Person otherPerson = other as Person;
        if (otherPerson == null)
            return 1;

        return Age.CompareTo(otherPerson.Age);
    }
}
```

```csharp
public interface IComparable<T>
{
    int CompareTo(T other);
}

public class Person : IComparable<Person>
{
    public string Name { get; set; }
    public int Age { get; set; }

    public int CompareTo(Person other)
    {
        if (other == null)
            return 1;

        return Age.CompareTo(other.Age);
    }
}
```

### Ordenación con `IComparable<T>`
```csharp
public interface IComparable<in T>
{
    int CompareTo(T other);
}
```

```csharp
#region ORDENAR CON ICOMPARABLE<T>
static void Ordenar<T>(T[] items) where T:IComparable<T>{
    for (int k = 0; k < items.Length - 1; k++)
        for (int j = k + 1; j < items.Length; j++)
            if (items[j]. CompareTo(items[k])<0) {
                T temp = items[j];
                items[j] = items[k];
                items[k] = temp;
            }
}
#endregion
```

### Ordenación con `IComparer<T>`

```csharp
public interface IComparer<in T>
{
    int Compare(T x, T y);
}
```

```csharp
#region ORDENAR CON ICOMPARER<T>
static void Ordenar<T>(T[] items, IComparer<T> comparador)
{
    for (int k = 0; k < items.Length - 1; k++)
        for (int j = k + 1; j < items.Length; j++)
            if (comparador.Compare(items[j], items[k]) < 0)
            {
                T temp = items[j];
                items[j] = items[k];
                items[k] = temp;
            }
}
#endregion
```