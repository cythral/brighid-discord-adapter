using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Serialization
{
    /// <summary>
    /// Injects a service provider into the converters collection.
    /// </summary>
    public class ServiceProviderInjectionConverter : JsonConverter<object>, IServiceProvider
    {
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderInjectionConverter" /> class.
        /// </summary>
        /// <param name="provider">The service provider to inject.</param>
        public ServiceProviderInjectionConverter(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return false;
        }

        /// <inheritdoc />
        public object? GetService(Type serviceType)
        {
            return provider.GetService(serviceType);
        }

        /// <inheritdoc />
        public override object Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(
            Utf8JsonWriter writer,
            object value,
            JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
