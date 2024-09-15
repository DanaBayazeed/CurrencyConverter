using CurrencyConverter.Contract;
using CurrencyConverter.DTO.Output;
using CurrencyConverter.DTO.Output.Pagination;
using CurrencyConverter.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System.Net;

namespace CurrencyConverter.Services
{
    public class FrankfurterService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiBaseUrl;

        private readonly int _maxRetryAttempts;

        private readonly TimeSpan _cacheExpiryInSeconds;

		private readonly IMemoryCache _cache;

		private readonly IConfiguration _configuration;

		private AsyncRetryPolicy RetryPolicy { get; }

		public FrankfurterService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _cache = cache;

			_configuration = configuration;

            _apiBaseUrl = _configuration["ExchangeRateService:Franfurt:URL"] ?? "https://api.frankfurter.app";

            _maxRetryAttempts = int.TryParse(_configuration["ExchangeRateService:Franfurt:RetryAttempts"], out var retryAttempts) ? retryAttempts : 3;

            _cacheExpiryInSeconds = TimeSpan.FromSeconds(int.TryParse(_configuration["ExchangeRateService:Franfurt:CacheExpiryInSeconds"], out var cacheExpiry) ? cacheExpiry : 10);

			_httpClient.BaseAddress = new Uri(_apiBaseUrl);

			RetryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(_maxRetryAttempts, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
		}

        public async Task<ExchangeRateDto> GetLatestAsync(string baseCurrency = "EUR")
        {
			baseCurrency = baseCurrency.ToUpper();

			var cacheKey = $"Latest-{baseCurrency}";

            if (_cache.TryGetValue(cacheKey, out ExchangeRateDto? cachedRates))
                return cachedRates!;

            try
            {
				var response = await RetryPolicy.ExecuteAsync(async () =>
				{
					return await _httpClient.GetAsync($"/latest?base={baseCurrency}");
				});

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NotFoundException("Currency not found.");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var exchangeRate = JsonConvert.DeserializeObject<ExchangeRateDto>(content);

                _cache.Set(cacheKey, exchangeRate, _cacheExpiryInSeconds);

                return exchangeRate!;

            }
            catch (BaseException) { throw; }

            catch (Exception ex)
            {
                throw new BaseException("An error occurred while fetching the latest exchange rates.", ex);
            }
        }

        public async Task<decimal> ConvertAsync(string baseCurrency, string quoteCurrency, decimal amount)
        {
			baseCurrency = baseCurrency.ToUpper();

            quoteCurrency = quoteCurrency.ToUpper();

            if (baseCurrency == quoteCurrency) return amount;

			var cacheKey = $"Latest-{baseCurrency}";

            if (_cache.TryGetValue(cacheKey, out ExchangeRateDto? cachedRates) && cachedRates!.Rates.TryGetValue(quoteCurrency, out decimal value))
                return value * amount;

			try
            {
				var response = await RetryPolicy.ExecuteAsync(async () =>
				{
					return await _httpClient.GetAsync($"/latest?amount={amount}&from={baseCurrency}&to={quoteCurrency}");
				});

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NotFoundException("Currency not found.");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var currency = JsonConvert.DeserializeObject<ExchangeRateDto>(content);

                return currency == null ? throw new Exception("Currency conversion not supported.") : currency.Rates[quoteCurrency];
            }
            catch (BaseException) { throw; }

            catch (Exception ex)
            {
                throw new BaseException("An error occurred while fetching the latest exchange rates.", ex);
            }
        }

		public async Task<ExchangeRateHistoryDto> GetHistoryAsync(string baseCurrency, string fromDate, string toDate)
		{
			baseCurrency = baseCurrency.ToUpper();

			DateTime startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);

			DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", null);

			var cacheKey = $"History-{baseCurrency}-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}";

			if (_cache.TryGetValue(cacheKey, out ExchangeRateHistoryDto? cachedHistory))
				return cachedHistory!;

			try
			{
				var response = await RetryPolicy.ExecuteAsync(async () =>
				{
					return await _httpClient.GetAsync($"/{fromDate}..{toDate}?from={baseCurrency}");
				});

				if (response.StatusCode == HttpStatusCode.NotFound)
					throw new NotFoundException("Currency history not found.");

				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();

				var history = JsonConvert.DeserializeObject<ExchangeRateHistoryDto>(content);

				_cache.Set(cacheKey, history, _cacheExpiryInSeconds);

				return history!;
			}
			catch (BaseException) { throw; }

			catch (Exception ex)
			{
				throw new BaseException("An error occurred while fetching the latest exchange rates.", ex);
			}
		}

		public async Task<PagedResponseDto<ExchangeRateHistoryDto>> GetHistoryAsync(string baseCurrency, string fromDate, string toDate, int page = 1, int pageSize = 10)
        {
			baseCurrency = baseCurrency.ToUpper();

			DateTime startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);

			DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", null);

			var pagination = new Pagination((endDate - startDate).Days + 1, page, pageSize);

			DateTime pageStartDate = startDate.AddDays((pagination.Page - 1) * pagination.PageSize);

			DateTime pageEndDate = new[] { pageStartDate.AddDays(pagination.PageSize - 1), endDate }.Min();  
			
			var cacheKey = $"History-{baseCurrency}-{pageStartDate:yyyy-MM-dd}-{pageEndDate:yyyy-MM-dd}";

			if (_cache.TryGetValue(cacheKey, out ExchangeRateHistoryDto? cachedHistory))
				return new PagedResponseDto<ExchangeRateHistoryDto>(cachedHistory!, pagination);

			try
            {
				var response = await RetryPolicy.ExecuteAsync(async () =>
				{
					return await _httpClient.GetAsync($"/{pageStartDate:yyyy-MM-dd}..{pageEndDate:yyyy-MM-dd}?from={baseCurrency}");
				});

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NotFoundException("Currency history not found.");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var history = JsonConvert.DeserializeObject<ExchangeRateHistoryDto>(content)!;

				history.Normalize(pageStartDate, pageEndDate);

				_cache.Set(cacheKey, history, _cacheExpiryInSeconds);

				return new PagedResponseDto<ExchangeRateHistoryDto>(history!, new Pagination((endDate - startDate).Days + 1, page, pageSize));
            }
            catch (BaseException) { throw; }

            catch (Exception ex)
            {
                throw new BaseException("An error occurred while fetching the latest exchange rates.", ex);
			}
		}
	}
}
