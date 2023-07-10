namespace BankingWebApi.Interfaces

{
    public interface ICurrencyClient
    {
        void OpenCurrencyClient();
        HttpResponse GetCurrencyRate();
    }
}
