Console.WriteLine("Hola, mi nombre es Eliza!");

string nombre = "";

while (true)
{
    Console.Write("> ");
    string message = Console.ReadLine();

    if (string.IsNullOrEmpty(message))
    {
        Console.WriteLine("¿Qué decías?");
    }
    else if (message.ToLower() == "hola")
    {
        Console.WriteLine("Hola de nuevo. ¿Cómo estás?");
    }
    else if (message.ToLower().StartsWith("mi nombre es"))
    {
        nombre = message.Substring("mi nombre es ".Length);
        System.Console.WriteLine("Hola {0}. Gusto en conocerte.", nombre);
    }
    else if (message.ToLower().StartsWith("cual es mi nombre"))
    {
        if (string.IsNullOrEmpty(nombre))
        {
            System.Console.WriteLine("No me lo has dicho 🙃");
        }
        else
        {
            System.Console.WriteLine("Me dijiste que te llamabas {0}. ¡Bonito nombre!", nombre);
        }
    }
    else if (message.ToLower().StartsWith("me "))
    {
        string claim = message.Substring(3);
        System.Console.WriteLine("¿Por qué dices que te {0}?", claim);
    }
    else if (message.ToLower().StartsWith("soy "))
    {
        string claim = message.Substring(4);
        System.Console.WriteLine("¿Por qué crees que eres {0}?", claim);
    }
    else if (message.ToLower().StartsWith("porque "))
    {
        System.Console.WriteLine("¿Seguro que no hay otra razón?");
    }
    else if (message.ToLower().StartsWith("tambien "))
    {
        System.Console.WriteLine("¡Impresionante! ¿Algo más?");
    }
    else if (message.ToLower() == "no")
    {
        System.Console.WriteLine("No hay porqué ser tan negativo.");
    }
    else if (message.ToLower() == "si")
    {
        System.Console.WriteLine("Me alegra que estemos de acuerdo.");
    }
    else if ((message.Contains("+") || message.Contains("-")) && message.Any(char.IsDigit))
    {
        System.Console.WriteLine("¡Me ofende que me consideres una simple calculadora!");
    }
    else if (message.ToLower().StartsWith("mi edad es "))
    {
        int age = int.Parse(message.Substring("mi edad es ".Length));

        if (age < 10 || age > 100)
        {
            System.Console.WriteLine("Que va, no te creo.");
        }
        else if (age < 20)
        {
            System.Console.WriteLine("Que joven! ¿Que estás estudiando?");
        }
        else if (age < 30)
        {
            System.Console.WriteLine("Interesante. ¿Donde trabajas?");
        }
        else if (age < 65)
        {
            System.Console.WriteLine("Genial. ¿Tienes hijos?");
        }
        else
        {
            System.Console.WriteLine("Vaya, cuánta experiencia habrás acumulado!");
        }
    }
    else if (message.ToLower() == "adios" || message.ToLower() == "chao" || message.ToLower() == "hasta luego")
    {
        System.Console.WriteLine("Ha sido un placer. ¡Nos vemos! 😉");
        break;
    }
    else if (message.Length < 10)
    {
        System.Console.WriteLine("¡Genial! Cuéntame más.");
    }
    else
    {
        System.Console.WriteLine("Lo siento, pero no te he entendido. ¿Puedes intentarlo de nuevo?");
    }
}
