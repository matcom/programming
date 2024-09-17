#region FACTORIAL
//Descomentar el codigo para probar
//long FactorialIterativo(long n)
//{
//  if (n < 0) throw new Exception("Parámetro debe ser positivo");
//  long result = 1;
//  for (long k = 1; k <= n; k++) result *= k;
//  return result;
//}

//long FactorialRecursivo(long n)
//{
//  long count = 0; //para contar la cantidad de llamadas recursivas que ocurren
//  long Fac(long n)
//    //Se define esta funcion interna que es la recursiva para que el control de si el parametro es coorecto no se este repitiendo
//  {
//    count++;
//    if (n == 1) return 1;
//    else
//      return n * Fac(n - 1);
//  }
//  if (n <= 0) throw new Exception("Parametro debe ser positivo");
//  else
//  {
//    var result = Fac(n);
//    Console.WriteLine("Se han hecho {0} llamadas", count);
//    return result;
//  }
//}
//do
//{
//  Console.Write("\nEntre el numero a hallar factorial ");
//  string s = Console.ReadLine();
//  if (s.Length == 0) break;
//  int k = Int32.Parse(s);
//  Console.WriteLine("Factorial iterativo de {0} es {1}", k, FactorialIterativo(k));
//  Console.WriteLine("Factorial recursivo de {0} es {1}", k, FactorialRecursivo(k));
//} while (true);
#endregion

#region FIBONACCI
//Calcular el enesimo elemento de la sucesion de Fibonacci
//int FibonacciIterativo(int n)
//{
//  if (n < 1) throw new Exception("Parámetro debe ser positivo");
//  int ultimo = 1, penultimo = 1;
//  for (int k = 3; k <= n; k++)
//  {
//    int temp = penultimo; penultimo = ultimo; ultimo = ultimo + temp;
//  }
//  return ultimo;
//}

//int FibonacciRecursivo(int n)
//{
//  long count = 0;
//  int Fib(int n)
//  {
//    count++;
//    if ((n == 1) || (n == 2)) return 1;
//    else return Fib(n - 1) + Fib(n - 2);
//  }
//  if (n < 1) throw new Exception("Parámetro debe ser >= 1");
//  var result = Fib(n);
//  Console.WriteLine("Se han hecho {0} llamadas", count);
//  return result;
//}

//int FibonacciConRecCola(int n)
//{
//  //Este ejemplo demuestra que la recursion no es ineficiente per se
//  //Note que en este caso se hace la misma cantidad de llamadas que iteraciones en el secuencial
//  long count = 0;
//  int Fib(int n, int penultimo, int ultimo)
//  {
//    count++;
//    if (n <= 2) return ultimo;
//    else return Fib(n - 1, ultimo, penultimo+ultimo);
//  }
//  if (n < 1) throw new Exception("Parámetro debe ser >= 1");
//  var result = Fib(n, 1, 1);
//  //Se le dice recursividad de cola porque despues de la llamada recursiva 
//  //no hay ningun codigo que haga otra llamada recursiva
//  Console.WriteLine("Se han hecho {0} llamadas", count);
//  return result;
//}

////Probando Fibonacci
//do
//{
//  Console.Write("\nEntre el numero a hallar Fibonacci ");
//  string s = Console.ReadLine();
//  if (s.Length == 0) break;
//  int k = Int32.Parse(s);
//  Console.WriteLine("El elemento {0} de la sucesion de Fibonacci es {1}", k, FibonacciIterativo(k));
//  Console.WriteLine("El elemento {0} de la sucesion de Fibonacci es {1}", k, FibonacciRecursivo(k));
//  Console.WriteLine("El elemento {0} de la sucesion de Fibonacci es {1}", k, FibonacciConRecCola(k));
//}while (true);
#endregion

#region TORRES DE HANOI
//void TorresHanoi(int n, string origen, string auxiliar, string destino)
//{
//  long count = 0;
//  void Hanoi(int n, string origen, string auxiliar, string destino)
//  {
//    void MueveUnDisco()
//    {
//      //Esta funcion esta dentro de Hanoi por tato tiene acceso a los parametros de la funcion que la contiene
//      //Esta implementacion se sustituiria segun la forma en que se quieran visualizar los movimientos
//      //Comente la linea de arriba cuando quiera probar con una cantidad grande de discos 
//      //y solo le interese saber la cantidad de llamadas que han ocurrido
//    }

//    count++; //Se incrementa el contador global cada vez que ocurre una llamada recursiva
//    if (n == 1)
//    {
//      MueveUnDisco();
//    }
//    else
//    {
//      Hanoi(n - 1, origen, destino, auxiliar);
//      //Al llegar aqui se han ejecutado todas las llamadas recursivas que han movido los n-1 discos
//      MueveUnDisco();
//      //La "MAGIA" de la recursividad garantiza que al regresar de todas las llamadas enteriores
//      //los valores de origen, destino y auxiliar son los mismos de antes
//      Hanoi(n - 1, auxiliar, origen, destino);
//    }
//  }
//  if (n <= 0) throw new Exception("El parametro debe ser mayor que 0");
//  else
//  {
//    Hanoi(n, origen, auxiliar, destino);
//    Console.WriteLine("Se han realizado {0} llamadas", count);
//  }
//}
////Probando Torres Hanoi
//do
//{
//  Console.Write("\nEntre el numero de discos a mover ");
//  string s = Console.ReadLine();
//  if (s.Length == 0) break;
//  int k = Int32.Parse(s);
//  TorresHanoi(k, "izquierda", "centro", "derecha");
//}while (true);

#endregion