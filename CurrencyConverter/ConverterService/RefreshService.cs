using CurrencyConverter.Data;
using CurrencyConverter.IConverterService;
using CurrencyConverter.Model;
using CurrencyConverter.Utilities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.ConverterService
{
    public class RefreshService : IRefreshService
    {
        private readonly AppDbContext _context;
        private readonly IExternalApiService _externalApiService;
        private readonly IImageService _imageService;
        private readonly ILogger<RefreshService> _logger;

        public RefreshService(
            AppDbContext context,
            IExternalApiService externalApiService,
            IImageService imageService,
            ILogger<RefreshService> logger)
        {
            _context = context;
            _externalApiService = externalApiService;
            _imageService = imageService;
            _logger = logger;
        }
        public async Task<RefreshResult> RefreshCountriesAsync()
        {
            var result = new RefreshResult();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch data from external APIs
                var countriesTask = _externalApiService.GetCountriesAsync();
                var exchangeRatesTask = _externalApiService.GetExchangeRatesAsync();

                await Task.WhenAll(countriesTask, exchangeRatesTask);

                var countriesResponse = await countriesTask;
                var exchangeRatesResponse = await exchangeRatesTask;

                // Check if APIs responded successfully
                if (!countriesResponse.Success || !exchangeRatesResponse.Success)
                {
                    var errorSources = new List<string>();
                    if (!countriesResponse.Success) errorSources.Add(countriesResponse.SourceApi!);
                    if (!exchangeRatesResponse.Success) errorSources.Add(exchangeRatesResponse.SourceApi!);

                    result.ErrorMessage = $"External data source unavailable: {string.Join(" and ", errorSources)}";
                    await LogRefreshAsync(false, result.ErrorMessage, 0);
                    return result;
                }

                var countries = countriesResponse.Data ?? new List<RestCountry>();
                var exchangeRates = exchangeRatesResponse.Data ?? new Dictionary<string, decimal>();

                result.CountriesProcessed = countries.Count;

                foreach (var restCountry in countries)
                {
                    try
                    {
                        var countryName = restCountry.Name.Common;
                        var capital = restCountry.Capital?.FirstOrDefault();
                        var flagUrl = restCountry.Flags?.Png;
                        var currencyCode = restCountry.Currencies?.Keys.FirstOrDefault();

                        decimal? exchangeRate = null;
                        decimal? estimatedGdp = null;

                        if (!string.IsNullOrEmpty(currencyCode))
                        {
                            if (exchangeRates.ContainsKey(currencyCode))
                            {
                                exchangeRate = exchangeRates[currencyCode];
                                estimatedGdp = GdpCalculator.CalculateEstimatedGdp(restCountry.Population, exchangeRate);
                            }
                            else
                            {
                                result.Warnings.Add($"Exchange rate not found for currency: {currencyCode} in country: {countryName}");
                            }
                        }
                        else
                        {
                            estimatedGdp = 0;
                            result.Warnings.Add($"No currency found for country: {countryName}");
                        }

                        var existingCountry = await _context.Countries
                            .FirstOrDefaultAsync(c => c.Name.ToLower() == countryName.ToLower());

                        if (existingCountry != null)
                        {
                            // Update existing country
                            existingCountry.Capital = capital;
                            existingCountry.Region = restCountry.Region;
                            existingCountry.Population = restCountry.Population;
                            existingCountry.CurrencyCode = currencyCode;
                            existingCountry.ExchangeRate = exchangeRate;
                            existingCountry.EstimatedGdp = estimatedGdp;
                            existingCountry.FlagUrl = flagUrl;
                            existingCountry.LastRefreshedAt = DateTime.UtcNow;
                            result.CountriesUpdated++;
                        }
                        else
                        {
                            // Create new country
                            var country = new Country
                            {
                                Name = countryName,
                                Capital = capital,
                                Region = restCountry.Region,
                                Population = restCountry.Population,
                                CurrencyCode = currencyCode,
                                ExchangeRate = exchangeRate,
                                EstimatedGdp = estimatedGdp,
                                FlagUrl = flagUrl,
                                LastRefreshedAt = DateTime.UtcNow
                            };
                            await _context.Countries.AddAsync(country);
                            result.CountriesCreated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing country: {CountryName}", restCountry.Name.Common);
                        result.Warnings.Add($"Failed to process country: {restCountry.Name.Common}");
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // FIXED: Generate summary image synchronously and handle errors properly
                try
                {
                    _logger.LogInformation("Starting image generation...");
                    var imageResult = await _imageService.GenerateSummaryImageAsync();

                    if (imageResult != null)
                    {
                        _logger.LogInformation("Summary image generated successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Image generation returned null");
                        result.Warnings.Add("Summary image generation failed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating summary image");
                    result.Warnings.Add($"Image generation failed: {ex.Message}");
                }

                result.Success = true;
                await LogRefreshAsync(true, null, result.CountriesProcessed);

                _logger.LogInformation(
                    "Refresh completed: {Processed} processed, {Updated} updated, {Created} created",
                    result.CountriesProcessed, result.CountriesUpdated, result.CountriesCreated);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error refreshing countries");

                result.ErrorMessage = "Internal server error during refresh";
                await LogRefreshAsync(false, ex.Message, 0);

                return result;
            }
        }
        public async Task<DateTime?> GetLastSuccessfulRefreshAsync()
        {
            return await _context.RefreshLogs
                .Where(r => r.Success)
                .OrderByDescending(r => r.RefreshedAt)
                .Select(r => (DateTime?)r.RefreshedAt)
                .FirstOrDefaultAsync();
        }

        private async Task LogRefreshAsync(bool success, string? errorMessage, int totalCountries)
        {
            var log = new RefreshLog
            {
                RefreshedAt = DateTime.UtcNow,
                Success = success,
                ErrorMessage = errorMessage,
                TotalCountries = totalCountries
            };

            await _context.RefreshLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

      
    }
}