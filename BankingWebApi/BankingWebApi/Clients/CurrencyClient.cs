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

        public async Task<Dictionary<string, double>> GetCurrencyRate(string currency)
        {
            var response = await _httpClient!.GetAsync(
                $"{_httpClient.BaseAddress}{_freeCurrencyApiKey}&currencies={currency}&base_currency=USD");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double>>>(jsonResponse);
            return result?["data"];
        }
    }
}
