namespace MatCom.Algorithms;

public class TSPProblem
{
    string[] nodes;
    int[,] costs;

    public int Size
    {
        get { return this.costs.GetLength(0); }
    }

    public TSPProblem(string[] nodes, int[,] costs)
    {
        this.nodes = (string[])nodes.Clone();
        this.costs = (int[,])costs.Clone();
    }

    public int Cost(int origin, int destination)
    {
        return this.costs[origin, destination];
    }

    public string Node(int i)
    {
        return this.nodes[i];
    }

    public int Cost(int[] indices, bool closed = false)
    {
        int totalCost = 0;

        for (int i = 1; i < indices.Length; i++)
            totalCost += this.Cost(indices[i - 1], indices[i]);

        if (closed)
            totalCost += this.Cost(indices[indices.Length - 1], indices[0]);

        return totalCost;
    }

    public static TSPProblem Random(int size)
    {
        string[] nodes = new string[size];
        Random r = new Random();

        for (int i = 0; i < size; i++)
            nodes[i] = ((char)('A' + i)).ToString();

        int[,] costs = new int[size, size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                if (i == j) continue;

                costs[i, j] = costs[j, i] = r.Next(1, 10);
            }

        return new TSPProblem(nodes, costs);
    }
}

public class TSPSolution
{
    TSPProblem problem;
    int[] indices;

    public int TotalCost { get; private set; }

    public int Size { get { return this.problem.Size; } }

    public (string, string, int) Step(int i)
    {
        int start, end;

        if (i < this.Size - 1)
        {
            start = this.indices[i];
            end = this.indices[i + 1];
        }
        else
        {
            start = this.indices[i];
            end = this.indices[0];
        }

        return (this.problem.Node(start),
                this.problem.Node(end),
                this.problem.Cost(start, end));
    }

    public TSPSolution(TSPProblem problem, int[] indices)
    {
        this.problem = problem;
        this.indices = (int[])indices.Clone();
        this.TotalCost = problem.Cost(indices, closed: true);
    }

    public static TSPSolution Find(TSPProblem problem)
    {
        int[] solution = new int[problem.Size];
        bool[] visited = new bool[problem.Size];
        return SolveWithBest(problem, solution, visited, 0, 0, null);
    }

    private static TSPSolution Solve(
        TSPProblem problem, int[] solution, bool[] visited, int current
    )
    {
        if (current >= problem.Size)
            return new TSPSolution(problem, solution);

        TSPSolution? best = null;

        for (int node = 0; node < problem.Size; node++)
        {
            if (visited[node]) continue;

            solution[current] = node;
            visited[node] = true;

            TSPSolution s = Solve(problem, solution, visited, current + 1);

            if (best == null || s.TotalCost < best.TotalCost)
                best = s;

            visited[node] = false;
        }

        return best!;
    }

    private static TSPSolution SolveWithBest(
        TSPProblem problem,
        int[] solution,
        bool[] visited,
        int current,
        int cost,
        TSPSolution? best
    )
    {
        if (current >= problem.Size)
            return new TSPSolution(problem, solution);

        if (best != null && cost > best.TotalCost)
            return best;

        for (int node = 0; node < problem.Size; node++)
        {
            if (visited[node]) continue;

            solution[current] = node;
            visited[node] = true;
            int newCost = cost;

            if (current > 0)
                newCost += problem.Cost(solution[current - 1], node);

            TSPSolution s = SolveWithBest(
                problem, solution, visited, current + 1, newCost, best
            );

            if (best == null || s.TotalCost < best.TotalCost)
                best = s;

            visited[node] = false;
        }

        return best!;
    }
}

