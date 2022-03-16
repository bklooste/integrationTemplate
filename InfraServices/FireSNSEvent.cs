using Newtonsoft.Json.Linq;

namespace IntegrationDemo.InfraServices
{
    internal class FireSNSEvent
    {
        //TODO  Convert  to json body , use the route for the type!
        internal Task Fire(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        internal Task Fire(JObject msg, string type)
        {
            throw new NotImplementedException();
        }
    }
}
