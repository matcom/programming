using System.Collections;

namespace DataStructures
{
    public class Lista<T>
    {
        T[] elements = new T[100];

        public void Add(T item)
        {
            if (Count == elements.Length)
                Grow();

            elements[Count++] = item;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0) return false;
            Array.Copy(elements, index + 1, elements, index, Count - index - 1);
            return true;
        }

        private int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (object.Equals(elements[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        private void Grow()
        {
            T[] aux = new T[(int)(elements.Length * 1.5)];
            Array.Copy(elements, aux, Count);
            elements = aux;
        }

        public int Count { get; private set; } = 0;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                return this.elements[index];
            }
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (object.Equals(this.elements[i], item))
                    return true;
            }

            return false;
        }
    }
}