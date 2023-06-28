namespace BankingWebApi.Interfaces
{
    public class IAccountCreate
    {
        string? Name { get; set; }
        decimal Balance { get; set; }
    }
}
