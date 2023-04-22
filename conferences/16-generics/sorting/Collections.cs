namespace MatCom.Sorting.Collections;

public class ArrayCollection<T> : ICollection<T>
{
    private readonly T[] items;

    public ArrayCollection(T[] items)
    {
        this.items = items;
    }

    public int Count
    {
        get { return this.items.Length; }
    }

    public T this[int index]
    {
        get
        {
            return this.items[index];
        }
        set
        {
            this.items[index] = value;
        }
    }
}
