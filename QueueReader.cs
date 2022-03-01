using Newtonsoft.Json.Linq;

internal class QueueReader
{
    private readonly string connection;

    public QueueReader(string connectionString)
    {
        this.connection = connectionString;
    }

    internal Task<byte[]> Read()
    {
        throw new NotImplementedException();
    }
}

public static class MsgHelper
{
    public static (JObject jsonBody, JObject msgMetaData) AsJson(this byte[] msg)
    {
        {
            return (new JObject(), new JObject());
        }
    }
}