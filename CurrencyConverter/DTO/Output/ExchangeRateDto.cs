using Newtonsoft.Json;

namespace CurrencyConverter.DTO.Output
{
    public class ExchangeRateDto
    {
        public string Base { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public Dictionary<string, decimal> Rates { get; set; } = [];
    }
}
