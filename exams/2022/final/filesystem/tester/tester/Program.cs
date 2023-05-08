using System.Text.Json;
using MatCom.Exam;
using MatCom.Tester;

Directory.CreateDirectory(".output");
var tester = new Tester();
var testCases = tester.GenerateResponses(0, 0);
var result = tester.Test(testCases, Wrapper.CreateFileSystem, Wrapper.Nombre + " " + Wrapper.Grupo)!;
System.IO.File.Delete(Path.Combine(".output", "result.json"));
System.IO.File.WriteAllText(Path.Combine(".output", "result.json"), JsonSerializer.Serialize(result));
