using Weboo.Examen;


public class Program
{
    public static void Main()
    {
        // Adicione aquí los tests que considere necesarios

        // Una asignatura convalida todas las demás
        Test(
            // Intersecciones
            new[,]
            {
                { true,   true,  true,  true,  true},
                { false,  true, false, false, false},
                { false, false,  true, false, false},
                { false, false, false,  true, false},
                { false, false, false, false,  true},
            },
            // Resultado esperado
            1
        );

        // Ninguna asignatura convalida a ninguna otra
        Test(
            // Intersecciones
            new[,]
            {
                { true,  false, false, false, false},
                { false,  true, false, false, false},
                { false, false,  true, false, false},
                { false, false, false,  true, false},
                { false, false, false, false,  true},
            },
            // Resultado esperado
            5
        );
    }

    public static void Test(bool[,] intersecciones, int esperado)
    {
        try
        {
            int resultado = Examenes.MinimoEstudio(intersecciones);

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