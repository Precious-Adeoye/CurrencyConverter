namespace CurrencyConverter.DTO
{
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Capital { get; set; }
        public string? Region { get; set; }
        public long Population { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? EstimatedGdp { get; set; }
        public string? FlagUrl { get; set; }
        public DateTime LastRefreshedAt { get; set; }
    }
}
