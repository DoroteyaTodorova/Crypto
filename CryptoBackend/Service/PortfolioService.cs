using CryptoBackend.Dto;
using CryptoBackend.Interface;

namespace CryptoBackend.Service
{
    public class PortfolioService : IPortfolioService
    {
        private readonly ICoinLoreService _coinLoreService;
        private readonly ISentimentService _sentimentService;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(
            ICoinLoreService coinLoreService,
            ISentimentService sentimentService,
            ILogger<PortfolioService> logger)
        {
            _coinLoreService = coinLoreService;
            _sentimentService = sentimentService;
            _logger = logger;
        }

        public async Task<List<PortfolioResult>> CalculatePortfolioAsync(PortfolioRequest request)
        {
            var results = new List<PortfolioResult>();

            try
            {
                _logger.LogInformation("Fetching current crypto prices.");
                var prices = await _coinLoreService.FetchCurrentPrices();

                var priceDict = prices.ToDictionary(
                    p => p.Symbol.Trim().ToUpper(),
                    p => p.PriceUsd,
                    StringComparer.OrdinalIgnoreCase);

                _logger.LogInformation("Processing {Count} portfolio items.", request.Portfolio.Count);

                foreach (var item in request.Portfolio)
                {
                    var symbol = item.Coin?.Trim().ToUpper();

                    if (string.IsNullOrWhiteSpace(symbol))
                    {
                        _logger.LogWarning("Skipped item with missing or empty coin symbol.");
                        continue;
                    }

                    if (!priceDict.TryGetValue(symbol, out var currentPrice))
                    {
                        _logger.LogWarning("Coin symbol '{Symbol}' not found in price data.", symbol);
                        continue;
                    }

                    double? change = item.BuyPrice > 0
                        ? ((currentPrice - item.BuyPrice) / item.BuyPrice) * 100
                        : null;

                    string sentiment = "N/A";

                    if (request.IncludeSentiment)
                    {
                        try
                        {
                            _logger.LogDebug("Fetching sentiment for {Symbol}.", symbol);
                            sentiment = await _sentimentService.AnalyzeSentimentAsync(symbol);
                        }
                        catch (Exception sentimentEx)
                        {
                            _logger.LogError(sentimentEx, "Sentiment analysis failed for {Symbol}.", symbol);
                            sentiment = "Error";
                        }
                    }

                    results.Add(new PortfolioResult
                    {
                        Coin = symbol,
                        Amount = item.Amount,
                        BuyPrice = item.BuyPrice,
                        CurrentPrice = currentPrice,
                        ChangePercent = change,
                        Sentiment = sentiment
                    });
                }

                _logger.LogInformation("Portfolio calculation completed. {Count} results returned.", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during portfolio calculation.");
                throw;
            }
        }
    }
}
