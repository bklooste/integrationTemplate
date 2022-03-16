namespace IntegrationDemo.InfraServices
{

    // Aready doing this 
    //TODO put in web host note it swallows all 
    public class HttpToEvent : DelegatingHandler
    {
        readonly FireSNSEvent fireSNSEvent = new FireSNSEvent();
        

        
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.ToString().Contains("integration-api"))
            {
                //TODO add offset uri to meta data as type
                await fireSNSEvent.Fire(request);
            }
            else
            {
                return await base.SendAsync(request, cancellationToken);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}
