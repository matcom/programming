# Profundizando sobre Árboles

Recordemos que un _árbol_ es una estructura de datos jerárquica que consiste en nodos conectados por aristas. Cada nodo contiene un valor o dato y puede tener nodos hijos. El nodo superior se llama raíz y cada nodo tiene cero o más nodos hijos. Los nodos sin hijos se llaman hojas.

Algunas definiciones:

- **Raíz**: el nodo superior del árbol.
- **Nodo interno**: nodo con al menos un hijo.
- **Hoja**: nodo sin hijos.
- **Altura**: la longitud del camino más largo desde la raíz hasta una hoja.

## Árbol Binario

Un árbol binario es un tipo de árbol en el cual cada nodo tiene como máximo dos hijos, denominados hijo izquierdo e hijo derecho.

### Ejemplo en C#

```csharp
public class NodoBinario<T> {
    public T Valor;
    public NodoBinario<T> Izquierdo;
    public NodoBinario<T> Derecho;

    public NodoBinario(int valor) {
        this.Valor = valor;
        Izquierdo = null;
        Derecho = null;
    }
}
```

Para implementar un árbol binario, se crean nodos y se establecen referencias para los hijos izquierdo y derecho. Los métodos comunes incluyen inserción, eliminación y búsqueda.

## Árbol Binario Ordenado (BST)

Un árbol binario ordenado es un árbol binario en el que para cada nodo, todos los valores en el subárbol izquierdo son menores que el valor del nodo, y todos los valores en el subárbol derecho son mayores.

### Consideraciones

- Facilita operaciones eficientes de búsqueda, inserción y eliminación.
- Mantiene sus propiedades a través de rotaciones y rebalanceos en árboles más avanzados como AVL o Red-Black.
    > Los estudiarán en EDA `;-)`

### Ejemplo en C#

```csharp
public class BST<T> {
    public NodoBinario<T> raiz;

    public void Insertar(int valor) {
        raiz = InsertarRecursivo(raiz, valor);
    }

    private NodoBinario<T> InsertarRecursivo(NodoBinario<T> raiz, T valor) {
        if (raiz == null) {
            raiz = new NodoBinario(valor);
            return raiz;
        }
        if (valor < raiz.Valor)
            raiz.Izquierdo = InsertarRecursivo(raiz.Izquierdo, valor);
        else if (valor > raiz.Valor)
            raiz.Derecho = InsertarRecursivo(raiz.Derecho, valor);

        return raiz;
    }
}
```

La inserción y búsqueda en un BST pueden ser implementadas de forma recursiva. La complejidad promedio es O(log n), pero en el peor caso puede ser O(n). Para que las operaciones sean eficientes, el árbol debe estár **balanceado** _(que tenga altura logarítmica respecto a la cantidad de nodos que almacena)_.

## Recorridos en Árboles

Existen varios métodos para recorrer un árbol. Los más comunes son los recorridos en profundidad (DFS) y en amplitud (BFS). Dentro del DFS, se incluyen los recorridos en pre-orden, post-orden y en-orden.

### Recorrido en Profundidad (DFS)

#### Recorrido en Pre-orden

En el recorrido en pre-orden, se visita primero el nodo actual, luego se recorre el subárbol izquierdo y finalmente el subárbol derecho.

```csharp
public void PreOrden<T>(Nodo<T> nodo) {
    Console.Write(nodo.Valor + " ");
    foreach (var hijo in nodo.Hijos) {
        PreOrden(hijo);
    }
}
```

#### Recorrido en Post-orden

En el recorrido en post-orden, se recorre primero el subárbol izquierdo, luego el subárbol derecho y finalmente se visita el nodo actual.

```csharp
public void PostOrden<T>(Nodo<T> nodo) {
    foreach (var hijo in nodo.Hijos) {
        PostOrden(hijo);
    }
    Console.Write(nodo.Valor + " ");
}
```

#### Recorrido en In-orden (para Árbol Binario)

En el recorrido en in-orden, se recorre primero el subárbol izquierdo, luego se visita el nodo actual y finalmente se recorre el subárbol derecho. Este recorrido es especialmente útil en árboles binarios de búsqueda, ya que produce los valores en orden ascendente.

```csharp
public void InOrden<T>(NodoBinario<T> nodo) {
    if (nodo == null) return;
    InOrden(nodo.Izquierdo);
    Console.Write(nodo.Valor + " ");
    InOrden(nodo.Derecho);
}
```

### Recorrido en Amplitud (BFS)

En el recorrido en amplitud, se visita cada nivel del árbol de izquierda a derecha, empezando por la raíz y avanzando hacia abajo nivel por nivel.

```csharp
public void BFS<T>(Nodo<T> raiz) {
    if (raiz == null) return;
    Queue<Nodo<T>> cola = new Queue<Nodo<T>>();
    cola.Enqueue(raiz);
    while (cola.Count > 0) {
        Nodo<T> nodo = cola.Dequeue();
        Console.Write(nodo.Valor + " ");
        foreach (var hijo in nodo.Hijos) {
            cola.Enqueue(hijo);
        }
    }
}
```

## Otros Tipos de Árboles

- **Quadtree:** Un quadtree es una estructura de datos en la que cada nodo tiene exactamente cuatro hijos. Se utiliza principalmente para dividir un espacio bidimensional en regiones más pequeñas.

    - Utilizado en gráficos computacionales y procesamiento de imágenes.
    - Eficiente para operaciones de colisión y búsquedas espaciales.


- **Árbol de Prefijos (Trie):** Un árbol de prefijos o trie es un árbol de búsqueda en el que cada nodo representa un prefijo común de un conjunto de cadenas. 

    - Utilizado para búsqueda eficiente de cadenas, autocompletado y corrección ortográfica.
    - Cada camino desde la raíz hasta una hoja representa una palabra.

## Notas Generales

- Los árboles son estructuras de datos fundamentales en la computación, usados en diversas aplicaciones como bases de datos, gráficos, inteligencia artificial, y más.
- La elección del tipo de árbol depende del caso de uso específico, considerando las operaciones principales y su eficiencia.
- Los árboles avanzados como AVL, Red-Black, y B-trees proporcionan mejor rendimiento garantizado en operaciones comunes, a costa de una mayor complejidad de implementación.

Con un entendimiento sólido de las estructuras de árbol y sus recorridos, se pueden abordar problemas complejos de manera más eficiente y efectiva.