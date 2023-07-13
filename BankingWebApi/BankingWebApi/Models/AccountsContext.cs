using Microsoft.EntityFrameworkCore;
using BankingWebApi.Interfaces;

namespace BankingWebApi.Models;

/// <summary>
/// Stores accounts locally.
/// </summary>
public class AccountsContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    public AccountsContext(DbContextOptions<AccountsContext> options)
        : base(options)
    {
        this.SavingChanges += AccountsContext_SavingChanges;
    }

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
        Console.WriteLine("test");
        var entityStates = new EntityState[]
        {
            EntityState.Added,
            EntityState.Modified,
            EntityState.Deleted,
        };

        //Console.WriteLine(ChangeTracker.Entries().ToList().Count);
        //ChangeTracker.Entries().ToList().ForEach(Console.WriteLine);

        foreach (var changedEntity in ChangeTracker
                    .Entries()
                    .Where(e => e.Entity is Entity)
                )
        {
            //Console.WriteLine(changedEntity.Entity.ToString());
            //var entity = changedEntity.Entity;
            //foreach (var property in entity.GetType().GetProperties())
            //{
            //    Console.WriteLine(property.ToString());
            //}
            if (changedEntity.Entity is Entity)
            {
                Console.WriteLine("yes!!");
            }
            var entity = (Entity)changedEntity.Entity;
            Console.WriteLine(changedEntity.State);
            if (changedEntity.Entity is Entity ent)
            {
                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        {
                            Console.WriteLine("added hit");
                            ent.CreatedDate = DateTime.UtcNow;
                            ent.UpdatedDate = DateTime.UtcNow;
                            break;
                        }
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        {
                            Console.WriteLine("modified/deleted");
                            ent.UpdatedDate = DateTime.UtcNow;
                            break;
                        }
                }
            }
        }
    }
}
