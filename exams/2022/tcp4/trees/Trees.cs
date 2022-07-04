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

        // Borre esta excepción y ponga su nombre como string, e.j.
        // Nombre => "Fulano Pérez Pérez";
        public static string Nombre => throw new NotImplementedException();

        // Borre esta excepción y ponga su grupo como string, e.j.
        // Grupo => "C2XX";
        public static string Grupo => throw new NotImplementedException();
    }

}