# Conferencia #3

## Arrays

¿Por qué son necesarios?

Agrupar determinada cantidad de elementos en una colección,
de forma que el procesamiento sobre cada valor se puede
escribir en función de un índice, y no necesariamente
de uno en uno.

> Por ejemplo, saber cuáles de los nombres de mis amigos son palíndromos.
> + La cantidad de nombres lo sé en compilación: No va a cambiar de ejecución en ejecución.
> + Sin embargo, no quiero escribir manualmente el llamado a cada uno, así que los guardo en un array y con un ciclo (``for`` hago los llamados, y los ``Console.WriteLine()``.

Ahora, en el momento que se escribe el programa, hay casos en los que ni siquiera se sabe cuántos valores se quieren analizar.

> Por ejemplo, los números primos en un intervalo.
Como el tope del intervalo no se conoce en compilación,
sino que depende de la ejecución, entonces el programa no puede saber cuanta memoria necesita reservar.

Hasta ahora estuvieron esquivando esa limitación.
    Cómo???
> * Contando solamente.
> * Dando los resultados poco a poco
    (``Console.WriteLine``).
> * Usando ``strings`` para guardar información.
>   - Son arrays de ``char``.
>   - Pero inmutables: las operaciones
        sobre ``string`` devuelven un nuevo ``string``.  
        **NO MODIFICAN EL STRING QUE SE TENÍA**.

### Sintaxis de *C#*:
- Array de tipo T: 
    ```csharp
    T[] // <- de esta forma se habla del tipo, como mismo int, string, etc.
    ```
    Ejemplos:
    ```csharp
    int[] numbers;
    bool[] selected;
    string[] names;
    ```

    > Seguridad de tipos:
    > - Solo se pueden guardar en el array objetos de     determinado tipo.
    > - Pero gracias a eso, el compilador nos puede ayudar chequeando que las operaciones que realicemos sobre los elementos del array sean válidas.
    > - En cualquier caso, si se quiere un array que guarde todo tipo de objetos, se puede declarar un array de ``objects``.
    >     * Todo valor es ``object``, así que se puede guardar correctamente en el array.

    > ¿Qué se puede hacer con un tipo???
    > - Usarlo como tipo de variable.
    > - Como tipo de parámetro, valor de retorno, etc.

- Crear una instancia / objeto:
    + A partir de valores constantes:
        ```
        new T[] { n0, n1, ..., nl}
        ```
        > En la asignación a una variable se puede omitir
        el ``new T[]``

        Ejemplo:
        ```csharp
        int[] numbers = new int[] {1, 2, 3};
        ```

    > Con esa sintaxis no es suficiente: si el 
    compilador puede inferir el tamaño del array en 
    compilación es porque la cantidad de elementos se
    sabe desde el momento en que se escribió el
    código, por lo que entonces bastaría con declarar
    una variable para cada elemento.

    + Reservando espacio dinámicamente (a partir de un entero ``n``):
        ```csharp
        new T[n]
        ```
    > En ambos casos el tamaño del array se mantiene fijo
    desde su creación.
    > - **CONSECUENCIA:** SI NO QUEDA ESPACIO PARA UN NUEVO
    ELEMENTO HAY QUE CREAR UNA COPIA DE MAYOR TAMAÑO.

- Operaciones básicas sobre arrays.
    + ``(...).Length:`` para saber la capacidad máxima de elementos.
        * Decimos máxima porque "cuánto espacio está siendo utilizado" es una consideración que hace el programador del problema.
    + ``(...)[i]``: indexar, acceder a la posición ``i`` del array.
        * Para guardar un valor si está a la izquierda de una asignación.
        * O para recuperar el valor en otro caso.
        * Qué valores son válidos para `i`??
            - ``0 <= i < a.Length``
            - ``a[0]``: primer elemento.
            - ``a[a.Length - 1]``: último elemento.

### Consideraciones importantes:

- Qué ocurre al crear un array???
    + Se reserva espacio para cada posible elemento.
    + Cada uno se inicializa en el valor por defecto del tipo.
        * numéricos en 0.
        * strings en ``null``.
        * bools en ``false``.

- Qué ocurre al asignar un array a una variable???
    ```csharp
    int[] a = {1, 2, 3, 4, 5};
    ```
    + Si las variables son cajitas, cuán grande tengo que
    hacer la caja para guardar un array? `100`??? `1000`???
    todo lo que pueda???
        - No puede ser en función del tamaño del array: porque puede cambiar el array que almacena.
    + Los arrays son tipos por referencia (como muchos otros,
    por ejemplo: `Stopwatch`, `DateTime`, `string`, etc).
    + Las referencias tienen un tamaño fijo: en las cajitas se guarda un papel que tiene anotada la dirección del estante en el que está guardado el objeto.
    + Y por tanto, la asignación:
        ```csharp
        int[] b = a;
        ```
        causa que tanto ``a`` como ``b`` guarden una dirección al mismo objeto (al mismo array).
    + Qué consecuencia tiene esto??? Qué pasa con uno si se 
    modifica el otro???
        - Se modifican ambos, porque ambos referencian al mismo objeto.
    + Y qué pasa ahora si se añade:
        ```csharp
        int[] c = b;
        c = {6, 7, 8, 9, 10};
        ```
        * Se modifican ``a`` y ``b``???
            - NO!
        * Si modifico ``c`` cambia el array al
        que apuntan ``a`` y ``b``???
            - NO!
            - O sea, "guardar 11 en la 1ra posición de c"
            causa que:  
            ``a -> {11, 2, 3, 4, 5}``,  
            ``b -> {11, 2, 3, 4, 5}``,  
            ``c -> {11, 7, 8, 9, 10}`` ???
                - NO!

            > **OJO:** La sintaxis para "guardar 11 en la 1ra posición de c" es ``c[0] = 11``.

- Y qué pasa cuando un array se pasa como parámetro???
    + Tras retornar se mantiene modificado??
        - Sí!
    + Puedo cambiar el que me pasaron a una nueva referencia??
        - Con los mecanismos que conocen actualmente NO.
- Pueden crearse arrays de arrays???
    + Sí!!!
    + ``T[]`` es un tipo ``T'``, así que un array de ``T'``
    sería ``T'[]``, concretamente ``T[][]``.
    + Y cuánto espacio se reserva entonces en la creación
    entonces??? Se reservaría el espacio de los subarrays??
        * No!!!
        * El tamaño de los subarrays no tiene si quiera que
        ser el mismo.
        * Incluso pueden cambiar sin cambiar el array que lo 
        contiene (como mismo pasa con entero, etc).
        * El espacio que se almacena es el de la referencia ;-)
    + Y con qué valor se inicializa???
        * ``null``, como todos los objetos por referencia.

