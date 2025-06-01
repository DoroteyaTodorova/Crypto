using CryptoBackend.Dto;
using CryptoBackend.Interface;
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
    public class PortfolioServiceTests
    {
        [Fact]
        public async Task CalculatePortfolioAsync_ShouldSkipInvalidCoinSymbols()
        {
            // Arrange
            var mockCoinService = new Mock<ICoinLoreService>();
            var mockSentimentService = new Mock<ISentimentService>();
            var mockLogger = new Mock<ILogger<PortfolioService>>();

            var request = new PortfolioRequest
            {
                IncludeSentiment = false,
                Portfolio = new List<PortfolioEntry>
                {
                    new() { Coin = null, Amount = 1, BuyPrice = 100 },
                    new() { Coin = "", Amount = 1, BuyPrice = 100 },
                    new() { Coin = "  ", Amount = 1, BuyPrice = 100 }
                }
            };

            mockCoinService.Setup(s => s.FetchCurrentPrices())
                .ReturnsAsync(new List<(string Symbol, double PriceUsd)>
                {
                    ("BTC", 30000),
                    ("ETH", 2000)
                });

            var service = new PortfolioService(mockCoinService.Object, mockSentimentService.Object, mockLogger.Object);

            // Act
            var result = await service.CalculatePortfolioAsync(request);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ShouldSkipItems_IfSymbolNotInPrices()
        {
            // Arrange
            var mockCoinService = new Mock<ICoinLoreService>();
            var mockSentimentService = new Mock<ISentimentService>();
            var mockLogger = new Mock<ILogger<PortfolioService>>();

            var request = new PortfolioRequest
            {
                IncludeSentiment = false,
                Portfolio = new List<PortfolioEntry>
                {
                    new() { Coin = "UNKNOWN", Amount = 1, BuyPrice = 100 }
                }
            };

            mockCoinService.Setup(s => s.FetchCurrentPrices())
                .ReturnsAsync(new List<(string Symbol, double PriceUsd)>
                {
                    ("BTC", 30000),
                    ("ETH", 2000)
                });

            var service = new PortfolioService(mockCoinService.Object, mockSentimentService.Object, mockLogger.Object);

            // Act
            var result = await service.CalculatePortfolioAsync(request);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ShouldReturnCorrectResults_WithoutSentiment()
        {
            // Arrange
            var mockCoinService = new Mock<ICoinLoreService>();
            var mockSentimentService = new Mock<ISentimentService>();
            var mockLogger = new Mock<ILogger<PortfolioService>>();

            var portfolioRequest = new PortfolioRequest
            {
                IncludeSentiment = false,
                Portfolio = new List<PortfolioEntry>
                {
                    new() { Coin = "BTC", Amount = 1, BuyPrice = 50000 }
                }
            };

            mockCoinService.Setup(s => s.FetchCurrentPrices())
                .ReturnsAsync(new List<(string Symbol, double PriceUsd)>
                {
                    ("BTC", 60000)
                });

            var service = new PortfolioService(mockCoinService.Object, mockSentimentService.Object, mockLogger.Object);

            // Act
            var result = await service.CalculatePortfolioAsync(portfolioRequest);

            // Assert
            result.Should().HaveCount(1);
            result[0].Coin.Should().Be("BTC");
            result[0].CurrentPrice.Should().Be(60000);
            result[0].ChangePercent.Should().Be(20); // (60000-50000)/50000*100
            result[0].Sentiment.Should().Be("N/A");
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ShouldIncludeSentiment_IfRequested()
        {
            // Arrange
            var mockCoinService = new Mock<ICoinLoreService>();
            var mockSentimentService = new Mock<ISentimentService>();
            var mockLogger = new Mock<ILogger<PortfolioService>>();

            var request = new PortfolioRequest
            {
                IncludeSentiment = true,
                Portfolio = new List<PortfolioEntry>
                {
                    new() { Coin = "ETH", Amount = 2, BuyPrice = 2000 }
                }
            };

            mockCoinService.Setup(s => s.FetchCurrentPrices())
                .ReturnsAsync(new List<(string Symbol, double PriceUsd)>
                {
                    ("ETH", 3000)
                });

            mockSentimentService.Setup(s => s.AnalyzeSentimentAsync("ETH")).ReturnsAsync("Positive");

            var service = new PortfolioService(mockCoinService.Object, mockSentimentService.Object, mockLogger.Object);

            // Act
            var result = await service.CalculatePortfolioAsync(request);

            // Assert
            result.Should().HaveCount(1);
            result[0].Sentiment.Should().Be("Positive");
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ShouldHandleCoinServiceFailure_Gracefully()
        {
            // Arrange
            var mockCoinService = new Mock<ICoinLoreService>();
            var mockSentimentService = new Mock<ISentimentService>();
            var mockLogger = new Mock<ILogger<PortfolioService>>();

            var request = new PortfolioRequest
            {
                IncludeSentiment = false,
                Portfolio = new List<PortfolioEntry>
                {
                    new() { Coin = "BTC", Amount = 1, BuyPrice = 50000 }
                }
            };

            mockCoinService.Setup(s => s.FetchCurrentPrices()).ThrowsAsync(new Exception("API down"));

            var service = new PortfolioService(mockCoinService.Object, mockSentimentService.Object, mockLogger.Object);

            // Act
            Func<Task> act = async () => await service.CalculatePortfolioAsync(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("*API down*");
        }
    }
}
