using BankingWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingWebApi.Context
{
    public class AccountsDbContext : DbContext
    {
        public AccountsDbContext(DbContextOptions options) : base(options)
        {
            SavingChanges += AccountsContext_SavingChanges;
        }

        public DbSet<Account> Accounts { get; set; }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private void AccountsContext_SavingChanges(object? sender, SavingChangesEventArgs e)
        {
            var entityStates = new EntityState[]
            {
            EntityState.Added,
            EntityState.Modified,
            EntityState.Deleted,
            };

            foreach (var changedEntity in ChangeTracker
                        .Entries()
                        .Where(e => e.Entity is Entity && entityStates.Contains(e.State)))
            {
                var entity = (Entity)changedEntity.Entity;
                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        {
                            entity.CreatedDate = DateTime.UtcNow;
                            entity.UpdatedDate = DateTime.UtcNow;
                            break;
                        }
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        {
                            entity.UpdatedDate = DateTime.UtcNow;
                            break;
                        }
                }
            }
        }
    }
}
