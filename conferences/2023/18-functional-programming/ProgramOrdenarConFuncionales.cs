using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programacion
{
  #region CLASES FECHA y EMAIL
  public class Fecha : IComparable<Fecha>
  {
    public int Dia { get; set; }
    public int Mes { get; set; }
    public int Año { get; set; }
    public Fecha(int d, int m, int a)
    {
      Dia = d; Mes = m; Año = a;
    }
    public int CompareTo(Fecha f)
    {
      if (Año < f.Año) return -1;
      else if (Año > f.Año) return 1;
      else if (Mes < f.Mes) return -1;
      else if (Mes > f.Mes) return 1;
      else if (Dia < f.Dia) return -1;
      else if (Dia > f.Dia) return 1;
      else return 0;
    }
    public override string ToString()
    {
      return String.Format("({0,2},{1,2},{2,4})", Dia, Mes, Año);
    }
  }
  public class Email : IComparable<Email>
  {
    public string Remitente { get; set; }
    public string Tema { get; set; }
    public Fecha FechaEnvio { get; set; }
    public int Tamaño { get; set; }
    //Limita la comparación solo a la fecha de envío
    public int CompareTo(Email em)
    {
      //Hay que poner explicitamente el criterio por el que se va a comparar
      //En este ejemplo hemos usado FechaEnvio
      return FechaEnvio.CompareTo(em.FechaEnvio);
    }
  }
  #endregion

  delegate int Compare<T>(T e1, T e2);
  static class Utils
  {
    public static void Print(IList<Email> emails)
    {
      Console.WriteLine("{0,-15}{1,-37}{2,-15}{3,-10}", "REMITENTE", "TEMA", "FECHA", "TAMAÑO");
      Console.WriteLine("============================================================================");
      foreach (Email em in emails)
        Console.WriteLine("{0,-15}{1,-37}{2,-15}{3,-10}", em.Remitente, em.Tema, em.FechaEnvio, em.Tamaño);
    }

    //public static void Ordena<T>(IList<T> a, Compare<T> comp)
    //{
    //  for (int i = 0; i < a.Count - 1; i++)
    //    for (int j = i + 1; j < a.Count; j++)
    //      if (comp(a[j], a[i]) < 0)
    //      {
    //        T temp = a[i]; a[i] = a[j]; a[j] = temp;
    //      }
    //}

    //Comentar el anterior y probar descomentando este comentando region 1
    public static void Ordena<T>(IList<T> a, Func<T, T, int> comp)
    {
      //Ver definicion de Func que es delegate R Func<T1, T2, R>(T1 x1, T2 x2);
      for (int i = 0; i < a.Count - 1; i++)
        for (int j = i + 1; j < a.Count; j++)
          if (comp(a[j], a[i]) < 0)
          {
            T temp = a[i]; a[i] = a[j]; a[j] = temp;
          }
    }

    #region Métodos comparadores para usar como delegados
    public static int CompareRemitente(Email x, Email y)
    {
      return x.Remitente.CompareTo(y.Remitente);
    }
    public static int CompareTema(Email x, Email y)
    {
      return x.Tema.CompareTo(y.Tema);
    }
    public static int CompareFechaEnvio(Email x, Email y)
    {
      return x.FechaEnvio.CompareTo(y.FechaEnvio);
    }
    public static int CompareTamaño(Email x, Email y)
    {
      return x.Tamaño.CompareTo(y.Tamaño);
    }
    #endregion
  }

  class ProgramOrdenarConFuncionalesEmails
  {
    [STAThread]
    static void Main(string[] args)
    {
      #region Poblar directamente una lista de emails
      IList<Email> emailList = new List<Email>();
      Email em;

      em = new Email();
      em.Remitente = "Juan";
      em.Tema = "Fotos de las vacaciones";
      em.FechaEnvio = new Fecha(16, 5, 2023);
      em.Tamaño = 200;
      emailList.Add(em);

      em = new Email();
      em.Remitente = "Ana";
      em.Tema = "Sobre la próxima reunión";
      em.FechaEnvio = new Fecha(2, 6, 2023);
      em.Tamaño = 50;
      emailList.Add(em);

      em = new Email();
      em.Remitente = "Miguel";
      em.Tema = "Borrador de artículo";
      em.FechaEnvio = new Fecha(10, 5, 2023);
      em.Tamaño = 150;
      emailList.Add(em);

      em = new Email();
      em.Remitente = "Paula";
      em.Tema = "Proyecto Programación";
      em.FechaEnvio = new Fecha(20, 3, 2023);
      em.Tamaño = 100;
      emailList.Add(em);
      #endregion

      #region Añadir mas elementos interactivamente a la lista de emails
      //while (true)
      //{
      //  em = new Email();
      //  Console.Write("\nNombre de remitente ");
      //  em.Remitente = Console.ReadLine();
      //  if (em.Remitente.Length == 0) break;

      //  Console.Write("Tema ");
      //  em.Tema = Console.ReadLine();

      //  Console.Write("Dia de envío ");
      //  int d = int.Parse(Console.ReadLine());
      //  Console.Write("Mes de envío ");
      //  int m = int.Parse(Console.ReadLine());
      //  Console.Write("Año de envío ");
      //  int a = int.Parse(Console.ReadLine());
      //  em.FechaEnvio = new Fecha(d, m, a);

      //  Console.Write("Tamaño del Mensaje ");
      //  em.Tamaño = int.Parse(Console.ReadLine());

      //  emailList.Add(em);
      //}
      #endregion

      Console.WriteLine("\nLISTA ORIGINAL DE EMAILS ...");
      Utils.Print(emailList);

      #region 1-Ordenar estando definidos los metodos
      //Comentar esta region cuando se va a probar el Func
      //Console.WriteLine("\n\nORDENANDO CON DELEGATES EXPLÍCITOS");
      //Console.WriteLine("\nPor remitente ...");
      //Utils.Ordena(emailList, new Compare<Email>(Utils.CompareRemitente));
      //Utils.Print(emailList);

      //Console.WriteLine("\nPor Fecha de Envío ...");
      //Utils.Ordena(emailList, new Compare<Email>(Utils.CompareFechaEnvio));
      //Utils.Print(emailList);
      #endregion

      #region Ordenar pasando directamente la definicion del metodo como parametro
      Console.WriteLine("\n\nORDENANDO CON DELEGATES ANONIMOS");
      Console.WriteLine("\nPor TEMA ...");
      Utils.Ordena(emailList, delegate (Email e1, Email e2) { return e1.Tema.CompareTo(e2.Tema); });
      Utils.Print(emailList);
      #endregion

      #region Ordenar Usando expresiones lambda
      Console.WriteLine("\n\nORDENANDO CON EXPRESIONES LAMBDA");
      Console.WriteLine("\nPor TAMAÑO poniendo explicitamente el tipo a los parametros...");
      Utils.Ordena(emailList, (Email e1, Email e2) => { return e1.Tamaño.CompareTo(e2.Tamaño); });
      Utils.Print(emailList);

      Console.WriteLine("\nPor FECHAENVÍO infiriendo el tipo de los parametros");
      //Notar que no hay que poner el tipo de los parámetros porque en este caso los infiere
      //Notar que vale poner solo la expresión que calcula el valor sin poner el cuerpo de un método con return
      Utils.Ordena(emailList, (e1, e2) => e1.FechaEnvio.CompareTo(e2.FechaEnvio));
      Utils.Print(emailList);
      #endregion
    }
  }
}

#region EJERCICIOS CLASE PRACTICA
//CP Defina un método Filtrar que a partir de un IEnumerable<T> y un
//delegate que exprese una condicion que deben cumplir los valores T
//sirva para "filtrar" los elementos del IEnumerable devolviendo
//un IEnumerable con los elemento de tipo T que cumplen con la condicion

//CP Defina un método Map que sobre un IEnumerable<T> y un delegado que
//exprese una forma de convertir de T a R devuelva un Ienumerable<R>
//con los elementos convertidos a R

//CP Defina un método que sobre un IEnumerable<T> y un delegado que permita
//"aplicar acumuladamente" a los valores de tipo T devuelva
//un valor de tipo T con el resultado de la acumulacion
//Ejemplo si T es int y el delegado suma dos int y devuelve Acumula devuelve
//la suma de todos los int del IEnumerable<int> original
#endregion
