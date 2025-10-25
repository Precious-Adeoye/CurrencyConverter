using Newtonsoft.Json;

namespace CurrencyConverter.Model
{
    public class RestCountry
    {
        [JsonProperty("name")]
        public CountryName Name { get; set; } = new();

        [JsonProperty("capital")]
        public List<string>? Capital { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("population")]
        public long Population { get; set; }

        [JsonProperty("flags")]
        public Flags? Flags { get; set; }

        [JsonProperty("currencies")]
        public Dictionary<string, Currency>? Currencies { get; set; }

    }
}
