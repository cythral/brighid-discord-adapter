using System;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Annotates an API Endpoint Enum with a category.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class ApiCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCategoryAttribute" /> class.
        /// </summary>
        /// <param name="categoryId">The ID of the API Category to use.</param>
        public ApiCategoryAttribute(char categoryId)
        {
            CategoryId = categoryId;
        }

        /// <summary>
        /// Gets the ID of the API Category.
        /// </summary>
        public char CategoryId { get; }
    }
}
