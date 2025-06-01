using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsController> _logger;

    public NewsController(IConfiguration config, ILogger<NewsController> logger)
    {
        _config = config;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetNews(string symbol)
    {
        _logger.LogInformation("Fetching news for symbol: {Symbol}", symbol);

        try
        {
            var apiUrl = _config["CryptoPanic:Url"];
            var apiKey = _config["CryptoPanic:ApiKey"];
            var url = $"{apiUrl}?auth_token={apiKey}&currencies={symbol.ToLower()}&public=true";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Failed to fetch news for {Symbol}: {Message}", symbol, ex.Message);
            _logger.LogError(ex, "Exception occurred in GetNews");
            return StatusCode(500, "Failed to fetch news.");
        }
    }
}
