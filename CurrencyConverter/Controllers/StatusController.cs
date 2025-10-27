using CurrencyConverter.Data;
using CurrencyConverter.DTO;
using CurrencyConverter.IConverterService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRefreshService _refreshService;
        private readonly ILogger<StatusController> _logger;

        public StatusController(
        AppDbContext context,
        IRefreshService refreshService,
        ILogger<StatusController> logger)
        {
            _context = context;
            _refreshService = refreshService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                _logger.LogInformation("Get status endpoint called");

                var totalCountries = await _context.Countries.CountAsync();
                var lastRefresh = await _refreshService.GetLastSuccessfulRefreshAsync();

                var response = new
                {
                    total_countries = totalCountries,
                    last_refreshed_at = lastRefresh?.ToString("u")
                };

                _logger.LogInformation("Returning status: {TotalCountries} countries, last refreshed: {LastRefresh}",
                    totalCountries, lastRefresh);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status");
                return StatusCode(500, new ErrorResponse { Error = "Internal server error" });
            }
    
        }  
    }
}
