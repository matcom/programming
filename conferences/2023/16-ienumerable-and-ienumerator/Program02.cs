using System.Collections;
namespace Programacion
{
  internal class Program02
  {
    public class ParesEnIntervalo : IEnumerable
    {
      public int Min { get; }
      public int Max { get; }
      public ParesEnIntervalo(int min, int max)
      {
        //Se podria verificar si forman un intervalo
        Min = min; Max = max;
      }

      #region IMPLEMENTACIÓN LOW LEVEL (SIN USAR YIELD)
      public IEnumerator GetEnumerator()
      {
        return new ParesEnumerator(Min, Max);
      }

      class ParesEnumerator : IEnumerator
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
        public object Current
        {
          //Un ejemplo de por que Current no solo devuelve un valor
          get
          {
            if (huboMoveNext) return current;
            else throw new Exception("There are no more elements");
          }
        }
        public void Reset()
        {
          //Ponerlo todo para empezar de nuevo
          if (Min % 2 == 0) cursor = Min;
          else cursor = Min + 1;
          huboMoveNext = false;
        }
        void Dispose()
        {
          //De momento no se implementa ninguna acción en Dispose
        }
      }

      #endregion
      static void Main(string[] args)
      {
        while (true)
        {
          Console.Write("Entre cota inferior --> ");
          int inf = int.Parse(Console.ReadLine());
          Console.Write("Entre cota superior --> ");
          int sup = int.Parse(Console.ReadLine());
          var pares = new ParesEnIntervalo(inf, sup);
          Console.WriteLine("Los pares en ({0},{1}) son", inf, sup);
          //La maquinaria del recorrido esta encapsulada en el enumerable
          foreach (var n in pares)
            Console.WriteLine(n);
          //Los puedo escribir porque el n es object y los int vistos como
          //object implementan el ToString

          ////Si si se descomenta lo siguiente da ERROR
          //foreach (var n in pares)
          //  Console.WriteLine(n+1);
        }
      }
    }
  }
}