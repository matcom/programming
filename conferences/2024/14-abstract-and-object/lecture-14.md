# Clases Abstractas y Objetos

Desde la programación orientada a objetos, C# nos ofrece varias herramientas poderosas para diseñar software flexible y reutilizable. Entre estas herramientas se encuentran las **clases abstractas**, que permiten definir comportamientos comunes y establecer contratos entre las clases, aún cuando no sabemos a priori cómo implementar una funcionalidad.

En esta conferencia desarrollaremos estos conceptos y su aplicación en C#. Además, estaremos interactuando con algunos [métodos comunes a todos los objetos de C#](https://learn.microsoft.com/en-us/dotnet/api/system.object?view=net-8.0), como `Equals` y `ToString`. Hacia el final, estaremos chocando con algunas limitación de la modelación basada, estrictamente, con clases abstractas.

## Clases Abstractas

Una clase abstracta es una clase que no puede instanciarse directamente y está destinada a ser una base para otras clases. En C#, se utiliza la palabra clave `abstract` para declarar una clase como abstracta.

```csharp
public abstract class <Name>
{
    // ...
}
```

Una vez que una clase ha sido declarada como abstract, podrá marcar varios de sus campos como abstractos usando la palabra clave `abstract`.

```csharp
public abstract class <Name>
{
    // notar que no se está dando implementación al método.
    <visibilidad> abstract <Type> <MethodName>(<Params>); 

    // ... y en este caso tampoco a la propiedad.
    <visibilidad> abstract <Type> <PropertyName> {
        get; set; 
    }
}
```

A efectos prácticos esto producirá el mismo comportamiento que `virtual`. La diferencia fundamental radica en que los descendientes deberán proveer una implementación de los campos marcados como `abstract` si quieren dejar de ser clases abstractas. En caso de no hacerlo, los descendientes deberán ser marcados como clases abstractas, lo cual conlleva pagar el precio de no poder crear instancias suyas (equivale a no poder utilizarlos, dado que en este tipo de clases los campos de instancia son los que valen).

> OJO: No todos los campos de una clase abstracta deben ser abstractos. Sin embargo, basta con que al menos algún campo deba ser abstracto para que la clase deba marcarse como abstracta

### ¿Por qué utilizar clases abtractas?


Las clases abstractas son fundamentales cuando se desea crear una jerarquía de clases que comparten una base común pero que también requieren implementaciones específicas en cada derivada.

- **Factorizar Funcionalidades Comunes**

    Es útil factorizar funcionalidades comunes en la clase padre cuando ninguna implementación concreta tiene sentido en ese nivel de abstracción. Por ejemplo:

    ```csharp
    public abstract class Animal
    {
        public abstract void MakeSound(); // No tiene sentido definir un sonido específico a este nivel.
    }
    ```

- **Definir Funciones Dependientes de Implementaciones Concretas**

    Las clases abstractas pueden definir métodos que dependen de otros métodos que tendrán implementaciones concretas en las clases derivadas:

    ```csharp
    public abstract class Shape
    {
        public abstract double Area();

        public void DisplayArea()
        {
            Console.WriteLine($"The area is {Area()}");
        }
    }
    ```

### Resumen de Reglas y Consideraciones

1. **Solo las clases abstractas pueden tener métodos abstractos**. Los métodos abstractos no tienen implementación en la clase base y deben ser implementados en las clases derivadas.

2. **Clases derivadas abstractas**.  Si una clase derivada no proporciona una definición para todos los métodos abstractos de su base, debe ser marcada también como `abstract`.

3. **No se pueden instanciar clases abstractas**. No se pueden crear instancias de clases abstractas porque no están completamente definidas. Existen campos de instancia para los que el programador todavía no ha dado una implementación.

4. **Métodos y clases `static` no pueden ser abstractos**. La palabra clave `static` no se puede combinar con `abstract`.

5. **Uso de la palabra clave `new`**. Usar `new` rompe el mecanismo de `(virtual|abstract)+override`. Para restaurarlo parcialmente, se debe volver a marcar el método como `virtual`.

    ```csharp
    public abstract class BaseClass
    {
        public abstract void Display();
    }

    public class DerivedClass : BaseClass
    {
        public override void Display()
        {
            Console.WriteLine("Base Display");
        }
    }

    public class OtherDerivedClass : DerivedClass
    {
        public new virtual void Display()
        {
            Console.WriteLine("Derived Display");
        }
    }
    ```

    > ¿Cómo se comportaría `.Display()` en cada situación?

    ```csharp
    BaseClass a = new BaseClass(); // <- error de compilación por supuesta ;-)
    BaseClass b = new DerivedClass();
    BaseClass c = new OtherDerivedClass();
    DerivedClass d = new DerivedClass();
    DerivedClass e = new OtherDerivedClass();
    OtherDerivedClass f = new OtherDerivedClass();
    ```

## Métodos `Equals` y `ToString`

Sabemos que todos los tipos en C# deriban del tipo `Object`. Por tanto, hay algunos métodos se son comunes a todos los objetos que usemos aunque no los hayamos definido explícitamente en la clase.

### Equals

El método `Equals` se utiliza para determinar si dos instancias de un objeto son iguales. Es crucial sobrescribir este método cuando se necesita comparar objetos basados en sus valores.

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Person other = (Person)obj;
        return Name == other.Name && Age == other.Age;
    }

    // Necesario siempre que sobreescribamos el Equals, pero por ahora ignoremos por qué, pues tendremos una conferencia solo para esto.
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Age);
    }
}
```

> ¿Por qué usar `==` en lugar de `Equals`?  
> R./ Si revisan verán que los operadores no son polimórficos aunque se puedan sobreescribir.

### ToString

El método `ToString` se utiliza para obtener una representación en cadena de un objeto. Es útil para proporcionar una descripción legible del objeto, especialmente para propósitos de depuración o registro.

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    public override string ToString()
    {
        return $"Name: {Name}, Age: {Age}";
    }
}
```

### ¿CompareTo?

`Object` no provee una implementación base (ni mucho menos abstracta, por supuesto) de una especie de método `CompareTo` que nos permita de forma polimórfica comparar alguna relación de orden entre objetos cualesquiera que tenga sentido comparar. Esto sería útil para ordenar colecciones de objetos.

> ¿Cómo podríamos definir un tipo que encapsule la idea de que todos los objetos que son comparables?


```csharp
public abstract class Comparable
{
    public int CompareTo(Comparable other);
}

public class Person : Comparable
{
    public string Name { get; set; }
    public int Age { get; set; }

    public int CompareTo(Comparable other)
    {
        if (other == null)
            return 1;

        return Age.CompareTo(other.Age);
    }
}
```

Recordemos que en C# una clase solo puede heredar directamente de otra clase. Así que el mecanismo diseñado arriba es muy restrictivo, pues toda clase debería heredar de ella justo al principio de la jerarquía. Esto por supuesto que no es viable porque cuando querramos modelar lo mismo para otra funcionalidad habrá conflicto entre si heredar de `Comparable` o de la nueva clase.

La semana próxima estaremos solventando esta limitante.