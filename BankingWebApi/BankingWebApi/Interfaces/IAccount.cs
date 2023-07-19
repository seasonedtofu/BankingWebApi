namespace BankingWebApi.Interfaces;
public interface IAccount
{
    Guid Id { get; init; }
    string? Name { get; set; }
    decimal Balance { get; set; }
    bool Active { get; set; }
}
