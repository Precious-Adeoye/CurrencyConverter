using CurrencyConverter.Model;

namespace CurrencyConverter.IConverterService
{
    public interface IExternalApiService
    {
        Task<ApiResponse<List<RestCountry>>> GetCountriesAsync();
        Task<ApiResponse<Dictionary<string, decimal>>> GetExchangeRatesAsync();
        Task<bool> TestApisAsync();
    }
}
