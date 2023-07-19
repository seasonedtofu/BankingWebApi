namespace BankingWebApi.Interfaces;
public interface IAccountTransfer
{
    Guid TransferFromId { get; set; }
    Guid TransferToId { get; set; }
    decimal Amount { get; set; }
}
