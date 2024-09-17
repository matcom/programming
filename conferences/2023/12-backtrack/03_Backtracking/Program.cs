// See https://aka.ms/new-console-template for more information
#region UBICA REINAS EN UN TABLERO DE N*N

using System.Diagnostics;

long count;

void VisualizaTablero(bool[,] tablero)
{
  Console.ForegroundColor = ConsoleColor.Black;
  for (int i = 0; i < tablero.GetLength(0); i++)
  {
    for (int j = 0; j < tablero.GetLength(1); j++)
    {
      if (tablero[i, j])
      {
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("Q ");
      }
      else
      {
        if ((i + j) % 2 == 0) Console.BackgroundColor = ConsoleColor.Red;
        else Console.BackgroundColor = ConsoleColor.White;
        Console.Write("  ");
      }
    }
    Console.BackgroundColor = ConsoleColor.Black;
    Console.WriteLine();
  }
  Console.BackgroundColor = ConsoleColor.Black;
  Console.ForegroundColor = ConsoleColor.White;
}

//Verifica si en el tablero hay reinas que se amenazan entre si
bool Amenaza(bool[,] tablero, int fila, int columna)
{

  // Verificar si hay alguna reina en la misma fila hacia la izquierda
  for (int j = 0; j < columna; j++)
  {
    if (tablero[fila, j]) return true;
  }
  // Diagonal noroeste (arriba a la izquierda)
  for (int i = fila - 1, j = columna - 1; i >= 0 && j >= 0; i--, j--)
  {
    if (tablero[i, j]) return true;
  }
  // Diagonal suroeste (abajo a la izquierda)
  for (int i = fila + 1, j = columna - 1; i < tablero.GetLength(0) && j >= 0; i++, j--)
  {
    if (tablero[i, j]) return true;
  }
  return false;
}

bool UbicaReinas(bool[,] tablero, int n)
{
  count++;
  // Condición de parada, cuando ya no queden reinas por ubicar
  if (n == 0) return true;

  //Quedan n reinas por ubicar
  int j = tablero.GetLength(0) - n;

  // Intentar ubicar una reina en la columna j
  for (int i = 0; i < tablero.GetLength(0); i++)
  {
    // Verificar si la reina en la ubicación no amenaza las anteriores
    if (!Amenaza(tablero, i, j))
    {
      // Intento de ubicar la reina en la celda i,j
      tablero[i, j] = true;

      //Visualizar el tablero
      //VisualizaTablero(tablero);
      //Console.WriteLine();
      //Console.ReadLine();

      // Ver si se puede ubicar las restantes reinas
      if (UbicaReinas(tablero, n - 1)) return true;

      // No se pudieron ubicar las restantes. Deshacer la ubicación que se hizo
      tablero[i, j] = false;
      //VisualizaTablero(tablero);
      //Console.WriteLine();
      //Console.ReadLine();
    }
  }
  return false; // No se pudieron ubicar las reinas en la columna j
}

#endregion

Stopwatch crono = new Stopwatch();
while (true)
{
  count = 0;
  Console.Write("\nEntre la cantidad de reinas ");
  string s = Console.ReadLine();
  if (s.Length == 0) break;
  int n = Int32.Parse(s);
  if (n <= 2) Console.WriteLine("Cantidad incorrecta");
  else
  {
    var tablero = new bool[n, n];
    for (int i = 0; i < n; i++)
      for (int j = 0; j < n; j++)
        tablero[i, j] = false;
    crono.Restart();
    var result = UbicaReinas(tablero, n);
    crono.Stop();
    if (result) 
      Console.WriteLine("Se ubicaron {0} reinas en {1} ms y {2} llamadas", n, crono.ElapsedMilliseconds, count);
    else 
      Console.WriteLine("No se pueden ubicar {0} reinas", n);
    Console.WriteLine("TABLERO FINAL"); 
    VisualizaTablero(tablero);
  }
}