## Ciclos "For"

Otra sintaxis que provee C# para repetir un conjunto de instrucciones ("hacer un ciclo").
Está diseñado para que quede indicado, explícitamente, cual es el valor que irá cambiando (y cómo lo hará) a lo largo de las iteraciones, el cual servirá para evaluar las instrucciones y determinar cuándo el ciclo debe terminar.

Son especialmente útiles para "iterar" sobre los valores que almacena un array.

### Sintaxis de **for**.

```csharp
for(<inicialización>; <condición>; <actualización>) {
    <instrucciones>;
}
```

- `<inicialización>`, `<condición>` y `<actualización>` son opcionales.  
- `<condición>` es una expresión que avalúa a `bool`.
- `<inicialización>`, `<actualización>` son instrucciones (ejemplo: asignación, incremento, decremento, llamado a métodos, instanciación de objetos).  
- `for( ; ; ) { //... }` es equivalente a `while( true ) { //... }`

#### Ejemplo

```csharp
int names = { "Juan", "Pablo", "Consuegra", "Ayala" };
for (int i = 0; i < names.Length; i++) {
    Console.WriteLine(names[i]);
}
```

En general, el ciclo `for` es equivalente al siguiente ciclo `while` (aunque las instrucciones de control de flujo como `continue` tiene un comportamiento diferente):

```csharp
<inicialización>;
while (<condición>) {
    <instrucciones>;
    <actualización>;
}
```
    
- El comportamiento de **break** y **return** es el mismo que con los ciclos `while`.
- El **continue** pasa por hacer `<actualización>` y luego es que evalúa `<condición>`. 