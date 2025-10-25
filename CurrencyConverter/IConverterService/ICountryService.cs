using CurrencyConverter.DTO;

namespace CurrencyConverter.IConverterService
{
    public interface ICountryService
    {
        Task<List<CountryDto>> GetCountriesAsync(string? region, string? currency, string? sort);
        Task<CountryDto?> GetCountryByNameAsync(string name);
        Task<bool> DeleteCountryAsync(string name);
        Task<int> GetTotalCountriesAsync();
        Task<DateTime?> GetLastRefreshTimeAsync();
    }
}
