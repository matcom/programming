public class Map
{
    private int[,] Distances { get; set; }
    public int M {get; private set; }
    public int[] Demand {get; private set;}

    public Map(int[,] distances, int[] demand)
    {
        Distances = distances;
        M = Distances.GetLength(0);
        Demand = demand;

    }

    public int this[int i, int j]
    {
        get => Distances[i,j];
    }
}

