using MatCom.Sorting;
using MatCom.Sorting.Sorters;
using MatCom.Sorting.Collections;


class Program
{
    static void Main()
    {
        SortNumbers();
        // SortDates();
    }

    static void SortNumbers()
    {
        Random r = new Random(42);
        object[] array = new object[20];

        for (int i = 0; i < array.Length; i++)
            array[i] = r.Next(100);

        ICollection collection = new ArrayCollection(array);
        System.Console.WriteLine("Before sorting:");
        Print(collection);

        ISorter sorter = new MergeSort();
        sorter.Sort(new ArrayCollection(array), new IntComparer(ascending: false));

        System.Console.WriteLine("\nAfter sorting:");
        Print(collection);
    }

    static void SortDates()
    {
        Random r = new Random(42);
        object[] array = new object[20];

        for (int i = 0; i < array.Length; i++)
        {
            int year = r.Next(1980, 2050);
            int month = r.Next(1, 12);
            int day = r.Next(1, DateTime.DaysInMonth(year, month));

            array[i] = new DateTime(year, month, day);
        }

        ICollection collection = new ArrayCollection(array);
        System.Console.WriteLine("Before sorting:");
        Print(collection);

        ISorter sorter = new MergeSort();
        sorter.Sort(new ArrayCollection(array), new DefaultComparer());

        System.Console.WriteLine("\nAfter sorting:");
        Print(collection);
    }

    static void Print(ICollection collection)
    {
        System.Console.WriteLine($"{collection.Count} items\n");

        for (int i = 0; i < collection.Count; i++)
        {
            System.Console.WriteLine(collection[i]);
        }
    }
}

class IntComparer : IComparer
{
    private readonly bool ascending;

    public IntComparer(bool ascending = true)
    {
        this.ascending = ascending;
    }

    public int Compare(object a, object b)
    {
        if (a is int ai && b is int bi)
        {
            return this.ascending ? ai - bi : bi - ai;
        }

        // Ugly!!!
        throw new ArgumentException("Can only compare integer!");
    }
}

class DefaultComparer : IComparer
{
    public int Compare(object a, object b)
    {
        if (a is IComparable ai)
        {
            return ai.CompareTo(b);
        }

        throw new ArgumentException("Can only compare IComparable instances!");
    }
}
