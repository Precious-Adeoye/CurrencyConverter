using CurrencyConverter.Data;
using CurrencyConverter.IConverterService;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace CurrencyConverter.ConverterService
{
    public class SkiaImageService : IImageService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SkiaImageService> _logger;
        private static string? _cachedImageBase64;

        public SkiaImageService(AppDbContext context, ILogger<SkiaImageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GenerateSummaryImageAsync()
        {
            try
            {
                _logger.LogInformation("Generating summary image with SkiaSharp...");

                var totalCountries = await _context.Countries.CountAsync();
                var topCountries = await _context.Countries
                    .Where(c => c.EstimatedGdp != null)
                    .OrderByDescending(c => c.EstimatedGdp)
                    .Take(5)
                    .Select(c => new { c.Name, c.EstimatedGdp })
                    .ToListAsync();

                var lastRefresh = await _context.Countries
                    .MaxAsync(c => (DateTime?)c.LastRefreshedAt) ?? DateTime.UtcNow;

                // Create image with SkiaSharp
                var imageInfo = new SKImageInfo(600, 400);
                using var surface = SKSurface.Create(imageInfo);
                var canvas = surface.Canvas;

                // Clear background
                canvas.Clear(SKColors.White);

                // Create paints
                var titlePaint = new SKPaint
                {
                    Color = SKColors.DarkBlue,
                    TextSize = 20,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                };

                var normalPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 12,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };

                var smallPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 10,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };

                var greenPaint = new SKPaint
                {
                    Color = SKColors.DarkGreen,
                    TextSize = 12,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };

                var grayPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 10,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };

                // Draw title
                canvas.DrawText("Country GDP Summary", 50, 30, titlePaint);
                canvas.DrawText($"Total Countries: {totalCountries}", 50, 70, normalPaint);
                canvas.DrawText("Top 5 Countries by GDP:", 50, 100, greenPaint);

                // Draw top countries
                float yPos = 120;
                foreach (var country in topCountries)
                {
                    var gdpText = $"{country.Name}: {country.EstimatedGdp:F2}";
                    canvas.DrawText(gdpText, 70, yPos, smallPaint);
                    yPos += 20;
                }

                // Draw last refresh time
                canvas.DrawText($"Last Refresh: {lastRefresh:yyyy-MM-dd HH:mm:ss} UTC", 50, 350, grayPaint);

                // Convert to PNG bytes
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                var imageBytes = data.ToArray();
                var base64String = Convert.ToBase64String(imageBytes);

                _cachedImageBase64 = base64String;
                _logger.LogInformation("Summary image generated successfully with SkiaSharp");

                return base64String;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary image with SkiaSharp");
                return null;
            }
        }

        public Task<byte[]?> GetSummaryImageAsync()
        {
            if (string.IsNullOrEmpty(_cachedImageBase64))
            {
                return Task.FromResult<byte[]?>(null);
            }

            var imageBytes = Convert.FromBase64String(_cachedImageBase64);
            return Task.FromResult<byte[]?>(imageBytes);
        }

        public static string? GetCachedImageBase64() => _cachedImageBase64;

       
    }
}
