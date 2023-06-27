namespace BankingWebApi.Models
{
    public class AccountTransfer
    {
        public Guid TransferToId { get; set; }
        public decimal Amount { get; set; }
    }
}
