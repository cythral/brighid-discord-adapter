namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Adapter context.
    /// </summary>
    public interface IAdapterContext
    {
        /// <summary>
        /// Retrieves an item from the context.
        /// </summary>
        /// <typeparam name="TType">The type to retrieve from the adapter context.</typeparam>
        /// <returns>The resulting adapter context item.</returns>
        TType Get<TType>();

        /// <summary>
        /// Sets an item in the context.
        /// </summary>
        /// <param name="value">The context item.</param>
        /// <typeparam name="TType">The type of context item to set.</typeparam>
        void Set<TType>(TType value);
    }
}
