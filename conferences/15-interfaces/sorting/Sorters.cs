namespace MatCom.Sorting.Sorters
{
    public class MergeSort : ISorter
    {
        public void Sort(ICollection collection, IComparer comparer)
        {
            object[] tmp = new object[collection.Count];
            Sort(comparer, collection, 0, collection.Count - 1, tmp);
        }

        private static void Sort(IComparer comparer, ICollection collection, int left, int right, object[] tmp)
        {
            if (left >= right)
                return;

            int mid = (left + right) / 2;

            Sort(comparer, collection, left, mid, tmp);
            Sort(comparer, collection, mid + 1, right, tmp);
            Merge(comparer, collection, left, mid, right, tmp);
        }

        private static void Merge(IComparer comparer, ICollection collection, int left, int mid, int right, object[] tmp)
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

    public class InsertionSort : ISorter
    {
        public void Sort(ICollection collection, IComparer comparer)
        {
            for (int i = 1; i < collection.Count; i++)
            {
                object x = collection[i];
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
}