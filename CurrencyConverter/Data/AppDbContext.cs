using System.Diagnostics.Metrics;
using CurrencyConverter.Model;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Country> Countries { get; set; }
        public DbSet<RefreshLog> RefreshLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Country entity for MySQL
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Region);
                entity.HasIndex(e => e.CurrencyCode);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Capital)
                    .HasMaxLength(100);

                entity.Property(e => e.Region)
                    .HasMaxLength(50);

                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(10);

                entity.Property(e => e.FlagUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.ExchangeRate)
                    .HasPrecision(18, 6);

                entity.Property(e => e.EstimatedGdp)
                    .HasPrecision(18, 2);
            });

            // Configure RefreshLog entity for MySQL
            modelBuilder.Entity<RefreshLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(1000);
            });
        }
    }
}
