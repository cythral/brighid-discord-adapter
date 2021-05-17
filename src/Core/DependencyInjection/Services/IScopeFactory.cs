namespace Brighid.Discord.DependencyInjection
{
    /// <summary>
    /// Service factory that can create scopes.
    /// </summary>
    public interface IScopeFactory
    {
        /// <summary>
        /// Create a new scope.
        /// </summary>
        /// <returns>The resulting scope.</returns>
        IScope CreateScope();
    }
}
