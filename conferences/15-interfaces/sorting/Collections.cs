namespace MatCom.Sorting.Collections
{
    public class ArrayCollection : ICollection
    {
        private readonly object[] items;

        public ArrayCollection(object[] items)
        {
            this.items = items;
        }

        public int Count
        {
            get { return this.items.Length; }
        }

        public object this[int index]
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
}