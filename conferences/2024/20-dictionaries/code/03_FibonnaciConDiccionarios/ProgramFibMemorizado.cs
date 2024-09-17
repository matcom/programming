using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Programacion
{

    class PruebaFibMemorizado
    {
        [STAThread]

        #region Fibonacci Recursivo Ineficiente
        static long Fibonacci(int n)
        {
            if (n == 1 || n == 2) return 1L;
            else return Fibonacci(n - 2) +
                        Fibonacci(n - 1);
        }
        #endregion

        #region Fibonacci Memorizado con un diccionario
        static Dictionary<int, long> dic = new Dictionary<int, long>();
        static long FibonacciMemorizado(int n)
        {
            long result;
            if (!dic.TryGetValue(n, out result))
            //Si está es porque ya ha sido calculado para ese valor.
            //Si no esta lo calculamos y lo guardamos en el diccionario
            {
                //Si no está en el diccionario entonces no ha sido calculado. 
                if (n == 1 || n == 2) result = 1L;
                else result = FibonacciMemorizado(n - 2) +
                              FibonacciMemorizado(n - 1);
                dic.Add(n, result);
                //El nuevo Fibonacci valculado para n lo guardamos en el diccionarioCalcularlo y Guardarlo
            }
            return result;
        }

        #endregion

        static void Main(string[] args)
        {
            #region  USANDO DICCIONARIO PARA EL PATRÓN MEMOIZE CON FIBONACCI
            //Empezar con el recursivo para recordar por qué es ineficiente
            Stopwatch crono = new Stopwatch();
            int valor; long result;
            while (true)
            {
                Console.Write("\nEntre número a calcular Fibonacci ");
                string s = Console.ReadLine();
                if (int.TryParse(s, out valor))
                {
                    //Empezar con el ineficiente para mostrar demora
                    crono.Start();
                    result = Fibonacci(valor);
                    crono.Stop();
                    Console.WriteLine("Fibonacci Recursivo de  {0} = {1} calculado en {2} ms", valor, result, crono.ElapsedMilliseconds);

                    //PRUEBA DE FIBONACCI MEMORIZADO. Descomentar este para probar con diccionario
                    //crono.Restart();
                    //result = FibonacciMemorizado(valor);
                    //crono.Stop();
                    //Console.WriteLine("Fibonacci Memorizado de {0} = {1} calculado en {2} ms", valor, result, crono.ElapsedMilliseconds);

                    //Descomentar para ver la cantidad de entradas(llaves) que se han guardado en el diccionario
                    //Console.WriteLine("Hay {0} entradas en el diccionario", dic.Count);
                }
                else break;
            }
            #endregion
        }
    }
}
#region EJERCICIOS CLASE PRACTICA
//Defina una funcion que reciba como parametro una funcion y devuelva una funcion
//que haga lo mismo pero con capacidad de memorizacion
#endregion