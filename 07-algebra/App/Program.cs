using MatCom.Algetool;


class Program
{
    static void Main()
    {
        TestMatrixDefinition();
        TestSingletonMatrixDefinition();
        TestEmptyMatrixDefinition();

        TestSum();
        TestSumException();

        TestDotProduct1();
        TestDotProduct2();
        TestDotProductException();

        TestTranspose();
        TestSingletonTranspose();

        Console.WriteLine("✅ Everything OK!");
    }


    static void Assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    static void TestMatrixDefinition()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        Assert(matrix_a.Size == 6, "Should have 6 elements");
        Assert(matrix_a.Rows == 2, "Should have 2 rows");
        Assert(matrix_a.Columns == 3, "Should have 3 columns");
        System.Console.WriteLine("TestMatrixDefinition PASS");
    }

    static void TestSingletonMatrixDefinition()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 100 } });
        Assert(matrix_a.Size == 1, "Should have 1 elements");
        Assert(matrix_a.Rows == 1, "Should have 1 rows");
        Assert(matrix_a.Columns == 1, "Should have 1 columns");
        System.Console.WriteLine("TestSingletonMatrixDefinition PASS");
    }

    static void TestEmptyMatrixDefinition()
    {
        try
        {
            Matrix matrix_a = new Matrix(null);
            Assert(false, "Should not be able to create empty matrix");
        }
        catch (System.ArgumentException)
        {
            System.Console.WriteLine("TestEmptyMatrixDefinition PASS");
        }
    }

    static void TestSum()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        Matrix matrix_b = new Matrix(new double[,] { { 10, 20, 30 }, { 40, 50, 60 } });
        Matrix matrix_sum = new Matrix(new double[,] { { 11, 22, 33 }, { 44, 55, 66 } });

        Assert((matrix_a + matrix_b).Equals(matrix_sum), $"Expected result:\n {matrix_sum}");
        System.Console.WriteLine("TestSum PASS");
    }

    static void TestSumException()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        Matrix matrix_b = new Matrix(new double[,] { { 1, 1 }, { 2, 2 }, { 3, 3 } });

        try
        {
            Matrix matrix_sum = matrix_a + matrix_b;
            Assert(false, $"Should not be able to sum matrices of ({matrix_a.Rows} x {matrix_a.Columns}) and ({matrix_b.Rows} x {matrix_b.Columns})");
        }
        catch (System.ArgumentException)
        {
            System.Console.WriteLine("TestSumException PASS");
        }
    }

    static void TestDotProduct1()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        Matrix matrix_b = new Matrix(new double[,] { { 7, 8 }, { 9, 10 }, { 11, 12 } });
        Matrix matrix_dot = new Matrix(new double[,] { { 58, 64 }, { 139, 154 } });
        Assert((matrix_a * matrix_b).Equals(matrix_dot), $"Expected result:\n {matrix_dot}");
        System.Console.WriteLine("TestDotProduct1 PASS");
    }

    static void TestDotProduct2()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 1, 1 } });
        Matrix matrix_b = new Matrix(new double[,] { { 1 }, { 1 }, { 1 } });
        Matrix matrix_dot = new Matrix(new double[,] { { 3 } });
        Assert((matrix_a * matrix_b).Equals(matrix_dot), $"Expected result:\n {matrix_dot}");
        System.Console.WriteLine("TestDotProduct2 PASS");
    }

    static void TestDotProductException()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        Matrix matrix_b = new Matrix(new double[,] { { 1, 1, 1, 1 }, { 2, 2, 2, 2 } });
        try
        {
            Matrix matrix_dot = matrix_a * matrix_b;
            Assert(false, $"Should not be able to multiply matrices of ({matrix_a.Rows} x {matrix_a.Columns}) and ({matrix_b.Rows} x {matrix_b.Columns})");
        }
        catch (System.ArgumentException)
        {
            System.Console.WriteLine("TestDotProductException PASS");
        }
    }

    static void TestTranspose()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        Matrix matrix_trans = new Matrix(new double[,] { { 1, 4 }, { 2, 5 }, { 3, 6 } });
        matrix_a.Transpose();
        Assert(matrix_a.Equals(matrix_trans), $"Expected result:\n {matrix_trans}");
        System.Console.WriteLine("TestTranspose PASS");

    }
    static void TestSingletonTranspose()
    {
        Matrix matrix_a = new Matrix(new double[,] { { 100 } });
        Matrix matrix_trans = new Matrix(new double[,] { { 100 } });
        matrix_a.Transpose();
        Assert(matrix_a.Equals(matrix_trans), "Shold be the same matrix");
        System.Console.WriteLine("TestSingletonTranspose PASS");

    }

}