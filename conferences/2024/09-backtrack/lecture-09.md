# Recursividad (Backtracking)

El backtracking es una técnica de búsqueda exhaustiva que se utiliza para encontrar todas las soluciones posibles para un problema, explorando de manera sistemática el espacio de soluciones. Se basa en la idea de construir soluciones de manera incremental y realizar retrocesos (backtracks) cuando se alcanzan caminos sin salida, lo que permite encontrar la solución óptima o todas las soluciones posibles.

A diferencia de los enfoques vistos en las clases anteriores, al utilizar backtrack cada llamado recursivo no necesariamente nos acerca más a la solución. O sea, existen llamados recursivos que no formarán parte de la solución final, cosa que no pasaba con los enfoques básicos o divide y vencerás.

## Estructura General

El algoritmo de backtracking sigue una estructura general recursiva:

1. **Definir la función de búsqueda**: Esta función recibe como parámetro el estado actual de la solución parcial y realiza una serie de pasos para construir la solución final.
2. **Evaluar condición de término**: Se verifica si la solución parcial es válida y si se ha alcanzado la solución final.
3. **Construir soluciones parciales**: Se generan todas las posibles extensiones de la solución parcial actual.
4. **Realizar retroceso (backtrack)**: Cuando no es posible extender más la solución parcial, se deshace la última elección y se retrocede a un estado anterior.
5. **Llamada recursiva**: Se llama a la función de búsqueda con la solución parcial actualizada.

## Ejemplos de Código

### Problema de las 8 reinas

```csharp
class NQueensProblem
    {
        public static bool Solve(int nQueens)
        {
            return PlaceQueen(new int[nQueens], 0);
        }

        static bool PlaceQueen(int[] queenColumnPerRow, int row)
        {
            if (row == queenColumnPerRow.Length)
            {
                return true;
            }
            for (int col = 0; col < queenColumnPerRow.Length; col++)
            {
                if (IsSafe(queenColumnPerRow, row, col))
                {
                    queenColumnPerRow[row] = col;
                    if (PlaceQueen(queenColumnPerRow, row + 1))
                        return true;
                }
            }
            return false;
        }

        static bool IsSafe(int[] queenColumnPerRow, int row, int col)
        {
            for (int prevRow = 0; prevRow < row; prevRow++)
            {
                int prevCol = queenColumnPerRow[prevRow];
                if (prevCol == col || Math.Abs(prevCol - col) == Math.Abs(prevRow - row))
                    return false;
            }
            return true;
        }
    }
```

### Salida del laberinto

```csharp
class MazeSolver {

    public static bool Solve(bool[,] maze, int x, int y, out bool[,] solution)
    {
        solution = new bool[maze.GetLength(0), maze.GetLength(1)];
        return SolveMaze(maze, x, y, solution);
    }

    static bool SolveMaze(bool[,] maze, int x, int y, bool[,] solution)
    {
        if (x < 0 || x >= maze.GetLength(0) || y < 0 || y >= maze.GetLength(1) || maze[x, y] || solution[x, y])
            return false;

        solution[x, y] = true;

        if (x == maze.GetLength(0) - 1 && y == maze.GetLength(1) - 1)
            return true;

        if (
            SolveMaze(maze, x + 1, y, solution) ||
            SolveMaze(maze, x, y + 1, solution) ||
            SolveMaze(maze, x - 1, y, solution) ||
            SolveMaze(maze, x, y - 1, solution)
        )
            return true;

        solution[x, y] = false;
        return false;
    }
}
```

## Consideraciones Generales

- **Complejidad temporal**: El backtracking puede tener una complejidad temporal exponencial en el peor de los casos, por lo que es importante implementar podas y optimizaciones para mejorar el rendimiento.
- **Uso eficiente de la memoria**: Es esencial optimizar el uso de la memoria, especialmente en problemas con un gran espacio de soluciones, para evitar desbordamientos de pila o agotamiento de recursos.
- **Identificación de soluciones parciales válidas**: Definir correctamente las condiciones para determinar si una solución parcial es válida resulta crucial para el funcionamiento correcto del algoritmo de backtracking.

## Consejos

- **Implementar podas (pruning)**: Identificar y eliminar ramas del árbol de búsqueda que no conducen a una solución válida puede reducir significativamente el tiempo de ejecución.
- **Ordenar las opciones de búsqueda**: En algunos casos, ordenar las opciones de búsqueda puede mejorar el rendimiento del algoritmo de backtracking al reducir el número de ramas exploradas.
