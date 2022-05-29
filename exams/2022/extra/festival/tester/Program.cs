using Weboo.Examen;


public class Program
{
    public static void Main()
    {
        // Adicione aquí los tests que considere necesarios

        // Todos son amigos de todos, así que serían 3 equipos de una persona
        Test(
            // Amigos
            new[,]
            {
                {false,  true,  true},
                { true, false,  true},
                { true,  true, false},
            },
            // Resultado esperado
            3
        );

        // Dos conjuntos disjuntos de amigos, se queda un equipo
        // con una sola persona que no se cuenta
        Test(
            // Amigos
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
            2
        );

        // Nadie conoce a nadie
        Test(
            // Amigos
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

    public static void Test(bool[,] amigos, int esperado)
    {
        try
        {
            int resultado = Festival.MenorCantidadEquipos(amigos);

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