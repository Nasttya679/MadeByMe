using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MadeByMe.Application.Services.Interfaces;

namespace MadeByMe.Application.Services.Implementations.Client
{
    public class NbuApiClient : IExchangeRateService
    {
        private readonly HttpClient _httpClient;

        public NbuApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://bank.gov.ua/");
        }

        public async Task<decimal> GetUsdRateAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<NbuRateResponse>>(
                    "NBUStatService/v1/statdirectory/exchange?valcode=USD&json");

                if (response != null && response.Any())
                {
                    return response.First().Rate;
                }
            }
            catch
            {
                return 40.0m;
            }

            return 40.0m;
        }

        private class NbuRateResponse
        {
            [JsonPropertyName("rate")]
            public decimal Rate { get; set; }
        }
    }
}