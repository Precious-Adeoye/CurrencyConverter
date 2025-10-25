namespace CurrencyConverter.DTO
{
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
}
