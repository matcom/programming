# Conferencia #1

## Introducción

¿Programación? ¿Qué es un programa?
- Modelar situaciones de la vida real.
- Secuencia **finita** de instrucciones que partiendo de un conjunto de datos de entrada permite obtener una salida.
- Instrucciones deben poder ser ejecutadas por un dispositivo de cómputo.

Lenguajes de Programación
- Definen la sintaxis que debe tener el programa para especificar un grupo de instrucciones semánticas.
- El que usaremos en el curso: **C#**.
- Lenguaje fuertemente tipado y con tipado estático.

Paradigma de programación: Programación Orientada a Objetos (POO)
- Tipos vs Objetos
- Los **tipos** definen las características que deben cumplir todas las instancias / objetos de ese tipo.
- Los **objetos** representan instancias concretas de tipos, a los que se le asigna un espacio en memoria para almacenar toda la información relevante para su funcionamiento.

Tipos básicos
- `int`y `long`: Números enteros. Ejemplo: `100`, `-5`, `0`, `123`.
- `float` y `double`: Números reales. Ejemplo: `-1.2`, `3.14`, `0`.
- `char`: Caracteres, como letras, números, símbolos, etc. Ejemplo: `'a'`, `'5'`, `'@'`, `'\n'`.
- `string`: Secuencia de caracteres. Ejemplo: `"Juan Pablo"`, `"MATCOM"`, `"Programación2024"`, `"Hola!!!"`.
- `bool`: Valores booleanos (verdadero o falso). Ejemplo: `true` y `false`.

Sobre los objetos de cada tipo se pueden hacer diversas operaciones, como sumar números (`1 + 2 + 3`), concatenar cadenas de caracteres (`"MAT" + "COM"`), verificar si dos objetos son iguales (`1 + 2 == 3`), etc.
Parte del estudio individual de la asignatura consistirá en "jugar" con el lenguaje para descubrir qué operaciones están disponibles para cada tipo.

## Estructura de un programa en C#

Lo siguiente es la estructura general que tiene toda aplicación de consola en C#.
La aplicación se modela con el tipo `Program`.
La secuencia de instrucciones a ejecutar se encuentran dentro del _método_ `Main`.

```csharp
class Program { // Tipo que engloba la definición del programa que estamos creando.
	static void Main() { // Método principal que se ejecutará al iniciar el programa.
		Console.WriteLine("Hola!!!"); // Instrucciones a ejecutar.
	}
}
```

- Las llaves (`{` y `}`) se utilizan para marcar el inicio y fin de ciertas estructuras sintácticas.
- El `;` se utiliza para marcar el fin de cada instrucción.

## Variables

Permiten _ponerle un nombre a un objeto en memoria_, de manera que podamos acceder a él.
Como su nombre lo indica, el objeto al que _referencia_ la variable puede cambiar a lo largo del programa.

Sintaxis (declaración): `<tipo> <nombre>;`

```csharp
int age;
string lastName;
float height;
bool isMarried;
DateTime birthDay;
(int, int) location;
```

Sintaxis (inicialización): `<tipo> <nombre> = <expresión>;`
```csharp
int age = 30;
string lastName = "Ayala";
float height = 1.71;
bool isMarried = false;
DateTime birthDay = DateTime.Today;
(int, int) location = (-10, 50);
```

Sintaxis (asignación): `<nombre> = <expresión>;`

```csharp
age = 27 + 3;
lastName = "Aya" + "la";
height = 1.69 + 0.02;
isMarried = false && true;
birthDay = DateTime.Today.AddMinutes(60);
location = (-10 + 0, 60 - 5 * 2);
```

Una vez que la variable ha sido inicializada, ya puede usar en cualquier lugar del código para acceder al objeto que "almacena" / "referencia".

## Métodos

Permiten _definir "funciones"_. O sea, secuencias de instrucciones que dependan de una entrada y en función de ello produzca (o no) una salida.
Los métodos están asociados a un tipo, de manera tal que siempre se definen dentro de un tipo.

Sintaxis (declaración):
```csharp
class MyTools {
/*|               acceso                                            |*/
/*|   visibilidad | tipo de retorno                                 |*/
/*|   |       ____| |    nombre   tipo y nombre de los parámetros   |*/
/*|   |      |      |   |         |      |                          |*/
	public static int Successor(int number) { // aquí estamos definiendo una función f: int -> int.
		return number + 1;
	}
	public static void SayHello(string name) { // aquí estamos definiendo una función g: string -> ().
		Console.WriteLine($"Hola {name}. Que tengas un lindo día!");
	}
}
```

Sintaxis ("llamado" / "invocación")
- `<tipo>.<método>(<expresión1>, ..., <expresiónN>);`
	```csharp
	int next = MyTools.Successor(5);
	MyTools.SayHello("Juan Pablo");
	```

- El `<tipo>` puede omitirse si el llamado se está haciendo dentro de un método del propio tipo.
	```csharp
	class MyTools {
		public static int Successor(int number) // ...
		public static void SayHello(string name) // ...
		public static void SayHelloAndCongratForNextBirthday(string name, int currentAge) {
			SayHello(name);
			int nextAge = Successor(currentAge);
			Console.WriteLine($"Felicidades, proximamente cumplirás {nextAge} años!");
		}
	}
	```


Inicialmente el uso fundamental que le daremos a los métodos será el de _reutilizar código_.

## Interactuar con el usuario

- Para mostrarle información al usuario: `Console.WriteLine(<expresión>);`.
- Para pedirle información al usuario: `string msg = Console.ReadLine();`.