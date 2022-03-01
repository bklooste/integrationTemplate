// See https://aka.ms/new-console-template for more information
Console.WriteLine("Processing!");

var queueReader = new QueueReader("aConnectionString");

var customIntegrationService = new IntegrationMicroService();
var configIntegrationService = new ConfigIntegrationMicroService();

//Ordered processing
while (true)
{
    try
    {
        var aEvent = await queueReader.Read();
        var (jsonBody, metaData) = aEvent.AsJson();

        // all services receive event
        customIntegrationService.Process((jsonBody, metaData));
        configIntegrationService.Process((jsonBody, metaData));



        //if (stoppingCts.IsCancellationRequested)
        //    break;
    }
    catch (ThreadAbortException ex)
    {
        // we die until this integration back end issue is resolved in which case we will process all the work
    }

    catch (Exception ex)
    {
        // log and ignore , handlers are best effort and responsable for retry
    }

}