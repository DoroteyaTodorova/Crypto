using CryptoBackend.Dto;
using CryptoBackend.Interface;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace CryptoBackend.Service
{
    public class CoinLoreService : ICoinLoreService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoinLoreService> _logger;

        public CoinLoreService(IHttpClientFactory clientFactory, ILogger<CoinLoreService> logger)
        {
            _httpClient = clientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<List<(string Symbol, double PriceUsd)>> FetchCurrentPrices()
        {
            var result = new List<(string Symbol, double PriceUsd)>();

            try
            {
                var response = await _httpClient.GetAsync("https://api.coinlore.net/api/tickers/");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var coinLoreResponse = JsonSerializer.Deserialize<CoinLoreResponse>(json);

                if (coinLoreResponse?.Data == null || !coinLoreResponse.Data.Any())
                {
                    _logger.LogWarning("No data returned from CoinLore API.");
                    return result;
                }

                foreach (var coin in coinLoreResponse.Data)
                {
                    if (string.IsNullOrWhiteSpace(coin.Symbol) || string.IsNullOrWhiteSpace(coin.PriceUsd))
                        continue;

                    if (double.TryParse(coin.PriceUsd, NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                    {
                        result.Add((coin.Symbol.ToUpper(), price));
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse price for {Symbol}: {Price}", coin.Symbol, coin.PriceUsd);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while fetching CoinLore data.");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error while processing CoinLore data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching CoinLore data.");
            }

            return result;
        }
    }
}
