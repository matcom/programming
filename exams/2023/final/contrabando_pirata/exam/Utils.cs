class Map
{
    private int[,] Distances { get; set; }
    public int N {get; private set; }
    public int[] A {get; private set;}
    public int[] B {get; private set;}

    public Map(int[,] distances, int[] A, int[] B)
    {
        Distances = distances;
        N = Distances.GetLength(0);
        this.A = A;
        this.B = B;
    }

    public int this[int i, int j]
    {
        get => Distances[i,j];
    }

    public bool IsTypeA(int node)
    {
        if (node >= N)
            throw new IndexOutOfRangeException();

        if (A.Contains(node))
            return true;
        return false;
    }

    public bool IsTypeB(int node)
    {
        if (node >= N)
            throw new IndexOutOfRangeException();

        if (B.Contains(node))
            return true;
        return false;
    }

}

