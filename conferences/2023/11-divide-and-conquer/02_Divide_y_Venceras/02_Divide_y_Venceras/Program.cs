// See https://aka.ms/new-console-template for more information
//using System.Diagnostics;
//using System.Runtime.CompilerServices;

//int[] GenerarRandomIntArray(int n)
////Devuelve un array de int de longitud n y con valores aleatorios menores que 1000
//{
//  var a = new int[n];
//  Random generador = new Random();
//  for (int k = 0; k < a.Length; k++)
//    a[k] = generador.Next(1000);
//  return a;
//}

#region BUSQUEDAS

////BUSQUEDA SECUENCIAL
//int BusquedaSecuencial(int[] a, int x)
//{
//  long count = 0;
//  for (int k = 0; k < a.Length; k++)
//  {
//    count++;
//    if (a[k] == x)
//    {
//      Console.WriteLine("Se han hecho {0} iteraciones", count);
//      return k;
//    }
//  }
//  Console.WriteLine("Se han hecho {0} iteraciones", count);
//  return -1;
//}

////BUSQUEDA BINARIA ITERATIVA EN SECUENCIA ORDENADA
//int BusquedaBinaria(int[] a, int x)
//{
//  if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
//  long count = 0;
//  int inf = 0;
//  int sup = a.Length - 1;
//  while (inf <= sup)
//  {
//    count++;
//    int medio = inf + (sup - inf) / 2;
//    if (x > a[medio])
//      inf = medio + 1;
//    else if (x < a[medio])
//      sup = medio - 1;
//    else
//    {
//      Console.WriteLine("Se han hecho {0} iteraciones", count);
//      return medio;
//    }
//  }
//  Console.WriteLine("Se han hecho {0} iteraciones", count);
//  return -1;
//}

////BUSQUEDA BINARIA RECURSIVA
//int BusquedaBinariaRecursiva(int[] a, int x)
//{
//  long count = 0;
//  int Busq(int[] a, int x, int inf, int sup)
//  {
//    count++;
//    if (inf > sup) return -1;
//    int medio = inf + (sup - inf) / 2;
//    if (x > a[medio])
//      return Busq(a, x, medio + 1, sup);
//    else if (x < a[medio])
//      return Busq(a, x, inf, medio - 1);
//    else return medio;
//  }
//  if (a == null) throw new Exception("Parámetro no puede ser null");
//  //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
//  var result = Busq(a, x, 0, a.Length - 1);
//  Console.WriteLine("Se han hecho {0} iteraciones", count);
//  return result;
//}

////PRUEBA DE BÚSQUEDA
//do
//{
//  Console.Write("\nEntre longitud para el array ");
//  string s = Console.ReadLine();
//  int n = Int32.Parse(s);
//  var a = GenerarRandomIntArray(n);
//  Console.Write("Entre el número a buscar ");
//  s = Console.ReadLine();
//  if (s.Length == 0) break;
//  int x = Int32.Parse(s);
//  Stopwatch crono = new Stopwatch();

//  Console.WriteLine("\nBusqueda secuencial en array no ordenado...");
//  crono.Restart();
//  var pos = BusquedaSecuencial(a, x);
//  //El array no esta ordenado
//  crono.Stop();
//  Console.WriteLine("El número {0} está en la posición {1}", x, pos);
//  Console.WriteLine("En {0} milisegundos", crono.ElapsedMilliseconds);

//  Console.WriteLine("\nOrdenando el array ...");
//  //En una situacion real no se va a ordenar cada vez que se quiere buscar, se sobreentiende que el array ya esta ordenado
//  Array.Sort(a);

//  Console.WriteLine("\nBusqueda binaria sin recursion...");
//  crono.Restart();
//  pos = BusquedaBinaria(a, x);
//  crono.Stop();
//  Console.WriteLine("El número {0} está en la posición {1}", x, pos);
//  Console.WriteLine("En {0} milisegundos", crono.ElapsedMilliseconds);

//  Console.WriteLine("\nBusqueda binaria con recursion...");
//  crono.Restart();
//  pos = BusquedaBinariaRecursiva(a, x);
//  crono.Stop();
//  Console.WriteLine("El número {0} está en la posición {1}", x, pos);
//  Console.WriteLine("En {0} milisegundos", crono.ElapsedMilliseconds);
//} while (true);

