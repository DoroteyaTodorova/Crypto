using CryptoBackend.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CryptoBackend.Tests
{
    public class CoinLoreServiceTests
    {
        [Fact]
        public async Task FetchCurrentPrices_ShouldReturnEmptyList_OnApiFailure()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<CoinLoreService>>();

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var client = new HttpClient(mockHandler.Object);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var service = new CoinLoreService(mockFactory.Object, mockLogger.Object);

            // Act
            var result = await service.FetchCurrentPrices();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchCurrentPrices_ShouldIgnoreInvalidPrice()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<CoinLoreService>>();

            var json = "{\"data\":[{\"symbol\":\"BTC\",\"price_usd\":\"not-a-number\"}]}";
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var client = new HttpClient(mockHandler.Object);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var service = new CoinLoreService(mockFactory.Object, mockLogger.Object);

            // Act
            var result = await service.FetchCurrentPrices();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchCurrentPrices_ShouldReturnPrices()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<CoinLoreService>>();

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":[{\"symbol\":\"BTC\",\"price_usd\":\"68000\"}]}")
                });

            var client = new HttpClient(mockHandler.Object);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var service = new CoinLoreService(mockFactory.Object, mockLogger.Object);

            // Act
            var result = await service.FetchCurrentPrices();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result[0].Symbol.Should().Be("BTC");
            result[0].PriceUsd.Should().Be(68000);
        }

        [Fact]
        public async Task FetchCurrentPrices_ShouldReturnMultipleCoins()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<CoinLoreService>>();

            var json = "{\"data\":[" +
                       "{\"symbol\":\"BTC\",\"price_usd\":\"68000\"}," +
                       "{\"symbol\":\"ETH\",\"price_usd\":\"3700\"}" +
                       "]}";

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var client = new HttpClient(mockHandler.Object);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var service = new CoinLoreService(mockFactory.Object, mockLogger.Object);

            // Act
            var result = await service.FetchCurrentPrices();

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Symbol).Should().BeEquivalentTo("BTC", "ETH");
        }

    }
}
