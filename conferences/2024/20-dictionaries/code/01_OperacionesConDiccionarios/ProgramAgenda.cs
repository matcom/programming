using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Programacion
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            Dictionary<string, string> agenda = new Dictionary<string, string>();

            string nombre, telefono;
            while (true)
            {
                #region CREANDO AGENDA VERSION 1
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
                #endregion

                #region VERSION 2 UN POCO MÁS EFICIENTE
                //Console.Write("\nEntra nombre en inglés (Enter para terminar): ");
                //nombre = Console.ReadLine();
                //if (nombre.Length == 0) break;
                //if (agenda.TryGetValue(nombre, out telefono)) //Si está devuelve true y a la vez el telefono en el parámetro
                //  Console.WriteLine("{0} => {1}", nombre, telefono);

                //else
                //{
                //  Console.Write("Entre su numero de telefono: ");
                //  telefono = Console.ReadLine();
                //  if (telefono.Length == 0) continue;
                //  else
                //    agenda.Add(nombre, telefono);
                //}
                #endregion

            }
            //Ejecutar el codigo anterior pero entrando el nombre con alguna mayuscula
            //Dictionary se basa en el Equals de string que es sensible a la diferencia
            //y por tanto juan no es igual a Juan

            #region PRUEBA DE EXCEPCIÓN POR USAR LLAVE INEXISTENTE
            //Probar con un nombre que se diferencie en las mayusc minus
            //supongamos que esta juan pero no Juan
            //lo siguiente debe dar excepcion

            //Console.WriteLine(agenda["Juan"]);
            #endregion

            #region QUITANDO DEL DICCIONARIO
            //while (true)
            //{
            //  Console.Write("\nEntre nombre a quitar: ");
            //  nombre = Console.ReadLine();
            //  if (nombre.Length == 0) break;
            //  agenda.Remove(nombre);
            //  //Si no esta da excepcion

            //  ////De esta forma no da excepción si no está
            //  //Console.WriteLine("{0} es {1}",
            //  //                  nombre,
            //  //                  agenda.TryGetValue(nombre, out telefono) ? telefono : "No esta en la agenda");
            //}
            #endregion

            #region RECORRER DICCIONARIO COMO IENUMERABLE
            //Ver las tres formas de hacer lo mismo
            //Console.WriteLine("\nLISTANDO LOS NOMBRES DE LA AGENDA\n");
            //Console.WriteLine(" Listar recorriendo los KeyValuePairs");
            //foreach (KeyValuePair<string, string> kv in agenda)
            //  Console.WriteLine("  {0, -20}{1, -20}", kv.Key, kv.Value);

            //Console.WriteLine("\nListar infiriendo el tipo segun la parte derecha");
            //foreach (var kv1 in agenda)
            //  Console.WriteLine("  {0, -20}{1, -20}", kv1.Key, kv1.Value);

            //Console.WriteLine("\nListar deconstruyendolo como tuplo");
            //foreach ((string name,string  phone) in agenda)
            //  Console.WriteLine("  {0, -20}{1, -20}", name, phone);
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

