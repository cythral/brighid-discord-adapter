using System;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.DependencyInjection
{
    /// <inheritdoc />
    public class DefaultScopeFactory : IScopeFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultScopeFactory" /> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider to create scopes with.</param>
        public DefaultScopeFactory(
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IScope CreateScope()
        {
            var innerScope = serviceProvider.CreateScope();
            return new Scope(innerScope, innerScope.ServiceProvider);
        }
    }
}
