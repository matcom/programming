int GetRoutes(Map map, int n)
{
    // Elimine la siguiente línea y ponga aquí su código
    throw new NotImplementedException();
}


int[,] matrix = new int[,]{{0, 1, 1, 3},
                           {1, 0, 3, 1},
                           {1, 3, 0, 5},
                           {3, 1, 5, 0}};

int[] start = new int[]{0, 3, 1, 1};
int[] end = new int[]{0, 7, 2, 4};
Map map = new Map(matrix, start, end);
//Esto debe tener costo 7
Console.WriteLine(GetRoutes(map, 2));