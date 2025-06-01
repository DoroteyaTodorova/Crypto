namespace CryptoBackend.Interface
{
    public interface ICoinLoreService
    {
        Task<List<(string Symbol, double PriceUsd)>> FetchCurrentPrices();
    }
}
