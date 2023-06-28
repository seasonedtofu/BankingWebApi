using BankingWebApi.Interfaces;

namespace BankingWebApi.Models;

/// <summary>
/// Shape of model for creating account endpoint.
/// </summary>
public class AccountCreate: IAccountCreate
{
    public string? Name { get; set; }
    public decimal Balance { get; set; }
}
