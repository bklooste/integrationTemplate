// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json.Linq;

using System.Net;
using System.Text;

internal class TransfromAndCallMicroService
{
    private TransformMicroService transformService;
    private HttpClient httpClient;

    public TransfromAndCallMicroService()
    {
        transformService = new TransformMicroService();
        httpClient = new HttpClient();
    }

    internal HttpStatusCode Fire(JObject jsonBody, JObject metaData, string destination, string security, string templateName)
    {
        var body = transformService.GetTransformedBody(templateName, jsonBody, metaData);

        //TOOD if security contains : use basic authentication eg new httpclient

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.clickatell.com/rest/message"),
            Headers = {
                { HttpRequestHeader.Authorization.ToString(), security },
            },
            Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json")
        };

        var response = httpClient.SendAsync(httpRequestMessage).Result;

        return response.StatusCode;
    }
}