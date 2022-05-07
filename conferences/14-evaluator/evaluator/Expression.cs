namespace Weboo.Evaluator;
public abstract class Expresion {
    public abstract double Result();
    public abstract string Type();
    public abstract string TypeParent();
}

public class Constant : Expresion {
    double value;
    public Constant(double value) {
        this.value = value;
    }
    public override string Type() {
        return "Constant";
    }
    public override string TypeParent() {
        return "Constant";
    }
    public override double Result() {
        return this.value;
    }
    public override string ToString() {
        return $"{this.value}";
    }
}


#region UnaryExpresions
public abstract class UnaryExpresion : Expresion {
    protected Expresion value;
    public UnaryExpresion(Expresion value) {
        this.value = value;
    }
    public override string TypeParent() {
        return "UnaryExpresion";
    }
}

public class Sin : UnaryExpresion {
    public Sin(Expresion value) : base(value){}
    public override double Result() {
        return Math.Sin( this.value.Result() );
    }
    public override string ToString() {
        return $"Sin({ this.value.ToString() })";
    }
    public override string Type() {
        return "Sin";
    }
}
public class Cos : UnaryExpresion {
    public Cos(Expresion value) : base(value){}
    public override double Result() {
        return Math.Cos( this.value.Result() );
    }
    public override string ToString() {
        return $"Cos({ this.value.ToString() })";
    }
    public override string Type() {
        return "Cos";
    }
}
public class Absolute : UnaryExpresion {
    public Absolute(Expresion value) : base(value){}
    public override double Result() {
        return Math.Abs( this.value.Result() );
    }
    public override string ToString() {
        return $"|{ this.value.ToString() }|";
    }
    public override string Type() {
        return "Absolute";
    }
}

#endregion


#region BinaryExpresions
public abstract class BinaryExpresion : Expresion {
    protected Expresion left;
    protected Expresion right;

