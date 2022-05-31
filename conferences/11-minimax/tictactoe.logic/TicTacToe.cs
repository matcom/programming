namespace tictactoe.logic;

public enum Mark
{
    None,
    Zero,
    Cross,
    Draw,
}

public class TicTacToe
{
    private Mark[,] board = new Mark[3, 3];

    public Mark Turn { get; private set; } = Mark.Cross;

    public Mark this[int row, int col]
    {
        get { return this.board[row, col]; }
    }

    public TicTacToe Clone()
    {
        return new TicTacToe()
        {
            board = (Mark[,])this.board.Clone(),
            Turn = this.Turn,
        };
    }

    public void Play(int row, int col)
    {
        if (!CanPlay(row, col))
            throw new ArgumentException($"Cannot play on {row}, {col}.");

        this.board[row, col] = this.Turn;
        this.Turn = this.Turn == Mark.Cross ? Mark.Zero : Mark.Cross;
    }

    public bool CanPlay(int row, int col)
    {
        return row >= 0 &&
                row < board.GetLength(0) &&
                col >= 0 &&
                col < board.GetLength(1) &&
                board[row, col] == Mark.None;
    }

    public bool CanPlay()
    {
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                if (this.board[row, col] == Mark.None) return true;

        return false;
    }

    public Mark Winner()
    {
        foreach (Mark mark in new[] { Mark.Zero, Mark.Cross })
        {
            for (int i = 0; i < 3; i++)
            {
                if (AllEqual(i, 0, 0, 1, mark)) return mark;
                if (AllEqual(0, i, 1, 0, mark)) return mark;
            }

            if (AllEqual(0, 0, 1, 1, mark)) return mark;
            if (AllEqual(0, 2, 1, -1, mark)) return mark;
        }

        if (!CanPlay()) return Mark.Draw;

        return Mark.None;
    }

    private bool AllEqual(int row, int col, int dr, int dc, Mark mark)
    {
        while (row >= 0 && row < this.board.GetLength(0) && col >= 0 && col < this.board.GetLength(1))
        {
            if (this.board[row, col] != mark)
                return false;

            row += dr;
            col += dc;
        }

        return true;
    }
}
