using CryptoBackend.Data;
using System.Text.Json.Serialization;

namespace CryptoBackend.Dto
{
    public class CoinLoreResponse
    {
        [JsonPropertyName("data")]
        public List<CoinData> Data { get; set; }
    }
}
