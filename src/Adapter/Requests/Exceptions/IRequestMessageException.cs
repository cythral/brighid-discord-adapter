namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// An exception dealing with a Request Message.
    /// </summary>
    public interface IRequestMessageException
    {
        /// <summary>
        /// Gets the request message associated with this exception.
        /// </summary>
        RequestMessage RequestMessage { get; init; }
    }
}
