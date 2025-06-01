using System.Text.Json;
using System.Text;
using CryptoBackend.Interface;

namespace CryptoBackend.Service
{
    public class SentimentService : ISentimentService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<SentimentService> _logger;

        public SentimentService(
            IHttpClientFactory clientFactory,
            IConfiguration config,
            ILogger<SentimentService> logger)
        {
            _clientFactory = clientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<string> AnalyzeSentimentAsync(string coinSymbol)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var newsUrl = $"{_config["AppBaseUrl"]}/api/News/{coinSymbol}";
                _logger.LogDebug("Requesting news for {Coin} from {Url}", coinSymbol, newsUrl);

                var newsRes = await client.GetAsync(newsUrl);

                if (!newsRes.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch news for {Coin}: Status {StatusCode}", coinSymbol, newsRes.StatusCode);
                    return "N/A";
                }

                var json = await newsRes.Content.ReadAsStringAsync();
                using var newsDoc = JsonDocument.Parse(json);

                if (!newsDoc.RootElement.TryGetProperty("results", out var resultsElement) || resultsElement.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Unexpected JSON structure for news results of {Coin}.", coinSymbol);
                    return "N/A";
                }

                var titles = resultsElement
                    .EnumerateArray()
                    .Select(n => n.TryGetProperty("title", out var t) ? t.GetString() : null)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                if (!titles.Any())
                {
                    _logger.LogInformation("No news titles found for {Coin}.", coinSymbol);
                    return "N/A";
                }

                var combinedText = string.Join(". ", titles);
                var payload = JsonSerializer.Serialize(new { text = combinedText });

                var sentimentUrl = $"{_config["AppBaseUrl"]}/api/Sentiment";
                _logger.LogDebug("Sending sentiment analysis request for {Coin} to {Url}", coinSymbol, sentimentUrl);

                var response = await client.PostAsync(
                    sentimentUrl,
                    new StringContent(payload, Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Sentiment API returned status {StatusCode} for {Coin}", response.StatusCode, coinSymbol);
                    return "N/A";
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                using var parsed = JsonDocument.Parse(resultJson);
                var root = parsed.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var sentimentElement = root[0].ValueKind == JsonValueKind.Array && root[0].GetArrayLength() > 0
                        ? root[0][0]
                        : root[0];

                    if (sentimentElement.TryGetProperty("label", out var labelProp) &&
                        sentimentElement.TryGetProperty("score", out var scoreProp))
                    {
                        var label = labelProp.GetString()?.ToLower();
                        var score = scoreProp.GetDouble();

                        if (string.IsNullOrWhiteSpace(label))
                        {
                            _logger.LogWarning("Sentiment label missing for {Coin}.", coinSymbol);
                            return "N/A";
                        }

                        return (label == "positive" || label == "negative") && score >= 0.4 && score <= 0.6
                            ? "Neutral"
                            : char.ToUpper(label[0]) + label[1..];
                    }

                    _logger.LogWarning("Sentiment data missing 'label' or 'score' for {Coin}.", coinSymbol);
                    return "N/A";
                }

                _logger.LogWarning("Unexpected sentiment JSON structure for {Coin}.", coinSymbol);
                return "N/A";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sentiment analysis failed for {Coin}.", coinSymbol);
                return "N/A";
            }
        }
    }
}
