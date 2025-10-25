using System;
using CurrencyConverter.ConverterService;
using CurrencyConverter.Data;
using CurrencyConverter.HealthCheck;
using CurrencyConverter.IConverterService;
using CurrencyConverter.MiddelWare;
using CurrencyConverter.Model.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configuration - Support both appsettings and environment variables
            builder.Services.Configure<ExternalApiConfig>(
                builder.Configuration.GetSection("ExternalApis"));

            // MySQL Database with environment variable support
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                ?? Environment.GetEnvironmentVariable("MYSQLCONNECTIONSTRING")
                                ?? "Server=localhost;Database=currency_converter;Uid=root;Pwd=;";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // HTTP Client
            builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>();

            // Services
            builder.Services.AddScoped<ICountryService, CountryService>();
            builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
            builder.Services.AddScoped<IRefreshService, RefreshService>();
            builder.Services.AddScoped<IImageService, ImageService>();

            // CORS for production
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database");

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            // Initialize database
            await InitializeDatabaseAsync(app);

            // Get port from environment variable (for Railway)
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Run($"http://0.0.0.0:{port}");
        }

        private static async Task InitializeDatabaseAsync(WebApplication webApp)
        {
            using var scope = webApp.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();

                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Database initialization failed");

                if (webApp.Environment.IsProduction())
                {
                    // In production, we can continue without throwing
                    logger.LogWarning("Continuing without database initialization");
                }
            }
        }
    }
}