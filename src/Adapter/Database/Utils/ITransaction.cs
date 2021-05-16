using System;

namespace Brighid.Discord.Adapter.Database
{
    /// <summary>
    /// Service for transacting buckets.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Marks the transaction as complete.
        /// </summary>
        void Complete();
    }
}
