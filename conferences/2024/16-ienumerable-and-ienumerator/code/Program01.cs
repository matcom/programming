using System.Collections;

namespace Programacion
{
  internal class Program01
  {
    static void Main(string[] args)
    {
      #region IENUMERABLE E IENUMERATOR SIN GENERICIDAD
      //LOS ARRAYS SON IMPLEMENTAN LA INTERFACE IENUMERATOR
      string[] colores = new string[] { "rojo", "azul", "blanco", "negro" };

      //RECORRIDO CON IENUMERATOR
      IEnumerator enumColores = colores.GetEnumerator();
      Console.WriteLine("\nRecorrido con IEnumerator ...");
      while (enumColores.MoveNext())
      {
        //Compila y ejecuta bien porque string implementa el ToString de object
        Console.WriteLine(enumColores.Current);

        //ERROR DE COMPILACION
        //Para el compiler lo que devuelve Current es object
        //y object NO TIENE Length
        //Console.WriteLine(enumColores.Current.Length); 
      }

      ////RECORRIDO DIRECTO DEL IENUMERABLE CON FOREACH
      //Console.WriteLine("\nRecorrido con IEnumerable y foreach ...");
      //foreach (object x in colores)
      //  Console.WriteLine(x);

      ////EL CODIGO ANTERIOR ES EQUIVALENTE A. El COMPILADOR HACE LA TRANSFORMACION
      //IEnumerator items = colores.GetEnumerator();
      //Console.WriteLine();
      //while (items.MoveNext())
      //{
      //  object x = items.Current;
      //  Console.WriteLine(x);
      //}
      #endregion

    }
  }
}