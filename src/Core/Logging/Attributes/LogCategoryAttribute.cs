using System;

namespace Brighid.Discord
{
    /// <summary>
    /// Attribute to specify what log category should be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class LogCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogCategoryAttribute" /> class.
        /// </summary>
        /// <param name="categoryName">The name of the log category to use.</param>
        public LogCategoryAttribute(string categoryName)
        {
            CategoryName = categoryName;
        }

        /// <summary>
        /// Gets or sets the log category to use when logging.
        /// </summary>
        public string CategoryName { get; set; }
    }
}
