# Conferencia #4

## Retomando tipos

Paradigma de programación: Programación Orientada a Objetos (POO)
- Tipos vs Objetos
- Los **tipos** definen las características que deben cumplir todas las instancias / objetos de ese tipo.
- Los **objetos** representan instancias concretas de tipos, a los que se le asigna un espacio en memoria para almacenar toda la información relevante para su funcionamiento.

Los lenguajes de programación predefinen un conjunto de tipos básicos (por ejemplo, en C# tenemos `int`, `string`, `bool`).

¿Podemos crear tipos propios?
> Sí, a partir de componer tipos básicos o complejos.

¿Por qué querríamos definir tipos nuevos?
- Utilizar con una sintaxis más cómoda para modelar el problema.
    > Hablar de los tipos y objetos propios del dominio, y no de tipos básicos.
- Encapsular comportamientos que realizan los objetos del dominio.
    > Hablar de "qué" pero ocultar el "cómo".

C# provee múltiples recursos para definir tipos, entre los que destacan las _clases_, _struct_, _enum_, _delegates_, etc.
En esta clase estudiaremos la sintaxis para definir tipos usando _clases_ y _enum_.

Vale recordar que una vez que un tipo existe en el programa este puede usarse como tipo de una variable, tipo de retorno de un método o de sus parámetros, posibilitar crear instancias de esos tipos (aunque veremos más adelante que podemos querer crear tipos de los que no se crearán instancias), etc.

## Clases

La sintaxis de C# para definir una clase es la siguiente ...

```csharp
class <Nombre> {
    <campos>
}
```

... donde los `<campos>` se utilizarán para definir las propiedades y funcionalidades que debe tener todo objeto del tipo que se está definiendo.

Algunos `<campos>` se pueden definir con la siguiente sintaxis:

- **Atributos**. Los valores que debe almacenar toda instancia (objeto) de la clase.

    ```
    <visibilidad> <acceso> <tipo> <nombre>;
    ```
    ```
    <visibilidad> <acceso> <tipo> <nombre> = <expresión> ;
    ```

    > La visibilidad se puede omitir y por defecto es `private`.
    >
    > El modificador de acceso se puede omitir, lo cual causa que el atributo sea de instancia. En caso de que se indique como `static` el acceso será a través de la clase.

    Por ejemplo

    ```csharp
    class ExampleClass {
        int number;
        public string textA = "This is a text.";
        public static string textB = "This is another text.";
    }
    ```

    Luego en otras secciones del programa podemos decir

    ```csharp
    ExampleClass example = new ExampleClass();
    Console.WriteLine(example.textA);
    Console.WriteLine(ExampleClass.textB);
    ```

- **Propiedades** para acceder y modificar los valores de las instancias.

    ```csharp
    <visibilidad> <acceso> <tipo> <nombre> {
        get {
            <instrucciones>;
        }
        set {
            <instrucciones>; // "value" es visible aquí
        }
    }
    ```
    ```csharp
    <visibilidad> <acceso> <tipo> <nombre> {get; private set;}
    ```
    ```csharp
    <visibilidad> <acceso> <tipo> <nombre> {get;}
    ```

    > La visibilidad se puede omitir y por defecto es `private`.
    >
    > El modificador de acceso se puede omitir, lo cual causa que el atributo sea de instancia. En caso de que se indique como `static` el acceso será a través de la clase.

    Por ejemplo

    ```csharp
    class Person {
        int age;
        public string Name {get; private set; }

        public int Age {
            get {
                return age;
            }
            private set {
                age = value;
            }
        }

        public int NextAge {
            get {
                return age + 1;
            }
        }
    }
    ```

    Luego en otras secciones del programa podemos decir

    ```csharp
    Person someone = new Person();
    someone.Name = "Juan";
    someone.Age = 30;
    Console.WriteLine(someone.Name); // Esto imprime "Juan"
    Console.WriteLine(someone.Age); // Esto imprime "30"
    Console.WriteLine(someone.NextAge); // Esto imprime "31"
    ```

- **Métodos** para interactuar con el estado del objeto o para cambiarlo.

    ```
    <visibilidad> <acceso> <tipo / void> <nombre> ( <parámetros> ) {
        <instrucciones>;
    }
    ```

    > La visibilidad se puede omitir y por defecto es `private`.
    >
    > El modificador de acceso se puede omitir, lo cual causa que el atributo sea de instancia. En caso de que se indique como `static` el acceso será a través de la clase.

    Por ejemplo

    ```csharp
    class Person {
        string name = "Juan";
        public void SayHello() {
            Console.WriteLine($"Hola, me llamo {name}");
        }
    }

    class MyTools {
        public static int Successor(int number) {
            return number + 1;
        }
    }
    ```

    Luego en otras secciones del programa podemos decir

    ```csharp
    Person someone = new Person();
    someone.SayHello(); // esto imprime "Juan"

    int number = MyTools.Successor(5);
    Console.WriteLine(number); // esto imprime "6"
    ```

- **Constructor**. Un tipo especial de método, encargado de especificar cómo construir instancias de la clase.

    ```
    <visibilidad> <nombre-de-la-clase> ( <parámetros> ) {
        <instrucciones>;
    }
    ```

    Por ejemplo

     ```csharp
    class Person {
        string name;
        public int Age {get; private set; }

        public Person(string name, int age) {
            this.name = name;
            Age = age;
        }

        public void SayHello() {
            Console.WriteLine($"Hola, me llamo {name}");
        }
    }
    ```

    > Si hay conflicto de nombre entre los parámetros y los cambos, se usa `this.<campo>` para desambiguar. Si no se incluye `this.` entonces se asume que se está refiriendo al parámetro o variable local en caso de conflicto.

    Luego en otras secciones del programa podemos decir

    ```csharp
    Person someone = new Person("Pablo", 30);
    someone.SayHello(); // esto imprime "Juan"
    Console.WriteLine(someone.Age) // esto imprime "30"
    ```

## Enum

Permite definir un tipo que consiste en una asignación semántica a los números enteros. Sirve para definir tipos que tienen valores fijos conocidos en tiempo de compilación.

La sintaxis de C# para definir un enum es la siguiente.

```csharp
enum <Nombre> {
    <valores separados por coma>
}
```

Por ejemplo

```csharp
enum Result {
    Win,
    Lose,
    Tie
}
```

Luego en otras secciones del programa podemos usar `Result` como cualquier tipo y para hablar de sus valores usamos `Result.Win`, `Result.Lose` y `Result.Tie`.

