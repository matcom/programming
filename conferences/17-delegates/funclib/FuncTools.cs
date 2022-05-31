namespace funclib
{
    public delegate TOut Mapping<TIn, TOut>(TIn item);

    public delegate bool Predicate<T>(T item);

    public delegate TResult Reductor<T, TResult>(T current, TResult accum);

    public static class FuncTools
    {
        public static T[] Filter<T>(T[] items, Predicate<T> predicate)
        {
            T[] result = new T[items.Length];
            int p = 0;

            foreach (T item in items)
            {
                if (predicate(item))
                {
                    result[p++] = item;
                }
            }

            Array.Resize(ref result, p);
            return result;
        }

        public static TOut[] Map<TIn, TOut>(TIn[] items, Mapping<TIn, TOut> mapping)
        {
            TOut[] result = new TOut[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                result[i] = mapping(items[i]);
            }

            return result;
        }

        public static TResult Reduce<T, TResult>(T[] items, Reductor<T, TResult> reductor, TResult seed = default(TResult))
        {
            TResult result = seed;

            foreach (var item in items)
            {
                result = reductor(item, result);
            }

            return result;
        }
    }
}
