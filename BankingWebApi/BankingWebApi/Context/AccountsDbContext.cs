using BankingWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingWebApi.Context
{
    public class AccountsDbContext : DbContext
    {
        public AccountsDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
    }
}
