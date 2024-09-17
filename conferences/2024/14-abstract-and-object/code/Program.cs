namespace MatCom.Programming
{

    public abstract class BaseClass
    {
        public abstract void Display();
    }

    public class DerivedClass : BaseClass
    {
        public override void Display()
        {
            Console.WriteLine("Base Display");
        }
    }

    public class OtherDerivedClass : DerivedClass
    {
        public new virtual void Display()
        {
            Console.WriteLine("Derived Display");
        }
    }

    class Program
    {
        static void Main()
        {
            // BaseClass a = new BaseClass(); // error de compilación ;-)

            BaseClass b = new DerivedClass();
            b.Display();

            BaseClass c = new OtherDerivedClass();
            c.Display();

            DerivedClass d = new DerivedClass();
            d.Display();

            DerivedClass e = new OtherDerivedClass();
            e.Display();

            OtherDerivedClass f = new OtherDerivedClass();
            f.Display();

            Rectangle rect = new Rectangle(100, 200, 30, 40);
            Circle circ = new Circle(new Point(300, 300), 100);
            Console.WriteLine(rect);
            Console.WriteLine(circ);
        }
    }


}
