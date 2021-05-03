using System;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <inheritdoc />
    public partial class GeneratedEventRouter : IEventRouter
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedEventRouter" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for resolving controllers.</param>
        public GeneratedEventRouter(
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }
    }
}
