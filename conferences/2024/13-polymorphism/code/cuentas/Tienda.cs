using System;
using System.Collections.Generic;
using System.Text;

namespace WEBOO.Programacion
{
    class Producto
    {
        public string Nombre { get; private set; }
        public float Precio { get; private set; }
        public Producto(string nombre, float precio)
        {
            Nombre = nombre; Precio = precio;
        }
    }
    class Tienda
    {
        public void Vende(Producto item, Cuenta cliente)
        {
            cliente.Extrae(item.Precio);
            Console.WriteLine(
                "Tienda vende {0} de precio {1} a {2}",
                item.Nombre,
                item.Precio,
                cliente.Titular
            );
        }
    }
}
