using funclib;

class Program
{
    void Main()
    {
        TestFilter();
    }

    private static void TestFilter()
    {
        int[] numbers = new int[100];

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i + 1;
        }

        int[] evenNumbers = FuncTools.Filter(numbers, new Predicate<int>(IsEven));

        int[] oddNumbers = FuncTools.Filter(numbers, new Predicate<int>(delegate (int x)
        {
            return x % 2 == 1;
        }));

        int[] fullNumbers = FuncTools.Filter(numbers, x => x % 10 == 0);

        DateTime[] dates = new DateTime[365];

        for (int i = 0; i < dates.Length; i++)
        {
            dates[i] = new DateTime(2022, 1, 1).AddDays(i);
        }

        DateTime[] weekends = FuncTools.Filter(dates,
            date => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
        );
    }

    private static void TestMap()
    {
        int[] numbers = new int[100];

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i + 1;
        }

        int[] squares = FuncTools.Map(items, x => x * x);
    }

    private static void TestReduce()
    {
        int[] numbers = new int[100];

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i + 1;
        }

        int sum = FuncTools.Reduce(numbers, (num, accum) => num + accum, seed=0);
    }

    static bool IsEven(int number)
    {
        return number % 2 == 0;
    }
}