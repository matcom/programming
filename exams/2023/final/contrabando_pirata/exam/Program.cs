int GetRoutes(Map map, int n)
{
    // Elimine la siguiente línea y ponga aquí su código
    throw new NotImplementedException();
}

int[,] matrix = new int[,]{{0, 1, 1, 3},
                           {1, 0, 3, 1},
                           {1, 3, 0, 1},
                           {3, 1, 1, 0}};

int[] A = new int[]{1, 2};
int[] B = new int[]{3};
Map map = new Map(matrix, A, B);
//Esto debe tener costo 7
Console.WriteLine(GetRoutes(map, 2));