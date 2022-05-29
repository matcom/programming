using MatCom.Backtrack;


public class Program
{
    public static void Main(string[] args)
    {
        int size = int.Parse(args[0]);
        int[,]? square = Square.Solve(size);

        if (square != null)
            PrintArray(square);
        else
            Console.WriteLine("Invalid size");
    }

    public static void PrintArray(int[,] array)
    {
        int max = array.Length;
        int pad = max.ToString().Length + 1;

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
                Console.Write(array[i, j].ToString().PadLeft(pad));

            Console.WriteLine();
        }
    }
}
