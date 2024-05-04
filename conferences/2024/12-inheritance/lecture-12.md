# Herencia

Desde las primeras clases del curso se ha hecho énfasis en la importancia de reutilizar código:
- Código más corto, conciso y legible.
- Detección de errores: las instrucciones se localizan en un solo lugar.
- Consistencia del programa: tras actualizar no hay réplicas que modificar.

¿Con qué recursos de programación contamos para reutilizar código?
- Métodos (para reutilizar instrucciones comunes a funciones distintas).
- Herencia de clases (para reutilizar funcionalidades comunes a distintos tipos).

    - Por ejemplo, podemos querer modelar el personal involucrado en una institución (los estudiantes, profesores y administrativos de la facultad).
    Sabemos que todos son personas y por tanto hay características comunes que vamos a querer almacenar de todos (nombre, fecha de nacimiento, años vinculados a la facultad, etc.).
    Por otro lado, de los estudiantes podríamos querer almacenar adicionalmente el año académico en el que se encuentran y de los profesores y administrativos la fecha de contratación.
    Además, algunas funcionalidades pueden ser comunes a todas las personas (por ejemplo, una función que incremente en 1 los años que lleva una persona vinculada con la facultad) mientras que otras pueden ser exclusivas de los estudiantes (por ejemplo, una función que lo promueve al siguiente año académico).

    - Otro ejemplo sería querer modelar un sistema de cuentas bancarias.
    Pueden haber cuentas básicas con las funcionalidades mínimas, pero pueden haber cuentas con servicios premium como puede ser pago por crédito en lugar de solo débito.
        > Ver ejemplos de código en la carpeta `code`.

## Tipo de herencia

La herencia se puede aplicar con distintos propósitos.

- Para **ampliar**: añadir nuevas funcionalidades al tipo (hacer lo mismo y **más**).
- Para **especializar**: cambiar alguna de sus funcionalidades (hacer lo mismo, pero **mejor**, más especializado).

> **OJO:** el mayor atractivo de la herencia no es la reutilización de código!!! De hecho, muchas veces se puede hacer un uso indevido de la herencia con dicho propósito.
> > **Ejemplo:** clase `Person` hereda de `Math` para "saber" hacer Pow, Min, ect. _(ignorando el hecho de que no se puede heredar de clases `static`)_
>
> El polimorfismo y principio de sustitución (se verá proximamente) son las verdaderas estrellas de la herencia y algunas de las bases fundamentales de la programación orientada a objetos.

## Sintaxis

```csharp
class A {
    // ...
}

class B : A { // B hereda las funcionalidades de A
              // (y con ello sus datos).
    // ...
}
```

## Recordatorios sobre "tipos" en programación

Representan conceptos: agrupan los datos y funcionalidades que deben cumplir todos los objetos de ese tipo.
- Tipos de referencia: clases
    - Heredan de `System.Object`.
+ Tipos por valor: structs
    - Heredan de `System.ValueType` (que a su vez hereda de `System.Object`).
        - No se puede heredar explicitamente de `System.ValueType`.
        - Pero sí usar como tipo de parámetro, etc.

- Campos de clase y instancia.
- Clases `static` y no `static`.
    + CONSECUENCIAS????
        > `static` y la herencia
        > - No se puede heredar de clases `static`.
        >- Las clases `static` solo pueden heredar de `object`.