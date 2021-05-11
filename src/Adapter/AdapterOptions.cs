using Destructurama.Attributed;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// Miscellaneous config used across adapter services.
    /// </summary>
    public class AdapterOptions
    {
        /// <summary>
        /// Gets or sets the auth scheme to use when making requests to the REST API.
        /// </summary>
        public string AuthScheme { get; set; } = "Bot";

        /// <summary>
        /// Gets or sets the token to authenticate against the gateway and REST API with.
        /// </summary>
        [NotLogged]
        public string Token { get; set; } = string.Empty;
    }
}
