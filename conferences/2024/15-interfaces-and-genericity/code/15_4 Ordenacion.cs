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
            return GetType().Name + ", Area " + Area
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

    class Program04
    {
        //TRES METODOS CON CODIGO SIMILAR
        static void Ordenar(int[] a)
        {
            if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
            for (int k = 0; k < a.Length - 1; k++)
                for (int j = k + 1; j < a.Length; j++)
                {
                    //if (a[j] < a[k])
                    //Comentar este y descomentar abajo  para probar que CompareTo tambien aplica a int
                    if (a[j].CompareTo(a[k]) < 0)
                    {
                        int temp = a[j];
                        a[j] = a[k];
                        a[k] = temp;
                    }
                }
        }
        static void Ordenar(string[] a)
        {
            if (a == null) throw new Exception("Parámetro no puede ser null"); //Este control no tiene por que incluirse dentro de la funcion recursiva para que no se repita
            for (int k = 0; k < a.Length - 1; k++)
                for (int j = k + 1; j < a.Length; j++)
                {
                    if (a[j].CompareTo(a[k]) < 0)
                    {
                        string temp = a[j];
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
                    if (a[j].Area < a[k].Area)
                    {
                        Figure temp = a[j];
                        a[j] = a[k];
                        a[k] = temp;
                    }
                }
        }

        static void Main(string[] args)
        {
            #region PROBANDO EL CompareTo DE STRING
            //string s1 = "rojo";
            //string s2 = "azul";
            //string s3 = "negro";
            //Console.WriteLine(s1.CompareTo(s2));
            //Console.WriteLine(s3.CompareTo(s1));
            //Console.WriteLine(s1.CompareTo("rojo"));
            #endregion

            #region ORDENANDO DISTINTOS TIPOS DE ARRAY
            var numeros = new int[] { 10, 5, 8, 4, 7 };
            Console.WriteLine("Array de numeros es");
            foreach (int k in numeros)
                //Si queremos recorrer todo el array para hacer algo con cada elemento y nada con el indice
                //no tenemos necesidad de usar un for int i=0 .....
                Console.WriteLine(k);
            Console.WriteLine("\nOrdenando el array de numeros...");
            Ordenar(numeros);
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
            Console.WriteLine("\nArray de figuras es");
            foreach (Figure f in figs)
                Console.WriteLine(f);
            Console.WriteLine("\nOrdenando el array de figuras...");
            Ordenar(figs);
            Console.WriteLine("Array ordenado de figuras es");
            foreach (Figure f in figs)
                Console.WriteLine(f);

            var colores = new string[] { "blanco", "azul", "rojo", "negro" };
            Console.WriteLine("\nArray de string es");
            foreach (string s in colores)
                Console.WriteLine(s);
            Console.WriteLine("\nOrdenando el array de string...");
            Ordenar(colores);
            Console.WriteLine("Array ordenado de string es");
            foreach (string s in colores)
                Console.WriteLine(s);
        }
        #endregion
    }
}
