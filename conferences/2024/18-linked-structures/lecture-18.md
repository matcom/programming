# Estructuras enlazadas (Pila, Cola, Árbol)

Las estructuras de datos son fundamentales en la programación, permitiendo organizar y manipular datos de manera eficiente. Muchas de estas estructuras se construyen enlazando nodos, formando estructuras como listas, pilas, colas y árboles. En C#, se ofrecen implementaciones genéricas que facilitan su uso. Este documento aborda tres estructuras de datos: pila (`Stack<T>`), cola (`Queue<T>`) y árbol, explicando su sintaxis, consideraciones y detalles de implementación.

## Estructura de Datos: Pila

### Concepto

Una pila es una estructura de tipo LIFO (Last In, First Out), donde el último elemento insertado es el primero en ser retirado.

### Implementación en C#

En C#, la clase `Stack<T>` proporciona una implementación de pila genérica.

### Métodos Principales

- `void Push(T item)`: Inserta un elemento en la parte superior de la pila.
- `T Pop()`: Elimina y devuelve el elemento superior de la pila.
- `T Peek()`: Devuelve el elemento superior de la pila sin eliminarlo.

### Ejemplo de Código

```csharp
Stack<int> pila = new Stack<int>();
pila.Push(1);
pila.Push(2);
pila.Push(3);
int cima = pila.Peek(); // cima = 3
int elemento = pila.Pop(); // elemento = 3
```

### Consideraciones de Implementación

- **Con `List<T>`**: Se evita el problema de los desplazamientos ya que el acceso es por el final. Sin embargo, hay desafíos de memoria no usada y expansión dinámica.
- **Con `LinkedList<T>`**: El acceso es eficiente ya que se limita al final de la lista, eliminando problemas de acceso a elementos interiores.

## Estructura de Datos: Cola

### Concepto

Una cola es una estructura de tipo FIFO (First In, First Out), donde el primer elemento insertado es el primero en ser retirado.

### Implementación en C#

En C#, la clase `Queue<T>` proporciona una implementación de cola genérica.

### Métodos Principales

- `void Enqueue(T item)`: Inserta un elemento al final de la cola.
- `T Dequeue()`: Elimina y devuelve el elemento al principio de la cola.
- `T Peek()`: Devuelve el elemento al principio de la cola sin eliminarlo.

### Ejemplo de Código

```csharp
Queue<int> cola = new Queue<int>();
cola.Enqueue(1);
cola.Enqueue(2);
cola.Enqueue(3);
int frente = cola.Peek(); // frente = 1
int elemento = cola.Dequeue(); // elemento = 1
```

### Consideraciones de Implementación

- **Con `List<T>`**: Ineficiente, ya que cada `Dequeue()` provoca un desplazamiento.
- **Con `LinkedList<T>`**: Eficiente, ya que el acceso se limita al principio y final de la lista.
- **Array Circular**: Mantiene un puntero al inicio, y si se llena, reserva memoria adicional y reordena los elementos almacenados.

## Estructura de Datos: Árbol

### Concepto

Un árbol es una estructura jerárquica donde cada nodo puede tener múltiples hijos, pero solo un padre. Es útil para representar relaciones jerárquicas como archivos y carpetas.

### Implementación en C#

Aunque no hay una clase específica en C# para árboles genéricos, se pueden implementar utilizando clases y nodos.

### Ejemplo de Implementación de un Nodo de Árbol

```csharp
public class Nodo<T>
{
    public T Valor { get; set; }
    public List<Nodo<T>> Hijos { get; private set; }

    public Nodo(T valor)
    {
        Valor = valor;
        Hijos = new List<Nodo<T>>();
    }

    public void AgregarHijo(T valor)
    {
        Hijos.Add(new Nodo<T>(valor));
    }
}
```

### Ejemplo de Uso

```csharp
Nodo<int> raiz = new Nodo<int>(1);
raiz.AgregarHijo(2);
raiz.AgregarHijo(3);
raiz.Hijos[0].AgregarHijo(4);
```

### Consideraciones de Implementación

- **Recorrido del Árbol**: Se pueden utilizar métodos recursivos para recorrer el árbol, como en profundidad (DFS) o en amplitud (BFS).
- **Balanceo**: Para garantizar que los árboles se mantengan balanceados (profundizaremos en este tema en futuras conferencias) se requieren implementaciones más complejas.

## Notas Generales

Las estructuras de datos enlazadas son fundamentales para resolver problemas de manera eficiente. C# proporciona clases genéricas para muchas de estas estructuras, facilitando su uso y implementación. La elección de la estructura adecuada depende del problema a resolver y de las operaciones que se necesiten optimizar.

- **Pila y Cola**: Usar `Stack<T>` y `Queue<T>` según se necesiten operaciones LIFO o FIFO.
- **Árbol**: Implementar nodos y árboles personalizados para representar jerarquías complejas.

Con estas estructuras, es posible manejar datos de manera eficiente y estructurada, optimizando el rendimiento de las aplicaciones.