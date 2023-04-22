using Weboo.Examen;


public class Program
{
    public static void Main()
    {
        // Adicione aquí los tests que considere necesarios

        // Un nodo central conectado con todos
        Test(
            // Intersecciones
            new[,]
            {
                {false,   true,  true,  true,  true},
                { true,  false, false, false, false},
                { true,  false, false, false, false},
                { true,  false, false, false, false},
                { true,  false, false, false, false},
            },
            // Resultado esperado
            1
        );
    }

    public static void Test(bool[,] intersecciones, int esperado)
    {
        try
        {
            int resultado = CCTV.UbicarCamaras(intersecciones);

            if (resultado != esperado)
            {
                throw new Exception($"Se esperaba {esperado} pero se obtuvo {resultado}");
            }

            Console.WriteLine($"🟢 Resultado correcto: {resultado}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"🔴 {e}");
        }
    }
}