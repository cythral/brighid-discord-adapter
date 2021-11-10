using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Models
{
    public class EndpointConverter : JsonConverter<Endpoint>
    {
        private static readonly Dictionary<char, Type> CategoryDictionary = new();

        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Will be converted to a source generator at a later date.")]
        static EndpointConverter()
        {
            foreach (var type in typeof(EndpointConverter).Assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(ApiCategoryAttribute), true);
                if (attributes.Length == 1)
                {
                    var categoryAttribute = (ApiCategoryAttribute)attributes[0];
                    CategoryDictionary.Add(categoryAttribute.CategoryId, type);
                }
            }
        }

        public override Endpoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (stringValue == null)
            {
                throw new SerializationException("Endpoint cannot be null.");
            }

            var category = stringValue[0];
            if (!CategoryDictionary.TryGetValue(category, out var enumType))
            {
                throw new SerializationException($"Invalid category {category}.");
            }

            if (!long.TryParse(stringValue[1..], out var enumValue))
            {
                throw new SerializationException($"Expected long after category, got {stringValue[1..]}");
            }

            var @enum = (Enum)Enum.ToObject(enumType, enumValue);
            return @enum == null
                ? throw new SerializationException("Failed to create new enum.")
                : new Endpoint(category, @enum);
        }

        public override void Write(Utf8JsonWriter writer, Endpoint value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.Category}{Convert.ToUInt64(value.Value)}");
        }
    }
}
