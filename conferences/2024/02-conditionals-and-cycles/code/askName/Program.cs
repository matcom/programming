class Program
{
    static void Main()
    {
        Console.WriteLine("Hola, cómo te llamas?");
        string name = Console.ReadLine();
        while(name == "") {
            Console.WriteLine("Olvidaste escribir tu nombre. Intenta de nuevo!");
            name = Console.ReadLine();
        }
        Console.WriteLine($"Un placer conocerte {name}!");
    }
}
