namespace MatCom.Logic;

public class Set
{
    private int[] elements;

    // Constructor privado que solo puedo llamar desde esta clase,
    // por lo tanto según el uso, a veces puedo confiar en que los
    // elementos no estén repetidos, y me ahorro tener que hacer esa
    // verificación 2 veces.
    private Set(int[] elements, bool safe)
    {
        if (!safe)
        {
            elements = Unique(elements);
        }

        this.elements = elements;
    }

    // Constructor público que llamarán los usuarios de la biblioteca,
    // por definición no puedo confiar en la entrada.
    public Set(params int[] elements)
    {
        this.elements = Unique(elements);
    }

    public int Size
    {
        get { return this.elements.Length; }
    }

    public bool Contains(int x)
    {
        for (int i = 0; i < this.elements.Length; i++)
        {
            if (this.elements[i] == x)
            {
                return true;
            }
        }

        return false;
    }

    public Set Union(Set other)
    {
        int[] union = new int[this.Size + other.Size];
        int total = 0;

        // Como cada conjunto no tiene elementos repetidos,
        // podemos poner con total seguridad los del conjunto actual
        for (int i = 0; i < this.Size; i++)
        {
            union[total++] = this.elements[i];
        }

        // Ahora ponemos los del otro conjunto, que algunos estarán repetidos
        for (int i = 0; i < other.Size; i++)
        {
            union[total++] = other.elements[i];
        }

        return new Set(union);
    }

    public Set Intersection(Set other)
    {
        int[] intersection = new int[Math.Min(this.Size, other.Size)];
        int total = 0;

        // Por cada elemento del conjunto actual, lo ponemos si y solo si
        // también está en el otro conjunto
        // Usaremos un ciclo foreach porque no nos interesa el índice
        foreach (int x in this.elements)
        {
            if (other.Contains(x))
            {
                intersection[total++] = x;
            }
        }

        // Finalmente devolvemos un Set nuevo pero solo con la cantidad
        // de elementos necesarios, usando el constructor privado
        return new Set(Resize(intersection, total), true);
    }

    #region Métodos auxiliares

    private static int[] Unique(int[] elements)
    {
        int[] temp = new int[elements.Length];
        int total = 0;

        // Poner en temp solamente aquellos elementos nuevos
        for (int i = 0; i < elements.Length; i++)
        {
            if (!Find(elements[i], temp, total))
            {
                temp[total++] = elements[i];
            }
        }

        return Resize(temp, total);
    }

    private static int[] Resize(int[] array, int newSize)
    {
        // Crear un nuevo array con la cantidad justa de elementos
        int[] result = new int[newSize];

        for (int i = 0; i < newSize; i++)
        {
            result[i] = array[i];
        }

        return result;
    }

    private static bool Find(int x, int[] array, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (array[i] == x)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}

// EJERCICIOS

// 1) Adicione el método de instancia `Set Difference(Set other)` que devuelve el conjunto diferencia entre el
//    conjunto actual y `other`, e implemente los tests que considere necesarios para evaluarlo.

// 2) Adicione un método de instancia `bool Equivalent(Set other)` que devuelve `true` si y solo si ambos
//    conjuntos tienen exactamente los mismos elementos, e implemente los tests necesarios.
//    Recuerde que los conjuntos no tienen orden intrínseco.

// 3) Adicione un método de instancia `string PrettyPrint()` que devuelve un `string` con los elementos del
//    conjunto, en la notación usual de lógica, e.j. `{ 1, 3, 2, 9, 0 }`. Los elementos pueden estar en cualquier orden.
//    Hint: Utilice la clase `StringBuilder` para construir un `string` de forma más eficiente que con el operador `+`.

// 4) Modifique el método `Union` para que no necesite llamar al constructor público, sino que garantice directamente
//    que los elementos no están duplicados y por tanto se pueda llamar al constructor privado.
