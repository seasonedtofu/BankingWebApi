﻿namespace BankingWebApi.Web.Interfaces
{
    public interface ICurrencyClient
    {
        Task<Dictionary<string, double>> GetCurrencyRate(string currency, string key);
    }
}
