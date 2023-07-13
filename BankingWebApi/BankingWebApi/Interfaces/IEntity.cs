namespace BankingWebApi.Interfaces
{
    public interface IEntity
    {
        DateTime CreatedDate { get; set; }
        DateTime UpdatedDate { get; set; }
    }
}
