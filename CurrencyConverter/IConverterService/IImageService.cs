namespace CurrencyConverter.IConverterService
{
    public interface IImageService
    {
        Task<string?> GenerateSummaryImageAsync();
        Task<byte[]?> GetSummaryImageAsync();
    }
}
