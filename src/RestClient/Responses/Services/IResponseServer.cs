using Microsoft.Extensions.Hosting;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// Server that listens for responses to REST API requests from the queue worker.
    /// </summary>
    public interface IResponseServer : IHostedService
    {
        /// <summary>
        /// Gets or sets a value indicating whether the response server is running or not.
        /// </summary>
        bool IsRunning { get; set; }
    }
}
