using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Responses
{
    /// <inheritdoc />
    public class DefaultRequestMap : ConcurrentDictionary<Guid, TaskCompletionSource<Response>>, IRequestMap
    {
    }
}
