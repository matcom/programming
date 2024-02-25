# Conferencia #5

## Arrays Bidimensionales

¿Para qué sirven los arrays simples?

- Vimos que los arrays se pueden usar para modelar colecciones de elementos de un mismo tipo.
- Los arrays definen una función de los enteros (en C#, enteros no negativos y acotados superiormente con la longitud del array) al tipo del array.

¿Para qué querríamos entonces los arrays bidimensionales?

- Para modelar situaciones en las que la asociación no es `entero -> tipo`, sino `(entero, entero) -> tipo`.
- Por ejemplo, una matriz, donde cada elemento de la matriz está determinado por su fila y columna.
- En estas situaciones, el tamañoo de cada dimensión (ejemplo, filas y columnas) no tiene por qué coincidir.

¿La introducción de arrays bidimiensionales nos permite modelar problemas que antes no podíamos?

- Realmente no, aunque sí de forma más cómoda.
- Todo array bidimensional se puede traducir en un array unidimensional cuya longitud permita almacenar la misma cantidad de elementos que el bidimensional.
    - La longitud sería la multiplicación de las longitudes de cada dimensión (`filas * columnas`).
    - El acceso al elemento en la fila `i` y columna `j` se haría a través del índice `i * (longitud de columnas) + columna`.
    - De hecho, la representación en memoria de los array multidimensionales es como un array unidimensional, y la notación usada en C# para hablar de arrays bidimensionales no es más que una comodidad sintáctica.

¿Qué los separa de los arrays de arrays?
- Cada array dentro de un array de arrays puede tener cualquier longitud. Por otro lado, cada "fila" dentro de un array bidimensional debe tener la misma cantidad de columnas y viceversa.
- Los arrays de arrays se inicializan con `null` en cada posición del array más externo. Por otro lado, los arrays bidimensionales reservan desde el inicio toda la memoria que hace falta (`filas * columnas`) e inicializa los elementos al valor por defecto del tipo.
- La sintaxis para definirlos es diferente. Lo veremos a continuación.

## Sintaxis

- Array Bidimensional de tipo T: 
    ```csharp
    T[,] // <- de esta forma se habla del tipo, como mismo int, string, etc.
    ```
    Ejemplos:
    ```csharp
    int[,] matrix;
    bool[,] mask;
    string[,] soup;
    ```

    > Se mantiene la seguridad de tipos de los arrays unidimensionales.

- Crear una instancia / objeto:
    + A partir de valores constantes:
        ```csharp
        new T[,] {
            {v00, v01, ..., v0M},
            {v10, v11, ..., v1M},
            ...,
            {vN0, vN1, ..., vNM}
        } // se está definiendo un array de NxM.
        ```
        > En la asignación a una variable se puede omitir
        el ``new T[,]``

        Ejemplo:
        ```csharp
        int[,] matrix = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } }; // matriz de 2x3.
        ```
    + Reservando espacio dinámicamente (a partir de dos enteros `n` y `m`):
        ```csharp
        new T[n, m] // array de NxM.
        ```
    > El igual que con los arrays unidimensionales, el tamaño del array se mantiene fijo, y si se quieren guardar más elementos de los que caben hay que crear una copia de mayor tamaño.

    > Aunque la cantidad de elementos entre cada dimensión (fila y columna) no tiene por qué coincidir, dentro de una misma dimensión la cantidad de elementos sí deben coincidir. O sea, todas las filas tienen la misma cantidad de columnas y todas las columnas tienen la misma cantidad de filas.
    > > De hecho, esta es una de las cosas que los diferencia de los arrays de arrays.

- Operaciones básicas sobre arrays.
    + ``(...).Length:`` para saber la capacidad máxima de elementos en total entre todas las dimensiones.
    + `(...).GetLength(dimension):` para saber la longitud de una dimensión en particular.
        * `GetLength(0):` cantidad de filas.
        * `GetLength(1):` cantidad de columnas.
    + ``(...)[i,j]``: indexar, acceder a la posición ``i,j`` del array (la interpretación habitual es de fila `i` y columna `j`).
        * Qué valores son válidos para `i` y `j`??
            - ``0 <= i < a.GetLength(0)``
            - ``0 <= j < a.GetLength(1)``
            - ``a[0, 0]``: primer elemento.
            - ``a[a.GetLength(0) - 1, a.GetLength(1) - 1]``: último elemento.

- En general toda la sintaxis anterior se puede extender para arrays N dimensionales a partir de añadir una coma (`,`) extra por cada dimensión.
    Ejemplo:
    ```csharp
    int[,,] Space3D = new int[,,] {
        { { 1,  2 }, {  3,  4 } },
        { { 5,  6 }, {  7,  8 } },
        { { 9, 10 }, { 11, 12 } }
    };
    int value = Space3D[2, 1, 0];
    Console.WriteLine(value); // 11
    Console.WriteLine(Space3D.GetLength(0)); // 3
    Console.WriteLine(Space3D.GetLength(1)); // 2
    Console.WriteLine(Space3D.GetLength(2)); // 2
    ```

> Un concepto muy útil para recorrer los arrays bidimensionales son los "arrays de movimiento". En los códigos de ejemplo de la conferencia pueden encontrar algunas referencias a ello. En clases prácticas estarán profundizando al respecto. 