// See https://aka.ms/new-console-template for more information
using IntegrationDemo.InfraServices;

using Newtonsoft.Json.Linq;
#pragma warning disable CS8602 // Dereference of a possibly null reference.


internal class CGUIntegrationMicroService
{
    const string integrationID = "SampleIntegration";
    const string destination = "https://someInsurer/";
    readonly IDMappingMicroService idMappingService = new();
    readonly TransfromAndCallMicroService transformAndCallService = new();
    private TransformMicroService transformService = new();
    readonly FireSNSEvent fireSNSEvent = new();


    public CGUIntegrationMicroService()
    {
    }

    internal void Process(JObject jsonBody, JObject metaData, string msgType)
    {
        switch (msgType)
        {
            case "QuoteRequest":
                QuoteRequest(jsonBody, metaData);
                break;
            case "QuoteResponse":
                QuoteResponse(jsonBody, metaData);
                break;
            case "BindRequest":
                BindRequest(jsonBody, metaData);
                break;
            case "BindResponse":
                BindResponse(jsonBody, metaData);
                break;
            default:
                break;
        }
    }

    private void QuoteResponse(JObject jsonBody, JObject metaData)
    {
        var storedData = idMappingService.GetOrUpdateID(integrationID, "CorelationId" + metaData["CorrelationId"], "ExternalQuoteId",  jsonBody["ExternalQuoteId"].ToString());
        jsonBody["QuoteId"] = storedData["QuoteID"].ToString();
        var json = transformService.GetTransformedBody("quoteReesponseToMsg", jsonBody, metaData);

        fireSNSEvent.Fire( json, "QuoteUpdated");
    }

    private void QuoteRequest(JObject jsonBody, JObject metaData)
    {
        var ids = idMappingService.GetOrUpdateID(integrationID, "CorelationId" + metaData["CorrelationId"], "QuoteId", jsonBody["QuoteId"].ToString());

        foreach (var id in ids)
            metaData.Add(new JProperty(id.Key, id.Value));

        EnrichWithCustom(jsonBody, metaData);

        //  sequential 1 message can make multiple calls (async)
        // NOTE destination does not have to be the customer it can be a http to WSL service , XML , one with custom security etc 
        var responses1 = transformAndCallService.Fire(jsonBody, metaData, destination, GetSecurityToken(), templateName: "quoteToCGUQuote");

        // sync api we can deal with here.
        //idMappingService.GetOrUpdateIDs(integrationID, ("policyDetailId", jsonBody["policyId"].ToString()), ("customerId", jsonBody["customerID"].ToString()));
        //TODO retry or give up on responses. If we give up insurer will not get the data until we publish the PolicyUpdatedEvent again. We can sit here for a long time though waiting for infra issues to be resolved since were not in a handler. 
    }

    private void BindRequest(JObject jsonBody, JObject metaData)
    {
        var ids = idMappingService.GetOrUpdateID(integrationID, "CorelationId" + metaData["CorrelationId"], "QuoteId", jsonBody["QuoteId"].ToString());

        foreach (var id in ids)
            metaData.Add(new JProperty(id.Key, id.Value));

        //  sequential 1 message can make multiple calls (async)
        // NOTE destination does not have to be the customer it can be a http to WSL service , XML , one with custom security etc 
        var responses1 = transformAndCallService.Fire(jsonBody, metaData, destination, GetSecurityToken(), templateName: "quoteToCGUQuote");
    }

    private void BindResponse(JObject jsonBody, JObject metaData)
    {
        var storedData = idMappingService.GetOrUpdateID(integrationID, "CorelationId" + metaData["CorrelationId"], "ExternalPolicyId", jsonBody["ExternalPolicyId"].ToString());
        jsonBody["QuoteId"] = storedData["QuoteID"].ToString();
        jsonBody["PolicyId"] = storedData["PolicyId"].ToString();
        var json = transformService.GetTransformedBody("policyBoundToMsg", jsonBody, metaData);

        fireSNSEvent.Fire(json, "ExternalPolicyBound");
        fireSNSEvent.Fire(transformService.GetTransformedBody("example of 1:2", jsonBody, metaData), "CustomerAddedPolicy");


    }


    private void EnrichWithCustom(JObject jsonBody, JObject metaData) => metaData.Add(new JProperty("occupationMap", GetInsurerOccupationCode(jsonBody["Occupation"].ToString())));

    private object[] GetInsurerOccupationCode(string occupation)
    {
        throw new NotImplementedException();
    }

    private string GetSecurityToken()
    {
        return String.Empty;
    }

    // custom logic 


}