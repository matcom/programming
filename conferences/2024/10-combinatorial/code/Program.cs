namespace MatCom.Programming
{

    class Combinatorial
    {
        public static void VariationsWithoutRepetitionsA(int[] items, int k, int pos = 0)
        {
            if (pos == k)
            {
                // item from 0 to k is a variation!!!
                Console.WriteLine(string.Join(", ", items.Take(k)));
                return;
            }

            for (int i = pos; i < items.Length; i++)
            {
                int temp = items[pos];
                items[pos] = items[i];
                items[i] = temp;

                VariationsWithoutRepetitionsA(items, k, pos + 1);

                temp = items[pos];
                items[pos] = items[i];
                items[i] = temp;
            }
        }

        public static void VariationsWithoutRepetitionsB(int[] items, int k)
        {
            int[] variation = new int[k];
            bool[] taken = new bool[items.Length];
            InternalVariationsWithoutRepetitionsB(items, k, variation, 0, taken);
        }

        static void InternalVariationsWithoutRepetitionsB(int[] items, int k, int[] variation, int count, bool[] taken)
        {
            if (count == k)
            {
                // variation is ready!!!
                Console.WriteLine(string.Join(", ", variation.Take(count)));
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (!taken[i])
                {
                    taken[i] = true;
                    variation[count] = items[i];
                    InternalVariationsWithoutRepetitionsB(items, k, variation, count + 1, taken);
                    taken[i] = false;
                }
            }
        }

        public static void Permutations(int[] items)
        {
            VariationsWithoutRepetitionsA(items, items.Length);
        }
    }
    class TravelingSalesman
    {
        public static int Solve(int[,] distances)
        {
            int nCities = distances.GetLength(0);
            int[] cities = Enumerable.Range(0, nCities).ToArray();
            int[] variation = new int[nCities];
            bool[] taken = new bool[nCities];
            return ModifiedVariationsWithoutRepetitionsB(cities, nCities, variation, 0, taken, distances, int.MaxValue);
        }

        static int ModifiedVariationsWithoutRepetitionsB(
            int[] items,
            int k,
            int[] variation,
            int count,
            bool[] taken,
            int[,] distances,
            int min
        )
        {
            if (count == k)
                return Math.Min(min, EvaluateVariation(variation, distances));

            for (int i = 0; i < items.Length; i++)
            {
                if (!taken[i])
                {
                    taken[i] = true;
                    variation[count] = items[i];
                    min = ModifiedVariationsWithoutRepetitionsB(items, k, variation, count + 1, taken, distances, min);
                    taken[i] = false;
                }
            }
            return min;
        }

        static int EvaluateVariation(int[] variation, int[,] distances)
        {
            int result = 0;
            for (int i = 0; i < variation.Length - 1; i++)
                result += distances[variation[i], variation[i + 1]];
            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int[] items = Enumerable.Range(0, 5).ToArray();
            Combinatorial.VariationsWithoutRepetitionsA(items, 2);
            Console.WriteLine("--------------");
            Combinatorial.VariationsWithoutRepetitionsB(items, 2);
            Console.WriteLine("--------------");
            Combinatorial.Permutations(items);
            Console.WriteLine("--------------");

            int[,] distances = new int[,] {
                { 0, 60, 30, 40, 35 },
                { 60, 0, 15, 55, 30 },
                { 30, 15, 0, 75, 45 },
                { 40, 55, 75, 0, 85 },
                { 35, 30, 45, 85, 0 }
            };
            int best = TravelingSalesman.Solve(distances);
            Console.WriteLine(best);
        }
    }
}
