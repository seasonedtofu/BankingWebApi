using BankingWebApi.Interfaces;

namespace BankingWebApi.Models;
public class Account: Entity, IAccount
{
    public Guid Id { get; init; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
    public bool Active { get; set; }
}
