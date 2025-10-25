namespace CurrencyConverter.Model.Configuration
{
    public class ExternalApiConfig
    {
        public string CountriesUrl { get; set; } = "https://restcountries.com/v3.1/all?fields=name,capital,region,population,flags,currencies";
        public string ExchangeRatesUrl { get; set; } = "https://open.er-api.com/v6/latest/USD";
        public int RequestTimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
    }
}
