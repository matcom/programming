namespace MatCom.Programming
{
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
                PrintSolution(queenColumnPerRow);
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

        static void PrintSolution(int[] queenColumnPerRow)
        {
            for (int i = 0; i < queenColumnPerRow.Length; i++)
            {
                Console.Write("|");
                for (int j = 0; j < queenColumnPerRow.Length; j++)
                {
                    if (queenColumnPerRow[i] == j)
                        Console.Write("♛ |");
                    else
                        Console.Write("  |");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }

    class MazeSolver
    {
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

        public static void PrintSolution(bool[,] maze, bool[,] solution)
        {
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                Console.Write("| ");
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    string symbol = maze[i, j] ? "⬛" : solution[i, j] ? "ꆜ" : "ㅤ";
                    Console.Write(symbol + " ");
                }
                Console.WriteLine("|");
            }
        }
    }

    class Program
    {


        static void Main(string[] args)
        {
            NQueensProblem.Solve(8);

            bool[,] maze = new bool[,] {
                { false, true, false, false, false },
                { false, false, false, true, false },
                { true, true, true, false, false },
                { true, true, true, false, true },
                { false, false, false, false, false }
            };
            bool[,] solution;

            if (MazeSolver.Solve(maze, 0, 0, out solution))
            {
                MazeSolver.PrintSolution(maze, solution);
            }
            else
            {
                Console.WriteLine("No solution exists.");
            }
        }


    }
}
