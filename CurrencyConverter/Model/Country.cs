using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyConverter.Model
{
    [Table("countries")]
    public class Country
    {
       
        
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("capital")]
        public string? Capital { get; set; }

        [MaxLength(50)]
        [Column("region")]
        public string? Region { get; set; }

        [Required]
        [Column("population")]
        public long Population { get; set; }

        [MaxLength(10)]
        [Column("currency_code")]
        public string? CurrencyCode { get; set; }

        [Column("exchange_rate", TypeName = "decimal(18,6)")]
        public decimal? ExchangeRate { get; set; }

        [Column("estimated_gdp", TypeName = "decimal(18,2)")]
        public decimal? EstimatedGdp { get; set; }

        [MaxLength(500)]
        [Column("flag_url")]
        public string? FlagUrl { get; set; }

        [Column("last_refreshed_at")]
        public DateTime LastRefreshedAt { get; set; } = DateTime.UtcNow;
    }
    [Table("refresh_logs")]
    public class RefreshLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("refreshed_at")]
        public DateTime RefreshedAt { get; set; } = DateTime.UtcNow;

        [Column("total_countries")]
        public int TotalCountries { get; set; }

        [Column("success")]
        public bool Success { get; set; }

        [MaxLength(1000)]
        [Column("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
