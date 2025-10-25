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

            // Configuration
            builder.Services.Configure<ExternalApiConfig>(
                builder.Configuration.GetSection("ExternalApis"));

            // MySQL Database with Railway support
            var connectionString = GetConnectionString(builder.Configuration);
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // HTTP Client
            builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>();

            // Services
            builder.Services.AddScoped<ICountryService, CountryService>();
            builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
            builder.Services.AddScoped<IRefreshService, RefreshService>();
            builder.Services.AddScoped<IImageService, ImageService>();

            // CORS
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

            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Run($"http://0.0.0.0:{port}");
        }

        private static string GetConnectionString(ConfigurationManager configuration)
        {
            // Try different environment variable names
            var railwayUrl = Environment.GetEnvironmentVariable("MYSQLCONNECTIONSTRING")
                          ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                          ?? Environment.GetEnvironmentVariable("MYSQL_URL");

            if (!string.IsNullOrEmpty(railwayUrl))
            {
                // Convert Railway format to standard MySQL format
                return ConvertRailwayConnectionString(railwayUrl);
            }

            // Fallback to appsettings
            return configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=currency_converter;Uid=root;Pwd=;";
        }

        private static string ConvertRailwayConnectionString(string railwayUrl)
        {
            // Remove mysql:// prefix if present
            if (railwayUrl.StartsWith("mysql://root:alzmuWvxyNKRnudpPoiMbFMvoJioIHhH@mysql.railway.internal:3306/railway"))
            {
                railwayUrl = railwayUrl.Substring(8);
            }

            // Parse the connection string
            var parts = railwayUrl.Split('@');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid Railway connection string format");
            }

            var userPass = parts[0].Split(':');
            var hostDb = parts[1].Split('/');
            var hostPort = hostDb[0].Split(':');

            if (userPass.Length != 2 || hostDb.Length != 2 || hostPort.Length != 2)
            {
                throw new ArgumentException("Invalid Railway connection string format");
            }

            var user = userPass[0];
            var password = userPass[1];
            var host = hostPort[0];
            var port = hostPort[1];
            var database = hostDb[1];

            return $"Server={host};Database={database};Uid={user};Pwd={password};Port={port};";
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
                    logger.LogWarning("Continuing without database initialization");
                }
            }
        }
    }
}