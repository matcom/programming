using Weboo.Evaluator;


class Program
{
    static void Main(string[] args)
    {
        Expression e = GetExpression();
        Console.WriteLine($"{e} = {e.Evaluate()}");
    }

    static Expression GetExpression() {
        return new Divide(
            new Sin(new Constant(4)),
            new Multiply(
                new Subtract(
                    new Cos(new Constant(7)),
                    new Exp(new Constant(2))
                ),
                new Pow(
                    new Constant(3),
                    new Constant(5)
                )
            )
        );
    }
}