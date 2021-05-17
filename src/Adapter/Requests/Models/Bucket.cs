using System;
using System.ComponentModel.DataAnnotations;

using Brighid.Discord.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Represents a Rate Limit Bucket.
    /// </summary>
    public class Bucket
    {
        /// <summary>
        /// Gets or sets the API category associated with the endpoints in this bucket.
        /// </summary>
        public char ApiCategory { get; set; }

        /// <summary>
        /// Gets or sets the endpoints associated with this bucket as a bitfield.
        /// </summary>
        public ulong Endpoints { get; set; } = 0;

        /// <summary>
        /// Gets or sets the ID of this bucket that was returned from the Discord API.
        /// </summary>
        [Key]
        public string RemoteId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the major parameters associated with this bucket, separated by '/'.
        /// </summary>
        public MajorParameters MajorParameters { get; set; }

        /// <summary>
        /// Gets or sets the number of hits remaining in this bucket.
        /// </summary>
        public int HitsRemaining { get; set; }

        /// <summary>
        /// Gets or sets the date/time that the bucket hits will reset.
        /// </summary>
        public DateTimeOffset ResetAfter { get; set; }

        /// <summary>
        /// Add an endpoint to the Bucket.
        /// </summary>
        /// <param name="endpoint">The endpoint to add to the bucket.</param>
        public void AddEndpoint(Endpoint endpoint)
        {
            ValidateEndpoint(endpoint);
            Endpoints |= Convert.ToUInt64(endpoint.Value);
        }

        /// <summary>
        /// Add an endpoint to the Bucket.
        /// </summary>
        /// <param name="endpoint">The endpoint to add to the bucket.</param>
        public void RemoveEndpoint(Endpoint endpoint)
        {
            ValidateEndpoint(endpoint);
            Endpoints ^= Convert.ToUInt64(endpoint.Value);
        }

        /// <summary>
        /// Determine if the bucket has an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to check for in the bucket.</param>
        /// <returns>True if the bucket contains the endpoint, or false if not.</returns>
        public bool HasEndpoint(Endpoint endpoint)
        {
            var int64Type = Convert.ToUInt64(endpoint.Value);
            return ApiCategory == endpoint.Category && (Endpoints & int64Type) == int64Type;
        }

        private void ValidateEndpoint(Endpoint endpoint)
        {
            if (ApiCategory == default(char))
            {
                ApiCategory = endpoint.Category;
            }

            if (ApiCategory != endpoint.Category)
            {
                throw new ArgumentException($"Endpoint must have category: {ApiCategory}");
            }
        }

        /// <inheritdoc />
        public class EntityConfig : IEntityTypeConfiguration<Bucket>
        {
            /// <inheritdoc />
            public void Configure(EntityTypeBuilder<Bucket> builder)
            {
                builder
                .Property(bucket => bucket.MajorParameters)
                .HasConversion(new ValueConverter<MajorParameters, string>(
                    value => value.Value,
                    value => new MajorParameters(value)
                ));
            }
        }
    }
}
