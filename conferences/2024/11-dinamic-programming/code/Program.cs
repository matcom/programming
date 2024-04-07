using System.Diagnostics;

namespace MatCom.Programming
{
    class Fibonacci
    {
        public static int Solve(int n)
        {
            if (n <= 1)
                return 1;

            int[] fib = new int[n + 1];
            fib[0] = 1;
            fib[1] = 1;

            for (int i = 2; i <= n; i++)
            {
                fib[i] = fib[i - 1] + fib[i - 2];
            }

            return fib[n];
        }
    }
    class ProblemaDeLaMochila
    {
        public static int Combinatoria(int[] ganancia, int[] peso, int capacidad, int n)
        {
            return n == 0 ?
                0 :
                peso[n - 1] > capacidad ?
                Combinatoria(ganancia, peso, capacidad, n - 1) :
                Math.Max(
                    Combinatoria(ganancia, peso, capacidad, n - 1),
                    Combinatoria(ganancia, peso, capacidad - peso[n - 1], n - 1) + ganancia[n - 1]
                );
        }

        public static int DPMatriz(int[] ganancia, int[] peso, int capacidad, int n)
        {
            int[,] m = new int[n + 1, capacidad + 1];

            for (int c = 0; c <= capacidad; c++)
                m[0, c] = 0;

            for (int i = 1; i <= n; i++)
                for (int c = 0; c <= capacidad; c++)
                {
                    m[i, c] = c < peso[i - 1] ?
                        m[i - 1, c] :
                        Math.Max(
                            m[i - 1, c],
                            m[i - 1, c - peso[i - 1]] + ganancia[i - 1]

                        );
                }

            return m[n, capacidad];
        }

        public static int DP2Filas(int[] ganancia, int[] peso, int capacidad, int n)
        {
            int[] completado = new int[capacidad + 1];
            int[] actual = new int[capacidad + 1];
            int[] aux;

            for (int c = 0; c <= capacidad; c++)
                completado[c] = 0;

            for (int i = 1; i <= n; i++)
            {
                for (int c = capacidad; c >= 0; c--)
                {
                    if (c < peso[i - 1])
                        actual[c] = completado[c];

                    else
                        actual[c] = Math.Max(
                            completado[c],
                            completado[c - peso[i - 1]] + ganancia[i - 1]
                        );
                }
                aux = completado;
                completado = actual;
                actual = aux;
            }

            return completado[capacidad];
        }

        public static int DP1Fila(int[] ganancia, int[] peso, int capacidad, int n)
        {
            int[] best = new int[capacidad + 1];

            for (int c = 0; c <= capacidad; c++)
                best[c] = 0;

            for (int i = 1; i <= n; i++)
                for (int c = capacidad; c >= 0; c--)
                {
                    if (c < peso[i - 1])
                        continue;

                    best[c] = Math.Max(
                        best[c],
                        best[c - peso[i - 1]] + ganancia[i - 1]
                    );
                }
            return best[capacidad];
        }
    }

    class Program
    {
        static void Evaluate(int[] ganancia, int[] peso, int capacidad, int n)
        {
            Stopwatch stopwatch = new Stopwatch();
            int best;

            Console.WriteLine("-----------------------------");

            stopwatch.Start();
            best = ProblemaDeLaMochila.Combinatoria(ganancia, peso, capacidad, n);
            stopwatch.Stop();
            Console.WriteLine($"Gain: {best}");
            Console.WriteLine($"Execution time: {stopwatch.Elapsed.TotalSeconds} seconds");

            stopwatch.Restart();
            best = ProblemaDeLaMochila.DPMatriz(ganancia, peso, capacidad, n);
            stopwatch.Stop();
            Console.WriteLine($"Gain: {best}");
            Console.WriteLine($"Execution time: {stopwatch.Elapsed.TotalSeconds} seconds");

            stopwatch.Restart();
            best = ProblemaDeLaMochila.DP2Filas(ganancia, peso, capacidad, n);
            stopwatch.Stop();
            Console.WriteLine($"Gain: {best}");
            Console.WriteLine($"Execution time: {stopwatch.Elapsed.TotalSeconds} seconds");

            stopwatch.Restart();
            best = ProblemaDeLaMochila.DP1Fila(ganancia, peso, capacidad, n);
            stopwatch.Stop();
            Console.WriteLine($"Gain: {best}");
            Console.WriteLine($"Execution time: {stopwatch.Elapsed.TotalSeconds} seconds");

            Console.WriteLine("-----------------------------");
        }
        static void Main(string[] args)
        {
            Console.WriteLine(Fibonacci.Solve(5));

            Evaluate(
                ganancia: new int[] { 92, 57, 49, 68, 60, 43, 67, 84, 87, 72, },
                peso: new int[] { 23, 31, 29, 44, 53, 38, 63, 85, 89, 82 },
                capacidad: 165,
                n: 10
            );

            Evaluate(
                ganancia: new int[] { 15, 25, 35, 45, 55, 65, 75, 85, 95, 105, 115, 125, 135, 145, 155, 165, 175, 185, 195, 205, 215, 225, 235, 245, 255, 265, 275, 285, 295, 305 } ,
                peso: new int [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 },
                capacidad: 100,
                n: 30
            );
        }
    }
}
