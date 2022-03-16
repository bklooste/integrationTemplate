using Microsoft.Extensions.Configuration;

using Newtonsoft.Json.Linq;
#pragma warning disable CS8602 // Dereference of a possibly null reference.


internal class ConfigIntegrationMicroService
{
    const string integrationID = "SampleConfigIntegration";
    const string destination = "https://someSimpleInsurer/";

    IDMappingMicroService idMappingService = new IDMappingMicroService();
    TransfromAndCallMicroService transformAndCallService = new TransfromAndCallMicroService();

    IConfigurationRoot config;

    public ConfigIntegrationMicroService()
    {
        // store configs, db etc 
        config = new ConfigurationBuilder()
                //.AddJsonFile()
                .Build();
    }

    // Can do this in batches if we want high performance 
    internal void Process(JObject jsonBody, JObject metaData, string msgType)
    {
        // important config is sequential 
        foreach ( var config in config.GetChildren())
            ProcessConfig(jsonBody, metaData, config["templateName"], config["security"]);
    }

    private void ProcessConfig(JObject jsonBody, JObject metaData, string templateName, string security)
    {
        // our provider uses an identity for policy we need to map this . 
        //TODO config should have id maps  config["Configs"]["IdMaps"]
        var ids = idMappingService.GetOrUpdateIDs(integrationID, metaData["CorrelationId"].ToString(), ("policyId", jsonBody["policyId"].ToString()), ("customerId", jsonBody["customerID"].ToString()));

        foreach (var id in ids)
            metaData.Add(new JProperty(id.Key, id.Value));


        var responses1 = transformAndCallService.Fire(jsonBody, metaData, destination, GetSecurityToken(security), templateName);

        //TODO retry or give up on responses. If we give up insurer will not get the data until we publish the PolicyUpdatedEvent again. We can sit here for a long time though waiting for infra issues to be resolved since were not in a handler. 
    }

    private string GetSecurityToken(string security)
    {
        return String.Empty;
    }

    // custom logic 


}