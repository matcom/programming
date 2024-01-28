namespace Programacion
{
  internal class Program05
  {
    static public IEnumerable<int>Pares()
    {
      int n = 0;
      while (true)
      {
        n += 2;
        yield return n;
      }
    }
    static public IEnumerable<T> First<T>(int n, IEnumerable<T> elems)
    {
      foreach (T x in elems)
      {
        n--;
        if (n >= 0) yield return x;
        else break; //Ilustrar que pasa si se quita este break
      }
    }
    static void Main(string[] args)
    {
      while (true)
      {
        Console.Write("\nCuantos pares a listar --> ");
        int n = int.Parse(Console.ReadLine());
        Console.WriteLine("Los {0} primeros pares son",n);
        foreach (int k in First(n, Pares()))
        {
          Console.WriteLine(k);
        }
      }
      Console.WriteLine("Hello, World!");
    }
  }
}

//EJERCICIOS
//Para un enumerable de enteros devolver aquellos que sean primos
//Para un enumerable de string devolver aquellos string que contengan una determinada subcadena
//Para un enumerable de string devuelva un enumerable de int con las longitudes de los string
//Para un enumerable de enumerables de int devuelva un enumerable con las sumas de los enumerables
//