using Newtonsoft.Json;

namespace CurrencyConverter.Model
{
    public class ExchangeRateResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; } = string.Empty;

        [JsonProperty("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();

        [JsonProperty("error")]
        public string? Error { get; set; }
    }
}
