using BankingWebApi.Domain.Entities;

namespace BankingWebApi.Application.Models
{
    public class AccountDto : Entity
    {
        public Guid Id { get; init; }
        public string? Name { get; set; }
        public decimal Balance { get; set; }
        public bool Active { get; set; }
    }
}