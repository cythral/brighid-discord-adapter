using System;
using System.Transactions;

namespace Brighid.Discord.Adapter.Database
{
    /// <inheritdoc />
    public class DefaultTransaction : ITransaction
    {
        private readonly TransactionScope transactionScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTransaction" /> class.
        /// </summary>
        public DefaultTransaction()
        {
            transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled);
        }

        /// <inheritdoc />
        public void Complete()
        {
            transactionScope.Complete();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            transactionScope.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
