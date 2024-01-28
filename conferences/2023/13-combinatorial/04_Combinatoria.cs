// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

double Factorial(int n)
{
  //Lo representamos en double para poder calcular factoriales grandes
  double result = 1d;
  for (int i = 1; i <= n; i++) result = result * i;
  return result;
}


//Datos para el ejemplo del problema del viajante
int[,] distancias =
  new int[,] {{ 0, 60, 30, 40, 35},
              {60,  0, 15, 55, 30},
              {30, 15,  0, 75, 45},
              {40, 55, 75,  0, 85},
              {35, 30, 45, 85,  0}};
string[] nombres = new string[] { "Habana", "San Jose", "Bauta", "Madruga", "Guines" };
int menorDistancia = int.MaxValue;
int[] mejorCamino;

//Para sumar los valores 
int LongitudRecorrido(int[] recorrido)
{
  int result = 0;
  for (int i = 0; i < recorrido.Length - 1; i++)
    result += distancias[recorrido[i], recorrido[i + 1]];
  return result;
}

//Aquí vendría la función que se quiere aplicar a cada permutación
//que se genere. Para el caso del problema del viajante seria
//calcular la logitud del recorrido y quedarnos con la menor
void EvaluarPermutacion(int[] a)
{
  ////DESCOMENTAR SI QUEREMOS IR VISUALIZANDO LAS PERMUTACIONES GENERADAS
  //for (int k = 0; k < a.Length; k++)
  //{
  //  Console.Write("{0,-3}", a[k]);
  //}
  //Console.ReadLine(); //Pausar para ver la permutación

  ////DESCOMENTAR PARA EL CASO DEL EJEMPLO DEL VIAJANTE
  //Calcular distancia recorrido
  int longitud_recorrido = LongitudRecorrido(a);
  if (longitud_recorrido < menorDistancia)
  {
    menorDistancia = longitud_recorrido;
    System.Array.Copy(a, mejorCamino, a.Length);
    //Ir manteniendo copia del mejor camino hasta el momento
  }
}

#region PERMUTACIONES DE M ELEMENTOS SIN REPETIR
long count = 0; //para contar las llamadas
long permutaciones = 0; //para contar las permutaciones
//Por la demora, no tiene sentido ejecutar el ejemplo para n >
void VariacionesSinRepeticion(int[] a, int desde, int hasta)
{
  count++;
  if (desde > hasta)
  {
    permutaciones++;
    //Ya no hay mas nada que variar
    //Descomentar lo que sigue para ver lo que se hace con cada permutación
    //Sugerencia no visualizar con n > 4
    EvaluarPermutacion(a);
  }
  else
  {
    for (int i = desde; i <= hasta; i++)
    {
      int temp = a[i];
      a[i] = a[desde];
      a[desde] = temp;
      VariacionesSinRepeticion(a, desde + 1, hasta);
      temp = a[i];
      a[i] = a[desde];
      a[desde] = temp;
    }
  }
}
void Permutaciones(int[] a)
{
  //Cuando la longitud de las secuencias es la misma que la cantidad de elementos de a se le llama permutaciones
  //Son n! secuencias (arrays) donde n es la longitud de origen. Note que es lo mismo que n!/(n-m)! donde m == n
  VariacionesSinRepeticion(a, 0, a.Length - 1);
}
#endregion

#region PRUEBA DE EJECUCION DE LAS PERMUTACIONES
//do
//{
//  Console.WriteLine("\nProbando permutaciones...");
//  Console.WriteLine("Pruebe con n <=5 para si quiere visualizar cada permutacion");
//  Console.WriteLine("Pruebe con n <=12 para medir en su PC (sin visualizar)"); //Con n=13 da casi 3 mins en mi laptop
//  Console.WriteLine("Pruebe con n >= 20 para que tenga una idea del tiempo mínimo global");
//  Console.Write("--> ");
//  string s = Console.ReadLine();
//  if (s.Length == 0) break;
//  int n = Int32.Parse(s);

////Depende de su CPU y lo que esté dispuesto a esperar pero 
////Vaya probando valores de n hasta donde le alcance la paciencia
//Stopwatch crono = new Stopwatch();
//var a = new int[n];
//for (int i = 0; i < n; i++)
//  a[i] = i;
//crono.Restart();
//Permutaciones(a);
//crono.Stop();
//Console.WriteLine("Se han hecho {0} llamadas \npara un total de {1} permutaciones", count, permutaciones);
//Console.WriteLine("Procesarlas en mi PC demora {0} ms", crono.ElapsedMilliseconds);

  #region PRUEBA DEL APROXIMADO DEL TIEMPO DE COSTO FACTORIAL
  ////El procesador AMD EPYC 7763 tiene un rendimiento teórico máximo de aproximadamente
  ////4.92 TFLOPS(teraflops) que son mas o menos 4,920 billones de sumas de punto flotante por seg
  ////Use este código para medir el tiempo que demoraría procesar todas las permutaciones
  //double AMD_EPYC_7763 = 4920000000000;
  //double fac = Factorial(n);
  //double segs = fac / AMD_EPYC_7763;
  //double minutos = segs / 60;
  //double horas = minutos / 60;
  //double dias = horas / 24;
  //double años = dias / 365;
  //Console.WriteLine("\nFactorial de {0} es {1} ", n, fac);
  //Console.WriteLine("Procesarlas en un AMD EPYC 7763 \n(a 4,920 billones de sumas x seg)");
  //Console.WriteLine("Demoraría {0} segs", segs);
  //Console.WriteLine("Demoraría {0} minutos", minutos);
  //Console.WriteLine("Demoraría {0} dias", dias);
  //Console.WriteLine("Demoraría {0} años", años);
  #endregion
//} while (true);
#endregion

#region PRUEBA PARA APLICACION AL PROBLEMA DEL VIAJANTE EJEMPLO
var ciudades = new int[] { 0, 1, 2, 3, 4 };
mejorCamino = new int[5];
Permutaciones(ciudades);
Console.WriteLine("El mejor recorrido pasando por todas es");
for (int k = 0; k < mejorCamino.Length; k++)
{
  Console.Write("{0} ", nombres[mejorCamino[k]]);
}
Console.WriteLine();
Console.WriteLine("Que tiene una longitud de {0}", menorDistancia);
#endregion
