namespace MatCom.Programming
{
    class Program
    {
        static void Main()
        {
            int[] to_sort;
            int[] numbers = new[] { 10, 9, -3, 8, 7, -1, 5, 4, 3, 2, 1 };

            Console.WriteLine(Pow(2, 10));

            to_sort = (int[])numbers.Clone();
            MergeSort(to_sort);
            Console.WriteLine("[{0}]", string.Join(", ", to_sort));

            to_sort = (int[])numbers.Clone();
            QuickSort(to_sort);
            Console.WriteLine("[{0}]", string.Join(", ", to_sort));

            Console.WriteLine(BinarySearch(to_sort, 4));
        }


        // Pow
        static int Pow(int n, int p)
        {
            if (p == 0)
                return 1;

            int half = Pow(n, p / 2);
            return half * half * (p % 2 == 0 ? 1 : n);
        }


        // BinarySearch
        public static int BinarySearch(int[] array, int target)
        {
            return BinarySearch(array, target, 0, array.Length - 1);
        }

        private static int BinarySearch(int[] array, int target, int left, int right)
        {
            if (left > right)
                return -1;

            int mid = left + (right - left) / 2;

            if (array[mid] == target)
                return mid;
            else if (array[mid] < target)
                return BinarySearch(array, target, mid + 1, right);
            else
                return BinarySearch(array, target, left, mid - 1);
        }

        // MergeSort
        static void MergeSort(int[] array)
        {
            MergeSort(array, 0, array.Length - 1, new int[array.Length]);
        }

        static void MergeSort(int[] array, int inicio, int fin, int[] aux)
        {
            if (inicio == fin)
                return;

            int medio = inicio + (fin - inicio) / 2;
            MergeSort(array, inicio, medio, aux);
            MergeSort(array, medio + 1, fin, aux);
            Merge(array, aux, inicio, medio, fin);
        }

        static void Merge(int[] array, int[] aux, int inicioA, int finA, int finB)
        {
            int i = inicioA;
            int j = finA + 1;
            int pos = inicioA;

            while (i <= finA && j <= finB)
            {
                if (array[i] <= array[j])
                    aux[pos++] = array[i++];
                else
                    aux[pos++] = array[j++];
            }
            while (i <= finA)
            {
                aux[pos++] = array[i++];
            }
            while (j <= finB)
            {
                aux[pos++] = array[j++];
            }
            Array.Copy(aux, inicioA, array, inicioA, finB - inicioA + 1);
        }

        // QuickSort
        static void QuickSort(int[] array)
        {
            QuickSort(array, 0, array.Length - 1);
        }

        static void QuickSort(int[] array, int inicio, int fin)
        {
            if (fin <= inicio)
                return;

            int pivotIndex = (inicio + fin) / 2;
            pivotIndex = ArrangePivot(array, inicio, fin, pivotIndex);
            QuickSort(array, inicio, pivotIndex - 1);
            QuickSort(array, pivotIndex + 1, fin);
        }

        static int ArrangePivot(int[] array, int inicio, int fin, int pivotIndex)
        {
            int i = inicio;
            int j = fin;

            while (true)
            {
                if (array[i] <= array[pivotIndex] && i != pivotIndex)
                    i++;
                if (array[j] >= array[pivotIndex] && j != pivotIndex)
                    j--;
                if (i == j)
                    break;
                if (array[i] >= array[pivotIndex] && array[j] <= array[pivotIndex])
                {
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    if (i == pivotIndex)
                    {
                        pivotIndex = j;
                        i++;
                    }
                    else if (j == pivotIndex)
                    {
                        pivotIndex = i;
                        j--;
                    }
                    else
                    {
                        i++;
                        j--;
                    }
                }
            }
            return i; // ~ RETURN J
        }
    }
}
