namespace MatCom.Sorting.Sorters;

public class MergeSort<T> : ISorter<T>
{
    public void Sort(ICollection<T> collection, IComparer<T> comparer)
    {
        T[] tmp = new T[collection.Count];
        Sort(comparer, collection, 0, collection.Count - 1, tmp);
    }

    private static void Sort(IComparer<T> comparer, ICollection<T> collection, int left, int right, T[] tmp)
    {
        if (left >= right)
            return;

        int mid = (left + right) / 2;

        Sort(comparer, collection, left, mid, tmp);
        Sort(comparer, collection, mid + 1, right, tmp);
        Merge(comparer, collection, left, mid, right, tmp);
    }

    private static void Merge(IComparer<T> comparer, ICollection<T> collection, int left, int mid, int right, T[] tmp)
    {
        int l = left;
        int r = mid + 1;
        int p = left;

        while (l <= mid && r <= right)
        {
            if (comparer.Compare(collection[l], collection[r]) <= 0)
                tmp[p++] = collection[l++];
            else
                tmp[p++] = collection[r++];
        }

        while (l <= mid)
            tmp[p++] = collection[l++];

        while (r <= right)
            tmp[p++] = collection[r++];

        for (int i = left; i <= right; i++)
            collection[i] = tmp[i];
    }
}

public class InsertionSort<T> : ISorter<T>
{
    public void Sort(ICollection<T> collection, IComparer<T> comparer)
    {
        for (int i = 1; i < collection.Count; i++)
        {
            T x = collection[i];
            int j = i - 1;

            while (j >= 0 && comparer.Compare(collection[j], x) > 0)
            {
                collection[j + 1] = collection[j];
                j = j - 1;
            }

            collection[j + 1] = x;
        }
    }
}
