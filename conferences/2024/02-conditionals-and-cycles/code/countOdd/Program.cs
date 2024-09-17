class Program
{
    static void Main()
    {
        Console.WriteLine("Dime números, para contar los impares. Cuando me escribas algo que no sea un número haré el conteo.");
        int count = 0;
        while(true) {
            string line = Console.ReadLine();
            if(!int.TryParse(line, out int number))
                break;
            if(number % 2 == 0)
                continue;
            count++;
        }
        Console.WriteLine($"Escribiste {count} números impares!");
    }
}
