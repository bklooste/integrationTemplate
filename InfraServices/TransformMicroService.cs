// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json.Linq;

internal class TransformMicroService
{
    public TransformMicroService()
    {
    }

    // represents http call
    internal string GetTransformedBody(string templateName, JObject jsonBody, JObject metaData)
    {
        var result = new JObject();
        var fields = jsonBody.Values().Concat(metaData.Values()).ToDictionary(x=> x.Path);
        foreach (var (fromPath, toPath, lookup) in GetTransform(templateName))
        {
            var value = fields[fromPath].ToString();

            // apply lookup in template
            if (lookup != null)
            {
                value = lookup[value];
            }

            result.Add(toPath, value);  
        }

        return result.ToString();
    }

    // many ways to template /  transform
    internal (string from, string to, IDictionary<string, string>? lookup)[] GetTransform(string templateName)
    {
        // in data / config 

        return new (string from, string to, IDictionary<string, string>? lookup)[]
        {
            ( "PolicyId" , "id", null),
            ( "Status" , "state",  new Dictionary<string, string>  {{ "1", "Open" }, { "2", "Paid" } }  ),    
        };
    }
}