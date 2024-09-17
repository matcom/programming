namespace MatCom.Sorting
{
    public static class Sort
    {
        public static int BinarySearch(int[] items, int x)
        {
            int l = 0;
            int r = items.Length - 1;

            while (l <= r)
            {
                int m = (l + r) / 2;

                if (items[m] < x)
                    l = m + 1;
                else if (items[m] > x)
                    r = m - 1;
                else
                    return m;
            }

            return -1;
        }


        private static void Swap(int[] array, int a, int b)
        {
            int temp = array[a];
            array[a] = array[b];
            array[b] = temp;
        }

        public static bool IsSorted(int[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
                if (array[i] > array[i + 1])
                    return false;

            return true;
        }

        public static void BubbleSort(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
                for (int j = 0; j < array.Length - 1; j++)
                    if (array[j] > array[j + 1])
                        Swap(array, j, j + 1);
        }

        public static void SelectionSort(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int min = i;

                for (int j = i + 1; j < array.Length; j++)
                    if (array[j] < array[min])
                        min = j;

                Swap(array, min, i);
            }
        }

        public static void InsertionSort(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                int j = i - 1;

                while (j >= 0 && array[j] > array[j + 1])
                {
                    Swap(array, j, j + 1);
                    j = j - 1;
                }
            }
        }
    }
}

// EJERCICIOS

// 1. ¿Qué sucede con BinarySeach cuando existen valores repetidos? Modifique el algoritmo para que en esos casos:
//    a) Devuelva el índice del valor más a la izquierda.
//    b) Devuelva el índice del valor más a la derecha.

// 2. En BubbleSort, si una iteración del ciclo más interno no hace ningún intercambio,
//    se puede garantizar que el array está ordenado (¿Por qué?).
//    Modifique el algoritmo para que termine en ese caso.

// 2.1) En el mismo algoritmo, note que no siempre es necesario siempre llevar el ciclo más interno
//      hasta el final (¿Por qué?). Modifique el algoritmo en consecuencia.

// 3. Modifique el método InsertionSort para que haga la menor cantidad de asignaciones posibles.
//    Hint: En el ciclo más interno, note que Swap(j,j+1), siempre se intercambia con el mismo elemento.

// 4. Bonus track: Modifique búsqueda binaria de forma que no necesite usar ciclos :)