#endregion

#region ORDENACION
bool Iguales(int[] a, int[] b)
{
  if (a.Length != b.Length) return false;
  for (int k = 0; k < a.Length; k++)
    if (a[k] != b[k]) return false;
  return true;
}

#region ORDENACIÓN FUERZA BRUTA MINIMOS SUCESIVOS
void OrdenarMinimosSucesivos(int[] a)
{
  if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
  long count = 0;
  for (int k = 0; k < a.Length - 1; k++)
    for (int j = k + 1; j < a.Length; j++)
    {
      count++;
      if (a[j] < a[k])
      {
        //intercambiar a[j] con a[k]
        int temp = a[j];
        a[j] = a[k];
        a[k] = temp;
      }
    }
  Console.WriteLine("Array ordenado en {0} iteraciones", count);
}
#endregion

#region DIVIDE Y VENCERAS ORDENACIÓN POR MEZCLA (MERGE SORT)
void OrdenarPorMezcla(int[] a)
{

  long count = 0;
  void Mezclar(int[] a, int[] aux, int inf, int medio, int sup)
  //Presupone los dos arrays ordenados y de
  {
    int izq = inf; // para recorrer la 1ra mitad
    int der = medio + 1; // para recorer la 2da mitad
    int pos = 0;
    //int pos = inf; // para ir dejando la mezcla en aux

    while (izq <= medio && der <= sup)
    {
      count++;
      if (a[izq] < a[der]) aux[pos++] = a[izq++];
      else aux[pos++] = a[der++];
    }
    while (izq <= medio)
    {
      count++;
      aux[pos++] = a[izq++];
    }
    while (der <= sup)
    {
      count++;
      aux[pos++] = a[der++];
    }
    //Pasar el resultado de la mezcla de nuevo para a
    Array.Copy(aux, 0, a, inf, sup - inf + 1);
    count += sup - inf + 1;
  }

  void OrdenarPorMezclaRec(int[] a, int[] aux, int inf, int sup)
  {
    if (inf < sup)
    {
      int medio = (inf + sup) / 2;
      OrdenarPorMezclaRec(a, aux, inf, medio);
      OrdenarPorMezclaRec(a, aux, medio + 1, sup);
      Mezclar(a, aux, inf, medio, sup);
    }
  }
  if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
  int[] aux = new int[a.Length];
  OrdenarPorMezclaRec(a, aux, 0, a.Length - 1);
  Console.WriteLine("Se han hecho {0} iteraciones", count);
}
#endregion

do
{
  //Varie la longitud del array en dependencia de su PC y lo que este dispuesto a esperar
  Console.Write("\nEntre longitud para el array (no pasar de 300,000) ");
  string s = Console.ReadLine();
  int n = Int32.Parse(s);
  Console.WriteLine("Generando random array y dos copias iguales...");
  var a = GenerarRandomIntArray(n);
  //Crear dos copias del mismo array
  var b = new int[a.Length];
  Array.Copy(a, b, a.Length);
  var c = new int[a.Length];
  Array.Copy(a, c, a.Length);
  Stopwatch crono = new Stopwatch();

  Console.WriteLine("\nOrdenando por Minimos Sucesivos n^2 (fuerza bruta) ...");
  crono.Restart();
  OrdenarMinimosSucesivos(a);
  crono.Stop();
  Console.WriteLine("Ordenados en {0} milisegundos", crono.ElapsedMilliseconds);

  Console.WriteLine("\nOrdenando por mezcla n*ln(n)...");
  crono.Restart();
  OrdenarPorMezcla(b);
  crono.Stop();
  Console.WriteLine("Ordenados en {0} milisegundos", crono.ElapsedMilliseconds);

  Console.WriteLine("\nOrdenando por QuickSort n*ln(n) mejorado ...");
  crono.Restart();
  Array.Sort(c);
  crono.Stop();
  Console.WriteLine("Ordenados en {0} milisegundos", crono.ElapsedMilliseconds);

  //Verificando que los tres arrays ordenados tienen los mismos valores
  if (Iguales(a, b) && Iguales(b, c))
    Console.WriteLine("\nBIEN !!! Los tres arrays estan igualmente ordenados!!");
  else
    Console.WriteLine("Ha ocurrido un error en la ordenacion");
} while (true);

#endregion

