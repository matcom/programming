using npuzzle.logic;


class Program
{
    static void Main(string[] args)
    {
        int size = int.Parse(args[0]);
        int movements = 0;
        NPuzzle puzzle = new NPuzzle(size);

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(movements);

            Draw(puzzle);

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.LeftArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Left);
                    movements++;
                    break;
                case ConsoleKey.RightArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Right);
                    movements++;
                    break;
                case ConsoleKey.UpArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Up);
                    movements++;
                    break;
                case ConsoleKey.DownArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Down);
                    movements++;
                    break;
                case ConsoleKey.R:
                    puzzle = puzzle.Randomize();
                    movements = 0;
                    break;
                case ConsoleKey.S:
                    var steps = NPuzzleSolver.Solve(puzzle);

                    foreach(var step in steps) {
                        puzzle = puzzle.Move(step);
                        movements += 1;
                        Thread.Sleep(100);
                    }

                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    public static void Draw(NPuzzle puzzle)
    {
        int pad = (puzzle.Size * puzzle.Size - 1).ToString().Length + 1;

        for (int i = 0; i < puzzle.Size; i++)
        {
            for (int j = 0; j < puzzle.Size; j++)
            {
                if (puzzle[i, j] == 0)
                {
                    Console.Write(String.Empty.PadLeft(pad));
                }
                else
                {
                    if (puzzle[i, j] == puzzle.Size * i + j)
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkRed;

                    Console.Write(puzzle[i, j].ToString().PadLeft(pad));
                }
            }

            Console.WriteLine();
        }
    }
}