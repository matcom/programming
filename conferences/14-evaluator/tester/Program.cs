using Weboo.Evaluator;


class Program
{
    static void Main(string[] args)
    {
        Test1();
        Test2();
        Test3();
        Test4();
    }

    static void Test1() {

        Expresion e = new Division(
            new Sin(new Constant(4)),
            new Multiplication(
                new Subtraction(
                    new Cos(new Constant(7)),
                    new Constant(2)
                ),
                new Power(
                    new Constant(3),
                    new Constant(5)
                )
            )
        );

        Console.WriteLine($"{e.ToString()} = {e.Result()}");
    }
    static void Test2() {
        // | Sin( (2*5/3) + (3 - 4) ) * 1000 |
        Expresion exp1 = new Division( new Multiplication(new Constant(2), new Constant(5)), new Constant(3) );
        Expresion exp2 = new Subtraction(new Constant(3), new Constant(4));
        Expresion exp12 = new Addition(exp1, exp2);

        Expresion exp3 = new Multiplication( new Sin(exp12), new Constant(1000) );
        Expresion e = new Absolute( new Multiplication(exp3, new Constant(-1)) );
    
        Console.WriteLine($"{e.ToString()} = {e.Result()}");
    }

    static void Test3() {
        // (5 - (4 + 3)) / 2 + (-3)
        Expresion exp1 = new Division( new Subtraction( new Constant(5), new Addition(new Constant(4), new Constant(3)) ), new Constant(2) );
        Expresion e = new Addition( exp1, new Constant(-3) );

        Console.WriteLine($"{e.ToString()} = {e.Result()}");
    }
    static void Test4() {
        // -3 - (-3)
        Expresion e = new Subtraction( new Constant(-3), new Constant(-3) );
        Console.WriteLine($"{e.ToString()} = {e.Result()}");
    }

}