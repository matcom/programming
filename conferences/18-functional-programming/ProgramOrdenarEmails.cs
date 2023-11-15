
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

  #region METODOS DE ORDENACION
  public static class Utils
  {
    #region ORDENAR USANDO CON REPETICION DE CODIGO
    //Se supone que los campos por lo que se ordena implementan IComparable<T>
    public static void OrdenaPorRemitente(IList<Email> emails)
    {
      for (int i = 0; i < emails.Count - 1; i++)
        for (int j = i + 1; j < emails.Count; j++)
          if (emails[j].Remitente.CompareTo(emails[i].Remitente) < 0)
          {
            Email temp = emails[i]; emails[i] = emails[j]; emails[j] = temp;
          }
    }
    public static void OrdenaPorTema(IList<Email> emails)
    {
      for (int i = 0; i < emails.Count - 1; i++)
        for (int j = i + 1; j < emails.Count; j++)
          if (emails[j].Tema.CompareTo(emails[i].Tema) == -1)
          {
            Email temp = emails[i]; emails[i] = emails[j]; emails[j] = temp;
          }
    }
    public static void OrdenaPorFecha(IList<Email> emails)
    {
      for (int i = 0; i < emails.Count - 1; i++)
        for (int j = i + 1; j < emails.Count; j++)
          if (emails[j].FechaEnvio.CompareTo(emails[i].FechaEnvio) == -1)
          {
            Email temp = emails[i]; emails[i] = emails[j]; emails[j] = temp;
          }
    }
    public static void OrdenaPorTamaño(IList<Email> emails)
    {
      for (int i = 0; i < emails.Count - 1; i++)
        for (int j = i + 1; j < emails.Count; j++)
          if (emails[j].Tamaño.CompareTo(emails[i].Tamaño) == -1)
          {
            Email temp = emails[i]; emails[i] = emails[j]; emails[j] = temp;
          }
    }
    #endregion

    #region ORDENAR SUPONIENDO QUE EMAIL ES ICOMPARABLE
    //Limita a ordenar por un único criterio que se defina en Email
    public static void Ordena<T>(IList<T> a) where T : IComparable<T>
    {
      for (int i = 0; i < a.Count - 1; i++)
        for (int j = i + 1; j < a.Count; j++)
          if (a[j].CompareTo(a[i]) == -1)
          {
            T temp = a[i]; a[i] = a[j]; a[j] = temp;
          }
    }
    #endregion

    #region ORDENAR USANDO UN OBJETO ICOMPARER
    //Ver definicion de IComparer
    public static void Ordena<T>(IList<T> a, IComparer<T> comp)
    {
      for (int i = 0; i < a.Count - 1; i++)
        for (int j = i + 1; j < a.Count; j++)
          if (comp.Compare(a[j], a[i]) == -1)
          {
            T temp = a[i]; a[i] = a[j]; a[j] = temp;
          }
    }
    #endregion

    public static void Print(IList<Email> emails)
    {
      Console.WriteLine("{0,-15}{1,-37}{2,-15}{3,-10}", "REMITENTE", "TEMA", "FECHA", "TAMAÑO");
      Console.WriteLine("============================================================================");
      foreach (Email em in emails)
        Console.WriteLine("{0,-15}{1,-37}{2,-15}{3,-10}", em.Remitente, em.Tema, em.FechaEnvio, em.Tamaño);
    }
  }
  #endregion

  #region IMPLEMENTACIONES DE ICOMPARER
  class ComparerByEmailFechaEnvio : IComparer<Email>
  {
    public int Compare(Email em1, Email em2)
    {
      return em1.FechaEnvio.CompareTo(em2.FechaEnvio);
    }
  }
  class ComparerByEmailRemitente : IComparer<Email>
  {
    public int Compare(Email em1, Email em2)
    {
      return em1.Remitente.CompareTo(em2.Remitente);
    }
  }
  class ComparerByEmailTema : IComparer<Email>
  {
    public int Compare(Email em1, Email em2)
    {
      return em1.Tema.CompareTo(em2.Tema);
    }
  }
  class ComparerByEmailTamaño : IComparer<Email>
  {
    public int Compare(Email em1, Email em2)
    {
      return em1.Tamaño.CompareTo(em2.Tamaño);
    }
  }
  #endregion

  class ProgramOrdenarEmails
  {
    [STAThread]
    static void Main(string[] args)
    {
      #region POBLAR LISTA DE EMAILS
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

      #region PARA POBLAR MAS LA LISTA DE EMAILS
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

      #region Ordenar a lo bruto. Usando un método Ordena para cada criterio
      Console.WriteLine("\n\nORDENANDO DE MANERA DIRECTA");
      Console.WriteLine("\nPor Remitente...");
      Utils.OrdenaPorRemitente(emailList);
      Utils.Print(emailList);

      Console.WriteLine("\nPor Tema...");
      Utils.OrdenaPorTema(emailList);
      Utils.Print(emailList);

      Console.WriteLine("\nPor Fecha de Envio...");
      Utils.OrdenaPorFecha(emailList);
      Utils.Print(emailList);

      Console.WriteLine("\nPor Tamaño...");
      Utils.OrdenaPorTamaño(emailList);
      Utils.Print(emailList);
      #endregion

      #region Ordenar usando IComparable
      Console.WriteLine("\n\nORDENANDO POR QUIEN ES ICOMPARABLE (Fecha de Envío en este ejemplo)...");
      Utils.Ordena(emailList);//Usa el criterio de ordenación por ser Email IComparable
      Utils.Print(emailList);
      #endregion

      #region Ordenar dando un IComparer para cada criterio
      Console.WriteLine("\n\nORDENANDO con ICOMPARER");
      Console.WriteLine("\nPor remitente ...");
      Utils.Ordena(emailList, new ComparerByEmailRemitente());
      Utils.Print(emailList);
      Console.WriteLine("\nPor tema ...");
      Utils.Ordena(emailList, new ComparerByEmailTema());
      Utils.Print(emailList);
      Console.WriteLine("\nPor tamaño ...");
      Utils.Ordena(emailList, new ComparerByEmailTamaño());
      Utils.Print(emailList);
      Console.WriteLine("\nPor por fecha de envío ...");
      Utils.Ordena(emailList, new ComparerByEmailFechaEnvio());
      Utils.Print(emailList);
      #endregion
    }
  }
}

//CP Defina un método y un delegate que sirva para "filtrar" los elementos de un IEnumerable devolviendo los elementos del
//IEnumerable que cumplen con el delegado

//CP Defina un método y un delegado convertidor que permita "convertir" los elementos de un IEnumerable a un IEnumerable con los elementos
//convertidos

//CP Defina un método Mapea que dado un IEnumerable<T> y un convertidor de T a R devuelva el IEnumerable<R> con los elementos convertidos

//CP Defina un método y un delegado que permita "aplicar acumuladamente" el delegado a todos los elementos de un IEnumerable devolviendo el resultado de la acumulación
//Si el delegado fuera una suma sería el equivalente a sumar (si fuesen sumables) todos los elementos del IEnumerable original