    public BinaryExpresion( Expresion left, Expresion right ) {
        this.left = left;
        this.right = right;
    }
    public override string TypeParent()
    {
        return "BinaryExpresion";
    }
}
public class Multiplication : BinaryExpresion {
    public Multiplication( Expresion left, Expresion right ) : base(left, right) {}
    public override double Result() {
        return this.left.Result() * this.right.Result();
    }
    public override string ToString() {
        string LType = this.left.Type();
        string LTypeParent = this.left.TypeParent();
        string RType = this.right.Type();
        string RTypeParent = this.right.TypeParent();

        string l = "", r = "";
        if( LType == "Constant" ) {
            l = this.left.ToString();
        } else
        if( LTypeParent == "UnaryExpresion" ) l = this.left.ToString();
        else
        if( LTypeParent == "BinaryExpresion" ) {
            if( LType == "Multiplication" || LType == "Division" || LType == "Power" ) l = this.left.ToString();
            else l = $"({this.left.ToString()})";
        }

        if( RType == "Constant" ) {
            if( this.right.Result() < 0 ) r = $"({ this.right.ToString() })";
            else r = this.right.ToString();
        } else
        if( RTypeParent == "UnaryExpresion" ) r = this.right.ToString();
        else
        if( RTypeParent == "BinaryExpresion" ) {
            if( RType == "Multiplication" || RType == "Division" || RType == "Power" ) r = this.right.ToString();
            else r = $"({this.right.ToString()})";
        }
        return l + "*" + r;
    }
    public override string Type() {
        return "Multiplication";
    }
}
public class Division : BinaryExpresion {
    public Division( Expresion left, Expresion right ) : base(left, right) {}
    public override double Result() {
        if( this.right.Result() == 0 ) 
            throw new  DivideByZeroException();
        return this.left.Result() / this.right.Result();
    }
    public override string ToString() {
        string LType = this.left.Type();
        string LTypeParent = this.left.TypeParent();
        string RType = this.right.Type();
        string RTypeParent = this.right.TypeParent();

        string l = "", r = "";
        if( LType == "Constant" ) {
            l = this.left.ToString();
        } else
        if( LTypeParent == "UnaryExpresion" ) l = this.left.ToString();
        else
        if( LTypeParent == "BinaryExpresion" ) {
            if( LType == "Multiplication" || LType == "Division" || LType == "Power" ) l = this.left.ToString();
            else l = $"({this.left.ToString()})";
        }

        if( RType == "Constant" ) {
            if( this.right.Result() < 0 ) r = $"({ this.right.ToString() })";
            else r = this.right.ToString();
        } else
        if( RTypeParent == "UnaryExpresion" ) r = this.right.ToString();
        else
        if( RTypeParent == "BinaryExpresion" ) {
            if( RType == "Multiplication" || RType == "Division" || RType == "Power" ) r = this.right.ToString();
            else r = $"({this.right.ToString()})";
        }
        return l + "/" + r;
    }
    public override string Type() {
        return "Division";
    }
}
public class Addition : BinaryExpresion {
    public Addition( Expresion left, Expresion right ) : base(left, right) {}
    public override double Result() {
        return this.left.Result() + this.right.Result();
    }
    public override string ToString() {
        string LType = this.left.Type();
        string LTypeParent = this.left.TypeParent();
        string RType = this.right.Type();
        string RTypeParent = this.right.TypeParent();

        string l = "", r = "";
        if( LType == "Constant" ) {
             l = this.left.ToString();
        } else
        if( LTypeParent == "UnaryExpresion" ) l = this.left.ToString();
        else
        if( LTypeParent == "BinaryExpresion" ) {
            if( LType == "Multiplication" || LType == "Division" || LType == "Power" ) l = this.left.ToString();
            else l = $"({this.left.ToString()})";
        }

        if( RType == "Constant" ) {
             r = this.right.ToString();
        } else
        if( RTypeParent == "UnaryExpresion" ) r = this.right.ToString();
        else
        if( RTypeParent == "BinaryExpresion" ) {
            if( RType == "Multiplication" || RType == "Division" || RType == "Power" ) r = this.right.ToString();
            else r = $"({this.right.ToString()})";
        }

        if( RType == "Constant" && this.right.Result() < 0 )
            return l + r;
        return l + "+" + r;
    }
    public override string Type() {
        return "Addition";
    }
}
public class Subtraction : BinaryExpresion {
    public Subtraction( Expresion left, Expresion right ) : base(left, right) {}
    public override double Result() {
        return this.left.Result() - this.right.Result();
    }
    public override string ToString() {
        string LType = this.left.Type();
        string LTypeParent = this.left.TypeParent();
        string RType = this.right.Type();
        string RTypeParent = this.right.TypeParent();

        string l = "", r = "";
        if( LType == "Constant" ) {
             l = this.left.ToString();
        } else
        if( LTypeParent == "UnaryExpresion" ) l = this.left.ToString();
        else
        if( LTypeParent == "BinaryExpresion" ) {
            if( LType == "Multiplication" || LType == "Division" || LType == "Power" ) l = this.left.ToString();
            else l = $"({this.left.ToString()})";
        }

        if( RType == "Constant" ) {
            r = this.right.ToString();
        } else
        if( RTypeParent == "UnaryExpresion" ) r = this.right.ToString();
        else
        if( RTypeParent == "BinaryExpresion" ) {
            if( RType == "Multiplication" || RType == "Division" || RType == "Power" ) r = this.right.ToString();
            else r = $"({this.right.ToString()})";
        }

        if( RType == "Constant" && this.right.Result() < 0 )
            return l + "+" + Math.Abs(this.right.Result());
        return l + "-" + r;
    }
    public override string Type() {
        return "Subtraction";
    }
}
public class Power : BinaryExpresion {
    public Power( Expresion left, Expresion right ) : base(left, right) {}
    public override double Result() {
        return Math.Pow(this.left.Result(), this.right.Result());
    }
    public override string ToString() {
        return $"Pow({ this.left.ToString() }, { this.right.ToString() })";
    }
    public override string Type() {
        return "Power";
    }
}

#endregion
