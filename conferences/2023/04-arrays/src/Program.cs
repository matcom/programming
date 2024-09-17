using System.IO;
using System;

class Program
{
    static void Main()
    {
        Console.Title = "☠️ Ahorcado v1.0";

        while (true)
        {
            Console.Clear();
            PrintMenu();

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.N:
                case ConsoleKey.Enter:
                    NewGame();
                    break;
                case ConsoleKey.P:
                    ShowWords();
                    break;
                case ConsoleKey.S:
                case ConsoleKey.Escape:
                    return;
                default:
                    break;
            }
        }
    }

    static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Bienvenido al Ahorcado ☠️");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Presione una tecla para continuar...");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[N]uevo juego");
        Console.WriteLine("[P]alabras");
        Console.WriteLine("[S]alir");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;

    }

    static void NewGame()
    {
        string word = GetRandomWord();
        bool[] marks = InitializeMarks(word);

        int lives = 5;
        int hints = 3;

        while (true)
        {
            Console.Clear();

            PrintInfo(lives, hints);
            PrintWord(word, marks);

            Console.WriteLine("\n ... Presione una tecla para jugar, [?] para hint, ESCAPE para salir ...");

            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
            {
                return;
            }

            if (key.KeyChar == '?')
            {
                if (hints > 0)
                {
                    GiveHint(word, marks);
                    hints -= 1;
                }
            }

            else
            {
                if (!Reveal(word, marks, key.KeyChar))
                {
                    lives -= 1;
                }
            }

            if (Complete(marks))
            {
                Win(word);
                break;
            }

            if (lives == 0)
            {
                GameOver(word);
                break;
            }
        }
    }

    static void Win(string word)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🥳 Has ganado!");
        PrintFinalWord(word);
    }

    static void GameOver(string word)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("☠️ Has perdido!");
        Console.ForegroundColor = ConsoleColor.White;
        PrintFinalWord(word);
    }
    static void PrintFinalWord(string word)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("La palabra era ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(word);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("\n ... Presiona ENTER para volver al menú ...");
        Console.ReadLine();
    }

    static string GetRandomWord()
    {
        string[] words = LoadWords();
        Random r = new Random();
        return words[r.Next(words.Length)];
    }

    static string[] LoadWords()
    {
        StreamReader reader = new StreamReader("words.txt");
        string[] words = new string[30];

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = reader.ReadLine()!;
        }

        return words;
    }

    static bool[] InitializeMarks(string word)
    {
        bool[] marks = new bool[word.Length];

        for (int i = 0; i < word.Length; i++)
        {
            if (word[i] == ' ')
            {
                marks[i] = true;
            }
            else
            {
                marks[i] = false;
            }
        }

        return marks;
    }

    static void PrintInfo(int lives, int hints)
    {
        for (int i = 0; i < lives; i++)
        {
            Console.Write("❤️ ");
        }

        Console.Write(" ");

        for (int i = 0; i < hints; i++)
        {
            Console.Write("❔");
        }

        Console.WriteLine("\n");
    }

    static void PrintWord(string word, bool[] marks)
    {
        Console.ForegroundColor = ConsoleColor.White;

        for (int i = 0; i < marks.Length; i++)
        {
            if (marks[i])
            {
                Console.Write(word[i]);
            }
            else
            {
                Console.Write('*');
            }
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    static void GiveHint(string word, bool[] marks)
    {
        Random r = new Random();

        while (true)
        {
            // Buscamos una posición aleatoria en la palabra
            int pos = r.Next(word.Length);

            // Si ya está marcada, probamos de nuevo
            if (marks[pos])
            {
                continue;
            }

            // De lo contrario, la  marcamos y salimos
            marks[pos] = true;
            break;
        }
    }

    static bool Reveal(string word, bool[] marks, char c)
    {
        bool found = false;

        for (int i = 0; i < word.Length; i++)
        {
            // Por cada letra, si la letra no está marcada ya, y coincide con el caracter c
            // entonces la marcamos
            if (!marks[i] && word[i] == c)
            {
                marks[i] = true;
                found = true;
            }
        }

        // Esto será true si y solo si alguna letra nueva fue marcada
        return found;
    }

    static bool Complete(bool[] marks)
    {
        for (int i = 0; i < marks.Length; i++)
        {
            // Si al menos una letra no está marcada, ya se sabe que no está terminado el juego.
            if (!marks[i])
            {
                return false;
            }
        }

        // Si llegamos aquí, es porque todas las letras estaban marcadas, verdad?
        return true;
    }

    static void ShowWords()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        string[] words = LoadWords();

        foreach (string word in words)
        {
            Console.WriteLine(word);
        }

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("\n ... Presione ENTER para volver al menú ...");
        Console.ReadLine();
    }
}

// EJERCICIOS
// ==========

// 1 - Modifique GiveHint para en vez de revelar una sola letra, revele todas las posiciones con esa letra aleatoria
//     Hint: reutilice los métodos que ya están implementados.

// 2 - Modifique la aplicación para que al terminar una partida:
// 2.1 - Pregunte si quieres continuar jugando y escoja una nueva palabra siguiendo con las mismas vidas y hints. Adicione una nueva vida (hasta 5) cada vez que ganes.
// 2.2 - Vaya acumulando una puntuación que será igual a Vidas * Longitud cada vez que ganes. Al perder (o decidir no seguir jugando), te diga la puntuación final.
// 2.3 - Garantice que aunque las palabras salgan aleatoriamente, nunca salga la misma dos veces en una partida (el juego termina al salir todas).

// 3 - Modifique el método LoadWords para que cargue todas las palabras que hay en el archivo words.txt sin necesidad de definir explícitamente la cantidad.
//     Hint: Puede necesitar leer el archivo más de una vez.