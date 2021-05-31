namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// Represents the status of an account link operation.
    /// </summary>
    public enum AccountLinkStatus
    {
        /// <summary>
        /// Status when linking an account succeeds.
        /// </summary>
        Success,

        /// <summary>
        /// Status when linking an account fails for an unknown reason.
        /// </summary>
        Failed,

        /// <summary>
        /// Status when linking an account fails because it's already linked.
        /// </summary>
        AlreadyLinked,
    }
}
