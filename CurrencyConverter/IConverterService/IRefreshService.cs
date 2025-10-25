using CurrencyConverter.ConverterService;

namespace CurrencyConverter.IConverterService
{
    public interface IRefreshService
    {
        Task<RefreshResult> RefreshCountriesAsync();
        Task<DateTime?> GetLastSuccessfulRefreshAsync();
    }
}
