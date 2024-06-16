using System;
using System.Collections.Generic;
using System.Collections;

namespace Programacion
{
    #region ARRAYLIST
    class ArrayList<T> : IList<T>
    {
        T[] array;
        int increaseLength;
        public ArrayList(int increaseLength = 1000)
        {
            array = new T[increaseLength];
            this.increaseLength = increaseLength;
            Count = 0;
        }

        #region MÉTODOS POR SER UN ICollection
        public int Count { get; private set; }
        public void Add(T x)
        {
            if (Count < array.Length)
                array[Count++] = x;
            else
            {
                //Crear un array de mayor longitud para poder poner el nuevo elemento
                T[] values = new T[array.Length + increaseLength];
                System.Array.Copy(array, 0, values, 0, array.Length);
                values[Count++] = x;
                array = values;
            }
        }
        public bool Contains(T x)
        {
            for (int k = 0; k < Count; k++)
                if (array[k].Equals(x)) return true;
            return false;
        }
        #endregion

        #region CP IMPLEMENTAR LOS OTROS MÉTODOS DE ICollection
        public bool IsReadOnly
        {
            get { throw new Exception("Implementar IsReadOnly en CP"); }
        }
        public void Clear()
        {
            throw new Exception("Implementar Clear en CP");
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new Exception("Implementar CopyTo en CP");
        }
        public bool Remove(T item)
        {
            throw new Exception("Implementar Remove en CP");
        }
        #endregion

        #region METODOS PROPIOS DE IList
        //...
        public T this[int i]
        {
            get
            {
                if (i < Count) return array[i];
                else throw new Exception("Index out of range");
            }
            set
            {
                if (i < Count) array[i] = value;
                else throw new Exception("Index out of range");
            }
        }
        #endregion

        #region CP IMPLEMENTAR LOS OTROS MÉTODOS DE IList<T>
        public int IndexOf(T item)
        {
            throw new Exception("Implementar IndexOf en CP");
        }
        public void Insert(int index, T item)
        {
            throw new Exception("Implementar Insert en CP");
        }
        public void RemoveAt(int index)
        {
            throw new Exception("Implementar RemoveAt en CP");
        }
        #endregion

        #region IMPLEMENTACIÓN DE IEnumerable USANDO UN IEnumerator
        //public IEnumerator<T> GetEnumerator()
        //{
        //  return new ArrayListEnumerator<T>(this);
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //  return GetEnumerator();
        //}

        //class ArrayListEnumerator<T> : IEnumerator<T>
        //{
        //  bool seHizoMoveNext;
        //  int cursor;
        //  int total;
        //  //T current;
        //  ArrayList<T> originalList;
        //  public ArrayListEnumerator(ArrayList<T> list)
        //  {
        //    seHizoMoveNext = false;
        //    originalList = list;
        //    cursor = 0;
        //    total = originalList.Count;
        //  }
        //  public bool MoveNext()
        //  {
        //    if (seHizoMoveNext)
        //    {
        //      if (++cursor < total)
        //      {
        //        return true;
        //      }
        //      else return false;
        //    }
        //    seHizoMoveNext = true;
        //    return (cursor < total);
        //  }
        //  public T Current
        //  {
        //    get
        //    {
        //      if (!seHizoMoveNext) throw new Exception("Hay que hacer moveNext");
        //      if (cursor < total) return originalList.array[cursor];
        //      else throw new Exception("No hay más elementos");
        //    }
        //  }
        //  object IEnumerator.Current
        //  {
        //    get
        //    {
        //      return Current;
        //    }
        //  }
        //  public void Reset()
        //  {
        //    //CP IMPLEMENTAME !!!!
        //  }
        //  public void Dispose()
        //  {
        //    //POR AHORA NO ME IMPLEMENTES
        //  }
        //} //ListEnumerator
        #endregion

        #region IMPLEMENTACION MÁS SIMPLE USANDO YIELD (no ver por ahora)
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    #endregion

    #region EJERCICIOS PARA CP IMPLEMENTAR MÉTODOS QUE OPEREN SOBRE IEnumerables y DEVUELVAN IEnumerables
    static class Enumerable
    {

        public static IEnumerable<T> Union<T>(IEnumerable<T> enum1, IEnumerable<T> enum2)
        {

            throw new InvalidOperationException("Union no implementada aún");

        }

        public static IEnumerable<T> Take<T>(IEnumerable<T> enum1, int n)
        {

            throw new InvalidOperationException("Take no implementado aún");

        }
    }
    #endregion

    class ProgramArrayList
    {
        static void Main(string[] args)
        {
            ArrayList<int> ints = new ArrayList<int>(5);
            for (int i = 0; i < 20; i++) ints.Add(i * 2);
            ints[10] = 1000;
            Console.WriteLine("Hay {0} items", ints.Count);
            Console.WriteLine("Iterando con for ...");
            for (int i = 0; i < ints.Count; i++) Console.WriteLine(ints[i]);
            Console.WriteLine("Iterando con foreach ...");
            foreach (int k in ints) Console.WriteLine(k);
            Console.WriteLine(ints.Contains(1000));
        }
    }
}
