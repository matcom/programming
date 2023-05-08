
using MatCom.Utils;
using MatCom.Tester;
using System.Diagnostics;

Directory.CreateDirectory(".output");
File.Delete(Path.Combine(".output", "result.md"));
File.WriteAllLines(Path.Combine(".output", "result.md"), new[]
{
    "# Results of MatCom Programming Contest #1",
    "",
    "| Student name | Approved |  1️⃣ |  2️⃣ |  3️⃣ |  4️⃣ |  5️⃣ |  6️⃣ |  7️⃣ |  8️⃣ |  9️⃣ | 1️⃣0️⃣ | 1️⃣1️⃣ | 1️⃣2️⃣ | 1️⃣3️⃣ | 1️⃣4️⃣ | 1️⃣5️⃣ | 1️⃣6️⃣ |",
    "| ------------ | ------ | --- | --- | --- | --- | --- | --- | --- | --- | --- | ----- | ---- | ----- | --- | ---- | ---- | ----- |",
});

foreach (var solution in Directory.GetFiles("solutions", "*.cs"))
{
    var oldFiles = Directory
        .EnumerateFiles("tester", "*.*", SearchOption.AllDirectories)
        .Where(f => Path.GetFileName(f) != "tester.csproj")
        .Where(f => Path.GetFileName(f) != "Inventory.cs")
        .Where(f => Path.GetFileName(f) != "UnitTest.cs")
        .Where(f => Path.GetFileName(f) != "Utils.cs");
    foreach (var oldFile in oldFiles) File.Delete(oldFile);
    
    File.Copy(solution, Path.Combine("tester", "Solution.cs"));
    
    var name = Path.GetFileNameWithoutExtension(solution);
    
    var info = new ProcessStartInfo("dotnet", "test --logger trx");
    
    var process = Process.Start(info);
    
    process?.WaitForExit();
    
    var dict = new Dictionary<TestType,bool>();

    try
    {
        var trx = Directory.GetFiles("tester/TestResults", "*.trx").Single();
        File.Copy(trx, Path.Combine(".output", $"{name}.trx"));
        dict = TestResult.GetResults($".output/{name}.trx");
       // Directory.Delete("Tester/TestResults", true);
    }
    catch 
    {
        File.AppendAllLines(Path.Combine(".output", "result.md"), new[]
        {
            $"| {name} | {( "🔴" )} | { "⚠️" } | { "⚠️" } " + 
            $"| { "⚠️" } | { "⚠️" } | { "⚠️" } | { "⚠️" } | { "⚠️" } " + 
            $"| { "⚠️" } | { "⚠️" } | { "⚠️" } | { "⚠️" } | { "⚠️" } " + 
            $"| { "⚠️" } | { "⚠️" } | { "⚠️" } | { "⚠️" } |" 
        });

        continue;
    }

    File.AppendAllLines(Path.Combine(".output", "result.md"), new[]
    {
        $"| {name} | {( TestResult.IsApproved(dict) ? "🟢" : "🔴" )} " + 
        $"| {( dict[TestType.RootCase] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.CreateSubcategory] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.UpdateProduct] ? "✅" : "❌" )} " +
        $"| {( dict[TestType.GetCategory] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.GetProduct] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.CategoryParent] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.ProductParent] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.Subcategories] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.FindAll] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.CreateSubcategoryException] ? "✅" : "❌" )} " +
        $"| {( dict[TestType.GetCategoryException] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.GetProductException] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.UpdateProductException] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.CombinedTest1] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.CombinedTest2] ? "✅" : "❌" )} " + 
        $"| {( dict[TestType.CombinedTest3] ? "✅" : "❌" )} |" 
    });

    File.Delete($".output/{name}.trx");
}

foreach (var file in Directory.GetFiles("solutions/base", "*.cs"))
{
    File.Copy(file, Path.Combine("tester", Path.GetFileName(file)), true);
}

Directory.GetFiles(".output", "*.trx").ToList().ForEach(File.Delete);
