using System;

class Program
{
    static void Main(string[] args)
    {
        int min = int.Parse(args[0]);
        int max = int.Parse(args[1]);

        Console.WriteLine($"Piensa un número entre {min} y {max}.");
        Console.WriteLine("Presiona ENTER cuando estés list@.");
        Console.ReadLine();

        while (min < max)
        {
            int mid = (min + max) / 2;

            Console.WriteLine($"¿Es tu número m[a]yor, m[e]nor, o [i]gual a {mid}?");
            char c = Console.ReadKey(true).KeyChar;

            switch(c) {
                case 'a':
                    min = mid + 1;
                    break;
                case 'e':
                    max = mid - 1;
                    break;
                case 'i':
                    Console.WriteLine($"¡Tu número es {mid}!");
                    return;
                default:
                    Console.WriteLine("Respuesta inválida. Inténtalo de nuevo.");
                    break;
            }
        }

        if (min == max)
        {
            Console.WriteLine($"¡Tu número es {min}!");
        }
        else
        {
            Console.WriteLine("Has hecho trampa, pillín...");
        }
    }
}

// EJERCICIOS

// Modifica el programa para que no necesite saber de antemano
// el número máximo. ¡Puede ser cualquier número!
// Trata de resolverlo en la menor cantidad de preguntas.
