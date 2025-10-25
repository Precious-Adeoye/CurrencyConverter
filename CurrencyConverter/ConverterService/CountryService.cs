using CurrencyConverter.Data;
using CurrencyConverter.DTO;
using CurrencyConverter.IConverterService;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.ConverterService
{
    public class CountryService : ICountryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CountryService> _logger;

        public CountryService(AppDbContext context, ILogger<CountryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CountryDto>> GetCountriesAsync(string? region, string? currency, string? sort)
        {
            try
            {
                var query = _context.Countries.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(region))
                {
                    query = query.Where(c => c.Region == region);
                }

                if (!string.IsNullOrEmpty(currency))
                {
                    query = query.Where(c => c.CurrencyCode == currency);
                }

                query = sort?.ToLower() switch
                {
                    "gdp_desc" => query.OrderByDescending(c => c.EstimatedGdp),
                    "gdp_asc" => query.OrderBy(c => c.EstimatedGdp),
                    "population_desc" => query.OrderByDescending(c => c.Population),
                    "population_asc" => query.OrderBy(c => c.Population),
                    "name_asc" => query.OrderBy(c => c.Name),
                    "name_desc" => query.OrderByDescending(c => c.Name),
                    _ => query.OrderBy(c => c.Name)
                };

                var countries = await query.Select(c => new CountryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Capital = c.Capital,
                    Region = c.Region,
                    Population = c.Population,
                    CurrencyCode = c.CurrencyCode,
                    ExchangeRate = c.ExchangeRate,
                    EstimatedGdp = c.EstimatedGdp,
                    FlagUrl = c.FlagUrl,
                    LastRefreshedAt = c.LastRefreshedAt
                }).ToListAsync();

                _logger.LogDebug("Retrieved {Count} countries with filters: Region={Region}, Currency={Currency}, Sort={Sort}",
                    countries.Count, region, currency, sort);

                return countries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries with filters: Region={Region}, Currency={Currency}, Sort={Sort}",
                    region, currency, sort);
                throw;
            }
        }

        public async Task<CountryDto?> GetCountryByNameAsync(string name)
        {
            try
            {
                var country = await _context.Countries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                if (country == null)
                {
                    _logger.LogWarning("Country not found: {Name}", name);
                    return null;
                }

                return new CountryDto
                {
                    Id = country.Id,
                    Name = country.Name,
                    Capital = country.Capital,
                    Region = country.Region,
                    Population = country.Population,
                    CurrencyCode = country.CurrencyCode,
                    ExchangeRate = country.ExchangeRate,
                    EstimatedGdp = country.EstimatedGdp,
                    FlagUrl = country.FlagUrl,
                    LastRefreshedAt = country.LastRefreshedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country by name: {Name}", name);
                throw;
            }
        }

        public async Task<bool> DeleteCountryAsync(string name)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                if (country == null)
                {
                    _logger.LogWarning("Country not found for deletion: {Name}", name);
                    return false;
                }

                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Country deleted successfully: {Name}", name);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting country: {Name}", name);
                throw;
            }
        }

        public async Task<int> GetTotalCountriesAsync()
        {
            try
            {
                return await _context.Countries.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting total countries");
                throw;
            }
        }

        public async Task<DateTime?> GetLastRefreshTimeAsync()
        {
            try
            {
                return await _context.Countries
                    .MaxAsync(c => (DateTime?)c.LastRefreshedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last refresh time");
                throw;
            }
        }
    }
}
