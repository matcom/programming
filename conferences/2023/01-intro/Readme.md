# Introducción

## Crear un nuevo proyecto

Para comenzar, debe crear un proyecto de C# nuevo. El tipo de proyecto más sencillo es una aplicación de consola, que se puede crear con el siguiente comando:

```bash
dotnet new console -o <nombre>
```

Sustituyendo `<nombre>` por el nombre que desea dar a su proyecto. Esto creará una nueva carpeta. Dentro encontrará un archivo `Program.cs` donde escribir su código.

## Estructura de un programa en C#

La estructura típica de un programa en C# se puede ver [en el siguiente ejemplo de código](./hello-world-oldskool/Program.cs):

```cs
// Clase principal
class Program
{
    // Método por donde comienza la ejecución
    static void Main() // <-- todo esto luego veremos para que sirve
    {
        // Todas las instrucciones van entre estos dos { }
        Console.WriteLine("Hello, World!");
    }
}
```

Para programas sencillos, es posible utilizar una estructura simplificada donde se omite la clase y método principal ([ver ejemplo de código](./hello-world/Program.cs)):

```cs
Console.WriteLine("Hello World!");
```

Esta forma simplificada es lo que crea por defecto `dotnet new console` para un nuevo proyecto de consola.

## Expresiones artiméticas


La expresión más sencilla en C# es un literal numérico:

```cs
42
```

Sin embargo, esto no es muy útil como programa. Al menos es necesario poner alguna **instrucción**, que tenga algún *efecto colateral*. La instrucción más simple es la invocación de un método como `Console.WriteLine`:

```cs
Console.WriteLine(42);
```

Además de literales, es posible construir expresiones más complejas usando operadores aritméticos. Aplican todas las reglas usuales de precedencia.

```cs
Console.WriteLine(695 * 584 / 123);
```

Existen también operaciones y valores predefinidos para la mayoría de las operaciones matemáticas usuales. En la clase `System.Math` se encuentran métodos y valores para esto:

```cs
Console.WriteLine(Math.Sin(0.5 * Math.PI));
```

Un tipo especial de operación es el formato de cadenas de texto, *string formatting*, que consiste en interpolar el valor de una expresión en un lugar predeterminado de un `string`:

```cs
Console.WriteLine("El sentido de la vida es {0}", 6 * 7);
```

[Ver el ejemplo de código completo](./expressions/Program.cs).

## Variables

Una variable es un nombre asociado a una dirección de memoria donde se almacena un valor. En C# todos los valores almacenados en memoria tienen asociado un **tipo**. Además, las variables también asociado un tipo, que no es necesariamente el mismo que el valor correspondiente, pero si debe ser "compatible". Esta noción de compatibilidad y todas las cosas interesantes que se pueden hacer con tipos las veremos más adelante.

```cs
string nombre = "Alejandro";
int edad = 33;
```

Dónde quiera que se puede usar un valor literal o una expresión de cierto tipo, se puede usar también una variable del mismo tipo (u otro "compatible"):

```cs
Console.WriteLine("Hola {0}, tu edad es {1}.", nombre, edad);
```

[Ver el ejemplo de código completo](./variables/Program.cs).

## Orden de ejecución de las operaciones

Una noción muy importante en programación es la secuencialidad de las operaciones. En C# (y en todo programa), todas las instrucciones se ejecutan en un orden que (generalmente) está determinado por el orden en que aparecen escritas en el código.

Considere el siguiente programa:

```cs
int x = 5;
int y = x;

// Qué imprime esto?
System.Console.WriteLine(x);
System.Console.WriteLine(y);

x = 10;

// Y ahora?
System.Console.WriteLine(y);
```

La primera vez que se ejecuta `Console.WriteLine(y)` el valor de `y` es 5. pues justo antes se ejecutó `y=x`.

Más adelante, se ejecuta `x=10`, pero el valor de `y` **no cambia**, pues no está de ninguna forma asociado al valor de x. Por tanto cuando se ejecuta la instrucción final, `y` todavía tiene el valor `5`.

[Ver el ejemplo de código completo](./order/Program.cs).
