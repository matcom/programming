using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        static void Main(string[] args)
        {
            StartGame();
        }

        #region PrettyPrinting

        static void PrintInfo(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static void PrintError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static void PrintBoard(char[,] array)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] != default(char))
                        Console.Write($"{array[i, j]} ");
                    else
                        Console.Write("- ");
                }
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        #endregion

        #region GameEngine

        static void StartGame()
        {
            int n = ReadInt("Introduzca el número de filas");
            int m = ReadInt("Introduzca el número de columnas");
            int p = ReadInt("Introduzca el número de jugadores", 2);
            char[] marks = ReadPlayerMarks(p);
            char[,] board = BuildBoard(n, m);

            Console.WriteLine();
            PrintBoard(board);
            Console.WriteLine();

            int player = 0;
            while (true)
            {
                PrintInfo($"------------- Jugador {player} -------------");

                int i = ReadInt("Introduzca fila.", 0, n - 1);
                int j = ReadInt("Introduzca columna.", 0, m - 1);
                Console.WriteLine();

                State result = PlayAtPosition(board, i, j, marks[player]);

                if (result != State.Invalid)
                {
                    PrintBoard(board);
                    player = (player + 1) % marks.Length;
                }

                Console.WriteLine(result);
                Console.WriteLine();

                if (result == State.Win || result == State.Draw)
                {
                    break;
                }
            }
        }

        static State PlayAtPosition(char[,] board, int i, int j, char mark)
        {
            // Validate position
            if (board[i, j] != default(char) || i < 0 || i >= board.GetLength(0) || j < 0 || j >= board.GetLength(1))
                return State.Invalid;

            int[,] dir = { { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 } };

            int winCondition = Math.Min(board.GetLength(0), board.GetLength(1));

            // Play at (i,j)
            board[i, j] = mark;

            // Check for win condition
            for (int k = 0; k < dir.GetLength(0); k++)
            {
                int countDir = CountInDirection(board, i, j, dir[k, 0], dir[k, 1]);
                int countOposDir = CountInDirection(board, i, j, -1 * dir[k, 0], -1 * dir[k, 1]);
                if (countDir + countOposDir - 1 >= winCondition)
                    return State.Win;
            }

            // Can game continue ????
            for (int k = 0; k < board.GetLength(0); k++)
            {
                for (int l = 0; l < board.GetLength(1); l++)
                {
                    if (board[k, l] == default(char))
                        return State.Continue;
                }
            }

            return State.Draw;
        }

        static int CountInDirection(char[,] tablero, int i, int j, int dx, int dy)
        {
            int count = 0;
            char mark = tablero[i, j];
            for (int posX = i, posY = j;
                posX >= 0 && posX < tablero.GetLength(0) && posY >= 0 && posY < tablero.GetLength(1);
                posX += dx, posY += dy)
            {

                if (tablero[posX, posY] != mark) // Podría incluirse en la condición del FOR
                    break;

                count++;

            }
            return count;
        }

        #endregion

        #region ConfigurationTools

        static int ReadInt(string prompt = "Introduzca un número entero.", int min = 3, int max = int.MaxValue)
        {
            int number;
            PrintInfo($"{prompt} ({min} <= number <= {max}).");

            while (!int.TryParse(Console.ReadLine(), out number) || number < min || number > max)
            {
                PrintError($"Numero no válido ({min} <= number <= {max}).");
            }
            return number;
        }

        static char ReadChar(string prompt = "Introduzca un caracter.")
        {
            PrintInfo(prompt);

            string text;
            while ((text = Console.ReadLine()).Length != 1 || !char.IsSymbol(text, 0) && !char.IsLetterOrDigit(text, 0))
            {
                PrintError("Solo un caracter y debe ser un símbolo, número o letra.");
            }
            return text[0];
        }

        static char[] ReadPlayerMarks(int p)
        {
            char[] marks = new char[p];
            int player = 0;
            while (player < p)
            {
                marks[player] = ReadChar($"Introduzca la marca del jugador {player}.");

                for (int i = 0; i < player; i++)
                {
                    if (marks[i] == marks[player])
                    {
                        PrintError($"Esa marca ya fue asignada al jugador {i}.");
                        player--;
                        break;
                    }
                }

                player++;
            }
            return marks;
        }

        static char[,] BuildBoard(int n, int m)
        {
            return new char[n, m];
        }

        #endregion

        enum State
        {
            Win,
            Draw,
            Continue,
            Invalid
        }
    }
}

