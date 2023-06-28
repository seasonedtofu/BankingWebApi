using BankingWebApi.Interfaces;

namespace BankingWebApi.Models;

public class Account: IAccount
{
    public Guid Id { get; init; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; init; }
    public DateTime UpdatedDate { get; set; }
}
