using BankingWebApi.Domain.Interfaces;

namespace BankingWebApi.Domain.Entities
{
    public class Entity: IEntity
    {
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
