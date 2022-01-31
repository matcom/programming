using npuzzle.logic;


class Program {
    static void Main(string[] args)
    {
        int size = int.Parse(args[0]);

        NPuzzle puzzle = new NPuzzle(size);

        while(true) {
            Console.Clear();
            Console.WriteLine(puzzle);

            switch(Console.ReadKey().Key) {
                case ConsoleKey.LeftArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Left);
                    break;
                case ConsoleKey.RightArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Right);
                    break;
                case ConsoleKey.UpArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Up);
                    break;
                case ConsoleKey.DownArrow:
                    puzzle = puzzle.Move(NPuzzle.Movement.Down);
                    break;
                case ConsoleKey.R:
                    puzzle = puzzle.Randomize();
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }
}