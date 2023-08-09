namespace BankingWebApi.Application.Models;
/// <summary>
/// Shape of model for creating account endpoint.
/// </summary>
public class CreateAccountDto
{
    public string? Name { get; set; }
    public decimal Balance { get; set; }
}
