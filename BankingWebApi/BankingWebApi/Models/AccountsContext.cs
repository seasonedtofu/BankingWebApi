using Microsoft.EntityFrameworkCore;

namespace BankingWebApi.Models;

/// <summary>
/// Stores accounts locally.
/// </summary>
public class AccountsContext : DbContext
{
    public AccountsContext(DbContextOptions<AccountsContext> options)
        : base(options)
    {
        this.SavingChanges += AccountsContext_SavingChanges;
    }

    public DbSet<Account> Accounts { get; set; } = null!;

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

        foreach (var changedEntity in ChangeTracker.Entries())
        {
            if (changedEntity.Entity is Entity entity && entityStates.Contains(changedEntity.State))
            {
                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        entity.CreatedDate = DateTime.UtcNow;
                        entity.UpdatedDate = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entity.UpdatedDate = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
