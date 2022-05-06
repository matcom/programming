namespace Weboo.Evaluator
{

    public abstract class Expression
    {
        public abstract double Evaluate();
    }

    public abstract class BinaryExpression : Expression
    {
        protected readonly Expression left;
        protected readonly Expression right;

        public BinaryExpression(Expression left, Expression right)
        {
            this.left = left;
            this.right = right;
        }

        public override double Evaluate()
        {
            double leftValue = this.left.Evaluate();
            double rightValue = this.right.Evaluate();

            return this.Evaluate(leftValue, rightValue);
        }

        protected abstract double Evaluate(double left, double right);
    }

    public class Add : BinaryExpression
    {
        public Add(Expression left, Expression right) : base(left, right)
        {

        }

        protected override double Evaluate(double left, double right)
        {
            return left + right;
        }

        public override string ToString()
        {
            return $"({left.ToString()}) + ({right.ToString()})";
        }
    }

    public class Subtract : BinaryExpression
    {
        public Subtract(Expression left, Expression right) : base(left, right)
        {

        }

        protected override double Evaluate(double left, double right)
        {
            return left - right;
        }

        public override string ToString()
        {
            return $"({left.ToString()}) - ({right.ToString()})";
        }
    }

    public class Multiply : BinaryExpression
    {
        public Multiply(Expression left, Expression right) : base(left, right)
        {

        }

        protected override double Evaluate(double left, double right)
        {
            return left * right;
        }

        public override string ToString()
        {
            return $"({left.ToString()}) * ({right.ToString()})";
        }
    }

    public class Divide : BinaryExpression
    {
        public Divide(Expression left, Expression right) : base(left, right)
        {

        }

        protected override double Evaluate(double left, double right)
        {
            return left / right;
        }

        public override string ToString()
        {

            return $"({left.ToString()}) / ({right.ToString()})";
        }
    }

    public class Pow : BinaryExpression
    {
        public Pow(Expression left, Expression right) : base(left, right)
        {

        }

        protected override double Evaluate(double left, double right)
        {
            return Math.Pow(left, right);
        }

        public override string ToString()
        {
            return $"({left.ToString()}) ^ ({right.ToString()})";
        }
    }

    public abstract class UnaryExpression : Expression
    {
        protected readonly Expression inner;

        public UnaryExpression(Expression inner)
        {
            this.inner = inner;
        }

        public override double Evaluate()
        {
            return this.Evaluate(this.inner.Evaluate());
        }

        protected abstract double Evaluate(double inner);
    }

    public class Exp : UnaryExpression
    {
        public Exp(Expression inner) : base(inner)
        {

        }

        protected override double Evaluate(double inner)
        {
            return Math.Exp(inner);
        }

        public override string ToString()
        {
            return $"e^({inner.ToString()})";
        }
    }

    public class Sin : UnaryExpression
    {
        public Sin(Expression inner) : base(inner)
        {

        }

        protected override double Evaluate(double inner)
        {
            return Math.Sin(inner);
        }

        public override string ToString()
        {
            return $"sin({inner.ToString()})";
        }
    }

    public class Cos : UnaryExpression
    {
        public Cos(Expression inner) : base(inner)
        {

        }

        protected override double Evaluate(double inner)
        {
            return Math.Cos(inner);
        }

        public override string ToString()
        {
            return $"cos({inner.ToString()})";
        }
    }

    public class Constant : Expression
    {
        double value;

        public Constant(double value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override double Evaluate()
        {
            return this.value;
        }
    }
}
