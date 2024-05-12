using System;
using System.Collections.Generic;
using System.Text;


namespace WEBOO.Programacion
{
    class Program
    {
        static void Main(string[] args)
        {
            Cuenta juan = new Cuenta("Juan", 500);
            CuentaCredito mk = new CuentaCredito("Miguel", 400);
            Console.WriteLine("{0} tiene un saldo de {1}", juan.Titular, juan.Saldo);
            Tienda t = new Tienda();
            Producto tv = new Producto("TV Sony", 500);
            Producto tableta = new Producto("Tableta Samsung", 300);
            Producto refri = new Producto("Refrigerador LG", 700);

            Console.WriteLine("\nTienda vende TV a Juan");
            t.Vende(tv, juan);
            Console.WriteLine("{0} tiene un saldo de {1}", juan.Titular, juan.Saldo);

            // Sin virtual-override Miguel no iba a poder comprar en la tienda a pesar de ser poseedor de una cuenta de crédito
            // (porque con `new` se usaría la implementación del tipo estático, no del tipo dinámico).
            Console.WriteLine("\nTienda vende TV a Miguel");
            t.Vende(tv, mk);
            Console.WriteLine("{0} tiene un saldo de {1}", mk.Titular, mk.Saldo);
        }
    }
}
