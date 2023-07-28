namespace BankingWebApi.Models;
public class Account: Entity
{
    public Guid Id { get; init; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
    public bool Active { get; set; }
}
