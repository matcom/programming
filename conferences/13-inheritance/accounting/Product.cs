namespace Accounting;

public class Product
{
    public Product(string name, int price, int units)
    {
        this.Name = name;
        this.Price = price;
        this.Units = units;
    }

    public string Name { get; private set; }
    public int Price { get; private set; }
    public int Units { get; private set; }

    public virtual int TotalCost()
    {
        return this.Price * this.Units;
    }

    public override string ToString()
    {
        return $"{this.Name} - ${this.Price} (x{this.Units}) = ${this.TotalCost()}";
    }
}

public class DiscountProduct : Product
{
    public double Discount { get; private set; }

    public DiscountProduct(string name, int price, int units, double discount) : base(name, price, units)
    {
        this.Discount = discount;
    }

    public override int TotalCost()
    {
        return (int)(base.TotalCost() * (1 - this.Discount));
    }

    public override string ToString()
    {
        return base.ToString() + $" [-{this.Discount * 100:0.0}%]";
    }
}
