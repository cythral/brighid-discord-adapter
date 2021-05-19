using Amazon.Lambda.SNSEvents;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    /// <summary>
    /// Mapper that maps SNS Records to other models.
    /// </summary>
    public interface ISnsRecordMapper
    {
        Request MapToRequest(SNSEvent.SNSRecord record);
    }
}
