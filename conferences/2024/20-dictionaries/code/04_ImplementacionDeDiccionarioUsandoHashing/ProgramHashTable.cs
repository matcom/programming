using Programacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programacion
{
    public class Fecha
    {
        public int D { get; }
        public int M { get; }
        public int A { get; }
        public Fecha(int d, int m, int a)
        {
            D = d; M = m; A = a;
        }
        public override string ToString()
        {
            return string.Format("({0},{1},{2})", D, M, A);
        }
    }
    class FechaEqualityComparer : IEqualityComparer<Fecha>
    {
        public bool Equals(Fecha f1, Fecha f2)
        {
            return f1.D == f2.D && f1.M == f2.M && f1.A == f2.A;
        }
        public int GetHashCode(Fecha f)
        {
            return (f.A - 1) * 365 + (f.M - 1) * 30 + f.D;
        }
    }

    class KeyValuePair<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; }
        public KeyValuePair(TKey key, TValue value)
        {
            Key = key; Value = value;
        }
    }
    class DictionaryLinkedNode<TKey, TValue>
    {
        public KeyValuePair<TKey, TValue> Pair { get; set; }

        public DictionaryLinkedNode<TKey, TValue> Next;
        public DictionaryLinkedNode(TKey key, TValue value, DictionaryLinkedNode<TKey, TValue> next = null)
        {
            Pair = new KeyValuePair<TKey, TValue>(key, value);
            Next = next;
        }
    }
    class Dictionary<TKey, TValue>
    //No vamos a implementar todos los metodos de diccionario
    {
        private DictionaryLinkedNode<TKey, TValue>[] tabla;
        IEqualityComparer<TKey> Comparer;
        private int count;
        public Dictionary(IEqualityComparer<TKey> comparer, int size = 101)
        {
            tabla = new DictionaryLinkedNode<TKey, TValue>[size];
            Comparer = comparer;
        }
        public bool ContainsKey(TKey key)
        {
            var index = Math.Abs(Comparer.GetHashCode(key)) % tabla.Length;
            DictionaryLinkedNode<TKey, TValue> cursor = tabla[index];
            while (cursor != null)
                if (Comparer.Equals(cursor.Pair.Key, key)) return true;
                else cursor = cursor.Next;
            return false;
        }
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new InvalidOperationException();
            var index = Math.Abs(Comparer.GetHashCode(key)) % tabla.Length;
            if (tabla[index] == null)
                tabla[index] = new DictionaryLinkedNode<TKey, TValue>(key, value, null);
            else
                tabla[index] = new DictionaryLinkedNode<TKey, TValue>(key, value, tabla[index]);
            count++;
        }
        public TValue this[TKey key]
        {
            get
            {
                var index = Math.Abs(Comparer.GetHashCode(key)) % tabla.Length;
                DictionaryLinkedNode<TKey, TValue> cursor = tabla[index];
                while (cursor != null)
                {
                    if (Comparer.Equals(cursor.Pair.Key, key)) return cursor.Pair.Value;
                    else cursor = cursor.Next;
                }
                throw new ArgumentException(); //La llave no está
            }
            set
            {
                var index = Math.Abs(Comparer.GetHashCode(key)) % tabla.Length;
                DictionaryLinkedNode<TKey, TValue> cursor = tabla[index];
                while (cursor != null)
                {
                    if (Comparer.Equals(cursor.Pair.Key, key))
                    {
                        cursor.Pair = new KeyValuePair<TKey, TValue>(key, value);
                        return;
                    }
                    else cursor = cursor.Next;
                }
                tabla[index] = new DictionaryLinkedNode<TKey, TValue>(key, value, tabla[index]);
            }
        }

        public bool Remove(TKey key)
        {
            //TO DO
            throw new Exception("Unimplemented instruction");
        }

        public int Count
        {
            get { return count; }
        }

        //Falta por implementar el IEnumerable que recorra los pares en el diccionario

    }

}

class Program
{
    static void Main(string[] args)
    {
        Programacion.Dictionary<Fecha, string> efemerides =
          new Programacion.Dictionary<Fecha, string>(new FechaEqualityComparer());

        while (true)
        {
            Console.WriteLine("\nEntre Fecha");
            Console.Write("Entre dia: ");
            int d = int.Parse(Console.ReadLine());
            if (d == 0) break;
            Console.Write("Entre mes: ");
            int m = int.Parse(Console.ReadLine());
            Console.Write("Entre año: ");
            int a = int.Parse(Console.ReadLine());

            Console.Write("Entre sucesos de esta fecha: ");
            string sucesos = Console.ReadLine();
            efemerides.Add(new Fecha(d, m, a), sucesos);
            Console.WriteLine("Las efemerides son {0}", efemerides[new Fecha(10, 10, 2023)]);
        }
    }
}