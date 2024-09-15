using CurrencyConverter.DTO.Output;
using CurrencyConverter.Exceptions;
using CurrencyConverter.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyConverter.Tests.Services
{
    [TestFixture]
    public class FrankfurterServiceTests
    {
        private FrankfurterService _frankfurterService;

        private Mock<HttpClient> _mockHttpClient;

        private Mock<IMemoryCache> _mockMemoryCache;

        private Mock<IConfiguration> _mockConfiguration;

        [SetUp]
        public void Setup()
        {
            _mockHttpClient = new Mock<HttpClient>();

            _mockMemoryCache = new Mock<IMemoryCache>();

            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["ExchangeRateService:Franfurt:URL"])
                .Returns("https://api.frankfurter.app");

            _mockConfiguration.Setup(c => c["ExchangeRateService:Franfurt:RetryAttempts"])
                .Returns("3");

            _mockConfiguration.Setup(c => c["ExchangeRateService:Franfurt:CacheExpiryInSeconds"])
                .Returns("10");

            _frankfurterService = new FrankfurterService(_mockHttpClient.Object, _mockMemoryCache.Object, _mockConfiguration.Object);
        }

        [Test]
        public async Task GetLatestAsync_ShouldReturnCachedValue_IfPresent()
        {
            var baseCurrency = "EUR";

            var cacheKey = $"Latest-{baseCurrency.ToUpper()}";

            var cachedRates = new ExchangeRateDto
            {
                Base = "EUR", Date = DateOnly.FromDateTime(DateTime.Now),
                Rates = new Dictionary<string, decimal>
                {
                    { "USD", 1.1m }, { "GBP", 0.85m }
                }
            };

            _mockMemoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object?>.IsAny))
             .Returns((object key, out object? value) =>
             {
                 value = cachedRates;
                 return true; 
             });

            var result = await _frankfurterService.GetLatestAsync(baseCurrency);

            Assert.That(result, Is.EqualTo(cachedRates));

            _mockHttpClient.VerifyNoOtherCalls();
        }

        [Test]
        public void GetLatestAsync_ShouldThrowException_WhenCurrencyNotFound()
        {
            var baseCurrency = "NOTFOUND";

            var cacheKey = $"Latest-{baseCurrency}";

            _mockMemoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object?>.IsAny))
             .Returns((object key, out object? value) =>
             {
                 value = null;
                 return false;
             });

            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _frankfurterService.GetLatestAsync(baseCurrency));

            Assert.That(ex.Code, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ConvertAsync_ShouldReturnCachedValue_IfPresent()
        {
            var baseCurrency = "EUR";

            var quoteCurrency = "USD";

            var amount = 100m;

            var cacheKey = $"Latest-{baseCurrency}";

            var cachedRates = new ExchangeRateDto
            {
                Base = baseCurrency, Date = DateOnly.FromDateTime(DateTime.Now),
                Rates = new Dictionary<string, decimal>
                {
                    { quoteCurrency, 1.1m }
                }
            };

            _mockMemoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object?>.IsAny))
             .Returns((object key, out object? value) =>
             {
                 value = cachedRates;
                 return true;
             });

            var result = await _frankfurterService.ConvertAsync(baseCurrency, quoteCurrency, amount);

            Assert.That(result, Is.Positive);

            Assert.That(result, Is.EqualTo(cachedRates.Rates[quoteCurrency] * amount));

            _mockHttpClient.VerifyNoOtherCalls();
        }

        [Test]
        public void ConvertAsync_ShouldThrowException_WhenCurrencyConversionNotSupported()
        {
            var baseCurrency = "USD";

            var quoteCurrency = "NOTFOUND";

            var amount = 100m;

            var cacheKey = $"Latest-{baseCurrency}";

            _mockMemoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object?>.IsAny))
             .Returns((object key, out object? value) =>
             {
                 value = null;
                 return false;
             });

            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _frankfurterService.ConvertAsync(baseCurrency, quoteCurrency, amount));

            Assert.That(ex.Code, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetHistoryAsync_ShouldReturnCachedValue__IfPresent()
        {
            var baseCurrency = "EUR";

            var fromDate = "2024-01-01";

            var toDate = "2024-01-15";

            var cacheKey = $"History-{baseCurrency}-{fromDate}-{toDate}";

            var cachedHistory = new ExchangeRateHistoryDto
            {
                StartDate = DateOnly.FromDateTime(DateTime.ParseExact(fromDate, "yyyy-MM-dd", null)),

                EndDate = DateOnly.FromDateTime(DateTime.ParseExact(toDate, "yyyy-MM-dd", null)),

                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { "2024-01-01", new Dictionary<string, decimal> { { "USD", 1.1m }, { "GBP", 0.9m } } },
                    { "2024-01-15", new Dictionary<string, decimal> { { "USD", 1.2m }, { "GBP", 0.85m } } }
                }
            };

            _mockMemoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object?>.IsAny))
             .Returns((object key, out object? value) =>
             {
                 value = cachedHistory;
                 return true;
             });

            var result = await _frankfurterService.GetHistoryAsync(baseCurrency, fromDate, toDate);

            Assert.That(result, Is.Not.Null);

            Assert.That(result, Is.EqualTo(cachedHistory));
        }
    }
}
