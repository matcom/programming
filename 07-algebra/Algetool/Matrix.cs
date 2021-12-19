using System;
using System.Text;

namespace MatCom.Algetool;

public class Matrix
{
    private double[,] elements;
    private bool transposed;

    // Constructor
    public Matrix(double[,] elements)
    {
        if (elements == null)
            throw new ArgumentException("The input matrix can't be null");

        this.elements = elements;
        this.transposed = false;
    }

    // Properties
    public int Size
    {
        get { return this.elements.Length; }
    }
    public int Rows
    {
        get
        {
            return this.transposed ? this.elements.GetLength(1) : this.elements.GetLength(0);
        }
    }
    public int Columns
    {
        get
        {
            return this.transposed ? this.elements.GetLength(0) : this.elements.GetLength(1);
        }
    }

    // Methods
    public void Transpose()
    {
        // what happened here 😱?
        this.transposed = true;
    }

    public override string ToString()
    {
        StringBuilder matrix_sb = new StringBuilder($"Matrix ({this.Rows}x{this.Columns}):");
        matrix_sb.AppendLine();

        for (int i = 0; i < this.Rows; i++)
        {
            if (i == 0)
                matrix_sb.AppendFormat("{0,-3}", "⌈");

            else if (i == this.Rows - 1)
                matrix_sb.AppendFormat("{0,-3}", "⌊");

            else
                matrix_sb.AppendFormat("{0,-3}", "|");


            for (int j = 0; j < this.Columns; j++)
            {
                matrix_sb.AppendFormat("{0,-4}", this[i, j]);
            }

            if (i == 0)
                matrix_sb.Append("⌉");

            else if (i == this.Rows - 1)
                matrix_sb.Append("⌋");

            else
                matrix_sb.Append("|");

            matrix_sb.AppendLine();
        }

        return matrix_sb.ToString();
    }

    public bool Equals(Matrix m)
    {
        if (this.Rows != m.Rows || this.Columns != m.Columns)
            return false;

        for (int i = 0; i < this.Rows; i++)
        {
            for (int j = 0; j < this.Columns; j++)
            {
                if (this[i, j] != m[i, j])
                    return false;
            }
        }
        return true;
    }
    // Indexer
    public double this[int r, int c]
    {
        get
        {
            return transposed ? this.elements[c, r] : this.elements[r, c];
        }
        set
        {
            if (transposed)
            {
                this.elements[c, r] = value;
            }
            else
            {
                this.elements[r, c] = value;
            }
        }
    }

    #region Static methods
    public static Matrix Sum(Matrix m1, Matrix m2)
    {
        // Verificar los valores de entrada
        CheckNullMatrix(m1);
        CheckNullMatrix(m2);

        if (m1.Rows != m2.Rows || m1.Columns != m2.Columns)
            throw new ArgumentException("Incompatible dimensions");

        // Crear el array para el resultado
        double[,] result = new double[m1.Rows, m1.Columns];

        // Recorrer las filas
        for (int i = 0; i < m1.Rows; i++)
            // Recorrer las columnas de una fila
            for (int j = 0; j < m1.Columns; j++)
                // Operar la suma
                result[i, j] = m1[i, j] + m2[i, j];

        return new Matrix(result);
    }

    public static Matrix ScalarProduct(double n, Matrix m)
    {
        // Verificar los valores de entrada
        CheckNullMatrix(m);

        // Crear el array para el resultado
        double[,] result = new double[m.Rows, m.Columns];

        // Recorrer las filas de m
        for (int i = 0; i < m.Rows; i++)
            // Recorrer las columnas de una fila de m
            for (int j = 0; j < m.Columns; j++)
                // Operar el producto escalar
                result[i, j] = m[i, j] * n;

        return new Matrix(result);
    }

    public static Matrix DotProduct(Matrix m1, Matrix m2)
    {
        // Verificar los valores de entrada
        CheckNullMatrix(m1);
        CheckNullMatrix(m2);

        if (m1.Columns != m2.Rows)
            throw new ArgumentException("Incompatible dimensions");

        // Crear el array resultado con la cantidad de filas de m1 y de columnas de m2 
        double[,] result = new double[m1.Rows, m2.Columns];

        // Recorrer las filas de m1
        for (int i = 0; i < m1.Rows; i++)
            // Recorrer las columnas de m2
            for (int j = 0; j < m2.Columns; j++)
            {
                // Dada una fila i de m1 y una columna j de m2, calcular el producto escalar de la fila por la columna
                for (int k = 0; k < m1.Columns; k++)
                    result[i, j] += m1[i, k] * m2[k, j];
            }

        return new Matrix(result);
    }
    #endregion

    #region Redefinition of operators
    public static Matrix operator +(Matrix a, Matrix b)
    {
        return Sum(a, b);
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        return DotProduct(a, b);
    }

    public static Matrix operator *(double a, Matrix b)
    {
        return ScalarProduct(a, b);
    }

    public static Matrix operator *(Matrix a, double b)
    {
        return ScalarProduct(b, a);
    }

    #endregion

    #region Helper methods
    private static void CheckNullMatrix(Matrix m)
    {
        // Verificar los valores de entrada
        if (m == null)
        {
            throw new ArgumentException("Operands cannot be null");
        }
    }
    #endregion
}

/* EJERCICIOS

1) Redefine el operador `==` de manera que sea posible comparar dos matrices de la forma `matrix_a == matrix_b`. 
   El resultado será `true` si ambas continen los mismos elementos en las mismas posiciones, de lo contrario debe 
   evaluar `false`.
   Hint: Para ello implementa el método `public static Matrix operator ==(Matrix a, Matrix b)`.
   Puedes reutilizar métodos implementados.

2) Adicione un método de instancia `bool IsSymmetrical(Matrix m)` que devuelve `true` si y solo si la matriz es una matriz
   simétrica, e implemente los tests que considere necesarios para evaluarlo.

3) Adicione un método estático `static Matrix Parse(string matrix_str)` que dado un string con el formato:
   <x1,...,xn | xn+1,...,x2n | ...> parsee el contenido y retorne la matriz correspondiente. Implemente los tests que 
   considere necesarios para evaluarlo.
   
   Ejemplo:
   A partir del string "<1,2,3 | 4,5,6 | 7,8,9>"  se obtiene la matriz:
   
   ⌈  1   2   3  ⌉
   |  4   5   6  |
   ⌊  7   8   9  ⌋

*/ 