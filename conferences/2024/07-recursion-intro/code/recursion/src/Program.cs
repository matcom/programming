namespace MatCom.Programming
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(Fibonacci(5));
            Console.WriteLine(Factorial(10));
            Console.WriteLine(MoverTorresHanoi(3, 'A', 'C', 'B'));
            Console.WriteLine(RecursiveSum(5));
            Console.WriteLine(RecursiveMin(new[] { 10, 9, 8, 7, -1, 5, 4, 3, 2, 1 }));
            Console.WriteLine(TaxiDriver(10, 10));
        }


        static int Factorial(int n)
        {
            if (n == 0 || n == 1)
                return 1;

            return n * CalcularFactorial(n - 1);
        }

        static int Fibonacci(int n)
        {
            if (n <= 1)
                return n;

            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }

        static void MoverTorresHanoi(int n, char origen, char destino, char auxiliar)
        {
            if (n == 1)
            {
                Console.WriteLine($"Mover disco 1 desde {origen} hasta {destino}");
            }
            else
            {
                MoverTorresHanoi(n - 1, origen, auxiliar, destino);
                Console.WriteLine($"Mover disco {n} desde {origen} hasta {destino}");
                MoverTorresHanoi(n - 1, auxiliar, destino, origen);
            }
        }

        static int RecursiveSum(int n)
        {
            return n == 0 ? 0 : n + RecursiveSum(n - 1);
        }

        static int RecursiveMin(int[] array)
        {
            return RecursiveMin(array, 0);
        }

        static int RecursiveMin(int[] array, int start)
        {
            if (start == array.Length)
                return int.MaxValue;

            return Math.Min(array[start], RecursiveMin(array, start + 1));
        }

        static int TaxiDriver(int m, int n)
        {
            if (m == 1 || n == 1)
                return 1;

            return TaxiDriver(m - 1, n) + TaxiDriver(m, n - 1);
        }
    }
}
