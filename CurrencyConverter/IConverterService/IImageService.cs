namespace CurrencyConverter.IConverterService
{
    public interface IImageService
    {
        Task GenerateSummaryImageAsync();
        Task<byte[]?> GetSummaryImageAsync();
    }
}
