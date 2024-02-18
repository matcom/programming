using MatCom.Logic;


class Program
{
    static void Main()
    {
        TestEmptySetHasSizeZero();
        TestSingletonSetHasSizeOne();
        TestContains();
        TestSetRemovesDuplicates();

        TestUnionOfDisjointSets();
        TestUnionOfSameElements();
        TestUnionOfSameAndDifferentElements();

        TestIntersection();

        Console.WriteLine("✅ Everything OK!");
    }

    static void Assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    static void TestEmptySetHasSizeZero()
    {
        Set s = new Set();
        Assert(s.Size == 0, "Size should be 0, not " + s.Size);
    }

    static void TestSingletonSetHasSizeOne()
    {
        Set s = new Set(42);
        Assert(s.Size == 1, "Size should be 1, not " + s.Size);
    }

    static void TestContains()
    {
        Set s = new Set(1, 2, 3);

        Assert(s.Contains(1), "Should contain 1");
        Assert(s.Contains(2), "Should contain 2");
        Assert(s.Contains(3), "Should contain 3");
        Assert(!s.Contains(4), "Should not contain 4");
    }

    static void TestSetRemovesDuplicates()
    {
        Set s = new Set(1, 2, 1, 3, 2);

        Assert(s.Size == 3, "Size should be 3, not " + s.Size);
    }

    static void TestUnionOfDisjointSets()
    {
        Set s1 = new Set(1, 2, 3);
        Set s2 = new Set(4, 5, 6, 7);
        Set s3 = s1.Union(s2);

        Assert(s3.Size == s1.Size + s2.Size, "Should have all of the elements");
    }

    static void TestUnionOfSameElements()
    {
        Set s1 = new Set(1, 2, 3);
        Set s2 = new Set(1, 2, 3);
        Set s3 = s1.Union(s2);

        Assert(s3.Size == s1.Size, "Should have exactly one copy of the elements");
    }

    static void TestUnionOfSameAndDifferentElements()
    {
        Set s1 = new Set(1, 2, 3);
        Set s2 = new Set(1, 2, 3, 4);
        Set s3 = s1.Union(s2);

        Assert(s3.Size == Math.Max(s1.Size, s2.Size), "Should have exactly one copy of the elements");
    }

    static void TestIntersection()
    {
        Set s1 = new Set(1, 2, 3, 4, 5);
        Set s2 = new Set(2, 4, 6, 8);
        Set s3 = s1.Intersection(s2);

        Assert(s3.Size == 2, "Should contain only 2 and 4");
    }
}