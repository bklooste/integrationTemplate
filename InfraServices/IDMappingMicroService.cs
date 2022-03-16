// See https://aka.ms/new-console-template for more information


//Responsability: store mappings so every service does not need a DB / can be stateless
using Newtonsoft.Json.Linq;

//Upsert
internal class IDMappingMicroService


{
    public IDMappingMicroService()
    {
    }

    internal JObject GetOrUpdateIDs(string integrationID, string key, params (string subKey, string values)[] pairs)
    {
        // service stores and retrieves values via upsert
        //Just a bag 
        return new JObject();
    }

    internal JObject GetOrUpdateID(string integrationID, string key, string subKey, string subValue)
    {
        var o = new JObject { { subKey, subValue } };

        // service stores and retrieves values via upsert.
        return GetOrUpdateIDs(integrationID,key, (subKey, subValue));
    }

}