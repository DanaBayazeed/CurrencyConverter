using CurrencyConverter.DTO.Output;
using CurrencyConverter.DTO.Output.Pagination;

namespace CurrencyConverter.Contract
{
    public interface IExchangeRateService
    {
        public Task<ExchangeRateDto> GetLatestAsync(string baseCurrency = "EUR");

        public Task<decimal> ConvertAsync(string baseCurrency, string quoteCurrency, decimal amount);

		public Task<ExchangeRateHistoryDto> GetHistoryAsync(string baseCurrency, string fromDate, string toDate);

		public Task<PagedResponseDto<ExchangeRateHistoryDto>> GetHistoryAsync(string baseCurrency, string fromDate, string toDate, int page, int pageSize);
    }
}
