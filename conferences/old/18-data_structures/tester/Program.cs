using System.Diagnostics;
using DataStructures;

class Program
{
    static void Main()
    {
        Lista<int> l = new Lista<int>();
        int N = 150;

        for (int i = 0; i < N; i++)
        {
            l.Add(i);
        }

        int count = l.Count;
        Debug.Assert(count == N, $"La lista debería tener {N} elementos");

        for (int i = 0; i < N; i++)
        {
            Debug.Assert(l.Contains(i), $"La lista debe tener el elemento {i}");
        }

        int x = l[20];
        Debug.Assert(x == 20, $"El elemento debe ser 20");

        try
        {
            int x2 = l[175];
            Debug.Assert(false, $"El elemento 75 no puede existir");
        }
        catch (IndexOutOfRangeException)
        {
            // OK
        }

        l.Remove(25);

        Debug.Assert(!l.Contains(25));
    }
}