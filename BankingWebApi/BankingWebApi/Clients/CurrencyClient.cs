using Newtonsoft.Json;

namespace BankingWebApi.Clients
{
    public class CurrencyClient
    {
        private const string _freeCurrencyApiKey = "pnQBM52UBxkiGmE9v8LB72Jpa27GFk31KKQlEVzh";
        private HttpClient? _httpClient;

        public CurrencyClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Object?> GetCurrencyRate(string currency)
        {
            var response = await _httpClient!.GetAsync(
                $"{_httpClient.BaseAddress}{_freeCurrencyApiKey}&currencies={currency}&base_currency=USD");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(jsonResponse);
            return result?["data"][currency];
        }
    }
}
