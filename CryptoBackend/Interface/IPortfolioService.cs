using CryptoBackend.Dto;

namespace CryptoBackend.Interface
{
    public interface IPortfolioService
    {
        Task<List<PortfolioResult>> CalculatePortfolioAsync(PortfolioRequest request);

    }
}
