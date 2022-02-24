﻿using Weboo.Examen;


public class Program
{
    public static void Main()
    {
        // Adicione aquí los tests que considere necesarios
        Test(
            tareas: new[] { 5, 10, 16 },
            desarrolladores: new double[,]
            {
                { 1.0, 0.5, 2.0 },
                { 2.0, 1.0, 0.5 },
            },
            esperado: 9
        );
    }

    public static void Test(int[] tareas, double[,] desarrolladores, double esperado)
    {
        try {

            double resultado = Manager.DuracionProyecto(tareas, desarrolladores);
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