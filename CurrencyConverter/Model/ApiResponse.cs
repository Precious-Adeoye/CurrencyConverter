namespace CurrencyConverter.Model
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SourceApi { get; set; }
    }
}
