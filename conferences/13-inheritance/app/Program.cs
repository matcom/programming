using Accounting;


class Program
{
    static Product[] Catalog = {
        new Product("🐟 Pescado (kg)", 100, 1),
        new DiscountProduct("🐟 Pescado (kg)", 100, 10, 0.1),
        new Product("🐔 Pollo (kg)", 150, 1),
        new Product("🐔 Pollo (kg)", 150, 5),
        new Product("🥚 Huevo", 10, 1),
        new DiscountProduct("🥚 Huevos (caja)", 10, 30, 0.05),
    };

    static void Main()
    {
        Cart cart = new CartWithShipment(10, 1000);

        while (true)
        {
            Console.Clear();

            System.Console.ForegroundColor = ConsoleColor.DarkBlue;
            System.Console.WriteLine("💝 Catálogo");

            System.Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < Catalog.Length; i++)
            {
                System.Console.WriteLine($"{i+1}: {Catalog[i]}");
            }

            System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            System.Console.WriteLine("\n\n🛒 Carrito - ${0}", cart.TotalCost());

            System.Console.ForegroundColor = ConsoleColor.Black;
            foreach (var item in cart.Products())
            {
                System.Console.WriteLine(item);
            }

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n\nAgregar producto: ");

            try
            {
                int index = int.Parse(Console.ReadLine());
                cart.Add(Catalog[index-1]);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                Thread.Sleep(1000);
                continue;
            }
        }
    }
}