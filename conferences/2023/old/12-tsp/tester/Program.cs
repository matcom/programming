using MatCom.Algorithms;


class Program
{
    static void Main(string[] args)
    {
        int size = int.Parse(args[0]);
        var problem = TSPProblem.Random(size);

        Print(problem);

        int time = Environment.TickCount;
        var solution = TSPSolution.Find(problem);
        int elapsed = Environment.TickCount - time;

        Console.WriteLine($"\n🚀 Solved in {elapsed / 1000.0} seconds.");
        Print(solution);
    }

    static void Print(TSPProblem problem)
    {
        Console.Write("   ");

        for (int i = 0; i < problem.Size; i++)
            Console.Write(problem.Node(i).PadLeft(3));

        Console.WriteLine();

        for (int i = 0; i < problem.Size; i++)
        {
            Console.Write(problem.Node(i).PadLeft(3));

            for (int j = 0; j < problem.Size; j++)
                Console.Write(problem.Cost(i, j).ToString().PadLeft(3));

            Console.WriteLine();
        }
    }

    static void Print(TSPSolution solution)
    {
        Console.WriteLine($"💲 Total cost: {solution.TotalCost}\n");

        for (int i = 0; i < solution.Size; i++)
        {
            (string start, string end, int cost) = solution.Step(i);
            Console.WriteLine($"{start} ➡️ {end} = {cost}");
        }
    }
}