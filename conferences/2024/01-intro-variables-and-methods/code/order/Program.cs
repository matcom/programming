class Program
{
    static void Main()
    {
        int x = 5;
        int y = x;

        // Qué imprime esto?
        Console.WriteLine(x);
        Console.WriteLine(y);

        x = 10;

        // Y ahora?
        Console.ReadLine();
        Console.WriteLine(y);
    }
}
