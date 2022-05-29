namespace Rogue;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();
            PrintMenu();

            ConsoleKey key = Console.ReadKey(true).Key;
            Console.Clear();

            switch (key)
            {
                case ConsoleKey.N:
                    NewGame();
                    break;
                case ConsoleKey.Q:
                    return;
            }
        }
    }

    static void NewGame()
    {
        Game game = new Game(35, 18);

        while (true)
        {
            Console.Clear();
            DrawBoard(game);
            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    game.MovePlayer(Direction.Up);
                    break;
                case ConsoleKey.DownArrow:
                    game.MovePlayer(Direction.Down);
                    break;
                case ConsoleKey.LeftArrow:
                    game.MovePlayer(Direction.Left);
                    break;
                case ConsoleKey.RightArrow:
                    game.MovePlayer(Direction.Right);
                    break;
                case ConsoleKey.Spacebar:
                    game.Attack();
                    break;
                case ConsoleKey.Q:
                    return;
            }

            game.Update();

            if (game.Lives == 0)
            {
                GameOver();
                break;
            }

            if (game.CountEnemies() == 0)
            {
                WinGame();
                break;
            }
        }
    }

    static void GameOver()
    {
        string message = @"
                                           ▄▄   ▄▄                 ▄▄
▀███▀   ▀██▀                             ▀███   ██               ▀███ ██
  ███   ▄█                                 ██                      ██ ██
   ███ ▄█    ▄██▀██▄▀███  ▀███        ▄█▀▀███ ▀███   ▄▄█▀██   ▄█▀▀███ ██
    █▓██    ██▀   ▀██ ██    ██      ▄██    ██   ██  ▄█▀   ██▄██    ██ █▓
     ▓█     ██     ██ ▓█    ██      █▓█    █▓   ▓█  ▓█▀▀▀▀▀▀█▓█    █▓ ▀▓
     ▓█     ██     ▓█ ▓█    █▓      ▀▓█    █▓   ▓█  ▓█▄    ▄▀▓█    █▓
     ▓█     ▓█     ▓▓ ▓█    ▓▓      ▓▓▓    ▓▓   ▓▓  ▓▓▀▀▀▀▀▀▓▓▓    ▓▓
     ▓▓     ▓▓▓   ▓▓▓ ▓▓    ▓▓      ▀▒▓    ▓▒   ▓▓  ▒▓▓     ▀▒▓    ▓▒ ▓▓
   ▒ ▒▒▒     ▒ ▒ ▒ ▒  ▒▒ ▓▒ ▒▓▒      ▒ ▒ ▒ ▓ ▒▒ ▒ ▒  ▒ ▒ ▒▒  ▒ ▒ ▒ ▓ ▒▒▒


        ";

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Press any key...");
        Console.ReadKey(true);
    }

    static void WinGame()
    {
        string message = @"

▀███▀   ▀██▀                                                            ██
  ███   ▄█                                                              ██
   ███ ▄█    ▄██▀██▄▀███  ▀███     ▀██▀    ▄█    ▀██▀ ▄██▀██▄▀████████▄ ██
    █▓██    ██▀   ▀██ ██    ██       ██   ▄███   ▄█  ██▀   ▀██ ██    ██ █▓
     ▓█     ██     ██ ▓█    ██        ██ ▄█  ██ ▄█   ██     ██ █▓    ██ ▀▓
     ▓█     ██     ▓█ ▓█    █▓         ███    █▓▓    ██     ▓█ █▓    ▓█
     ▓█     ▓█     ▓▓ ▓█    ▓▓         ▓█▓▓   ▓▒▓    ▓█     ▓▓ ▓▓    ▓▓
     ▓▓     ▓▓▓   ▓▓▓ ▓▓    ▓▓         ▓▓▓    ▓▒▓    ▓▓▓   ▓▓▓ ▓▓    ▓▓ ▓▓
   ▒ ▒▒▒     ▒ ▒ ▒ ▒  ▒▒ ▓▒ ▒▓▒         ▒      ▒      ▒ ▒ ▒ ▒▒ ▒▒▒  ▒▓▒ ▒▒


        ";

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Press any key...");
        Console.ReadKey(true);
    }

    static void DrawBoard(Game game)
    {
        for (int i = 0; i < game.Lives; i++)
        {
            Console.Write("💖");
        }

        Console.WriteLine("\n");

        for (int row = 0; row < game.Height; row++)
        {
            for (int col = 0; col < game.Width; col++)
            {
                if (game.PlayerCol == col && game.PlayerRow == row)
                {
                    Console.Write("🧍");
                    continue;
                }

                switch (game.ObjectAt(col, row))
                {
                    case GameObject.Floor:
                        Console.Write("  ");
                        break;
                    case GameObject.Wall:
                        Console.Write("🧱");
                        break;
                    case GameObject.Enemy:
                        Console.Write("🕷️ ");
                        break;
                    case GameObject.Corpse:
                        Console.Write("☠️ ");
                        break;
                    case GameObject.Trap:
                        Console.Write("🕸️ ");
                        break;
                    case GameObject.Life:
                        Console.Write("💝");
                        break;
                }
            }
            Console.WriteLine();
        }
    }

    static void PrintMenu()
    {
        string banner = @"
 ██▀███   ▒█████   ▄████  █    ██  ▓█████
▓██ ▒ ██▒▒██▒  ██▒ ██▒ ▀█ ██  ▓██▒ ▓█   ▀
▓██ ░▄█ ▒▒██░  ██▒▒██░▄▄▄▓██  ▒██░ ▒███
▒██▀▀█▄  ▒██   ██░░▓█  ██▓▓█  ░██░ ▒▓█  ▄
░██▓ ▒██▒░ ████▓▒░▒▓███▀▒▒▒█████▓ ▒░▒████
░ ▒▓ ░▒▓░░ ▒░▒░▒░ ░▒   ▒ ░▒▓▒ ▒ ▒ ░░░ ▒░
  ░▒ ░ ▒░  ░ ▒ ▒░  ░   ░ ░░▒░ ░ ░ ░ ░ ░
   ░   ░ ░ ░ ░ ▒   ░   ░  ░░░ ░ ░     ░
   ░         ░ ░       ░    ░     ░   ░
        ";

        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine(banner);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⭐ [N]ew Game");
        Console.WriteLine("🚪 [Q]uit");
    }
}