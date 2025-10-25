using Newtonsoft.Json;

namespace CurrencyConverter.Model
{
    public class Flags
    {
        [JsonProperty("png")]
        public string? Png { get; set; }

        [JsonProperty("svg")]
        public string? Svg { get; set; }
    }
}
