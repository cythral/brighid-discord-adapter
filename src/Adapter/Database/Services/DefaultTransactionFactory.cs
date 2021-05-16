namespace Brighid.Discord.Adapter.Database
{
    /// <inheritdoc />
    public class DefaultTransactionFactory : ITransactionFactory
    {
        /// <inheritdoc />
        public ITransaction CreateTransaction()
        {
            return new DefaultTransaction();
        }
    }
}
