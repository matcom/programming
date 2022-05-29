using tictactoe.logic;


class Program
{
    static void Main(string[] args)
    {
        // MainHuman(args);
        MainAI(args);
    }

    static void MainHuman(string[] args)
    {
        while (true)
        {
            TicTacToe game = new TicTacToe();

            while (game.Winner() == Mark.None)
            {
                Console.Clear();
                Draw(game);
                (int row, int col) = ReadInput(game.Turn);

                if (game.CanPlay(row, col))
                    game.Play(row, col);
            }

            Console.Clear();
            Draw(game);

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write($"🥳 The winner is {game.Winner()}! Press any key...");

            Console.ReadKey();
            Console.Clear();
        }
    }

    static void MainAI(string[] args)
    {
        string mark = args[0];
        Mark player;

        if (mark == "X")
        {
            player = Mark.Cross;
        }
        else if (mark == "O")
        {
            player = Mark.Zero;
        }
        else
        {
            throw new ArgumentException("Choose one of X or O.");
        }

        while (true)
        {
            TicTacToe game = new TicTacToe();

            while (game.Winner() == Mark.None)
            {
                Console.Clear();
                Draw(game);
                int row, col;

                if (game.Turn == player)
                {
                    (row, col) = ReadInput(game.Turn);
                }
                else
                {
                    (row, col) = TicTacToeAI.BestMove(game);
                }

                if (game.CanPlay(row, col))
                    game.Play(row, col);
            }

            Console.Clear();
            Draw(game);

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write($"🥳 The winner is {game.Winner()}! Press any key...");

            Console.ReadKey();
            Console.Clear();
        }
    }

    static void Draw(TicTacToe game)
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                switch (game[r, c])
                {
                    case Mark.None:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("_");
                        break;
                    case Mark.Zero:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("O");
                        break;
                    case Mark.Cross:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("X");
                        break;
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }

    static (int, int) ReadInput(Mark turn)
    {
        switch (turn)
        {
            case Mark.Zero:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("O: ");
                break;
            case Mark.Cross:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("X: ");
                break;
        }

        Console.ForegroundColor = ConsoleColor.White;

        while (true)
        {
            try
            {
                string? line = Console.ReadLine();
                var numbers = line!.Split(' ', 2, StringSplitOptions.TrimEntries);
                return (int.Parse(numbers[0]), int.Parse(numbers[1]));
            }
            catch (Exception)
            {
                (int l, int t) = Console.GetCursorPosition();
                Console.SetCursorPosition(0, t);
                continue;
            }
        }
    }
}