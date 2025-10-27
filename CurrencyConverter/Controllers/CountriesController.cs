using CurrencyConverter.DTO;
using CurrencyConverter.IConverterService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IRefreshService _refreshService;
        private readonly IImageService _imageService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(
            ICountryService countryService,
            IRefreshService refreshService,
            IImageService imageService,
            ILogger<CountriesController> logger)
        {
            _countryService = countryService;
            _refreshService = refreshService;
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshCountries()
        {
            try
            {
                _logger.LogInformation("Refresh countries endpoint called");

                var result = await _refreshService.RefreshCountriesAsync();

                if (result.Success)
                {
                    var response = new
                    {
                        message = "Countries refreshed successfully",
                        processed = result.CountriesProcessed,
                        updated = result.CountriesUpdated,
                        created = result.CountriesCreated,
                        warnings = result.Warnings
                    };

                    return Ok(response);
                }

                return StatusCode(503, new ErrorResponse
                {
                    Error = result.ErrorMessage ?? "External data source unavailable"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshCountries endpoint");
                return StatusCode(503, new ErrorResponse
                {
                    Error = "External data source unavailable",
                    Details = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries(
            [FromQuery] string? region,
            [FromQuery] string? currency,
            [FromQuery] string? sort)
        {
            try
            {
                _logger.LogInformation("Get countries called - Region: {Region}, Currency: {Currency}, Sort: {Sort}",
                    region, currency, sort);

                var countries = await _countryService.GetCountriesAsync(region, currency, sort);

                _logger.LogInformation("Returning {Count} countries", countries.Count);

                return Ok(countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting countries");
                return StatusCode(500, new ErrorResponse { Error = "Internal server error" });
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetCountryByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new ErrorResponse { Error = "Country name is required" });
                }

                _logger.LogInformation("Get country by name: {Name}", name);

                var country = await _countryService.GetCountryByNameAsync(name);

                if (country == null)
                {
                    _logger.LogWarning("Country not found: {Name}", name);
                    return NotFound(new ErrorResponse { Error = "Country not found" });
                }

                return Ok(country);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting country by name: {Name}", name);
                return StatusCode(500, new ErrorResponse { Error = "Internal server error" });
            }
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteCountry(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new ErrorResponse { Error = "Country name is required" });
                }

                _logger.LogInformation("Delete country: {Name}", name);

                var deleted = await _countryService.DeleteCountryAsync(name);

                if (!deleted)
                {
                    _logger.LogWarning("Country not found for deletion: {Name}", name);
                    return NotFound(new ErrorResponse { Error = "Country not found" });
                }

                _logger.LogInformation("Country deleted successfully: {Name}", name);
                return Ok(new { message = "Country deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting country: {Name}", name);
                return StatusCode(500, new ErrorResponse { Error = "Internal server error" });
            }
        }

        [HttpGet("image")]
        public async Task<IActionResult> GetCountriesImage()
        {
            try
            {
                _logger.LogInformation("Get countries image called");

                var imageBytes = await _imageService.GetSummaryImageAsync();

                if (imageBytes == null)
                {
                    _logger.LogWarning("Summary image not found");
                    return NotFound(new ErrorResponse { Error = "Summary image not found" });
                }

                _logger.LogInformation("Returning summary image");
                return File(imageBytes, "image/png", "summary.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting countries image");
                return StatusCode(500, new ErrorResponse { Error = "Internal server error" });
            }
        }

        [HttpGet("image-test")]
        public async Task<IActionResult> GetImageTest()
        {
            try
            {
                // Generate image on-demand
                var imageService = HttpContext.RequestServices.GetRequiredService<IImageService>();

                // Use reflection to call the image generation
                var method = imageService.GetType().GetMethod("GenerateSummaryImageAsync");
                if (method != null)
                {
                    var task = (Task)method.Invoke(imageService, null);
                    await task;
                }

                // Try to get the image
                var imageBytes = await imageService.GetSummaryImageAsync();

                if (imageBytes == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "Image generation failed",
                        Details = "The image service could not generate or save the image"
                    });
                }

                return File(imageBytes, "image/png", "summary.png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "Image test failed",
                    Details = ex.Message
                });
            }
        }
    }
}
