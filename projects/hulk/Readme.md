# HULK: Havana University Language for Kompilers

En este proyecto usted implementará un intérprete del lenguaje de programación [HULK](https://matcom.in/hulk).

Para completar el proyecto, usted debe implementar un subconjunto de HULK, que definiremos a continuación. 
HULK es un lenguaje mucho más grande que lo requerido en este proyecto, y usted tiene la libertad de implementar cualquier funcionalidad adicional que desee.

> En 3er año, en la asignatura de Compilación, usted verá como implementar un compilador completamente funcional del lenguaje HULK en su totalidad.

## El lenguaje HULK (simplificado)

HULK es un lenguaje de programación imperativo, funcional, estática y fuertemente tipado. Casi todas las instrucciones en HULK son expresiones. 
En particular, el subconjunto de HULK que usted implementar se compone solamente de expresiones que pueden escribirse en una línea.

### Expresiones básicas

Todas las instrucciones en HULK terminan en `;`. La instrucción más simple en HULK que hace algo es la siguiente:

```js
print("Hello World");
```

HULK además tiene expresiones aritméticas:

```js
print((((1 + 2) ^ 3) * 4) / 5);
```

Y funciones matemáticas básicas:

```js
print(sin(2 * PI) ^ 2 + cos(3 * PI / log(4, 64)));
```

> HULK soporta también expresiones multi-línea, pero esas no son requeridas en este proyecto.
> HULK tiene tres tipos básicos: `string`, `number`, y `boolean`. Además en HULK se pueden definir tipos nuevos, pero en este proyecto no es requerido.

### Funciones

En HULK hay dos tipos de funciones, las funciones _inline_ y las funciones regulares. En este proyecto solo debe implementar las funciones _inline_. Tienen la siguiente forma:

```js
function tan(x) => sin(x) / cos(x);
```

Una vez definida una función, puede usarse en una expresión cualquiera:

```js
print(tan(PI/2));
```

El cuerpo de una función _inline_ es una expresión cualquiera, que por supuesto puede incluir otras funciones y expresiones básicas, o cualquier combinación.

### Variables

En HULK es posible declarar variables usando la expresión `let-in`, que funciona de la siguiente forma:

```js
let x = PI/2 in print(tan(x));
```

En general, una expresión `let-in` consta de una o más declaraciones de variables, y un cuerpo, que puede ser cualquier expresión donde además se pueden utilizar las variables declaradas en el `let`. 
Fuera de una expresión `let-in` las variables dejan de existir.

Por ejemplo, con dos variables:

```js
let number = 42, text = "The meaning of life is" in print(text @ number);
```

Que es equivalente a:

```js
let number = 42 in (let text = "The meaning of life is" in (print(text @ number)));
```

El valor de retorno de una expresión `let-in` es el valor de retorno del cuerpo, por lo que es posible hacer:

```js
print(7 + (let x = 2 in x * x));
```

Que da como resultado `11`.

> La expresión `let-in` permite hacer mucho más, pero para este proyecto usted solo necesita implementar las funcionalidades anteriores.

### Condicionales

Las condiciones en HULK se implementan con la expresión `if-else`, que recibe una expresión booleana entre paréntesis, y dos expresiones para el cuerpo del `if` y el `else` respectivamente.
Siempre deben incluirse ambas partes:

```js
let a = 42 in if (a % 2 == 0) print("Even") else print("odd");
```

Como `if-else` es una expresión, se puede usar dentro de otra expresión (al estilo del operador ternario en C#):

```js
let a = 42 in print(if (a % 2 == 0) "even" else "odd");
```

> En HULK hay expresiones condicionales con más de una condición, usando `elif`, pero para este proyecto usted no tiene que implementarlas.

### Recursión

Dado que HULK tiene funciones compuestas, por definición tiene también soporte para recursión. Un ejemplo de una función recursiva en HULK es la siguiente:

```js
function fib(n) => if (n > 1) fib(n-1) + fib(n-2) else 1;
```

Usted debe garantizar que su implementación permite este tipo de definiciones recursivas.

## El intérprete

Su intérprete de HULK será una aplicación de consola, donde el usuario puede introducir una expresión de HULK, presionar ENTER, e immediatamente se verá el resultado de evaluar expresión (si lo hubiere)
Este es un ejemplo de una posible interacción:

```js
> let x = 42 in print(x);
42
> function fib(n) => if (n > 1) fib(n-1) + fib(n-2) else 1;
> fib(5)
13
> let x = 3 in fib(x+1);
8
> print(fib(6));
21
```

Cada línea que comienza con `>` representa una entrada del usuario, e immediatamente después se imprime el resultado de evaluar esa expresión, si lo hubiere.

> Note que cuando una expresión tiene valor de retorno (como en el caso de un llamado a una función), directamente se imprime el valor retornado, aunque no haya una instrucción `print`.

Todas las funciones declaradas anteriormente son visibles en cualquier expresión subsiguiente. Las funciones no pueden redefinirse.

### Errores

En HULK hay 3 tipos de errores que usted debe detectar. En caso de detectarse un error, el intérprete debe imprimir una línea indicando el error que sea lo más informativa posible.

#### Error léxico

Errores que se producen por la presencia de tokens inválidos. Por ejemplo:

```js
> let 14a = 5 in print(14a); 
! LEXICAL ERROR: `14a` is not valid token.
```

#### Error sintático

Errores que se producen por expresiones mal formadas como paréntesis no balanceados o expresiones incompletas. Por ejemplo:

```js
> let a = 5 in print(a;
! SYNTAX ERROR: Missing closing parenthesis after `a`.
> let a = 5 inn print(a);
! SYNTAX ERROR: Invalid token `inn` in `let-in` expression.
> let a = in print(a);
! SYNTAX ERROR: Missing expression in `let-in` after variable `a`.
```

### Error semántico

Errores que se producen por el uso incorrecto de los tipos y argumentos. Por ejemplo:

```js
> let a = "hello world" in print(a + 5);
! SEMANTIC ERROR: Operator `+` cannot be used between `string` and `number`.
> print(fib("hello world"));
! SEMANTIC ERROR: Function `fib` receives `number`, not `string`.
> print(fib(4,3));
! SEMANTIC ERROR: Function `fib` receives 1 argument(s), but 2 were given.
```

En caso de haber más de un error, usted debe detectar solamente **uno** de los errores.

## Detalles de implementación

Este proyecto es fundamentalmente para implementar una jerarquía de tipos conveniente que represente el lenguaje HULK (o al menos el subconjunto de lenguaje que usted debe soportar).
Usted deberá tener un conjunto de clases que representen los tipos de expresiones e instrucciones, así como otro conjunto de clases que representen los procesos y conceptos más importantes de su intérprete. 

Para ello, su solución debe consistir al menos en dos proyectos en el lenguaje C#, usando .NET Core 7 o superior:

- Una biblioteca de clases donde se implementa toda la lógica de parsing y evaluación del lenguaje HULK. En esta biblioteca no puede utilizar ninguna herramienta externa, más allá de la biblioteca estándar de .NET Core.
- Una aplicación de consola (o cualquier otra tecnología de visualización) donde se implementa la parte interactiva del intérprete.

En este proyecto **no es necesario** tener conocimientos avanzados de compilación, pues se ha definido un subconjunto de HULK que es posible resolver de manera directa. 
Sin embargo, usted puede utilizar cualquier algoritmo o técnica de compilación que desee, siempre que sea capaz de implementarla desde cero y explicar su funcionamiento.

Un consejo que sí podemos darle es estudiar el concepto de **parsing recursivo descendente** que le simplificará notablemente la tarea de interpretar el lenguaje HULK.
