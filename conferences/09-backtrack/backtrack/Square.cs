namespace MatCom.Backtrack;


public static class Square
{
    public static int[,]? Solve(int size)
    {
        int[,] square = new int[size, size];
        int max = size * size;
        int sum = max * (max + 1) / (2 * size);

        if (Solve(square, 0, 0, sum, max))
            return square;

        return null;
    }

    private static bool Solve(int[,] square, int row, int col, int sum, int max)
    {
        if (row >= square.GetLength(0))
            return Solve(square, 0, col+1, sum, max);

        if (col >= square.GetLength(1))
            return IsValid(square, sum, max);

        for (int value = 1; value <= max; value++)
        {
            square[row, col] = value;

            if (Solve(square, row+1, col, sum, max))
                return true;
        }

        return false;
    }

    private static bool IsValid(int[,] square, int sum, int max)
    {
        for (int row = 0; row < square.GetLength(0); row++)
            if (SumRow(square, row) != sum)
                return false;

        for (int col = 0; col < square.GetLength(1); col++)
            if (SumCol(square, col) != sum)
                return false;

        bool[] used = new bool[max];

        for (int i = 0; i < square.GetLength(0); i++)
        {
            for (int j = 0; j < square.GetLength(1); j++)
            {
                int val = square[i,j] - 1;

                if (val >= max)
                    return false;

                if (used[val])
                    return false;

                used[val] = true;
            }
        }

        return true;
    }

    private static int SumRow(int[,] square, int row)
    {
        int sum = 0;

        for (int j = 0; j < square.GetLength(1); j++)
            sum += square[row, j];

        return sum;
    }

    private static int SumCol(int[,] square, int col)
    {
        int sum = 0;

        for (int i = 0; i < square.GetLength(0); i++)
            sum += square[i, col];

        return sum;
    }
}
