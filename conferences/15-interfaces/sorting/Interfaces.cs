namespace MatCom.Sorting
{
    public interface ICollection
    {
        int Count { get; }

        object this[int index] { get; set; }
    }

    public interface ISorter
    {
        void Sort(ICollection collection, IComparer comparer);
    }

    public interface IComparer
    {
        int Compare(object a, object b);
    }
}