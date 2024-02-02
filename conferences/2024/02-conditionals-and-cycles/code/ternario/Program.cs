class Program
{
    static void Main()
    {
        int number = int.Parse(Console.ReadLine());
        string sign = (number > 0)? "positivo" : (number < 0)? "negativo" : "cero";
        string parity = (number % 2 == 0) ? "par" : "impar";
        Console.WriteLine($"El número es {sign} y además {parity}");
    }
}
