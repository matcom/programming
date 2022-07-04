namespace MatCom.Exam
{
    public interface ITree<T>
    {
        T Value { get; }
        IEnumerable<ITree<T>> Children { get; }
    }

    public delegate bool Predicate<T>(T item);

    public static class Exam
    {
        public static IEnumerable<ITree<T>> MaximalSubtreesWhere<T>(
            ITree<T> tree, Predicate<T> predicate)
        {
            // ponga su código aquí
            throw new NotImplementedException();
        }

        public static string Nombre => "Nombre Apellido Apellido";

        public static string Grupo => "C2XX";
    }

}