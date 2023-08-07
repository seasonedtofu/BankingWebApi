namespace BankingWebApi.Application.Models;
/// <summary>
/// Shape of object for transferring money between accounts endpoint.
/// </summary>
public class AccountTransferDto
{
    public Guid TransferFromId { get; set; }
    public Guid TransferToId { get; set; }
    public decimal Amount { get; set; }
}
