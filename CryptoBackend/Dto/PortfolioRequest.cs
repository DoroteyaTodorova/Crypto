namespace CryptoBackend.Dto
{
    public class PortfolioRequest
    {
        public List<PortfolioEntry> Portfolio { get; set; } = new();
        public bool IncludeSentiment { get; set; }
    }
}
