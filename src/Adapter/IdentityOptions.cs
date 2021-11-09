using System;
using System.Diagnostics.CodeAnalysis;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// Options for the identity server.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class IdentityOptions
    {
        /// <summary>
        /// Gets or sets the Identity Server URI.
        /// </summary>
        public Uri IdentityServerUri { get; set; } = new Uri("https://identity.brigh.id");
    }
}
