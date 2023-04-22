class Map
{
    private int[,] Distances { get; set; }
    public int N {get; private set; }
    public int[] Start {get; private set;}
    public int[] End {get; private set;}

    public Map(int[,] distances, int[] start, int[] end)
    {
        Distances = distances;
        N = Distances.GetLength(0);
        Start = start;
        End = end;
    }

    public int this[int i, int j]
    {
        get => Distances[i,j];
    }

    public bool IsOnTime(int node, int time)
    {
        return Start[node] <= time && End[node] >= time;
    }
}
