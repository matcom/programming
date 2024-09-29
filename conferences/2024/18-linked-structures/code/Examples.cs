class Program
{
    public class Nodo<T>
    {
        public T Valor { get; set; }
        public List<Nodo<T>> Hijos { get; private set; }

        public Nodo(T valor)
        {
            Valor = valor;
            Hijos = new List<Nodo<T>>();
        }

        public void AgregarHijo(T valor)
        {
            Hijos.Add(new Nodo<T>(valor));
        }
    }
    static void Main()
    {
        Stack<int> pila = new Stack<int>();
        pila.Push(1);
        pila.Push(2);
        pila.Push(3);
        int cima = pila.Peek(); // cima = 3
        int elemento = pila.Pop(); // elemento = 3
        Console.WriteLine(cima); // 3
        Console.WriteLine(elemento); // 3
        Console.WriteLine(pila.Peek()); // 2

        Queue<int> cola = new Queue<int>();
        cola.Enqueue(1);
        cola.Enqueue(2);
        cola.Enqueue(3);
        int frente = cola.Peek(); // frente = 1
        int elemento2 = cola.Dequeue(); // elemento2 = 1
        Console.WriteLine(frente); // 1
        Console.WriteLine(elemento2); // 1
        Console.WriteLine(pila.Peek()); // 2

        Nodo<int> raiz = new Nodo<int>(1);
        raiz.AgregarHijo(2);
        raiz.AgregarHijo(3);
        raiz.Hijos[0].AgregarHijo(4);
    }
}
