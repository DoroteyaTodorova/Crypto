using System.Text.Json.Serialization;

namespace CryptoBackend.Data
{
    public class CoinData
    {
        [JsonPropertyName("symbol")]

        public string Symbol { get; set; }

        [JsonPropertyName("price_usd")]
        public string PriceUsd { get; set; }
    }
}
