// See https://aka.ms/new-console-template for more information


using System.Reflection.Metadata.Ecma335;

#region LABERINTOs

Celda[,] lab = 
{
  {Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall},
  {Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Free, Celda.Free},
  {Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free},
  {Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Wall, Celda.Free, Celda.Free, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free, Celda.Wall, Celda.Free, Celda.Wall, Celda.Wall, Celda.Wall, Celda.Free},
  {Celda.Free, Celda.Wall, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free, Celda.Free}
};
#endregion

bool PosValida(Celda[,] b, int fila, int columna)
{
  //Para controlar no salirnos de los rangos del array
  return fila >= 0 && fila < b.GetLength(0) && columna >= 0 && columna < b.GetLength(1);
}

void VisualizaLaberinto(Celda[,] lab, out int longSalida)
{
  longSalida = 0;
  for (int i = 0; i < lab.GetLength(0); i++)
  {
    for (int j = 0; j < lab.GetLength(1); j++)
    {
      if (lab[i, j] == Celda.Wall)
      {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.Write("  ");
        Console.BackgroundColor = ConsoleColor.Black;
      }
      else if (lab[i, j] == Celda.Free)
      {
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("  ");
        Console.BackgroundColor = ConsoleColor.Black;
      }
      else if (lab[i, j] == Celda.Exit)
      {
        longSalida++;
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.Write("  ");
        Console.BackgroundColor = ConsoleColor.Black;
      }
      else
      {
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("  ");
        Console.BackgroundColor = ConsoleColor.Black;
      }
    }
    Console.WriteLine();
  }
}

int count = 0;
bool HaySalida(Celda[,] lab, int i, int j,int m, int n)
{
  count++;
  if (!PosValida(lab, i, j)) return false;
  if (lab[i, j] == Celda.Free)
  {
    if ((i == m) && (j == n))
    {
      lab[i, j] = Celda.Exit;
      return true;
    }
    lab[i, j] = Celda.Pass;
    //Descomentar si se quiere visualizar paso a paso
    //VisualizaLaberinto(lab);
    //Console.ReadLine();
    if (HaySalida(lab, i, j + 1, m, n) ||
        HaySalida(lab, i + 1, j, m, n) ||
        HaySalida(lab, i, j - 1, m, n) ||
        HaySalida(lab, i - 1, j, m, n)) 
    {
      lab[i, j] = Celda.Exit;
      //VisualizaLaberinto(lab);
      //Console.ReadLine();
      return true;
    }
    return false;
    //Si llega a aqui se queda conque ya paso por el i,j
  }
  else return false; //La celda no esta Free (hay Wall o ya paso por aqui
}

int salida;
VisualizaLaberinto(lab, out salida);
//var result = HaySalida(lab, 0, 0, lab.GetLength(0) - 1, lab.GetLength(0) - 1);
var result = HaySalida(lab, 0, 0, lab.GetLength(0) - 1, lab.GetLength(0) - 13);
Console.WriteLine();
VisualizaLaberinto(lab, out salida);
if (result) Console.WriteLine("\nHAY SALIDA en {0} pasos", salida);
else Console.WriteLine("\nNO HAY SALIDA");
Console.WriteLine("Total de pasos {0}", count);

enum Celda { Wall, Free, Exit, Pass };



#region CP IMPLEMENTAR BUSCAR EL CAMINO MAS CORTO
//TO DO
#endregion

