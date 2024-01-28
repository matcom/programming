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
  }

  abstract class Figure
  {
    public abstract double Area { get; }
    public abstract double Perimeter { get; }
    public override string ToString()
    {
      return "I am a " + GetType().Name + ", Area " + Area
              + ", Perimeter " + Perimeter;
    }
  }

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
      TopLeft = new Point(x,y);
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
  }

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
      get { return 2*Math.PI*Radius; }
    }
    public override double Area
    {
      get { return Math.PI*Radius*Radius; }
    }
  }
  class Program_02
  {
    static void Main(string[] args)
    {
      Rectangle rect = new Rectangle(100, 200, 30, 40);
      Circle circ = new Circle(new Point(300, 300), 100);
      Console.WriteLine(rect);
      Console.WriteLine(circ);
    }
  }
}
#region EJERCICIOS CLASE PRACTICA
//Incorporar en la jerarquia clases de Figura y usando herencia donde venga mejor
//clases para definir
//Triangulo (por tres puntos), Cuadrado, Sector de circulo, Poligono
#endregion