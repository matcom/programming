﻿using System;
namespace Accounting
{
    public class Cart
    {
        List<Product> products;

        public Cart()
        {
            this.products = new List<Product>();
        }

        public virtual int TotalCost()
        {
            int cost = 0;

            foreach (var item in this.Products())
            {
                cost += item.TotalCost();
            }

            return cost;
        }

        public virtual IEnumerable<Product> Products()
        {
            foreach (var product in this.products)
            {
                yield return product;
            }
        }

        public void Add(Product product)
        {
            int index = products.FindIndex(p => p.Name == product.Name && p.GetType() == product.GetType());
            if (index == -1)
                this.products.Add();
            else
                this.products[index].AddUnits(product.Units);
        }
    }

    public class CartWithShipment : Cart
    {
        private int freeCost;

        private int costPerUnit;

        public CartWithShipment(int costPerUnit, int freeCost)
        {
            this.costPerUnit = costPerUnit;
            this.freeCost = freeCost;
        }

        public override IEnumerable<Product> Products()
        {
            int cost = 0;
            int units = 0;

            foreach (var product in base.Products())
            {
                cost += product.TotalCost();
                units += product.Units;
                yield return product;
            }

            if (cost >= freeCost)
            {
                yield return new Product("🚙 Shipment", 0, units);
            }
            else
            {
                yield return new Product("🚙 Shipment", this.costPerUnit, units);
            }
        }
    }
}
