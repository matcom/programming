public enum State
{
    Awake,
    Asleep,
    Dead,
}


public class Program
{
    static int life = 100;
    static int hunger = 0;
    static int energy = 75;
    static int happiness = 50;
    static State state = State.Awake;

    static void Main()
    {
        System.Console.Write("¿Cómo se llama tu mascota? > ");
        string name = Console.ReadLine();

        while (true)
        {

            Console.Clear();
            UpdateStatus(name);
            PrintStatus(name);
            Console.WriteLine(name);

            PrintMenu();

            if (state == State.Dead)
            {
                Console.WriteLine("\n😢 {0} ha muerto!", name);
                return;
            }

            Console.Write("> ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            int option = int.Parse(input);
            Console.WriteLine();

            switch (option)
            {
                case 1:
                    Feed(25);
                    break;

                case 2:
                    Play(10);
                    break;

                case 3:
                    Sleep();
                    break;

                case 4:
                    Awake();
                    break;

                case 5:
                    Console.WriteLine("👋 Adiós!");
                    return;

                default:
                    break;
            }

            Thread.Sleep(1000);
        }
    }

    static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Opciones\n--------");
        Console.WriteLine("1️⃣ Alimentar");
        Console.WriteLine("2️⃣ Jugar");
        Console.WriteLine("3️⃣ Dormir");
        Console.WriteLine("4️⃣ Despertar");
        Console.WriteLine("5️⃣ Salir");
    }

    static void PrintStatus(string id)
    {
        Console.WriteLine();

        if (state == State.Awake)
        {
            Console.WriteLine("🙂 {0}", id);
        }
        else if (state == State.Asleep)
        {
            Console.WriteLine("😴 {0}", id);
        }
        else if (state == State.Dead)
        {
            Console.WriteLine("☠️ {0}", id);
        }

        Console.WriteLine("💓 {0}", life);
        Console.WriteLine("☀️  {0}", happiness);
        Console.WriteLine("⚡ {0}", energy);
        Console.WriteLine("🍖 {0}", hunger);
    }

    static void Feed(int food)
    {
        Console.Write("🍖 alimentando ... ");

        if (hunger > 0)
        {
            hunger -= food;
            Console.WriteLine("👍");
        }
        else
        {
            happiness -= 10;
            Console.WriteLine("👎");
        }
    }

    static void Play(int intensity)
    {
        Console.Write("🦴 jugando ... ");

        if (energy > 20)
        {
            energy -= intensity;
            hunger += intensity;
            happiness += 10;
            Console.WriteLine("👍");
        }
        else
        {
            happiness -= 10;
            hunger += intensity;
            Console.WriteLine("👎");
        }
    }

    static void Sleep()
    {
        Console.Write("💤 poniendo a dormir... ");

        if (energy < 50)
        {
            state = State.Asleep;
            Console.WriteLine("👍");
        }
        else
        {
            happiness -= 20;
            Console.WriteLine("👎");
        }
    }

    static void Awake()
    {
        Console.Write("🔔 despertando... ");

        if (energy > 30)
        {
            state = State.Awake;
            Console.WriteLine("👍");
        }
        else
        {
            happiness -= 20;
            Console.WriteLine("👎");
        }
    }

    static void UpdateStatus(string name)
    {
        if (life <= 0)
        {
            state = State.Dead;
            return;
        }

        switch (state)
        {
            case State.Awake:
                if (hunger > 50)
                {
                    happiness -= 10;
                }
                if (hunger >= 90)
                {
                    life -= 5;
                }
                if (energy <= 0)
                {
                    Console.WriteLine("😱 {0} se ha desmayado!", name);
                    state = State.Asleep;
                }

                energy -= 5;
                hunger += 5;
                break;

            case State.Asleep:
                energy += 20;
                hunger += 5;
                happiness += 5;

                if (hunger > 50 || energy > 75)
                {
                    Console.WriteLine("😱 {0} se ha despertado!", name);
                    state = State.Awake;
                }

                break;

            default:
                break;
        }

        life = Normalize(life);
        hunger = Normalize(hunger, max: 90);
        energy = Normalize(energy);
        happiness = Normalize(happiness);
    }

    static int Normalize(int value, int min = 0, int max = 100)
    {
        return Math.Min(max, Math.Max(value, min));
    }
}
