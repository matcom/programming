using Weboo.Examen;


class Program
{
    static void Main()
    {
        Case1();
        Case2();
    }

    static void Case1()
    {
        // Entrada
        bool[,] mapa = new bool[5,5];
        int[,] posiciones = {{2,2}};
        int max = 3;

        // Salida esperada
        int[,] esperado =
        {
            {1,1,1,1,1},
            {1,2,2,2,1},
            {1,2,3,2,1},
            {1,2,2,2,1},
            {1,1,1,1,1},
        };

        PrintArray(esperado);

        int[,] respuesta = Wifi.IntensidadDeSeñal(mapa, posiciones, max);
        PrintArray(respuesta);
    }

    static void Case2()
    {
        // Entrada
        bool[,] mapa =
        {
            {false, false, false, false, false, false, false, false},
            {true,  true,  true,  false, false, false, false, false},
            {false, false, true,  false, true,  true,  true,  true},
            {false, false, true,  false, true,  false, false, false},
            {false, false, true,  false, true,  false, false, false},
            {false, false, false, false, true,  true,  true,  false},
            {false, false, false, false, false, false, false, false},
        };
        int[,] posiciones = {{2,1}, {4,6}};
        int max = 10;

        // Salida esperada
        int[,] esperado =
        {
            {0,1,2,2,2,2,1,0},
            {-10,-10,-10,4,4,2,1,0},
            {10,10,-10,6,-10,-10,-10,-10},
            {11,11,-10,8,-10,9,9,9},
            {10,11,-10,10,-10,9,10,10},
            {9,10,11,11,-10,-10,-10,11},
            {8,9,10,11,11,11,11,10},
        };

        PrintArray(esperado);

        int[,] respuesta = Wifi.IntensidadDeSeñal(mapa, posiciones, max);
        PrintArray(respuesta);
    }

    static void PrintArray(int[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
                Console.Write(array[i, j].ToString().PadLeft(5));

            Console.WriteLine();
        }
    }
}
