using Microsoft.Extensions.Hosting;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Background worker to take requests of the queue and invoke them.
    /// </summary>
    public interface IRequestWorker : IHostedService
    {
    }
}
