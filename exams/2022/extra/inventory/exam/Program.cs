using System.Diagnostics;

namespace MatCom.Exam
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(Exam.Nombre);
            Console.WriteLine(Exam.Grupo);

            ExampleCase();
        }

        static void ExampleCase()
        {
            IInventory inv = Exam.GetInventory();

            // Referencia a la categoría raíz
            ICategory root = inv.Root;

            // Crear las categorías iniciales
            ICategory alimentos = root.CreateSubcategory("Alimentos");
            ICategory electronica = root.CreateSubcategory("Electronica");

            // Crear las subcategorías
            ICategory carnicos = alimentos.CreateSubcategory("Carnicos");
            ICategory vegetales = alimentos.CreateSubcategory("Vegetales");
            ICategory electrodomesticos = electronica.CreateSubcategory("Electrodomesticos");
            ICategory informatica = electronica.CreateSubcategory("Informatica");

            // Crear los productos
            alimentos.UpdateProduct("Arroz", 10);

            carnicos.UpdateProduct("Pescado", 5);
            carnicos.UpdateProduct("Carne de Cerdo", 10);

            vegetales.UpdateProduct("Tomate", 20);
            vegetales.UpdateProduct("Lechuga", 4);

            electrodomesticos.UpdateProduct("Lavadora", 2);
            informatica.UpdateProduct("Ordenador", 3);

            // Hasta aquí tenemos el inventario del ejemplo

            // Crear una nueva categoría
            ICategory moviles = informatica.CreateSubcategory("Moviles");
            Debug.Assert(moviles == inv.GetCategory("Electronica",
                                                    "Informatica",
                                                    "Moviles"));

            // Disminuye en 1 la cantidad de ordenadores
            informatica.UpdateProduct("Ordenador", -1);
            Debug.Assert(inv.GetProduct("Ordenador", "Electronica", "Informatica").Count == 2);

            // Crea un nuevo producto
            moviles.UpdateProduct("Samsung Galaxy", 10);
            Debug.Assert(inv.GetProduct("Samsung Galaxy", "Electronica", "Informatica", "Moviles").Count == 10);

            // Obtener todos los productos con menos de 3 elementos
            foreach (var product in inv.FindAll(p => p.Count < 5))
            {
                // Verificando que efectivamente tiene menos de 5
                Debug.Assert(product.Count > 0 && product.Count < 5);
            }
        }
    }
}