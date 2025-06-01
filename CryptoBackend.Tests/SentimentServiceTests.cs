using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CryptoBackend.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CryptoBackend.Tests
{
    public class SentimentServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<SentimentService>> _loggerMock;

        public SentimentServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<SentimentService>>();
        }

        private HttpClient CreateHttpClient(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(response);

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task AnalyzeSentimentAsync_ReturnsSentiment_WhenSuccessful()
        {
            // Arrange
            var coin = "BTC";
            var newsJson = """
        {
          "results": [
            { "title": "Bitcoin surges past $30k" },
            { "title": "BTC adoption increases" }
          ]
        }
        """;

            var sentimentJson = """
        [
          {
            "label": "positive",
            "score": 0.85
          }
        ]
        """;

            var httpClient = CreateHttpClientSequence(new[]
            {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(newsJson, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sentimentJson, Encoding.UTF8, "application/json")
            }
        });

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _configMock.Setup(c => c["AppBaseUrl"]).Returns("https://fake.api");

            var service = new SentimentService(_httpClientFactoryMock.Object, _configMock.Object, _loggerMock.Object);

            // Act
            var result = await service.AnalyzeSentimentAsync(coin);

            // Assert
            Assert.Equal("Positive", result);
        }

        [Fact]
        public async Task AnalyzeSentimentAsync_ReturnsNA_WhenNewsFails()
        {
            var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _configMock.Setup(c => c["AppBaseUrl"]).Returns("https://fake.api");

            var service = new SentimentService(_httpClientFactoryMock.Object, _configMock.Object, _loggerMock.Object);

            var result = await service.AnalyzeSentimentAsync("BTC");

            Assert.Equal("N/A", result);
        }

        [Fact]
        public async Task AnalyzeSentimentAsync_ReturnsNA_WhenNewsTitlesEmpty()
        {
            var newsJson = """{ "results": [] }""";
            var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(newsJson, Encoding.UTF8, "application/json")
            });

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _configMock.Setup(c => c["AppBaseUrl"]).Returns("https://fake.api");

            var service = new SentimentService(_httpClientFactoryMock.Object, _configMock.Object, _loggerMock.Object);

            var result = await service.AnalyzeSentimentAsync("BTC");

            Assert.Equal("N/A", result);
        }

        [Fact]
        public async Task AnalyzeSentimentAsync_ReturnsNA_OnInvalidSentimentResponse()
        {
            var newsJson = """
        {
          "results": [{ "title": "Bitcoin news" }]
        }
        """;

            var invalidSentimentJson = """{ "unexpected": "data" }""";

            var httpClient = CreateHttpClientSequence(new[]
            {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(newsJson, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(invalidSentimentJson, Encoding.UTF8, "application/json")
            }
        });

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _configMock.Setup(c => c["AppBaseUrl"]).Returns("https://fake.api");

            var service = new SentimentService(_httpClientFactoryMock.Object, _configMock.Object, _loggerMock.Object);

            var result = await service.AnalyzeSentimentAsync("BTC");

            Assert.Equal("N/A", result);
        }

        [Fact]
        public async Task AnalyzeSentimentAsync_HandlesException_ReturnsNA()
        {
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Throws<Exception>();
            _configMock.Setup(c => c["AppBaseUrl"]).Returns("https://fake.api");

            var service = new SentimentService(_httpClientFactoryMock.Object, _configMock.Object, _loggerMock.Object);

            var result = await service.AnalyzeSentimentAsync("BTC");

            Assert.Equal("N/A", result);
        }

        // Helper to sequence multiple responses (news, then sentiment)
        private HttpClient CreateHttpClientSequence(HttpResponseMessage[] responses)
        {
            var callIndex = 0;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => responses[callIndex++]);

            return new HttpClient(handlerMock.Object);
        }
    }
}
