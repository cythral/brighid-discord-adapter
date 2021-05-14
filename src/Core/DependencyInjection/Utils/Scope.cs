using System;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.DependencyInjection
{
    /// <inheritdoc />
    public class Scope : IScope
    {
        private readonly IServiceScope scope;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scope" /> class.
        /// </summary>
        /// <param name="scope">The inner scope object to proxy.</param>
        /// <param name="serviceProvider">Provider to pull services from.</param>
        public Scope(IServiceScope scope, IServiceProvider serviceProvider)
        {
            this.scope = scope;
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public TService GetService<TService>()
            where TService : notnull
        {
            return serviceProvider.GetRequiredService<TService>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            scope.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
