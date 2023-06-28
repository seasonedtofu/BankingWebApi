namespace BankingWebApi.Interfaces;

public interface IAccount
{
    Guid Id { get; set; }
    string? Name { get; set; }
    decimal Balance { get; set; }
    bool Active { get; set; }
    DateTime CreatedDate { get; set; }
    DateTime UpdatedDate { get; set; }
}
