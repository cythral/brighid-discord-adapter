using System;

namespace Brighid.Discord
{
    /// <summary>
    /// Options for the identity server.
    /// </summary>
    public class IdentityOptions
    {
        /// <summary>
        /// Gets or sets the Identity Server URI.
        /// </summary>
        public Uri IdentityServerUri { get; set; } = new Uri("https://identity.brigh.id");
    }
}
