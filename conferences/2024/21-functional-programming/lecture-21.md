# Programación Funcional en C#

La programación funcional es un paradigma de programación que trata la computación como la evaluación de funciones matemáticas y evita el cambio de estado y los datos mutables. En lugar de realizar tareas mediante instrucciones secuenciales, la programación funcional se enfoca en aplicar y componer funciones.

En C#, este paradigma se facilita al permitir que las funciones sean ciudadanos de primera clase. Esto significa que las funciones pueden ser asignadas a variables, pasadas como argumentos a otras funciones, y retornadas como resultados de otras funciones. Esto potencia la flexibilidad y expresividad del código, permitiendo construir abstracciones más claras y reutilizables.

## Delegados [Define un tipo]

Un delegado en C# es un tipo que representa referencias a métodos con una firma específica. Los delegados son útiles porque permiten tratar los métodos como objetos que pueden ser asignados a variables y pasados como parámetros.

### Sintaxis de los Delegados

Para declarar un delegado, se utiliza la palabra clave `delegate`, seguida de la firma del método que el delegado puede representar.

```csharp
public delegate int Operacion(int x, int y);
```

### Uso de Delegados

Una vez declarado, se puede crear una instancia del delegado y asignarle un método que coincida con su firma.

```csharp
public class Calculadora
{
    public static int Sumar(int x, int y) => x + y;
    public static int Restar(int x, int y) => x - y;
}

class Program
{
    static void Main()
    {
        Operacion operacion = new Operacion(Calculadora.Sumar);
        int resultado = operacion(3, 4); // 7

        operacion = Calculadora.Restar;
        resultado = operacion(10, 5); // 5
    }
}
```

## Delegados Anónimos [Define un objeto]

Los delegados anónimos permiten definir un delegado en línea sin tener que declarar un método separado.

### Sintaxis de los Delegados Anónimos

Se utilizan con la palabra clave `delegate`, seguida de los parámetros y el cuerpo del método.

```csharp
Operacion operacion = delegate(int x, int y)
{
    return x * y;
};
```

### Uso de Delegados Anónimos

Los delegados anónimos pueden ser utilizados de la misma manera que los delegados regulares.

```csharp
class Program
{
    static void Main()
    {
        Operacion operacion = delegate(int x, int y)
        {
            return x * y;
        };
        int resultado = operacion(3, 4); // 12
    }
}
```

## Funciones Lambda [Define un objeto]

Las funciones lambda son una forma más concisa de escribir delegados anónimos. Utilizan la sintaxis `=>`, conocida como el operador lambda, para separar los parámetros del cuerpo de la función.

### Sintaxis de las Funciones Lambda

```csharp
Operacion operacion = (x, y) => x / y;
```

### Uso de Funciones Lambda

Las funciones lambda simplifican el uso de delegados, haciéndolos más fáciles de leer y escribir.

```csharp
class Program
{
    static void Main()
    {
        Operacion operacion = (x, y) => x / y;
        int resultado = operacion(12, 4); // 3
    }
}
```

## Expresiones Lambda y LINQ

Las expresiones lambda son extremadamente útiles cuando se combinan con LINQ (Language Integrated Query), proporcionando una forma declarativa y poderosa de manipular colecciones de datos.

```csharp
class Program
{
    static void Main()
    {
        int[] numeros = { 1, 2, 3, 4, 5, 6 };
        var numerosPares = numeros.Where(n => n % 2 == 0).ToList();

        foreach (var num in numerosPares)
        {
            Console.WriteLine(num); // 2, 4, 6
        }
    }
}
```

## Eventos

La programación funcional en C# no solo mejora la claridad del código, sino que también permite una mayor flexibilidad y reusabilidad. Los eventos en C# son un recurso que utiliza delegados, permitiendo que los métodos de gestión de eventos sean asignados y llamados de manera flexible.

Los eventos en C# son una forma de que una clase notifique a otras clases cuando ocurre algo interesante. Los eventos se basan en delegados y pueden beneficiarse de las mismas técnicas funcionales discutidas anteriormente.

```csharp
public class Publicador
{
    public event EventHandler Evento;

    public void DispararEvento()
    {
        Evento?.Invoke(this, EventArgs.Empty);
    }
}

public class Suscriptor
{
    public void ManejarEvento(object sender, EventArgs e)
    {
        Console.WriteLine("Evento manejado.");
    }
}

class Program
{
    static void Main()
    {
        var publicador = new Publicador();
        var suscriptor = new Suscriptor();

        publicador.Evento += suscriptor.ManejarEvento;
        publicador.DispararEvento(); // "Evento manejado."
    }
}
```

Utilizar delegados, delegados anónimos y expresiones lambda en C# facilita la adopción de conceptos de programación funcional, haciendo el código más modular, legible y fácil de mantener.

## Conclusiones

La programación funcional en C# proporciona una manera poderosa y expresiva de escribir código, aprovechando las capacidades del lenguaje para tratar las funciones como ciudadanos de primera clase. A través del uso de delegados, delegados anónimos y funciones lambda, es posible escribir código más conciso, flexible y fácil de mantener. Estas características permiten abstraer y modularizar mejor la lógica de las aplicaciones, reduciendo la complejidad y mejorando la legibilidad.

Adoptar técnicas de programación funcional en C# no solo enriquece las capacidades del desarrollador para crear soluciones más elegantes y robustas, sino que también abre la puerta a un estilo de programación que promueve la inmutabilidad, la composición y la modularidad. Por lo tanto, es altamente recomendable familiarizarse con estos conceptos y aplicarlos en el desarrollo diario para aprovechar al máximo las ventajas que ofrecen.