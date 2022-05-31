using MatCom.Sorting;
using MatCom.Sorting.Sorters;
using MatCom.Sorting.Collections;


class Program
{
    static void Main()
    {
        SortNumbers();
        SortDates();
    }

    static void SortNumbers()
    {
        Random r = new Random(42);
        int[] array = new int[20];

        for (int i = 0; i < array.Length; i++)
            array[i] = r.Next(100);

        MatCom.Sorting.ICollection<int> collection = new ArrayCollection<int>(array);
        System.Console.WriteLine("Before sorting:");
        Print(collection);

        ISorter<int> sorter = new MergeSort<int>();
        sorter.Sort(new ArrayCollection<int>(array), new IntComparer(ascending: false));

        System.Console.WriteLine("\nAfter sorting:");
        Print(collection);
    }

    static void SortDates()
    {
        Random r = new Random(42);
        DateTime[] array = new DateTime[20];

        for (int i = 0; i < array.Length; i++)
        {
            int year = r.Next(1980, 2050);
            int month = r.Next(1, 12);
            int day = r.Next(1, DateTime.DaysInMonth(year, month));

            array[i] = new DateTime(year, month, day);
        }

        MatCom.Sorting.ICollection<DateTime> collection = new ArrayCollection<DateTime>(array);
        System.Console.WriteLine("Before sorting:");
        Print(collection);

        ISorter<DateTime> sorter = new MergeSort<DateTime>();
        sorter.Sort(new ArrayCollection<DateTime>(array), new DefaultComparer<DateTime>());

        System.Console.WriteLine("\nAfter sorting:");
        Print(collection);
    }

    // 💠 Ejercicio de Conferencia: ¿cómo generalizar este código? 👆

    static void Print<T>(MatCom.Sorting.ICollection<T> collection)
    {
        System.Console.WriteLine($"{collection.Count} items\n");

        for (int i = 0; i < collection.Count; i++)
        {
            System.Console.WriteLine(collection[i]);
        }
    }
}

class IntComparer : MatCom.Sorting.IComparer<int>
{
    private readonly bool ascending;

    public IntComparer(bool ascending = true)
    {
        this.ascending = ascending;
    }

    public int Compare(int a, int b)
    {
        return this.ascending ? a - b : b - a;
    }
}

class DefaultComparer<T> : MatCom.Sorting.IComparer<T> where T : IComparable<T>
{
    public int Compare(T a, T b)
    {
        return a.CompareTo(b);
    }
}
