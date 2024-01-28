// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
int[] CreateRandomArray(int n)
{
  int[] a = new int[n];
  Random r = new Random();
  for (int k = 0; k < n; k++)
  {
    //Para que haya negativos en el array los pares los ponemos como positivos
    //y los impares como negativos
    var j = r.Next(n);
    if (j % 2 == 0)
      a[k] = j;
    else
      a[k] = -j;
  }
  return a;
}

void Print(int[] a)
{
  for (int k = 0; k < a.Length; k++)
  {
    Console.Write("{0,4}  ", a[k]);
    if ((k + 1) % 10 == 0) Console.WriteLine();
  }
}

#region SUBSECUENCIA SUMA MAXIMA
//DE ORDEN CÚBICO, n^3 (n longitud del array)
int SubSumaMaxFuerzaBruta(int[] a)
{
  long count = 0;
  int maxSuma = 0;
  for (int i = 0; i < a.Length; i++)
    for (int j = i; j < a.Length; j++)
    {
      int suma = 0;
      for (int k = i; k <= j; k++)
      {
        count++;
        suma += a[k];
      }
      if (suma > maxSuma) maxSuma = suma;
    }
  Console.WriteLine("\nSubSumaMax por Fuerza Bruta Cubico n^3 {0} iteraciones", count);
  return maxSuma;
}

//DE ORDEN CUADRÁTICO, n^2 (n longitud del array)
int SubSumaMaxCuadratico(int[] a)
{
  int maxSuma = 0;
  long count = 0;
  for (int i = 0; i < a.Length; i++)
  {
    int suma = 0;
    for (int j = i; j < a.Length; j++)
    {
      count++;
      suma += a[j];
      if (suma > maxSuma) maxSuma = suma;
    }
  }
  Console.WriteLine("\nSubSumaMax por Fuerza Bruta Cuadratico n^2 {0} iteraciones", count);
  return maxSuma;
}

//DIVIDE Y VENCERAS RECURSIVO, n*ln(n) (n LONGITUD DEL ARRAY)
int SubSumaMaxDivideVenceras(int[] a)
{
  long count = 0;
  int SubSumaMaxRecursivo(int[] a, int inf, int sup)
  {
    count++;
    int maxHaciaIzq = 0; int maxHaciaDer = 0;
    int medio, mayorSumaIzq, mayorSumaCentro, mayorSumaDer;
    if (inf == sup) return (a[inf] > 0 ? a[inf] : 0);
    else
    {
      medio = (inf + sup) / 2;
      mayorSumaIzq = SubSumaMaxRecursivo(a, inf, medio);
      mayorSumaDer = SubSumaMaxRecursivo(a, medio + 1, sup);
      int sum = 0;
      for (int i = medio; i >= inf; i--)
      {
        count++;
        sum += a[i];
        if (sum > maxHaciaIzq) maxHaciaIzq = sum;
      }
      sum = 0;
      for (int i = medio + 1; i <= sup; i++)
      {
        count++;
        sum += a[i];
        if (sum > maxHaciaDer) maxHaciaDer = sum;
      }
      mayorSumaCentro = maxHaciaIzq + maxHaciaDer;
      return Math.Max(mayorSumaIzq, Math.Max(mayorSumaCentro, mayorSumaDer));
    }
  }
  var result = SubSumaMaxRecursivo(a, 0, a.Length - 1);
  Console.WriteLine("\nSubSumaMax Div y Vencera n*ln(n) {0} iteraciones", count);
  return result;
}

  #region LINEAL
  //DE ORDEN LINEAL (EN UN SOLO RECORRIDO DEL ARRAY)
  int SubSumaMaxLineal(int[] a)
  {
    long count=0;
    int maxSuma = 0;
    int suma = 0;
    for (int i = 0; i < a.Length; i++)
    {
      count++;
      suma += a[i];
      if (suma > maxSuma) maxSuma = suma;
      else if (suma < 0) suma = 0;
    }
    Console.WriteLine("\nSubSumaMax Lineal n {0} iteraciones", count);
    return maxSuma;
  }
#endregion
#endregion

//PRUEBA SUBSECUENCIA SUMA MAXIMA
do
{
  Stopwatch crono = new Stopwatch();
  Console.Write("\nEntre longitud para la secuencia ");
  string s = Console.ReadLine();
  if (s.Length == 0) break;
  int n = Int32.Parse(s);
  var secuencia = CreateRandomArray(n);
  int result;

  crono.Restart();
  result = SubSumaMaxFuerzaBruta(secuencia);
  crono.Stop();
  Console.WriteLine("SubSumaMax (Fuerza Bruta) Cubico n^3 {0} en {1} ms", result, crono.ElapsedMilliseconds);

  crono.Restart();
  result = SubSumaMaxCuadratico(secuencia);
  crono.Stop();
  Console.WriteLine("SubSumaMax Cuadratico n*n {0} en {1} ms", result, crono.ElapsedMilliseconds);

  crono.Restart();
  result = SubSumaMaxDivideVenceras(secuencia);
  crono.Stop();
  Console.WriteLine("SubSumaMax Div y Venceras n*ln(n) {0} en {1} ms", result, crono.ElapsedMilliseconds);

  //crono.Restart();
  //result = SubSumaMaxLineal(secuencia);
  //crono.Stop();
  //Console.WriteLine("SubSumaMax Lineal n {0} en {1} ms", result, crono.ElapsedMilliseconds);

} while (true);


