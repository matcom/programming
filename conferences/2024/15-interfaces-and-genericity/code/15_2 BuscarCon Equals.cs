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
        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
        //Descomentar para que funcione el Equals al buscar
        //public override bool Equals(object? obj)
        //{
        //  if (!(obj is Point)) return false;//Un punto no es igual a otra cosa que no sea punto
        //  Point p = (Point)obj;
        //  return X == p.X && Y == p.Y; //Son iguales si sus X y Y son iguales
        //}
    }//Point

    internal class Program02

    {
        //TRES IMPLEMENTACIONES DE BUSCART UNA POR CADA TIPO. CADA UNO USANDO Equals
        static int Buscar(int x, int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(x)) return i;
            }
            return -1;
        }

        static int Buscar(string x, string[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(x)) return i;
            }
            return -1;
        }

        static int Buscar(Point x, Point[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(x)) return i;
                //El mismo problema se aplica el Equals que se hereda de object
                //que tiene la misma semantica de comparar las referencias
                //es decir son iguales si es el mismo objeto

                //Para que funcione implementar Equals en Point
            }
            return -1;
        }

        static void Main(string[] args)
        {
            int[] nums = new int[] { 100, 20, 80, 40, 60 };
            string[] colores = new string[] { "rojo", "azul", "blanco", "negro" };
            Point p = new Point(300, 40);
            Point[] puntos = new Point[]
              {
          new Point(10,20),
          p,
          new Point(500,60)
              };
            Console.WriteLine("Array de enteros es");
            foreach (int k in nums)
                Console.WriteLine(k);
            Console.WriteLine("80 esta en la posicion {0}", Buscar(80, nums));
            Console.WriteLine("77 esta en la posicion {0}", Buscar(77, nums));

            Console.WriteLine("\nArray de colores es");
            foreach (string s in colores)
                Console.WriteLine(s);
            Console.WriteLine("blanco esta en la posicion {0}", Buscar("blanco", colores));
            Console.WriteLine("verde esta en la posicion {0}", Buscar("verde", colores));
            string s1 = "az";
            string s2 = "ul";
            Console.WriteLine("{0} esta en la posicion {1}",
                              s1 + s2, Buscar(s1 + s2, colores));

            Console.WriteLine("\nArray de puntos es");
            foreach (Point q in puntos)
                Console.WriteLine(q);
            Console.WriteLine("{0} esta en la posicion {1}",
                              p, Buscar(p, puntos));
            Point p1 = new Point(333, 444);
            Console.WriteLine("{0} esta en la posicion {1}",
                        p1, Buscar(p1, puntos));
            Point p2 = new Point(500, 60);
            Console.WriteLine("{0} esta en la posicion {1}",
                        p2, Buscar(p2, puntos));
        }
    }
}