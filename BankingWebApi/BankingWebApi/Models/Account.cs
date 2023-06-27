namespace BankingWebApi.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal Balance { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
