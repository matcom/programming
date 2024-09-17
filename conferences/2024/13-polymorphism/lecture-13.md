# Polimorfismo

Los objetivos de esta conferencia serán:

1. Identificar que es atractivo poder cambiar objetos de un tipo por objetos de sus subtipos.
    - Facilita la modelación.
    - Mayor desacoplamiento del código (permite cambiar las partes siempre que resuelvan el mismo problema).
2. Entender a nivel de lenguaje de programación como se evidencia eso, y cómo es posible.
    - Restringir el tipo de una variable pero aceptar instancias de cualquiera de sus subtipos.
        + ¿Pero como quién se comporta? ¿como su tipo o como los subtipos?
    - ¿Cómo se organiza la memoria para que sea posible el polimorfismo (un objeto "haciendose pasar" por distintos tipos)?
    - ¿Por qué es que caben en las variables con independencia del tipo en concreto?
        + Boxing y unboxing para tipos por valor.
3. ¿Cómo se aplica todo esto al problema visto en conferencia anterior (cuenta de crédito)?
    - Identificar la limitación de la herencia sin `virtual`-`override`.
4. Inferir el comportamiento de las clases estáticas con la herencia.

## Principio de sustitución (_de Liskov_)

> Cada clase que hereda de otra puede usarse como su padre sin necesidad de conocer las diferencias entre ellas.

o dicho de otra forma:

> Si se espera trabajar con un objeto de un tipo `T`, entonces cualquier objeto de un tipo heredero de `T` debería poder ser utilizado en su lugar de forma transparente.

**Es principal razón por la que usar herencia!!!** Constituye un salto inmenso en lo que refiere a expresividad del código (en el sentido de las situaciones que puede modelar).
La forma natural en la que se clasifican objetos en tipos (en la vida cotidiana) es por características que tienen en común, pero que se resuelven de forma diferente. Se parte de conceptos básicos que se van especializando.

## Memoria y herencia

- `S` subtipo de `T` entonces:
    1. `S s = new T();` es válido? **R./ No**
    2. `T t = new S();` es válido? **R./ Sí**

> Les propongo buscar la respuesta a las siguientes dos preguntas para entender por qué funciona de la forma anterior.

### ¿Por qué tiene sentido?

- Desde el punto de vista semántico (_significado o propósito_) `S` puede hacer lo mismo que `T` y más. Por tanto tiene sentido que un objeto de tipo `S` pueda _"hacerse pasar"_ por uno de tipo `T`.
- Pero al invocarse una función de la variable, debe comportarse como `T` o como `S`. **(¿Se comporta como el tipo de la variable o como el subtipo? R./ Como el subtipo)**

    > **YA VIMOS EL EJEMPLO DE LOS PERROS Y LOS PERROS CON PROBLEMAS VOCALES ;-)** 
    >
    > - La caja dice que puede contener perros.
    > - Tanto los perro normales como los perros con problemas vocales pueden ir en la caja.
    > - Pero qué tal si ordeno al perro dentro de la caja que ladre?
    >     + Solo porque esté dicho que lo que hay es un perro, sin más información, ¿eso implicaría que mi perro con problemas vocales va milagrosamente a poder ladrar?
    >       > R./ Por supuesto que no.

### ¿Por qué es posible?

- Si `S` es potencialmente más grande que `T`, entonces por qué la alternativa (2) es la válida?
- ¿Cómo es posible organizar/reservar la memoria para que un objeto de tipo `S` pueda hacerse pasar por otro de tipo `T`?
- ¿Qué pasa con las variables? Lo que puede almacenar un objeto de tipo `T` puede almacenar también uno de tipo `S`.
    + Boxing y unboxing para tipos por valor.

### Tipo estático vs tipo dinámico

- Tipo estático: el tipo que garantiza el compilador.
- Tipos dinámico: el tipo real que tendrá el objeto en runtime.

C# nos da dos mecanismos para forzarlo a utilizar un tipo estático.
- Casteo: `(<StaticType>) <expression>`. Si en runtime el tipo dinámico del objeto al que evaluó la expresión no es descendiente del tipo `<StaticType>` que se haya indica, se lanzará una excepción.
- Instrucción `as`: `<expression> as <StaticType>`. Si en runtime el tipo dinámico del objeto al que evaluó la expresión no es descendiente del tipo `<StaticType>` que se haya indica, se devolverá `null`.

> **OJOOOOOOOOOOO**: en ninguno de los dos casos el tipo dinámico del objeto cambia.

## Herencia por especialización

Volviendo al problema de herencia para **especializar** (cambiar alguna de sus funcionalidades, o sea, hacer lo mismo, pero **mejor**, más especializado) veremos una nueva sintaxis (`virtual` y `override`).

```csharp
class Perro {
    //      ___ EN EL MOMENTO EN QUE SE DEFINE SE ABRE A SOBREESCRITURA.
    //     |
    public virtual void Ladrar() {
        Console.WriteLine("Jau jau :-)");
    }
}

class PerroConProblemasVocales : Perro {
    //      ___ EL DESCENDIENTE ES LIBRE DE SOBREESCRIBIR EL COMPORTAMIENTO.
    //     |
    public override void Ladrar() {
        Console.WriteLine("... :-(");
    }
}

class PerroConFalsosProblemasVocales : Perro {
    //      ___ EL DESCENDIENTE DA UNA NUEVA IMPLEMENTACIÓN PARA EL TIPO ESTÁTICO.
    //     |
    public new void Ladrar() {
        Console.WriteLine("... :-(");
    }
}
```

### Palabras claves: `virtual` vs `new`.
- `virtual` es el comportamiento esperado, sin embargo no es el por defecto.
- `new` es el comportamiento por defecto, sin embargo dado que no es lo esperado. Existe para asegurar que esa es la intención.

> La semana próxima veremos otra palabra clave que nos será muy útil en todo este contexto de herencia para especialización. **ESTA SEMANA ES LA INTRA DE RECURSIVIDAD**

## Herencia y `static`

No tiene sentido heredar de una clase estática porque no en esa herencia no se aprovecharía el principio de sustitución (sería simplemente por ampliación), puesto que al estar marcada como estática todos sus campos son estáticos (de ahí que tampoco se permita crear instancias).

Por otro lado, sí tiene sentido heredar los campos estáticos de clases no estáticas; la clase es instanciable al no estar marcada como estática, por lo que tiene sentido aplicar el principio de sustitución, y se heredan los campos estáticos puesto que estos pueden ser llamados de forma transparente desde los métodos de instancia.

> **OJO:** Los llamados/accesos a métodos/variables de clase heredados es el mismo entre la clase heredera como en la clase heredada.

Las clases estáticas no pueden heredar de otras porque:
1. No pueden heredar de una clase no estática porque potencialmente podrían tener campos de instancia.
2. No pueden heredar de clases estáticas puesto que ya vimos que no tiene sentido que ninguna clase herede de ellas.

Los métodos estáticos no se pueden marcar como virtual. ¿Por qué?