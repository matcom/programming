# Conferencia #2

## Condicionales

¿Por qué las necesitamos?
- Hasta ahora toda línea de código que escribiamos en el programa se debía ejecutar, lo cual limita el número de problemas que podemos modelar.
- Hay instrucciones que podemos querer ejecutar solo cuando ciertas condiciones se cumplan.
- Es importante porque los datos del programa no se saben siempre al 100% en el momento en que se escribe, sino que muchos datos vienen del usuario.

### Condicionales IF-ELSE

> Definiremos bloques de instrucciones, los cuales serán ejecutados si se cumple determinada condición.

#### Sintaxis (condición simple)

```csharp
if (<condición>) { // si esta condición es verdadera (true)
    <instrucciones> // entonces se ejecutan estas instrucciones
}
```

#### Sintaxis (condición completa)

```csharp
if (<condición>) { // si esta condición es verdadera (true)
    <instrucciones> // entonces se ejecutan estas instrucciones
}
else { // sino no era verdadera aquella condición
    <instrucciones> // entonces se ejecutan estas instrucciones
}
```

#### Sintaxis (condición compleja)

```csharp
if (<condición>) { // si esta condición es verdadera (true)
    <instrucciones> // entonces se ejecutan estas instrucciones
}
else if (<condición>) { // si la condición anterior era falsa (false) pero esta nueva es verdadera (true)
    <instrucciones> // entonces se ejecutan estas instrucciones
}
...
else if (<condición>) { // si todas las condiciones anteriores eran falsas (false) pero esta nueva es verdadera (true)
    <instrucciones> // entonces se ejecutan estas instrucciones
}
else { // sino ninguna de las condiciones anteriores fue verdadera
    <instrucciones> // entonces se ejecutan estas instrucciones
}
```

> Si dentro de una condicional solo se va a poner una instrucción, entonces se pueden omitir las llaves.

#### Ejemplos

```csharp
Console.WriteLine("Hola, cómo te llamas?");
string name = Console.ReadLine();
if (name != "") {
    Console.WriteLine($"Un placer conocerte {name}!");
}
else {
    Console.WriteLine("Creo que no entendí bien tu nombre :')");
}
```

```csharp
int userInput = Console.ReadLine();
if(int.TryParse(userInput, out int number)) {
    Console.WriteLine("Escribiste un número entero.");
}
else {
    Console.WriteLine("No escribiste un entero.")
}
```

#### Las condiciones se pueden anidar

```csharp
int number = int.Parse(Console.ReadLine());
if(number > 0) {
    if(number % 2 == 0) {
        Console.WriteLine("Es un número positivo y par.");
    }
    else {
        Console.WriteLine("Es un número positivo e impar.");
    }
}
else {
    Console.WriteLine("El número no es positivo.");
}
```

### Operador Condicional Ternario

El operador condicional ternario constituye una expresión, por lo que el resultado de su evaluación puede guardarse en un variable o usarse directamente como parte de expresiones más complejas.

#### Sintaxis

```csharp
//              se obtiene el valor evaluar esta expresión
// si esta condición es cierta   |
// |                        _____|  sino, el valor de evaluar esta.
// |                       |                         |
<condición> ? <expresión-si-es-verdad> : <expresión-si-es-falso>
```

#### Ejemplo

```csharp
int number = int.Parse(Console.ReadLine());
string sign = (number > 0)? "positivo" : (number < 0)? "negativo" : "cero";
string parity = (number % 2 == 0) ? "par" : "impar";
Console.WriteLine($"El número es {sign} y además {parity}.");
```

## Ciclos

¿Por qué los necesitamos?
- _"Con un numero finito de instrucciones"_ queremos _"resolver problemas para entradas infinitamente grandes"_.
- Algunos procesamientos no se pueden hacer en un número constante de instrucciones para cualquier entrada (por ejemplo, saber si un número es primo).
- Los ciclos nos permitirán ejecutar un número finito de instrucciones una cantidad infinita de veces.

    > *INICIALMENTE* -> el código se ejecuta de forma secuencial
    y completamente lineal.
    >
    > *CONDICIONALES* -> permiten controlar qué instrucciones ejecutar según
    el contexto.  
    > La decisión es de *0* o *1*.
    >
    > *CICLOS* -> permite controlar cuántas veces ejecutar un
    grupo de instrucciones.  
    > La decisión es de *0* a *infinito*.  


### Sintaxis (While)

```csharp
while (<condición>){ // mientras se cumpla esta condición
    <instrucciones> // se ejecutan estas instrucciones
}
```
### Sintaxis (Do_While)
```csharp
do {
    <instrucciones> // se ejecutan estas instrucciones
} while(<condición>) // mientras se cumpla esta condición
```

>  Notar que en estos ciclos las instrucciones se ejecutan como mínimo una vez.

### Ejemplos

```csharp
Console.WriteLine("Hola, cómo te llamas?");
string name = Console.ReadLine();
while(name == "") {
    Console.WriteLine("Olvidaste escribir tu nombre. Intenta de nuevo!");
    name = Console.ReadLine();
}
Console.WriteLine($"Un placer conocerte {name}!");
```

```csharp
Console.WriteLine("Dime un número, para sumar desde cero hasta ese número.");
int number = int.Parse(Console.ReadLine());
int current = 1;
int sum = 0;
string expression = "0";
while (current <= number) {
    sum += current;
    expression += $" + {current}";
    current++;
}
Console.WriteLine($"{expression} = {sum}.");
```

### Modificar flujo del ciclo?
- **break:** aborta el ciclo más cercano que lo contiene
(*se continua en la linea siguiente al bloque del ciclo*).
- **continue:** aborta la iteración del ciclo más cercano que lo
contiene (*se pasa a la evaluación de la condición del ciclo*).
- **return:** aborta el método y con ello la ejecución del
ciclo (*se devuelve el control al invocador de la función*).

#### Ejemplos

```csharp
Console.WriteLine("Hola, cómo te llamas?");
while(true) {
    string name = Console.ReadLine();
    if (name != "")
        break;
    Console.WriteLine("Olvidaste escribir tu nombre. Intenta de nuevo!")
    name = Console.ReadLine();
}
Console.WriteLine($"Un placer conocerte {name}!");
```

```csharp
Console.WriteLine("Dime números, para contar los impares. Cuando me escribas algo que no sea un número haré el conteo.");
int count = 0;
while(true) {
    string line = Console.ReadLine();
    if(!int.TryParse(line, out int number))
        break;
    if(number % 2 == 0)
        continue;
    count++;
}
Console.WriteLine($"Escribiste {count} números impares!");
```