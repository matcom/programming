
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MatCom.Tester;

namespace MatCom.Utils;

public static class TestResult
{
    public static Dictionary<TestType, bool> GetResults(string path)
    {
        var doc = new XmlDocument();
        doc.Load(path);

        string json = JsonConvert.SerializeXmlNode(doc);

        JObject Json = JObject.Parse(json);

        var list = Json["TestRun"]!["Results"]!["UnitTestResult"]!;
        
        var dict = new Dictionary<TestType,bool>();

        foreach (var item in list)
        {
            var testName = item["@testName"]!.ToString()
                .Split(new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Split(new char[]{'('}, StringSplitOptions.RemoveEmptyEntries)
                .First();
                
            var outcome = item["@outcome"]!.ToString();

            var test = (TestType)Enum.Parse(typeof(TestType), testName);
            
            if (outcome == "Passed")
                dict[test] = true;
            else
                dict[test] = false;
        }

        return dict;
    }    

    public static bool IsApproved(Dictionary<TestType, bool> dict)
    {
        return dict[TestType.RootCase] && dict[TestType.CreateSubcategory] &&
            dict[TestType.UpdateProduct] && dict[TestType.Subcategories] &&
            dict[TestType.CategoryParent] && dict[TestType.ProductParent] &&
            dict[TestType.GetCategory] && dict[TestType.GetProduct] &&
            dict[TestType.FindAll];
    }
}
