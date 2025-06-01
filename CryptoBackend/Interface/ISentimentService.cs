namespace CryptoBackend.Interface
{
    public interface ISentimentService
    {
        Task<string> AnalyzeSentimentAsync(string coinSymbol);

    }
}
