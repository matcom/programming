using Weboo.Examen;


public class Program
{
    public static void Main()
    {
        // Adicione aquí los tests que considere necesarios

        // Todos conectados con todos, debe haber K frecuencias distintas
        Test(
            // Conexiones
            new[,]
            {
                {false,  true,  true},
                { true, false,  true},
                { true,  true, false},
            },
            // Resultado esperado
            3
        );

        // Dos conjuntos disjuntos de cliques
        Test(
            // Conexiones
            new[,]
            {
                // Estos tres están conectados entre sí
                {false,  true,  true, false, false},
                { true, false,  true, false, false},
                { true,  true, false, false, false},
                // Estos dos están conectados entre sí
                {false,  false, false, false, true},
                {false,  false, false, true, false},
            },
            // Resultado esperado
            3
        );

        // Nadie conectado
        Test(
            // Conexiones
            new[,]
            {
                {false, false, false, false, false, false},
                {false, false, false, false, false, false},
                {false, false, false, false, false, false},
                {false, false, false, false, false, false},
                {false, false, false, false, false, false},
                {false, false, false, false, false, false},
            },
            // Resultado esperado
            1
        );
    }

    public static void Test(bool[,] conexiones, int esperado)
    {
        try
        {
            int resultado = Frecuencias.AsignarFrecuencias(conexiones);

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