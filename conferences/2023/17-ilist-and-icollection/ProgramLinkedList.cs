using System;
using System.Collections.Generic;
using System.Collections;

namespace Programacion
{
  #region LINKEDLIST
  class Linkable<T>
  {
    public T valor;
    public Linkable<T> prox;
    public Linkable(T valor, Linkable<T> prox)
    {
      this.valor = valor;
      this.prox = prox;
    }
  }
  class LinkedList<T> : IList<T>
  {
    Linkable<T> primero;
    Linkable<T> ultimo;
    public LinkedList()
    {
      Count = 0;
    }

    #region MÉTODOS POR SER IList UN ICollection
    public int Count { get; private set; }
    public void Add(T x)
    {
      Linkable<T> nodo = new Linkable<T>(x, null);
      if (Count == 0)
      {
        primero = nodo;
        ultimo = nodo;
        Count = 1;
      }
      else
      {
        ultimo.prox = nodo;
        ultimo = nodo;
        Count++;
      }
    }
    public bool Contains(T x)
    {
      Linkable<T> cursor = primero;
      for (int i = 0; i < Count; i++)
        if (cursor.valor.Equals(x)) return true;
        else
          cursor = cursor.prox;
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
    public T this[int i]
    {
      get
      {
        Linkable<T> cursor;
        if (i >= 0 && i < Count)
        {
          cursor = primero;
          for (int k = 0; k < i; k++)
            cursor = cursor.prox;
          return cursor.valor;
        }
        else throw new Exception("Index out of range");
      }
      set
      {
        Linkable<T> cursor;
        if (i >= 0 && i < Count)
        {
          cursor = primero;
          for (int k = 0; k < i; k++)
            cursor = cursor.prox;
          cursor.valor = value;
        }
        else throw new Exception("Indice fuera de rango");
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
    public IEnumerator<T> GetEnumerator()
    {
      return new LinkedListEnumerator<T>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    class LinkedListEnumerator<T> : IEnumerator<T>
    {
      bool seHizoMoveNext;
      Linkable<T> cursor;
      int indiceCursor;
      int total;
      public LinkedListEnumerator(LinkedList<T> list)
      {
        cursor = list.primero; total = list.Count;
      }
      public bool MoveNext()
      {
        if (seHizoMoveNext)
        {
          indiceCursor++;
          cursor = cursor.prox;
          return (indiceCursor < total);
        }
        seHizoMoveNext = true;
        return (indiceCursor < total);
      }
      public T Current
      {
        get
        {
          if (!seHizoMoveNext) throw new Exception("Hay que hacer moveNext");
          if (indiceCursor < total) return cursor.valor;
          else throw new Exception("No hay más elementos");
        }
      }
      object IEnumerator.Current
      {
        get
        {
          return Current;
        }
      }
      public void Reset()
      {
        //CP IMPLEMENTAME !!!!
      }
      public void Dispose()
      {
        //POR AHORA NO ME IMPLEMENTES
      }
    } //LinkedListEnumerator
    #endregion

    #region IMPLEMENTACION MÁS SIMPLE USANDO YIELD
    //public IEnumerator<T> GetEnumerator()
    //{
    //  if (Count > 0)
    //  {
    //    Linkable<T> cursor = primero;
    //    for (int i = 0; i < Count; i++)
    //    {
    //      yield return cursor.valor;
    //      cursor = cursor.prox;
    //    }
    //  }
    //}
    //IEnumerator IEnumerable.GetEnumerator()
    //{
    //  return GetEnumerator();
    //}
    #endregion
  }
  #endregion LINKEDLIST

  #region EJERCICIOS PARA CP IMPLEMENTAR MÉTODOS QUE OPEREN SOBRE IEnumerables y DEVUELVAN IEnumerables
  static class Enumerable
  {
    public static IEnumerable<T> Union<T>(IEnumerable<T> items1, IEnumerable<T> items2)
    {
      throw new InvalidOperationException("Union no implementada aún");
    }

    public static IEnumerable<T> Take<T>(IEnumerable<T> items, int n)
    {
      throw new InvalidOperationException("Take no implementado aún");
    }

    public static IEnumerable<T> TakeLast<T>(IEnumerable<T> items, int n)
    {
      throw new InvalidOperationException("Take no implementado aún");
    }

    public static IEnumerable<T> Reverse<T>(IEnumerable<T> items)
    {
      throw new InvalidOperationException("Reverse no implementado aún");
    }
  }
    #endregion

  class ProgramLinkedList
  {
    static void Main(string[] args)
    {
      LinkedList<int> ints = new LinkedList<int>();
      for (int i = 0; i < 20; i++) ints.Add(i * 2);
      ints[10] = 1000;
      Console.WriteLine("Hay {0} items", ints.Count);
      Console.WriteLine("Iterando con for ...");
      for (int i = 0; i < ints.Count; i++) Console.WriteLine(ints[i]);
      Console.WriteLine("Iterando con foreach ...");
      foreach (int k in ints) Console.WriteLine(k);

      //ERRORES POR MAL USO DE LOS TIPOS
      //Console.WriteLine(ints.Contains("white"));
      //ints[2] = "orange";
      //string s = ints[5];
    }  
  }
}