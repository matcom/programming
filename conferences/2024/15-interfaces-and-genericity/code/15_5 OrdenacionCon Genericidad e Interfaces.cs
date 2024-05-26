using System.Diagnostics;

namespace Programacion
{
    #region FIGURAS
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

    abstract class Figure : IComparable<Figure>
    {
        public abstract double Area { get; }
        public abstract double Perimeter { get; }
        public override string ToString()
        {
            return GetType().Name + ", Area " + Area
                    + " and Perimeter " + Perimeter;
        }
        public int CompareTo(Figure f)
        {
            return this.Area.CompareTo(f.Area);
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
    #endregion

    class Program05
    {
        //UN UUNICO METODO PARA Ordenar
        //Ver la interface IComparable<T>
        static void Ordenar<T>(T[] a) where T : IComparable<T>
        {
            if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
            for (int k = 0; k < a.Length - 1; k++)
                for (int j = k + 1; j < a.Length; j++)
                {
                    if (a[j].CompareTo(a[k]) < 0)
                    {
                        T temp = a[j];
                        a[j] = a[k];
                        a[k] = temp;
                    }
                }
        }
        static void Main(string[] args)
        {
            #region ORDENANDO DISTINTOS TIPOS DE ARRAY
            var numeros = new int[] { 10, 5, 8, 4, 7 };
            Console.WriteLine("Mi array de numeros es");
            foreach (int k in numeros)
                //Si queremos recorrer todo el array para hacer algo con cada elemento y nada con el indice
                //no tenemos necesidad de usar un for int i=0 .....
                Console.WriteLine(k);
            Console.WriteLine("\nOrdenando el array de numeros...");
            Ordenar<int>(numeros);
            Console.WriteLine("Array ordenado de numeros es");
            foreach (int k in numeros)
                Console.WriteLine(k);

            var figs = new Figure[]
            {
        new Rectangle(100, 200, 300, 40),
        new Circle(new Point(300, 300), 100),
        new Rectangle(500, 600, 50, 40),
        new Circle(new Point(400, 300), 200)
            };
            Console.WriteLine("Array Figure es");
            foreach (Figure f in figs)
                Console.WriteLine(f);
            Console.WriteLine("\nOrdenando el array de figuras...");
            Ordenar<Figure>(figs);
            Console.WriteLine("Array ordenado de figuras es");
            foreach (Figure f in figs)
                Console.WriteLine(f);

            var colores = new string[] { "blanco", "azul", "rojo", "negro" };
            Console.WriteLine("\nArray de string es");
            foreach (string s in colores)
                Console.WriteLine(s);
            Console.WriteLine("\nOrdenando el array de string...");
            Ordenar<string>(colores);
            Console.WriteLine("Array ordenado de string es");
            foreach (string s in colores)
                Console.WriteLine(s);
        }
        #endregion
    }
}