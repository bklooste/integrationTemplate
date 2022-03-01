// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json.Linq;
#pragma warning disable CS8602 // Dereference of a possibly null reference.


internal class IntegrationMicroService
{
    const string integrationID = "SampleIntegration";
    const string destination = "https://someInsurer/";

    IDMappingMicroService idMappingService = new IDMappingMicroService();
    TransfromAndCallMicroService transformAndCallService = new TransfromAndCallMicroService();


    public IntegrationMicroService()
    {
    }

    // Can do this in batches if we want high performance 
    internal void Process((JObject jsonBody, JObject metaData) policyUpdated)
    {
        // our provider uses an identity for policy we need to map this . 

        var ids = idMappingService.GetOrUpdateIDs(integrationID, ("policyId", policyUpdated.jsonBody["policyId"].ToString()), ("customerId", policyUpdated.jsonBody["customerID"].ToString()));

        foreach (var id in ids)
            policyUpdated.metaData.Add(new JProperty(id.key, id.value));

        EnrichWithCustom(policyUpdated.jsonBody, policyUpdated.metaData);

        //  sequential 1 message can make multiple calls (async)
        // NOTE destination does not have to be the customer it can be a http to WSL service , XML , one with custom security etc 
        var responses1 = transformAndCallService.Fire(policyUpdated.jsonBody, policyUpdated.metaData, destination, GetSecurityToken(), templateName: "policyToSampleIntegratorPolicy");
        var responses2 = transformAndCallService.Fire(policyUpdated.jsonBody, policyUpdated.metaData, destination + "/Financial", GetSecurityToken(), templateName: "policyToSampleIntegratorPolicyFinancial");
        var responses3 = transformAndCallService.Fire(policyUpdated.jsonBody, policyUpdated.metaData, destination+"/Customer", GetSecurityToken(), templateName: "policyToSampleIntegratorCustomer");

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