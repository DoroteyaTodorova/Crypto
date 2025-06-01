using CryptoBackend.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class SentimentController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SentimentController> _logger;
    public SentimentController(IConfiguration config, ILogger<SentimentController> logger)
    {
        _config = config;
        _httpClient = new HttpClient();
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] SentimentRequest request)
    {
        _logger.LogInformation("Sentiment request: {Text}", request.Text);

        try
        {
            var apiUrl = _config["HuggingFace:Url"];
            var apiKey = _config["HuggingFace:ApiKey"];

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var content = new StringContent(JsonSerializer.Serialize(new { inputs = request.Text }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("External API error: {Message}", ex.Message);
            _logger.LogError(ex, "Exception occurred during sentiment analysis");
            return StatusCode(500, "Failed to analyze sentiment.");
        }
    }
}
