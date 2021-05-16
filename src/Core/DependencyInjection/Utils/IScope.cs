using System;

namespace Brighid.Discord.DependencyInjection
{
    /// <summary>
    /// Service provider providing access to scoped services.
    /// </summary>
    public interface IScope : IDisposable
    {
        /// <summary>
        /// Get a service from the scope.
        /// </summary>
        /// <typeparam name="TService">The type of service to retrieve.</typeparam>
        /// <returns>The resulting service.</returns>
        TService GetService<TService>()
            where TService : notnull;
    }
}
