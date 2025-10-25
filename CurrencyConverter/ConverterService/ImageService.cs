using CurrencyConverter.Data;
using CurrencyConverter.IConverterService;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Imaging;

namespace CurrencyConverter.ConverterService
{
    public class ImageService : IImageService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(AppDbContext context, IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task GenerateSummaryImageAsync()
        {
            try
            {
                var totalCountries = await _context.Countries.CountAsync();
                var topCountries = await _context.Countries
                    .Where(c => c.EstimatedGdp != null)
                    .OrderByDescending(c => c.EstimatedGdp)
                    .Take(5)
                    .Select(c => new { c.Name, c.EstimatedGdp })
                    .ToListAsync();

                var lastRefresh = await _context.Countries
                    .MaxAsync(c => (DateTime?)c.LastRefreshedAt) ?? DateTime.UtcNow;

                using var bitmap = new Bitmap(600, 400);
                using var graphics = Graphics.FromImage(bitmap);

                // Set background
                graphics.Clear(Color.White);

                // Draw title
                using var titleFont = new Font("Arial", 20, FontStyle.Bold);
                using var normalFont = new Font("Arial", 12);
                using var smallFont = new Font("Arial", 10);

                graphics.DrawString("Country GDP Summary", titleFont, Brushes.DarkBlue, new PointF(50, 20));

                // Draw total countries
                graphics.DrawString($"Total Countries: {totalCountries}", normalFont, Brushes.Black, new PointF(50, 70));

                // Draw top 5 countries by GDP
                graphics.DrawString("Top 5 Countries by GDP:", normalFont, Brushes.DarkGreen, new PointF(50, 100));

                float yPos = 130;
                foreach (var country in topCountries)
                {
                    var gdpText = $"{country.Name}: {country.EstimatedGdp:F2}";
                    graphics.DrawString(gdpText, smallFont, Brushes.Black, new PointF(70, yPos));
                    yPos += 25;
                }

                // Draw last refresh time
                graphics.DrawString($"Last Refresh: {lastRefresh:yyyy-MM-dd HH:mm:ss} UTC", smallFont, Brushes.Gray, new PointF(50, 350));

                // Ensure cache directory exists
                var cachePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "cache");
                Directory.CreateDirectory(cachePath);

                // Save image
                var imagePath = Path.Combine(cachePath, "summary.png");
                bitmap.Save(imagePath, ImageFormat.Png);

                _logger.LogInformation("Summary image generated successfully at {Path}", imagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary image");
            }
        }

        public async Task<byte[]?> GetSummaryImageAsync()
        {
            try
            {
                var cachePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "cache", "summary.png");

                if (!File.Exists(cachePath))
                {
                    _logger.LogWarning("Summary image not found at {Path}", cachePath);
                    return null;
                }

                return await File.ReadAllBytesAsync(cachePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading summary image");
                return null;
            }
        }
    }
}