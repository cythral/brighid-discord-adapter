using System.Collections.Generic;

#pragma warning disable IDE0060

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Represents major parameters in a Discord REST API Endpoint.
    /// </summary>
    public readonly struct MajorParameters
    {
        private static readonly HashSet<string> ValidParameters = new() { "channel.id", "guild.id", "webhook.id", "webhook.token" };

        /// <summary>
        /// Initializes a new instance of the <see cref="MajorParameters" /> struct.
        /// </summary>
        /// <param name="value">The string value of the major parameters.</param>
        public MajorParameters(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MajorParameters" /> struct.
        /// </summary>
        /// <param name="parameters">The parameters to look through for major parameters.</param>
        public MajorParameters(Dictionary<string, string> parameters)
        {
            Value = string.Empty;

            foreach (var (key, value) in parameters)
            {
                if (ValidParameters.Contains(key))
                {
                    Value += $"/{value}";
                }
            }
        }

        /// <summary>
        /// Gets the value of the major parameters.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Casts the major parameters to a string.
        /// </summary>
        /// <param name="parameters">The parameters to cast.</param>
        public static implicit operator string(MajorParameters parameters)
        {
            return parameters.Value;
        }

        /// <summary>
        /// Casts a dictionary of parameters to MajorParameters.
        /// </summary>
        /// <param name="parameters">The parameters to cast.</param>
        public static implicit operator MajorParameters(Dictionary<string, string> parameters)
        {
            return new MajorParameters(parameters);
        }

        /// <summary>
        /// Does an equality check against two major parameters.
        /// </summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        public static bool operator ==(MajorParameters a, MajorParameters b)
        {
            return a.Value == b.Value;
        }

        /// <summary>
        /// Does an inequality check against two major parameters.
        /// </summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        public static bool operator !=(MajorParameters a, MajorParameters b)
        {
            return a.Value != b.Value;
        }

        /// <summary>
        /// Does an equality check against a major parameter and a string.
        /// </summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        public static bool operator ==(MajorParameters a, string b)
        {
            return a.Value == b;
        }

        /// <summary>
        /// Does an inequality check against a major parameter and a string.
        /// </summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        public static bool operator !=(MajorParameters a, string b)
        {
            return a.Value != b;
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns>The resulting hash code.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Determines whether this object is equal to <paramref name="obj" />.
        /// </summary>
        /// <param name="obj">The object to check equality against.</param>
        /// <returns>True if this object is equal to <paramref name="obj" /> or false if not.</returns>
        public override bool Equals(object? obj)
        {
            return (obj is MajorParameters parameters && Value == parameters.Value) ||
                    (obj is string stringObj && Value == stringObj);
        }
    }
}
