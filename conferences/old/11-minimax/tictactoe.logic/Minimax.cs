namespace tictactoe.logic;

public class TicTacToeAI
{
    public static (int, int) BestMove(TicTacToe game)
    {
        (int score, int row, int col) = PlayMax(game, game.Turn);

        Console.WriteLine($"I can achieve {score}");
        Thread.Sleep(1000);

        return (row, col);
    }

    static int FinalScore(TicTacToe game, Mark player)
    {
        Mark winner = game.Winner();

        if (winner == Mark.None)
            throw new InvalidOperationException("Game hasn't finished yet!");
        else if (winner == player)
            return 1;
        else if (winner == Mark.Draw)
            return 0;
        else
            return -1;
    }

    static (int, int, int) PlayMax(TicTacToe game, Mark player)
    {
        int bestScore = int.MinValue;
        (int bestRow, int bestCol) = (-1, -1);

        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
            {
                if (!game.CanPlay(row, col)) continue;

                TicTacToe nextGame = game.Clone();
                nextGame.Play(row, col);

                int otherScore;

                if (nextGame.Winner() == Mark.None)
                    (otherScore, _, _) = PlayMin(nextGame, player);
                else
                    otherScore = FinalScore(nextGame, player);

                if (otherScore > bestScore)
                {
                    bestScore = otherScore;
                    bestRow = row;
                    bestCol = col;
                }
            }

        return (bestScore, bestRow, bestCol);
    }

    static (int, int, int) PlayMin(TicTacToe game, Mark player)
    {
        int bestScore = int.MaxValue;
        (int bestRow, int bestCol) = (-1, -1);

        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
            {
                if (!game.CanPlay(row, col)) continue;

                TicTacToe nextGame = game.Clone();
                nextGame.Play(row, col);

                int otherScore;

                if (nextGame.Winner() == Mark.None)
                    (otherScore, _, _) = PlayMax(nextGame, player);
                else
                    otherScore = FinalScore(nextGame, player);

                if (otherScore < bestScore)
                {
                    bestScore = otherScore;
                    bestRow = row;
                    bestCol = col;
                }
            }

        return (bestScore, bestRow, bestCol);
    }
}
