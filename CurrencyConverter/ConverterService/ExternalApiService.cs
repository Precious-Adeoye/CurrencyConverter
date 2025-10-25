using CurrencyConverter.IConverterService;
using CurrencyConverter.Model;
using CurrencyConverter.Model.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CurrencyConverter.ConverterService
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalApiService> _logger;
        private readonly ExternalApiConfig _config;

        public ExternalApiService(
            HttpClient httpClient,
            ILogger<ExternalApiService> logger,
            IOptions<ExternalApiConfig> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;

            // Configure HttpClient
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.RequestTimeoutSeconds);
        }

        public async Task<ApiResponse<List<RestCountry>>> GetCountriesAsync()
        {
            for (int attempt = 1; attempt <= _config.RetryCount; attempt++)
            {
                try
                {
                    _logger.LogInformation("Fetching countries from API (attempt {Attempt})", attempt);

                    var response = await _httpClient.GetAsync(_config.CountriesUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var countries = JsonConvert.DeserializeObject<List<RestCountry>>(content) ?? new List<RestCountry>();

                        _logger.LogInformation("Successfully fetched {Count} countries", countries.Count);

                        return new ApiResponse<List<RestCountry>>
                        {
                            Success = true,
                            Data = countries,
                            SourceApi = "RestCountries"
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Countries API returned {StatusCode} on attempt {Attempt}",
                            response.StatusCode, attempt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching countries from API on attempt {Attempt}", attempt);

                    if (attempt == _config.RetryCount)
                    {
                        return new ApiResponse<List<RestCountry>>
                        {
                            Success = false,
                            ErrorMessage = "Could not fetch data from countries API",
                            SourceApi = "RestCountries"
                        };
                    }

                    // Wait before retry
                    await Task.Delay(1000 * attempt);
                }
            }

            return new ApiResponse<List<RestCountry>>
            {
                Success = false,
                ErrorMessage = "All attempts to fetch countries failed",
                SourceApi = "RestCountries"
            };
        }

        public async Task<ApiResponse<Dictionary<string, decimal>>> GetExchangeRatesAsync()
        {
            for (int attempt = 1; attempt <= _config.RetryCount; attempt++)
            {
                try
                {
                    _logger.LogInformation("Fetching exchange rates from API (attempt {Attempt})", attempt);

                    var response = await _httpClient.GetAsync(_config.ExchangeRatesUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var exchangeData = JsonConvert.DeserializeObject<ExchangeRateResponse>(content);

                        if (exchangeData?.Result == "success" && exchangeData.Rates.Any())
                        {
                            _logger.LogInformation("Successfully fetched {Count} exchange rates",
                                exchangeData.Rates.Count);

                            return new ApiResponse<Dictionary<string, decimal>>
                            {
                                Success = true,
                                Data = exchangeData.Rates,
                                SourceApi = "ExchangeRates"
                            };
                        }
                        else
                        {
                            _logger.LogWarning("Exchange rates API returned error: {Error}",
                                exchangeData?.Error);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Exchange rates API returned {StatusCode} on attempt {Attempt}",
                            response.StatusCode, attempt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching exchange rates from API on attempt {Attempt}", attempt);

                    if (attempt == _config.RetryCount)
                    {
                        return new ApiResponse<Dictionary<string, decimal>>
                        {
                            Success = false,
                            ErrorMessage = "Could not fetch data from exchange rates API",
                            SourceApi = "ExchangeRates"
                        };
                    }

                    // Wait before retry
                    await Task.Delay(1000 * attempt);
                }
            }

            return new ApiResponse<Dictionary<string, decimal>>
            {
                Success = false,
                ErrorMessage = "All attempts to fetch exchange rates failed",
                SourceApi = "ExchangeRates"
            };
        }

        public async Task<bool> TestApisAsync()
        {
            var countriesTest = await GetCountriesAsync();
            var exchangeTest = await GetExchangeRatesAsync();

            return countriesTest.Success && exchangeTest.Success;
        }
    }
}
