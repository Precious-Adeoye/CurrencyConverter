using Newtonsoft.Json;

namespace CurrencyConverter.Model
{
    public class Currency
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("symbol")]
        public string? Symbol { get; set; }
    }
}
