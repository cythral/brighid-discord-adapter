namespace Brighid.Discord.CacheExpirer
{
    public class CacheExpirationRequest
    {
        public CacheExpirationType Type { get; set; }

        public string Id { get; set; } = string.Empty;
    }
}
