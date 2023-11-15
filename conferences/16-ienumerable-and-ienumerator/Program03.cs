using System.Collections;
using System.Collections.Generic;
namespace Programacion
{
  internal class Program03
  {
    public class ParesEnIntervalo : IEnumerable<int>
    {
      public int Min { get; }
      public int Max { get; }
      public ParesEnIntervalo(int min, int max)
      {
        //Se podria verificar si forman un intervalo
        Min = min; Max = max;
      }

      #region IMPLEMENTACIÓN LOW LEVEL (SIN USAR YIELD)
      public IEnumerator<int> GetEnumerator()
      {
        return new ParesEnumerator(Min, Max);
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
      class ParesEnumerator : IEnumerator<int>
      {
        public int Min { get; }
        public int Max { get; }
        int cursor; bool huboMoveNext;
        int current;
        public ParesEnumerator(int min, int max)
        {
          Min = min; Max = max;
          if (Min % 2 == 0) cursor = min;
          else cursor = Min + 1;
          //Garantizando empezar con un par
          huboMoveNext = false;
        }
        public bool MoveNext()
        {
          if (cursor <= Max)
          {
            current = cursor;
            cursor += 2;
            return huboMoveNext = true;
          }
          else return huboMoveNext = false;
        }
        public int Current
        {
          //Un ejemplo de por que Current no solo devuelve un valor
          get
          {
            if (huboMoveNext) return current;
            else throw new Exception("There are no more elements");
          }
        }
        object IEnumerator.Current
        {
          //Un ejemplo de por que Current no solo devuelve un valor
          get { return Current; }
        }
        public void Reset()
        {
          //Ponerlo todo para empezar de nuevo
          if (Min % 2 == 0) cursor = Min;
          else cursor = Min + 1;
          huboMoveNext = false;
        }
        public void Dispose()
        {
          //De momento no se implementa ninguna acción en Dispose
        }
      }

      #endregion
      static void Main(string[] args)
      {
        #region INTERVALO DE ENTEROS
        //while (true)
        //{
        //  Console.Write("Entre cota inferior --> ");
        //  int inf = int.Parse(Console.ReadLine());
        //  Console.Write("Entre cota superior --> ");
        //  int sup = int.Parse(Console.ReadLine());
        //  var pares = new ParesEnIntervalo(inf, sup);
        //  Console.WriteLine("Los cuadrados de los pares en ({0},{1}) son", inf, sup);
        //  foreach (var n in pares)
        //    Console.WriteLine(n*n);
        //}
        #endregion

        #region string[] es IENUMERABLE<string>
        //IEnumerable<string> colores = new string[] { "rojo", "azul", "blanco", "negro" };
        //Console.WriteLine("\nColores por foreach");
        //foreach (var s in colores)
        //{
        //  Console.WriteLine("{0} tiene longitud {1}", s, s.Length);
        //}
        ////Usando la maquinaria de IEnumerator
        //var colors = colores.GetEnumerator();
        //Console.WriteLine("Colores por IEnumerator");
        //while (colors.MoveNext())
        //{
        //  Console.WriteLine("{0} tiene longitud {1}", 
        //                      colors.Current, 
        //                      colors.Current.Length);
        //}
        #endregion
      }
    }
  }
}