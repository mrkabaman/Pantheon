using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BankingSolution.Logic.Interfaces;
using BankingSolution.Logic.Poco;
using BankingSolution.Logic.ValueTypes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BankingSolution.Api.Services
{
    public class RatesService : IRatesService
    {
        private readonly ILogger<RatesService> _logger;
        private readonly RatesApiOption _options;
        private readonly IHttpClientFactory _clientFactory;

        public RatesService(ILogger<RatesService> logger, IHttpClientFactory clientFactory, RatesApiOption options)
        {
            _logger = logger;
            _options = options;

            _clientFactory = clientFactory;
        }

        public async Task<Rate> GetExchangeRate(Currency fromCurrency, Currency targetCurrency, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(RatesService)}.{nameof(GetExchangeRate)} - From:{fromCurrency} To: {targetCurrency}");
            
            try
            {
                using var httpClient = _clientFactory.CreateClient();

                var response = await httpClient.GetAsync($"{_options.Url}?base={fromCurrency.Value}&symbols={targetCurrency.Value}",
                    stoppingToken);

                if (response.IsSuccessStatusCode == false)
                {
                    _logger.LogCritical(
                        $"Rates api failed with Http Status Code: {response.StatusCode} at time: {DateTimeOffset.Now}");

                    return Rate.None;
                }

                using var sr = new StreamReader(await response.Content.ReadAsStreamAsync(stoppingToken));

                using var jsonTextReader = new JsonTextReader(sr);

                JObject jObject = (JObject) JToken.ReadFrom(jsonTextReader);

                string baseRate = jObject["base"]?.ToString();

                decimal conversionFactor = (decimal) jObject["rates"]?.First;

                string dateText = jObject["date"]?.ToString();

                DateTime date = string.IsNullOrWhiteSpace(dateText) == false
                    ? DateTime.Parse(dateText)
                    : DateTime.MinValue;

                var rate = new Rate(baseRate, targetCurrency.Value, conversionFactor, date);

                return rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting rates api");
                throw;
            }
        }
    }
}