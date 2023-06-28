using BankingWebApi.Interfaces;

namespace BankingWebApi.Models
{
    public class AccountTransfer: IAccountTransfer
    {
        public Guid TransferFromId { get; set; }
        public Guid TransferToId { get; set; }
        public decimal Amount { get; set; }
    }
}
