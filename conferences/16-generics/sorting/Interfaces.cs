namespace MatCom.Sorting;

public interface ICollection<T>
{
    int Count { get; }

    T this[int index] { get; set; }
}

public interface ISorter<T>
{
    void Sort(ICollection<T> collection, IComparer<T> comparer);
}

public interface IComparer<T>
{
    int Compare(T a, T b);
}
