namespace CurrencyConverter.Utilities
{
    public class GdpCalculator
    {
        private static readonly Random _random = new();

        public static decimal? CalculateEstimatedGdp(long population, decimal? exchangeRate)
        {
            if (exchangeRate == null || exchangeRate == 0)
                return null;

            var randomMultiplier = _random.Next(1000, 2001);
            return (population * randomMultiplier) / exchangeRate.Value;
        }

        public static decimal? CalculateEstimatedGdp(long population, decimal? exchangeRate, int multiplier)
        {
            if (exchangeRate == null || exchangeRate == 0)
                return null;

            return (population * multiplier) / exchangeRate.Value;
        }
    }
}
