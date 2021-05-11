using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS1591, SA1600, SA1313
namespace Brighid.Discord.Adapter.Events
{
    public class IMessageEventConverterFactory : JsonConverterFactory
    {
        private static readonly Dictionary<Type, JsonConverter> Cache = new();

        public override bool CanConvert(Type type)
        {
            return type.IsAssignableTo(typeof(IMessageEvent));
        }

        public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions _)
        {
            if (Cache.TryGetValue(type, out var cachedConverter))
            {
                return cachedConverter;
            }

            var converterType = typeof(IMessageEventConverter<>).MakeGenericType(new[] { type });
            var converter = (JsonConverter?)Activator.CreateInstance(converterType);

            if (converter != null)
            {
                Cache.Add(type, converter);
            }

            return converter;
        }
    }
}
