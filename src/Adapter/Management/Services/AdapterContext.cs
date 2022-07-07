using System;
using System.Collections.Generic;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class AdapterContext : IAdapterContext
    {
        private readonly Dictionary<Type, object> context = new();

        /// <inheritdoc />
        public TType Get<TType>()
        {
            return (TType)context[typeof(TType)];
        }

        /// <inheritdoc />
        public void Set<TType>(TType value)
        {
            context[typeof(TType)] = value!;
        }
    }
}
