using Brighid.Discord.Models;

using Lambdajection.Sns;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    /// <summary>
    /// Mapper that maps SNS Records to other models.
    /// </summary>
    public interface ISnsRecordMapper
    {
        Request MapToRequest(SnsMessage<string> record);
    }
}
