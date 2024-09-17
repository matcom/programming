using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Programacion
{
    class StringEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            return x.ToUpper().Equals(y.ToUpper());
        }

        public int GetHashCode(string obj)
        {
            return obj.ToUpper().GetHashCode();

            //Comentar arriba y descomentar abajo
            //Ver que funciona
            //Llaves iguales deben tener el mismo GetHashCode
            //pero llaves diferentes tambien pueden tener un mismo GetHashCode
            //es un concepto que se usa para eficiencia de implementacion como se vera
            //mas adelante

            //return 1000;
        }
    }
    static class ProgramIqualityComparer
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Ver la sobrecarga del constructor de Dictionary
            Dictionary<string, string> agenda =
                new Dictionary<string, string>(new StringEqualityComparer());

            string nombre, telefono;
            while (true)
            {
                #region 
                Console.Write("\nEntre nombre: ");
                nombre = Console.ReadLine();
                if (nombre.Length == 0) break;

                if (agenda.ContainsKey(nombre)) //Aquí estaría buscando dos veces. Primero por el Contains
                    Console.WriteLine("{0} ya esta en agenda su num es {1}", nombre, agenda[nombre]);
                else
                {
                    Console.Write("Entra su numero de telefono: ");
                    telefono = Console.ReadLine();
                    agenda.Add(nombre, telefono);
                }
            }
            #endregion

            #region RECORRER DICCIONARIO ESCRIBIENDO SU GETHASHCODE
            //Ver las tres formas de hacer lo mismo
            Console.WriteLine("\nLISTANDO LOS NOMBRES DE LA AGENDA\n");
            Console.WriteLine("\nListar tambien el GetHashCode");
            foreach ((string name, string phone) in agenda)
                Console.WriteLine("  {0, -20}{1, -20}{2, -20}", name, phone, name.GetHashCode());
            #endregion
        }
    }
}
#region EJERCICIOS PARA CLASE PRACTICA
//1) Implemente un metodo que dado un diccionario dicc de tipo Dictionary<TKet, TValue)
//devuelva un IEnumerable de la forma IEnumerable<TValue, IEnumerable<TKey>>
//donde IEnumerable<TKey> regresenta a la coleccion de todos los valores que tienen
//La misma llave en el diccionario original
#endregion