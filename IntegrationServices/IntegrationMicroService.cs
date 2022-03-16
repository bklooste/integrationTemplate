// See https://aka.ms/new-console-template for more information
using IntegrationDemo.InfraServices;

using Newtonsoft.Json.Linq;
#pragma warning disable CS8602 // Dereference of a possibly null reference.


internal class IntegrationMicroService
{
    const string integrationID = "SampleIntegration";
    const string destination = "https://someInsurer/";

    readonly IDMappingMicroService idMappingService = new IDMappingMicroService();
    readonly TransfromAndCallMicroService transformAndCallService = new TransfromAndCallMicroService();
    private TransformMicroService transformService = new TransformMicroService();
    readonly FireSNSEvent fireSNSEvent = new FireSNSEvent();


    public IntegrationMicroService()
    {
    }

    internal void Process((JObject jsonBody, JObject metaData, string msgType) policyUpdated)
    {
        switch (policyUpdated.msgType)
        {
            case "QuoteRequest":
                QuoteRequest(policyUpdated.jsonBody, policyUpdated.metaData);
                break;
            case "QuoteResponse":
                QuoteResponse(policyUpdated.jsonBody, policyUpdated.metaData);
                break;
            case "BindRequest":
                BindRequest(policyUpdated.jsonBody, policyUpdated.metaData);
                break;
            case "BindResponse":
                QuoteRequest(policyUpdated.jsonBody, policyUpdated.metaData);
                break;
            case "PolicyUpdated":
                PolicyUpdated(policyUpdated.jsonBody, policyUpdated.metaData);
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

        fireSNSEvent.Fire( JObject.Parse(json), "QuoteUpdated");
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

        EnrichWithCustom(jsonBody, metaData);

        //  sequential 1 message can make multiple calls (async)
        // NOTE destination does not have to be the customer it can be a http to WSL service , XML , one with custom security etc 
        var responses1 = transformAndCallService.Fire(jsonBody, metaData, destination, GetSecurityToken(), templateName: "quoteToCGUQuote");
    }

    private void PolicyUpdated(JObject jsonBody, JObject metaData)
    {
        // our provider uses an identity for policy we need to map this . 

        var ids = idMappingService.GetOrUpdateIDs(integrationID, "CorelationId" + metaData["CorrelationId"], ("policyId", jsonBody["policyId"].ToString()), ("customerId", jsonBody["customerID"].ToString()));

        foreach (var id in ids)
            metaData.Add(new JProperty(id.Key, id.Value));          

        EnrichWithCustom(jsonBody, metaData);

        //  sequential 1 message can make multiple calls (async)
        // NOTE destination does not have to be the customer it can be a http to WSL service , XML , one with custom security etc 
        var responses1 = transformAndCallService.Fire(jsonBody, metaData, destination, GetSecurityToken(), templateName: "policyToSampleIntegratorPolicy");
        var responses2 = transformAndCallService.Fire(jsonBody, metaData, destination + "/Financial", GetSecurityToken(), templateName: "policyToSampleIntegratorPolicyFinancial");
        var responses3 = transformAndCallService.Fire(jsonBody, metaData, destination + "/Customer", GetSecurityToken(), templateName: "policyToSampleIntegratorCustomer");

        idMappingService.GetOrUpdateIDs(integrationID, "CorelationId" + metaData["CorrelationId"], ("policyDetailId", jsonBody["policyId"].ToString()), ("customerId", jsonBody["customerID"].ToString()));
        //TODO retry or give up on responses. If we give up insurer will not get the data until we publish the PolicyUpdatedEvent again. We can sit here for a long time though waiting for infra issues to be resolved since were not in a handler. 
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