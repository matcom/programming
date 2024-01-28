class MyTools
{
    public static int Successor(int number)
    {
        return number + 1;
    }
    public static void SayHello(string name)
    {
        Console.WriteLine($"Hola {name}. Que tengas un lindo día!");
    }
    public static void SayHelloAndCongratForNextBirthday(string name, int currentAge)
    {
        SayHello(name);
        int nextAge = Successor(currentAge);
        Console.WriteLine($"Felicidades, proximamente cumplirás {nextAge} años!");
    }
}

class Program
{
    static void Main()
    {
        int next = MyTools.Successor(5);
        Console.WriteLine(next);

        Console.WriteLine(MyTools.Successor(10));

        MyTools.SayHello("Juan Pablo");

        MyTools.SayHelloAndCongratForNextBirthday("Juan Pablo", 30);
    }
}