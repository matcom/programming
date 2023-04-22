using MatCom.Exam;
using filesystem;
namespace MatCom.Tester;

public class Tester : TesterBase<int, int, IFileSystem, IFileSystem>
{
    public Func<int, IFileSystem, IFileSystem, bool>[] Tests = new Func<int, IFileSystem, IFileSystem, bool>[]
    {
        Test1.CreateFolderAndFileTest,
        Test2.GetFolderAndFileTest,
        Test3.CopyTest,
        Test4.DeleteTest,
        Test5.MoveTest,
        Test6.CopyTest2,
        Test7.MoveTest2,
    };

    public override int InputGenerator(int seed, int arg)
    {
        // var random = new Random(seed);

        // Selecting value node generator and predicate
        return seed;
    }

    public override bool OutputChecker(int input, IFileSystem output, IFileSystem expectedOutput)
    {
        var testCase = Tests[input%Tests.Length];
        return testCase(input, expectedOutput, output);
    }

    public override IFileSystem OutputGenerator(int input) => new MatCom.Tester.MyFileSystem();
}
