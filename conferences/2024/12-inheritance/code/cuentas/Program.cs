using System;
using System.Collections.Generic;
using System.Text;


namespace WEBOO.Programacion
{
    class Program
    {

        //Probar Herencia con ampliación, clases Cuenta y Cuenta con Transferencia
        static void ProbarCuentaConTransferencia()
        {
            Console.WriteLine("Prueba de Herencia en Código Fuente...");
            Console.WriteLine("Crea cuenta Juan con saldo inicial de 500");
            Cuenta cuentaJuan = new Cuenta("Juan", 500);
            Console.WriteLine("{0} tiene un saldo de {1}", cuentaJuan.Titular, cuentaJuan.Saldo);

            Console.WriteLine("\nEntra cantidad a depositar en la cuenta de Juan");
            int cantidad = int.Parse(Console.ReadLine());
            cuentaJuan.Deposita(cantidad);
            Console.WriteLine("{0} tiene un saldo de {1}", cuentaJuan.Titular, cuentaJuan.Saldo);

            Console.WriteLine("\nEntra cantidad a extraer de la cuenta de Juan (prueba también con una cantidad imposible)");
            cantidad = int.Parse(Console.ReadLine());
            cuentaJuan.Extrae(cantidad);
            Console.WriteLine("{0} tiene un saldo de {1}", cuentaJuan.Titular, cuentaJuan.Saldo);

            // Descomentar para ver error de compilación porque Cuenta no tiene transfiere
            // cuentaJuan.Transfiere(200);

            CuentaTransferencia cuentaLuis = new CuentaTransferencia("Luis", 100);
            Console.WriteLine("\n{0} tiene un saldo de {1}", cuentaLuis.Titular, cuentaLuis.Saldo);
            Console.WriteLine("Deposita 100");
            cuentaLuis.Deposita(100);
            Console.WriteLine("Extrae 20");
            cuentaLuis.Extrae(20);
            Console.WriteLine("\n{0} tiene un saldo de {1}", cuentaLuis.Titular, cuentaLuis.Saldo);

            Console.WriteLine("\nEntra cantidad a transferir de Luis a Juan");
            cantidad = int.Parse(Console.ReadLine());
            Console.WriteLine("...transferir {0} de {1} a {2}", cantidad, cuentaLuis.Titular, cuentaJuan.Titular);
            cuentaLuis.Transfiere(cantidad, cuentaJuan);
            Console.WriteLine("\n{0} tiene un saldo de {1}", cuentaJuan.Titular, cuentaJuan.Saldo);
            Console.WriteLine("\n{0} tiene un saldo de {1}", cuentaLuis.Titular, cuentaLuis.Saldo);
            Console.ReadLine();
        }

        //Para probar cambiar Cuenta por CuentaCredito
        static void ProbarCuentaDeCrédito()
        {
            Cuenta juan = new Cuenta("Juan", 1000);
            Console.WriteLine("{0} tiene un saldo de {1}", juan.Titular, juan.Saldo);
            Tienda t = new Tienda();
            Producto tv = new Producto("TV Sony", 500);
            Producto tableta = new Producto("Tableta Samsung", 300);
            Producto refri = new Producto("Refrigerador LG", 700);

            Console.WriteLine("\nTienda vende TV a Juan");
            t.Vende(tv, juan);
            Console.WriteLine("{0} tiene un saldo de {1}", juan.Titular, juan.Saldo);

            //Si se descomenta lo siguiente da excepción porque Juan no tiene saldo
            //Console.WriteLine("\nVeamos si Juan sin saldo puede comprar");
            //Console.ReadLine();
            //Console.WriteLine("Tienda vende Refrigerador a Juan");
            //t.Vende(refri, juan);
            //Console.WriteLine("{0} tiene un saldo de {1}", juan.Titular, juan.Saldo);

            CuentaCredito mk = new CuentaCredito("Miguel", 1000);
            TiendaCredito tc = new TiendaCredito();
            Console.WriteLine("{0} tiene un saldo de {1}", mk.Titular, mk.Saldo);

            Console.WriteLine("\nMiguel va a comprar TV en Tienda de Credito");
            tc.Vende(tv, mk);
            Console.WriteLine("{0} tiene un saldo de {1}", mk.Titular, mk.Saldo);

            Console.WriteLine("\nMiguel va a comprar Refrigerador en Tienda de Crédito");
            tc.Vende(refri, mk);
            Console.WriteLine("{0} tiene un saldo de {1}", mk.Titular, mk.Saldo);

            //Debe dar excepción lo siguiente, eso está bien que ocurra de esa manera  
            // Console.WriteLine("\nMiguel va a comprar Tableta en Tienda normal");
            // Console.ReadLine();
            // t.Vende(tableta, mk);

            //Pero tampoco alguien aunque tenga saldo puede comprar en la tienda de crédito
            //Error de compilación si se descomenta
            //tc.Vende(tableta, juan); 

            //Este no da error de compilación y puede comprar
            Console.WriteLine("\nJuan va a comprar Tableta en Tienda normal");
            t.Vende(tableta, juan);
            Console.WriteLine("{0} tiene un saldo de {1}", juan.Titular, juan.Saldo);
        }


        static void Main(string[] args)
        {
            ProbarCuentaConTransferencia();
            Console.WriteLine("-------------------");
            ProbarCuentaDeCrédito();
        }
    }
}
