using CurrencyConverter.ConverterService;

namespace CurrencyConverter.IConverterService
{
    public interface IRefreshService
    {
        Task<RefreshResult> RefreshCountriesAsync();
        Task<DateTime?> GetLastSuccessfulRefreshAsync();
    }

    public class RefreshResult
    {
        public bool Success { get; set; }
        public int CountriesProcessed { get; set; }
        public int CountriesUpdated { get; set; }
        public int CountriesCreated { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
