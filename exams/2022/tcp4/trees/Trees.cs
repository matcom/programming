namespace MatCom.Exam
{
    public interface ITree<T>
    {
        T Value { get; }
        IEnumerable<ITree<T>> Children { get; }
    }

    public delegate bool Predicate<T>(T item);
}