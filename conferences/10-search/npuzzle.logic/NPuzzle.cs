using System.Text;


namespace npuzzle.logic;

public class NPuzzle
{
    public enum Movement
    {
        Left,
        Right,
        Up,
        Down,
    }

    private int[,] board;

    public int BlankRow { get; private set; } = 0;
    public int BlankCol { get; private set; } = 0;

    public int this[int row, int col]
    {
        get { return this.board[row, col]; }
    }

    public int Size { get; private set; }

    public NPuzzle(int size)
    {
        this.board = new int[size, size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                this.board[i, j] = size * i + j;

        this.Size = size;
    }

    private NPuzzle(int[,] board, int blankRow, int blankCol)
    {
        this.board = board;
        this.Size = board.GetLength(0);
        this.BlankRow = blankRow;
        this.BlankCol = blankCol;
    }

    public NPuzzle Randomize()
    {
        Random r = new Random();
        NPuzzle puzzle = this;

        for (int i = 0; i < 100 * Size * Size; i++)
        {
            puzzle = puzzle.Move((NPuzzle.Movement)r.Next(4));
        }

        return puzzle;
    }

    public NPuzzle Move(NPuzzle.Movement movement)
    {
        if (!CanMove(movement))
        {
            return this;
        }

        (int newRow, int newCol) = this.NewBlank(movement);
        int[,] newBoard = (int[,])this.board.Clone();
        newBoard[this.BlankRow, this.BlankCol] = newBoard[newRow, newCol];
        newBoard[newRow, newCol] = 0;

        return new NPuzzle(newBoard, newRow, newCol);
    }

    public bool CanMove(NPuzzle.Movement movement)
    {
        (int newRow, int newCol) = this.NewBlank(movement);
        return IsValid(newRow, newCol);
    }

    public bool Solved()
    {
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                if (this.board[i, j] != Size * i + j)
                    return false;

        return true;
    }

    private (int, int) NewBlank(NPuzzle.Movement movement)
    {
        int[] drow = { 0, 0, -1, 1 };
        int[] dcol = { -1, 1, 0, 0 };

        int newRow = this.BlankRow + drow[(int)movement];
        int newCol = this.BlankCol + dcol[(int)movement];

        return (newRow, newCol);
    }

    private bool IsValid(int row, int col)
    {
        return row >= 0 && row < Size && col >= 0 && col < Size;
    }

    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return this.ToString() == obj.ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        int pad = (this.Size * this.Size - 1).ToString().Length + 1;

        for (int i = 0; i < this.Size; i++)
        {
            for (int j = 0; j < this.Size; j++)
            {
                if (this.board[i, j] == 0)
                {
                    sb.Append(String.Empty.PadLeft(pad));
                }
                else
                {
                    sb.Append(this.board[i, j].ToString().PadLeft(pad));
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
