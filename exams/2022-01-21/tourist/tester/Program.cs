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

        // Salida esperada
        int[,] esperado =
        {
            {2,2,2,2,2},
            {2,1,1,1,2},
            {2,1,0,1,2},
            {2,1,1,1,2},
            {2,2,2,2,2},
        };

        PrintArray(esperado);

        int[,] respuesta = Senderismo.CalculaDistancias(mapa, posiciones);
        PrintArray(respuesta);
    }

    static void Case2()
    {
        // Entrada
        bool[,] mapa =
        {
            {false, false, false, false, false, false, false, false, false, false},
            {false, false, false, false, false, false, false, false, false, false},
            {true,  true,  true,  true,  false, false, true,  false, false, false},
            {false, false, false, true,  false, false, true,  false, false, false},
            {false, false, false, true,  false, false, true,  false, false, false},
            {false, false, false, true,  false, false, true,  false, true,  false},
            {false, false, false, true,  true,  true,  true,  false, true,  false},
            {false, false, false, false, false, false, false, false, true,  false},
        };
        int[,] posiciones = {{7,1}, {4,4}, {1,8}};

        // Salida esperada
        int[,] esperado =
        {
            {6,5,4,4,4,3,2,1,1,1},
            {6,5,4,3,3,3,2,1,0,1},
            {-10,-10,-10,-10,2,2,-10,1,1,1},
            {4,4,4,-10,1,1,-10,2,2,2},
            {3,3,3,-10,0,1,-10,3,3,3},
            {2,2,2,-10,1,1,-10,4,-10,4},
            {1,1,1,-10,-10,-10,-10,5,-10,5},
            {1,0,1,2,3,4,5,6,-10,6},
        };

        PrintArray(esperado);

        int[,] respuesta = Senderismo.CalculaDistancias(mapa, posiciones);
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
