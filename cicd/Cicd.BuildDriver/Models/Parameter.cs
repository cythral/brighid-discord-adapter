namespace Brighid.Discord.Cicd.Utils
{
    /// <summary>
    /// Represents a parameter passed to a CloudFormation template.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Gets or sets the dev value for the parameter.
        /// </summary>
        public string Dev { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the prod value for the parameter.
        /// </summary>
        public string Prod { get; set; } = string.Empty;
    }
}
