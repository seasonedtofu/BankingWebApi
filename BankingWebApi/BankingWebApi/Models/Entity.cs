using BankingWebApi.Interfaces;

namespace BankingWebApi.Models

{
    public class Entity: IEntity
    {
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
