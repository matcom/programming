// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

namespace Programacion
{
  class Point
  {
    public int X
    {
      get;
      private set;
    }
    public int Y
    {
      get;
      private set;
    }
    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }
    public void Move(int plusX, int plusY)
    {
      this.X += plusX;
      this.Y += plusY;
    }
  }//Point

  abstract class Figure
  {
    public abstract double Area { get; }
    public abstract double Perimeter { get; }
    public override string ToString()
    {
      return "I am a " + GetType().Name + ", Area " + Area
              + " and Perimeter " + Perimeter;
    }
  }//Figure

  class Rectangle : Figure
  {
    public int High
    {
      get; private set;
    }
    public int Width
    {
      get; private set;
    }
    public Point TopLeft
    {
      get; private set;
    }

    //Dos sobrecargas del constructor para 
    public Rectangle(Point p, int high, int width)
    {
      TopLeft = p;
      Width = width;
      High = high;
    }
    public Rectangle(int x, int y, int high, int width)
    {
      TopLeft = new Point(x, y);
      Width = width;
      High = high;
    }
    public override double Perimeter
    {
      get { return (High + Width) * 2; }
    }
    public override double Area
    {
      get { return High * Width; }
    }
  }//Rectangle

  class Circle : Figure
  {
    public Point Center
    {
      get; private set;
    }
    public double Radius
    {
      get; private set;
    }
    public Circle(Point center, double r)
    {
      Center = center;
      Radius = r;
    }
    public Circle(int x, int y, double r)
    {
      Center = new Point(x, y);
      Radius = r;
    }
    public override double Perimeter
    {
      get { return 2 * Math.PI * Radius; }
    }
    public override double Area
    {
      get { return Math.PI * Radius * Radius; }
    }
  }//Circle

  class Program_03
  {
    static void OrdenarMinimosSucesivos(int[] a)
    {
      if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
      for (int k = 0; k < a.Length - 1; k++)
        for (int j = k + 1; j < a.Length; j++)
        {
          if (a[j] < a[k])
          {
            //intercambiar a[j] con a[k]
            int temp = a[j];
            a[j] = a[k];
            a[k] = temp;
          }
        }
    }
    static void Ordenar(Figure[] a)
    {
      if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
      for (int k = 0; k < a.Length - 1; k++)
        for (int j = k + 1; j < a.Length; j++)
        {
          //if (a[j] < a[k]) //No existe la comparacion de menor para figuras
          if (a[j].Area < a[k].Area) 
          //Crea dependencia entre el metodo de ordenacion y el criterio por el que se compara
          {
            Figure temp = a[j];
            a[j] = a[k];
            a[k] = temp;
          }
        }
    }
    static void Main(string[] args)
    {
      var figs = new Figure[]
      {
        new Rectangle(100, 200, 300, 40),
        new Circle(new Point(300, 300), 100),
        new Rectangle(500, 600, 50, 40),
        new Circle(new Point(400, 300), 200)
      };
      Console.WriteLine("Mi array de figuras es");
      foreach (Figure f in figs) 
        //Si queremos recorrer todo el array para hacer algo con cada elemento y nada con el indice
        //no tenemos necesidad de usar un for int i=0 .....
        Console.WriteLine(f);
      Console.WriteLine("\nOrdenando el array de figuras");
      Ordenar(figs);
      Console.WriteLine("Mi array ordenado de figuras es");
      foreach (Figure f in figs)
        Console.WriteLine(f);
    }
  }
}

#region PREGUNTAS CLASE PRACTICA
//Si quiero ordenar por el perimetro que tengo que hacer
//Si quiero que puedan haber triangulos en el array que tengo que hacer
//Que limitaciones le encuentra a este enfoque?
//1)Donde debe estar el codigo de ordenar?
//2)Hay que replicar este codigo para los distintos tipos de los elementos del array?
//2)Donde debe estar el criterio por el cual se hace la comparacion para ordenar  


#endregion