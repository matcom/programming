using System.Collections;
using System.Collections.Generic;
namespace Programacion
{
  class FirstIEnumerable<T> : IEnumerable<T>
  {
    class FirstEnumerator<T> : IEnumerator<T>
    {
      int count, cursor;
      T current;
      bool huboMoveNext;
      IEnumerator<T> internalEnumerator;
      public FirstEnumerator(int n, IEnumerable<T> items)
      {
        count = n; cursor = 0; huboMoveNext = false;
        internalEnumerator = items.GetEnumerator();
      }
      public bool MoveNext()
      {
        if ((cursor < count) && internalEnumerator.MoveNext())
        {
          current = internalEnumerator.Current;
          cursor++;
          return (huboMoveNext = true);
        }
        return false;
      }
      public T Current
      {
        get
        {
          if (huboMoveNext) return current;
          else throw new InvalidOperationException("No more elements");
        }
      }
      object IEnumerator.Current
      {
        get { return Current; }
      }
      public void Reset()
      {
      }
      public void Dispose()
      {
      }
    }
    IEnumerator<T> enumerator;
    public FirstIEnumerable(int n, IEnumerable<T> items)
    {
      enumerator = new FirstEnumerator<T>(n, items);
    }
    public IEnumerator<T> GetEnumerator()
    {
      return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return enumerator;
    }
  }
  internal class Program04
  {
    static public IEnumerable<T> First<T>(int n, IEnumerable<T> elems)
    {
      return new FirstIEnumerable<T>(n, elems);
    }

    static public IEnumerable<T> MagicFirst<T>(int n, IEnumerable<T> elems)
    {
      foreach (T x in elems)
      {
        n--;
        if (n>=0) yield return x; 
      }
    }

    static void Main(string[] args)
    {
      IEnumerable<int> nums = new int[]
      {
        1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20
      };
      Console.WriteLine("Los primeros 11 via basica");
      foreach (int k in First<int>(11, nums))
        Console.WriteLine(k);

      Console.WriteLine("\nLos primeros 15 via magic");
      foreach (int k in MagicFirst(15, nums))
        Console.WriteLine(k);
      //Comentar el codigo de las clases enumeradoras y probar
      //que Magic no depende de eso
    }
  }
}
