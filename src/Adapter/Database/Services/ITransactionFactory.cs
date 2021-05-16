namespace Brighid.Discord.Adapter.Database
{
    /// <summary>
    /// A factory for creating transactions with.
    /// </summary>
    public interface ITransactionFactory
    {
        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <returns>The resulting transaction.</returns>
        ITransaction CreateTransaction();
    }
}
