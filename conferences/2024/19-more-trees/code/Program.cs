using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WEBOO.Programacion
{
  class Program
  {
    static void Main(string[] args)
    {
      #region Ejemplo de recorrido EntreOrden
      ArbolBinario<string> expresion = new ArbolBinario<string>("*",
                                               new ArbolBinario<string>("+", new ArbolBinario<string>("a"), new ArbolBinario<string>("b")),
                                               new ArbolBinario<string>("-", new ArbolBinario<string>("c"), new ArbolBinario<string>("d")));

      Console.WriteLine("\nRecorriendo en EntreOrden un árbol...");
      foreach (var s in expresion.EntreOrden())
        Console.Write("{0}  ", s);
      Console.WriteLine();
      #endregion

      #region Ejemplo de recorrido en postorden para evaluar un árbol de expresion
      ArbolBinario<object> expr = new ArbolBinario<object>('*',
                                     new ArbolBinario<object>('+', new ArbolBinario<object>(4), new ArbolBinario<object>(2)),
                                     new ArbolBinario<object>('-', new ArbolBinario<object>(5), new ArbolBinario<object>(3)));
      //Evaluar
      Stack<object> pila = new Stack<object>();
      int op1, op2;
      Console.WriteLine("\nRecorriendo un árbol de expresión en postorden para evaluar la expresión...");
      foreach (var x in expr.PostOrden())
      {
        Console.Write("{0}  ", x);
        if (x is int) pila.Push(x);
        else if (x is char)
        {
          if (pila.Count > 0) op2 = (int)(pila.Pop());
          else throw new Exception("Arbol no corresponde a una expresión");
          if (pila.Count > 0) op1 = (int)(pila.Pop());
          else throw new Exception("Arbol no corresponde a una expresión");
          switch ((char)x)
          {
            case '+': pila.Push(op1 + op2); break;
            case '-': pila.Push(op1 - op2); break;
            case '*': pila.Push(op1 * op2); break;
            case '/': pila.Push(op1 / op2); break;
            case '%': pila.Push(op1 % op2); break;
            default: throw new Exception("Operador inválido");
          }
        }
        else throw new Exception("No es operando entero ni operador");
      }
      if (pila.Count == 1)
        Console.WriteLine("\nResultado de evaluar la expresión es {0}", pila.Pop());
      else throw new Exception("Arbol no corresponde a una expresión");
      #endregion

      ArbolBinarioOrdenado<int> arbol = new ArbolBinarioOrdenado<int>(20,
                           new ArbolBinarioOrdenado<int>(15, new ArbolBinarioOrdenado<int>(10), new ArbolBinarioOrdenado<int>(18)),
                           new ArbolBinarioOrdenado<int>(40, new ArbolBinarioOrdenado<int>(33), new ArbolBinarioOrdenado<int>(52)));

      Console.WriteLine("\nRecorriendo en EntreOrden árbol ordenado...");
      foreach (var x in arbol.EntreOrden())
        Console.Write("{0}  ", x);
      Console.WriteLine();

      while (true)
      {
        Console.Write("\nEntre numero a buscar ");
        string s = Console.ReadLine();
        if (s.Length == 0) break;
        int k = Int32.Parse(s);
        Console.WriteLine("Arbol contiene a {0} es {1}", k, arbol.Contiene(k));
      }
    }

  }
}
