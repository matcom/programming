using MatCom.Sorting;
using System.Text;


class Program
{
    static int[] GetRandomArray(int length)
    {
        Random r = new Random();
        int[] array = new int[length];

        for (int i = 0; i < length; i++)
        {
            array[i] = r.Next(2 * length);
        }

        return array;
    }

    static string Format(int[] array)
    {
        StringBuilder sb = new StringBuilder("[");

        for (int i = 0; i < array.Length; i++)
        {
            sb.Append(array[i]);

            if (i < array.Length - 1)
                sb.Append(", ");
        }

        sb.Append("]");
        return sb.ToString();
    }

    static void Main()
    {
        for (int length = 1; length <= 1000; length = (int)(length * 2.5))
        {
            Test(length, Sort.BubbleSort);
            Test(length, Sort.MinSort);
            Test(length, Sort.InsertionSort);
            Test(length, Sort.MergeSort);
        }
    }

    static void Test(int length, Action<int[]> method)
    {
        int[] array = GetRandomArray(length);
        method(array);

        if (!Sort.IsSorted(array))
            throw new Exception(String.Format("Array {0} is not sorted!", Format(array)));
    }
}