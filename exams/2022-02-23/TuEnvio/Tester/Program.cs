using Weboo.Examen;


public class Program
{
    public static void Main()
    {
        // Adicione aquí los tests que considere necesarios
        Test(
            // Pesos
            new[] { 20,15, 10, 13, 17},
            // Combustible
            new[,]
            {
                {  0, 10, 5,  1,  2 },
                { 10,  0, 3,  3,  4 },
                {  5,  3, 0,  2,  5 },
                {  1,  3, 2,  0, 15 },
                {  2,  4, 5, 15,  0 },
            },
            // Resultado esperado
            36
        );
    }

    public static void Test(int[] pesos, int[,] combustible, int esperado)
    {
        try {
            int resultado = TuEnvio.CombustibleDiario(pesos, combustible);

            if (resultado != esperado) {
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