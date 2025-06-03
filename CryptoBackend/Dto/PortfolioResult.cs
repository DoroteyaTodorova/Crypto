namespace CryptoBackend.Dto
{
    public class PortfolioResult : PortfolioEntry
    {
        public double? CurrentPrice { get; set; }
        public double? ChangePercent { get; set; }
        public string Sentiment { get; set; }
    }
}
