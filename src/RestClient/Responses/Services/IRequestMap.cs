using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// An in-memory representation dictionary that maps request IDs to their promises.
    /// </summary>
    public interface IRequestMap : IDictionary<Guid, TaskCompletionSource<Response>>
    {
    }
}
