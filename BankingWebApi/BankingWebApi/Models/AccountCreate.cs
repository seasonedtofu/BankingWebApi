using BankingWebApi.Interfaces;

namespace BankingWebApi.Models;

public class AccountCreate: IAccountCreate
{
    public string? Name { get; set; }
    public decimal Balance { get; set; }
}
