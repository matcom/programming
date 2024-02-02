class Program
{
    static void Main()
    {
        Console.WriteLine("Dime un número, para sumar desde cero hasta ese número.");
        int number = int.Parse(Console.ReadLine());
        int current = 1;
        int sum = 0;
        string expression = "0";
        while (current <= number) {
            sum += current;
            expression += $" + {current}";
            current++;
        }
        Console.WriteLine($"{expression} = {sum}.");
    }
}
