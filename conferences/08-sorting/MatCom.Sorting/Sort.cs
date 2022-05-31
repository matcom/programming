namespace MatCom.Sorting;

public static class Sort
{
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

    public static void MinSort(int[] array)
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
            int x = array[i];
            int j = i - 1;

            while (j >= 0 && array[j] > x)
            {
                array[j + 1] = array[j];
                j = j - 1;
            }

            array[j + 1] = x;
        }
    }

    #region Black magic

    public static void MergeSort(int[] array)
    {
        int[] tmp = new int[array.Length];
        MergeSort(array, 0, array.Length - 1, tmp);
    }

    private static void MergeSort(int[] array, int left, int right, int[] tmp)
    {
        if (left >= right)
            return;

        int mid = (left + right) / 2;

        MergeSort(array, left, mid, tmp);
        MergeSort(array, mid + 1, right, tmp);
        Merge(array, left, mid, right, tmp);
    }

    private static void Merge(int[] array, int left, int mid, int right, int[] tmp)
    {
        int l = left;
        int r = mid + 1;
        int p = left;

        while (l <= mid && r <= right)
        {
            if (array[l] <= array[r])
                tmp[p++] = array[l++];
            else
                tmp[p++] = array[r++];
        }

        while (l <= mid)
            tmp[p++] = array[l++];

        while (r <= right)
            tmp[p++] = array[r++];

        for (int i = left; i <= right; i++)
            array[i] = tmp[i];
    }

    #endregion
}
