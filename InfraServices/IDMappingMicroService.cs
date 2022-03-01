// See https://aka.ms/new-console-template for more information


//Responsability: store mappings so every service does not need a DB / can be stateless
using Newtonsoft.Json.Linq;

internal class IDMappingMicroService


{
    public IDMappingMicroService()
    {
    }

    internal IEnumerable<(string key, string value)> GetOrUpdateIDs(string integrationID, params (string key, string value)[] pairs)
    {
        // service stores and retrieves values

        return pairs;
    }
}