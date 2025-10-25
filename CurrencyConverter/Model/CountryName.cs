using Newtonsoft.Json;

namespace CurrencyConverter.Model
{
    public class CountryName
    {
        [JsonProperty("common")]
        public string Common { get; set; } = string.Empty;

        [JsonProperty("official")]
        public string Official { get; set; } = string.Empty;
    }
}
